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

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.HPRegen))
                return;

            if (LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.HPRegen].GetLevel() == 0)
                return;

            if (__instance.healthRegenerateTimer <= 0f)
            {
                Skill skill = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.HPRegen];
                float hps = skill.GetTrueValue();
                // Then turn that into seconds. So, if hps is 0.5, then it will take 2 seconds to regen 1 health.
                __instance.healthRegenerateTimer = 1f / hps;
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
