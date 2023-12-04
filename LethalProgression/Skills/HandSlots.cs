using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using UnityEngine.InputSystem.Utilities;
using GameNetcodeStuff;

namespace LethalProgression.Skills
{
    internal class HandSlots
    {
        public static void HandSlotsUpdate(int oldHandSlots, int newHandSlots)
        {
            // Credit to Mike for this code! https://github.com/MikeS-MS/MikesTweaks/

            GameObject inventory = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory");
            List<string> slotIgnore = new List<string>() { "Slot0", "Slot1", "Slot2", "Slot3" };
            for (int i = 0; i < inventory.transform.childCount; i++)
            {
                Transform child = inventory.transform.GetChild(i);
                if (slotIgnore.Contains(child.gameObject.name))
                    continue;

                Object.Destroy(child.gameObject);
            }

            int skillPercent = (int)LethalProgression.XPHandler.xpInstance.skillList.skills["Hand Slot"].GetTrueValue();
            int skillAmount = (int)Math.Floor((double)(skillPercent / 100));

            // Prepare the arrays
            Image[] ItemSlotIconFrames = new Image[4 + skillAmount];
            ItemSlotIconFrames[0] = HUDManager.Instance.itemSlotIconFrames[0];
            ItemSlotIconFrames[1] = HUDManager.Instance.itemSlotIconFrames[1];
            ItemSlotIconFrames[2] = HUDManager.Instance.itemSlotIconFrames[2];
            ItemSlotIconFrames[3] = HUDManager.Instance.itemSlotIconFrames[3];

            Image[] ItemSlotIcons = new Image[4 + skillAmount];
            ItemSlotIcons[0] = HUDManager.Instance.itemSlotIcons[0];
            ItemSlotIcons[1] = HUDManager.Instance.itemSlotIcons[1];
            ItemSlotIcons[2] = HUDManager.Instance.itemSlotIcons[2];
            ItemSlotIcons[3] = HUDManager.Instance.itemSlotIcons[3];

            GameObject Slot4 = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory/Slot3");
            GameObject CurrentSlot = Slot4;

            // Spawn more UI slots.
            for (int i = 0; i < skillAmount; i++)
            {
                GameObject NewSlot = Object.Instantiate(Slot4);
                NewSlot.name = $"Slot{3 + (i + 1)}";
                NewSlot.transform.SetParent(inventory.transform);

                // Change locations.
                Vector3 localPosition = CurrentSlot.transform.localPosition;
                NewSlot.transform.SetLocalPositionAndRotation(
                                                            new Vector3(localPosition.x + 50, localPosition.y, localPosition.z),
                                                            CurrentSlot.transform.localRotation);
                CurrentSlot = NewSlot;

                ItemSlotIconFrames[3 + (i + 1)] = NewSlot.GetComponent<Image>();
                ItemSlotIcons[3 + (i + 1)] = NewSlot.transform.GetChild(0).GetComponent<Image>(); ;
            }

            HUDManager.Instance.itemSlotIconFrames = ItemSlotIconFrames;
            HUDManager.Instance.itemSlotIcons = ItemSlotIcons;



            // Tell the server we've updated our hand slots.
            ulong playerID = GameNetworkManager.Instance.localPlayerController.playerClientId;
            LethalProgression.XPHandler.xpInstance.ServerHandSlots_ServerRpc(skillAmount, playerID);
        }
    }
}
