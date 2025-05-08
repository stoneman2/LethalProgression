using HarmonyLib;
using LethalProgression.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalProgression.Patches
{
    [HarmonyPatch]
    internal class QuickMenuManagerPatch
    {
        private static GameObject skillTreeButton;
        private static GameObject _xpBar;
        private static GameObject _xpInfoContainer;
        private static GameObject _xpBarProgress;
        private static TextMeshProUGUI _xpText;
        private static TextMeshProUGUI _xpLevel;
        private static TextMeshProUGUI _profit;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
        private static void QuickMenuXPBar(QuickMenuManager __instance)
        {
            // Check if menucontainer is active
            if (!__instance.isMenuOpen)
            {
                return;
            }

            if (!_xpBar || !_xpBarProgress)
            {
                MakeNewXPBar();
            }

            _xpBar.SetActive(true);
            _xpBarProgress.SetActive(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        private static void XPMenuUpdate(QuickMenuManager __instance)
        {
            if (!_xpInfoContainer || !_xpBar || !_xpBarProgress)
            {
                return;
            }

            // If the settings menu or exit game menu is open, we don't want to show the XP bar.
            bool activeState = __instance.mainButtonsPanel.activeSelf;
            _xpInfoContainer.SetActive(activeState);


            // Set actual XP:
            // XP Text. Values of how much XP you need to level up.
            // XP Level, which is just the level you're on.
            // Profit, which is how much money you've made.
            _xpText.text = $"{LP_NetworkManager.xpInstance.GetXP()} / {LP_NetworkManager.xpInstance.xpReq.Value}";
            _xpLevel.text = $"Level: {LP_NetworkManager.xpInstance.GetLevel()}";
            _profit.text = $"You've made.. {LP_NetworkManager.xpInstance.GetProfit().ToString()}$";
            // Set the bar fill
            _xpBarProgress.GetComponent<Image>().fillAmount = LP_NetworkManager.xpInstance.GetXP() / (float)LP_NetworkManager.xpInstance.xpReq.Value;
        }

        public static void MakeNewXPBar()
        {
            GameObject _pauseMenu = GameObject.Find("/Systems/UI/Canvas/QuickMenu");
            GameObject _gameXPText = GameObject.Find("/Systems/UI/Canvas/EndgameStats/LevelUp/Total");
            //Container => [XpBar => BarProgression], [Profit,Level]
            if (!_xpInfoContainer)
            {
                _xpInfoContainer = new GameObject("XpInfoContainer");
                //setlocal pos to be the same as old one
                _xpInfoContainer.transform.SetParent(_pauseMenu.transform, false);
                _xpInfoContainer.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                _xpInfoContainer.transform.Translate(-1.7f, 0.9f, 0f);
            }
            if (!_xpBar)
            {
                ////// XP Bar //////
                GameObject _gameXPBar = GameObject.Find("/Systems/UI/Canvas/EndgameStats/LevelUp/LevelUpBox");
                _xpBar = GameObject.Instantiate(_gameXPBar);
                _xpBar.transform.SetParent(_xpInfoContainer.transform, false);
                _xpBar.name = "XPBar";

                ////// XP Text //////
                _xpText = GameObject.Instantiate(_gameXPText).GetComponent<TextMeshProUGUI>();
                _xpText.transform.SetParent(_xpBar.transform, false);
                _xpText.transform.Translate(-0.75f, 0.21f, 0f);
                _xpText.name = "XPText";
                _xpText.alignment = TextAlignmentOptions.Center;
                _xpText.SetText("0/1000");

                _xpText.color = new Color(1f, 0.6f, 0f, 1f);
                ////// Level Text /////
                _xpLevel = GameObject.Instantiate(_gameXPText).GetComponent<TextMeshProUGUI>();
                _xpLevel.transform.SetParent(_xpInfoContainer.transform, false);
                _xpLevel.transform.position = new Vector3(_xpBar.transform.position.x,
                    _xpBar.transform.position.y, _xpBar.transform.position.z);
                _xpLevel.transform.Translate(-0.3f, 0.2f, 0f);//x +.7, y -.2
                _xpLevel.name = "XPLevel";
                _xpLevel.alignment = TextAlignmentOptions.Center;
                _xpLevel.SetText("Level: 0");
                _xpLevel.color = new Color(1f, 0.6f, 0f, 1f);
                //Level x.7 y.2
                //Profit x.7 -y.2
                ///// PROFIT! /////
                _profit = GameObject.Instantiate(_gameXPText).GetComponent<TextMeshProUGUI>();
                _profit.transform.SetParent(_xpInfoContainer.transform, false);
                _profit.transform.position = new Vector3(_xpBar.transform.position.x,
                    _xpBar.transform.position.y, _xpBar.transform.position.z);
                _profit.transform.Translate(-0.10f, -0.2f, 0f);//x +.7, y -.2
                _profit.name = "XPProfit";
                _profit.alignment = TextAlignmentOptions.Center;
                _profit.SetText("You've made.. 0$.");
                _profit.color = new Color(1f, 0.6f, 0f, 1f);
            }

            if (!_xpBarProgress)
            {
                ////// XP Progress //////
                GameObject _gameXPBarProgress = GameObject.Find("/Systems/UI/Canvas/EndgameStats/LevelUp/LevelUpMeter");
                _xpBarProgress = GameObject.Instantiate(_gameXPBarProgress);
                _xpBarProgress.transform.SetParent(_xpBar.transform, false);
                _xpBarProgress.transform.localScale = new Vector3(0.597f, 5.21f, 1f);
                _xpBarProgress.transform.Translate(-0.8f, 0.2f, 0f);
                Vector3 pos = _xpBarProgress.transform.localPosition;
                _xpBarProgress.transform.localPosition = new Vector3(pos.x + 7f, pos.y - 3.5f, 0f);
                _xpBarProgress.name = "XPBarProgress";
                _xpBarProgress.GetComponent<Image>().fillAmount = 0f;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.OpenQuickMenu))]
        private static void SkillTreeAwake(QuickMenuManager __instance)
        {
            // Check if menucontainer is active
            if (!__instance.isMenuOpen)
            {
                return;
            }

            if (!skillTreeButton)
            {
                MakeSkillTreeButton();
            }
        }

        private static void MakeSkillTreeButton()
        {
            GameObject ResumeButton = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons/Resume");
            skillTreeButton = GameObject.Instantiate(ResumeButton);

            GameObject MainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
            skillTreeButton.transform.SetParent(MainButtons.transform, false);
            Vector3 position = _xpBar.transform.position;
            skillTreeButton.transform.position = new Vector3(0.55f + position.x, 1.09f + position.y, position.z);
            skillTreeButton.name = "Skills";
            skillTreeButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Skills";
            position = _xpBar.transform.position;
            skillTreeButton.transform.localPosition = new Vector3(position.x, position.y, position.z);
            // Change the onClick event to our own.
            skillTreeButton.transform.position += new Vector3(-0.15f, 1.056f);
            Button.ButtonClickedEvent OnClickEvent = new Button.ButtonClickedEvent();
            OnClickEvent.AddListener(OpenSkillTree);
            skillTreeButton.GetComponent<Button>().onClick = OnClickEvent;

            //Make the level bar clickable.
            Button button = _xpBar.GetOrAddComponent<Button>();
            button.onClick = OnClickEvent;

            button = _xpBarProgress.GetOrAddComponent<Button>();
            button.onClick = OnClickEvent;
        }

        private static void OpenSkillTree()
        {
            LP_NetworkManager.xpInstance.guiObj.OpenSkillMenu();
        }
    }
}
