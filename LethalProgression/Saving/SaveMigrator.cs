using System.IO;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace LethalProgression.Saving
{
    ///BU page 92 
    ///3.) let the american bleed to death
    ///4.) leave the house
    ///A.I page 93
    ///1.) it would easier for him to relate to him as an enemey.
    ///2.)his pride and ego made him want to save his life
    ///3.) on the one hand she knows the servants are right and she should turn him over to the police.
    ///on the other hnad she feels an impulse to protect him
    ///4.)he may have been experiencing a similar conflict as hana
    ///5.) like hana he didnt want to think the man had been tortured he prefers to believe the newspaper  reports 
    ///    than tothink the army would act 
    ///6.) shes scared. hes in a army uniform.
    ///7.)the messenger has just come to say the general is sick and needs sadao
    ///8.) he wants to spare hana any more turmoi
    ///
    ///vocab
    ///ruthless-cruel (adj)
    ///execute/ execution
    ///to execute = to carry out an order
    ///an execution = death sentence
    ///timid  = shy
    ///gaunt = very thin from hunger or age
    ///bough = branch
    ///fortify = strengthen
    ///pawnshop = a place you can trade things for money
    internal static class SaveMigrator
    {
        public static int saveFileSlot = 0;
        private static readonly int SteamIdLength = $"{(ulong)SteamClient.SteamId}".Length;
        private static void Delete(string Path, bool IsFile = true)
        {
            if (!Config.SkillConfig.DeleteJsonSaves)
            {
                return;
            }
            if (IsFile)
            {
                File.Delete(Path);
                return;
            }
            Directory.Delete(Path);
        }
        public static void MigrateSaves()
        {
            string folder = GetSavesFolderPath();
            foreach (string saveFolder in Directory.EnumerateDirectories(folder))
            {
                if (!int.TryParse(saveFolder.ToString().Replace("save", string.Empty), out int saveSlot))
                {
                    continue;
                }

                string folderPath = $"{folder}/{saveFolder}";
                if (!HasSaveFile(folderPath, saveFolder, saveSlot))//brain.exe has stopped working helpme(kil me)
                {
                    LethalPlugin.Log.LogWarning($"{saveFolder} is missing the SaveDataFile");
                }

                string sharedFilePath = $"{folderPath}/shared.json";
                SaveSharedData sharedData = LoadShared(sharedFilePath);
                switch (sharedData)
                {
                    case null:
                        LethalPlugin.Log.LogWarning($"{saveFolder} is missing the SharedDataSave file");
                        break;
                    default:
                        SaveManager.SaveShared(sharedData.xp, sharedData.level, sharedData.quota, saveSlot);
                        break;
                }
                Delete(sharedFilePath);
                Delete(saveFolder, false);
            }
            Delete(folder, false);
        }
        public static bool HasSaveFile(string folderPath, string saveFolder, int saveSlot)
        {
            foreach (string fileName in Directory.EnumerateFiles(saveFolder))
            {
                string filePath = $"{folderPath}/{fileName}";
                if (fileName.Length != SteamIdLength || !ulong.TryParse(fileName, out ulong steamid))
                {
                    continue;
                }
                string json = Load(steamid, filePath);

                SaveManager.Save(steamid, json, saveSlot);
                Delete(fileName);
                return true;
            }
            return false;
        }
        public static string GetSavesFolderPath()
        {
            return $"{Application.persistentDataPath}/lethalprogression";
        }
        public static string GetSavePath()
        {
            return $"{GetSavesFolderPath()}/save{saveFileSlot + 1}/";
        }

        public static string Load(ulong? steamId, string json = null)
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;
            if (json == null)
            {
                json = $"{GetSavePath()}{steamId.Value}.json";
            }
            if (!File.Exists(json))
            {
                return null;
            }

            json = File.ReadAllText(json);
            return json;
        }

        public static SaveSharedData LoadShared(string json = null)
        {
            saveFileSlot = GameNetworkManager.Instance.saveFileNum;

            if (json == null)
            {
                json = $"{GetSavePath()}shared.json";
            }
            if (!File.Exists(json))
            {
                LethalPlugin.Log.LogInfo("Shared file doesn't exist");
                return null;
            }

            json = File.ReadAllText(json);
            LethalPlugin.Log.LogInfo("Shared file exists");
            return JsonConvert.DeserializeObject<SaveSharedData>(json);
        }
    }
}
