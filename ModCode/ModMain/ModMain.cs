using System;
using System.IO;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

// Shared entry point for the in-game C# mod and the standalone MelonMod.
namespace MOD_Rk7Qp2
{
    public class ModMain
    {
        private const string HarmonyId = "MakRatanakKh.TaleOfImmortal.AffinityProtection";
        private const int MaxNumericLogLines = 240;
        // Plausibility check, not proof of the raw field's semantics.
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
            LogStatus("Init ENTERED (cap-aware fractional guard build 0.2.4-test)");
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
            var set = AccessTools.Method(owner, "SetIntim", new[]
            {
                typeof(string), typeof(float)
            });
            if (add == null || set == null)
            {
                LogFailure("incompatible game build: required RelationData affinity overload missing; no patches installed. AddIntim=" + (add != null) + ", SetIntim=" + (set != null));
                return;
            }

            var addPrefix = AccessTools.Method(typeof(ModMain), nameof(AddIntimPrefix));
            var setPrefix = AccessTools.Method(typeof(ModMain), nameof(SetIntimPrefix));
            var addPostfix = AccessTools.Method(typeof(ModMain), nameof(AddIntimPostfix));
            var setPostfix = AccessTools.Method(typeof(ModMain), nameof(SetIntimPostfix));
            if (addPrefix == null || setPrefix == null || addPostfix == null || setPostfix == null)
            {
                LogFailure("internal affinity patch lookup failed; no patches installed.");
                return;
            }

            var candidate = new HarmonyLib.Harmony(HarmonyId);
            try
            {
                candidate.Patch(add, prefix: new HarmonyMethod(addPrefix), postfix: new HarmonyMethod(addPostfix));
                candidate.Patch(set, prefix: new HarmonyMethod(setPrefix), postfix: new HarmonyMethod(setPostfix));
                harmony = candidate;
                LogStatus("patched " + add.DeclaringType.FullName + "." + add.Name + "(" + string.Join(", ", Array.ConvertAll(add.GetParameters(), p => p.ParameterType.Name)) + ")");
                LogStatus("patched " + set.DeclaringType.FullName + "." + set.Name + "(" + string.Join(", ", Array.ConvertAll(set.GetParameters(), p => p.ParameterType.Name)) + ")");
                LogStatus("cap-aware fractional guard: protected affinity capped at " + AffinityCapPolicy.Maximum + "; raw reads validated against GetIntim(int) with integer fallback; maximum " + MaxNumericLogLines + " numeric lines per session.");
                LogStatus("startup patch installation complete; test only with a disposable backup save.");
            }
            catch (Exception error)
            {
                try { candidate.UnpatchSelf(); }
                catch (Exception cleanupError) { LogFailure("patch cleanup failed: " + cleanupError); }
                harmony = null;
                LogFailure("patch installation FAILED: " + error);
            }
        }

        public void Destroy()
        {
            if (harmony == null)
                return;
            try
            {
                harmony.UnpatchSelf();
                harmony = null;
                LogStatus("removed patches.");
            }
            catch (Exception error) { LogFailure("Destroy failed: " + error); }
        }

        // Keep negative-delta suppression as before. Positive deltas reach the
        // game's SetIntim writer, where the cap-aware guard limits the final write.
        // A blocked negative AddIntim does not itself repair an existing over-cap
        // value; normalization happens on the next protected SetIntim write.
        private static bool AddIntimPrefix(DataUnit.RelationData __instance, string unitID, float value)
        {
            TraceAffinity(__instance, unitID, "AddIntim BEFORE; requestedDelta=" + value);
            if (value >= 0f || !IsProtectedPair(__instance, unitID))
                return true;
            blockedAdd++;
            TraceAffinity(__instance, unitID, "AddIntim BLOCKED; requestedDelta=" + value);
            LogOccasionally("blocked negative AddIntim", blockedAdd);
            return false;
        }

        private static void AddIntimPostfix(DataUnit.RelationData __instance, string unitID)
        {
            TraceAffinity(__instance, unitID, "AddIntim AFTER");
        }

