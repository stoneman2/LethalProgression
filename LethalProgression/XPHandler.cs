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
using LethalProgression.Config;
using LethalProgression.Skills;
using LethalProgression.GUI;
using LethalProgression.Patches;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System.Linq;

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

            xpInstance.SetSkillPoints(5);
            xpInstance.xpLevel.Value = 0;
            xpInstance.xpPoints.Value = 0;
            xpInstance.profit.Value = 0;
            xpInstance.teamLootValue.Value = 0;
            xpInstance.xpReq.Value  = xpInstance.GetXPRequirement();

            foreach(Skill skill in xpInstance.skillList.skills.Values)
            {
                skill.SetLevel(0);
            }
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
            int lootLevel = xpInstance.skillList.skills[UpgradeType.HandSlot].GetLevel();
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
            xpInstance.xpReq.Value = xpInstance.GetXPRequirement();
        }

        internal static void ClientConnectInitializer(Scene sceneName, LoadSceneMode sceneEnum)
        {

            if (sceneName.name == "SampleSceneRelay")
            {
                GameObject xpHandlerObj = new GameObject("XPHandler");
                xpHandlerObj.AddComponent<NetworkObject>();
                xpInstance = xpHandlerObj.AddComponent<XP>();
                LethalPlugin.Log.LogInfo("Initialized XPHandler.");
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
            SyncConfig_ServerRpc();

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

                LethalPlugin.Log.LogError("Loading XP!");
                xpLevel.Value = int.Parse(lines[0]);
                xpPoints.Value = int.Parse(lines[1]);
                profit.Value = int.Parse(lines[2]);
                xpReq.Value = GetXPRequirement();

                LethalPlugin.Log.LogInfo(GetXPRequirement().ToString());
            }

            // For now: Give 1 skillpoint per level.
            skillPoints = xpLevel.Value + 5;

            skillList = new SkillList();
            skillList.InitializeSkills();

            guiObj = new SkillsGUI();


            teamLootValue.OnValueChanged += guiObj.TeamLootHudUpdate;

            HandSlotSync_ServerRpc();
            ChangeXPRequirement_ServerRpc();
        }

        public int GetXPRequirement()
        {
            // First, we need to check how many players.
            int playerCount = StartOfRound.Instance.connectedPlayersAmount;
            int quota = TimeOfDay.Instance.timesFulfilledQuota;

            int initialXPCost = int.Parse(SkillConfig.hostConfig["XP Minimum"]);
            int maxXPCost = int.Parse(SkillConfig.hostConfig["XP Maximum"]);

            int personScale = int.Parse(SkillConfig.hostConfig["Person Multiplier"]);
            int personValue = playerCount * personScale;
            int req = initialXPCost + personValue;

            // Quota multiplier
            int quotaMult = int.Parse(SkillConfig.hostConfig["Quota Multiplier"]);
            int quotaVal = quota * quotaMult;
            req += (int)(req * (quotaVal / 100f));

            if (req > maxXPCost)
            {
                req = maxXPCost;
            }

            LethalPlugin.Log.LogInfo($"{playerCount} players, {quota} quotas, {initialXPCost} initial cost, {personValue} person value, {quotaVal} quota value, {req} total cost.");
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
            HUDManagerPatch.ShowXPUpdate(oldXP, newXP, newXP - oldXP);
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
            LethalPlugin.Log.LogInfo($"Team loot value updated by {updatedValue}.");
        }

        /////////////////////////////////////////////////
        /// Hand Slot Sync
        /////////////////////////////////////////////////

        // When updates.
        [ServerRpc(RequireOwnership = false)]
        public void ServerHandSlots_ServerRpc(ulong playerID, int newSlots)
        {
            if (LethalPlugin.ReservedSlots)
                return;

            SetPlayerHandslots_ClientRpc(playerID, newSlots);
        }

        [ClientRpc]
        public void SetPlayerHandslots_ClientRpc(ulong playerID, int newSlots)
        {
            SetHandSlot(playerID, newSlots);
        }

        public void SetHandSlot(ulong playerID, int newSlots)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == playerID)
                {
                    int newAmount = 4 + newSlots;
                    List<GrabbableObject> objects = new List<GrabbableObject>(player.ItemSlots);

                    player.ItemSlots = new GrabbableObject[newAmount];
                    for (int i = 0; i < newAmount; i++)
                    {
                        LethalPlugin.Log.LogInfo($"Setting slot {i}!");
                        if (objects.Count < newAmount)
                            continue;

                        player.ItemSlots[i] = objects[i];
                    }
                    LethalPlugin.Log.LogInfo($"Player {playerID} has {player.ItemSlots.Length} slots after setting.");
                    break;
                }
            }
        }

        // When joining.
        [ServerRpc(RequireOwnership = false)]
        public void HandSlotSync_ServerRpc()
        {
            if (LethalPlugin.ReservedSlots)
                return;

            // Compile a list of all players and their handslots.
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!player.gameObject.activeSelf)
                    continue;
                ulong playerID = player.playerClientId;
                int handSlots = (int)skillList.skills[UpgradeType.HandSlot].GetTrueValue();
                SendEveryoneHandSlots_ClientRpc(playerID, handSlots);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncConfig_ServerRpc()
        {
            IDictionary<string, string> localConfig = SkillConfig.configFile.GetConfigEntries().ToDictionary(
                entry => entry.Definition.Key,
                entry => entry.GetSerializedValue()
            );

            string serializedConfig = JsonConvert.SerializeObject(localConfig);
            SendEveryoneConfigs_ClientRpc(serializedConfig);
        }

        [ClientRpc]
        public void SendEveryoneHandSlots_ClientRpc(ulong playerID, int handSlots)
        {
            SetHandSlot(playerID, handSlots);
        }

        [ClientRpc]
        public void SendEveryoneConfigs_ClientRpc(string serializedConfig)
        {
            IDictionary<string, string> configs = JsonConvert.DeserializeObject<IDictionary<string, string>>(serializedConfig);
            // Apply configs.
            foreach (KeyValuePair<string, string> entry in configs)
            {
                SkillConfig.hostConfig[entry.Key] = entry.Value;
                LethalPlugin.Log.LogInfo($"Loaded host config: {entry.Key} = {entry.Value}");
            }
        }
    }
}