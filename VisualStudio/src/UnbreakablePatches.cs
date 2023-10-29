using Il2Cpp;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppTLD.Gear;

namespace Personality
{
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
    }
}