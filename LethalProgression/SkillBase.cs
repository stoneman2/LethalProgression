using System;
using System.Collections.Generic;
using System.Globalization;
using LethalProgression.Config;

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
        JumpHeight,
        SprintSpeed,
        Strength
    }

    internal class SkillList
    {
        public Dictionary<UpgradeType, Skill> skills = new Dictionary<UpgradeType, Skill>();

        public void CreateSkill(UpgradeType upgrade, string name, string description, string shortname, string attribute, int cost, int maxLevel, float multiplier, Action<int, int> callback = null, bool teamShared = false)
        {
            Skill newSkill = new Skill(name, description, shortname, attribute, upgrade, cost, maxLevel, multiplier, callback, teamShared);
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

        public Skill GetSkill(UpgradeType upgrade)
        {
            if (!IsSkillValid(upgrade))
            {
                return null;
            }

            return skills[upgrade];
        }

        public Dictionary<UpgradeType, Skill> GetSkills()
        {
            return skills;
        }

        public void InitializeSkills()
        {
            if (bool.Parse(SkillConfig.hostConfig["Health Regen Enabled"]))
            {
                LethalPlugin.Log.LogInfo("HP Regen check 1");
                CreateSkill(UpgradeType.HPRegen,
                    "Health Regen",
                    "The company installs a basic healer into your suit, letting you regenerate health slowly. Only regenerate up to 100 HP.",
                    "HPR",
                    "Health Regeneration",
                    1,
                    int.Parse(SkillConfig.hostConfig["Health Regen Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Health Regen Multiplier"], CultureInfo.InvariantCulture));
            }

            if (bool.Parse(SkillConfig.hostConfig["Stamina Enabled"]))
            {
                CreateSkill(UpgradeType.Stamina,
                    "Stamina",
                    "Hours on that company gym finally coming into play. Allows you to run for longer, but has to regenerate it slower.",
                    "STM",
                    "Stamina",
                    1,
                    int.Parse(SkillConfig.hostConfig["Stamina Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Stamina Multiplier"], CultureInfo.InvariantCulture),
                    Stamina.StaminaUpdate);
            }

            if (bool.Parse(SkillConfig.hostConfig["Battery Life Enabled"]))
            {
                CreateSkill(UpgradeType.Battery,
                    "Battery Life",
                    "The company provides you with better batteries. Replace your batteries AT THE SHIP'S CHARGER to see an effect.",
                    "BAT",
                    "Battery Life",
                    1,
                    int.Parse(SkillConfig.hostConfig["Battery Life Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Battery Life Multiplier"], CultureInfo.InvariantCulture));
            }

            if (bool.Parse(SkillConfig.hostConfig["Hand Slots Enabled"]) && !LethalPlugin.ReservedSlots)
            {
                /*
                 CreateSkill(UpgradeType.HandSlot,
                     "Hand Slot",
                     "The company finally gives you a better belt! Fit more stuff! (One slot every 100%.)",
                     "HND",
                     "Hand Slots",
                     1,
                     int.Parse(SkillConfig.hostConfig["Hand Slots Max Level"]),
                     float.Parse(SkillConfig.hostConfig["Hand Slots Multiplier"], CultureInfo.InvariantCulture),
                     HandSlots.HandSlotsUpdate);
                */
            }

            if (bool.Parse(SkillConfig.hostConfig["Loot Value Enabled"]))
            {
                CreateSkill(UpgradeType.Value,
                    "Loot Value",
                    "The company gives you a better pair of eyes, allowing you to see the value in things.",
                    "VAL",
                    "Loot Value",
                    1,
                    int.Parse(SkillConfig.hostConfig["Loot Value Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Loot Value Multiplier"], CultureInfo.InvariantCulture),
                    LootValue.LootValueUpdate);
            }

            if (bool.Parse(SkillConfig.hostConfig["Oxygen Enabled"]))
            {
                CreateSkill(UpgradeType.Oxygen,
                    "Oxygen",
                    "The company installs you with oxygen tanks. You gain extra time in the water. (Start drowning when the bar is empty.)",
                    "OXY",
                    "Extra Oxygen",
                    1,
                    int.Parse(SkillConfig.hostConfig["Oxygen Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Oxygen Multiplier"], CultureInfo.InvariantCulture));
            }

            if (bool.Parse(SkillConfig.hostConfig["Strength Enabled"]))
            {
                CreateSkill(UpgradeType.Strength,
                    "Strength",
                    "More work at the Company's gym gives you pure muscles! You can carry better. (Reduces weight by a percentage.)",
                    "STR",
                    "Weight Reduction",
                    1,
                    int.Parse(SkillConfig.hostConfig["Strength Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Strength Multiplier"], CultureInfo.InvariantCulture),
                    Strength.StrengthUpdate);
            }

            if (bool.Parse(SkillConfig.hostConfig["Jump Height Enabled"]))
            {
                CreateSkill(UpgradeType.JumpHeight,
                    "Jump Height",
                    "The company installs you with jumping boots! (The company is not responsible for any broken knees.)",
                    "JMP",
                    "Jump Height",
                    1,
                    int.Parse(SkillConfig.hostConfig["Jump Height Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Jump Height Multiplier"], CultureInfo.InvariantCulture),
                    JumpHeight.JumpHeightUpdate);
            }

            if (!LethalPlugin.MikesTweaks && bool.Parse(SkillConfig.hostConfig["Sprint Speed Enabled"]))
            {
                CreateSkill(UpgradeType.SprintSpeed,
                    "Sprint Speed",
                    "The company empowers you with pure steroids, run, spaceman.",
                    "SPD",
                    "Sprint Speed",
                    1,
                    int.Parse(SkillConfig.hostConfig["Sprint Speed Max Level"]),
                    float.Parse(SkillConfig.hostConfig["Sprint Speed Multiplier"], CultureInfo.InvariantCulture),
                    SprintSpeed.SprintSpeedUpdate);
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
        public Skill(string name, string description, string shortname, string attribute,
            UpgradeType upgradeType, int cost, int maxLevel, float multiplier,
            Action<int, int> callback = null, bool teamShared = false)
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

        public void SetLevel(int newLevel)
        {
            _level = newLevel;
            // level is number of changes
            int changes = newLevel - _level;
            _callback?.Invoke(changes, newLevel);
        }

        public void AddLevel(int change)
        {
            _level += change;
            int newLevel = _level;

            _callback?.Invoke(change, newLevel);
        }
    }
}