        // Only current spouses/partners are protected. Use the exact stored float
        // when plausibly validated, otherwise fall back to the prior integer getter.
        // An above-300 affinity may normalize to 300; a below-300 affinity may not
        // decline, including fractional decreases. Gains cannot exceed 300.
        // ClearIntim is intentionally not patched (breakups remain game-controlled).
        private static void SetIntimPrefix(DataUnit.RelationData __instance, string unitID, ref float value)
        {
            TraceAffinity(__instance, unitID, "SetIntim BEFORE; requestedAbsolute=" + value);
            if (!IsProtectedPair(__instance, unitID))
                return;

            // Do not reinterpret exceptional game input. These calls need a log,
            // not an artificial affinity value that may make the situation worse.
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
                    LogFailure("fractional guard unavailable for at least one relation; using GetIntim(int) fallback. Fractional precision cannot be assured for that relation.");
                }
            }
            else if (!reportedRawSuccess)
            {
                reportedRawSuccess = true;
                LogStatus("RAW READ validated: " + source + "; storedFloat=" + current.ToString("R") + "; GetIntim(int)=" + integerCurrent);
            }

            float requested = value;
            float adjusted = AffinityCapPolicy.Apply(current, requested);
            if (adjusted == requested)
                return;

            value = adjusted;
            string reason = current > AffinityCapPolicy.Maximum
                ? "CAP NORMALIZED"
                : requested > AffinityCapPolicy.Maximum
                    ? "CAP LIMITED"
                    : "DECAY BLOCKED";
            adjustedSet++;
            TraceAffinity(__instance, unitID, "SetIntim " + reason + "; requestedAbsolute=" + requested.ToString("R") + "; passedAbsolute=" + value.ToString("R") + "; previous=" + current.ToString("R") + "; guard=" + source);
            LogOccasionally("adjusted protected SetIntim (" + reason + ")", adjustedSet);
        }

        private static void SetIntimPostfix(DataUnit.RelationData __instance, string unitID)
        {
            TraceAffinity(__instance, unitID, "SetIntim AFTER");
        }

        // Game metadata exposes these candidate raw float stores:
        // NPC->player: relation.intimToPlayerUnit
        // player->NPC: relation.intimToUnit[otherId]
        // Verify each reading against GetIntim; neither metadata nor this check
        // proves the complete runtime semantics on every game version.
        private static bool TryGetValidatedRawAffinity(DataUnit.RelationData relation, string otherId, int integerCurrent, out float raw, out string source)
        {
            raw = 0f;
            source = "unavailable";
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null)
                    return false;
                string playerId = world.playerUnit.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId))
                    return false;

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
                else
                    return false;

                if (float.IsNaN(raw) || float.IsInfinity(raw) || Math.Abs(raw - integerCurrent) > MaxRawGetterDifference)
                {
                    if (!reportedRawReadError)
                    {
                        reportedRawReadError = true;
                        LogFailure("RAW READ candidate inconsistent: " + source + "=" + raw + ", GetIntim(int)=" + integerCurrent + "; refusing raw clamp, using integer fallback.");
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

        // Diagnostic-only readout: focus on protected pairs to avoid exhausting
        // the quota on unrelated NPCs. No logging decision changes protection.
        private static void TraceAffinity(DataUnit.RelationData relation, string otherId, string action)
        {
            if (numericLogLines >= MaxNumericLogLines || relation == null || string.IsNullOrEmpty(otherId))
                return;
            if (!IsProtectedPair(relation, otherId))
                return;
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null)
                    return;
                string playerId = world.playerUnit.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId))
                    return;
                bool ownerIsPlayer = string.Equals(ownerId, playerId, StringComparison.Ordinal);
                bool targetIsPlayer = string.Equals(otherId, playerId, StringComparison.Ordinal);
                if (ownerIsPlayer == targetIsPlayer)
                    return;

                int integerAffinity = relation.GetIntim(otherId);
                float raw;
                string source;
                bool precise = TryGetValidatedRawAffinity(relation, otherId, integerAffinity, out raw, out source);
                string direction = ownerIsPlayer ? "player->NPC" : "NPC->player";
                string npcId = ownerIsPlayer ? otherId : ownerId;
                numericLogLines++;
                LogStatus("NUMERIC PROTECTED " + action + "; direction=" + direction + "; npcId=" + npcId + "; GetIntim(int)=" + integerAffinity + "; raw=" + (precise ? raw.ToString("R") : "unverified") + "; rawSource=" + source);
                if (numericLogLines == MaxNumericLogLines)
                    LogStatus("NUMERIC trace limit reached (" + MaxNumericLogLines + "); suppressing further numeric lines this session. Protection remains active.");
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

        private static bool IsProtectedPair(DataUnit.RelationData relation, string otherId)
        {
            if (relation == null || string.IsNullOrEmpty(otherId))
                return false;
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null)
                    return false;
                string playerId = world.playerUnit.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId))
                    return false;
                bool ownerIsPlayer = string.Equals(ownerId, playerId, StringComparison.Ordinal);
                bool targetIsPlayer = string.Equals(otherId, playerId, StringComparison.Ordinal);
                if (ownerIsPlayer == targetIsPlayer)
                    return false;
                string partnerId = ownerIsPlayer ? otherId : ownerId;
                var playerRelation = world.playerUnit.data.unitData.relationData;
                if (playerRelation == null)
                    return false;
                return playerRelation.IsRelation(partnerId, UnitRelationType.Married)
                    || playerRelation.IsRelation(partnerId, UnitRelationType.Lover);
            }
            catch (Exception ex)
            {
                if (!reportedLookupError)
                {
                    reportedLookupError = true;
                    LogFailure("relation lookup failed; protection disabled for this call: " + ex);
                }
                return false;
            }
        }

        private static void LogOccasionally(string action, int count)
        {
            if (count <= 5 || count % 100 == 0)
                LogStatus(action + " (total " + count + ")");
        }

        private static void LogStatus(string message)
        {
            WriteDiagnostic(message);
            try { MelonLogger.Msg("AffinityProtection: " + message); }
            catch (Exception) { /* Logger unavailable; diagnostic is best effort. */ }
        }

        private static void LogFailure(string message)
        {
            WriteDiagnostic("ERROR: " + message);
            try { MelonLogger.Error("AffinityProtection: " + message); }
            catch (Exception) { /* Logger unavailable; diagnostic is best effort. */ }
        }

        private static void WriteDiagnostic(string message)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "AffinityProtection-diagnostic.log");
                File.AppendAllText(path, DateTime.Now.ToString("O") + " AffinityProtection: " + message + Environment.NewLine);
            }
            catch (Exception) { /* Logging must never alter game behavior. */ }
        }
    }
}
