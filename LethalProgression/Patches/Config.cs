using System.Collections.Generic;

namespace LethalProgression.Config
{
    internal class SkillConfig
    {
        public static IDictionary<string, string> hostConfig = new Dictionary<string, string>();
        public static void InitConfig()
        {
            LethalPlugin.Instance.BindConfig<int>(
                "General",
                "Person Multiplier",
                35,
                "How much does XP cost to level up go up per person?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "General",
                "Quota Multiplier",
                30,
                "How much more XP does it cost to level up go up per quota? (Percent)"
                );
            LethalPlugin.Instance.BindConfig<int>(
                "General",
                "XP Minimum",
                40,
                "Minimum XP to level up."
                );
            LethalPlugin.Instance.BindConfig<int>(
                "General",
                "XP Maximum",
                750,
                "Maximum XP to level up."
                );
            LethalPlugin.Instance.BindConfig<bool>(
                "General",
                "Unspec in Ship Only",
                false,
                "Disallows unspecing stats if you're not currently on the ship."
                );
            LethalPlugin.Instance.BindConfig<bool>(
                "General",
                "Unspec in Orbit Only",
                true,
                "Disallows unspecing stats if you're not currently in orbit."
                );
            LethalPlugin.Instance.BindConfig<bool>(
                "General",
                "Disable Unspec",
                false,
                "Disallows unspecing altogether."
                );

            // Skill Configs
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Health Regen Enabled",
                true,
                "Enable the Health Regen skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Health Regen Max Level",
                20,
                "Maximum level for the health regen."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Health Regen Multiplier",
                0.05f,
                "How much does the health regen skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Stamina Enabled",
                true,
                "Enable the Stamina skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Stamina Max Level",
                99999,
                "Maximum level for the stamina."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Stamina Multiplier",
                2,
                "How much does the stamina skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Battery Life Enabled",
                true,
                "Enable the Battery Life skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Battery Life Max Level",
                99999,
                "Maximum level for the battery life."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Battery Life Multiplier",
                5,
                "How much does the battery life skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Hand Slots Enabled",
                true,
                "Enable the Hand Slots skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Hand Slots Max Level",
                30,
                "Maximum level for the hand slots."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Hand Slots Multiplier",
                10,
                "How much does the hand slots skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Loot Value Enabled",
                true,
                "Enable the Loot Value skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Loot Value Max Level",
                250,
                "Maximum level for the loot value."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Loot Value Multiplier",
                0.1f,
                "How much does the loot value skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Oxygen Enabled",
                true,
                "Enable the Oxygen skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Oxygen Max Level",
                99999,
                "Maximum level for Oxygen."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Oxygen Multiplier",
                1f,
                "How much does the Oxygen skill increase per level?"
                );
            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Jump Height Enabled",
                true,
                "Enable the Jump Height skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Jump Height Max Level",
                99999,
                "Maximum level for Jump Height."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Jump Height Multiplier",
                3f,
                "How much does the Jump Height skill increase per level?"
                );
            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Sprint Speed Enabled",
                true,
                "Enable the Sprint Speed skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Sprint Speed Max Level",
                99999,
                "Maximum level for Sprint Speed."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Sprint Speed Multiplier",
                0.25f,
                "How much does the Sprint Speed skill increase per level?"
                );
            //
            LethalPlugin.Instance.BindConfig<bool>(
                "Skills",
                "Strength Enabled",
                true,
                "Enable the strength skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(
                "Skills",
                "Strength Max Level",
                75,
                "Maximum level for Strength."
                );

            LethalPlugin.Instance.BindConfig<float>(
                "Skills",
                "Strength Multiplier",
                1f,
                "How much does the Strength skill increase per level?"
                );
        }
    }
}