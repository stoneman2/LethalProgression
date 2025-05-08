namespace LethalProgression.Saving
{
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
