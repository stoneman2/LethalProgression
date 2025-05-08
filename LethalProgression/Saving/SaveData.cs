using System.Collections.Generic;
using LethalProgression.Skills;

namespace LethalProgression.Saving
{
    internal class SaveData
    {
        public ulong steamId;
        public int skillPoints { get; set; }
        public Dictionary<UpgradeType, int> skillAllocation = new Dictionary<UpgradeType, int>();
    }
}
