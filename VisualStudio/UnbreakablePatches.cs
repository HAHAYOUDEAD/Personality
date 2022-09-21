using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using UnityEngine.SceneManagement;
using ModSettings;
using System.IO;
using System.Reflection;

namespace CharacterCustomizer
{
    public static class HarmonyStuff
    {
        public static bool UpdateSlotIfNeeded(GearItem item, Slot slot)
        {
            string itemNameStripped = item?.name.Replace("GEAR_", "");

            Utility.Log(ConsoleColor.DarkGray, $"CheckForChangeLayer - checking for change in {slot} for {itemNameStripped}");

            if (item)
            {
                string clothingSet = Equipment.CheckSlotShouldBeChanged(slot, itemNameStripped);

                if (clothingSet != null)
                {
                    Equipment.ChangeEquipIfExistsOtherwiseDefault(slot, clothingSet);
                    Equipment.ChangeEquipVariant(slot, PartVariant.Normal);
                    Utility.Log(ConsoleColor.Gray, $"CheckForChangeLayer - {slot} should be changed to {clothingSet}");
                    return true;
                }
            }

            if (!item && Equipment.currentEquipment[slot].currentVariantEnum != PartVariant.Disabled)
            {
                Utility.Log(ConsoleColor.Gray, $"CheckForChangeLayer - {slot} should be disabled");
                Equipment.ChangeEquipVariant(slot, PartVariant.Disabled);
                return true;
            }
            return false;
        }
    }



    [HarmonyPatch(typeof(ClothingSlot), "CheckForChangeLayer")]
    public class GetClothingSlotUpdate
    {
        public static void Prefix(ClothingSlot __instance)
        {
            ClothingRegion region = __instance.GetClothingRegion();
            ClothingLayer layer = __instance.GetClothingLayer();
            
            GearItem item = __instance.m_GearItem;

            bool changed = false;
            
            if (region == ClothingRegion.Chest)
            {
                if (layer == ClothingLayer.Mid) // shirt
                {
                    if (!item)
                    {
                        item = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Base); // if no item in outer, check inner slot
                    }

                    changed = HarmonyStuff.UpdateSlotIfNeeded(item, Slot.Shirt);
                }

                if (layer == ClothingLayer.Top2) // jacket 
                {
                    if (!item)
                    {
                        item = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Top);
                    }

                    changed = HarmonyStuff.UpdateSlotIfNeeded(item, Slot.Jacket);
                }
            }
            if (region == ClothingRegion.Hands) changed = HarmonyStuff.UpdateSlotIfNeeded(item, Slot.Gloves); // gloves

            if (changed) CCSetup.SmartUpdateOutfit();

        }
    }
}