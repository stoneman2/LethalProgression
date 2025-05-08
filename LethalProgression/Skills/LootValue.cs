using HarmonyLib;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class LootValue
    {

        /// OnSpawnScrap
        /// UpdateLootValue
        /// 
        /// 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        private static void AddLootValue()
        {
            SkillList skillList = LP_NetworkManager.xpInstance.skillList;
            if (!skillList.IsSkillListValid() || !skillList.IsSkillValid(UpgradeType.Value))
            {
                return;
            }

            float scrapValueAdded = (LP_NetworkManager.xpInstance.teamLootValue.Value / 100);
            try
            {
                RoundManager.Instance.scrapValueMultiplier += scrapValueAdded;
            }
            catch { }
            LethalPlugin.Log.LogDebug($"Added {scrapValueAdded} to scrap value multiplier, " +
                $"resulting in {RoundManager.Instance.scrapValueMultiplier}");
        }

        public static void LootValueUpdate(int change)
        {
            SkillList skillList = LP_NetworkManager.xpInstance.skillList;
            if (!skillList.IsSkillListValid() || !skillList.IsSkillValid(UpgradeType.Value))
            {
                return;
            }

            LP_NetworkManager.xpInstance.TeamLootValueUpdate(change);
        }
    }
}
