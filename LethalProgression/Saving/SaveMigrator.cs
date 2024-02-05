using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace LethalProgression.Saving
{
    internal static class SaveMigrator
    {
        public static int saveFileSlot = 0;
        public static void MigrateSaves()
        {
            foreach (string saveFolder in Directory.EnumerateDirectories(GetSavesFolderPath()))
            {
                if (saveFolder.StartsWith("save"))
                {

                }
            }
        }
        public static void DeleteSave(int _saveFileSlot)
        {
            saveFileSlot = _saveFileSlot;
            // Delete entire folder
            if (Directory.Exists(GetSavePath()))
            {
                Directory.Delete($"{Application.persistentDataPath}/lethalprogression/save{saveFileSlot + 1}", true);
            }
        }
        public static string GetSavesFolderPath()
        {
            return $"{Application.persistentDataPath}/lethalprogression";
        }
        public static string GetSavePath()
        {
            return $"{Application.persistentDataPath}/lethalprogression/save{saveFileSlot + 1}/";
        }

        public static string Load(ulong steamId)
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            if (!File.Exists($"{GetSavePath()}{steamId}.json"))
            {
                return null;
            }

            string json = File.ReadAllText($"{GetSavePath()}{steamId}.json");

            return json;
        }

        public static SaveSharedData LoadShared()
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            if (!File.Exists($"{GetSavePath()}shared.json"))
            {
                LethalPlugin.Log.LogInfo("Shared file doesn't exist");
                return null;
            }

            string json = File.ReadAllText($"{GetSavePath()}shared.json");
            LethalPlugin.Log.LogInfo("Shared file exists");
            return JsonConvert.DeserializeObject<SaveSharedData>(json);
        }
    }
}
