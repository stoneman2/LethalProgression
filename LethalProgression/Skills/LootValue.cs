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
            RoundManager.Instance.scrapValueMultiplier = RoundManager.Instance.scrapValueMultiplier + (XPHandler.xpInstance.teamLootValue.Value / 100);
        }
    }
}
