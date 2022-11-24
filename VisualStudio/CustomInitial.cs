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

namespace CharacterCustomizer
{
    public class CCMain : MelonMod
    {
        public static GameObject[] vanillaMaleHandsMesh = new GameObject[] { };
        public static GameObject[] vanillaFemaleHandsMesh = new GameObject[] { };

        public static GameObject vanillaCharacter;

        public static AssetBundle customArmsBundleAstrid;
        public static AssetBundle customArmsBundleWill;

        public static GameObject allInOnePrefabAstridGO;
        public static GameObject allInOnePrefabWillGO;

        public static GameObject physicsObjectAstrid;

        public static GameObject whateverObject;

        public static bool startLoading;
        public static bool assetLoadComplete;
        public static bool allLoadCompleteAstrid;
        public static bool allLoadCompleteWill;

        public static bool needUpdate;

        public static bool characterForceChanged;

        public static string modsPath;
        public static readonly string modFolderName = "personality";

        public override void OnApplicationStart()
        {
            // Load assets
            customArmsBundleAstrid = AssetBundle.LoadFromFile("Mods/" + modFolderName + "/bundleastrid");
            customArmsBundleWill = AssetBundle.LoadFromFile("Mods/" + modFolderName + "/bundlewill");



            // Get Mods folder path
            modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "\\..\\..\\Mods");

            // Get shaders
            CCSetup.vanillaSkinnedShader = Shader.Find("Shader Forge/TLD_StandardSkinned");
            CCSetup.vanillaDefaultShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

            // Enable settings
            Settings.OnLoad();
        }


        public override void OnSceneWasLoaded(int level, string name)
        {
            if (Utility.IsNonGameScene()) CCSetup.currentCharacter = Character.Undefined; 
        }

        public override void OnSceneWasUnloaded(int level, string name) // started loading scene
        {
            if (!Utility.IsNonGameScene() && startLoading)
            {
                Utility.Log(ConsoleColor.DarkCyan, "Disabling update checks");
                startLoading = false;
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "Deserialize")]
        public class Start
        {
            public static void Postfix()
            {
                Utility.Log(ConsoleColor.Yellow, "PlayerManager.Deserialize");
                if (Utility.IsNonGameScene()) return;
                startLoading = true;
                needUpdate = true;

                if (GameManager.GetPlayerManagerComponent().m_VoicePersona == VoicePersona.Female) Settings.options.selectedCharacter = 0;
                if (GameManager.GetPlayerManagerComponent().m_VoicePersona == VoicePersona.Male) Settings.options.selectedCharacter = 1;



            }
        }


