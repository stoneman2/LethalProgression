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

            RoundManager.Instance.scrapValueMultiplier = RoundManager.Instance.scrapValueMultiplier + (LP_NetworkManager.xpInstance.teamLootValue.Value / 100);
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
