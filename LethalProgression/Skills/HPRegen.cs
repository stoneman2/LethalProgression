using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class HPRegen
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.LateUpdate))]
        private static void HPRegenUpdate(PlayerControllerB __instance)
        {
            if (__instance.health >= 100)
            {
                return;
            }
            SkillList skillList = LP_NetworkManager.xpInstance.skillList;
            if (!skillList.IsSkillListValid() || !skillList.IsSkillValid(UpgradeType.HPRegen)
                || skillList.skills[UpgradeType.HPRegen].GetLevel() == 0)
            {
                return;
            }

            if (__instance.healthRegenerateTimer > 0f)
            {
                __instance.healthRegenerateTimer -= Time.deltaTime;
                return;
            }

            Skill skill = skillList.skills[UpgradeType.HPRegen];
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
    }
}
