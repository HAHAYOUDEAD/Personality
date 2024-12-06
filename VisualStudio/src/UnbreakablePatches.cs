using Il2Cpp;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppTLD.Gear;
using UnityEngine.AddressableAssets;

namespace Personality
{
    /*
    [HarmonyPatch(typeof(vp_FPSWeapon), nameof(vp_FPSWeapon.UpdateZoom))]
    public class ChangeWeaponCameraFov
    {
        public static void Postfix(vp_FPSWeapon __instance)
        {

            switch (Settings.options.weaponCameraFov)
            {
                case 0:
                    return;
                case 1:
                    __instance.m_WeaponCamera.GetComponent<Camera>().fieldOfView *= 1.1f;
                    break;
                case 2:
                    __instance.m_WeaponCamera.GetComponent<Camera>().fieldOfView *= 1.2f;
                    break;
                case 3:
                    __instance.m_WeaponCamera.GetComponent<Camera>().fieldOfView *= 1.3f;
                    break;
                case 4:
                    __instance.m_WeaponCamera.GetComponent<Camera>().fieldOfView *= 1.4f;
                    break;
            }
        }
        
    }   */
    [HarmonyPatch(typeof(Il2Cpp.Utils), nameof(Il2Cpp.Utils.SetCameraFOVSafe))]
    public class ChangeWeaponCameraFov
    {
        public static void Prefix(ref Camera cam, ref float fov)
        {
            if (cam != GameManager.GetWeaponCamera()) return;

            switch (Settings.options.weaponCameraFov)
            {
                case 0:
                    return;
                case 1:
                    fov *= 1.1f;
                    break;
                case 2:
                    fov *= 1.2f;
                    break;
                case 3:
                    fov *= 1.3f;
                    break;
                case 4:
                    fov *= 1.4f;
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(ClothingSpawner), nameof(ClothingSpawner.Update))]
    public class TempHandsVisibilityPatch
    {
        public static bool Prefix(ClothingSpawner __instance)
        {
            if (!CCMain.startLoading) return true;

            bool flag = Settings.options.specialEventOverride && (!Settings.options.forceDisplayClothes || !CCSetup.currentCustomMeshIsValid);

            __instance.EnableActiveClothing(!flag); //if (CCMain.vanillaClothingComponent?.m_ClothingParent?.transform?.GetChild(0)?.gameObject.active == true) 
            return !flag;
        }

        public static void Postfix(ClothingSpawner __instance) // move to less frequent method
        {
            if (!CCMain.startLoading) return;

            if (Settings.options.hideGloves)
            {
                CCMain.vanillaClothingComponent.ForceDefaultClothing(false, true);
            }
            else
            {
                CCMain.vanillaClothingComponent.ForceDefaultClothing(false, false);
            }

            if (Settings.options.specialEventOverride)
            {
                if (Settings.options.forceDisplayClothes)
                {
                    GameObject hands = CCMain.vanillaClothingComponent.m_WornHands?.m_Clothing?.gameObject;
                    GameObject arms = CCMain.vanillaClothingComponent.m_WornArms?.m_Clothing?.gameObject;
                    
                    List<GameObject> gloves = new List<GameObject>();
                    List<GameObject> sleeves = new List<GameObject>();

                    if (CCSetup.currentCustomMeshIsValid)
                    {
                        GameObject[] customMeshes = CCSetup.GetCurrentCustomHands();
                        foreach (GameObject go in customMeshes)
                        {
                            foreach (SkinnedMeshRenderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                            {
                                if (r.name.EndsWith("-GLOVE")) gloves.Add(r.gameObject);
                                if (r.name.EndsWith("-SLEEVE")) sleeves.Add(r.gameObject);
                            }
                        }
                        if (hands?.name.Contains("BareHands") == true)
                        {
                            hands.active = false;

                            if (gloves.Count > 0) foreach (GameObject go in gloves) go.active = true;
                        }
                        else
                        {
                            hands.active = true;

                            if (gloves.Count > 0) foreach (GameObject go in gloves) go.active = false;
                        }

                        if (arms?.name.Contains("BareArms") == true)
                        {
                            arms.active = false;

                            if (sleeves.Count > 0) foreach (GameObject go in sleeves) go.active = true;
                        }
                        else
                        {
                            arms.active = true;

                            if (sleeves.Count > 0) foreach (GameObject go in sleeves) go.active = false;
                        }
                    }


                }
            }
            


        }
    }

    /*
    [HarmonyPatch(typeof(ClothingSpawner), nameof(ClothingSpawner.UpdateClothingForRegion))]
    public class HideGloves
    {

        public static void Prefix(ClothingSpawner __instance, ref ClothingRegion region, ref bool forceDefault)
        {
            if (!CCMain.startLoading) return;
            if (Settings.options.hideGloves) forceDefault = true;
        }
    }
    */

    [HarmonyPatch(typeof(ClothingItem), nameof(ClothingItem.Awake))]
    public class ModClothingAwake
    {
        public static void Prefix(ClothingItem __instance)
        {
            if (__instance.name.ToLower().Contains("deerskingloves"))
            {
                //MelonLogger.Msg("found gloves");
                __instance.m_FirstPersonPrefabMale = new AssetReferenceFirstPersonClothing("Assets/FPH_deerSkinGloves_M.prefab");
                __instance.m_FirstPersonPrefabFemale = new AssetReferenceFirstPersonClothing("Assets/FPH_deerSkinGloves_M.prefab");
            }
        }
    }
}