using HarmonyLib;
using LethalProgression.Config;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalProgression.GUI
{
    [HarmonyPatch]
    internal class GUIUpdate
    {
        public static bool isMenuOpen = false;
        public static SkillsGUI guiInstance;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.Update))]
        private static void SkillMenuUpdate(QuickMenuManager __instance)
        {
            if (guiInstance == null || !guiInstance.mainPanel)
            {
                return;
            }

            // If the menu is open, activate mainPanel.
            if (!isMenuOpen)
            {
                guiInstance.mainPanel.SetActive(false);
                return;
            }

            if (bool.Parse(SkillConfig.hostConfig["Unspec in Ship Only"]) && !bool.Parse(SkillConfig.hostConfig["Disable Unspec"]))
            {
                // Check if you are in the ship right now
                guiInstance.SetUnspec(GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
            }

            if (bool.Parse(SkillConfig.hostConfig["Unspec in Orbit Only"]))
            {
                // Check if you are in orbit right now

                guiInstance.SetUnspec(StartOfRound.Instance.inShipPhase);
            }

            if (bool.Parse(SkillConfig.hostConfig["Disable Unspec"]))
            {
                guiInstance.SetUnspec(false);
            }
            GameObject pointsPanel = guiInstance.mainPanel.transform.GetChild(2).gameObject;
            GameObject toolTip = pointsPanel.transform.GetChild(2).gameObject;
            // If the mouse is on the points panel, show the tooltip.
            toolTip.SetActive(IsHovering());

            guiInstance.mainPanel.SetActive(true);
            GameObject mainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
            mainButtons.SetActive(false);

            GameObject playerList = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList");
            playerList.SetActive(false);

            RealTimeUpdateInfo();
        }
        public static bool IsHovering()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            // If the mouse is currently on the PointsPanel
            GameObject pointsPanel = guiInstance.mainPanel.transform.GetChild(2).gameObject;
            float xLeast = pointsPanel.transform.position.x - pointsPanel.GetComponent<RectTransform>().rect.width;
            float xMost = pointsPanel.transform.position.x + pointsPanel.GetComponent<RectTransform>().rect.width;
            float yLeast = pointsPanel.transform.position.y - pointsPanel.GetComponent<RectTransform>().rect.height;
            float yMost = pointsPanel.transform.position.y + pointsPanel.GetComponent<RectTransform>().rect.height;

            return mousePos.x >= xLeast && mousePos.x <= xMost && mousePos.y >= yLeast && mousePos.y <= yMost;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.CloseQuickMenu))]
        private static void SkillMenuClose(QuickMenuManager __instance)
        {
            isMenuOpen = false;
        }
        private static void RealTimeUpdateInfo()
        {
            GameObject tempObj = guiInstance.mainPanel.transform.GetChild(2).gameObject;
            tempObj = tempObj.transform.GetChild(1).gameObject;

            TextMeshProUGUI points = tempObj.GetComponent<TextMeshProUGUI>();
            points.text = LP_NetworkManager.xpInstance.GetSkillPoints().ToString();
        }
    }
}