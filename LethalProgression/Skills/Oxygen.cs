using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;

namespace LethalProgression.Skills
{
    [HarmonyPatch]
    internal class Oxygen
    {
        private static GameObject oxygenBar;
        private static float oxygen = 0f;
        private static float oxygenTimer = 0f;
        private static bool inWater = false;
        private static bool canDrown = true;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "SetFaceUnderwaterClientRpc")]
        private static void EnteredWater(PlayerControllerB __instance)
        {
            inWater = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "SetFaceOutOfWaterClientRpc")]
        private static void LeftWater(PlayerControllerB __instance)
        {
            inWater = false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "SetFaceUnderwaterFilters")]
        private static void ShouldDrown(PlayerControllerB __instance)
        {
            if (!canDrown)
            {
                StartOfRound.Instance.drowningTimer = 99f;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        private static void OxygenUpdate(PlayerControllerB __instance)
        {

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.Oxygen))
                return;

            if (__instance.isPlayerDead)
            {
                if (oxygenBar)
                {
                    oxygenBar.SetActive(false);
                }
                return;
            }

            if (LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Oxygen].GetLevel() == 0)
            {
                if (oxygenBar)
                {
                    oxygenBar.SetActive(false);
                }
                if (!canDrown)
                {
                    canDrown = true;
                    StartOfRound.Instance.drowningTimer = 1f;
                }
                return;
            }

            if (!oxygenBar)
                CreateOxygenBar();

            Skill skill = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Oxygen];
            float maxOxygen = skill.GetTrueValue();
            if (inWater)
            {
                oxygenBar.SetActive(true);
                if (oxygenTimer <= 0f)
                {
                    oxygenTimer = 0.1f;
                    if (oxygen > 0)
                    {
                        oxygen -= 0.1f;
                    }
                    else
                    {
                        // It's 0! We're drowning!
                        if (!canDrown)
                        {
                            canDrown = true;
                            StartOfRound.Instance.drowningTimer = 1f;
                        }
                    }
                }
                else
                {
                    oxygenTimer -= Time.deltaTime;
                }
            }
            if (!inWater)
            {
                if (oxygenTimer <= 0f)
                {
                    oxygenTimer = 0.1f;
                    if (oxygen < maxOxygen)
                    {
                        oxygenBar.SetActive(true);
                        oxygen += 0.1f;
                        canDrown = false;
                    }
                    else
                    {
                        oxygenBar.SetActive(false);
                    }
                }
                else
                {
                    oxygenTimer -= Time.deltaTime;
                }
            }
            if (oxygen > maxOxygen)
            {
                oxygenBar.SetActive(false);
                oxygen = maxOxygen;
            }
            if (oxygenBar.activeSelf)
            {
                float fill = oxygen / maxOxygen;
                oxygenBar.transform.GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = fill;
            }
            //LethalPlugin.Log.LogInfo($"Underwater: {__instance.isUnderwater} | Oxygen: {oxygen} | Oxygen Timer: {oxygenTimer} | In Water: {inWater}");
        }
        public static void CreateOxygenBar()
        {
            oxygenBar = GameObject.Instantiate(LethalPlugin.skillBundle.LoadAsset<GameObject>("OxygenBar"));
            oxygenBar.SetActive(false);
        }
    }
}