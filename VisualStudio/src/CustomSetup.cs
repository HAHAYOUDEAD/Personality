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
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using System.Net.NetworkInformation;
using static System.Net.Mime.MediaTypeNames;

namespace Personality
{
    public class CCSetup : MelonMod
    {
        // variables
        //public static Character currentCharacter = Character.Undefined;

        public static Outfit currentMeshSet = Outfit.Undefined;

        public static GameObject currentPhysicsObject;
        public static Dictionary<string, GameObject> currentCustomBones = new Dictionary<string, GameObject>();

        private static readonly int renderLayer = LayerMask.NameToLayer("Weapon");

        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;

        public static bool currentCustomMeshIsValid;



        // vanilla mesh operations
        public static GameObject[] GetVanillaHandsObject(Character character)
        {

            if (character == Character.Will || character == Character.Lincoln)
            {
                CCMain.vanillaMaleHandsMesh = new GameObject[] { CCMain.vanillaClothingComponent.m_BaseMaleArmsPrefab.GetOrLoadAsset(), CCMain.vanillaClothingComponent.m_BaseMaleHandsPrefab.GetOrLoadAsset() };
                return CCMain.vanillaMaleHandsMesh;
            }

            if (character == Character.Astrid || character == Character.Marcene)
            {
                CCMain.vanillaFemaleHandsMesh = new GameObject[] { CCMain.vanillaClothingComponent.m_BaseMaleArmsPrefab.GetOrLoadAsset(), CCMain.vanillaClothingComponent.m_BaseMaleHandsPrefab.GetOrLoadAsset() };
                return CCMain.vanillaFemaleHandsMesh;
            }

            return null;
        }


        public static void HideVanillaHands(bool hideVanillaHands) => CCMain.vanillaClothingComponent.m_IsVisible = !hideVanillaHands;

        public static Character getCurrentChar()
        {
            if (!CCMain.startLoading) return Character.Undefined;

            if (PlayerManager.m_VoicePersona == VoicePersona.Female)
            {
                return Character.Astrid;
            }
            if (PlayerManager.m_VoicePersona == VoicePersona.Male)
            {
                return Character.Will;
            }

            return Character.Undefined;
        }
        
        // custom mesh operations
        public static GameObject SetupCustomMesh(GameObject go, bool useCustomBones = false)
        {
            Utility.Log(System.ConsoleColor.Gray, "SetupCustomMesh - Start");
            go.active = false;


            Character character = getCurrentChar();

            GameObject tempGo;
            GameObject vanillaHands = GetVanillaHandsObject(character)?[0];

            Utility.Log(System.ConsoleColor.Yellow, "SetupCustomMesh - character: " + character);

            if (!vanillaHands)
            {
                Utility.Log(System.ConsoleColor.Yellow, "SetupCustomMesh - Couldn't grab vanilla hands");
                return go;
            }

            KillCustomMeshes();

            tempGo = GameObject.Instantiate(go);
            tempGo.name = go.name;

            tempGo.transform.SetParent(CCMain.vanillaClothingComponent.m_ClothingParent.transform);

            GameObject meshObject = tempGo.transform.FindDeepChild("Meshes")?.gameObject;

            Transform? rootbone = null;

            int properMeshCount = 0;

            if (meshObject)
            {
                foreach (SkinnedMeshRenderer r in tempGo.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    //child.gameObject.name = go.name;
                    r.gameObject.layer = renderLayer;
                    foreach (Material m in r.materials)
                    {
                        m.shader = vanillaSkinnedShader;
                    }
                    

                    r.updateWhenOffscreen = true;
                    r.castShadows = false;

                    if (!rootbone) rootbone = r.rootBone;

                    if (r.name.EndsWith("-GLOVE") || r.name.EndsWith("-SLEEVE")) properMeshCount += 1;
                }
            }
            else
            {
                Utility.Log(System.ConsoleColor.Yellow, $"SetupCustomMesh - Couldn't find mesh object for {tempGo.name}");
                return tempGo;
            }

            FirstPersonClothing fpc = tempGo.AddComponent<FirstPersonClothing>();               

            fpc.Initialize(null, CCMain.vanillaClothingComponent.m_SkinningReference, rootbone);

            if (properMeshCount > 0) currentCustomMeshIsValid = true;
            else currentCustomMeshIsValid = false;

            Utility.Log(System.ConsoleColor.DarkYellow, "SetupCustomMesh - Done");
            return tempGo;


        }


