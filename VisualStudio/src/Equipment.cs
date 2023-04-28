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
            { Slot.Trinkets, null },
            { Slot.Bandages, null }
        };

        public static void RegisterEquip(string clothingSet, Character character, Slot slot, string textureName, GameObject normal, GameObject undermask, GameObject injured, SpecialFlag specialFlag = SpecialFlag.None)
        {
            Equip newEquip = new Equip()
            {
                clothingSet = clothingSet,
                textureName = textureName,
                slot = slot,
                normalVariantPrefab = normal,
                maskedVariantPrefab = undermask,
                injuredVariantPrefab = injured,
                specialFlag = specialFlag
            };

            if (!allEquipment[character].ContainsKey(clothingSet))
            {
                allEquipment[character].Add(clothingSet, new Dictionary<Slot, Equip>());
            }

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

            Utility.Log(System.ConsoleColor.DarkGray, $"CheckSlotShouldBeChanged - current clothing set in {slot}: {currentEquipment[slot].clothingSet} = {clothingSet}");

            if (currentEquipment[slot].clothingSet != clothingSet)
            {
                Utility.Log(System.ConsoleColor.DarkGray, $"CheckSlotShouldBeChanged - current clothing set in {slot}: {currentEquipment[slot].clothingSet} != {clothingSet}");
                if (GetEquipForSlot(slot, clothingSet) != null)
                {
                    return clothingSet;
                }
                Utility.Log(System.ConsoleColor.DarkGray, $"CheckSlotShouldBeChanged - Equip for slot {slot} doesn't exist");
            }
            else
            {
                return null;
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

        public static void ChangeEquipVariant(Slot slot, PartVariant variant, PartVariant rightHandVariant = PartVariant.Undefined)
        {
            currentEquipment[slot]?.SwapVariant(variant, rightHandVariant);
            
            //MelonLogger.Msg(System.ConsoleColor.Blue, "slot: " + slot + " item: " + item);
        }

        public static void ChangeEquipVariant(PartVariant gloves, PartVariant shirt, PartVariant jacket)//, PartVariant trinkets)
        {
            if (Settings.options.displayProperClothes) // check if equipped
            {
                Utility.Log(ConsoleColor.Blue, "Gloves should be: " + gloves + " | Shirt should be: " + shirt + " | Jacket should be: " + jacket);
                Utility.Log(ConsoleColor.Red, "Gloves equipped: " + currentEquipment[Slot.Gloves]?.isActuallyEquipped + " | Shirt equipped: " + currentEquipment[Slot.Shirt]?.isActuallyEquipped + " | Jacket equipped: " + currentEquipment[Slot.Jacket]?.isActuallyEquipped);
                if (currentEquipment[Slot.Gloves]?.isActuallyEquipped == true) currentEquipment[Slot.Gloves]?.SwapVariant(gloves);
                if (currentEquipment[Slot.Shirt]?.isActuallyEquipped == true) currentEquipment[Slot.Shirt]?.SwapVariant(shirt);
                if (currentEquipment[Slot.Jacket]?.isActuallyEquipped == true) currentEquipment[Slot.Jacket]?.SwapVariant(jacket);
            }
            else
            {
                CCSetup.SetClothesToDefault();
                currentEquipment[Slot.Gloves]?.SwapVariant(gloves);
                currentEquipment[Slot.Shirt]?.SwapVariant(shirt);
                currentEquipment[Slot.Jacket]?.SwapVariant(jacket);
            }
        }

        public static void SetActuallyEquipped(Slot slot, bool yes) => currentEquipment[slot].isActuallyEquipped = yes;

        public static void ToggleTrinkets(bool enable)
        {
            ChangeEquipVariant(Slot.Trinkets, enable ? PartVariant.Normal : PartVariant.Disabled);
        }

        public static void ToggleBandages(bool both, bool? right = null)
        {
            if (both) 
            {
                if (right == false)
                {
                    currentEquipment[Slot.Bandages].SwapVariant(PartVariant.Normal, PartVariant.Disabled);
                }
                else
                {
                    currentEquipment[Slot.Bandages].SwapVariant(PartVariant.Normal);
                }
            }
            else
            {
                if (right == true)
                {
                    currentEquipment[Slot.Bandages].SwapVariant(PartVariant.Disabled, PartVariant.Normal);
                }
                else
                {
                    currentEquipment[Slot.Bandages].SwapVariant(PartVariant.Disabled);
                }
            }
        }


    }
    
    public class Equip // one particular clothing
    {
        public string clothingSet;

        public Slot slot;

        public string textureName;

        public GameObject normalVariantPrefab = null;
        public GameObject maskedVariantPrefab = null;
        public GameObject injuredVariantPrefab = null;

        public GameObject normalVariant = null;
        public GameObject maskedVariant = null;
        public GameObject injuredVariant = null;

        public GameObject currentVariant = null;
        public PartVariant currentVariantEnum = PartVariant.Undefined;

        public GameObject rightHandVariant = null;
        public PartVariant rightHandVariantEnum = PartVariant.Undefined;

        public SpecialFlag specialFlag = SpecialFlag.None;

        public bool isActuallyEquipped;

        public void ResetBothHands(GameObject variant)
        {
            if (variant != null)
            {
                foreach (GameObject child in variant.GetAllImmediateChildren())
                {
                    child.active = true;
                }
                variant.active = false;
            }
        }


        public void SwapVariant(PartVariant e, PartVariant r = PartVariant.Undefined)
        {
            ResetBothHands(currentVariant);
            ResetBothHands(rightHandVariant);

            if (r == e) r = PartVariant.Undefined;

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
            else if (e == PartVariant.Masked)
            {
                currentVariant = maskedVariant;
            }
            currentVariantEnum = e;
            if (currentVariant) currentVariant.active = true;


            if (r != PartVariant.Undefined)
            {
                if (r == PartVariant.Disabled)
                {
                    rightHandVariant = null;
                }
                else if (r == PartVariant.Normal)
                {
                    rightHandVariant = normalVariant;
                }
                else if (r == PartVariant.Injured)
                {
                    rightHandVariant = injuredVariant;
                }
                else if (r == PartVariant.Masked)
                {
                    rightHandVariant = maskedVariant;
                }
                rightHandVariantEnum = r;

                if (rightHandVariant != null)
                {
                    rightHandVariant.active = true;

                    foreach (GameObject child in rightHandVariant.GetAllImmediateChildren())
                    {
                        if (child.name.EndsWith("_L")) child.active = false;
                    }
                }

                if (currentVariant != null)
                {
                    foreach (GameObject child in currentVariant.GetAllImmediateChildren())
                    {
                        if (child.name.EndsWith("_R")) child.active = false;
                    }
                }
            }
        }
    }
}