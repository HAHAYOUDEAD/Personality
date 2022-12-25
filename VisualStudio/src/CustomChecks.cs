using MelonLoader;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Il2CppSystem.Reflection;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using Il2Cpp;

namespace Personality
{

    public class CCChecks : MelonMod
    {
        public static bool currentlyIndoors;
        public static bool currentlyInjured;

        private static bool skipItemInHandsCheckOnce;

        public static bool IsInjured()
        {
            if (GameManager.GetSprainPainComponent().HasSprainPain())
            {
                for (int i = 0; i > GameManager.GetSprainPainComponent().GetAfflictionsCount(); i++)
                {
                    if (GameManager.GetSprainPainComponent().GetLocation(i) == AfflictionBodyArea.HandLeft ||
                        GameManager.GetSprainPainComponent().GetLocation(i) == AfflictionBodyArea.HandRight ||
                        GameManager.GetSprainPainComponent().GetLocation(i) == AfflictionBodyArea.ArmLeft ||
                        GameManager.GetSprainPainComponent().GetLocation(i) == AfflictionBodyArea.ArmRight) return true;
                }
            }
            return false;
        }

        private static bool ShouldCheckForOutfitChange()
        {

            if (!CCMain.startLoading) skipItemInHandsCheckOnce = true;

            if (!Settings.options.dynamicOutfit || !GameManager.GetPlayerManagerComponent() || !(CCMain.allLoadCompleteAstrid || CCMain.allLoadCompleteWill)) return false;

            bool noItemInHands = !GameManager.GetPlayerManagerComponent().m_ItemInHands || skipItemInHandsCheckOnce;

            if (!noItemInHands) return false;

            return true;
        }

        public static bool UpdateSlotIfNeeded(GearItem item, Slot slot)
        {
            string itemNameStripped = item?.name.Replace("GEAR_", "");

            Utility.Log(System.ConsoleColor.DarkGray, $"CheckForChangeLayer - checking for change in {slot} for {itemNameStripped}");

            if (item)
            {
                string clothingSet = Equipment.CheckSlotShouldBeChanged(slot, itemNameStripped);

                if (clothingSet != null)
                {
                    Equipment.ChangeEquipIfExistsOtherwiseDefault(slot, clothingSet);
                    Equipment.ChangeEquipVariant(slot, PartVariant.Normal);
                    Utility.Log(System.ConsoleColor.Gray, $"CheckForChangeLayer - {slot} should be changed to {clothingSet}");
                    return true;
                }

                else if (Equipment.currentEquipment[slot] != null && Equipment.currentEquipment[slot].currentVariantEnum == PartVariant.Disabled) // if clothingset is the same, but should be enabled
                {
                    Equipment.ChangeEquipVariant(slot, PartVariant.Normal);
                    Utility.Log(System.ConsoleColor.Gray, $"CheckForChangeLayer - {slot} should be enabled");
                    return true;
                }
            }
            if (!item && Equipment.currentEquipment[slot] != null && Equipment.currentEquipment[slot].currentVariantEnum != PartVariant.Disabled)
            {
                Utility.Log(System.ConsoleColor.Gray, $"CheckForChangeLayer - {slot} should be disabled");
                Equipment.ChangeEquipVariant(slot, PartVariant.Disabled);
                return true;
            }
            return false;
        }



        [HarmonyPatch(typeof(Weather), "IsIndoorEnvironment")] 
        public class DressForOutdoors
        {
            public static void Postfix(ref bool __result)
            {
                if (!ShouldCheckForOutfitChange()) return;

                skipItemInHandsCheckOnce = false;

                if (!__result && CCSetup.currentMeshSet != Outfit.Outdoors) // outdoors
                {
                    Utility.Log(System.ConsoleColor.DarkGreen, "Outdoors check");
                    CCChecks.currentlyIndoors = false;
                    CCSetup.SmartUpdateOutfit();
                    
                    return;
                }

                if (__result && CCSetup.currentMeshSet != Outfit.Indoors && CCSetup.currentMeshSet != Outfit.Injured) // indoors
                {
                    Utility.Log(System.ConsoleColor.DarkGreen, "Indoors check");
                    CCChecks.currentlyIndoors = true;
                    CCSetup.SmartUpdateOutfit();
                    
                    return;
                }
            }
        }


        [HarmonyPatch(typeof(SprainPain), "HasSprainPain")]
        public class ShowBandagesWhenInjured
        {
            public static void Postfix(ref bool __result)
            {
                if (!ShouldCheckForOutfitChange()) return;
                
                if (CCSetup.currentMeshSet == Outfit.Outdoors) return;
               
                skipItemInHandsCheckOnce = false;

                if (__result && CCSetup.currentMeshSet != Outfit.Injured)
                {
                    Utility.Log(System.ConsoleColor.DarkGreen, "Injured check");
                    CCChecks.currentlyInjured = true;
                    CCSetup.SmartUpdateOutfit();

                    return;
                }

                if (!__result && CCSetup.currentMeshSet != Outfit.Indoors) 
                {
                    Utility.Log(System.ConsoleColor.DarkGreen, "Not-injured check");
                    CCChecks.currentlyInjured = false;
                    CCSetup.SmartUpdateOutfit();

                    return;
                }

                if (__result && CCSetup.currentMeshSet != Outfit.Indoors && CCSetup.currentMeshSet != Outfit.Injured) // indoors
                {
                    Utility.Log(System.ConsoleColor.DarkGreen, "Indoors check");
                    CCChecks.currentlyIndoors = true;
                    CCSetup.SmartUpdateOutfit();

                    return;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_Clothing), "Enable")] // can be optipized by triggering when closing menu altogether, not switching tabs
        public class UpdateSlotsOnMenuClose
        {
            public static bool visited;

            public static void Postfix(bool enable, ref Panel_Clothing __instance)
            {
                if (enable && !__instance.m_ShowPaperDollOnly) visited = true;

                if (!enable && visited)
                {
                    CCSetup.UpdateClothingSlots();
                    visited = false;
                }
            }
        }

        [HarmonyPatch(typeof(ConsoleManager), "CONSOLE_voice_female")] // disable commands
        public class DisableVoiceFemale
        {
            public static bool Prefix()
            {
                Debug.Log("Please use Personality Mod settings instead!");

                return false;
            }
        }

        [HarmonyPatch(typeof(ConsoleManager), "CONSOLE_voice_male")] // disable commands
        public class DisableVoiceMale
        {
            public static bool Prefix()
            {
                Debug.Log("Please use Personality Mod settings instead!");

                return false;
            }
        }


    }
}