        public static void KillCustomMeshes()
        {
            GameObject[] gos = GetCurrentCustomHands();

            for (int i = gos.Length - 1; i >= 0; i--)
            {
                GameObject.Destroy(gos[i]);
            }
        }
        /*
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
        */

        /*
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
        */

        public static GameObject[] GetCurrentVanillaHands(bool prefabs = false)
        {
            List<GameObject> list = new List<GameObject>();

            GameObject hands = getCurrentChar() == Character.Astrid || getCurrentChar() == Character.Marcene ? CCMain.vanillaClothingComponent.m_BaseFemaleHandsPrefab.GetOrLoadAsset() : CCMain.vanillaClothingComponent.m_BaseMaleHandsPrefab.GetOrLoadAsset();
            GameObject arms = getCurrentChar() == Character.Astrid || getCurrentChar() == Character.Marcene ? CCMain.vanillaClothingComponent.m_BaseFemaleArmsPrefab.GetOrLoadAsset() : CCMain.vanillaClothingComponent.m_BaseMaleArmsPrefab.GetOrLoadAsset();
            
            if (prefabs)
            {
                List<GameObject> list2 = new List<GameObject>() { hands, arms };
                return list2.ToArray();
            }

            foreach (GameObject go in CCMain.vanillaClothingComponent.m_ClothingParent.GetAllImmediateChildren())
            {
                if (go.name.Contains(hands.name) || go.name.Contains(arms.name)) list.Add(go);
            }

            return list.ToArray();
        }

        public static GameObject[] GetCurrentCustomHands()
        {
            List<GameObject> list = new List<GameObject>();

            foreach (GameObject go in CCMain.vanillaClothingComponent.m_ClothingParent.GetAllImmediateChildren())
            {
                if (go.name.StartsWith("Personality_")) list.Add(go);
            }

            return list.ToArray();
        }

        public static Texture2D? TryGrabCustomTextureFromFolder(string folder, string name)
        {
            Texture2D tex = new Texture2D(2, 2) { name = name };

            byte[] file = null;

            try
            {
                file = File.ReadAllBytes("Mods/" + folder + name + ".png");
            }
            catch (Exception)
            {
                Utility.Log(System.ConsoleColor.DarkGray, $"ApplyCustomTextures - could not find {name} texture, reverting to vanilla");
                return null;

            }
            if (file != null)
            {
                ImageConversion.LoadImage(tex, file);
                Utility.Log(System.ConsoleColor.DarkCyan, $"ApplyCustomTextures - loaded custom texture: {name}");
                return tex;
            }

            return null;
        }

