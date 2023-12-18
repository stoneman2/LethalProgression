using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Unity.Netcode;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class LootValue
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        private static void AddLootValue()
        {
            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.Value))
                return;

            float scrapValueAdded = (LP_NetworkManager.xpInstance.teamLootValue.Value / 100);
            // Every time, set it to the default value, then add the multiplier.
            RoundManager.Instance.scrapValueMultiplier = 1f + scrapValueAdded;
        }

        public static void LootValueUpdate(int change, int newLevel)
        {
            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.Value))
                return;

            LP_NetworkManager.xpInstance.TeamLootValueUpdate(change, newLevel);
        }
    }
}
