using System;
using System.IO;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;
using MOD_Rk7Qp2;

namespace MasterAffinityProtection
{
    public sealed class MasterAffinityProtectionCore
    {
        private const string HarmonyId = "MakRatanakKh.TaleOfImmortal.MasterAffinityProtection";
        private const int MaxNumericLogLines = 240;
        private const float MaxRawGetterDifference = 1.01f;
        private static HarmonyLib.Harmony harmony;
        private static int blockedAdd;
        private static int adjustedSet;
        private static int numericLogLines;
        private static bool reportedLookupError;
        private static bool reportedNumericReadError;
        private static bool reportedRawReadError;
        private static bool reportedRawFallback;
        private static bool reportedRawSuccess;

        public void Init()
        {
            LogStatus("Init ENTERED (master affinity protection 0.1.0)");
            try { InitializePatches(); }
            catch (Exception error) { LogFailure("unhandled Init exception: " + error); }
        }

        private static void InitializePatches()
        {
            if (harmony != null)
            {
                LogStatus("Init called again; patches already installed.");
                return;
            }

            var owner = typeof(DataUnit.RelationData);
            var add = AccessTools.Method(owner, "AddIntim", new[]
            {
                typeof(string), typeof(float), typeof(int), typeof(string), typeof(bool)
            });
            var set = AccessTools.Method(owner, "SetIntim", new[] { typeof(string), typeof(float) });

            if (add == null || set == null)
            {
                LogFailure("incompatible game build: required RelationData affinity overload missing; no patches installed.");
                return;
            }

            var candidate = new HarmonyLib.Harmony(HarmonyId);
            try
            {
                candidate.Patch(add, prefix: new HarmonyMethod(typeof(MasterAffinityProtectionCore), nameof(AddIntimPrefix)), postfix: new HarmonyMethod(typeof(MasterAffinityProtectionCore), nameof(AddIntimPostfix)));
                candidate.Patch(set, prefix: new HarmonyMethod(typeof(MasterAffinityProtectionCore), nameof(SetIntimPrefix)), postfix: new HarmonyMethod(typeof(MasterAffinityProtectionCore), nameof(SetIntimPostfix)));
                harmony = candidate;
                LogStatus("patched " + add.DeclaringType.FullName + "." + add.Name);
                LogStatus("patched " + set.DeclaringType.FullName + "." + set.Name);
                LogStatus("protecting player <-> current Master affinity; cap-aware fractional guard maximum=" + AffinityCapPolicy.Maximum + ".");
                LogStatus("startup patch installation complete; verify on a backup save.");
            }
            catch (Exception error)
            {
                try { candidate.UnpatchSelf(); } catch { }
                harmony = null;
                LogFailure("patch installation FAILED: " + error);
            }
        }

        public void Destroy()
        {
            if (harmony == null) return;
            try { harmony.UnpatchSelf(); harmony = null; LogStatus("removed patches."); }
            catch (Exception error) { LogFailure("Destroy failed: " + error); }
        }

        private static bool AddIntimPrefix(DataUnit.RelationData __instance, string unitID, float value)
        {
            TraceAffinity(__instance, unitID, "AddIntim BEFORE; requestedDelta=" + value);
            if (value >= 0f || !IsProtectedMasterPair(__instance, unitID)) return true;
            blockedAdd++;
            TraceAffinity(__instance, unitID, "AddIntim BLOCKED; requestedDelta=" + value);
            LogOccasionally("blocked negative AddIntim", blockedAdd);
            return false;
        }

        private static void AddIntimPostfix(DataUnit.RelationData __instance, string unitID)
        {
            TraceAffinity(__instance, unitID, "AddIntim AFTER");
        }

        private static void SetIntimPrefix(DataUnit.RelationData __instance, string unitID, ref float value)
        {
            TraceAffinity(__instance, unitID, "SetIntim BEFORE; requestedAbsolute=" + value);
            if (!IsProtectedMasterPair(__instance, unitID)) return;
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                LogFailure("non-finite protected SetIntim request left to game logic.");
                return;
            }

            int integerCurrent = __instance.GetIntim(unitID);
            float current;
            string source;
            bool precise = TryGetValidatedRawAffinity(__instance, unitID, integerCurrent, out current, out source);
            if (!precise)
            {
                current = integerCurrent;
                source = "integer-fallback";
                if (!reportedRawFallback)
                {
                    reportedRawFallback = true;
                    LogFailure("fractional guard unavailable for at least one master relation; using GetIntim(int) fallback.");
                }
            }
            else if (!reportedRawSuccess)
            {
                reportedRawSuccess = true;
                LogStatus("RAW READ validated: " + source + "; storedFloat=" + current.ToString("R") + "; GetIntim(int)=" + integerCurrent);
            }

            float requested = value;
            float adjusted = AffinityCapPolicy.Apply(current, requested);
            if (adjusted == requested) return;
            value = adjusted;

            string reason = current > AffinityCapPolicy.Maximum ? "CAP NORMALIZED" : requested > AffinityCapPolicy.Maximum ? "CAP LIMITED" : "DECAY BLOCKED";
            adjustedSet++;
            TraceAffinity(__instance, unitID, "SetIntim " + reason + "; requestedAbsolute=" + requested.ToString("R") + "; passedAbsolute=" + value.ToString("R") + "; previous=" + current.ToString("R") + "; guard=" + source);
            LogOccasionally("adjusted protected SetIntim (" + reason + ")", adjustedSet);
        }

