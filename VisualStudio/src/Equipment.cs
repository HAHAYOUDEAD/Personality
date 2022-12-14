using MelonLoader;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Il2CppSystem.Reflection;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using Il2Cpp;

namespace Personality
{

    public static class Equipment // all equipment
    {
        // allEquipment[character][clothingSet][slot] == Equip
        public static Dictionary<Character, Dictionary<string, Dictionary<Slot, Equip>>> allEquipment = new Dictionary<Character, Dictionary<string, Dictionary<Slot, Equip>>>()
        {
            // creating empty dictionary with "Default" clothing set
            { Character.Astrid, new Dictionary<string, Dictionary<Slot, Equip>>() { { "Default", new Dictionary<Slot, Equip>() } } },
            { Character.Will, new Dictionary<string, Dictionary<Slot, Equip>>() { { "Default", new Dictionary<Slot, Equip>() } } },
            { Character.Marcene, new Dictionary<string, Dictionary<Slot, Equip>>() { { "Default", new Dictionary<Slot, Equip>() } } },
            { Character.Lincoln, new Dictionary<string, Dictionary<Slot, Equip>>() { { "Default", new Dictionary<Slot, Equip>() } } }
        };

        public static Dictionary<Slot, Equip> currentEquipment = new Dictionary<Slot, Equip>()
        {
            { Slot.Hands, null },
            { Slot.Gloves, null },
            { Slot.Arms, null },
            { Slot.Shirt, null },
            { Slot.Jacket, null },
            { Slot.Trinkets, null }
        };

        public static void RegisterEquip(string clothingSet, Character character, Slot slot, string textureName, GameObject normal, GameObject undermask, GameObject injured, SpecialFlag specialFlag = SpecialFlag.None)
        {
            Equip newEquip = new Equip()
            {
                clothingSet = clothingSet,
                textureName = textureName,
                slot = slot,
                normalVariantPrefab = normal,
                undermaskVariantPrefab = undermask,
                injuredVariantPrefab = injured,
                specialFlag = specialFlag
            };

            allEquipment[character][clothingSet][slot] = newEquip;
        }

        public static Dictionary<Slot, Equip> GetClothingSet(string clothingSet)
        {
            allEquipment[CCSetup.currentCharacter].TryGetValue(clothingSet, out Dictionary<Slot, Equip> equips);

            return equips;
        }

        public static Equip GetEquipForSlot(Slot slot, string clothingSet)
        {
            Equip equip = null;

            if (GetClothingSet(clothingSet) != null)
            {
                allEquipment[CCSetup.currentCharacter][clothingSet].TryGetValue(slot, out equip);

            }
            return equip;
        }

        public static string CheckSlotShouldBeChanged(Slot slot, string clothingSet)
        {
            if (currentEquipment[slot] == null) return null;

            if (currentEquipment[slot].clothingSet != clothingSet)
            {
                if (GetEquipForSlot(slot, clothingSet) != null)
                {
                    return clothingSet;
                }
            }

            if (currentEquipment[slot].clothingSet != "Default")
            {
                return "Default";
            }

            return null;
        }

        public static void ChangeEquipIfExistsOtherwiseDefault(Slot slot, string clothingSet)
        {
            Utility.Log(System.ConsoleColor.Gray, "ChangeEquipIfExistsOtherwiseDefault - Start");

            Equip equip = GetEquipForSlot(slot, clothingSet);

            if (equip == null)
            {
                GetEquipForSlot(slot, "Default");
                Utility.Log(System.ConsoleColor.DarkGray, $"ChangeEquipIfExistsOtherwiseDefault - couldn't find available slot {slot} in {clothingSet}, checking 'Default'");
            }

            PartVariant prevVariant = PartVariant.Normal;

            if (currentEquipment[slot] != null)
            {
                if (currentEquipment[slot].currentVariantEnum != PartVariant.Undefined || currentEquipment[slot].currentVariantEnum != PartVariant.Disabled)
                {
                    prevVariant = currentEquipment[slot].currentVariantEnum;
                    currentEquipment[slot].SwapVariant(PartVariant.Disabled);
                }
            }

            if (equip != null) equip.SwapVariant(prevVariant);
            else Utility.Log(System.ConsoleColor.DarkGray, $"ChangeEquipIfExistsOtherwiseDefault - couldn't find available slot {slot}");

            currentEquipment[slot] = equip;
            Utility.Log(System.ConsoleColor.DarkYellow, "ChangeEquipIfExistsOtherwiseDefault - Done");
        }

        public static void ChangeEquipVariant(Slot slot, PartVariant variant)
        {
            currentEquipment[slot]?.SwapVariant(variant);
        }

        public static void ChangeEquipVariant(PartVariant gloves, PartVariant shirt, PartVariant jacket)//, PartVariant trinkets)
        {
            //currentEquipment[Slot.Hands]?.SwapVariant(hands);
            currentEquipment[Slot.Gloves]?.SwapVariant(gloves);
            //currentEquipment[Slot.Arms]?.SwapVariant(arms);
            currentEquipment[Slot.Shirt]?.SwapVariant(shirt);
            currentEquipment[Slot.Jacket]?.SwapVariant(jacket);
            //currentEquipment[ClothingSlot.Trinkets]?.SwapVariant(trinkets);
        }

        public static void ToggleTrinkets(bool enable)
        {
            ChangeEquipVariant(Slot.Trinkets, enable ? PartVariant.Normal : PartVariant.Disabled);
        }
    }
    
    public class Equip // one particular clothing
    {
        public string clothingSet;

        public Slot slot;

        public string textureName;

        public GameObject normalVariantPrefab = null;
        public GameObject undermaskVariantPrefab = null;
        public GameObject injuredVariantPrefab = null;

        public GameObject normalVariant = null;
        public GameObject undermaskVariant = null;
        public GameObject injuredVariant = null;

        public GameObject currentVariant = null;
        public PartVariant currentVariantEnum = PartVariant.Undefined;

        public SpecialFlag specialFlag = SpecialFlag.None;


        public void SwapVariant(PartVariant e)
        {
            if (currentVariant) currentVariant.active = false;
            if (e == PartVariant.Disabled)
            {
                currentVariant = null;
            }
            else if (e == PartVariant.Normal)
            {
                currentVariant = normalVariant;
            }
            else if (e == PartVariant.Injured)
            {
                currentVariant = injuredVariant;
            }
            else if (e == PartVariant.Undermask)
            {
                currentVariant = undermaskVariant;
            }
            currentVariantEnum = e;
            if (currentVariant) currentVariant.active = true;
        }
    }

}