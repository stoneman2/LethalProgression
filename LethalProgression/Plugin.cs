using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalProgression.Config;
using UnityEngine;

namespace LethalProgression
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class LethalPlugin : BaseUnityPlugin
    {
        private const string modGUID = "Stoneman.LethalProgression";
        private const string modName = "Lethal Progression";
        private const string modVersion = "1.4.1";
        private const string modAuthor = "Stoneman";
        public static AssetBundle skillBundle;

        internal static ManualLogSource Log;
        internal static bool ReservedSlots;
        internal static bool MikesTweaks;
        internal static bool LethalConfig;
        public static LethalPlugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            Harmony harmony = new Harmony(modGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            skillBundle = AssetBundle.LoadFromMemory(LethalProgression.Properties.Resources.skillmenu);

            Log = Logger;

            Log.LogInfo("Lethal Progression loaded.");

            foreach (var plugin in Chainloader.PluginInfos)
            {
                if (plugin.Value.Metadata.GUID.IndexOf("ReservedItem") >= 0)
                {
                    ReservedSlots = true;
                }

                if (plugin.Value.Metadata.GUID.IndexOf("mikestweaks") >= 0)
                {
                    // Get "ExtraItemSlots" config entry from Mike's Tweaks
                    ConfigEntryBase[] mikesEntries = plugin.Value.Instance.Config.GetConfigEntries();

                    MikesTweaks = true;

                    foreach (var entry in mikesEntries)
                    {
                        if (entry.Definition.Key == "ExtraItemSlots")
                        {
                            if (int.Parse(entry.GetSerializedValue()) > 0)
                            {
                                ReservedSlots = true;
                            }
                        }
                    }
                }
                if (plugin.Value.Metadata.GUID.IndexOf("lethalconfig") >= 0)
                {
                    LethalConfig = true;
                }
            }


            // Network patcher!
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            SkillConfig.InitConfig();
        }

        public ConfigEntry<T> BindConfig<T>(string section, string key, T defaultValue, string description = "")
        {
            return Config.Bind(section, key, defaultValue, description);
        }

        public IDictionary<string, string> GetAllConfigEntries()
        {
            IDictionary<string, string> localConfig = Config.GetConfigEntries().ToDictionary(
                entry => entry.Definition.Key,
                entry => entry.GetSerializedValue()
            );

            return localConfig;
        }
    }
}
