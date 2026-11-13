using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The title sequence's definitions: its console lights, music, cards, floor scenes, sprite placement, installed graphics extents and timing.</summary>
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

    /// <summary>The colour effects that fade the logo and the copyright in from black as each first shows (#1714).</summary>
    public static class Fades
    {
        /// <summary>The logo's fade, started on the update the final zoom completes and the logo first shows.</summary>
        public const PaletteFxObject Logo = PaletteFxObject.FadeInSuperMetroidTitleLogo;

        /// <summary>The copyright's fade, started on the update the logo's hold ends and the copyright first shows.</summary>
        public const PaletteFxObject Copyright = PaletteFxObject.FadeInNintendoCopyright;
    }

    /// <summary>The tracks the normal and skip paths select.</summary>
    public static class Music
    {
        /// <summary>The title music's song for the normal opening sequence: the set's first own song.</summary>
        public const MusicTrack OpeningTrack = MusicTrack.FirstSetSong;
        /// <summary>The song the skip path selects before rebuilding the title screen: the set's second own song.</summary>
        public const MusicTrack ImmediateTitleTrack = MusicTrack.SecondSetSong;
    }

    /// <summary>The four title cards: where each draws and with which sprite palette.</summary>
    public static class TextSequences
    {
        /// <summary>The year card.</summary>
        public static readonly TitleTextSequenceDefinition Year =
            new(TitleSequencePhase.YearText, new(129, 112), TitleSpriteColouring.Text);
        /// <summary>The Nintendo card.</summary>
        public static readonly TitleTextSequenceDefinition Nintendo =
            new(TitleSequencePhase.NintendoText, new(129, 112), TitleSpriteColouring.Text);
        /// <summary>The Presents card.</summary>
        public static readonly TitleTextSequenceDefinition Presents =
            new(TitleSequencePhase.PresentsText, new(129, 112), TitleSpriteColouring.Text);
        /// <summary>The Metroid 3 card.</summary>
        public static readonly TitleTextSequenceDefinition MetroidThree =
            new(TitleSequencePhase.MetroidThreeText, new(129, 112), TitleSpriteColouring.Text);
    }

    /// <summary>The floor scenes' starting transforms, motion and completion thresholds.</summary>
    public static class Scenes
    {
        /// <summary>The lower pan: zoom 72/256 at background offset (315, 225).</summary>
        public static readonly TitleFloorSceneDefinition SceneZero =
            new(TitleSequencePhase.SceneZeroPan, FloorZoom.FromTwoHundredFiftySixths(72), new(315, 225));
        /// <summary>The upper pan: zoom 96/256 at background offset (44, -155).</summary>
        public static readonly TitleFloorSceneDefinition SceneOne =
            new(TitleSequencePhase.SceneOnePan, FloorZoom.FromTwoHundredFiftySixths(96), new(44, -155));
        /// <summary>The downward pan: zoom 96/256 at background offset (-177, -160).</summary>
        public static readonly TitleFloorSceneDefinition SceneTwo =
            new(TitleSequencePhase.SceneTwoPan, FloorZoom.FromTwoHundredFiftySixths(96), new(-177, -160));
        /// <summary>The final zoom: zoom 67/256 with no background offset.</summary>
        public static readonly TitleFloorSceneDefinition SceneThree =
            new(TitleSequencePhase.SceneThreeZoom, FloorZoom.FromTwoHundredFiftySixths(67), new(0, 0));
        /// <summary>The pan speed, 1.5 pixels per update; scenes zero and one pan left, scene two down.</summary>
        public static SignedSixteenSixteen PanVelocity { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(384);
        /// <summary>The lower pan's X threshold, -7 pixels; passing it starts the Nintendo card.</summary>
        public const int SceneZeroEndX = -7;
        /// <summary>The upper pan's X threshold, -176 pixels; passing it starts the Presents card.</summary>
        public const int SceneOneEndX = -176;
        /// <summary>The downward pan's Y threshold, 163 pixels; reaching it starts the Metroid 3 card.</summary>
        public const int SceneTwoEndY = 163;
        /// <summary>The final zoom's step every second update, in 256ths of a floor pixel per screen pixel.</summary>
        public const int FinalZoomStep = 1;
        /// <summary>
        /// The updates of the final zoom on which the zoom grows one step (#1619): every second update of the phase's own
        /// count, on its even place, from the first.
        /// </summary>
        public static readonly UpdateCadence FinalZoomStepEverySecondUpdate = new(2, 0);
        /// <summary>The floor scenes' rotation: none; the scenes pan and zoom only.</summary>
        public static RotationAngle Rotation => RotationAngle.Zero;
        /// <summary>The floor point the floor scenes zoom about.</summary>
        public static Offset16<FloorPixels> FloorCentre => new(128, 128);
    }

    /// <summary>Where the logo and copyright draw and with which sprite palettes.</summary>
    public static class Sprites
    {
        /// <summary>The title logo's origin in screen pixels.</summary>
        public static Position16<ScreenPixels> LogoOrigin => new(128, 48);
        /// <summary>The Nintendo copyright's origin in screen pixels, below the logo.</summary>
        public static Position16<ScreenPixels> CopyrightOrigin => new(128, 196);
        /// <summary>The colouring the logo draws in, as the final zoom and the skip show it.</summary>
        public const TitleSpriteColouring LogoColouring = TitleSpriteColouring.Logo;
        /// <summary>The colouring the copyright draws in.</summary>
        public const TitleSpriteColouring CopyrightColouring = TitleSpriteColouring.Copyright;

        /// <summary>
        /// The colour group of each colouring whose colours a fade animates (#1714): a composition drawn in the colouring
        /// shows the record the fade last showed over its drawn colours.
        /// </summary>
        public static IReadOnlyDictionary<TitleSpriteColouring, PaletteFxColours> FadedColours { get; } =
            new Dictionary<TitleSpriteColouring, PaletteFxColours>
            {
                [TitleSpriteColouring.Logo] = PaletteFxColours.TitleLogo,
                [TitleSpriteColouring.Copyright] = PaletteFxColours.NintendoCopyright,
            };
    }

    /// <summary>The title's animated artwork.</summary>
    public static class Graphics
    {
        /// <summary>The poses the Baby Metroid lying on the floor pulses through, one each animation frame, in turn.</summary>
        public static IReadOnlyList<TitleBabyPose> BabyAnimation { get; } =
            [TitleBabyPose.Closed, TitleBabyPose.Middle, TitleBabyPose.Open, TitleBabyPose.Middle];
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
        /// <summary>Updates the Baby Metroid holds each pose of its animation.</summary>
        public const int BabyFrameDuration = 10;
        /// <summary>The buttons whose new presses skip the opening or leave the idle title screen.</summary>
        public static ControllerButton ConfirmButtons =>
            ControllerButton.B | ControllerButton.Start | ControllerButton.A;
    }
}
