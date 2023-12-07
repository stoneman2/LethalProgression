using HarmonyLib;
using LethalProgression.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LethalProgression.Patches;
using LethalProgression.Config;
using GameNetcodeStuff;

namespace LethalProgression.GUI
{
    [HarmonyPatch]
    internal class QuickMenuManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "Update")]
        private static void SkillMenuUpdate(QuickMenuManager __instance)
        {
            if (!XPHandler.xpInstance.guiObj.mainPanel)
                return;

            // If the menu is open, activate mainPanel.
            if (XPHandler.xpInstance.guiObj.isMenuOpen)
            {
                if (bool.Parse(SkillConfig.hostConfig["Unspec in Ship Only"]))
                {
                    // Check if you are in the ship right now
                    if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                    {
                        XPHandler.xpInstance.guiObj.SetUnspec(true);
                    }
                    else
                    {
                        XPHandler.xpInstance.guiObj.SetUnspec(false);
                    }
                }

                GameObject scrollRect = XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(3).gameObject;
                if (scrollRect.GetComponent<ScrollRect>().verticalNormalizedPosition >= 0.95f) // Scrolled near end
                {
                    XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(4).gameObject.SetActive(true);
                    XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(5).gameObject.SetActive(false);
                }
                else if (scrollRect.GetComponent<ScrollRect>().verticalNormalizedPosition <= 0.05f) // At the top
                {
                    XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(4).gameObject.SetActive(false);
                    XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(5).gameObject.SetActive(true);
                }
                else
                {
                    XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(4).gameObject.SetActive(false);
                }

                // Get mouse position.
                Vector2 mousePos = Mouse.current.position.ReadValue();
                // If the mouse is currently on the PointsPanel
                GameObject pointsPanel = XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(2).gameObject;
                float xLeast = pointsPanel.transform.position.x - pointsPanel.GetComponent<RectTransform>().rect.width;
                float xMost = pointsPanel.transform.position.x + pointsPanel.GetComponent<RectTransform>().rect.width;

                float yLeast = pointsPanel.transform.position.y - pointsPanel.GetComponent<RectTransform>().rect.height;
                float yMost = pointsPanel.transform.position.y + pointsPanel.GetComponent<RectTransform>().rect.height;
                if (mousePos.x >= xLeast && mousePos.x <= xMost)
                {
                    if (mousePos.y >= yLeast && mousePos.y <= yMost)
                    {
                        // If the mouse is on the points panel, show the tooltip.
                        XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(2).GetChild(2).gameObject.SetActive(true);
                    }
                    else
                    {
                        XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
                    }
                }
                else
                {
                    XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
                }

                XPHandler.xpInstance.guiObj.mainPanel.SetActive(true);
                GameObject mainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
                mainButtons.SetActive(false);

                GameObject playerList = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList");
                playerList.SetActive(false);

                RealTimeUpdateInfo();
            }
            else
            {
                XPHandler.xpInstance.guiObj.mainPanel.SetActive(false);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "CloseQuickMenu")]
        private static void SkillMenuClose(QuickMenuManager __instance)
        {
            // This runs when someone presses esc in our menu
            XPHandler.xpInstance.guiObj.isMenuOpen = false;
        }

        private static void RealTimeUpdateInfo()
        {
            GameObject tempObj = XPHandler.xpInstance.guiObj.mainPanel.transform.GetChild(2).gameObject;
            tempObj = tempObj.transform.GetChild(1).gameObject;

            TextMeshProUGUI points = tempObj.GetComponent<TextMeshProUGUI>();
            points.text = XPHandler.xpInstance.GetSkillPoints().ToString();
        }
    }
    internal class SkillsGUI
    {
        public GameObject mainPanel;
        public GameObject infoPanel;
        public Skill activeSkill;
        public List<GameObject> skillButtonsList = new List<GameObject>();
        public bool isMenuOpen = false;
        public GameObject templateSlot;
        public SkillsGUI()
        {
            CreateSkillMenu();
        }
        public void OpenSkillMenu()
        {
            isMenuOpen = true;
            mainPanel.SetActive(true);
        }

        public void CreateSkillMenu()
        {
            mainPanel = GameObject.Instantiate(LethalPlugin.skillBundle.LoadAsset<GameObject>("SkillMenu"));
            mainPanel.name = "SkillMenu";

            templateSlot = GameObject.Instantiate(GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory/Slot3"));
            templateSlot.name = "TemplateSlot";
            templateSlot.SetActive(false);

            mainPanel.SetActive(false);

            infoPanel = mainPanel.transform.GetChild(1).gameObject;
            infoPanel.SetActive(false);

            GameObject resumeButton = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons/Resume");
            GameObject backButton = GameObject.Instantiate(resumeButton);
            backButton.name = "Back";
            backButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Back";

            backButton.transform.SetParent(mainPanel.transform, false);
            backButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            backButton.GetComponent<Button>().onClick.AddListener(BackButton);

            backButton.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            backButton.transform.Translate(-700f, -900f, 0f);

            //////////////////////////////////////////////////
            /// Different upgrade buttons:
            //////////////////////////////////////////////////
            shownSkills = 0;

            foreach (KeyValuePair<UpgradeType, Skill> skill in XPHandler.xpInstance.skillList.skills)
            {
                LethalPlugin.Log.LogInfo("Creating button for " + skill.Value.GetShortName());
                GameObject skillButton = SetupUpgradeButton(skill.Value);

                skillButtonsList.Add(skillButton);
                LoadSkillData(skill.Value, skillButton);
            }

            TeamLootHudUpdate(1, 1);
        }

        public void BackButton()
        {
            isMenuOpen = false;
            GameObject mainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
            mainButtons.SetActive(true);

            GameObject playerList = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList");
            playerList.SetActive(true);
        }

        public int shownSkills = 0;
        public GameObject SetupUpgradeButton(LethalProgression.Skills.Skill skill)
        {
            GameObject templateButton = mainPanel.transform.GetChild(0).gameObject;
            GameObject button = GameObject.Instantiate(templateButton);
            if (!templateButton)
            {
                LethalPlugin.Log.LogError("Couldn't find template button!");
                return null;
            }

            button.name = skill.GetShortName();

            GameObject skillScroller = mainPanel.transform.GetChild(3).gameObject;
            GameObject skillContents = skillScroller.transform.GetChild(0).gameObject;
            button.transform.SetParent(skillContents.transform, false);
            //button.transform.Translate(0f, -100f * shownSkills, 0f);

            shownSkills++;

            GameObject displayLabel = button.transform.GetChild(0).gameObject;
            displayLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetShortName());

            GameObject bonusLabel = button.transform.GetChild(1).gameObject;
            bonusLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetLevel().ToString());
            GameObject attributeLabel = button.transform.GetChild(2).gameObject;
            attributeLabel.GetComponent<TextMeshProUGUI>().SetText("(" + skill.GetLevel() + " " + skill.GetAttribute() + ")");

            button.GetComponentInChildren<TextMeshProUGUI>().SetText(skill.GetShortName() + ":");

            button.SetActive(true);

            button.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<Button>().onClick.AddListener(delegate { UpdateStatInfo(skill); });
            return button;
        }

        public void LoadSkillData(LethalProgression.Skills.Skill skill, GameObject skillButton)
        {
            if (skill._teamShared)
                return;
            GameObject displayLabel = skillButton.transform.GetChild(0).gameObject;
            displayLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetShortName());

            GameObject bonusLabel = skillButton.transform.GetChild(1).gameObject;
            bonusLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetLevel().ToString());
            GameObject attributeLabel = skillButton.transform.GetChild(2).gameObject;
            attributeLabel.GetComponent<TextMeshProUGUI>().SetText("(+" + skill.GetLevel() * skill.GetMultiplier() + "% " + skill.GetAttribute() + ")");

            skillButton.GetComponentInChildren<TextMeshProUGUI>().SetText(skill.GetShortName() + ":");
        }

        public void UpdateStatInfo(LethalProgression.Skills.Skill skill)
        {
            if (!infoPanel.activeSelf)
                infoPanel.SetActive(true);
            TextMeshProUGUI upgradeName = infoPanel.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI upgradeAmt = infoPanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI upgradeDesc = infoPanel.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();

            activeSkill = skill;
            upgradeName.SetText(skill.GetName());
            if (skill.GetMaxLevel() == 99999)
            {
                upgradeAmt.SetText($"{skill.GetLevel()}");
            }
            else
            {
                upgradeAmt.SetText($"{skill.GetLevel()} / {skill.GetMaxLevel()}");
            }
            upgradeAmt.SetText(skill.GetLevel().ToString());
            upgradeDesc.SetText(skill.GetDescription());

            // Make all the buttons do something:
            GameObject plusFive = infoPanel.transform.GetChild(3).gameObject;
            GameObject plusTwo = infoPanel.transform.GetChild(4).gameObject;
            GameObject plusOne = infoPanel.transform.GetChild(5).gameObject;

            plusFive.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            plusFive.GetComponent<Button>().onClick.AddListener(delegate { AddSkillPoint(skill, 5); });

            plusTwo.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            plusTwo.GetComponent<Button>().onClick.AddListener(delegate { AddSkillPoint(skill, 2); });

            plusOne.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            plusOne.GetComponent<Button>().onClick.AddListener(delegate { AddSkillPoint(skill, 1); });

            GameObject minusFive = infoPanel.transform.GetChild(6).gameObject;
            GameObject minusTwo = infoPanel.transform.GetChild(7).gameObject;
            GameObject minusOne = infoPanel.transform.GetChild(8).gameObject;

            minusFive.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            minusFive.GetComponent<Button>().onClick.AddListener(delegate { RemoveSkillPoint(skill, 5); });

            minusTwo.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            minusTwo.GetComponent<Button>().onClick.AddListener(delegate { RemoveSkillPoint(skill, 2); });

            minusOne.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            minusOne.GetComponent<Button>().onClick.AddListener(delegate { RemoveSkillPoint(skill, 1); });
        }

        public void SetUnspec(bool show)
        {
            GameObject minusFive = infoPanel.transform.GetChild(6).gameObject;
            GameObject minusTwo = infoPanel.transform.GetChild(7).gameObject;
            GameObject minusOne = infoPanel.transform.GetChild(8).gameObject;

            minusFive.SetActive(show);
            minusTwo.SetActive(show);
            minusOne.SetActive(show);

            GameObject unSpecHelpText = infoPanel.transform.GetChild(9).gameObject;
            unSpecHelpText.SetActive(!show);
        }
        public void AddSkillPoint(LethalProgression.Skills.Skill skill, int amt)
        {
            if (XPHandler.xpInstance.GetSkillPoints() <= 0)
            {
                return;
            }

            int skillPoints = XPHandler.xpInstance.GetSkillPoints();

            if (skillPoints < amt)
            {
                amt = skillPoints;
            }

            if (skill.GetLevel() + amt > skill.GetMaxLevel())
            {
                amt = skill.GetMaxLevel() - skill.GetLevel();
            }

            skill.AddLevel(amt);
            XPHandler.xpInstance.SetSkillPoints(XPHandler.xpInstance.GetSkillPoints() - amt);
            UpdateStatInfo(skill);

            foreach (var button in skillButtonsList)
            {
                if (button.name == skill.GetShortName())
                LoadSkillData(skill, button);
            }
        }

        public void RemoveSkillPoint(LethalProgression.Skills.Skill skill, int amt)
        {
            if (skill.GetLevel() == 0)
                return;

            int allocatedPoints = skill.GetLevel();
            if (allocatedPoints < amt)
            {
                amt = allocatedPoints;
            }

            skill.AddLevel(-amt);
            XPHandler.xpInstance.SetSkillPoints(XPHandler.xpInstance.GetSkillPoints() + amt);
            UpdateStatInfo(skill);

            foreach (var button in skillButtonsList)
            {
                if (button.name == skill.GetShortName())
                    LoadSkillData(skill, button);
            }
        }

        // START SPECIAL BOYS:
        public void TeamLootHudUpdate(int oldValue, int newValue)
        {
            foreach (var button in skillButtonsList)
            {
                if (button.name == "VAL")
                {
                    Skill skill = XPHandler.xpInstance.skillList.skills[UpgradeType.Value];
                    LoadSkillData(skill, button);

                    GameObject displayLabel = button.transform.GetChild(0).gameObject;
                    displayLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetShortName());

                    GameObject bonusLabel = button.transform.GetChild(1).gameObject;
                    bonusLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetLevel().ToString());
                    button.GetComponentInChildren<TextMeshProUGUI>().SetText(skill.GetShortName() + ":");

                    GameObject attributeLabel = button.transform.GetChild(2).gameObject;
                    attributeLabel.GetComponent<TextMeshProUGUI>().SetText("(+" + XPHandler.xpInstance.teamLootValue.Value * skill.GetMultiplier() + "% " + skill.GetAttribute() + ")");
                }
            }
        }
    }
}
