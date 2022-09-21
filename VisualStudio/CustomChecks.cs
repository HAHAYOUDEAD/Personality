using MelonLoader;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Il2CppSystem.Reflection;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

namespace CharacterCustomizer
{

    public class CCChecks : MelonMod
    {
        public static string currentLocation = "none";
        public static bool currentlyInjured;


        public static bool DEBUG_FORCE_INJURED;
        public static bool DEBUG_FORCE_INDOORS;

        public static bool IsIndoors() => GameManager.GetWeatherComponent().IsIndoorEnvironment();

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

        public static void CheckIndoors()
        {
            if (CCMain.allLoadCompleteAstrid || CCMain.allLoadCompleteWill)
            {
                if (!Settings.options.undressIndoors) return;

                if (GameManager.GetPlayerManagerComponent().m_ItemInHands != null) return;

                MelonLogger.Msg("checking indoors ");

                if (currentLocation != "indoors" && (IsIndoors() || DEBUG_FORCE_INDOORS))
                {
                    currentLocation = "indoors";
                    CCSetup.SmartUpdateOutfit();
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, "Indoors check");
                    return;
                }

                if (currentLocation != "outdoors" && !IsIndoors())
                {
                    currentLocation = "outdoors";
                    CCSetup.SmartUpdateOutfit();
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, "Outdoors check");
                    return;
                }
            }

        }

        public static void CheckInjured()
        {
            if (CCMain.allLoadCompleteAstrid || CCMain.allLoadCompleteWill)
            {
                if (!Settings.options.showInjuries) return;

                if (!currentlyInjured && (IsInjured() || DEBUG_FORCE_INJURED))
                {
                    currentlyInjured = true;
                    CCSetup.SmartUpdateOutfit();
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkRed, "Injured check");
                    return;
                }

                if (currentlyInjured && !IsInjured())
                {
                    currentlyInjured = false;
                    CCSetup.SmartUpdateOutfit();
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkRed, "Not injured check");
                    return;
                }

            }
        }

    }
}