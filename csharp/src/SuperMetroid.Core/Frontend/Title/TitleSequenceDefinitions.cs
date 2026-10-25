using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The title sequence's definitions: its console lights, music, cards, floor scenes, sprite placement, installed graphics extents, skip colours and timing.</summary>
public static class TitleSequenceDefinitions
{
    /// <summary>The console lights title setup and the skip path start.</summary>
    public static class ConsolePaletteFx
    {
        /// <summary>The Baby Metroid tube's slow ten-update light.</summary>
        public const PaletteFxObject SlowLights = PaletteFxObject.TitleScreenBabyMetroidTubeLight;

        /// <summary>The flickering displays' alternating colours.</summary>
        public const PaletteFxObject FastLights = PaletteFxObject.TitleScreenFlickeringDisplays;
    }

    /// <summary>The tracks the normal and skip paths select.</summary>
    public static class Music
    {
        /// <summary>The title music's track for the normal opening sequence.</summary>
        public const byte OpeningTrack = 5;
        /// <summary>The track the skip path selects before rebuilding the title screen.</summary>
        public const byte ImmediateTitleTrack = 6;
    }

    /// <summary>The four title cards: where each draws and with which sprite palette.</summary>
    public static class TextSequences
    {
        /// <summary>The year card.</summary>
        public static readonly TitleTextSequenceDefinition Year =
            new(TitleSequencePhase.YearText, new(129, 112), SnesObjPalettes.Palette(1));
        /// <summary>The Nintendo card.</summary>
        public static readonly TitleTextSequenceDefinition Nintendo =
            new(TitleSequencePhase.NintendoText, new(129, 112), SnesObjPalettes.Palette(1));
        /// <summary>The Presents card.</summary>
        public static readonly TitleTextSequenceDefinition Presents =
            new(TitleSequencePhase.PresentsText, new(129, 112), SnesObjPalettes.Palette(1));
        /// <summary>The Metroid 3 card.</summary>
        public static readonly TitleTextSequenceDefinition MetroidThree =
            new(TitleSequencePhase.MetroidThreeText, new(129, 112), SnesObjPalettes.Palette(1));
    }

    /// <summary>The floor scenes' starting transforms, motion and completion thresholds.</summary>
    public static class Scenes
    {
        /// <summary>The lower pan: zoom 72 at background offset (315, 225).</summary>
        public static readonly TitleFloorSceneDefinition SceneZero =
            new(TitleSequencePhase.SceneZeroPan, 72, new(315, 225));
        /// <summary>The upper pan: zoom 96 at background offset (44, -155).</summary>
        public static readonly TitleFloorSceneDefinition SceneOne =
            new(TitleSequencePhase.SceneOnePan, 96, new(44, -155));
        /// <summary>The downward pan: zoom 96 at background offset (-177, -160).</summary>
        public static readonly TitleFloorSceneDefinition SceneTwo =
            new(TitleSequencePhase.SceneTwoPan, 96, new(-177, -160));
        /// <summary>The final zoom: zoom 67 with no background offset.</summary>
        public static readonly TitleFloorSceneDefinition SceneThree =
            new(TitleSequencePhase.SceneThreeZoom, 67, new(0, 0));
        /// <summary>The pan speed, 1.5 pixels per update in 1/65536 pixels; scenes zero and one pan left, scene two down.</summary>
        public const int PanVelocity16Point16 = 98304;
        /// <summary>The lower pan's X threshold, -7 pixels; passing it starts the Nintendo card.</summary>
        public const int SceneZeroEndX = -7;
        /// <summary>The upper pan's X threshold, -176 pixels; passing it starts the Presents card.</summary>
        public const int SceneOneEndX = -176;
        /// <summary>The downward pan's Y threshold, 163 pixels; reaching it starts the Metroid 3 card.</summary>
        public const int SceneTwoEndY = 163;
        /// <summary>The unscaled zoom, in 1/256 units; reaching it completes the final zoom.</summary>
        public const int IdentityScale = 256;
        /// <summary>The floor scenes' rotation: none; the scenes pan and zoom only.</summary>
        public static RotationAngle Rotation => RotationAngle.Zero;
    }

