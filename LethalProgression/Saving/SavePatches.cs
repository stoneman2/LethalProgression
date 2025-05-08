using System.Collections.Generic;
using HarmonyLib;
using LethalProgression.Skills;
using Newtonsoft.Json;

namespace LethalProgression.Saving
{
    [HarmonyPatch]
    internal class SavePatches
    {
        // Whenever game saves, do save!
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveGame))]
        private static void SaveGame(GameNetworkManager __instance)
        {
            DoSave();
        }

        // Whenever disconnect
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Disconnect))]
        private static void Disconnect(GameNetworkManager __instance)
        {
            DoSave();
        }

        public static void DoSave(SaveType type = SaveType.PlayerPrefs)
        {
            SaveData saveData = new SaveData();
            saveData.steamId = GameNetworkManager.Instance.localPlayerController.playerSteamId;
            saveData.skillPoints = LP_NetworkManager.xpInstance.skillPoints;

            foreach (KeyValuePair<UpgradeType, Skill> skill in LP_NetworkManager.xpInstance.skillList.skills)
            {
                LethalPlugin.Log.LogInfo($"Skill is {skill.Key} and value is {skill.Value.GetLevel()}");
                saveData.skillAllocation.Add(skill.Key, skill.Value.GetLevel());
            }

            string data = JsonConvert.SerializeObject(saveData);
            LP_NetworkManager.xpInstance.SaveData_ServerRpc(saveData.steamId, data, type);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeleteFileButton), nameof(DeleteFileButton.DeleteFile))]
        private static void DeleteSaveFile(DeleteFileButton __instance)
        {
            SaveManager.DeleteSave(__instance.fileToDelete);
        }
    }
}
