using ModSettings;
using System.Reflection;
using Il2Cpp;
using MelonLoader;


namespace Personality
{
    internal static class Settings
    {
        public static CCSettings options;

        public static void OnLoad()
        {
            options = new CCSettings();
            options.AddToModSettings("Personality", MenuType.InGameOnly);
            ExpandTextureColoring(options.useTextureTint);
            HideEverythingWhenSpecialOverride(options.specialEventOverride);
        }

        internal static void ExpandTextureColoring(bool visible)
        {
            options.SetFieldVisible(nameof(options.skinTextureHue), visible);
            options.SetFieldVisible(nameof(options.skinTextureSat), visible);
            options.SetFieldVisible(nameof(options.skinTextureLum), visible);
        }


        internal static void HideEverythingWhenSpecialOverride(bool visible)
        {
            options.SetFieldVisible(nameof(options.specialEventOutfit), visible);
            options.SetFieldVisible(nameof(options.forceDisplayClothes), visible);

            //options.SetFieldVisible(nameof(options.selectedCharacter), !visible);
            //options.SetFieldVisible(nameof(options.dynamicOutfit), !visible);
            //options.SetFieldVisible(nameof(options.displayProperClothes), !visible);
            //options.SetFieldVisible(nameof(options.mittensAppearance), !visible);
            //options.SetFieldVisible(nameof(options.defaultAppearance), !visible);
            //options.SetFieldVisible(nameof(options.enableTrinkets), !visible);
            //options.SetFieldVisible(nameof(options.useCustomTextures), !visible);
            //options.SetFieldVisible(nameof(options.useTextureTint), !visible);
            //options.SetFieldVisible(nameof(options.skinTextureHue), !visible);
            //options.SetFieldVisible(nameof(options.skinTextureSat), !visible);
            //options.SetFieldVisible(nameof(options.skinTextureLum), !visible);
            //options.SetFieldVisible(nameof(options.reloadTextures), !visible);

        }

        
    }

    internal class CCSettings : JsonModSettings
    {
        [Section("Specials")]
        [Name("Enable override")]
        [Description("Turn on a special outfit override, for thematic events or whatever else I came up with \n\nDefault: false")]
        public bool specialEventOverride = false;

        [Name("Special outfit")]
        [Description("Select the occasion \n\nTribute - classic Will and Astrid outfits\nHalloween 2023 - Plague Witch costume for Astrid and Pirate Skeleton for Will\nCustom - will load your custom model from '.../Mods/personality/custom' bundle")]
        [Choice(new string[]
        {
            "Tribute",
            "Halloween 2023",
            "Custom"
        })]
        public int specialEventOutfit;

        [Name("Keep clothes")]
        [Description("Keep clothes when override is enabled. Only works for supported meshes. \n\nDefault: false")]
        public bool forceDisplayClothes = false;

        //[Section("Customization")]
        /*
        [Name("Character")]
        [Description("You can change your survivor here")]
        [Choice(new string[]
        {
            "Astrid",
            "Will"
        })]
        public int selectedCharacter;
        */
        /*
        [Name("Left-handed")]
        [Description("Make your character left-handed. \n\nDefault: false")]
        public bool leftHanded = false;
        */

        [Section("Texture customization")]

        [Name("Use custom textures")]
        [Description("Load textures from .../Mods/characterCustomizer/customTextures/ \n\nCheck readme inside that folder for more info. \n\nDefault: false")]
        public bool useCustomTextures = false;

        [Name("Tint textures")]
        [Description("Customize textures by tinting them with HSL values \n\nDefault: false")]
        public bool useTextureTint = false;

        [Name("Skin texture Hue")]
        [Description("Hue is limited to 10-30 degrees, which is more or less in human skin tone range \n\nLow value = red\n\nHigh value = yellow")]
        [Slider(10, 30)]
        public int skinTextureHue = 10;

        [Name("Skin texture Saturation")]
        [Description("Saturation is limited because I'm boring \n\nLow value = less saturated color\n\nHigh value = more saturated value")]
        [Slider(20, 50)]
        public int skinTextureSat = 20;

        [Name("Skin texture Luminocity")]
        [Description("Luminocity is a bit limited as well\n\nLow value = dark \n\nHigh value = light \n\nSetting this to 100 will make texture unchanged ")]
        [Slider(10, 100)]
        public int skinTextureLum = 100;

        [Name("Reload textures")]
        [Description("Tick this to reload textures from files")]
        public bool reloadTextures = false;

        [Section("Misc settings")]

