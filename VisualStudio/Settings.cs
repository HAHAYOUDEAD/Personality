using System;
using System.IO;
using MelonLoader;
using ModSettings;
using UnityEngine;

namespace CharacterCustomizer
{
    internal static class Settings
    {
        public static CCSettings options;

        public static void OnLoad()
        {
            options = new CCSettings();
            options.AddToModSettings("Survivor Customizer Settings");
        }
    }

    internal class CCSettings : JsonModSettings
    {
        [Section("Mesh customization")]

        [Name("Display current clothes")]
        [Description("(this is not done at all, just testing things out)\n\nOverrides automatic outfit switch")]
        public bool displayProperClothes = false;

        [Name("Switch outfit for indoors/outdoors")]
        [Description("Put on the default character outfit when inside, dress up when going outside. \n\nOnly changes when arms are hidden to preserve immersion. \n\nDefault: true")]
        public bool undressIndoors = true;

        [Name("Show injures")]
        [Description("Show bandages when injured. Only visible on indoors clothes \n\nDefault: true")]
        public bool showInjuries = true;

        [Name("Default outfit")]
        [Description("How your character looks by default. Has no effect depending on previous options. \n\nDefault: Vanilla")]
        [Choice(new string[]
        {
            "Vanilla",
            "Full outfit",
            "Injured",
            "Undressed"
        })]
        public int defaultAppearance;

        [Name("Enable trinkets")]
        [Description("Add some custom trinkets to your character. \n\nDefault: false")]
        public bool enableTrinkets = false;



        [Section("Texture customization")]

        [Name("Use custom textures")]
        [Description("Load textures from .../Mods/characterCustomizer/customTextures/ \n\nCheck readme inside that folder for more info. \n\nDefault: false")]
        public bool useCustomTextures = false;

        [Name("Skin texture HUE")]
        [Description("Hue is limited to 10-35 degrees. I don't want you to make hulk ;) \n\nYeah yeah no fun\n\nLow value = red\n\nHigh value = yellow")]
        [Slider(10, 35)]
        public int skinTextureHue = 10;

        [Name("Skin texture SATuration")]
        [Description("Saturation is limited because I'm boring \n\nLow value = less saturated color\n\nHigh value = more saturated value")]
        [Slider(20, 50)]
        public int skinTextureSat = 20;

        [Name("Skin texture LUMinocity")]
        [Description("Luminocity is not limited\n\nLow value = black \n\nSetting this to 100 will make texture unchanged \n\nHigh value = white")]
        [Slider(0, 100)]
        public int skinTextureLum = 100;

        [Section("Debug settings")]

        [Name("Enable debug messages")]
        [Description("To help developer find the issue")]
        public bool debugLog = false;

        protected override void OnConfirm()
        {
            CCSetup.DoEverything(CCSetup.currentCharacter, 0, 3);

            CCSetup.TintTexture(Slot.Hands, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
            CCSetup.TintTexture(Slot.Arms, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);

            CCSetup.SmartUpdateOutfit();

            base.OnConfirm();
        }
    }
}
