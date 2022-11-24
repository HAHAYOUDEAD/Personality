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
    public class CCSetup : MelonMod
    {
        // variables
        public static Character currentCharacter = Character.Undefined;

        public static Outfit currentMeshSet = Outfit.Undefined;

        public static GameObject currentPhysicsObject;
        public static Dictionary<string, GameObject> currentCustomBones = new Dictionary<string, GameObject>();

        private static readonly int renderLayer = LayerMask.NameToLayer("Weapon");

        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;


        // vanilla mesh operations
        public static GameObject[] GetVanillaHandsObject(Character character)
        {
            if (character == Character.Will || character == Character.Lincoln)
            {
                CCMain.vanillaMaleHandsMesh = new GameObject[] { CCMain.vanillaCharacter?.transform?.Find("NEW_FPHand_Rig/GAME_DATA/Clothing/FPH_Male_Arms/GAME_DATA/Meshes/Will_Hands")?.gameObject,
                        CCMain.vanillaCharacter?.transform?.Find("NEW_FPHand_Rig/GAME_DATA/Clothing/FPH_Male_Arms/GAME_DATA/Meshes/Will_Sleeves")?.gameObject };
                return CCMain.vanillaMaleHandsMesh;
            }

            if (character == Character.Astrid || character == Character.Marcene)
            {
                CCMain.vanillaFemaleHandsMesh = new GameObject[] { CCMain.vanillaCharacter?.transform?.Find("NEW_FPHand_Rig/GAME_DATA/Clothing/FPH_Female_Arms/GAME_DATA/Meshes/Astrid_Arms_NoRing")?.gameObject };
                return CCMain.vanillaFemaleHandsMesh;
            }

            return null;
        }

        public static void HideVanillaHands(Character character, bool hideVanillaHands)
        {
            Utility.Log(ConsoleColor.Gray, "HideVanillaHands - Start");

            GameObject[] vanillaHands = GetVanillaHandsObject(character);

            if (vanillaHands.Length == 0) return;

            foreach (GameObject go in vanillaHands)
            {
                go.active = !hideVanillaHands;
            }

            Utility.Log(ConsoleColor.DarkYellow, "HideVanillaHands - Done");
        }

        // custom mesh operations
        public static GameObject SetupCustomMesh(GameObject go, Character character, bool stealBones = true, bool useCustomBones = false)
        {
            Utility.Log(ConsoleColor.Gray, "SetupCustomMesh - Start");
            GameObject tempGo;
            GameObject vanillaHands = GetVanillaHandsObject(character)?[0];

            if (!vanillaHands)
            {
                Utility.Log(ConsoleColor.Yellow, "SetupCustomMesh - Couldn't grab vanilla hands");
                return go;
            }

            tempGo = GameObject.Instantiate(go);

            tempGo.transform.SetParent(vanillaHands.transform.GetParent());

            foreach (Transform child in Utility.GetAllChildrenRecursive(tempGo.transform))
            {
                child.gameObject.name = go.name;
                child.gameObject.layer = renderLayer;
                child.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMaterial.shader = vanillaSkinnedShader;

                child.gameObject.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                child.gameObject.GetComponent<SkinnedMeshRenderer>().castShadows = false;


                if (stealBones)
                {
                    child.gameObject.AddComponent(vanillaHands.GetComponent<UseParentBones>());
                    child.gameObject.GetComponent<UseParentBones>().DoWork();
                }
            }

            

            if (useCustomBones)
            {
                Transform[] customBones = tempGo.GetComponent<SkinnedMeshRenderer>().bones;

                for (int i = 0; i < customBones.Length; i++)
                {
                    if (currentCustomBones.ContainsKey(customBones[i].name))
                    {
                        customBones[i] = currentCustomBones[customBones[i].name].transform;
                    }
                }

                tempGo.GetComponent<SkinnedMeshRenderer>().bones = customBones;
            }

            Utility.Log(ConsoleColor.DarkYellow, "SetupCustomMesh - Done");
            return tempGo;
        }

        public static void RemovePhysicsBones()
        {
            Utility.Log(ConsoleColor.Gray, "RemovePhysicsBones - Start");

            foreach (KeyValuePair<string, GameObject> entry in currentCustomBones)
            {
                if (entry.Value)
                {
                    GameObject.Destroy(entry.Value);
                }
            }
            currentCustomBones = new Dictionary<string, GameObject>();
            Utility.Log(ConsoleColor.DarkYellow, "RemovePhysicsBones - Done");
        }

        public static void SetupPhysicsBones(GameObject container, bool setupRigidBody = true)
        {
            Utility.Log(ConsoleColor.Gray, "SetupPhysicsBones - Start");

            foreach (GameObject dummyBone in container.GetAllImmediateChildren()) // DummyBone_*
            {
                Transform realBone;

                realBone = Utility.FindDeepChild(CCMain.vanillaCharacter.transform, dummyBone.name.Replace("DummyBone_", ""));
                if (realBone) // attach custom bones to real bone
                {
                    dummyBone.transform.position = realBone.transform.position;
                    dummyBone.transform.rotation = realBone.transform.rotation;

                    foreach (GameObject customBone in dummyBone.GetAllImmediateChildren()) // main custom bone
                    {
                        if (setupRigidBody)
                        {
                            Rigidbody root = realBone.gameObject.GetComponent<Rigidbody>();
                            if (root == null)
                            {
                                root = realBone.gameObject.AddComponent<Rigidbody>();
                                root.isKinematic = true;
                                root.useGravity = false;
                            }
                            if (!customBone.GetComponent<Joint>().connectedBody) customBone.GetComponent<Joint>().connectedBody = root;
                            //bone.GetComponent<Joint>().autoConfigureConnectedAnchor = false;
                        }

                        customBone.GetComponent<Rigidbody>().inertiaTensor = Vector3.one * 0.0005f;
                        customBone.transform.SetParent(realBone);
                        //customBone.layer = 15; // NoCollidePlayer
                        currentCustomBones[customBone.name] = customBone;

                    }
                }
            }

            Utility.Log(ConsoleColor.DarkYellow, "SetupPhysicsBones - Done");
        }

        // texture operations
        public static IEnumerator ApplyCustomTextures(Character character)
        {
            List<string> tempNames = new List<string>();
            Dictionary<Slot, Equip> equips = Equipment.allEquipment[character]["Default"];

            if (Settings.options.displayProperClothes) // when proper clothes enabled, only change arms texture
            {
                equips = new Dictionary<Slot, Equip>()
                {
                    { Slot.Arms, equips[Slot.Arms] },
                    { Slot.Hands, equips[Slot.Hands] }
                };
            }
            //Equip[] equips = Equipment.GetAllEquipment(character);

            foreach (Equip e in equips.Values)
            {
                if (e == null) continue;
                if (!tempNames.Contains(e.textureName)) tempNames.Add(e.textureName);
            }
            List<Texture2D> tempTextures = new List<Texture2D>();

            foreach (string n in tempNames)
            {
                Texture2D tex = new Texture2D(2, 2) { name = n };
                byte[] file = null;
                try
                {
                    file = File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/customTextures/" + n + ".png");
                }
                catch (Exception)
                {
                    Utility.Log(ConsoleColor.DarkGray, $"ApplyCustomTextures - could not find {n} texture, skipping");
                    continue;
                }
                if (file != null)
                {
                    ImageConversion.LoadImage(tex, file);
                    Utility.Log(ConsoleColor.DarkCyan, $"ApplyCustomTextures - loaded custom texture: {n}");
                    tempTextures.Add(tex);
                }
            }

            foreach (Equip e in equips.Values)
            {
                foreach (Texture2D tex in tempTextures)
                {
                    if (e == null) continue;
                    if (tex.name == e.textureName)
                    {
                        if (e.normalVariant) e.normalVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                        if (e.undermaskVariant) e.undermaskVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                        if (e.injuredVariant) e.injuredVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                    }
                }
            }

            tempTextures = null;

            yield return null;
        }

        public static void TintTexture(Slot slot, int H, int S, int L)
        {

            Color color = Utility.HslToRgb(H / 360f, S / 100f, L / 100f);

            Equip equip = Equipment.allEquipment[currentCharacter]["Default"][slot];

            if (equip.normalVariant) equip.normalVariant.GetComponent<SkinnedMeshRenderer>().material.color = color;
            if (equip.undermaskVariant) equip.undermaskVariant.GetComponent<SkinnedMeshRenderer>().material.color = color;
            if (equip.injuredVariant) equip.injuredVariant.GetComponent<SkinnedMeshRenderer>().material.color = color;

            Utility.Log(ConsoleColor.Cyan, "TintTexture - Done. Changed texture to " + color.ToString());
        }

        // custom mesh variants management
        public static void SwitchDefaultOutfit(Outfit meshSet)
        {
            PartVariant visible = PartVariant.Normal;
            PartVariant disabled = PartVariant.Disabled;
            PartVariant injured = PartVariant.Injured;

            if (meshSet == Outfit.Indoors || meshSet == Outfit.Injured) Equipment.ChangeEquipVariant(disabled, visible, disabled);
            if (meshSet == Outfit.Undressed) Equipment.ChangeEquipVariant(disabled, disabled, disabled);
            if (meshSet == Outfit.Outdoors) Equipment.ChangeEquipVariant(visible, visible, visible);

            currentMeshSet = meshSet;

            AutoSwitchMeshVariant(meshSet == Outfit.Injured);
        }

        public static void AutoSwitchMeshVariant(bool isInjured = false)
        {
            PartVariant visible = PartVariant.Normal;
            PartVariant disabled = PartVariant.Disabled;
            PartVariant injured = PartVariant.Injured;
            PartVariant masked = PartVariant.Undermask;

            //MelonLogger.Msg("a " + Equipment.currentEquipment[Slot.Gloves]);
            if (Equipment.currentEquipment[Slot.Gloves] == null || Equipment.currentEquipment[Slot.Gloves].normalVariant == null || Equipment.currentEquipment[Slot.Gloves].currentVariantEnum == disabled)
            {
                if (isInjured) Equipment.ChangeEquipVariant(Slot.Hands, injured);
                else Equipment.ChangeEquipVariant(Slot.Hands, visible);
            }
            else if (Equipment.currentEquipment[Slot.Gloves].currentVariantEnum == visible)
            {
                Equipment.ChangeEquipVariant(Slot.Hands, masked);
            }

            //if (Equipment.currentEquipment[Slot.Shirt] == null) return;

            if (Equipment.currentEquipment[Slot.Shirt].currentVariantEnum == masked && Equipment.currentEquipment[Slot.Jacket].currentVariantEnum != visible)
            {
                Equipment.ChangeEquipVariant(Slot.Shirt, visible);
            }
            if (Equipment.currentEquipment[Slot.Shirt].currentVariantEnum == visible)
            {
                if (isInjured) Equipment.ChangeEquipVariant(Slot.Shirt, injured);
                Equipment.ChangeEquipVariant(Slot.Arms, disabled);
            }
            else if (Equipment.currentEquipment[Slot.Shirt].currentVariantEnum == disabled)
            {
                Equipment.ChangeEquipVariant(Slot.Arms, visible);
            }
            else if (Equipment.currentEquipment[Slot.Shirt].currentVariantEnum == injured)
            {
                if (!isInjured) Equipment.ChangeEquipVariant(Slot.Shirt, visible);
            }

            //if (Equipment.currentEquipment[Slot.Jacket] == null) return;

            if (Equipment.currentEquipment[Slot.Jacket].currentVariantEnum == visible && Equipment.currentEquipment[Slot.Shirt].currentVariantEnum != disabled)
            {
                Equipment.ChangeEquipVariant(Slot.Shirt, masked);
            }


        }

        public static void SetClothesToDefault()
        {
            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
            {
                Equipment.ChangeEquipIfExistsOtherwiseDefault(slot, "Default");
            }
        }

        public static void SmartUpdateOutfit()
        {
            Utility.Log(ConsoleColor.Gray, "UpdateVisibility - Start");

            // toggle trinkets
            Equipment.ToggleTrinkets(Settings.options.enableTrinkets);


            if (Settings.options.displayProperClothes)
            {
                currentMeshSet = Outfit.Custom;
                AutoSwitchMeshVariant();
                
                Utility.Log(ConsoleColor.DarkYellow, $"UpdateVisibility - Done for {currentMeshSet} outfit");
                return;
            }




            Outfit newOutfit = Outfit.Undefined; 

            // swith between indoors/ outdoors when enabled
            if (Settings.options.dynamicOutfit)
            {
                if (CCChecks.currentlyIndoors) newOutfit = Outfit.Indoors;
                if (!CCChecks.currentlyIndoors) newOutfit = Outfit.Outdoors;
            }

            // change default appearance if auto switch disabled 
            else
            {
                switch (Settings.options.defaultAppearance)
                {
                    case 0: // vanilla
                        newOutfit = Outfit.Indoors;
                        break;
                    case 1: // full outfit
                        newOutfit = Outfit.Outdoors;
                        break;
                    case 2: // injured
                        newOutfit = Outfit.Injured;
                        break;
                    case 3: // undressed
                        newOutfit = Outfit.Undressed;
                        break;
                }
            }

            // switch between injured/normal
            if (newOutfit == Outfit.Indoors && Settings.options.dynamicOutfit)
            {
                if (CCChecks.currentlyInjured && currentMeshSet != Outfit.Injured)
                {
                    newOutfit = Outfit.Injured;
                }
            }

            SwitchDefaultOutfit(newOutfit);

            


            Utility.Log(ConsoleColor.DarkYellow, $"UpdateVisibility - Done for {currentMeshSet} outfit");
        }

        public static void UpdateClothingSlots()
        {
            bool changed = false;

            GearItem ChestBase = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Base);
            GearItem ChestMid = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Mid);
            GearItem ChestTop = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Top);
            GearItem ChestTop2 = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Top2);
            GearItem Gloves = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Hands, ClothingLayer.Base);

            GearItem Shirt = ChestMid ? ChestMid : ChestBase;
            GearItem Jacket = ChestTop2 ? ChestTop2 : ChestTop;

            if (CCChecks.UpdateSlotIfNeeded(Shirt, Slot.Shirt)) changed = true;
            if (CCChecks.UpdateSlotIfNeeded(Jacket, Slot.Jacket)) changed = true;
            if (CCChecks.UpdateSlotIfNeeded(Gloves, Slot.Gloves)) changed = true;

            if (changed) CCSetup.SmartUpdateOutfit();
        }

        public static void DoEverything(Character character, int arg, int specificArg = 0)
        {
            // 1 get active player hands for mesh replacement
            if (arg >= 1 || specificArg == 1)
            {
                if (GetVanillaHandsObject(character).Length == 0)
                {
                    Utility.Log(ConsoleColor.Red, "Couldn't load vanilla arms, returning");
                    return;
                }
                else Utility.Log(ConsoleColor.DarkCyan, $"1 - Vanilla arms mesh is loaded, character: {character}");

                if (specificArg == 1) return;
            }

            // 2 transfer bone components from vanilla arms to custom meshes
            if (arg >= 2 || specificArg == 2)
            {
                /*
                if (character == Character.Astrid)
                {
                    RemovePhysicsBones();
                    
                    if (!currentPhysicsObject) currentPhysicsObject = GameObject.Instantiate(CCMain.physicsObjectAstrid);

                    SetupPhysicsBones(currentPhysicsObject); // should be done before SetupCustomMesh
                    
                }
                */
                
                foreach (Dictionary<Slot, Equip> equipDict in Equipment.allEquipment[character].Values)
                {
                    foreach (Equip equip in equipDict.Values)
                    {
                        if (equip == null) continue;
                        if (equip.normalVariant) equip.normalVariant.active = false;
                        if (equip.undermaskVariant) equip.undermaskVariant.active = false;
                        if (equip.injuredVariant) equip.injuredVariant.active = false;

                        if (equip.slot == Slot.Trinkets)
                        {
                            if (equip.normalVariant) equip.normalVariant = SetupCustomMesh(equip.normalVariant, character, true, true);
                            continue;
                        }
                        else
                        {
                            if (equip.normalVariantPrefab && !equip.normalVariant) equip.normalVariant = SetupCustomMesh(equip.normalVariantPrefab, character, true, false);
                            if (equip.undermaskVariantPrefab && !equip.undermaskVariant) equip.undermaskVariant = SetupCustomMesh(equip.undermaskVariantPrefab, character, true, false);
                            if (equip.injuredVariantPrefab && !equip.injuredVariant) equip.injuredVariant = SetupCustomMesh(equip.injuredVariantPrefab, character, true, false);
                        }
                    }
                }

                SetClothesToDefault();
                SmartUpdateOutfit();
                HideVanillaHands(character, true);

                if (Settings.options.displayProperClothes)
                {
                    UpdateClothingSlots();
                }

                Utility.Log(ConsoleColor.DarkCyan, $"2 - Stole bone components to activate custom mesh rigs, character:{character}");

                if (specificArg == 2) return;
            }


            // 3 scan for custom textures
            if (arg >= 3 || specificArg == 3)
            {
                if (Settings.options.useCustomTextures)
                {
                    MelonCoroutines.Start(ApplyCustomTextures(character));

                    Utility.Log(ConsoleColor.DarkCyan, $"3 - Replaced textures with custom ones, character:{character}");
                }
                else
                {
                    //Equip[] equips = Equipment.GetAllEquipment(character);

                    Dictionary<Slot, Equip> equips = Equipment.allEquipment[character]["Default"];

                    foreach (Equip equip in equips.Values)
                    {
                        if (equip == null) continue;
                        Texture tex = equip.normalVariantPrefab?.GetComponent<SkinnedMeshRenderer>().material.mainTexture;

                        if (tex)
                        {
                            if (equip.normalVariant) equip.normalVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                            if (equip.undermaskVariant) equip.undermaskVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                            if (equip.injuredVariant) equip.injuredVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                        }
                    }

                    Utility.Log(ConsoleColor.DarkCyan, $"3 - Reloaded textures from prefabs, character:{character}");
                }

                if (Settings.options.useTextureTint)
                {
                    TintTexture(Slot.Hands, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                    TintTexture(Slot.Arms, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                }

                if (specificArg == 3) return;
            }

            // 4 apply custom textures to vanilla arms for mods that don't use vanilla rig (binoculars, pastime reading)
            if (arg >= 4 || specificArg == 4)
            {
                /*
                Texture2D texF = new Texture2D(2, 2);
                ImageConversion.LoadImage(texF, File.ReadAllBytes("Mods/characterCustomizer/handsF.png"));
                CCMain.vanillaFemaleHandsMesh.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = texF;

                Texture2D texM = new Texture2D(2, 2);
                ImageConversion.LoadImage(texM, File.ReadAllBytes("Mods/characterCustomizer/handsM.png"));
                CCMain.vanillaMaleHandsMesh.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = texM;

    */
                Utility.Log(ConsoleColor.DarkCyan, "4 - Does nothing for now");
                

                if (specificArg == 4) return;
            }
            

            // 5 apply doll textures
            if (arg >= 5 || specificArg == 5)
            {
                GameObject dollF = InterfaceManager.GetPanel<Panel_Clothing>().m_PaperDollFemale;
                GameObject dollFHands = dollF.transform.Find("DollHands").gameObject;
                GameObject dollFHead = dollF.transform.Find("DollHead").gameObject;
                GameObject dollFFeet = dollF.transform.Find("DollFeet").gameObject;
                GameObject dollFBody = dollF.transform.Find("DollBody").gameObject;

                GameObject dollM = InterfaceManager.GetPanel<Panel_Clothing>().m_PaperDollMale;
                GameObject dollMHands = dollM.transform.Find("DollHands").gameObject;
                GameObject dollMHead = dollM.transform.Find("DollHead").gameObject;
                GameObject dollMFeet = dollM.transform.Find("DollFeet").gameObject;
                GameObject dollMBody = dollM.transform.Find("DollBody").gameObject;


                Texture2D headTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture2D handsTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture2D feetTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture2D bodyTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);

                ImageConversion.LoadImage(headTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/F/head.png"));
                ImageConversion.LoadImage(handsTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/F/hands.png"));
                ImageConversion.LoadImage(feetTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/F/feet.png"));
                ImageConversion.LoadImage(bodyTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/F/body.png"));

                dollFHead.GetComponent<UITexture>().mainTexture = headTex;
                dollFHands.GetComponent<UITexture>().mainTexture = handsTex;
                dollFFeet.GetComponent<UITexture>().mainTexture = feetTex;
                dollFBody.GetComponent<UITexture>().mainTexture = bodyTex;

                Texture2D headTex2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture2D handsTex2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture2D feetTex2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture2D bodyTex2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);

                ImageConversion.LoadImage(headTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/M/head.png"));
                ImageConversion.LoadImage(handsTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/M/hands.png"));
                ImageConversion.LoadImage(feetTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/M/feet.png"));
                ImageConversion.LoadImage(bodyTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "/paperDoll/M/body.png"));

                dollMHead.GetComponent<UITexture>().mainTexture = headTex2;
                dollMHands.GetComponent<UITexture>().mainTexture = handsTex2;
                dollMFeet.GetComponent<UITexture>().mainTexture = feetTex2;
                dollMBody.GetComponent<UITexture>().mainTexture = bodyTex2;

                Utility.Log(ConsoleColor.DarkCyan, "5 - Applied custom PAPERDOLL textures");

                if (specificArg == 5) return;
            }

            Utility.Log(ConsoleColor.DarkCyan, $"All operations complete, shutting down. Character: {character}");
            if (character == Character.Astrid) CCMain.allLoadCompleteAstrid = true;
            if (character == Character.Will) CCMain.allLoadCompleteWill = true;

        }

    }
}