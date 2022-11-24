using ModSettings;
using System.Reflection;

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

        internal static void SetKeySettingsVisible(bool visible)
        {
            options.SetFieldVisible(nameof(options.skinTextureHue), visible);
            options.SetFieldVisible(nameof(options.skinTextureSat), visible);
            options.SetFieldVisible(nameof(options.skinTextureLum), visible);
        }
    }

    internal class CCSettings : JsonModSettings
    {
        [Section("Mesh customization")]

        [Name("Character")]
        [Description("You can change your survivor here")]
        [Choice(new string[]
        {
            "Astrid",
            "Will"
        })]
        public int selectedCharacter;

        [Name("Dynamic outfit")]
        [Description("Automatically switch between outdoors/indoors/injured outfits. \n\nOnly changes when arms are hidden to preserve immersion. \n\nDefault: true")]
        public bool dynamicOutfit = true;
        /*
        [Name("Show injures")]
        [Description("Show bandages when injured. Only visible on indoors clothes \n\nDefault: true")]
        public bool showInjuries = true;
        */
        [Name("Default outfit")]
        [Description("How your character looks by default. Has no effect if Dynamic outfit is on. \n\nDefault: Vanilla")]
        [Choice(new string[]
        {
            "Vanilla",
            "Full outfit",
            "Injured",
            "Undressed"
        })]
        public int defaultAppearance;


        [Section("Texture customization")]

        [Name("Use custom textures")]
        [Description("Load textures from .../Mods/characterCustomizer/customTextures/ \n\nCheck readme inside that folder for more info. \n\nDefault: false")]
        public bool useCustomTextures = false;

        [Name("Tint textures")]
        [Description("Customize textures by tinting them with HSL values \n\nDefault: false")]
        public bool useTextureTint = false;

        [Name("Skin texture HUE")]
        [Description("Hue is limited to 10-30 degrees, which is more or less in human skin tone range \n\nLow value = red\n\nHigh value = yellow")]
        [Slider(10, 30)]
        public int skinTextureHue = 10;

        [Name("Skin texture SATuration")]
        [Description("Saturation is limited because I'm boring \n\nLow value = less saturated color\n\nHigh value = more saturated value")]
        [Slider(20, 50)]
        public int skinTextureSat = 20;

        [Name("Skin texture LUMinocity")]
        [Description("Luminocity is a bit limited as well\n\nLow value = dark \n\nHigh value = light \n\nSetting this to 100 will make texture unchanged ")]
        [Slider(10, 100)]
        public int skinTextureLum = 100;

        [Section("Debug/incomplete stuff")]

        [Name("Enable debug messages")]
        [Description("To help developer find the issue")]
        public bool debugLog = false;

        [Name("Display current clothes")]
        [Description("(this is just a proof of concept, I didn't make any clothing meshes and don't have plans for it yet)\n\nOverrides automatic outfit switch")]
        public bool displayProperClothes = false;

        [Name("Enable trinkets")]
        [Description("Add some custom trinkets to your character.(does nothing at the moment) \n\nDefault: false")]
        public bool enableTrinkets = false;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            if (field.Name == nameof(useTextureTint)) Settings.SetKeySettingsVisible((bool)newValue);
        }

        protected override void OnConfirm()
        {
            CCSetup.DoEverything(CCSetup.currentCharacter, 0, 3);

            if (Settings.options.useTextureTint)
            {
                CCSetup.TintTexture(Slot.Hands, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
                CCSetup.TintTexture(Slot.Arms, Settings.options.skinTextureHue, Settings.options.skinTextureSat, Settings.options.skinTextureLum);
            }
            else
            {
                CCSetup.TintTexture(Slot.Hands, 0, 0, 100);
                CCSetup.TintTexture(Slot.Arms, 0, 0, 100);
            }

            switch (Settings.options.selectedCharacter)
            {
                case 0: // Astrid
                    InterfaceManager.m_Panel_OptionsMenu.m_State.m_VoicePersona = VoicePersona.Female;
                    GameManager.GetPlayerManagerComponent().m_VoicePersona = VoicePersona.Female;
                    GameManager.GetPlayerVoiceComponent().SetPlayerVoicePersona();
                    break;
                case 1: // Will
                    InterfaceManager.m_Panel_OptionsMenu.m_State.m_VoicePersona = VoicePersona.Male;
                    GameManager.GetPlayerManagerComponent().m_VoicePersona = VoicePersona.Male;
                    GameManager.GetPlayerVoiceComponent().SetPlayerVoicePersona();
                    break;

            }

            CCSetup.SmartUpdateOutfit();

            base.OnConfirm();
        }
    }
}
