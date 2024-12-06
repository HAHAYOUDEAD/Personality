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

        //private static bool debugOverrideIndoors;
        //private static bool debugOverrideInjured;

        public static bool skipItemInHandsCheckForIndoors;
        public static bool skipItemInHandsCheckForInjured;

        public static bool shouldDoOutfitUpdate;

        //public static void DebugSetInjured(bool set) => debugOverrideInjured = set;
        //public static void DebugSetIndoors(bool set) => debugOverrideIndoors = set;

        /*
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
        */
        /*
        private static bool ShouldCheckForOutfitChange()
        {
            // skip check when just loaded a level
            //if (GameManager.GetPlayerManagerComponent().m_ItemInHands && !skipItemInHandsCheckOnce) skipItemInHandsCheckOnce = true;
            //if (!CCMain.startLoading) skipItemInHandsCheckOnce = false;

            if (Settings.options.specialEventOverride) return false;

            if (!CCMain.characterIsLoaded) return false;

            if (!Settings.options.dynamicOutfit || !GameManager.GetPlayerManagerComponent() || !(CCMain.allLoadCompleteAstrid || CCMain.allLoadCompleteWill)) return false; // || Settings.options.displayProperClothes

            bool noItemInHands = !GameManager.GetPlayerManagerComponent().m_ItemInHands || skipItemInHandsCheckForInjured || skipItemInHandsCheckForIndoors;

            if (!noItemInHands) return false;

            return true;
        }
        */
        /*
        public static bool UpdateSlotIfNeeded(GearItem item, Slot slot)
        {
            string itemNameStripped = item?.name.Replace("GEAR_", "");

            Utility.Log(System.ConsoleColor.DarkGray, $"CheckForChangeLayer - checking for change in {slot} for {itemNameStripped}");

            //Equipment.ActuallyEquipped(slot, item != null);

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

                else if (Equipment.currentEquipment[slot] != null && Equipment.currentEquipment[slot].currentVariantEnum != PartVariant.Normal) // if clothingset is the same, but should be enabled
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
        */



        /*
        [HarmonyPatch(typeof(Panel_Clothing), "Enable")] // can be optipized by triggering when closing menu altogether, not switching tabs
        public class UpdateSlotsOnMenuClose
        {
            public static bool visited;

            public static void Postfix(bool enable, ref Panel_Clothing __instance)
            {
                if (!Settings.options.displayProperClothes) return;

                if (enable && !__instance.m_ShowPaperDollOnly) visited = true;

                if (!enable && visited)
                {
                    CCSetup.UpdateClothingSlots();
                    visited = false;
                }
            }
        }
        */
        /*
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
        */

    }
}