using BepInEx;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using System.Reflection;
using System.IO;
using UnityEngine.SceneManagement;

namespace LethalProgression
{
    [BepInPlugin("Stoneman.LethalProgression", "Lethal Progression", "1.0.1")]
    internal class LethalProgress : BaseUnityPlugin
    {
        private const string modGUID = "Stoneman.LethalProgression";
        private const string modName = "Lethal Progression";
        private const string modVersion = "1.0.1";
        private const string modAuthor = "Stoneman";

        // Make a public AssetBundle
        public static AssetBundle skillBundle;

        internal static ManualLogSource Log;

        internal static ConfigEntry<int> configPersonScale;
        internal static ConfigEntry<int> configQuotaMult;
        internal static ConfigEntry<int> configXPMin;
        internal static ConfigEntry<int> configXPMax;

        private void Awake()
        {
            var harmony = new Harmony(modGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            skillBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "skillmenu"));

            Log = Logger;

            Log.LogInfo("Lethal Progression loaded.");

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

            SceneManager.sceneLoaded += LethalProgression.XPHandler.ClientConnectInitializer;

            // Config binders
            configPersonScale = Config.Bind("General", "Person Multiplier", 35, "How much does XP cost to level up go up per person?");

            configQuotaMult = Config.Bind("General", "Quota Multiplier", 30, "How much more XP does it cost to level up go up per quota? (Percent)");

            configXPMin = Config.Bind("General", "XP Minimum", 40, "Minimum XP to level up.");

            configXPMax = Config.Bind("General", "XP Maximum", 750, "Maximum XP to level up.");
        }
    }
}
