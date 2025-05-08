using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LethalProgression.Skills
{
    internal class HandSlots
    {
        public static int currentSlotCount = 4;
        public static void HandSlotsUpdate(int updateValue)
        {
            if (LethalPlugin.ReservedSlots)
            {
                return;
            }

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillListValid())
            {
                return;
            }

            if (!LP_NetworkManager.xpInstance.skillList.IsSkillValid(UpgradeType.HandSlot))
            {
                return;
            }

            LC_XP xpInstance = LP_NetworkManager.xpInstance;

            int skillPercent = (int)xpInstance.skillList.skills[UpgradeType.HandSlot].GetTrueValue();
            int slotsToAdd = (int)Math.Floor((double)(skillPercent / 100));

            // Tell the server we've updated our hand slots.
            ulong playerID = GameNetworkManager.Instance.localPlayerController.playerClientId;
            xpInstance.ServerHandSlots_ServerRpc(playerID, slotsToAdd);
        }

        public static void UpdateHudSlots()
        {
            LC_XP xpInstance = LP_NetworkManager.xpInstance;

            // Credit to Mike for this code! https://github.com/MikeS-MS/MikesTweaks/
            float addedSlots = xpInstance.skillList.skills[UpgradeType.HandSlot].GetTrueValue() / 100;
            int slotCount = 4 + (int)Math.Floor(addedSlots);
            int addedSlot = slotCount - currentSlotCount;

            GameObject inventory = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory");
            List<string> slotIgnore = new List<string>() { "Slot0", "Slot1", "Slot2", "Slot3" };
            for (int i = 0; i < inventory.transform.childCount; i++)
            {
                Transform child = inventory.transform.GetChild(i);
                if (slotIgnore.Contains(child.gameObject.name))
                {
                    continue;
                }

                Object.Destroy(child.gameObject);
            }

            int skillPercent = (int)xpInstance.skillList.skills[UpgradeType.HandSlot].GetTrueValue();
            int slotsToAdd = (int)Math.Floor((double)(skillPercent / 100));

            // Prepare the arrays
            Image[] ItemSlotIconFrames = new Image[slotCount];
            ItemSlotIconFrames[0] = HUDManager.Instance.itemSlotIconFrames[0];
            ItemSlotIconFrames[1] = HUDManager.Instance.itemSlotIconFrames[1];
            ItemSlotIconFrames[2] = HUDManager.Instance.itemSlotIconFrames[2];
            ItemSlotIconFrames[3] = HUDManager.Instance.itemSlotIconFrames[3];

            Image[] ItemSlotIcons = new Image[slotCount];
            ItemSlotIcons[0] = HUDManager.Instance.itemSlotIcons[0];
            ItemSlotIcons[1] = HUDManager.Instance.itemSlotIcons[1];
            ItemSlotIcons[2] = HUDManager.Instance.itemSlotIcons[2];
            ItemSlotIcons[3] = HUDManager.Instance.itemSlotIcons[3];

            GameObject Slot3 = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory/Slot3");
            GameObject Slot4 = xpInstance.guiObj.templateSlot;
            GameObject CurrentSlot = Slot3;
            currentSlotCount = slotCount;

            // Spawn more UI slots.
            for (int i = 0; i < (int)addedSlots; i++)
            {
                GameObject NewSlot = Object.Instantiate(Slot4);

                // This might break if someone else makes slots with these names in their mod..
                NewSlot.name = $"Slot{3 + (i + 1)}";

                NewSlot.transform.SetParent(inventory.transform);

                // Change locations.
                Vector3 localPosition = CurrentSlot.transform.localPosition;
                NewSlot.transform.SetLocalPositionAndRotation(
                                                            new Vector3(localPosition.x + 50, localPosition.y, localPosition.z),
                                                            CurrentSlot.transform.localRotation);
                CurrentSlot = NewSlot;

                ItemSlotIconFrames[4 + i] = NewSlot.GetComponent<Image>();
                ItemSlotIcons[4 + i] = NewSlot.transform.GetChild(0).GetComponent<Image>();
                NewSlot.SetActive(true);
            }

            for (int i = 0; i < ItemSlotIconFrames.Length; i++)
            {
                // use addedSlot to determine how much to move the slots.
                Vector3 localPositionFrame = ItemSlotIconFrames[i].transform.localPosition;
                ItemSlotIconFrames[i].transform.SetLocalPositionAndRotation(
                    new Vector3(localPositionFrame.x - (addedSlot * 25), localPositionFrame.y, localPositionFrame.z),
                    ItemSlotIconFrames[i].transform.localRotation);
            }

            HUDManager.Instance.itemSlotIconFrames = ItemSlotIconFrames;
            HUDManager.Instance.itemSlotIcons = ItemSlotIcons;
        }

        public static bool IsItemSwitchPossible(PlayerControllerB player)
        {
            return (double)player.timeSinceSwitchingSlots >= 0.01 &&
                !player.inTerminalMenu && !player.isGrabbingObjectAnimation
                && !player.inSpecialInteractAnimation && !player.throwingObject
                && !player.isTypingChat && !player.twoHanded && !player.activatingItem
                && !player.jetpackControls && !player.disablingJetpackControls;
        }

        // Thanks @evaisa
        public static bool SwitchItemSlots(PlayerControllerB player, int requestedSlot)
        {
            if (!IsItemSwitchPossible(player) || player.currentItemSlot == requestedSlot)
            {
                return false;
            }
            LethalPlugin.Log.LogDebug($"Trying to switch to slot {requestedSlot}");

            int num = player.currentItemSlot - requestedSlot;
            bool flag = num > 0;
            if (Math.Abs(num) == player.ItemSlots.Length - 1)
            {
                player.SwitchItemSlotsServerRpc(flag);
            }
            else
            {
                do
                {
                    player.SwitchItemSlotsServerRpc(flag);
                    num += (flag ? -1 : 1);
                }
                while (num != 0);
            }
            ShipBuildModeManager.Instance.CancelBuildMode(true);
            player.playerBodyAnimator.SetBool("GrabValidated", false);
            LethalPlugin.Log.LogDebug($"Switched to slot {requestedSlot}");

            player.SwitchToItemSlot(requestedSlot, null);
            if (player.currentlyHeldObjectServer != null)
            {
                player.currentlyHeldObjectServer.gameObject.GetComponent<AudioSource>().PlayOneShot(player.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
            }
            return true;
        }
    }
}
