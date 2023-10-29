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
using Il2CppTLD;
using Il2Cpp;

namespace Personality
{
    public class ClothingInfo
    {
        public string? bundleName;
        public Slot clothingSlot;
        public bool hasInjured;
        public bool hasMasked;
        public SpecialFlag specialFlag;

        public static ClothingInfo Jacket(string? bn = null, SpecialFlag sf = SpecialFlag.None) => new ClothingInfo() 
        { 
            bundleName = bn, 
            clothingSlot = Slot.Jacket, 
            hasInjured = false, 
            hasMasked = true, 
            specialFlag = sf 
        };

        public static ClothingInfo Shirt(string? bn = null, SpecialFlag sf = SpecialFlag.None) => new ClothingInfo()
        {
            bundleName = bn,
            clothingSlot = Slot.Shirt,
            hasInjured = true,
            hasMasked = true,
            specialFlag = sf
        };

        public static ClothingInfo Gloves(string? bn = null, SpecialFlag sf = SpecialFlag.None) => new ClothingInfo()
        {
            bundleName = bn,
            clothingSlot = Slot.Gloves,
            hasInjured = true, // right hand dangle
            hasMasked = true, // right hand off
            specialFlag = sf
        };
    }


    public class CCMain : MelonMod
    {
        public static GameObject[] vanillaMaleHandsMesh = new GameObject[] { };
        public static GameObject[] vanillaFemaleHandsMesh = new GameObject[] { };

        public static GameObject vanillaCharacter;
        public static Chirality characterChirality;

        public static AssetBundle customArmsBundleAstrid;
        public static AssetBundle customArmsBundleWill;
        public static AssetBundle customPhysicsBundle;
        public static AssetBundle customTrinketsBundle;
        public static AssetBundle customEventsBundle;

        public static GameObject physicsObject;
        public static GameObject trinketsObject;
        public static GameObject allInOnePrefabAstridGO;
        public static GameObject allInOnePrefabWillGO;

        public static Dictionary<string, AssetBundle> clothingBundles = new();

        public static readonly string cb_handmade = "pack_handmade";
        public static readonly string cb_events = "pack_events";

        public static Dictionary<string, ClothingInfo> clothingData = new()
        {
            //BallisticVest
            //NewsprintBootStuffing
            //NewsprintInsulation

            { "BearSkinCoat", ClothingInfo.Jacket(cb_handmade) }, //Bearskin Coat
            { "MooseHideCloak", ClothingInfo.Jacket(cb_handmade, SpecialFlag.LargeJacket) }, //Moose-Hide Cloak
            { "WolfSkinCape", ClothingInfo.Jacket(cb_handmade, SpecialFlag.LargeJacket) }, //Wolfskin Coat
            { "RabbitSkinMittens", ClothingInfo.Gloves(cb_handmade, SpecialFlag.LargeMittens) }, //Rabbitskin Mitts

            { "BasicWinterCoat", ClothingInfo.Jacket() }, //Windbreaker
            { "DownParka", ClothingInfo.Jacket() }, //Urban Parka
            { "DownSkiJacket", ClothingInfo.Jacket() }, //Ski Jacket
            { "DownVest", ClothingInfo.Jacket(null, SpecialFlag.SleevelessJacket) }, //Down Vest
            { "HeavyParka", ClothingInfo.Jacket() }, //Old Fashioned Parka
            { "InsulatedVest", ClothingInfo.Jacket(null, SpecialFlag.SleevelessJacket) }, //Sport Vest
            { "LightParka", ClothingInfo.Jacket() }, //Simple Parka
            { "MackinawJacket", ClothingInfo.Jacket() }, //Mackinaw Jacket
            { "MilitaryParka", ClothingInfo.Jacket() }, //Military Coat
            { "PremiumWinterCoat", ClothingInfo.Jacket() }, //Expedition Parka
            { "QualityWinterCoat", ClothingInfo.Jacket() }, //Mariner's Pea Coat
            { "SkiJacket", ClothingInfo.Jacket() }, //Light Shell

            { "CottonHoodie", ClothingInfo.Shirt() }, //Hoodie
            { "CottonShirt", ClothingInfo.Shirt() }, //Dress Shirt
            { "CowichanSweater", ClothingInfo.Shirt() }, //Cowichan Sweater
            { "FishermanSweater", ClothingInfo.Shirt() }, //Fisherman's Sweater
            { "FleeceSweater", ClothingInfo.Shirt() }, //Sweatshirt
            { "HeavyWoolSweater", ClothingInfo.Shirt() }, //Thick Wool Sweater
            { "PlaidShirt", ClothingInfo.Shirt() }, //Plaid Shirt (red one)
            { "TeeShirt", ClothingInfo.Shirt(null, SpecialFlag.Teeshirt) }, //T-Shirt
            { "WoolShirt", ClothingInfo.Shirt() }, //Wool Shirt (green one)
            { "WoolSweater", ClothingInfo.Shirt() }, //Thin Wool Sweater

            { "BasicGloves", ClothingInfo.Gloves() }, //Driving Gloves
            { "FleeceMittens", ClothingInfo.Gloves(null, SpecialFlag.Mittens) }, //Fleece Mittens (red ones)
            { "Gauntlets", ClothingInfo.Gloves(null, SpecialFlag.LargeGloves) }, //Gauntlets
            { "ImprovisedMittens", ClothingInfo.Gloves(null, SpecialFlag.Mittens) }, //Improvised Hand Wraps
            { "Mittens", ClothingInfo.Gloves(null, SpecialFlag.Mittens) }, //Wool Mittens (blue ones)
            { "SkiGloves", ClothingInfo.Gloves(null, SpecialFlag.LargeGloves) }, //Ski Gloves
            { "WorkGloves", ClothingInfo.Gloves(null, SpecialFlag.LargeGloves) } //Work Gloves
        };



