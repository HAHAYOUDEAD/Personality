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
    public class CCMain : MelonMod
    {
        public static GameObject vanillaMaleHandsMesh;
        public static GameObject vanillaFemaleHandsMesh;

        public static GameObject vanillaCharacter;

        public static AssetBundle customArmsBundleAstrid;
        public static AssetBundle customArmsBundleWill;

        public static GameObject physicsObjectAstrid;

        public static bool startLoading;
        public static bool assetLoadComplete;
        public static bool allLoadCompleteAstrid;
        public static bool allLoadCompleteWill;

        public static bool characterForceChanged;

        public static string modsPath;

        public override void OnApplicationStart()
        {
            // Load assets
            customArmsBundleAstrid = AssetBundle.LoadFromFile("Mods/characterCustomizer/bundleastrid");
            //customArmsBundleWill = AssetBundle.LoadFromFile("Mods/characterCustomizer/bundlewill");

            // Get Mods folder path
            modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "\\..\\..\\Mods");

            // Get shaders
            CCSetup.vanillaSkinnedShader = Shader.Find("Shader Forge/TLD_StandardSkinned");
            CCSetup.vanillaDefaultShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

            // Enable settings
            Settings.OnLoad();
        }

        public override void OnSceneWasLoaded(int level, string name) // started loading scene
        {
            if (level >= 3 && startLoading)
            {
                if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, "Disabling update checks");
                startLoading = false;
            }
        }

        public override void OnSceneWasInitialized(int level, string name) // finished loading scene
        {
            if (level >= 3)
            {
                if (!assetLoadComplete)
                {
                    physicsObjectAstrid = customArmsBundleAstrid.LoadAsset<GameObject>("allInOnePrefabAstrid").transform.Find("customPhysics").gameObject;

                    Transform allInOnePrefabAstrid = customArmsBundleAstrid.LoadAsset<GameObject>("allInOnePrefabAstrid").transform;

                    Equipment.RegisterEquip("Default", Character.Astrid, Slot.Hands, "AstridArms", allInOnePrefabAstrid.Find("hands").gameObject, allInOnePrefabAstrid.Find("hands_Mask").gameObject, allInOnePrefabAstrid.Find("hands_Bandaged").gameObject);
                    Equipment.RegisterEquip("Default", Character.Astrid, Slot.Gloves, "AstridGloves", allInOnePrefabAstrid.Find("gloves").gameObject, null, null);
                    Equipment.RegisterEquip("Default", Character.Astrid, Slot.Arms, "AstridArms", allInOnePrefabAstrid.Find("arms").gameObject, null, null);
                    Equipment.RegisterEquip("Default", Character.Astrid, Slot.Shirt, "AstridSweater", allInOnePrefabAstrid.Find("sweater").gameObject, allInOnePrefabAstrid.Find("sweater_Mask").gameObject, allInOnePrefabAstrid.Find("sweater_RolledSleeve").gameObject);
                    Equipment.RegisterEquip("Default", Character.Astrid, Slot.Jacket, "AstridJacket", allInOnePrefabAstrid.Find("jacket").gameObject, null, null);
                    Equipment.RegisterEquip("Default", Character.Astrid, Slot.Trinkets, "AstridTrinkets", null, null, null);
                    //Equipment.RegisterEquip("Default", Character.Astrid, ClothingSlot.Trinkets, "AstridTrinkets", allInOnePrefabAstrid.Find("trinkets").gameObject, null, null);


                    vanillaCharacter = GameManager.GetTopLevelCharacterFpsPlayer();

                    assetLoadComplete = true;

                    //MelonCoroutines.Start(Utility.InvokeRepeating(CCChecks.CheckIndoors, true));
                    //MelonCoroutines.Start(Utility.InvokeRepeating(CCChecks.CheckInjured, true));

                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, "Asset load complete");
                }

                if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, "Enabling update checks");
                startLoading = true;
            }
            else // if (name == "MainMenu") //\\\<breaks when changing scene?
            {
                if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, "Went through loading screen, resetting");
                startLoading = false;
                assetLoadComplete = false;
                allLoadCompleteAstrid = false;
                allLoadCompleteWill = false;
            }
        }

        public override void OnSceneWasUnloaded(int level, string name) // unloading scene
        {
            if (level >= 3 && startLoading)
            {
                if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, "Disabling update checks");
                startLoading = false;
            } 
        }

        





        public override void OnUpdate()
        {
            if (startLoading)
            {
                if (GameManager.GetPlayerManagerComponent().m_VoicePersona == VoicePersona.Female && CCSetup.currentCharacter != Character.Astrid)
                {
                    CCSetup.currentCharacter = Character.Astrid; 
                    characterForceChanged = true;
                    vanillaFemaleHandsMesh = null;
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, $"F - character updated to {CCSetup.currentCharacter}");
                }

                if (GameManager.GetPlayerManagerComponent().m_VoicePersona == VoicePersona.Male && CCSetup.currentCharacter != Character.Will)
                {
                    CCSetup.currentCharacter = Character.Will;
                    characterForceChanged = true;
                    vanillaMaleHandsMesh = null;
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, $"M - character updated to {CCSetup.currentCharacter}");
                }


                if (CCSetup.currentCharacter == Character.Astrid)
                {
                    if (!vanillaFemaleHandsMesh) // load vanilla hands before doing anything else
                    {
                        if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"F - Vanilla hands not loaded yet, loading, character: {CCSetup.currentCharacter}");
                        CCSetup.DoEverything(CCSetup.currentCharacter, 0, 1);
                        
                    }
                    else // hands loaded, continue
                    {
                        if (!allLoadCompleteAstrid) // initial load, also if quit to menu
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"F - Vanilla hands loaded, doing everything else, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 5);
                            characterForceChanged = false;
                            return;
                        }
                        else if (characterForceChanged) // character change during gameplay via console
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"F - Most operations already done, reloading only custom meshes, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 2);
                            characterForceChanged = false;
                            return;
                        }
                    }
                }


                if (CCSetup.currentCharacter == Character.Will)
                {
                    if (!vanillaMaleHandsMesh)
                    {
                        if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"M - Vanilla hands not loaded yet, loading, character: {CCSetup.currentCharacter}");
                        CCSetup.DoEverything(CCSetup.currentCharacter, 0, 1);
                        
                    }
                    else
                    {
                        if (!allLoadCompleteWill)
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"M - Vanilla hands loaded, doing everything else, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 5);
                            characterForceChanged = false;
                            return;
                        }
                        else if (characterForceChanged)
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"M - Most operations already done, reloading only custom meshes, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 2);
                            characterForceChanged = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}




