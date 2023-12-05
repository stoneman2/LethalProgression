using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
    }

    internal class SkillList
    {
        public Dictionary<UpgradeType, Skill> skills = new Dictionary<UpgradeType, Skill>();

        public void CreateSkill(UpgradeType upgrade, string name, string description, string shortname, string attribute, UpgradeType upgradeType, int cost, int maxLevel, float multiplier, Action<int, int> callback = null, bool teamShared = false)
        {
            Skill newSkill = new Skill(name, description, shortname, attribute, upgradeType, cost, maxLevel, multiplier, callback, teamShared);
            skills.Add(upgrade, newSkill);
        }

        public void InitializeSkills()
        {
            if (SkillConfig.configHealthRegenEnabled.Value)
            {
                CreateSkill(UpgradeType.HPRegen,
                    "Health Regen",
                    "Your body heals itself faster, allowing you to recover from injuries quicker. Only regenerate up to 100 HP.",
                    "HPR",
                    "Health Regeneration",
                    UpgradeType.HPRegen,
                    1,
                    SkillConfig.configHealthRegenMaxLevel.Value,
                    SkillConfig.configHealthRegenMultiplier.Value);
            }

            if (SkillConfig.configStaminaEnabled.Value)
            {
                CreateSkill(UpgradeType.Stamina,
                    "Stamina",
                    "The company gives you a better pair of lungs, allowing you to run for longer.",
                    "STM",
                    "Stamina",
                    UpgradeType.Stamina,
                    1,
                    SkillConfig.configStaminaMaxLevel.Value,
                    SkillConfig.configStaminaMultiplier.Value,
                    Stamina.StaminaUpdate);
            }

            if (SkillConfig.configBatteryLifeEnabled.Value)
            {
                CreateSkill(UpgradeType.Battery,
                    "Battery Life",
                    "You brought better batteries. Replace your batteries AT THE SHIP'S CHARGER to see an effect.",
                    "BAT",
                    "Battery Life",
                    UpgradeType.Battery,
                    1,
                    SkillConfig.configBatteryLifeMaxLevel.Value,
                    SkillConfig.configBatteryLifeMultiplier.Value);
            }

            if (SkillConfig.configHandSlotsEnabled.Value && !LethalPlugin.ReservedSlots)
            {
                CreateSkill(UpgradeType.HandSlot,
                     "Hand Slot",
                     "The company finally gives you a better belt! Fit more stuff! (Reach 100% for one more slot! 10 per slot.)",
                     "HND",
                     "Hand Slots",
                     UpgradeType.HandSlot,
                     1,
                     SkillConfig.configHandSlotsMaxLevel.Value,
                     SkillConfig.configHandSlotsMultiplier.Value,
                     HandSlots.HandSlotsUpdate);
            }

            if (SkillConfig.configLootValueEnabled.Value)
            {
                CreateSkill(UpgradeType.Value,
                    "Scrap Value",
                    "The company favors you, giving you better deals when bartering.",
                    "VAL",
                    "Team Loot Value",
                    UpgradeType.Value,
                    1,
                    SkillConfig.configLootValueMaxLevel.Value,
                    SkillConfig.configLootValueMultiplier.Value,
                    XPHandler.xpInstance.TeamLootValueUpdate);
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