        public static bool startLoading;
        public static bool assetLoadComplete;
        public static bool allLoadCompleteAstrid;
        public static bool allLoadCompleteWill;
        private static bool alreadyStarted;

        public static bool needVariableUpdate;

        public static bool characterIsLoaded;

        public static object mainCoroutine;

        public static string modsPath;
        public static readonly string modFolderName = "personality/";

        public override void OnInitializeMelon()
        {
            // Load assets
            string path = "Mods/" + modFolderName;

            customArmsBundleAstrid = AssetBundle.LoadFromFile(path + "bundleastrid");
            customArmsBundleWill = AssetBundle.LoadFromFile(path + "bundlewill");
            customPhysicsBundle = AssetBundle.LoadFromFile(path + "customphysics");
            customTrinketsBundle = AssetBundle.LoadFromFile(path + "trinkets");
            

            path += "clothingBundles/";
            clothingBundles.Add(cb_handmade, AssetBundle.LoadFromFile(path + cb_handmade));
            customEventsBundle = AssetBundle.LoadFromFile(path + cb_events);


            // Get Mods folder path
            modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "\\..\\..\\Mods");

            // Get shaders
            CCSetup.vanillaSkinnedShader = Shader.Find("Shader Forge/TLD_StandardSkinned");
            CCSetup.vanillaDefaultShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

            // Enable settings
            Settings.OnLoad();

            if (Utility.IsAssemblyPresent("TLDHalloween"))
            {
                MelonLogger.Msg(System.ConsoleColor.Blue, "Howl-oween detected! Enabling Personality override!");
                Settings.options.specialEventOverride = true;
                Settings.options.specialEventOutfit = 0;

                Settings.options.Save();
                Settings.HideEverythingWhenSpecialOverride(true);
            }


        }


        public override void OnSceneWasLoaded(int level, string name)
        {
            if (!Utility.IsScenePlayable(name))
            {
                CCSetup.currentCharacter = Character.Undefined;
            }
            else
            {
                startLoading = true;
            }

            if (Utility.IsMainMenu(name))
            {
                needVariableUpdate = true;
            }
            
        }

        public override void OnSceneWasInitialized(int level, string name)
        {
            CCChecks.skipItemInHandsCheckForIndoors = true;
            CCChecks.skipItemInHandsCheckForInjured = true;
        }

        public override void OnSceneWasUnloaded(int level, string name)
        {
            if (Utility.IsScenePlayable(name) && startLoading)
            {
                startLoading = false;
                characterIsLoaded = false;
            }
        }


