using System;
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
            // This must be the FIRST operation: the previous build logged only after
            // reflection and Harmony setup, making early startup failures invisible.
            // Unity logs go to Player.log, while MelonLogger goes to MelonLoader logs.
            LogStatus("Init ENTERED (diagnostic build 0.1.1)");
            try
            {
                InitializePatches();
            }
            catch (Exception error)
            {
                // Include failures from method resolution, type initialization, and
                // even the normal error-reporting code in InitializePatches.
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

        // Log independently to both sinks. Failure in a logging subsystem must never
        // prevent affinity processing or conceal a diagnostic from the other sink.
        private static void LogStatus(string message)
        {
            try { UnityEngine.Debug.Log("AffinityProtection: " + message); }
            catch (Exception) { /* Unity logger unavailable; try MelonLogger. */ }
            try { MelonLogger.Msg("AffinityProtection: " + message); }
            catch (Exception) { /* No logging sink available at this point. */ }
        }

        private static void LogFailure(string message)
        {
            try { UnityEngine.Debug.LogError("AffinityProtection: " + message); }
            catch (Exception) { /* Unity logger unavailable; try MelonLogger. */ }
            try { MelonLogger.Error("AffinityProtection: " + message); }
            catch (Exception) { /* No logging sink available at this point. */ }
        }
    }
}