        public static void InitializeHands()
        {
            if (!allInOnePrefabAstridGO)
            {
                allInOnePrefabAstridGO = GameObject.Instantiate(customArmsBundleAstrid.LoadAsset<GameObject>("allInOnePrefabAstrid"));
                allInOnePrefabAstridGO.name = "allInOnePrefabAstrid";
                allInOnePrefabAstridGO.active = false;
                GameObject.DontDestroyOnLoad(allInOnePrefabAstridGO);
            }

            if (!allInOnePrefabWillGO)
            {
                allInOnePrefabWillGO = GameObject.Instantiate(customArmsBundleWill.LoadAsset<GameObject>("allInOnePrefabWill"));
                allInOnePrefabWillGO.name = "allInOnePrefabWill";
                allInOnePrefabWillGO.active = false;
                GameObject.DontDestroyOnLoad(allInOnePrefabWillGO);
            }

            Transform allInOnePrefabAstrid = allInOnePrefabAstridGO.transform;
            Transform allInOnePrefabWill = allInOnePrefabWillGO.transform;

            physicsObjectAstrid = allInOnePrefabAstrid.Find("customPhysics").gameObject;

            //1 - set name | 2 - character enum | 3 - slot enum | 4 - texture name(to look for when using custom) | 5 - GO for normal variant | 6 - GO for maask variant | 7 - GO for injured variant

            Equipment.RegisterEquip("Default", Character.Astrid, Slot.Hands, "AstridArms", allInOnePrefabAstrid.Find("hands").gameObject, allInOnePrefabAstrid.Find("hands_Mask").gameObject, allInOnePrefabAstrid.Find("hands_Bandaged").gameObject);
            Equipment.RegisterEquip("Default", Character.Astrid, Slot.Gloves, "AstridGloves", allInOnePrefabAstrid.Find("gloves").gameObject, null, null);
            Equipment.RegisterEquip("Default", Character.Astrid, Slot.Arms, "AstridArms", allInOnePrefabAstrid.Find("arms").gameObject, null, null);
            Equipment.RegisterEquip("Default", Character.Astrid, Slot.Shirt, "AstridSweater", allInOnePrefabAstrid.Find("sweater").gameObject, allInOnePrefabAstrid.Find("sweater_Mask").gameObject, allInOnePrefabAstrid.Find("sweater_RolledSleeve").gameObject);
            Equipment.RegisterEquip("Default", Character.Astrid, Slot.Jacket, "AstridJacket", allInOnePrefabAstrid.Find("jacket").gameObject, null, null);
            Equipment.RegisterEquip("Default", Character.Astrid, Slot.Trinkets, "AstridTrinkets", null, null, null);
            //Equipment.RegisterEquip("Default", Character.Astrid, ClothingSlot.Trinkets, "AstridTrinkets", allInOnePrefabAstrid.Find("trinkets").gameObject, null, null);

            Equipment.RegisterEquip("Default", Character.Will, Slot.Hands, "WillArms", allInOnePrefabWill.Find("hands").gameObject, allInOnePrefabWill.Find("hands_Mask").gameObject, allInOnePrefabWill.Find("hands_Bandaged").gameObject);
            Equipment.RegisterEquip("Default", Character.Will, Slot.Gloves, "WillGloves", null, null, null);
            Equipment.RegisterEquip("Default", Character.Will, Slot.Arms, "WillArms", allInOnePrefabWill.Find("arms").gameObject, null, null);
            Equipment.RegisterEquip("Default", Character.Will, Slot.Shirt, "WillSweater", allInOnePrefabWill.Find("sweater").gameObject, allInOnePrefabWill.Find("sweater_Mask").gameObject, allInOnePrefabWill.Find("sweater_RolledSleeve").gameObject);
            Equipment.RegisterEquip("Default", Character.Will, Slot.Jacket, "WillJacket", allInOnePrefabWill.Find("jacket").gameObject, null, null);
            Equipment.RegisterEquip("Default", Character.Will, Slot.Trinkets, "WillTrinkets", null, null, null);

            vanillaCharacter = GameManager.GetTopLevelCharacterFpsPlayer();
        }

        public override void OnUpdate()
        {

            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Y))
            {
                bool indoor = GameManager.GetWeatherComponent().IsIndoorEnvironment();
                bool injured = CCChecks.IsInjured();

                HUDMessage.AddMessage("Indoors: " + indoor + " | Injured: " + injured, true, true);

            }

        }

        [HarmonyPatch(typeof(PlayerManager), "Update")]
        public class Update
        {
            public static void Postfix(ref PlayerManager __instance)
            {
                if (!startLoading) return;

                if (GameManager.GetPlayerManagerComponent().m_VoicePersona == VoicePersona.Female && CCSetup.currentCharacter != Character.Astrid)
                {
                    InitializeHands();
                    allLoadCompleteAstrid = false;
                    CCSetup.currentCharacter = Character.Astrid;
                    //characterForceChanged = true;
                    vanillaFemaleHandsMesh = new GameObject[] { };
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, $"F - character updated to {CCSetup.currentCharacter}");
                }

                if (GameManager.GetPlayerManagerComponent().m_VoicePersona == VoicePersona.Male && CCSetup.currentCharacter != Character.Will)
                {
                    InitializeHands();
                    allLoadCompleteWill = false;
                    CCSetup.currentCharacter = Character.Will;
                    //characterForceChanged = true;
                    vanillaMaleHandsMesh = new GameObject[] { };
                    if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkCyan, $"M - character updated to {CCSetup.currentCharacter}");
                }


                if (CCSetup.currentCharacter == Character.Astrid)
                {
                    if (vanillaFemaleHandsMesh.Length == 0) // load vanilla hands before doing anything else
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
                            needUpdate = false;
                            return;
                        }
                        else if (needUpdate) // character change during gameplay via console
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"F - Most operations already done, reloading only custom meshes, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 3);
                            needUpdate = false;
                            return;
                        }
                    }
                }


                if (CCSetup.currentCharacter == Character.Will)
                {
                    if (vanillaMaleHandsMesh.Length == 0)
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
                            needUpdate = false;
                            return;
                        }
                        else if (needUpdate)
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(ConsoleColor.DarkGreen, $"M - Most operations already done, reloading only custom meshes, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 3);
                            needUpdate = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}




