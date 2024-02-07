using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace LethalProgression.Saving
{
    internal static class SaveManager
    {
        public static ulong steamid;

        public static int saveFileSlot;
        private static int GetSaveSlot()
        {
            return GameNetworkManager.Instance.saveFileNum + 1;
        }
        public static string GetSaveKey(ulong? steamid = null, int saveFileSlot = -1)
        {
            if (saveFileSlot == -1)
            {
                saveFileSlot = GetSaveSlot();
            }
            steamid = steamid.GetValueOrDefault(SteamClient.SteamId);
            return $"{steamid}/LCSaveFile{saveFileSlot}";
        }
        public static string GetSharedSaveKey(int saveFileSlot = -1)
        {
            if (saveFileSlot == -1)
            {
                saveFileSlot = GetSaveSlot();
            }
            return $"LCSaveFile{saveFileSlot}/SharedData";
        }

        public static void Save(ulong steamid, string data, int saveFileSlot = -1)
        {
            if (saveFileSlot == -1)
            {
                saveFileSlot = GetSaveSlot();
            }
            LethalPlugin.Log.LogInfo($"Saving to slot {saveFileSlot}");
            PlayerPrefs.SetString($"{GetSaveKey(steamid)}", data);
        }
        public static void SaveShared(int xp, int level, int quota, int saveFileSlot = -1)
        {
            if (saveFileSlot == -1)
            {
                saveFileSlot = GetSaveSlot();
            }
            LethalPlugin.Log.LogInfo($"Saving to slot {saveFileSlot}");
            PlayerPrefs.SetString(GetSharedSaveKey(), JsonConvert.SerializeObject(new SaveSharedData(xp, level, quota)));
        }

        public static void DeleteSave(int saveFileSlot)
        {
            string key = GetSaveKey(null, saveFileSlot);
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }


        public static string Load(ulong? steamid, SaveType type = SaveType.PlayerPrefs)
        {
            if (type == SaveType.Json)
            {
                return SaveMigrator.Load(steamid);
            }
            string json = PlayerPrefs.GetString(GetSaveKey(steamid.Value));
            return json;
        }

        public static SaveSharedData LoadShared()
        {
            string json = PlayerPrefs.GetString(GetSharedSaveKey(), null);
            if (json == null)
            {
                LethalPlugin.Log.LogInfo("Shared file doesn't exists");
                return null;
            }
            LethalPlugin.Log.LogInfo("Shared file exists");
            return JsonConvert.DeserializeObject<SaveSharedData>(json);
        }
    }
}
