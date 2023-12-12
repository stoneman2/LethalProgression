using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LethalProgression.Config;
using System.Globalization;

namespace LethalProgression.Skills
{
    public enum UpgradeType
    {
        HPRegen,
        Stamina,
        Battery,
        HandSlot,
        Value,
        Oxygen,
    }

    internal class SkillList
    {
        public Dictionary<UpgradeType, Skill> skills = new Dictionary<UpgradeType, Skill>();

        public void CreateSkill(UpgradeType upgrade, string name, string description, string shortname, string attribute, UpgradeType upgradeType, int cost, int maxLevel, float multiplier, Action<int, int> callback = null, bool teamShared = false)
        {
            Skill newSkill = new Skill(name, description, shortname, attribute, upgradeType, cost, maxLevel, multiplier, callback, teamShared);
            skills.Add(upgrade, newSkill);
        }

        public bool IsSkillListValid()
        {
            if (skills.Count == 0)
            {
                return false;
            }

            return true;
        }

        public bool IsSkillValid(UpgradeType upgrade)
        {
            if (!skills.ContainsKey(upgrade))
            {
                LethalPlugin.Log.LogInfo("Skill " + upgrade.ToString() + " is not in the skill list!");
                return false;
            }

            return true;
        }

        public void InitializeSkills()
        {
            LethalPlugin.Log.LogInfo("Trying to initialize skills...");

            LethalPlugin.Log.LogInfo("Checking health regen");
            if (bool.Parse(SkillConfig.hostConfig["Health Regen Enabled"]))
            {
                LethalPlugin.Log.LogInfo("HP Regen check 1");
                CreateSkill(UpgradeType.HPRegen,
                    "Health Regen",
                    "Your body heals itself faster, allowing you to recover from injuries quicker. Only regenerate up to 100 HP.",
                    "HPR",
                    "Health Regeneration",
                    UpgradeType.HPRegen,
                    1,
                    int.Parse(SkillConfig.hostConfig["Health Regen Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Health Regen Multiplier"], CultureInfo.InvariantCulture));
                LethalPlugin.Log.LogInfo("HP Regen check 2");

            }

            LethalPlugin.Log.LogInfo("Checking stamina");
            if (bool.Parse(SkillConfig.hostConfig["Stamina Enabled"]))
            {
                LethalPlugin.Log.LogInfo("Stamina check 1");
                CreateSkill(UpgradeType.Stamina,
                    "Stamina",
                    "The company gives you a better pair of lungs, allowing you to run for longer.",
                    "STM",
                    "Stamina",
                    UpgradeType.Stamina,
                    1,
                    int.Parse(SkillConfig.hostConfig["Stamina Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Stamina Multiplier"], CultureInfo.InvariantCulture),
                    Stamina.StaminaUpdate);
                LethalPlugin.Log.LogInfo("Stamina check 2");
            }

            LethalPlugin.Log.LogInfo("Checking battery");
            if (bool.Parse(SkillConfig.hostConfig["Battery Life Enabled"]))
            {
                LethalPlugin.Log.LogInfo("Battery check 1");
                CreateSkill(UpgradeType.Battery,
                    "Battery Life",
                    "You brought better batteries. Replace your batteries AT THE SHIP'S CHARGER to see an effect.",
                    "BAT",
                    "Battery Life",
                    UpgradeType.Battery,
                    1,
                    int.Parse(SkillConfig.hostConfig["Battery Life Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Battery Life Multiplier"], CultureInfo.InvariantCulture));
                LethalPlugin.Log.LogInfo("Battery check 2");
            }

            LethalPlugin.Log.LogInfo("Checking hand slots");
            if (bool.Parse(SkillConfig.hostConfig["Hand Slots Enabled"]) && !LethalPlugin.ReservedSlots)
            {
                LethalPlugin.Log.LogInfo("Hand Slots check 1");
                CreateSkill(UpgradeType.HandSlot,
                     "Hand Slot",
                     "The company finally gives you a better belt! Fit more stuff! (Reach 100% for one more slot! 10 per slot.)",
                     "HND",
                     "Hand Slots",
                     UpgradeType.HandSlot,
                     1,
                     int.Parse(SkillConfig.hostConfig["Hand Slots Max Level"]),
                     float.Parse(SkillConfig.hostConfig["Hand Slots Multiplier"], CultureInfo.InvariantCulture),
                     HandSlots.HandSlotsUpdate);
                LethalPlugin.Log.LogInfo("Hand Slots check 2");
            }

            LethalPlugin.Log.LogInfo("Checking loot value");
            if (bool.Parse(SkillConfig.hostConfig["Loot Value Enabled"]))
            {
                LethalPlugin.Log.LogInfo("Loot Value check 1");
                CreateSkill(UpgradeType.Value,
                    "Loot Value",
                    "The company gives you a better pair of eyes, allowing you to see the value in things.",
                    "VAL",
                    "Loot Value",
                    UpgradeType.Value,
                    1,
                    int.Parse(SkillConfig.hostConfig["Loot Value Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Loot Value Multiplier"], CultureInfo.InvariantCulture),
                    LootValue.LootValueUpdate);
                LethalPlugin.Log.LogInfo("Loot Value check 2");
            }

            LethalPlugin.Log.LogInfo("Checking oxygen");
            if (bool.Parse(SkillConfig.hostConfig["Oxygen Enabled"]))
            {
                LethalPlugin.Log.LogInfo("Oxygen check 1");
                CreateSkill(UpgradeType.Oxygen,
                    "Oxygen",
                    "The company gives you a better pair of lungs, allowing you to hold your breath for longer.",
                    "OXY",
                    "Oxygen",
                    UpgradeType.Oxygen,
                    1,
                    int.Parse(SkillConfig.hostConfig["Oxygen Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Oxygen Multiplier"], CultureInfo.InvariantCulture));
                LethalPlugin.Log.LogInfo("Oxygen check 2");
            }
        }
    }

    internal class Skill
    {
        private readonly string _shortName;
        private readonly string _name;
        private readonly string _attribute;
        private readonly string _description;
        private readonly UpgradeType _upgradeType;
        private readonly int _cost;
        private readonly int _maxLevel;
        private readonly float _multiplier;
        private readonly Action<int, int> _callback;
        public bool _teamShared;
        private int _level;
        public Skill(string name, string description, string shortname, string attribute, UpgradeType upgradeType, int cost, int maxLevel, float multiplier, Action<int, int> callback = null, bool teamShared = false)
        {
            _name = name;
            _shortName = shortname;
            _attribute = attribute;
            _description = description;
            _upgradeType = upgradeType;
            _cost = cost;
            _maxLevel = maxLevel;
            _multiplier = multiplier;
            _level = 0;
            _callback = callback;
            _teamShared = teamShared;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetShortName()
        {
            return _shortName;
        }

        public string GetAttribute()
        {
            return _attribute;
        }

        public string GetDescription()
        {
            return _description;
        }

        public UpgradeType GetUpgradeType()
        {
            return _upgradeType;
        }

        public int GetCost()
        {
            return _cost;
        }

        public int GetMaxLevel()
        {
            return _maxLevel;
        }

        public int GetLevel()
        {
            return _level;
        }

        public float GetMultiplier()
        {
            return _multiplier;
        }

        public float GetTrueValue()
        {
            return _multiplier * _level;
        }

        public void SetLevel(int level)
        {
            _level = level;
        }

        public void AddLevel(int level)
        {
            _level += level;
            int newLevel = _level;

            _callback?.Invoke(level, newLevel);
        }
    }
}
