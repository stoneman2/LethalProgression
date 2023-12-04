using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class HPRegen
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        private static void HPRegenUpdate(PlayerControllerB __instance)
        {
            if (__instance.health >= 100)
                return;

            if (LethalProgression.XPHandler.xpInstance.skillList.skills["HP Regen"].GetLevel() == 0)
                return;

            if (__instance.healthRegenerateTimer <= 0f)
            {
                // 0.1f per level. So, 1 health after 10 seconds.
                __instance.healthRegenerateTimer = 10f - (0.1f * LethalProgression.XPHandler.xpInstance.skillList.skills["HP Regen"].GetLevel());
                __instance.health++;

                if (__instance.health >= 20)
                {
                    __instance.MakeCriticallyInjured(false);
                }
                HUDManager.Instance.UpdateHealthUI(__instance.health, false);
            }
            else
            {
                __instance.healthRegenerateTimer -= Time.deltaTime;
            }
        }
    }
}
