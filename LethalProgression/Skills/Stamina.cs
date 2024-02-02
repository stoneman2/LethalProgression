using GameNetcodeStuff;

namespace LethalProgression.Skills
{
    internal class Stamina
    {
        public static void StaminaUpdate(int updatedValue, int newStamina)
        {
            SkillList skillList = LP_NetworkManager.xpInstance.skillList;
            if (!skillList.IsSkillListValid() || !skillList.IsSkillValid(UpgradeType.Stamina))
            {
                return;
            }

            Skill skill = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Stamina];
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            // 1 level adds 2% more to 5. So that is equals to 5 * 1.02.
            float addedStamina = (updatedValue * skill.GetMultiplier() / 100f) * 11f;
            localPlayer.sprintTime += addedStamina;
            LethalPlugin.Log.LogInfo($"{updatedValue} change, {newStamina} new stamina points, Adding {addedStamina} resulting in {localPlayer.sprintTime} stamina");
        }
    }
}
