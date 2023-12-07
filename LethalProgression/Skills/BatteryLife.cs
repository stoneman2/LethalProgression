using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;

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
                charge += (int)XPHandler.xpInstance.skillList.skills[UpgradeType.Battery].GetTrueValue();
        }
    }
}
