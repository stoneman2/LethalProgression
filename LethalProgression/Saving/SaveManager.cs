using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Steamworks;

namespace LethalProgression.Saving
{
    internal static class SaveManager
    {
        public static int saveFileSlot = 0;
        public static void Save(ulong steamid, string data)
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            LethalPlugin.Log.LogInfo("Saving to slot " + saveFileSlot + 1);

            // If file doesn't exist, create it
            if (!Directory.Exists(GetSavePath()))
            {
                Directory.CreateDirectory(GetSavePath());
            }

            File.WriteAllText(GetSavePath() + steamid + ".json", data);
        }

        public static void SaveShared(int xp, int level, int quota)
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            LethalPlugin.Log.LogInfo("Saving to slot " + saveFileSlot + 1);

            // If file doesn't exist, create it
            if (!Directory.Exists(GetSavePath()))
            {
                Directory.CreateDirectory(GetSavePath());
            }

            File.WriteAllText(GetSavePath() + "shared.json", JsonConvert.SerializeObject(new SaveSharedData(xp, level, quota)));
        }

        public static void DeleteSave(int _saveFileSlot)
        {
            saveFileSlot = _saveFileSlot;
            // Delete entire folder
            if (Directory.Exists(GetSavePath()))
            {
                Directory.Delete(Application.persistentDataPath + "/lethalprogression/save" + (saveFileSlot + 1), true);
            }
        }

        public static string GetSavePath()
        {
            return Application.persistentDataPath + "/lethalprogression/save" + (saveFileSlot + 1) + "/";
        }

        public static string Load(ulong steamId)
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            if (!File.Exists(GetSavePath() + steamId + ".json"))
            {
                return null;
            }

            string json = File.ReadAllText(GetSavePath() + steamId + ".json");

            return json;
        }

        public static SaveSharedData LoadShared()
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            if (!File.Exists(GetSavePath() + "shared.json"))
            {
                LethalPlugin.Log.LogInfo("Shared file doesn't exist");
                return null;
            }

            string json = File.ReadAllText(GetSavePath() + "shared.json");
            LethalPlugin.Log.LogInfo("Shared file exists");
            return JsonConvert.DeserializeObject<SaveSharedData>(json);
        }
    }
}
