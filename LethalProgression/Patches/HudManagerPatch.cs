using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalProgression.Patches
{
    [HarmonyPatch]
    internal class HUDManagerPatch
    {
        private static GameObject _tempBar;
        private static TextMeshProUGUI _tempText;
        private static float _tempBarTime;

        private static GameObject levelText;
        private static float levelTextTime;

        private static Dictionary<string, int> _enemyReward = new Dictionary<string, int>()
        {
            { "HoarderBug (EnemyType)" , 30 },
            { "BaboonBird (EnemyType)", 15},
            { "MouthDog (EnemyType)", 200},
            { "Centipede (EnemyType)", 30 },
            { "Flowerman (EnemyType)", 200 },
            { "SandSpider (EnemyType)", 50 },
            { "Crawler (EnemyType)", 50 },
            { "Puffer (EnemyType)", 15 },
        };

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        //private static void DebugScan()
        //{
        //    LethalProgression.XPHandler.xpInstance.AddXPServerRPC(10);
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "AddNewScrapFoundToDisplay")]
        private static void GiveXPForScrap(GrabbableObject GObject)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            // Now we got the loot list that's about to be displayed, we add XP for each one that gets shown.
            int scrapCost = GObject.scrapValue;

            // Give XP for the amount of money this scrap costs.
            LP_NetworkManager.xpInstance.AddXPServerRPC(scrapCost);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "KillEnemy")]
        private static void GiveXPForKill(EnemyAI __instance)
        {
            string enemyType = __instance.enemyType.ToString();
            LethalPlugin.Log.LogInfo("Enemy type: " + enemyType);
            // Give XP for the amount of money this scrap costs.
            int enemyReward = 30;
            if (_enemyReward.ContainsKey(enemyType))
            {
                enemyReward = _enemyReward[enemyType];
            }
            LP_NetworkManager.xpInstance.AddXPServerRPC(enemyReward);
        }
        public static void ShowXPUpdate(int oldXP, int newXP, int xp)
        {
            // Makes one if it doesn't exist on screen yet.
            if (!_tempBar)
                MakeBar();

            GameObject _tempprogress = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/XPUpdate/XPBarProgress");

            _tempprogress.GetComponent<Image>().fillAmount = newXP / (float)LP_NetworkManager.xpInstance.GetXPRequirement();
            _tempText.text = newXP + " / " + (float)LP_NetworkManager.xpInstance.GetXPRequirement();

            _tempBarTime = 2f;

            if (!_tempBar.activeSelf)
            {
                GameNetworkManager.Instance.StartCoroutine(XPBarCoroutine());
            }
        }

        private static IEnumerator XPBarCoroutine()
        {
            _tempBar.SetActive(true);
            while (_tempBarTime > 0f)
            {
                float time = _tempBarTime;
                _tempBarTime = 0f;
                yield return new WaitForSeconds(time);
            }
            _tempBar.SetActive(false);
        }

        public static void ShowLevelUp()
        {
            if (!levelText)
                MakeLevelUp();

            levelTextTime = 5f;

            if (!levelText.gameObject.activeSelf)
                GameNetworkManager.Instance.StartCoroutine(LevelUpCoroutine());
        }

        public static void MakeLevelUp()
        {
            levelText = GameObject.Instantiate(LethalPlugin.skillBundle.LoadAsset<GameObject>("LevelUp"));

            levelText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Level Up! Spend your skill points.";

            // Make this not active
            levelText.gameObject.SetActive(false);
        }

        private static IEnumerator LevelUpCoroutine()
        {
            levelText.gameObject.SetActive(true);
            while (levelTextTime > 0f)
            {
                float time = levelTextTime;
                levelTextTime = 0f;
                yield return new WaitForSeconds(time);
            }
            levelText.gameObject.SetActive(false);
        }

        private static void MakeBar()
        {
            GameObject _xpBar = GameObject.Find("/Systems/UI/Canvas/QuickMenu/XPBar");
            QuickMenuManagerPatch.MakeNewXPBar();
            _xpBar = GameObject.Find("/Systems/UI/Canvas/QuickMenu/XPBar");
            _tempBar = GameObject.Instantiate(_xpBar);
            _tempBar.name = "XPUpdate";

            _tempText = _tempBar.GetComponentInChildren<TextMeshProUGUI>();

            GameObject _igHud = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle");
            _tempBar.transform.SetParent(_igHud.transform, false);
            _tempBar.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            GameObject _xpBarLevel = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/XPUpdate/XPLevel");
            GameObject.Destroy(_xpBarLevel);

            GameObject _xpBarProfit = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/XPUpdate/XPProfit");
            GameObject.Destroy(_xpBarProfit);

            _tempBar.transform.Translate(3.1f, -2.1f, 0f);
            Vector3 localPos = _tempBar.transform.localPosition;

            _tempBar.transform.localPosition = new Vector3(localPos.x, localPos.y + 5f, localPos.z);

            _tempBar.SetActive(false);
        }
    }
}