        public static void ChangeGearTexture(bool loadCustomFromFolder = false)
        {
            string d = "Mods/" + CCMain.gearTextureFolderName;
            DirectoryInfo di = new DirectoryInfo(d);
            string fn;

            
           
            foreach (var file in di.GetFiles("*.png"))
            {
                fn = file.Name.StartsWith("GEAR_") ? file.Name : "GEAR_" + file.Name; // add suffix
                string fnTrunc = fn[0..^4]; // trim last 4 chars
                GearItem gi = GearItem.LoadGearItemPrefab(fnTrunc);
                
                if (!gi) continue;

                ClothingItem ci = gi.GetComponent<ClothingItem>();
                GameObject fphF = ci.m_FirstPersonPrefabFemale.GetOrLoadAsset();
                GameObject fphM = ci.m_FirstPersonPrefabMale.GetOrLoadAsset();

                if (loadCustomFromFolder)
                {
                    Texture2D tex = TryGrabCustomTextureFromFolder(CCMain.gearTextureFolderName, fnTrunc);
                    Texture2D? texDmg = TryGrabCustomTextureFromFolder(CCMain.gearTextureFolderName, fnTrunc + "_dmg");

                    
                    if (ci)
                    {
                        // first person textures
                        // "_dmg_texture"
                        
                        foreach (SkinnedMeshRenderer smr in fphF.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.material.SetTexture("_MainTex", tex);
                            if (texDmg) smr.material.SetTexture("_dmg_texture", texDmg);
                            else smr.material.SetTexture("_dmg_texture", tex);
                        }

                        foreach (SkinnedMeshRenderer smr in fphM.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.material.SetTexture("_MainTex", tex);
                            if (texDmg) smr.material.SetTexture("_dmg_texture", texDmg);
                            else smr.material.SetTexture("_dmg_texture", tex);
                        }

                        // inspect textures
                        /*
                        foreach (MeshRenderer mr in gi.GetComponentsInChildren<MeshRenderer>())
                        {
                            mr.sharedMaterial.mainTexture = tex;
                        }
                        */
                    }
                    /*
                    else
                    {
                        foreach (MeshRenderer mr in gi.GetComponentsInChildren<MeshRenderer>())
                        {
                            mr.sharedMaterial.mainTexture = tex;
                        }
                    }
                    */
                }
                else
                {
                    Texture tex = gi.GetComponent<Inspect>().m_NormalMesh.GetComponent<MeshRenderer>().material.mainTexture;
                    Texture texDmg = gi.GetComponent<Inspect>().m_NormalMesh.GetComponent<MeshRenderer>().material.GetTexture("_dmg_texture");


                    foreach (SkinnedMeshRenderer smr in fphF.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        smr.material.SetTexture("_MainTex", tex);
                        if (texDmg) smr.material.SetTexture("_dmg_texture", texDmg);
                        else smr.material.SetTexture("_dmg_texture", tex);
                    }

                    foreach (SkinnedMeshRenderer smr in fphM.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        smr.material.SetTexture("_MainTex", tex);
                        if (texDmg) smr.material.SetTexture("_dmg_texture", texDmg);
                        else smr.material.SetTexture("_dmg_texture", tex);
                    }
                }
            }
        }
            
        // texture operations
        public static void ChangeHandsTexture(bool loadCustomFromFolder = false)
        {
            string suffix = getCurrentChar() == Character.Astrid || getCurrentChar() == Character.Marcene ? "_F" : "_M";
            string texName = "Arms" + suffix;
            string vanillaTexName = "FPH_BareHands" + suffix;
            //string vanillaTexName = "Arms" + suffix;


            Texture2D newTex = new Texture2D(2, 2) { name = texName };
            Texture vanillaTex = new Texture();

            if (!loadCustomFromFolder)
            {
                vanillaTex = CCMain.everythingBundle.LoadAsset<Texture>("Assets/Tex/FPH_BareHands" + suffix + ".png");

            }
            else 
            {
                if (TryGrabCustomTextureFromFolder(CCMain.textureFolderName, texName))
                {
                    newTex = TryGrabCustomTextureFromFolder(CCMain.textureFolderName, texName);
                }
            }

            foreach (GameObject go in GetCurrentVanillaHands(true)) // prefabs
            {
                foreach (Renderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.sharedMaterial.mainTexture = loadCustomFromFolder ? newTex : vanillaTex;
                }
            }


            foreach (GameObject go in GetCurrentVanillaHands(false)) // instances
            {
                foreach (Renderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.sharedMaterial.mainTexture = loadCustomFromFolder ? newTex : vanillaTex;
                }
            }

            foreach (GameObject go in GetCurrentCustomHands())
            {
                foreach (Renderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    foreach (Material m in r.materials)
                    {
                        if (m.mainTexture?.name == texName || m.mainTexture?.name == vanillaTexName)
                        {
                            m.mainTexture = loadCustomFromFolder ? newTex : vanillaTex;
                        }
                        else
                        {
                            if (loadCustomFromFolder && TryGrabCustomTextureFromFolder(CCMain.textureFolderName, m.mainTexture?.name))
                            {
                                m.mainTexture = TryGrabCustomTextureFromFolder(CCMain.textureFolderName, m.mainTexture?.name);
                            }
                        }
                    }
                }
            }
        }

