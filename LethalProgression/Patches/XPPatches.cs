using HarmonyLib;
using LethalProgression.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Netcode;

namespace LethalProgression.Patches
{
    internal class XPPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "SaveGameValues")]
        private static void SaveXPValues(GameNetworkManager __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                return;
            }
            if (!StartOfRound.Instance.inShipPhase)
            {
                return;
            }
            // We don't care about the version number. If we disconnect, we want to save our XP.
            // Make a new file.
            int saveFileNum = __instance.saveFileNum + 1;
            string path = Application.persistentDataPath + "/lethalprogression/save" + saveFileNum + ".txt";

            string xpBuild = "";
            xpBuild += LP_NetworkManager.xpInstance.GetLevel() + "\n";
            xpBuild += LP_NetworkManager.xpInstance.GetXP() + "\n";
            xpBuild += LP_NetworkManager.xpInstance.GetProfit() + "\n";
            System.IO.File.WriteAllText(path, xpBuild);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "FirePlayersAfterDeadlineClientRpc")]
        private static void ResetXPValues(StartOfRound __instance)
        {
            var xpInstance = LP_NetworkManager.xpInstance;
            int saveFileNum = GameNetworkManager.Instance.saveFileNum + 1;
            string path = Application.persistentDataPath + "/lethalprogression/save" + saveFileNum + ".txt";

            // Delete the file.
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // Make a new file.
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/lethalprogression");
            System.IO.File.WriteAllText(path, "");

            string xpBuild = "";
            xpBuild += "0\n";
            xpBuild += "0\n";
            xpBuild += "0\n";

            System.IO.File.WriteAllText(path, xpBuild);

            xpInstance.xpReq.Value = xpInstance.GetXPRequirement();
            foreach (Skill skill in xpInstance.skillList.skills.Values)
            {
                skill.SetLevel(0);
            }


            xpInstance.SetSkillPoints(5);
            xpInstance.xpLevel.Value = 0;
            xpInstance.xpPoints.Value = 0;
            xpInstance.profit.Value = 0;
            xpInstance.teamLootValue.Value = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeleteFileButton), "DeleteFile")]
        private static void XPFileDeleteReset()
        {
            int saveFileNum = GameNetworkManager.Instance.saveFileNum + 1;
            string path = Application.persistentDataPath + "/lethalprogression/save" + saveFileNum + ".txt";

            // Delete the file.
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // Make a new file.
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/lethalprogression");
            System.IO.File.WriteAllText(path, "");

            string xpBuild = "";
            xpBuild += "0\n";
            xpBuild += "0\n";
            xpBuild += "0\n";

            System.IO.File.WriteAllText(path, xpBuild);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        private static void DisconnectXPHandler()
        {
            int lootLevel = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.HandSlot].GetLevel();
            LP_NetworkManager.xpInstance.TeamLootValueUpdate(-lootLevel, 0);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        private static void ProfitQuotaUpdate(TimeOfDay __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                return;
            }
            LP_NetworkManager.xpInstance.xpReq.Value = LP_NetworkManager.xpInstance.GetXPRequirement();
        }
    }
}
