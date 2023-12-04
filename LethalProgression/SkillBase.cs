using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalProgression.Skills
{
    public enum UpgradeType
    {
        Health,
        Stamina,
        Battery,
        HandSlot,
        Value,
    }

    internal class SkillList
    {
        public Dictionary<string ,Skill> skills = new Dictionary<string, Skill>()
        {
            { "HP Regen", new Skill("Health Regen", "Your body heals itself faster, allowing you to recover from injuries quicker. Only regenerate up to 100 HP.", "HPR", "Health Regeneration",
                UpgradeType.Health, 1, 20, 0.05f) },
            { "Stamina", new Skill("Extra Stamina", "Work on your stamina, sprint just that much longer!", "STM", "Stamina",
                UpgradeType.Stamina, 1, 99999, 2f, LethalProgression.Skills.Stamina.StaminaUpdate)},
            { "Battery Life", new Skill("Battery Life",
                "You brought better batteries. Replace your batteries AT THE SHIP'S CHARGER to see an effect.", "BAT", "Battery Life",
                UpgradeType.Battery, 1, 99999, 5f) },
            { "Hand Slot", new Skill("Hand Slot", "The company finally gives you a better belt! Fit more stuff! (Reach 100% for one more slot! 20 per slot.)", "HND", "Hand Slots",
                UpgradeType.HandSlot, 1, 30, 10f, LethalProgression.Skills.HandSlots.HandSlotsUpdate) },
            { "Scrap Value", new Skill("Team Scrap Value", "The company favors you, giving you better deals when bartering.", "VAL", "Team Loot Value",
                UpgradeType.Value, 1, 99999, 1f, LethalProgression.XPHandler.xpInstance.TeamLootValueUpdate, true) },
        };
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
            int oldLevel = _level;
            _level += level;
            int newLevel = _level;

            _callback?.Invoke(oldLevel, newLevel);
        }
    }
}
