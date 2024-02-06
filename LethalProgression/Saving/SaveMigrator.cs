using System.IO;
using Newtonsoft.Json;
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
        public static void MigrateSaves()
        {
            string folder = GetSavesFolderPath();
            foreach (string saveFolder in Directory.EnumerateDirectories(folder))
            {

                if (!saveFolder.StartsWith("save"))
                {
                    continue;
                }

                foreach (string filePath in Directory.EnumerateFiles(saveFolder))
                {
                    string json;
                    if (filePath.EndsWith("shared.json"))
                    {
                        json = File.ReadAllText(filePath);
                        SaveSharedData sharedData = JsonConvert.DeserializeObject<SaveSharedData>(json);
                        SaveManager.SaveShared(sharedData.xp, sharedData.level, sharedData.quota);
                        File.Delete(filePath);
                    }
                    else
                    {
                        json = File.ReadAllText(filePath);
                        var dir = filePath.Split(('/'));
                        SaveManager.Save(ulong.Parse(dir[dir.Length - 1]), File.ReadAllText(filePath));
                        File.Delete(filePath);
                    }
                }
                Directory.Delete(saveFolder);
            }
            Directory.Delete(folder);
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
