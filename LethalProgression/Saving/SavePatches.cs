using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using LethalProgression.Skills;
using Newtonsoft.Json;

namespace LethalProgression.Saving
{
    [HarmonyPatch]
    internal class SavePatches
    {
        // Whenever a player disconnects, do save!
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        private static void SaveGame(GameNetworkManager __instance)
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
            LP_NetworkManager.xpInstance.SaveData_ServerRpc(GameNetworkManager.Instance.localPlayerController.playerSteamId, data);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeleteFileButton), "DeleteFile")]
        private static void DeleteSaveFile(DeleteFileButton __instance)
        {
            SaveManager.DeleteSave(__instance.fileToDelete);
        }
    }
}