        public static void InitializeHands()
        {
            string prefix;

            //1 - set name | 2 - character enum | 3 - slot enum | 4 - texture name(to look for when using custom) | 5 - GO for normal variant | 6 - GO for maask variant | 7 - GO for injured variant

            if (!physicsObject)
            {
                physicsObject = LoadAndSetupFromBundle(customPhysicsBundle, "customPhysics");
            }

            if (!trinketsObject)
            {
                trinketsObject = LoadAndSetupFromBundle(customTrinketsBundle, "trinkets");
            }

            if (!allInOnePrefabAstridGO)
            {
                allInOnePrefabAstridGO = LoadAndSetupFromBundle(customArmsBundleAstrid, "allInOnePrefabAstridSplit");

                Transform allInOnePrefabAstrid = allInOnePrefabAstridGO.transform;

                prefix = "astrid_";
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Hands, "AstridArms", allInOnePrefabAstrid.Find(prefix + "hands").gameObject, allInOnePrefabAstrid.Find(prefix + "hands_Mask").gameObject, null);
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Gloves, "AstridGloves", allInOnePrefabAstrid.Find(prefix + "gloves").gameObject, null, null);
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Arms, "AstridArms", allInOnePrefabAstrid.Find(prefix + "arms").gameObject, allInOnePrefabAstrid.Find(prefix + "arms_Mask").gameObject, null); 
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Shirt, "AstridSweater", allInOnePrefabAstrid.Find(prefix + "sweater").gameObject, allInOnePrefabAstrid.Find(prefix + "sweater_Mask").gameObject, allInOnePrefabAstrid.Find(prefix + "sweater_RolledSleeve").gameObject);
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Jacket, "AstridJacket", allInOnePrefabAstrid.Find(prefix + "jacket").gameObject, allInOnePrefabAstrid.Find(prefix + "jacket_Mask").gameObject, null);
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Trinkets, "AstridTrinkets", trinketsObject.transform.Find(prefix + "trinkets").gameObject, null, null);
                Equipment.RegisterEquip("Default", Character.Astrid, Slot.Bandages, "AstridArms", allInOnePrefabAstrid.Find(prefix + "bandages").gameObject, null, null);


                foreach (KeyValuePair<string, ClothingInfo> e in clothingData)
                {
                    if (e.Value.bundleName != null)
                    {
                        GameObject prefab = LoadAndSetupFromBundle(clothingBundles[e.Value.bundleName]);
                        GameObject? normalVariant = prefab.transform.Find(prefix + e.Key)?.gameObject;
                        GameObject? maskedVariant = e.Value.hasMasked ? prefab.transform.Find(prefix + e.Key + "_Mask")?.gameObject : null;
                        GameObject? injuredVariant = e.Value.hasInjured ? prefab.transform.Find(prefix + e.Key + "_RolledSleeve")?.gameObject : null;
                        //if (injuredVariant == null) injuredVariant = e.Value.hasInjured ? prefab.transform.Find(prefix + e.Key + "_Stringed")?.gameObject : null;
                        Equipment.RegisterEquip(e.Key, Character.Astrid, e.Value.clothingSlot, "", normalVariant, maskedVariant, injuredVariant, e.Value.specialFlag);
                    }
                }

                Equipment.RegisterEquip("Event-Halloween2023", Character.Astrid, Slot.Shirt, "", LoadAndSetupFromBundle(customEventsBundle).transform.Find(prefix + "Halloween2023").gameObject, null, null);

                Utility.Log(System.ConsoleColor.Yellow, "InitializeHands - Astrid - done");
            }

            if (!allInOnePrefabWillGO)
            {
                allInOnePrefabWillGO = LoadAndSetupFromBundle(customArmsBundleWill, "allInOnePrefabWillSplit");;

                Transform allInOnePrefabWill = allInOnePrefabWillGO.transform;

                prefix = "will_";
                Equipment.RegisterEquip("Default", Character.Will, Slot.Hands, "WillArms", allInOnePrefabWill.Find(prefix + "hands").gameObject, allInOnePrefabWill.Find(prefix + "hands_Mask").gameObject, null);
                Equipment.RegisterEquip("Default", Character.Will, Slot.Gloves, "WillGloves", null, null, null);
                Equipment.RegisterEquip("Default", Character.Will, Slot.Arms, "WillArms", allInOnePrefabWill.Find(prefix + "arms").gameObject, allInOnePrefabWill.Find(prefix + "arms_Mask").gameObject, null); //
                Equipment.RegisterEquip("Default", Character.Will, Slot.Shirt, "WillSweater", allInOnePrefabWill.Find(prefix + "sweater").gameObject, allInOnePrefabWill.Find(prefix + "sweater_Mask").gameObject, allInOnePrefabWill.Find(prefix + "sweater_RolledSleeve").gameObject);
                Equipment.RegisterEquip("Default", Character.Will, Slot.Jacket, "WillJacket", allInOnePrefabWill.Find(prefix + "jacket").gameObject, allInOnePrefabWill.Find(prefix + "jacket_Mask").gameObject, null);
                Equipment.RegisterEquip("Default", Character.Will, Slot.Trinkets, "WillTrinkets", trinketsObject.transform.Find(prefix + "trinkets").gameObject, null, null);
                Equipment.RegisterEquip("Default", Character.Will, Slot.Bandages, "WillArms", allInOnePrefabWill.Find(prefix + "bandages").gameObject, null, null);

                foreach (KeyValuePair<string, ClothingInfo> e in clothingData)
                {
                    if (e.Value.bundleName != null)
                    {
                        GameObject prefab = LoadAndSetupFromBundle(clothingBundles[e.Value.bundleName]);
                        GameObject? normalVariant = prefab.transform.Find(prefix + e.Key)?.gameObject;
                        GameObject? maskedVariant = e.Value.hasMasked ? prefab.transform.Find(prefix + e.Key + "_Mask")?.gameObject : null;
                        GameObject? injuredVariant = e.Value.hasInjured ? prefab.transform.Find(prefix + e.Key + "_RolledSleeve")?.gameObject : null;
                        Equipment.RegisterEquip(e.Key, Character.Will, e.Value.clothingSlot, "", normalVariant, maskedVariant, injuredVariant, e.Value.specialFlag);
                    }
                }

                Equipment.RegisterEquip("Event-Halloween2023", Character.Will, Slot.Shirt, "", LoadAndSetupFromBundle(customEventsBundle).transform.Find(prefix + "Halloween2023").gameObject, null, null);


                Utility.Log(System.ConsoleColor.Yellow, "InitializeHands - Will - done");
            }

            


        }

        public static GameObject LoadAndSetupFromBundle(AssetBundle ab, string? assetName = null)
        {
            GameObject go = null;

            if (assetName == null) go = GameObject.Instantiate(ab.LoadAllAssets<GameObject>()[0]);
            else go = GameObject.Instantiate(ab.LoadAsset<GameObject>(assetName));
            if (!go) return go;
            go.name = assetName;
            go.active = false;
            GameObject.DontDestroyOnLoad(go);
            return go;

        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Deserialize))]
        public class Start
        {
            public static void Postfix()
            {
                Utility.Log(System.ConsoleColor.Yellow, "PlayerManager.Start");
                if (!Utility.IsScenePlayable()) return;
                characterIsLoaded = true;
                /*
                if (PlayerManager.m_VoicePersona == VoicePersona.Female)
                {
                    Utility.Log(System.ConsoleColor.Yellow, "Female");
                    //Settings.options.selectedCharacter = 0;
                    //CCSetup.currentCharacter = Character.Astrid;
                }
                if (PlayerManager.m_VoicePersona == VoicePersona.Male)
                {
                    Utility.Log(System.ConsoleColor.Yellow, "Male");
                    //Settings.options.selectedCharacter = 1;
                    //CCSetup.currentCharacter = Character.Will;
                }
                */
                if (Settings.options.selectedCharacter == 0)
                {
                    CCSetup.currentCharacter = Character.Astrid;
                    PlayerManager.m_VoicePersona = VoicePersona.Female;
                }
                if (Settings.options.selectedCharacter == 1)
                {
                    CCSetup.currentCharacter = Character.Will;
                    PlayerManager.m_VoicePersona = VoicePersona.Male;
                }

                needVariableUpdate = true;

                GameManager.GetPlayerVoiceComponent().SetPlayerVoicePersona();


                Utility.Log(System.ConsoleColor.Yellow, "PlayerManager.Start - done");
            }
        }

        [HarmonyPatch(typeof(PlayerVoice), nameof(PlayerVoice.SetPlayerVoicePersona))]
        public class CharacterChange
        {
            static bool? characterLoadComplete = null;

            public static void Postfix()
            {
                if (!startLoading) return;

                

                if (Settings.options.selectedCharacter == 0 && PlayerManager.m_VoicePersona == VoicePersona.Female) // Astrid
                {
                    CCSetup.currentCharacter = Character.Astrid;
                    characterLoadComplete = allLoadCompleteAstrid;
                }
                if (Settings.options.selectedCharacter == 1 && PlayerManager.m_VoicePersona == VoicePersona.Male) // Will
                {
                    CCSetup.currentCharacter = Character.Will;
                    characterLoadComplete = allLoadCompleteWill;
                }
                /*
                if (Settings.options.selectedCharacter == 2 && PlayerManager.m_VoicePersona == VoicePersona.Female) // Marcene
                {
                    CCSetup.currentCharacter = Character.Marcene;
                }
                if (Settings.options.selectedCharacter == 3 && PlayerManager.m_VoicePersona == VoicePersona.Male) // Lincoln
                {
                    CCSetup.currentCharacter = Character.Lincoln;
                }
                */
                if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"Character recognized as {CCSetup.currentCharacter}");

                if (characterLoadComplete == null) return;

                if (characterLoadComplete == false) // initial load
                {
                    InitializeHands();
                    if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"Initial load for {CCSetup.currentCharacter}");
                    if (mainCoroutine != null) MelonCoroutines.Stop(mainCoroutine);
                    mainCoroutine = MelonCoroutines.Start(CCSetup.DoEverything(CCSetup.currentCharacter, 5));
                    needVariableUpdate = false;
                    return;
                }
                if (needVariableUpdate) // bump after stripping
                {
                    InitializeHands();
                    if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"Bump for {CCSetup.currentCharacter}");
                    if (mainCoroutine != null) MelonCoroutines.Stop(mainCoroutine);
                    mainCoroutine = MelonCoroutines.Start(CCSetup.DoEverything(CCSetup.currentCharacter, 3));
                    needVariableUpdate = false;
                    return;
                }
            }
        }




        /*

        [HarmonyPatch(typeof(PlayerManager), "Update")]
        public class Update
        {
            public static void Postfix(ref PlayerManager __instance)
            {
                if (!startLoading) return;
                if (vanillaCharacter == null) vanillaCharacter = GameManager.GetTopLevelCharacterFpsPlayer();


                if (PlayerManager.m_VoicePersona == VoicePersona.Female && CCSetup.currentCharacter != Character.Astrid)
                {
                    InitializeHands();
                    allLoadCompleteAstrid = false;
                    CCSetup.currentCharacter = Character.Astrid;
                    //characterForceChanged = true;
                    vanillaFemaleHandsMesh = new GameObject[] { };
                    if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkCyan, $"F - character updated to {CCSetup.currentCharacter}");
                }

                if (PlayerManager.m_VoicePersona == VoicePersona.Male && CCSetup.currentCharacter != Character.Will)
                {
                    InitializeHands();
                    allLoadCompleteWill = false;
                    CCSetup.currentCharacter = Character.Will;
                    //characterForceChanged = true;
                    vanillaMaleHandsMesh = new GameObject[] { };
                    if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkCyan, $"M - character updated to {CCSetup.currentCharacter}");
                }


                if (CCSetup.currentCharacter == Character.Astrid)
                {
                    if (vanillaFemaleHandsMesh.Length == 0 || vanillaFemaleHandsMesh[0] == null) // load vanilla hands before doing anything else
                    {
                        if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"F - Vanilla hands not loaded yet, loading, character: {CCSetup.currentCharacter}");
                        CCSetup.DoEverything(CCSetup.currentCharacter, 0, 1);

                    }
                    else // hands loaded, continue
                    {
                        if (!allLoadCompleteAstrid) // initial load, also if quit to menu
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"F - Vanilla hands loaded, doing everything else, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 5);
                            needVariableUpdate = false;
                            return;
                        }
                        else if (needVariableUpdate) // character change during gameplay via console
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"F - Most operations already done, reloading only custom meshes, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 3);
                            needVariableUpdate = false;
                            return;
                        }
                    }
                }


                if (CCSetup.currentCharacter == Character.Will)
                {
                    if (vanillaMaleHandsMesh.Length == 0 || vanillaMaleHandsMesh[0] == null)
                    {
                        if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"M - Vanilla hands not loaded yet, loading, character: {CCSetup.currentCharacter}");
                        CCSetup.DoEverything(CCSetup.currentCharacter, 0, 1);

                    }
                    else
                    {
                        if (!allLoadCompleteWill)
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"M - Vanilla hands loaded, doing everything else, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 5);
                            needVariableUpdate = false;
                            return;
                        }
                        else if (needVariableUpdate)
                        {
                            if (Settings.options.debugLog) MelonLogger.Msg(System.ConsoleColor.DarkGreen, $"M - Most operations already done, reloading only custom meshes, character: {CCSetup.currentCharacter}");
                            CCSetup.DoEverything(CCSetup.currentCharacter, 3);
                            needVariableUpdate = false;
                            return;
                        }
                    }
                }
            }
        }
        */
    }
}




