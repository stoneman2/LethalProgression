using HarmonyLib;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LethalProgression.Patches
{
    [HarmonyPatch]
    internal class QuickMenuManagerPatch
    {
        private static GameObject _xpBar;
        private static GameObject _xpBarProgress;
        private static TextMeshProUGUI _xpText;
        private static TextMeshProUGUI _xpLevel;
        private static TextMeshProUGUI _profit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "OpenQuickMenu")]
        private static void QuickMenuXPBar(QuickMenuManager __instance)
        {
            // Check if menucontainer is active
            if (!__instance.isMenuOpen)
                return;

            if (!_xpBar || !_xpBarProgress)
                MakeNewXPBar();

            _xpBar.SetActive(true);
            _xpBarProgress.SetActive(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "Update")]
        private static void XPMenuUpdate(QuickMenuManager __instance)
        {
            if (!_xpBar || !_xpBarProgress)
                return;

            // If the settings menu or exit game menu is open, we don't want to show the XP bar.
            if (__instance.mainButtonsPanel.activeSelf)
            {
                _xpBar.SetActive(true);
                _xpBarProgress.SetActive(true);
            }
            else
            {
                _xpBar.SetActive(false);
                _xpBarProgress.SetActive(false);
            }

            // Set actual XP:
            // XP Text. Values of how much XP you need to level up.
            // XP Level, which is just the level you're on.
            // Profit, which is how much money you've made.
            _xpText.text = LP_NetworkManager.xpInstance.GetXP().ToString() + " / " + LP_NetworkManager.xpInstance.xpReq.Value.ToString();
            _xpLevel.text = "Level: " + LP_NetworkManager.xpInstance.GetLevel().ToString();
            _profit.text = "You've made.. " + LP_NetworkManager.xpInstance.GetProfit().ToString() + "$";
            // Set the bar fill
            _xpBarProgress.GetComponent<Image>().fillAmount = LP_NetworkManager.xpInstance.GetXP() / (float)LP_NetworkManager.xpInstance.xpReq.Value;
        }

        public static void MakeNewXPBar()
        {
            GameObject _pauseMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu");
            if (!_xpBar)
            {
                ////// XP Bar //////
                GameObject _gameXPBar = GameObject.Find("/Systems/UI/Canvas/EndgameStats/LevelUp/LevelUpBox");
                _xpBar = GameObject.Instantiate(_gameXPBar);
                _xpBar.name = "XPBar";
                _xpBar.transform.SetParent(_pauseMenu.transform, false);

                _xpBar.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                _xpBar.transform.Translate(-2f, 1f, 0f);
            }

            if (!_xpBarProgress)
            {
                ////// XP Progress //////
                GameObject _gameXPBarProgress = GameObject.Find("/Systems/UI/Canvas/EndgameStats/LevelUp/LevelUpMeter");
                _xpBarProgress = GameObject.Instantiate(_gameXPBarProgress);
                _xpBarProgress.name = "XPBarProgress";

                _xpBarProgress.transform.SetParent(_xpBar.transform, false);
                _xpBarProgress.GetComponent<Image>().fillAmount = 0f;
                _xpBarProgress.transform.localScale = new Vector3(0.597f, 5.21f, 1f);
                _xpBarProgress.transform.Translate(-0.8f, 0.2f, 0f);
                Vector3 pos = _xpBarProgress.transform.localPosition;

                _xpBarProgress.transform.localPosition = new Vector3(pos.x + 7, pos.y - 3.5f, 0f);

                ////// XP Text //////
                GameObject _gameXPText = GameObject.Find("/Systems/UI/Canvas/EndgameStats/LevelUp/Total");
                _xpText = GameObject.Instantiate(_gameXPText).GetComponent<TextMeshProUGUI>();
                _xpText.name = "XPText";
                _xpText.alignment = TextAlignmentOptions.Center;
                _xpText.SetText("0/1000");
                _xpText.transform.SetParent(_xpBar.transform, false);

                _xpText.color = new Color(1f, 0.6f, 0f, 1f);
                _xpText.transform.Translate(-0.75f, 0.21f, 0f);

                ////// Level Text /////
                _xpLevel = GameObject.Instantiate(_gameXPText).GetComponent<TextMeshProUGUI>();
                _xpLevel.name = "XPLevel";
                _xpLevel.alignment = TextAlignmentOptions.Center;
                _xpLevel.SetText("Level: 0");
                _xpLevel.transform.SetParent(_xpBar.transform, false);
                _xpLevel.color = new Color(1f, 0.6f, 0f, 1f);

                _xpLevel.transform.Translate(-1f, 0.4f, 0f);

                ///// PROFIT! /////
                _profit = GameObject.Instantiate(_gameXPText).GetComponent<TextMeshProUGUI>();
                _profit.name = "XPProfit";
                _profit.alignment = TextAlignmentOptions.Center;
                _profit.SetText("You've made.. 0$.");
                _profit.transform.SetParent(_xpBar.transform, false);
                _profit.color = new Color(1f, 0.6f, 0f, 1f);

                _profit.transform.Translate(-0.8f, 0f, 0f);
            }
        }
        private static GameObject skillTreeButton;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "OpenQuickMenu")]
        private static void SkillTreeAwake(QuickMenuManager __instance)
        {
            // Check if menucontainer is active
            if (!__instance.isMenuOpen)
                return;

            if (!skillTreeButton)
                MakeSkillTreeButton();
        }

        private static void MakeSkillTreeButton()
        {
            GameObject ResumeButton = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons/Resume");
            skillTreeButton = GameObject.Instantiate(ResumeButton);

            GameObject MainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
            skillTreeButton.transform.SetParent(MainButtons.transform, false);

            skillTreeButton.name = "Skills";
            skillTreeButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Skills";

            skillTreeButton.transform.Translate(0.7f, 1.1f, 0f);

            // Change the onClick event to our own.
            skillTreeButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            skillTreeButton.GetComponent<Button>().onClick.AddListener(OpenSkillTree);
        }

        private static void OpenSkillTree()
        {
            LP_NetworkManager.xpInstance.guiObj.OpenSkillMenu();
        }
    }
}
