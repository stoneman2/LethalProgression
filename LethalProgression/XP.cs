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
using Newtonsoft.Json;

namespace LethalProgression
{
    internal class XP : NetworkBehaviour
    {
        public NetworkVariable<int> xpPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> xpLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> profit = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> xpReq = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Special boys
        public NetworkVariable<float> teamLootValue = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public int skillPoints;
        public SkillList skillList;
        public SkillsGUI guiObj;
        public bool Initialized = false;
        public void Start()
        {
            LethalPlugin.Log.LogInfo("XP Network Behavior Made!");
            PlayerConnect_ServerRpc();
        }
        public void LoadSaveData()
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
            HUDManagerPatch.ShowLevelUp();
        }

        [ClientRpc]
        public void Givepoint_ClientRPC()
        {
            skillPoints++;
        }

        /////////////////////////////////////////////////
        /// Team Loot Upgrade Sync
        /////////////////////////////////////////////////
        public void TeamLootValueUpdate(float update, int newValue)
        {
            TeamLootValueUpdate_ServerRpc(update);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeamLootValueUpdate_ServerRpc(float updatedValue)
        {
            float mult = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Value].GetMultiplier();
            float newvalue = updatedValue * mult;
            teamLootValue.Value += newvalue;
            LethalPlugin.Log.LogInfo($"Changed team loot value by {updatedValue * mult} turning into {teamLootValue.Value}.");
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
                        //LethalPlugin.Log.LogInfo($"Setting slot {i}!");
                        if (objects.Count < newAmount)
                        {
                            continue;
                        }
                        player.ItemSlots[i] = objects[i];
                    }
                    LethalPlugin.Log.LogInfo($"Player {playerID} has {player.ItemSlots.Length} slots after setting.");
                    break;
                }
            }
        }

        // When joining.
        [ServerRpc(RequireOwnership = false)]
        public void GetEveryoneHandSlots_ServerRpc()
        {
            if (LethalPlugin.ReservedSlots)
                return;

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.HandSlot))
            {
                return;
            }

            // Compile a list of all players and their handslots.
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!player.gameObject.activeSelf)
                    continue;
                ulong playerID = player.playerClientId;
                int handSlots = player.ItemSlots.Length - 4;
                SendEveryoneHandSlots_ClientRpc(playerID, handSlots);
            }
        }

        [ClientRpc]
        public void SendEveryoneHandSlots_ClientRpc(ulong playerID, int handSlots)
        {
            SetHandSlot(playerID, handSlots);
        }

        // CONFIGS
        [ServerRpc(RequireOwnership = false)]
        public void PlayerConnect_ServerRpc()
        {
            IDictionary<string, string> configDict = LethalPlugin.Instance.GetAllConfigEntries();
            string serializedConfig = JsonConvert.SerializeObject(configDict);
            SendEveryoneConfigs_ClientRpc(serializedConfig);
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

            if (!Initialized)
            {
                Initialized = true;
                LP_NetworkManager.xpInstance = this;
                skillList = new SkillList();
                skillList.InitializeSkills();

                guiObj = new SkillsGUI();

                teamLootValue.OnValueChanged += guiObj.TeamLootHudUpdate;

                if (GameNetworkManager.Instance.isHostingGame)
                {
                    LoadSaveData();
                }

                skillPoints = xpLevel.Value + 5;

                GetEveryoneHandSlots_ServerRpc();

                ChangeXPRequirement_ServerRpc();
            }
        }
    }
}