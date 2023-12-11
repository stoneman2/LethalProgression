using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class BatteryLife
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrabbableObject), "SyncBatteryServerRpc")]
        public static void BatteryUpdate(ref int charge)
        {
            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.Battery))
                return;

            if (charge == 100)
                charge += (int)LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Battery].GetTrueValue();
        }
    }
}
