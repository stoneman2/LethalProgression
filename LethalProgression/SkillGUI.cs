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

namespace LethalProgression.GUI
{
    [HarmonyPatch]
    internal class GUIUpdate
    {
        public static bool isMenuOpen = false;
        public static SkillsGUI guiInstance;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "Update")]
        private static void SkillMenuUpdate(QuickMenuManager __instance)
        {
            if (guiInstance == null)
                return;

            if (!guiInstance.mainPanel)
                return;

            // If the menu is open, activate mainPanel.
            if (!isMenuOpen)
            {
                guiInstance.mainPanel.SetActive(false);
                return;
            }


            if (bool.Parse(SkillConfig.hostConfig["Unspec in Ship Only"]) && !bool.Parse(SkillConfig.hostConfig["Disable Unspec"]))
            {
                // Check if you are in the ship right now
                if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                {
                    guiInstance.SetUnspec(true);
                }
                else
                {
                    guiInstance.SetUnspec(false);
                }
            }

            if (bool.Parse(SkillConfig.hostConfig["Unspec in Orbit Only"]))
            {
                // Check if you are in orbit right now
                if (StartOfRound.Instance.inShipPhase)
                {
                    guiInstance.SetUnspec(true);
                }
                else
                {
                    guiInstance.SetUnspec(false);
                }
            }

            if (bool.Parse(SkillConfig.hostConfig["Disable Unspec"]))
            {
                guiInstance.SetUnspec(false);
            }

            // Get mouse position.



            guiInstance.mainPanel.SetActive(true);
            GameObject mainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
            mainButtons.SetActive(false);

            GameObject playerList = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList");
            playerList.SetActive(false);

            RealTimeUpdateInfo();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuickMenuManager), "CloseQuickMenu")]
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
    internal class SkillsGUI
    {
        public GameObject mainPanel;
        public GameObject infoPanel;
        public Skill activeSkill;
        public GameObject templateSlot;
        public List<GameObject> skillButtonsList = new List<GameObject>();
        public SkillsGUI()
        {
            CreateSkillMenu();
            GUIUpdate.guiInstance = this;
        }
        public void OpenSkillMenu()
        {
            GUIUpdate.isMenuOpen = true;
            mainPanel.SetActive(true);
        }
        public int shownSkills = 0;
        public void CreateSkillMenu()
        {
            mainPanel = GameObject.Instantiate(LethalPlugin.skillBundle.LoadAsset<GameObject>("SkillMenu"));
            mainPanel.name = "SkillMenu";

            mainPanel.SetActive(false);

            templateSlot = GameObject.Instantiate(GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory/Slot3"));
            templateSlot.name = "TemplateSlot";
            templateSlot.SetActive(false);

            infoPanel = mainPanel.transform.GetChild(1).gameObject;
            infoPanel.SetActive(false);

            GameObject backButton = mainPanel.transform.GetChild(4).gameObject;
            backButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            backButton.GetComponent<Button>().onClick.AddListener(BackButton);

            //////////////////////////////////////////////////
            /// Different upgrade buttons:
            //////////////////////////////////////////////////
            shownSkills = 0;

            if (LP_NetworkManager.xpInstance.skillList.skills == null)
            {
                //LethalPlugin.Log.LogInfo("Skill list is null!");
                return;
            }

            foreach (KeyValuePair<UpgradeType, Skill> skill in LP_NetworkManager.xpInstance.skillList.skills)
            {
                LethalPlugin.Log.LogInfo("Creating button for " + skill.Value.GetShortName());
                GameObject skillButton = SetupUpgradeButton(skill.Value);
                LethalPlugin.Log.LogInfo("Setup passed!");

                skillButtonsList.Add(skillButton);
                LethalPlugin.Log.LogInfo("Added to skill list..");
                LoadSkillData(skill.Value, skillButton);
            }

            TeamLootHudUpdate(1, 1);
        }

        public void BackButton()
        {
            GUIUpdate.isMenuOpen = false;
            GameObject mainButtons = GameObject.Find("Systems/UI/Canvas/QuickMenu/MainButtons");
            mainButtons.SetActive(true);

            GameObject playerList = GameObject.Find("Systems/UI/Canvas/QuickMenu/PlayerList");
            playerList.SetActive(true);
        }
        public void SetUnspec(bool show)
        {
            GameObject minusFive = infoPanel.transform.GetChild(6).gameObject;
            GameObject minusTwo = infoPanel.transform.GetChild(7).gameObject;
            GameObject minusOne = infoPanel.transform.GetChild(8).gameObject;
            minusFive.SetActive(show);
            minusTwo.SetActive(show);
            minusOne.SetActive(show);

            if (!bool.Parse(SkillConfig.hostConfig["Disable Unspec"]))
            {
                GameObject unSpecHelpText = infoPanel.transform.GetChild(9).gameObject;
                unSpecHelpText.SetActive(!show);
            }

            if (bool.Parse(SkillConfig.hostConfig["Unspec in Orbit Only"]))
            {
                GameObject unSpecHelpText = infoPanel.transform.GetChild(9).gameObject;
                unSpecHelpText.transform.GetComponent<TextMeshProUGUI>().SetText("Return to orbit to unspec.");
            }
        }
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
            GameObject skillContents = skillScroller.transform.GetChild(1).gameObject;
            button.transform.SetParent(skillContents.transform, false);

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

        public void LoadSkillData(Skill skill, GameObject skillButton)
        {
            if (skill._teamShared)
                return;
            GameObject bonusLabel = skillButton.transform.GetChild(1).gameObject;
            bonusLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetLevel().ToString());
            GameObject attributeLabel = skillButton.transform.GetChild(2).gameObject;
            attributeLabel.GetComponent<TextMeshProUGUI>().SetText($"(+{skill.GetLevel() * skill.GetMultiplier()}% {skill.GetAttribute()})");

            skillButton.GetComponentInChildren<TextMeshProUGUI>().SetText($"{skill.GetShortName()}:");
        }

        public void UpdateAllStats()
        {
            foreach (KeyValuePair<UpgradeType, Skill> skill in LP_NetworkManager.xpInstance.skillList.skills)
            {
                if (skill.Value._teamShared)
                    continue;
                GameObject skillButton = skillButtonsList.Find(x => x.name == skill.Value.GetShortName());
                LoadSkillData(skill.Value, skillButton);
            }
        }

        public void UpdateStatInfo(Skill skill)
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
            //upgradeAmt.SetText(skill.GetLevel().ToString());
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

        public void AddSkillPoint(Skill skill, int amt)
        {
            if (LP_NetworkManager.xpInstance.GetSkillPoints() <= 0)
            {
                return;
            }

            int skillPoints = LP_NetworkManager.xpInstance.GetSkillPoints();

            if (skillPoints < amt)
            {
                amt = skillPoints;
            }

            if (skill.GetLevel() + amt > skill.GetMaxLevel())
            {
                amt = skill.GetMaxLevel() - skill.GetLevel();
            }

            skill.AddLevel(amt);
            LP_NetworkManager.xpInstance.SetSkillPoints(LP_NetworkManager.xpInstance.GetSkillPoints() - amt);
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
            LP_NetworkManager.xpInstance.SetSkillPoints(LP_NetworkManager.xpInstance.GetSkillPoints() + amt);
            UpdateStatInfo(skill);

            foreach (var button in skillButtonsList)
            {
                if (button.name == skill.GetShortName())
                    LoadSkillData(skill, button);
            }
        }

        // START SPECIAL BOYS:
        public void TeamLootHudUpdate(float oldValue, float newValue)
        {
            foreach (var button in skillButtonsList)
            {
                if (button.name == "VAL")
                {
                    Skill skill = LP_NetworkManager.xpInstance.skillList.skills[UpgradeType.Value];
                    LoadSkillData(skill, button);

                    GameObject displayLabel = button.transform.GetChild(0).gameObject;
                    displayLabel.GetComponent<TextMeshProUGUI>().SetText(skill.GetShortName());

                    GameObject bonusLabel = button.transform.GetChild(1).gameObject;
                    bonusLabel.GetComponent<TextMeshProUGUI>().SetText($"{skill.GetLevel()}");
                    button.GetComponentInChildren<TextMeshProUGUI>().SetText($"{skill.GetShortName()}:");

                    GameObject attributeLabel = button.transform.GetChild(2).gameObject;
                    attributeLabel.GetComponent<TextMeshProUGUI>().SetText($"(+{LP_NetworkManager.xpInstance.teamLootValue.Value}% {skill.GetAttribute()})");
                    LethalPlugin.Log.LogInfo($"Setting team value hud to {LP_NetworkManager.xpInstance.teamLootValue.Value}");
                }
            }
        }
}