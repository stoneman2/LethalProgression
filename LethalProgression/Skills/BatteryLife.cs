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
            if (charge == 100)
                charge += (int)LethalProgression.XPHandler.xpInstance.skillList.skills["Battery Life"].GetTrueValue();
        }
    }
}