        public static void TintTexture(int H, int S, int L)
        {
            Color color = Utility.HslToRgb(H / 360f, S / 100f, L / 100f);

            foreach (GameObject go in GetCurrentVanillaHands(true)) // prefabs
            {
                foreach (Renderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.sharedMaterial.color = color;
                }
            }



            foreach (GameObject go in GetCurrentVanillaHands(false)) // instances
            {
                foreach (Renderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.sharedMaterial.color = color;
                }
            }

            string suffix = getCurrentChar() == Character.Astrid || getCurrentChar() == Character.Marcene ? "_F" : "_M";
            string texName = "Arms" + suffix;
            string vanillaTexName = "FPH_BareHands" + suffix;

            foreach (GameObject go in GetCurrentCustomHands())
            {
                foreach (Renderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    foreach (Material m in r.materials)
                    {
                        if (m.mainTexture?.name == texName || m.mainTexture?.name == vanillaTexName)
                        {
                            m.color = color;
                        }
                    }
                }
            }


            Utility.Log(System.ConsoleColor.Cyan, "TintTexture - Done. Changed texture to " + color.ToString());
        }

        /*
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
        */

        public static void OverrideOutfitForEvent(bool enable = true)
        {

            //Equipment.ToggleTrinkets(false);
            string suffix = "";
            GameObject go = null;

            if (getCurrentChar() == Character.Astrid) suffix = "_F";
            if (getCurrentChar() == Character.Will) suffix = "_M";
            //Equipment.ToggleBandages(false);

            switch (Settings.options.specialEventOutfit)
            {
                case 0: //Tribute
                    go = CCMain.LoadAndSetupFromBundle(CCMain.everythingBundle, CCMain.modAssetPrefix + "Tribute" + suffix);
                    break;
                case 1: //Halloween 2023
                    go = CCMain.LoadAndSetupFromBundle(CCMain.everythingBundle, CCMain.modAssetPrefix + "Halloween2023" + suffix);
                    break;
                case 2:
                    go = CCMain.LoadAndSetupFromBundle(CCMain.customBundle, CCMain.modAssetPrefix + "CustomOutfit" + suffix);
                    break;
                   
            }
           

            if (enable) SetupCustomMesh(go);
            else KillCustomMeshes();
        }

        /*
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
        */

        /*
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
        */

        /*
        public static void SetClothesToDefault()
        {
            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
            {
                Equipment.ChangeEquipIfExistsOtherwiseDefault(slot, "Default");
            }
        }
        */


        
        public static void SmartUpdateOutfit()
        {
            /*
            Utility.Log(System.ConsoleColor.Gray, "UpdateVisibility - Start");


            if (Settings.options.specialEventOverride)
            {
                OverrideOutfitForEvent();
                return;
            }


            // toggle trinkets
            Equipment.ToggleTrinkets(Settings.options.enableTrinkets);

            
            //if (Settings.options.displayProperClothes)
            //{
            //    currentMeshSet = Outfit.Custom;
            //    AutoSwitchMeshVariant();
            //    
            //    Utility.Log(System.ConsoleColor.DarkYellow, $"UpdateVisibility - Done for {currentMeshSet} outfit");
            //    return;
            //}
            
            

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
        */
        }

        /*
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
        */

        /*
        public static void UpdateClothingSlots()
        {
            bool changed = false;

            if (CCChecks.UpdateSlotIfNeeded(GetGearItemEquippedInSlot(Slot.Shirt), Slot.Shirt)) changed = true;
            if (CCChecks.UpdateSlotIfNeeded(GetGearItemEquippedInSlot(Slot.Jacket), Slot.Jacket)) changed = true;
            if (CCChecks.UpdateSlotIfNeeded(GetGearItemEquippedInSlot(Slot.Gloves), Slot.Gloves)) changed = true;
            if (changed) CCSetup.SmartUpdateOutfit();
        }
        */