        private static void SetIntimPostfix(DataUnit.RelationData __instance, string unitID)
        {
            TraceAffinity(__instance, unitID, "SetIntim AFTER");
        }

        private static bool TryGetValidatedRawAffinity(DataUnit.RelationData relation, string otherId, int integerCurrent, out float raw, out string source)
        {
            raw = 0f;
            source = "unavailable";
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null) return false;
                string playerId = world.playerUnit.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId)) return false;

                if (string.Equals(ownerId, playerId, StringComparison.Ordinal))
                {
                    var entries = relation.intimToUnit;
                    if (entries == null || !entries.TryGetValue(otherId, out raw))
                    {
                        source = "intimToUnit missing entry";
                        return false;
                    }
                    source = "intimToUnit";
                }
                else if (string.Equals(otherId, playerId, StringComparison.Ordinal))
                {
                    raw = relation.intimToPlayerUnit;
                    source = "intimToPlayerUnit";
                }
                else return false;

                if (float.IsNaN(raw) || float.IsInfinity(raw) || Math.Abs(raw - integerCurrent) > MaxRawGetterDifference)
                {
                    if (!reportedRawReadError)
                    {
                        reportedRawReadError = true;
                        LogFailure("RAW READ candidate inconsistent: " + source + "=" + raw + ", GetIntim(int)=" + integerCurrent + "; using integer fallback.");
                    }
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!reportedRawReadError)
                {
                    reportedRawReadError = true;
                    LogFailure("RAW READ exception; retaining integer fallback: " + ex);
                }
                return false;
            }
        }

        private static void TraceAffinity(DataUnit.RelationData relation, string otherId, string action)
        {
            if (numericLogLines >= MaxNumericLogLines || relation == null || string.IsNullOrEmpty(otherId)) return;
            if (!IsProtectedMasterPair(relation, otherId)) return;
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null) return;
                string playerId = world.playerUnit.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId)) return;
                bool ownerIsPlayer = string.Equals(ownerId, playerId, StringComparison.Ordinal);
                bool targetIsPlayer = string.Equals(otherId, playerId, StringComparison.Ordinal);
                if (ownerIsPlayer == targetIsPlayer) return;

                int integerAffinity = relation.GetIntim(otherId);
                float raw;
                string source;
                bool precise = TryGetValidatedRawAffinity(relation, otherId, integerAffinity, out raw, out source);
                string direction = ownerIsPlayer ? "player->MASTER" : "MASTER->player";
                string masterId = ownerIsPlayer ? otherId : ownerId;
                numericLogLines++;
                LogStatus("NUMERIC MASTER " + action + "; direction=" + direction + "; masterId=" + masterId + "; GetIntim(int)=" + integerAffinity + "; raw=" + (precise ? raw.ToString("R") : "unverified") + "; rawSource=" + source);
                if (numericLogLines == MaxNumericLogLines) LogStatus("NUMERIC trace limit reached; protection remains active.");
            }
            catch (Exception ex)
            {
                if (!reportedNumericReadError)
                {
                    reportedNumericReadError = true;
                    LogFailure("numerical affinity readout failed; patch behavior unchanged: " + ex);
                }
            }
        }

        private static bool IsProtectedMasterPair(DataUnit.RelationData relation, string otherId)
        {
            if (relation == null || string.IsNullOrEmpty(otherId)) return false;
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null) return false;
                string playerId = world.playerUnit.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId)) return false;
                bool ownerIsPlayer = string.Equals(ownerId, playerId, StringComparison.Ordinal);
                bool targetIsPlayer = string.Equals(otherId, playerId, StringComparison.Ordinal);
                if (ownerIsPlayer == targetIsPlayer) return false;

                string masterId = ownerIsPlayer ? otherId : ownerId;
                var playerRelation = world.playerUnit.data.unitData.relationData;
                if (playerRelation == null) return false;
                return playerRelation.IsRelation(masterId, UnitRelationType.Master);
            }
            catch (Exception ex)
            {
                if (!reportedLookupError)
                {
                    reportedLookupError = true;
                    LogFailure("master relation lookup failed; protection disabled for this call: " + ex);
                }
                return false;
            }
        }

        private static void LogOccasionally(string action, int count)
        {
            if (count <= 5 || count % 100 == 0) LogStatus(action + " (total " + count + ")");
        }

        private static void LogStatus(string message)
        {
            WriteDiagnostic(message);
            try { MelonLogger.Msg("MasterAffinityProtection: " + message); } catch { }
        }

        private static void LogFailure(string message)
        {
            WriteDiagnostic("ERROR: " + message);
            try { MelonLogger.Error("MasterAffinityProtection: " + message); } catch { }
        }

        private static void WriteDiagnostic(string message)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "MasterAffinityProtection-diagnostic.log");
                File.AppendAllText(path, DateTime.Now.ToString("O") + " MasterAffinityProtection: " + message + Environment.NewLine);
            }
            catch { }
        }
    }
}