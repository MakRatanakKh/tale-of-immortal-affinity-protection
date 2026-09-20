using System;
using System.IO;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

// Tale of Immortal's in-game C# mod entry point (also reused by the standalone MelonMod).
namespace MOD_Rk7Qp2
{
    public class ModMain
    {
        private const string HarmonyId = "MakRatanakKh.TaleOfImmortal.AffinityProtection";
        // Prioritize protected relationships: unrelated NPC events exhausted the old
        // 120-line cap during a single month rollover. Logging does not alter patches.
        private const int MaxNumericLogLines = 240;
        // The game also exposes a namespace named Harmony; qualify the class to avoid CS0118.
        private static HarmonyLib.Harmony harmony;
        private static int blockedAdd;
        private static int blockedSet;
        private static int numericLogLines;
        private static bool reportedLookupError;
        private static bool reportedNumericReadError;

        public void Init()
        {
            // Must run before reflection or patch setup. In-game mod logging is not
            // guaranteed to appear in Player.log or MelonLoader/Latest.log, so also
            // write a best-effort diagnostic to the user's temporary directory.
            LogStatus("Init ENTERED (protected-pair numeric diagnostic build 0.2.2-test)");
            try
            {
                InitializePatches();
            }
            catch (Exception error)
            {
                LogFailure("unhandled Init exception: " + error);
            }
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
                LogStatus("numeric diagnostics: PROTECTED spouse/partner pairs only, on intercepted writes; GetIntim returns integer values rather than raw float affinity; maximum " + MaxNumericLogLines + " numeric lines per session.");
                LogStatus("startup patch installation complete; confirm behavior using a backup save.");
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
            catch (Exception error)
            {
                LogFailure("Destroy failed: " + error);
            }
        }

        // Suppress negative deltas before the game's AddIntim method processes them.
        // Zero and positive deltas and all unrelated NPCs use the original method.
        private static bool AddIntimPrefix(DataUnit.RelationData __instance, string unitID, float value)
        {
            TraceIntegerAffinity(__instance, unitID, "AddIntim BEFORE; requestedDelta=" + value);
            if (value >= 0f || !IsProtectedPair(__instance, unitID))
                return true;
            blockedAdd++;
            TraceIntegerAffinity(__instance, unitID, "AddIntim BLOCKED; requestedDelta=" + value);
            LogOccasionally("blocked negative AddIntim", blockedAdd);
            return false;
        }

        // Postfix reads the actual numeric value after an allowed write; it can also
        // run when a prefix skips the original, so treat its result as an observation.
        private static void AddIntimPostfix(DataUnit.RelationData __instance, string unitID)
        {
            TraceIntegerAffinity(__instance, unitID, "AddIntim AFTER");
        }

        // Also catch direct absolute writes that bypass AddIntim. GetIntim returns an int
        // in this game build, so this clamp may miss sub-integer changes: test carefully.
        // ClearIntim is intentionally not patched to avoid interfering with breakups.
        private static void SetIntimPrefix(DataUnit.RelationData __instance, string unitID, ref float value)
        {
            TraceIntegerAffinity(__instance, unitID, "SetIntim BEFORE; requestedAbsolute=" + value);
            if (!IsProtectedPair(__instance, unitID))
                return;

            int current = __instance.GetIntim(unitID);
            if (value >= current)
                return;
            float requested = value;
            value = current;
            blockedSet++;
            TraceIntegerAffinity(__instance, unitID, "SetIntim CLAMPED; requestedAbsolute=" + requested + "; passedAbsolute=" + value);
            LogOccasionally("clamped decreasing SetIntim", blockedSet);
        }

        private static void SetIntimPostfix(DataUnit.RelationData __instance, string unitID)
        {
            TraceIntegerAffinity(__instance, unitID, "SetIntim AFTER");
        }

        // Limit detailed traces to actual CURRENT protected pairs. Filtering on
        // relationship rather than all NPC->player writes prevents unrelated world
        // events from exhausting the quota before later month/year transitions.
        // GetIntim is an int getter: do NOT label this the exact stored float value.
        private static void TraceIntegerAffinity(DataUnit.RelationData relation, string otherId, string action)
        {
            if (numericLogLines >= MaxNumericLogLines || relation == null || string.IsNullOrEmpty(otherId))
                return;
            // Diagnostic-only filter: intentionally does not change the decisions in
            // AddIntimPrefix/SetIntimPrefix. Errors fail open in IsProtectedPair.
            if (!IsProtectedPair(relation, otherId))
                return;
            try
            {
                var world = g.world;
                if (world == null || world.playerUnit == null)
                    return;
                var player = world.playerUnit;
                string playerId = player.data.unitData.unitID;
                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(ownerId))
                    return;

                bool ownerIsPlayer = string.Equals(ownerId, playerId, StringComparison.Ordinal);
                bool targetIsPlayer = string.Equals(otherId, playerId, StringComparison.Ordinal);
                if (ownerIsPlayer == targetIsPlayer)
                    return;

                int integerAffinity = relation.GetIntim(otherId);
                string direction = ownerIsPlayer ? "player->NPC" : "NPC->player";
                string npcId = ownerIsPlayer ? otherId : ownerId;
                numericLogLines++;
                LogStatus("NUMERIC PROTECTED " + action + "; direction=" + direction + "; npcId=" + npcId + "; GetIntim(int)=" + integerAffinity);
                if (numericLogLines == MaxNumericLogLines)
                    LogStatus("NUMERIC trace limit reached (" + MaxNumericLogLines + "); suppressing further numerical lines this session. Blocking counters remain active.");
            }
            catch (Exception ex)
            {
                // Logging is diagnostic only; never let a readout change game logic.
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
                var player = world.playerUnit;
                // UnitInfoData.unitID is present in this exact assembly's inventory.
                string playerId = player.data.unitData.unitID;
                if (string.IsNullOrEmpty(playerId))
                    return false;

                string ownerId = relation.unitID;
                if (string.IsNullOrEmpty(ownerId))
                    return false;
                bool ownerIsPlayer = string.Equals(ownerId, playerId, StringComparison.Ordinal);
                bool targetIsPlayer = string.Equals(otherId, playerId, StringComparison.Ordinal);
                if (ownerIsPlayer == targetIsPlayer)
                    return false; // neither involves the player, or a self-relation

                string partnerId = ownerIsPlayer ? otherId : ownerId;
                var playerRelation = player.data.unitData.relationData;
                if (playerRelation == null)
                    return false;

                // Check the player's actual current relations, not hearts or old partners.
                return playerRelation.IsRelation(partnerId, UnitRelationType.Married)
                    || playerRelation.IsRelation(partnerId, UnitRelationType.Lover);
            }
            catch (Exception ex)
            {
                // Fail open rather than break relationship processing on unsupported builds.
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

        // Both sinks are best-effort. No UnityEngine.Debug dependency: the user's
        // IL2CPP game assembly references Unity types but the Debug module was not
        // referenced by this project (CS0234/CS0012 in diagnostic build 0.1.1).
        private static void LogStatus(string message)
        {
            WriteDiagnostic(message);
            try { MelonLogger.Msg("AffinityProtection: " + message); }
            catch (Exception) { /* Host logger unavailable at this stage. */ }
        }

        private static void LogFailure(string message)
        {
            WriteDiagnostic("ERROR: " + message);
            try { MelonLogger.Error("AffinityProtection: " + message); }
            catch (Exception) { /* Host logger unavailable at this stage. */ }
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