        public static IEnumerator DoEverything(int arg, int specificArg = 0)
        {
            Character character = getCurrentChar();

            // 1 get active player hands for mesh replacement
            if (arg >= 1 || specificArg == 1)
            {
                GameObject? isLoaded = null;

                CCMain.vanillaCharacter = GameManager.GetTopLevelCharacterFpsPlayer();


                while (isLoaded == null)
                {
                    
                    while (!CCMain.vanillaClothingComponent)
                    {
                        Utility.Log(System.ConsoleColor.Gray, "Looking for vanilla hands...");
                        CCMain.vanillaClothingComponent = CCMain.vanillaCharacter.transform.Find("NEW_FPHand_Rig")?.GetComponent<ClothingSpawner>();
                        CCMain.vanillaClothingComponent?.gameObject.GetOrAddComponent<Chirality>();
                        yield return new WaitForEndOfFrame();
                    }
                    isLoaded = GetVanillaHandsObject(character)[0];
                    
                    yield return new WaitForEndOfFrame();
                }



                Utility.Log(System.ConsoleColor.DarkCyan, $"1 - Vanilla arms mesh is loaded, character: {character}");

                if (specificArg == 1) yield break;
            }

            
            // 2 transfer bone components from vanilla arms to custom meshes
            if (arg >= 2 || specificArg == 2)
            {
                /*
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


                */
                
                OverrideOutfitForEvent(Settings.options.specialEventOverride);
                HideVanillaHands(!Settings.options.specialEventOverride);

                Utility.Log(System.ConsoleColor.DarkCyan, $"2 - Stole bone components to activate custom mesh rigs, character:{character}");

                if (specificArg == 2) yield break;
            }
            

                // 3 scan for custom textures
                if (arg >= 3 || specificArg == 3)
            {
                if (Settings.options.useCustomTextures)
                {
                    ChangeHandsTexture(true);
                    ChangeGearTexture(true);

                    Utility.Log(System.ConsoleColor.DarkCyan, $"3 - Replaced textures with custom ones, character:{character}");
                }
                else
                {
                    ChangeHandsTexture(false);
                    ChangeGearTexture(false);

                    Utility.Log(System.ConsoleColor.DarkCyan, $"3 - Restored textures to vanilla, character:{character}");
                }

               

                if (Settings.options.useTextureTint)
                {
                    TintTexture(Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                    TintTexture(Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                }

                if (specificArg == 3) yield break;
            }

            // 4 apply custom textures to vanilla arms for mods that don't use vanilla rig (binoculars, pastime reading)

            /*
            if (arg >= 4 || specificArg == 4)
            {
                
                Texture2D texF = new Texture2D(2, 2);
                ImageConversion.LoadImage(texF, File.ReadAllBytes("Mods/characterCustomizer/handsF.png"));
                CCMain.vanillaFemaleHandsMesh.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = texF;

                Texture2D texM = new Texture2D(2, 2);
                ImageConversion.LoadImage(texM, File.ReadAllBytes("Mods/characterCustomizer/handsM.png"));
                CCMain.vanillaMaleHandsMesh.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = texM;

                
            Utility.Log(System.ConsoleColor.DarkCyan, "4 - Does nothing for now");
                

                if (specificArg == 4) yield break;
            }
            */

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

                ImageConversion.LoadImage(headTex, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "F/head.png"));
                ImageConversion.LoadImage(handsTex, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "F/hands.png"));
                ImageConversion.LoadImage(feetTex, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "F/feet.png"));
                ImageConversion.LoadImage(bodyTex, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "F/body.png"));

                dollFHead.GetComponent<UITexture>().mainTexture = headTex;
                dollFHands.GetComponent<UITexture>().mainTexture = handsTex;
                dollFFeet.GetComponent<UITexture>().mainTexture = feetTex;
                dollFBody.GetComponent<UITexture>().mainTexture = bodyTex;

                Texture2D headTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D handsTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D feetTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                Texture2D bodyTex2 = new Texture2D(2, 2, TextureFormat.RGBA4444, false);

                ImageConversion.LoadImage(headTex2, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "M/head.png"));
                ImageConversion.LoadImage(handsTex2, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "M/hands.png"));
                ImageConversion.LoadImage(feetTex2, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "M/feet.png"));
                ImageConversion.LoadImage(bodyTex2, File.ReadAllBytes("Mods/" + CCMain.paperDollFolderName + "M/body.png"));

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