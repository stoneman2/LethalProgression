using System;

namespace LethalProgression.Skills
{
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
        private readonly Action<int> _callback;
        public bool _teamShared;
        private int _level;
        public Skill(string name, string description, string shortname, string attribute,
            UpgradeType upgradeType, int cost, int maxLevel, float multiplier,
            Action<int> callback = null, bool teamShared = false)
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
            // level is number of changes
            int changes = newLevel - _level;
            _level = newLevel;
            _callback?.Invoke(changes);
        }

        public void AddLevel(int change)
        {
            _level += change;
            int newLevel = _level;

            _callback?.Invoke(change);
        }
    }
}
