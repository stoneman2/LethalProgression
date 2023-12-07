using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx.Configuration;

namespace LethalProgression.Config
{
    internal class SkillConfig
    {
        internal static ConfigEntry<int> configPersonScale;
        internal static ConfigEntry<int> configQuotaMult;
        internal static ConfigEntry<int> configXPMin;
        internal static ConfigEntry<int> configXPMax;

        // Skill Configs
        internal static ConfigEntry<bool> configHealthRegenEnabled;
        internal static ConfigEntry<int> configHealthRegenMaxLevel;
        internal static ConfigEntry<float> configHealthRegenMultiplier;

        internal static ConfigEntry<bool> configStaminaEnabled;
        internal static ConfigEntry<int> configStaminaMaxLevel;
        internal static ConfigEntry<float> configStaminaMultiplier;

        internal static ConfigEntry<bool> configHandSlotsEnabled;
        internal static ConfigEntry<int> configHandSlotsMaxLevel;
        internal static ConfigEntry<float> configHandSlotsMultiplier;

        internal static ConfigEntry<bool> configBatteryLifeEnabled;
        internal static ConfigEntry<int> configBatteryLifeMaxLevel;
        internal static ConfigEntry<float> configBatteryLifeMultiplier;

        internal static ConfigEntry<bool> configLootValueEnabled;
        internal static ConfigEntry<int> configLootValueMaxLevel;
        internal static ConfigEntry<float> configLootValueMultiplier;

        public static void InitConfig()
        {
            LethalPlugin.Instance.BindConfig<int>(ref configPersonScale,
                "General",
                "Person Multiplier",
                35,
                "How much does XP cost to level up go up per person?"
                );

            LethalPlugin.Instance.BindConfig<int>(ref configQuotaMult,
                "General",
                "Quota Multiplier",
                30,
                "How much more XP does it cost to level up go up per quota? (Percent)"
                );
            LethalPlugin.Instance.BindConfig<int>(ref configXPMin,
                "General",
                "XP Minimum",
                40,
                "Minimum XP to level up."
                );
            LethalPlugin.Instance.BindConfig<int>(ref configXPMax,
                "General",
                "XP Maximum",
                750,
                "Maximum XP to level up."
                );

            // Skill Configs
            LethalPlugin.Instance.BindConfig<bool>(ref configHealthRegenEnabled,
                "Skills",
                "Health Regen Enabled",
                true,
                "Enable the Health Regen skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(ref configHealthRegenMaxLevel,
                "Skills",
                "Health Regen Max Level",
                20,
                "Maximum level for the health regen."
                );

            LethalPlugin.Instance.BindConfig<float>(ref configHealthRegenMultiplier,
                "Skills",
                "Health Regen Multiplier",
                0.05f,
                "How much does the health regen skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(ref configStaminaEnabled,
                "Skills",
                "Stamina Enabled",
                true,
                "Enable the Stamina skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(ref configStaminaMaxLevel,
                "Skills",
                "Stamina Max Level",
                99999,
                "Maximum level for the stamina."
                );

            LethalPlugin.Instance.BindConfig<float>(ref configStaminaMultiplier,
                "Skills",
                "Stamina Multiplier",
                2,
                "How much does the stamina skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(ref configBatteryLifeEnabled,
                "Skills",
                "Battery Life Enabled",
                true,
                "Enable the Battery Life skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(ref configBatteryLifeMaxLevel,
                "Skills",
                "Battery Life Max Level",
                99999,
                "Maximum level for the battery life."
                );

            LethalPlugin.Instance.BindConfig<float>(ref configBatteryLifeMultiplier,
                "Skills",
                "Battery Life Multiplier",
                5,
                "How much does the battery life skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(ref configHandSlotsEnabled,
                "Skills",
                "Hand Slots Enabled",
                true,
                "Enable the Hand Slots skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(ref configHandSlotsMaxLevel,
                "Skills",
                "Hand Slots Max Level",
                30,
                "Maximum level for the hand slots."
                );

            LethalPlugin.Instance.BindConfig<float>(ref configHandSlotsMultiplier,
                "Skills",
                "Hand Slots Multiplier",
                10,
                "How much does the hand slots skill increase per level?"
                );

            //
            LethalPlugin.Instance.BindConfig<bool>(ref configLootValueEnabled,
                "Skills",
                "Loot Value Enabled",
                true,
                "Enable the Loot Value skill?"
                );

            LethalPlugin.Instance.BindConfig<int>(ref configLootValueMaxLevel,
                "Skills",
                "Loot Value Max Level",
                99999,
                "Maximum level for the loot value."
                );

            LethalPlugin.Instance.BindConfig<float>(ref configLootValueMultiplier,
                "Skills",
                "Loot Value Multiplier",
                1f,
                "How much does the loot value skill increase per level?"
                );
        }
    }
}