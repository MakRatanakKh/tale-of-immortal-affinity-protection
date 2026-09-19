using System;
using System.IO;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

// Tale of Immortal's in-game C# mod entry point (not a standalone MelonMod).
namespace MOD_AffinityProtection
{
    public class ModMain
    {
        private const string HarmonyId = "MakRatanakKh.TaleOfImmortal.AffinityProtection";
        // The game also exposes a namespace named Harmony; qualify the class to avoid CS0118.
        private static HarmonyLib.Harmony harmony;
        private static int blockedAdd;
        private static int blockedSet;
        private static bool reportedLookupError;

        public void Init()
        {
            // Must run before reflection or patch setup. In-game mod logging is not
            // guaranteed to appear in Player.log or MelonLoader/Latest.log, so also
            // write a best-effort diagnostic to the user's temporary directory.
            LogStatus("Init ENTERED (diagnostic build 0.1.2)");
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
            if (addPrefix == null || setPrefix == null)
            {
                LogFailure("internal prefix lookup failed; no patches installed.");
                return;
            }

            var candidate = new HarmonyLib.Harmony(HarmonyId);
            try
            {
                candidate.Patch(add, prefix: new HarmonyMethod(addPrefix));
                candidate.Patch(set, prefix: new HarmonyMethod(setPrefix));
                harmony = candidate;
                LogStatus("patched " + add.DeclaringType.FullName + "." + add.Name + "(" + string.Join(", ", Array.ConvertAll(add.GetParameters(), p => p.ParameterType.Name)) + ")");
                LogStatus("patched " + set.DeclaringType.FullName + "." + set.Name + "(" + string.Join(", ", Array.ConvertAll(set.GetParameters(), p => p.ParameterType.Name)) + ")");
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
            if (value >= 0f || !IsProtectedPair(__instance, unitID))
                return true;
            blockedAdd++;
            LogOccasionally("blocked negative AddIntim", blockedAdd);
            return false;
        }

        // Also catch direct absolute writes that bypass AddIntim. GetIntim returns an int
        // in this game build, so this clamp may miss sub-integer changes: test carefully.
        // ClearIntim is intentionally not patched to avoid interfering with breakups.
        private static void SetIntimPrefix(DataUnit.RelationData __instance, string unitID, ref float value)
        {
            if (!IsProtectedPair(__instance, unitID))
                return;

            int current = __instance.GetIntim(unitID);
            if (value >= current)
                return;
            value = current;
            blockedSet++;
            LogOccasionally("clamped decreasing SetIntim", blockedSet);
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