    /// <summary>Where the logo and copyright draw and with which sprite palettes.</summary>
    public static class Sprites
    {
        /// <summary>The title logo's origin in screen pixels.</summary>
        public static Position16<ScreenPixels> LogoOrigin => new(128, 48);
        /// <summary>The Nintendo copyright's origin in screen pixels, below the logo.</summary>
        public static Position16<ScreenPixels> CopyrightOrigin => new(128, 196);
        /// <summary>The sprite palette the title logo draws with.</summary>
        public static readonly SnesObjAttributeWord LogoPalette = SnesObjPalettes.Palette(2);
        /// <summary>The sprite palette the Nintendo copyright draws with.</summary>
        public static readonly SnesObjAttributeWord CopyrightPalette = SnesObjPalettes.Palette(4);
    }

    /// <summary>The installed title graphics' extents and the Baby Metroid's animation pages.</summary>
    public static class Graphics
    {
        /// <summary>The title sprites' 512 four-bit characters.</summary>
        public const int ObjectCharacterByteCount = 512 * 32;
        /// <summary>The Baby Metroid's floor characters: four pages of four characters.</summary>
        public const int BabyCharacterByteCount = 4 * BabyCharacterPageByteCount;
        /// <summary>The title floor's 256 eight-bit characters.</summary>
        public const int FloorCharacterByteCount = 256 * 64;
        /// <summary>The cells of the title floor's map that the scenes show.</summary>
        public const int FloorMapByteCount = 64 * 64;
        /// <summary>The floor character every cell outside the shown map holds.</summary>
        public const byte FloorInitialMapByte = 255;
        /// <summary>One Baby Metroid animation page: four eight-bit characters.</summary>
        public const int BabyCharacterPageByteCount = 4 * 64;
        /// <summary>The Baby Metroid's four-phase page cycle 0, 1, 2, 1.</summary>
        public static TitleBabyPageSequence BabyAnimationSourcePages => default;
    }

    /// <summary>The colours only the skip path installs.</summary>
    public static class Palette
    {
        /// <summary>The copyright's white on the skipped-to title screen.</summary>
        public static Bgr555 CopyrightWhite => Bgr555.White;
        /// <summary>The skip-only copyright colour, kept under its historical Red name.</summary>
        public static Bgr555 CopyrightRed => Bgr555.FromWord(0x7d80);
    }

    /// <summary>Holds, fades and the buttons that skip.</summary>
    public static class Timing
    {
        /// <summary>The fully lit brightness level, which clamps fades and scales rendered colours.</summary>
        public const int MaximumBrightness = 15;
        /// <summary>Brightness change per update for the skip's fade-out and the title's fade-in.</summary>
        public const int SkipFadeBrightnessStep = 2;
        /// <summary>Updates the logo holds before the copyright.</summary>
        public const int LogoHoldFrames = 32;
        /// <summary>Updates the copyright holds before the idle timeout starts.</summary>
        public const int CopyrightHoldFrames = 32;
        /// <summary>The idle title screen's timeout in updates; expiring selects the demo before a confirmation that update.</summary>
        public const int TitleScreenNtscFrames = 900;
        /// <summary>Updates between brightness decrements when leaving the idle title screen.</summary>
        public const int TitleFadeCadenceFrames = 2;
        /// <summary>Updates per Baby Metroid animation phase.</summary>
        public const int BabyFrameDuration = 10;
        /// <summary>The buttons whose new presses skip the opening or leave the idle title screen.</summary>
        public static ControllerButton ConfirmButtons =>
            ControllerButton.B | ControllerButton.Start | ControllerButton.A;
    }
}
