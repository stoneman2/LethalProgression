using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;

namespace LethalProgression.Skills
{
    internal class JumpHeight
    {
        public static void JumpHeightUpdate(int updatedValue, int newStamina)
        {
            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.JumpHeight))
                return;

            Skill skill = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.JumpHeight];
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            // 5 is 100%. So if 1 level adds 1% more, then it is 5 * 1.01.
            float addedJump = (updatedValue * skill.GetMultiplier() / 100f) * 5f;
            localPlayer.jumpForce += addedJump;
            LethalPlugin.Log.LogInfo($"{updatedValue} change, {newStamina} new jump height, Adding {addedJump} resulting in {localPlayer.jumpForce} jump force");
        }
    }
}
