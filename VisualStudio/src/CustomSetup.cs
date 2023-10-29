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
            //if (CCMain.vanillaCharacter == null) CCMain.vanillaCharacter = GameManager.GetTopLevelCharacterFpsPlayer();

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
            Utility.Log(System.ConsoleColor.Gray, "HideVanillaHands - Start");

            GameObject[] vanillaHands = GetVanillaHandsObject(character);

            if (vanillaHands.Length == 0) return;

            foreach (GameObject go in vanillaHands)
            {
                go.active = !hideVanillaHands;
            }

            Utility.Log(System.ConsoleColor.DarkYellow, "HideVanillaHands - Done");
        }

        // custom mesh operations
        public static GameObject SetupCustomMesh(GameObject go, Character character, bool stealBones = true, bool useCustomBones = false)
        {
            Utility.Log(System.ConsoleColor.Gray, "SetupCustomMesh - Start");
            go.active = false;

            GameObject tempGo;
            GameObject vanillaHands = GetVanillaHandsObject(character)?[0];

            Utility.Log(System.ConsoleColor.Yellow, "SetupCustomMesh - character: " + character);

            if (!vanillaHands)
            {
                Utility.Log(System.ConsoleColor.Yellow, "SetupCustomMesh - Couldn't grab vanilla hands");
                return go;
            }

            tempGo = vanillaHands.transform.GetParent().Find(go.name)?.gameObject;

            if (!tempGo) // if doesn't already exist
            {
                tempGo = GameObject.Instantiate(go);
                tempGo.name = go.name;

                tempGo.transform.SetParent(vanillaHands.transform.GetParent());

                foreach (Transform child in Utility.GetAllChildrenRecursive(tempGo.transform))
                {
                    //child.gameObject.name = go.name;
                    child.gameObject.layer = renderLayer;
                    if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        child.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMaterial.shader = vanillaSkinnedShader;

                        child.gameObject.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                        child.gameObject.GetComponent<SkinnedMeshRenderer>().castShadows = false;

                        if (stealBones)
                        {
                            child.gameObject.AddComponent(vanillaHands.GetComponent<UseParentBones>());
                            child.gameObject.GetComponent<UseParentBones>().DoWork();
                        }
                    }




                }


                if (useCustomBones)
                {
                    foreach (GameObject tempGoChild in tempGo.GetAllImmediateChildren())
                    {
                        if (tempGoChild.GetComponent<SkinnedMeshRenderer>() != null)
                        {
                            Transform[] customBones = tempGoChild.GetComponent<SkinnedMeshRenderer>().bones;

                            for (int i = 0; i < customBones.Length; i++)
                            {
                                if (currentCustomBones.ContainsKey(customBones[i].name))
                                {
                                    customBones[i] = currentCustomBones[customBones[i].name].transform;
                                }
                            }

                            tempGoChild.GetComponent<SkinnedMeshRenderer>().bones = customBones;
                            tempGoChild.GetComponent<SkinnedMeshRenderer>().rootBone = customBones[0];
                        }


                    }

                }




                Utility.Log(System.ConsoleColor.DarkYellow, "SetupCustomMesh - Done");
                return tempGo;
            }
            else
            {
                Utility.Log(System.ConsoleColor.Cyan, "SetupCustomMesh - Restored");
                return tempGo;
            }

        }

        public static void RemovePhysicsBones()
        {
            Utility.Log(System.ConsoleColor.Gray, "RemovePhysicsBones - Start");

            foreach (KeyValuePair<string, GameObject> entry in currentCustomBones)
            {
                if (entry.Value)
                {
                    GameObject.Destroy(entry.Value);
                }
            }
            currentCustomBones = new Dictionary<string, GameObject>();
            Utility.Log(System.ConsoleColor.DarkYellow, "RemovePhysicsBones - Done");
        }

        public static void SetupPhysicsBones(GameObject container, bool setupRigidBody = true)
        {
            Utility.Log(System.ConsoleColor.Gray, "SetupPhysicsBones - Start");

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
                        /*
                        if (setupRigidBody) // setup rigidBody in vanilla bone (if custom root bone has joint)
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
                        */
                        

                        foreach (Rigidbody rb in customBone.GetComponentsInChildren<Rigidbody>())
                        {
                            rb.inertiaTensor = Vector3.one * 0.05f;
                        }

                        //customBone.GetComponent<Rigidbody>().inertiaTensor = Vector3.one * 0.0005f;
                        customBone.transform.SetParent(realBone);
                        //MelonLogger.Msg(Utility.GetGameObjectPath(customBone));
                        //MelonLogger.Msg(Utility.GetGameObjectPath(realBone.gameObject));

                        //MelonLogger.Msg("ALLO BLYAT");
                        foreach (Transform childBone in Utility.GetAllChildrenRecursive(customBone.transform))
                        {
                            //MelonLogger.Msg(childBone.name);
                            currentCustomBones[childBone.name] = childBone.gameObject;
                            childBone.gameObject.layer = vp_Layer.NoCollidePlayer;
                        }

                        //customBone.layer = 15; // NoCollidePlayer
                        
                    }
                }
            }

            Utility.Log(System.ConsoleColor.DarkYellow, "SetupPhysicsBones - Done");
        }

        // texture operations
        public static IEnumerator ApplyCustomTextures(Character character)
        {
            List<string> tempNames = new List<string>();
            Dictionary<Slot, Equip> equips = Equipment.allEquipment[character]["Default"];

            /*
            if (Settings.options.displayProperClothes) // when proper clothes enabled, only change arms texture
            {
                equips = new Dictionary<Slot, Equip>()
                {
                    { Slot.Arms, equips[Slot.Arms] },
                    { Slot.Hands, equips[Slot.Hands] }
                };
            }
            */
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
                    file = File.ReadAllBytes("Mods/" + CCMain.modFolderName + "customTextures/" + n + ".png");
                }
                catch (Exception)
                {
                    Utility.Log(System.ConsoleColor.DarkGray, $"ApplyCustomTextures - could not find {n} texture, skipping");
                    continue;
                }
                if (file != null)
                {
                    ImageConversion.LoadImage(tex, file);
                    Utility.Log(System.ConsoleColor.DarkCyan, $"ApplyCustomTextures - loaded custom texture: {n}");
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
                        if (e.normalVariant)
                        {
                            foreach(GameObject child in e.normalVariant.GetAllImmediateChildren())
                            {
                                if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = tex;
                            }
                        }
                        if (e.maskedVariant)
                        {
                            foreach (GameObject child in e.maskedVariant.GetAllImmediateChildren())
                            {
                                if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = tex;
                            }
                        }
                        if (e.injuredVariant)
                        {
                            foreach (GameObject child in e.injuredVariant.GetAllImmediateChildren())
                            {
                                if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = tex;
                            }
                        }

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


            if (equip.normalVariant)
            {
                foreach (GameObject child in equip.normalVariant.GetAllImmediateChildren())
                {
                    if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.color = color;
                }
            }
            if (equip.maskedVariant)
            {
                foreach (GameObject child in equip.maskedVariant.GetAllImmediateChildren())
                {
                    if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.color = color;
                }
            }
            if (equip.injuredVariant)
            {
                foreach (GameObject child in equip.injuredVariant.GetAllImmediateChildren())
                {
                    if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.color = color;
                }
            }


            Utility.Log(System.ConsoleColor.Cyan, "TintTexture - Done. Changed texture to " + color.ToString());
        }

        // custom mesh variants management
        public static void SwitchDefaultOutfit(Outfit meshSet)
        {
            PartVariant visible = PartVariant.Normal;
            PartVariant disabled = PartVariant.Disabled;
            PartVariant injured = PartVariant.Injured;

            if (meshSet == Outfit.Indoors || meshSet == Outfit.InjuredIndoors) Equipment.ChangeEquipVariant(disabled, visible, disabled);
            if (meshSet == Outfit.Undressed) Equipment.ChangeEquipVariant(disabled, disabled, disabled);
            if (meshSet == Outfit.Outdoors || meshSet == Outfit.InjuredOutdoors) Equipment.ChangeEquipVariant(visible, visible, visible);

            currentMeshSet = meshSet;

            bool isInjured = meshSet == Outfit.InjuredIndoors || meshSet == Outfit.InjuredOutdoors;
            AutoSwitchMeshVariant(isInjured);
        }


        public static void OverrideOutfitForEvent()
        {

            Equipment.ToggleTrinkets(false);
            Equipment.ToggleBandages(false);

            switch (Settings.options.specialEventOutfit)
            {

                case 0: //Halloween 2023

                    Equipment.ChangeEquipIfExistsOtherwiseDefault(Slot.Shirt, "Event-Halloween2023");

                    if (currentCharacter == Character.Astrid)
                    {
                        Equipment.ChangeEquipVariant(Slot.Arms, PartVariant.Disabled);
                        Equipment.ChangeEquipVariant(Slot.Hands, PartVariant.Normal, PartVariant.Disabled);
                        Equipment.ChangeEquipVariant(Slot.Gloves, PartVariant.Disabled);
                        Equipment.ChangeEquipVariant(Slot.Shirt, PartVariant.Normal);
                        Equipment.ChangeEquipVariant(Slot.Jacket, PartVariant.Disabled);
                    }

                    if (currentCharacter == Character.Will)
                    {
                        Equipment.ChangeEquipVariant(Slot.Arms, PartVariant.Disabled);
                        Equipment.ChangeEquipVariant(Slot.Hands, PartVariant.Disabled);
                        Equipment.ChangeEquipVariant(Slot.Gloves, PartVariant.Disabled);
                        Equipment.ChangeEquipVariant(Slot.Shirt, PartVariant.Normal);
                        Equipment.ChangeEquipVariant(Slot.Jacket, PartVariant.Disabled);
                    }

                    if (currentCharacter == Character.Marcene)
                    {

                    }

                    if (currentCharacter == Character.Lincoln)
                    {

                    }

                    break;
            }
        }

        public static void AutoSwitchMeshVariant(bool isInjured = false, bool? asIfMittenIsOff = null) // asIfMittenIsOff true = right, false = left
        {
            PartVariant visible = PartVariant.Normal;
            PartVariant disabled = PartVariant.Disabled;
            PartVariant injured = PartVariant.Injured;
            PartVariant masked = PartVariant.Masked;

            Equip gloves = Equipment.currentEquipment[Slot.Gloves];
            Equip hands = Equipment.currentEquipment[Slot.Hands];
            Equip arms = Equipment.currentEquipment[Slot.Arms];
            Equip shirt = Equipment.currentEquipment[Slot.Shirt];
            Equip jacket = Equipment.currentEquipment[Slot.Jacket];

            PartVariant finalGloves = PartVariant.Undefined;
            PartVariant finalHands = PartVariant.Undefined;
            PartVariant finalArms = PartVariant.Undefined;
            PartVariant finalShirt = PartVariant.Undefined;
            PartVariant finalJacket = PartVariant.Undefined;

            bool largeGloves = false;

            finalHands = visible;



            // show shirt when jacket is disabled
            if (shirt.currentVariantEnum == masked && jacket.currentVariantEnum != visible)
            {
                finalShirt = visible;
            }

            // hide arms when shirt is visible
            if (shirt.currentVariantEnum == visible)
            {
                if (isInjured)
                {
                    finalShirt = injured;
                    finalArms = masked;
                }
                else
                {
                    finalArms = disabled;
                }
            }

            // show arms when shirt is disabled
            if (shirt.currentVariantEnum == disabled)
            {
                finalArms = visible;

            }

            // switch shirt to normal when no longer injured
            if (shirt.currentVariantEnum == injured)
            {
                if (!isInjured)
                {
                    finalShirt = visible;
                    finalArms = disabled;
                }
            }



            // mask shirt when jacket is visible


            // tuck in jacket for large gloves
            if (asIfMittenIsOff == null)
            {

                if (isInjured)
                {
                    Equipment.ToggleBandages(true);
                }
                else
                {
                    Equipment.ToggleBandages(false);
                }

                if (jacket.currentVariantEnum != disabled)
                {
                    if (shirt.currentVariantEnum != disabled)
                    {
                        finalShirt = masked;
                        finalArms = disabled;
                    }

                    if (isInjured)
                    {
                        if (currentCharacter == Character.Astrid)
                        {
                            Equipment.ToggleBandages(true, false); // left bandage is on the hand, keep it
                        }
                        if (currentCharacter == Character.Will)
                        {
                            Equipment.ToggleBandages(false); // only one bandage on left wrist, hide it
                        }
                        if (currentCharacter == Character.Marcene)
                        {

                        }
                        if (currentCharacter == Character.Lincoln)
                        {

                        }
                    }

                    Equipment.ToggleTrinkets(false);
                }

                if (gloves != null && gloves.normalVariant != null && gloves.currentVariantEnum != disabled)
                {
                    finalHands = masked;

                    if (isInjured)
                    {
                        Equipment.ToggleBandages(false);
                    }

                    if (gloves.specialFlag == SpecialFlag.LargeMittens || gloves.specialFlag == SpecialFlag.LargeGloves)
                    {
                        largeGloves = true;



                        if (jacket.currentVariantEnum == visible)
                        {
                            if (gloves.specialFlag == SpecialFlag.LargeGloves || jacket.specialFlag != SpecialFlag.LargeJacket) // no masking if rabbitskin mitts and large jacket
                            {
                                finalJacket = masked;
                            }
                            else
                            {
                                finalJacket = visible;
                            }
                            
                            if (shirt.currentVariantEnum != disabled) finalShirt = disabled;
                        }

                    }
                    else
                    {
                        if (jacket.currentVariantEnum == masked)
                        {
                            finalJacket = visible;
                        }

                        //finalHands = masked;
                    }
                }


                if (finalArms != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Arms, finalArms);
                if (finalHands != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Hands, finalHands);
                if (finalGloves != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Gloves, finalGloves);
                if (finalShirt != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Shirt, finalShirt);
                if (finalJacket != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Jacket, finalJacket);



                if (largeGloves)
                {
                    if (Settings.options.mittensAppearance != 0) // take off one glove
                    {

                        if (Settings.options.mittensAppearance == 2) // dynamic and fun
                        {
                            if (finalJacket == visible || finalJacket == masked)
                            {
                                SwitchGlovesAppearance(true, isInjured, MittenVariant.RightOff); // right dangle
                            }
                            else
                            {
                                SwitchGlovesAppearance(false, isInjured, MittenVariant.RightOff); // right off
                            }
                        }
                        else // dynamic
                        {
                            SwitchGlovesAppearance(false, isInjured, MittenVariant.RightOff); // right off
                        }

                    }

                }

                Utility.Log(System.ConsoleColor.Cyan, "AutoSwitchMeshVariant - Done. generic");

            }
            else
            {
                PartVariant currentArms = arms.currentVariantEnum;
                PartVariant currentHands = hands.currentVariantEnum;
                PartVariant currentShirt = shirt.currentVariantEnum;
                PartVariant currentJacket = jacket.currentVariantEnum;

                if (jacket.currentVariantEnum == masked)
                {
                    finalJacket = visible;
                    if (shirt.isActuallyEquipped) finalShirt = masked;
                }

                if (asIfMittenIsOff == true) // right
                {
                    //MelonLogger.Msg("!!!!!!!!!!! " + currentJacket + " " + finalJacket);
                    if (finalArms != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Arms, currentArms, finalArms);
                    if (finalHands != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Hands, currentHands, finalHands);
                    if (finalShirt != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Shirt, currentShirt, finalShirt);
                    if (finalJacket != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Jacket, currentJacket, finalJacket);
                    if (isInjured)
                    {
                        Equipment.ToggleBandages(false, true);

                        if (gloves.specialFlag == SpecialFlag.LargeMittens && 
                            currentCharacter == Character.Will &&
                            jacket.currentVariantEnum == disabled &&
                            isInjured) // show bandage for will under large mitts when no jacket
                        {
                            Equipment.ToggleBandages(true);
                        }
                    }



                }
                if (asIfMittenIsOff == false) // left
                {
                    if (finalArms != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Arms, finalArms, currentArms);
                    if (finalHands != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Hands, finalHands, currentHands);
                    if (finalShirt != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Shirt, finalShirt, currentShirt);
                    if (finalJacket != PartVariant.Undefined) Equipment.ChangeEquipVariant(Slot.Jacket, finalJacket, currentJacket);
                    if (isInjured)
                    {
                        Equipment.ToggleBandages(true, false);
                    }
                }

                Utility.Log(System.ConsoleColor.Cyan, "AutoSwitchMeshVariant - Done. asIfMittenIsOff " + asIfMittenIsOff);
            }

            
        }

        public static void SwitchGlovesAppearance(bool dangle, bool isInjured, MittenVariant variant = MittenVariant.BothOn)
        {
            if (dangle)
            {
                if (variant == MittenVariant.RightOff)
                {
                    Equipment.ChangeEquipVariant(Slot.Gloves, PartVariant.Normal, PartVariant.Masked);
                    AutoSwitchMeshVariant(isInjured, true);
                }

                if (variant == MittenVariant.LeftOff)
                {
                    Equipment.ChangeEquipVariant(Slot.Gloves, PartVariant.Masked, PartVariant.Normal);
                    AutoSwitchMeshVariant(isInjured, false);
                }
            }
            else
            {
                if (variant == MittenVariant.RightOff)
                {
                    Equipment.ChangeEquipVariant(Slot.Gloves, PartVariant.Normal, PartVariant.Disabled);
                    AutoSwitchMeshVariant(isInjured, true);
                }

                if (variant == MittenVariant.LeftOff)
                {
                    Equipment.ChangeEquipVariant(Slot.Gloves, PartVariant.Disabled, PartVariant.Normal);
                    AutoSwitchMeshVariant(isInjured, false);
                }
            }

            // lantern - both on
            // climbing - both on
            // revolver - right off
            // rifle - right off
            // flare - both on
            // matches - right off
            // flaregun - right off
            // bow - right off
            // flashlight - both on
            // spray paint - right off
            // torch - both on
            // shortwave - right off
            // noisemaker - both on
            // stone - both on

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
            Utility.Log(System.ConsoleColor.Gray, "UpdateVisibility - Start");


            if (Settings.options.specialEventOverride)
            {
                OverrideOutfitForEvent();
                return;
            }


            // toggle trinkets
            Equipment.ToggleTrinkets(Settings.options.enableTrinkets);

            /*
            if (Settings.options.displayProperClothes)
            {
                currentMeshSet = Outfit.Custom;
                AutoSwitchMeshVariant();
                
                Utility.Log(System.ConsoleColor.DarkYellow, $"UpdateVisibility - Done for {currentMeshSet} outfit");
                return;
            }
            */
            

            Outfit newOutfit = Outfit.Undefined; 

            // swith between indoors/ outdoors when enabled
            if (Settings.options.dynamicOutfit)
            {
                if (CCChecks.currentlyIndoors) newOutfit = Outfit.Indoors;
                if (!CCChecks.currentlyIndoors) newOutfit = Outfit.Outdoors;
            }

            // change default appearance if auto switch disabled 
            else if (!Settings.options.displayProperClothes)
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
                        newOutfit = Outfit.InjuredIndoors;
                        break;
                    case 3: // undressed
                        newOutfit = Outfit.Undressed;
                        break;
                }
            }
            else 
            {
                newOutfit = Outfit.Outdoors; // full outfit when dynamicOutfit is off
            }

            // switch between injured/normal
            if (Settings.options.dynamicOutfit && CCChecks.currentlyInjured)
            {
                if (newOutfit == Outfit.Indoors) newOutfit = Outfit.InjuredIndoors;
                if (newOutfit == Outfit.Outdoors) newOutfit = Outfit.InjuredOutdoors;
            }

            Equipment.SetActuallyEquipped(Slot.Gloves, GetGearItemEquippedInSlot(Slot.Gloves) != null);
            Equipment.SetActuallyEquipped(Slot.Jacket, GetGearItemEquippedInSlot(Slot.Jacket) != null);
            Equipment.SetActuallyEquipped(Slot.Shirt, GetGearItemEquippedInSlot(Slot.Shirt) != null);

            SwitchDefaultOutfit(newOutfit);

            


            Utility.Log(System.ConsoleColor.DarkYellow, $"UpdateVisibility - Done for {currentMeshSet} outfit");
        }

        public static GearItem GetGearItemEquippedInSlot(Slot slot)
        {
            GearItem ChestBase = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Base);
            GearItem ChestMid = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Mid);
            GearItem ChestTop = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Top);
            GearItem ChestTop2 = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Chest, ClothingLayer.Top2);
            GearItem Gloves = GameManager.GetPlayerManagerComponent().GetClothingInSlot(ClothingRegion.Hands, ClothingLayer.Base);

            GearItem Shirt = ChestMid ? ChestMid : ChestBase;
            GearItem Jacket = ChestTop2 ? ChestTop2 : ChestTop;

            if (slot == Slot.Shirt) return Shirt;
            if (slot == Slot.Jacket) return Jacket;
            if (slot == Slot.Gloves) return Gloves;

            return null;
        }

        public static void UpdateClothingSlots()
        {
            bool changed = false;

            if (CCChecks.UpdateSlotIfNeeded(GetGearItemEquippedInSlot(Slot.Shirt), Slot.Shirt)) changed = true;
            if (CCChecks.UpdateSlotIfNeeded(GetGearItemEquippedInSlot(Slot.Jacket), Slot.Jacket)) changed = true;
            if (CCChecks.UpdateSlotIfNeeded(GetGearItemEquippedInSlot(Slot.Gloves), Slot.Gloves)) changed = true;
            if (changed) CCSetup.SmartUpdateOutfit();
        }

        public static IEnumerator DoEverything(Character character, int arg, int specificArg = 0)
        {
            // 1 get active player hands for mesh replacement
            if (arg >= 1 || specificArg == 1)
            {
                GameObject? isLoaded = null;
                CCMain.vanillaCharacter = GameManager.GetTopLevelCharacterFpsPlayer();

                while (isLoaded == null)
                {
                    isLoaded = GetVanillaHandsObject(character)[0];
                    Utility.Log(System.ConsoleColor.Gray, "Looking for vanilla hands...");
                    yield return new WaitForEndOfFrame();
                }

                Utility.Log(System.ConsoleColor.DarkCyan, $"1 - Vanilla arms mesh is loaded, character: {character}");

                if (specificArg == 1) yield break;
            }

            // 2 transfer bone components from vanilla arms to custom meshes
            if (arg >= 2 || specificArg == 2)
            {
                if (!currentPhysicsObject)
                {
                    currentPhysicsObject = GameObject.Instantiate(CCMain.physicsObject);
                    SetupPhysicsBones(currentPhysicsObject); // should be done before SetupCustomMesh
                }

                foreach (Dictionary<Slot, Equip> equipDict in Equipment.allEquipment[character].Values)
                {
                    foreach (Equip equip in equipDict.Values)
                    {
                        if (equip == null) continue;
                        if (equip.normalVariant) equip.normalVariant.active = false;
                        if (equip.maskedVariant) equip.maskedVariant.active = false;
                        if (equip.injuredVariant) equip.injuredVariant.active = false;

                        if (equip.slot == Slot.Gloves)
                        {
                            if (equip.specialFlag == SpecialFlag.Mittens || equip.specialFlag == SpecialFlag.LargeMittens || equip.specialFlag == SpecialFlag.LargeGloves)
                            {
                                if (equip.maskedVariantPrefab && !equip.maskedVariant) equip.maskedVariant = SetupCustomMesh(equip.maskedVariantPrefab, character, true, true);
                                if (equip.normalVariantPrefab && !equip.normalVariant) equip.normalVariant = SetupCustomMesh(equip.normalVariantPrefab, character, true, false);

                                continue;
                            }
                        }

                        if (equip.normalVariantPrefab && !equip.normalVariant) equip.normalVariant = SetupCustomMesh(equip.normalVariantPrefab, character, true, false);
                        if (equip.maskedVariantPrefab && !equip.maskedVariant) equip.maskedVariant = SetupCustomMesh(equip.maskedVariantPrefab, character, true, false);
                        if (equip.injuredVariantPrefab && !equip.injuredVariant) equip.injuredVariant = SetupCustomMesh(equip.injuredVariantPrefab, character, true, false);
                    }
                }

                SetClothesToDefault();
                SmartUpdateOutfit();
                HideVanillaHands(character, true);

                if (!Settings.options.specialEventOverride)
                {
                    if (Settings.options.displayProperClothes)
                    {
                        UpdateClothingSlots();
                    }
                }

                Utility.Log(System.ConsoleColor.DarkCyan, $"2 - Stole bone components to activate custom mesh rigs, character:{character}");

                if (specificArg == 2) yield break;
            }


            // 3 scan for custom textures
            if (arg >= 3 || specificArg == 3)
            {
                if (Settings.options.useCustomTextures)
                {
                    MelonCoroutines.Start(ApplyCustomTextures(character));

                    Utility.Log(System.ConsoleColor.DarkCyan, $"3 - Replaced textures with custom ones, character:{character}");
                }
                else
                {
                    //Equip[] equips = Equipment.GetAllEquipment(character);

                    Dictionary<Slot, Equip> equips = Equipment.allEquipment[character]["Default"];

                    foreach (Equip equip in equips.Values)
                    {
                        if (equip == null) continue;

                        Texture? tex = equip.normalVariantPrefab?.GetComponent<SkinnedMeshRenderer>()?.material.mainTexture;


                        if (tex != null)
                        {
                            if (equip.normalVariant)
                            {
                                foreach (GameObject child in equip.normalVariant.GetAllImmediateChildren())
                                {
                                    if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = tex;
                                }
                            }
                            if (equip.maskedVariant)
                            {
                                foreach (GameObject child in equip.maskedVariant.GetAllImmediateChildren())
                                {
                                    if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = tex;
                                }
                            }
                            if (equip.injuredVariant)
                            {
                                foreach (GameObject child in equip.injuredVariant.GetAllImmediateChildren())
                                {
                                    if (child.GetComponent<SkinnedMeshRenderer>()) child.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = tex;
                                }
                            }
                        }

                        /*
                        Texture tex = equip.normalVariantPrefab?.GetComponent<SkinnedMeshRenderer>().material.mainTexture;

                        if (tex)
                        {
                            if (equip.normalVariant) equip.normalVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                            if (equip.maskedVariant) equip.maskedVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                            if (equip.injuredVariant) equip.injuredVariant.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
                        }
                        */
                    }

                    Utility.Log(System.ConsoleColor.DarkCyan, $"3 - Reloaded textures from prefabs, character:{character}");
                }

                if (Settings.options.useTextureTint)
                {
                    TintTexture(Slot.Hands, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                    TintTexture(Slot.Arms, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                }

                if (specificArg == 3) yield break;
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
                Utility.Log(System.ConsoleColor.DarkCyan, "4 - Does nothing for now");
                

                if (specificArg == 4) yield break;
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

                Texture2D headTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D handsTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D feetTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D bodyTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);

                ImageConversion.LoadImage(headTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/F/head.png"));
                ImageConversion.LoadImage(handsTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/F/hands.png"));
                ImageConversion.LoadImage(feetTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/F/feet.png"));
                ImageConversion.LoadImage(bodyTex, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/F/body.png"));

                dollFHead.GetComponent<UITexture>().mainTexture = headTex;
                dollFHands.GetComponent<UITexture>().mainTexture = handsTex;
                dollFFeet.GetComponent<UITexture>().mainTexture = feetTex;
                dollFBody.GetComponent<UITexture>().mainTexture = bodyTex;

                Texture2D headTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D handsTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D feetTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D bodyTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);

                ImageConversion.LoadImage(headTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/M/head.png"));
                ImageConversion.LoadImage(handsTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/M/hands.png"));
                ImageConversion.LoadImage(feetTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/M/feet.png"));
                ImageConversion.LoadImage(bodyTex2, File.ReadAllBytes("Mods/" + CCMain.modFolderName + "paperDoll/M/body.png"));

                dollMHead.GetComponent<UITexture>().mainTexture = headTex2;
                dollMHands.GetComponent<UITexture>().mainTexture = handsTex2;
                dollMFeet.GetComponent<UITexture>().mainTexture = feetTex2;
                dollMBody.GetComponent<UITexture>().mainTexture = bodyTex2;

                Utility.Log(System.ConsoleColor.DarkCyan, "5 - Applied custom PAPERDOLL textures");

                if (specificArg == 5) yield break;
            }

            Utility.Log(System.ConsoleColor.DarkCyan, $"All operations complete, shutting down. Character: {character}");
            if (character == Character.Astrid) CCMain.allLoadCompleteAstrid = true;
            if (character == Character.Will) CCMain.allLoadCompleteWill = true;


        }

    }
}