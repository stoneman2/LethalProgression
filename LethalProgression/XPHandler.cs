using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;
using Unity.Networking;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using GameNetcodeStuff;

namespace LethalProgression
{
    [HarmonyPatch]
    internal class XPHandler
    {
        // Saving and loading and syncing.
        public static XP xpInstance;

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
            xpBuild += xpInstance.GetLevel() + "\n";
            xpBuild += xpInstance.GetXP() + "\n";
            xpBuild += xpInstance.GetProfit() + "\n";
            System.IO.File.WriteAllText(path, xpBuild);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "FirePlayersAfterDeadlineClientRpc")]
        private static void ResetXPValues(StartOfRound __instance)
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

            foreach (KeyValuePair<string, LethalProgression.Skills.Skill> skill in LethalProgression.XPHandler.xpInstance.skillList.skills)
            {
                skill.Value.SetLevel(0);
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
            int lootLevel = LethalProgression.XPHandler.xpInstance.skillList.skills["Scrap Value"].GetLevel();
            xpInstance.TeamLootValueUpdate(-lootLevel, 0);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        private static void ProfitQuotaUpdate(TimeOfDay __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                return;
            }
            LethalProgression.XPHandler.xpInstance.xpReq.Value = LethalProgression.XPHandler.xpInstance.GetXPRequirement();
        }

        internal static void ClientConnectInitializer(Scene sceneName, LoadSceneMode sceneEnum)
        {

            if (sceneName.name == "SampleSceneRelay")
            {
                GameObject xpHandlerObj = new GameObject("XPHandler");
                xpHandlerObj.AddComponent<NetworkObject>();
                xpInstance = xpHandlerObj.AddComponent<XP>();
                LethalProgress.Log.LogInfo("Initialized XPHandler.");
            }
        }
    }
    internal class XP : NetworkBehaviour
    {
        public NetworkVariable<int> xpPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> xpLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> profit = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> xpReq = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Special boys
        public NetworkVariable<int> teamLootValue = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public int skillPoints;
        public LethalProgression.Skills.SkillList skillList;
        public LethalProgression.GUI.SkillsGUI guiObj;
        public void Start()
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                int savefileNum = GameNetworkManager.Instance.saveFileNum + 1;
                string path = Application.persistentDataPath + "/lethalprogression/save" + savefileNum + ".txt";
                if (!System.IO.File.Exists(path))
                {
                    // Make directory and empty file.
                    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/lethalprogression");
                    System.IO.File.WriteAllText(path, "");

                    string xpBuild = "";
                    xpBuild += "0\n";
                    xpBuild += "0\n";
                    xpBuild += "0\n";

                    System.IO.File.WriteAllText(path, xpBuild);

                }

                string[] lines = System.IO.File.ReadAllLines(path);

                LethalProgress.Log.LogError("Loading XP!");
                xpLevel.Value = int.Parse(lines[0]);
                xpPoints.Value = int.Parse(lines[1]);
                profit.Value = int.Parse(lines[2]);
                xpReq.Value = GetXPRequirement();

                LethalProgress.Log.LogInfo(GetXPRequirement().ToString());
            }

            // For now: Give 1 skillpoint per level.
            skillPoints = xpLevel.Value + 5;

            skillList = new LethalProgression.Skills.SkillList();
            guiObj = new LethalProgression.GUI.SkillsGUI();

            teamLootValue.OnValueChanged += guiObj.TeamLootHudUpdate;

            GetEveryoneHandSlots_ServerRpc();

            ChangeXPRequirement_ServerRpc();
        }
        public int GetXPRequirement()
        {
            // First, we need to check how many players.
            int playerCount = StartOfRound.Instance.connectedPlayersAmount;
            int quota = TimeOfDay.Instance.timesFulfilledQuota;

            int initialXPCost = LethalProgress.configXPMin.Value;

            int personScale = LethalProgress.configPersonScale.Value;
            int personValue = playerCount * personScale;
            int req = initialXPCost + personValue;

            // Quota multiplier
            int quotaMult = LethalProgress.configQuotaMult.Value;
            int quotaVal = quota * quotaMult;
            req += (int)(req * (quotaVal / 100f));

            LethalProgress.Log.LogInfo($"{playerCount} players, {quota} quotas, {initialXPCost} initial cost, {personValue} person value, {quotaVal} quota value, {req} total cost.");
            return req;
        }

        public int GetXP()
        {
            return xpPoints.Value;
        }

        public int GetLevel()
        {
            return xpLevel.Value;
        }

        public int GetProfit()
        {
            return profit.Value;
        }

        public int GetSkillPoints()
        {
            return skillPoints;
        }

        public void SetSkillPoints(int num)
        {
            skillPoints = num;
        }

        public void AddSkillPoint()
        {
            skillPoints++;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeXPRequirement_ServerRpc()
        {
            StartCoroutine(XPRequirementCoroutine());
        }

        public IEnumerator XPRequirementCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            xpReq.Value = GetXPRequirement();
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddXPServerRPC(int xp)
        {
            int oldXP = GetXP();

            // Update XP values
            xpPoints.Value += xp;
            profit.Value += xp;

            int newXP = GetXP();

            XPHUDUpdate_ClientRPC(oldXP, newXP, xp);

            // If we have enough XP to level up.
            if (newXP >= xpReq.Value)
            {
                // How many times do we level up?
                int levelUps = 0;

                while (newXP >= xpReq.Value)
                {
                    levelUps++;
                    newXP -= xpReq.Value;
                    Givepoint_ClientRPC();
                }

                // Update XP values
                xpPoints.Value = newXP;
                xpLevel.Value += levelUps;


                LevelUp_ClientRPC();
            }
        }

        [ClientRpc]
        public void XPHUDUpdate_ClientRPC(int oldXP, int newXP, int xpGained)
        {
            LethalProgression.Patches.HUDManagerPatch.ShowXPUpdate(oldXP, newXP, newXP - oldXP);
        }

        [ClientRpc]
        public void LevelUp_ClientRPC()
        {
            LethalProgression.Patches.HUDManagerPatch.ShowLevelUp();
        }

        [ClientRpc]
        public void Givepoint_ClientRPC()
        {
            skillPoints++;
        }

        /////////////////////////////////////////////////
        /// Team Loot Upgrade Sync
        /////////////////////////////////////////////////
        public void TeamLootValueUpdate(int oldValue, int newValue)
        {
            TeamLootValueUpdate_ServerRpc(oldValue);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeamLootValueUpdate_ServerRpc(int updatedValue)
        {
            LethalProgression.XPHandler.xpInstance.teamLootValue.Value += updatedValue;
            LethalProgress.Log.LogInfo($"Team loot value updated by {updatedValue}.");
        }

        /////////////////////////////////////////////////
        /// Hand Slot Sync
        /////////////////////////////////////////////////

        // When updates.
        [ServerRpc(RequireOwnership = false)]
        public void ServerHandSlots_ServerRpc(int newSlots, ulong playerID)
        {
            SetPlayerHandslots_ClientRpc(newSlots, playerID);
        }

        [ClientRpc]
        public void SetPlayerHandslots_ClientRpc(int newSlots, ulong playerID)
        {
            SetHandSlot(newSlots, playerID);
        }

        public void SetHandSlot(int newSlots, ulong playerID)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == playerID)
                {
                    List<GrabbableObject> objects = new List<GrabbableObject>(player.ItemSlots);
                    player.ItemSlots = new GrabbableObject[4 + newSlots];
                    for (int i = 0; i < objects.Count; i++)
                    {
                        player.ItemSlots[i] = objects[i];
                    }
                    break;
                }
            }
        }

        // When joining.
        [ServerRpc(RequireOwnership = false)]
        public void GetEveryoneHandSlots_ServerRpc()
        {
            Dictionary<ulong, int> handSlotDict = new Dictionary<ulong, int>();

            // Compile a list of all players and their handslots.
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];
                ulong playerID = player.playerClientId;
                int handSlots = (int)LethalProgression.XPHandler.xpInstance.skillList.skills["Hand Slot"].GetTrueValue();
                SendEveryoneHandSlots_ClientRpc(playerID, handSlots);
            }
        }

        [ClientRpc]
        public void SendEveryoneHandSlots_ClientRpc(ulong playerID, int handSlots)
        {
            SetHandSlot(handSlots, playerID);
        }
    }
}