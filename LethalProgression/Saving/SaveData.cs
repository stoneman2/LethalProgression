using LethalProgression.Skills;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalProgression.Saving
{
    internal class SaveData
    {
        public ulong steamId;
        public int skillPoints { get; set; }
        public Dictionary<UpgradeType, int> skillAllocation = new Dictionary<UpgradeType, int>();
    }

    internal class SaveSharedData
    {
        public int xp { get; set; }
        public int level { get; set; }
        public int quota { get; set; }

        public SaveSharedData(int xp, int level, int quota)
        {
            this.xp = xp;
            this.level = level;
            this.quota = quota;
        }
    }
}