        [Name("Left-handed")]
        [Description("Make your character left-handed. Experimental feature")]
        public bool leftHanded = false;

        [Name("Hands Field of View")]
        [Description("NOT regular camera FOV, this lets you see more of your character's hands.\n\nNot intended to be changed like this, so some animations might look off")]
        [Choice(new string[]
        {
            "Default",
            "+10%",
            "+20%",
            "+30%",
            "Quake"
        })]
        public int weaponCameraFov;

        [Name("Hide gloves")]
        [Description("Keep bare hands, doesn't affect the actual item being worn, it's only visual")]
        public bool hideGloves = false;

        [Section("Debug/incomplete stuff")]

        [Name("Enable debug messages")]
        [Description("To help developer find the issue")]
        public bool debugLog = false;

        /*
        [Name("WIP ! Dynamic outfit")]
        [Description("Take off jacket and gloves when indoors. \n\nOnly changes when arms are hidden to preserve immersion. \n\nDoesn't actually remove clothes, only visually \n\nDefault: false")]
        public bool dynamicOutfit = false;

        [Name("WIP ! Mittens")]
        [Description("How do you like your mittens?\n\n" +
            "Always on - vanilla\n\n" +
            "Dynamic - take one mitten off when need free hand\n\n" +
            "Dynamic and fun - dangle from a string when need free hand. Works when you have a jacket on, otherwise you just take it off\n\n" +
            "Default: Dynamic and fun")]
        [Choice(new string[]
        {
            "Always on",
            "Dynamic",
            "Dynamic and fun"
        })]
        public int mittensAppearance = 2;

        [Name("WIP ! Enable trinkets")]
        [Description("Show trinkets on character's hands. \n\nDefault: false")]
        public bool enableTrinkets = false;
        */


        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            if (field.Name == nameof(useTextureTint)) Settings.ExpandTextureColoring((bool)newValue);
            if (field.Name == nameof(specialEventOverride))
            {
                Settings.HideEverythingWhenSpecialOverride((bool)newValue);
                base.OnConfirm();
            }
        }

        protected override void OnConfirm()
        {
            /*
            switch (Settings.options.selectedCharacter)
            {
                case 0: // Astrid
                    //InterfaceManager.GetPanel<Panel_OptionsMenu>().m_State.m_VoicePersona = VoicePersona.Female;
                    if (CCSetup.currentCharacter != Character.Astrid)
                    {
                        PlayerManager.m_VoicePersona = VoicePersona.Female;
                        GameManager.GetPlayerVoiceComponent().SetPlayerVoicePersona();
                        if (CCMain.mainCoroutine != null) MelonCoroutines.Stop(CCMain.mainCoroutine);
                        CCMain.mainCoroutine = MelonCoroutines.Start(CCSetup.DoEverything(CCSetup.currentCharacter, 5));
                    }
                    break;
                case 1: // Will
                    //InterfaceManager.GetPanel<Panel_OptionsMenu>().m_State.m_VoicePersona = VoicePersona.Male;
                    if (CCSetup.currentCharacter != Character.Will)
                    {
                        PlayerManager.m_VoicePersona = VoicePersona.Male;
                        GameManager.GetPlayerVoiceComponent().SetPlayerVoicePersona();
                        if (CCMain.mainCoroutine != null) MelonCoroutines.Stop(CCMain.mainCoroutine);
                        CCMain.mainCoroutine = MelonCoroutines.Start(CCSetup.DoEverything(CCSetup.currentCharacter, 5));
                    }

                    break;
            }
            */

            if (Settings.options.specialEventOverride)
            {
                CCSetup.OverrideOutfitForEvent();
            }
            else
            {
                CCSetup.KillCustomMeshes();
            }

            if (Settings.options.reloadTextures)
            {
                MelonCoroutines.Start(CCSetup.DoEverything(0, 3));
                MelonCoroutines.Start(CCSetup.DoEverything(0, 5));
                Settings.options.reloadTextures = false;
            }
            else
            {
                CCSetup.ChangeHandsTexture(Settings.options.useCustomTextures);
                CCSetup.ChangeGearTexture(Settings.options.useCustomTextures);
            }


            if (Settings.options.useTextureTint)
            {
                CCSetup.TintTexture(Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                CCSetup.TintTexture(Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
            }
            else
            {
                CCSetup.TintTexture(0, 0, 100);
                CCSetup.TintTexture(0, 0, 100);
            }

            //CCMain.characterChirality.isLeftHanded = Settings.options.leftHanded;


            
            //CCSetup.SmartUpdateOutfit();

            base.OnConfirm();
        }
    }
}
