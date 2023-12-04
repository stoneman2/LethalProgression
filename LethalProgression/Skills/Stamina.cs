using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;

namespace LethalProgression.Skills
{
    internal class Stamina
    {
        public static void StaminaUpdate(int oldStamina, int newStamina)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            localPlayer.sprintTime += (newStamina * 0.05f);
            LethalProgress.Log.LogInfo("Stamina upgraded! New level: " + localPlayer.sprintTime);
        }
    }
}
