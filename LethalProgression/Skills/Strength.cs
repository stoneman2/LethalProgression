using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class Strength
    {
        public static void StrengthUpdate(int change = 0)
        {
            SkillList skillList = LP_NetworkManager.xpInstance.skillList;
            if (!skillList.IsSkillListValid() || !skillList.IsSkillValid(UpgradeType.Strength)
                || skillList.skills[UpgradeType.Strength].GetLevel() == 0)
            {
                return;
            }

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            // Get every object in our hands.
            List<GrabbableObject> objects = player.ItemSlots.ToList();

            LethalPlugin.Log.LogDebug($"Carry weight was {player.carryWeight}");

            // Apply multiplier
            float level = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Strength].GetTrueValue();
            float multiplier = level / 100;

            float newCarryWeight = 0f;
            foreach (GrabbableObject obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                float oldItemWeight = obj.itemProperties.weight - 1f;
                oldItemWeight *= (1 - multiplier);

                newCarryWeight += oldItemWeight;
                LethalPlugin.Log.LogDebug($"Item weight was {obj.itemProperties.weight - 1f} and is now {oldItemWeight}");
                LethalPlugin.Log.LogDebug($"Adding carryweight.. now up to {newCarryWeight}");
            }

            // Reduce by percent
            player.carryWeight = Math.Clamp(1 + newCarryWeight, 0, 10f);

            LethalPlugin.Log.LogDebug($"Player carry weight is now {player.carryWeight}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.GrabObjectClientRpc))]
        private static void UpdateObjects()
        {
            // have to recalculate, grabobject runs too many times.. somewhat inefficient
            StrengthUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.DiscardItem))]
        private static void UpdateByDiscard(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                StrengthUpdate();
            }
        }
    }
}
