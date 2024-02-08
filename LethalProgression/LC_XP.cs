using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using LethalProgression.Config;
using LethalProgression.GUI;
using LethalProgression.Patches;
using LethalProgression.Saving;
using LethalProgression.Skills;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace LethalProgression
{
    internal class LC_XP : NetworkBehaviour
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
        public bool loadedSave = false;
        public void Start()
        {
            LethalPlugin.Log.LogInfo("XP Network Behavior Made!");
            PlayerConnect_ServerRpc();
        }
        public void LoadSharedSaveData()
        {
            SaveSharedData sharedData = SaveManager.LoadShared();

            if (sharedData == null)
            {
                LethalPlugin.Log.LogInfo("Shared data is null!");
                return;
            }
            LethalPlugin.Log.LogInfo("Loading XP!");
            xpLevel.Value = sharedData.level;
            xpPoints.Value = sharedData.xp;
            profit.Value = sharedData.quota;
            xpReq.Value = GetXPRequirement();

            LethalPlugin.Log.LogInfo(GetXPRequirement().ToString());
        }
        public void LoadLocalData(string data)
        {
            if (loadedSave)
            {
                return;
            }

            loadedSave = true;
            LethalPlugin.Log.LogInfo("Loading local XP!");
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(data);
            skillPoints = saveData.skillPoints;

            int skillCheck = 0;
            foreach (KeyValuePair<UpgradeType, int> skill in saveData.skillAllocation)
            {
                skillList.skills[skill.Key].AddLevel(skill.Value);
                skillCheck += skill.Value;
            }
            LethalPlugin.Log.LogInfo(GetXPRequirement().ToString());
            if ((skillCheck + skillPoints) < xpLevel.Value + 5)
            {
                LethalPlugin.Log.LogInfo($"Skill check is less than current level, adding {((xpLevel.Value + 5) - (skillCheck + skillPoints))} skill points.");
                skillPoints += (xpLevel.Value + 5) - (skillCheck + skillPoints);
            }
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
        public void TeamLootValueUpdate(float update)
        {
            TeamLootValueUpdate_ServerRpc(update);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeamLootValueUpdate_ServerRpc(float updatedValue)
        {
            float mult = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Value].GetMultiplier();
            float value = updatedValue * mult;

            teamLootValue.Value += value;
            if (value == 0)
            {
                teamLootValue.Value = value;
            }
            LethalPlugin.Log.LogInfo($"Changed team loot value by {updatedValue * mult} turning into {teamLootValue.Value}.");
        }

        /////////////////////////////////////////////////
        /// Hand Slot Sync
        /////////////////////////////////////////////////

        // When updates.
        [ServerRpc(RequireOwnership = false)]
        public void ServerHandSlots_ServerRpc(ulong playerID, int slotChange)
        {
            if (LethalPlugin.ReservedSlots)
                return;

            // Send the request to all clients
            SetPlayerHandslots_ClientRpc(playerID, slotChange);
        }

        [ClientRpc]
        public void SetPlayerHandslots_ClientRpc(ulong playerID, int slotChange)
        {
            SetHandSlot(playerID, slotChange);
        }

        public void SetHandSlot(ulong playerID, int newSlots)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == playerID)
                {
                    int newAmount = 4 + newSlots;
                    List<GrabbableObject> objects = player.ItemSlots.ToList<GrabbableObject>();

                    if (player.currentItemSlot > newAmount - 1)
                    {
                        HandSlots.SwitchItemSlots(player, newAmount - 1);
                    }

                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (i > newAmount - 1)
                        {
                            // In a slot that is getting removed, drop it instead
                            if (objects[i] != null)
                            {
                                HandSlots.SwitchItemSlots(player, i);
                                player.DiscardHeldObject();
                            }
                        }
                    }

                    player.ItemSlots = new GrabbableObject[newAmount];
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (objects[i] == null)
                        {
                            continue;
                        }
                        player.ItemSlots[i] = objects[i];
                    }
                    LethalPlugin.Log.LogDebug($"Player {playerID} has {player.ItemSlots.Length} slots after setting.");

                    if (player == GameNetworkManager.Instance.localPlayerController)
                    {
                        LethalPlugin.Log.LogDebug($"Updating HUD slots.");
                        HandSlots.UpdateHudSlots();
                    }
                    break;
                }
            }
        }

        // When joining.
        [ServerRpc(RequireOwnership = false)]
        public void GetEveryoneHandSlots_ServerRpc()
        {
            if (LethalPlugin.ReservedSlots || !LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.HandSlot))
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

                if (GameNetworkManager.Instance.isHostingGame)
                {
                    LoadSharedSaveData();
                }

                guiObj = new SkillsGUI();
                teamLootValue.OnValueChanged += guiObj.TeamLootHudUpdate;

                skillPoints = xpLevel.Value + 5;

                GetEveryoneHandSlots_ServerRpc();

                ChangeXPRequirement_ServerRpc();
            }
        }

        // Saving
        [ServerRpc(RequireOwnership = false)]
        public void SaveData_ServerRpc(ulong steamID, string saveData, SaveType type = SaveType.PlayerPrefs)
        {
            if (type == SaveType.Json)
            {
                SaveMigrator.MigrateSaves();
                return;
            }
            SaveManager.Save(steamID, saveData);
            SaveManager.SaveShared(xpPoints.Value, xpLevel.Value, profit.Value);
        }

        // Loading
        [ServerRpc(RequireOwnership = false)]
        public void RequestSavedData_ServerRpc(ulong? steamID)
        {
            string saveData = SaveManager.Load(steamID);
            SendSavedData_ClientRpc(saveData);
        }

        [ClientRpc]
        public void SendSavedData_ClientRpc(string saveData)
        {
            if (saveData == null)
            {
                return;
            }

            LoadLocalData(saveData);
        }
    }
}