using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

// Tale of Immortal's in-game C# mod entry point (not a standalone MelonMod).
namespace MOD_AffinityProtection
{
    public class ModMain
    {
        private const string HarmonyId = "MakRatanakKh.TaleOfImmortal.AffinityProtection";
        private static Harmony harmony;
        private static int blockedAdd;
        private static int blockedSet;
        private static bool reportedLookupError;

        public void Init()
        {
            if (harmony != null)
                return;

            // Resolve the exact overloads identified in the user's own generated assembly.
            // Never report success if either overload is missing.
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
                MelonLogger.Error("AffinityProtection: incompatible game build: required RelationData affinity overload missing; no patches installed.");
                return;
            }

            var addPrefix = AccessTools.Method(typeof(ModMain), nameof(AddIntimPrefix));
            var setPrefix = AccessTools.Method(typeof(ModMain), nameof(SetIntimPrefix));
            if (addPrefix == null || setPrefix == null)
            {
                MelonLogger.Error("AffinityProtection: internal prefix lookup failed; no patches installed.");
                return;
            }

            var candidate = new Harmony(HarmonyId);
            try
            {
                candidate.Patch(add, prefix: new HarmonyMethod(addPrefix));
                candidate.Patch(set, prefix: new HarmonyMethod(setPrefix));
                harmony = candidate;
                MelonLogger.Msg("AffinityProtection: patched " + add.DeclaringType.FullName + "." + add.Name + "(" + string.Join(", ", Array.ConvertAll(add.GetParameters(), p => p.ParameterType.Name)) + ")");
                MelonLogger.Msg("AffinityProtection: patched " + set.DeclaringType.FullName + "." + set.Name + "(" + string.Join(", ", Array.ConvertAll(set.GetParameters(), p => p.ParameterType.Name)) + ")");
                MelonLogger.Msg("AffinityProtection: startup patch installation complete; confirm behavior using a backup save.");
            }
            catch (Exception error)
            {
                candidate.UnpatchSelf();
                MelonLogger.Error("AffinityProtection: patch installation FAILED: " + error);
            }
        }

        public void Destroy()
        {
            if (harmony == null)
                return;
            harmony.UnpatchSelf();
            harmony = null;
            MelonLogger.Msg("AffinityProtection: removed patches.");
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
                string playerId = player.GetUnitId();
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
                    MelonLogger.Error("AffinityProtection: relation lookup failed; protection disabled for this call: " + ex);
                }
                return false;
            }
        }

        private static void LogOccasionally(string action, int count)
        {
            if (count <= 5 || count % 100 == 0)
                MelonLogger.Msg("AffinityProtection: " + action + " (total " + count + ")");
        }
    }
}
