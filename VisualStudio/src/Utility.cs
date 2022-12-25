using MelonLoader;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Il2CppSystem.Reflection;
using System.Collections;
using System.Collections.Generic;
using Il2Cpp;

namespace Personality
{
    public enum Character
    {
        Undefined,
        Astrid,
        Will,
        Marcene,
        Lincoln
    }

    public enum Slot
    {
        Hands,
        Gloves,
        Arms,
        Shirt,
        Jacket,
        Trinkets
    }

    public enum PartVariant
    {
        Undefined,
        Disabled,
        Normal,
        Undermask,
        Injured
    }

    public enum Outfit
    {
        Undefined,
        Undressed,
        Indoors,
        Outdoors,
        Injured,
        Custom
    }

    public enum SpecialFlag
    {
        None,
        Mittens,
        SleevelessJacket,
        LargeGloves,
        Teeshirt
    }

    public enum Environment
    {
        Indoors,
        Outdoors
    }

    public static class ClassExtension
    {
        public static List<GameObject> GetAllImmediateChildren(this GameObject Go)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < Go.transform.childCount; i++)
            {
                list.Add(Go.transform.GetChild(i).gameObject);
            }
            return list;
        }
    }



    public static class Utility
    {
        public static void Log(ConsoleColor color, string message)
        {
            if (Settings.options.debugLog)
            {
                MelonLogger.Msg(color, message);
            }
        }

        public static bool IsScenePlayable()
        {
            return !(string.IsNullOrEmpty(GameManager.m_ActiveScene) || GameManager.m_ActiveScene.Contains("MainMenu") || GameManager.m_ActiveScene == "Boot" || GameManager.m_ActiveScene == "Empty");
        }

        public static bool IsScenePlayable(string scene)
        {
            return !(string.IsNullOrEmpty(scene) || scene.Contains("MainMenu") || scene == "Boot" || scene == "Empty");
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Il2CppSystem.Type typeT = comp.GetIl2CppType();
            if (typeT != other.GetIl2CppType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = typeT.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = typeT.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }



        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }




        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        /*
        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }
        */

        public static Transform FindDeepChild(Transform parent, string childName)
        {

            foreach (Transform g in parent.GetComponentsInChildren<Transform>())
            {
                if (g.name == childName) return g;
            }

            return null;
        }

        public static List<Transform> GetAllChildrenRecursive(Transform parent)
        {
            List<Transform> list = new List<Transform>();

            foreach (Transform g in parent.GetComponentsInChildren<Transform>())
            {
                list.Add(g);
            }

            return list;
        }


        public static Color HslToRgb(float h, float s, float l)
        {
            double red, green, blue;

            if (Math.Abs(s - 0.0) < double.Epsilon)
            {
                red = l;
                green = l;
                blue = l;
            }
            else
            {
                double var2;

                if (l < 0.5)
                {
                    var2 = l * (1.0 + s);
                }
                else
                {
                    var2 = l + s - s * l;
                }

                var var1 = 2.0 * l - var2;

                red = hue2Rgb(var1, var2, h + 1.0 / 3.0);
                green = hue2Rgb(var1, var2, h);
                blue = hue2Rgb(var1, var2, h - 1.0 / 3.0);
            }

            var nRed = Convert.ToInt32(red * 255.0);
            var nGreen = Convert.ToInt32(green * 255.0);
            var nBlue = Convert.ToInt32(blue * 255.0);

            return new Color((float)red, (float)green, (float)blue, 1f);
        }

        private static double hue2Rgb(double v1, double v2, double vH)
        {
            if (vH < 0.0)
            {
                vH += 1.0;
            }
            if (vH > 1.0)
            {
                vH -= 1.0;
            }
            if (6.0 * vH < 1.0)
            {
                return v1 + (v2 - v1) * 6.0 * vH;
            }
            if (2.0 * vH < 1.0)
            {
                return v2;
            }
            if (3.0 * vH < 2.0)
            {
                return v1 + (v2 - v1) * (2.0 / 3.0 - vH) * 6.0;
            }

            return v1;
        }
    }
}