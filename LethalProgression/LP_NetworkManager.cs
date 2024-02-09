using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalProgression
{
    [HarmonyPatch]
    internal class LP_NetworkManager
    {
        public static LC_XP xpInstance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        public static void Init()
        {
            if (xpNetworkObject != null)
            {
                return;
            }

            xpNetworkObject = (GameObject)LethalPlugin.skillBundle.LoadAsset("LP_XPHandler");
            xpNetworkObject.AddComponent<LC_XP>();
            NetworkManager.Singleton.AddNetworkPrefab(xpNetworkObject);
        }

        public static GameObject xpNetworkObject;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        static void SpawnNetworkHandler()
        {
            if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
            {
                return;
            }
            var networkHandlerHost = Object.Instantiate(xpNetworkObject, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            xpInstance = networkHandlerHost.GetComponent<LC_XP>();
            LethalPlugin.Log.LogInfo("XPHandler Initialized.");
        }
    }
}
