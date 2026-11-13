using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The Ceres station's destruction and the approach to Zebes: their timing, motion, floor views and actor slots.</summary>
internal static class CeresDestructionSceneDefinitions
{
    /// <summary>Updates the scene waits before it sets up the station.</summary>
    internal const int InitialFrameWaits = 9;

    /// <summary>The gunship engine's flickering colours, started when the approach to Zebes is set up.</summary>
    internal const PaletteFxObject GunshipEngineFlicker = PaletteFxObject.CutsceneGunshipEngineFlicker;

    /// <summary>
    /// The floor point, in floor pixels, the Ceres explosion's floor turns about before its fade-out; the station's
    /// explosions are also placed relative to it.
    /// </summary>
    internal static Offset16<FloorPixels> CeresFloorCenter => new(52, 48);

    /// <summary>The floor point, in floor pixels, the following Zebes approach's floor turns about.</summary>
    internal static Offset16<FloorPixels> ZebesFloorCenter => new(56, 24);

    /// <summary>The width, in pixels, of the world the approach's actors wrap around horizontally.</summary>
    internal const int WorldWidth = 512;

    /// <summary>The tracks the scene plays.</summary>
    internal static class Music
    {
        /// <summary>The station's destruction track when Samus has escaped.</summary>
        internal const MusicTrack CeresTrack = MusicTrack.ThirdSetSong;
        /// <summary>The station's destruction track when Samus is still aboard.</summary>
        internal const MusicTrack CeresWithSamusTrack = MusicTrack.FourthSetSong;
        /// <summary>The queue delay the destruction track is requested with.</summary>
        internal const int QueueDelay = 14;
    }

    /// <summary>Authored holds and the planet reveal's mosaic fade, in updates.</summary>
    internal static class Timing
    {
        /// <summary>Updates the completed Ceres explosion stays shown.</summary>
        internal const int ExplosionHoldFrames = 192;
        /// <summary>Updates the completed Zebes approach stays shown.</summary>
        internal const int ZebesHoldFrames = 64;
        /// <summary>The planet reveal's starting mosaic size; each fade step lowers it by one until it clears at zero.</summary>
        internal const int InitialMosaicSize = 8;
        /// <summary>The mosaic sizes the screen's mosaic setting holds; lowering the smallest wraps to the largest.</summary>
        internal const int MosaicSizeCount = 16;
    }

    /// <summary>The floor's zoom thresholds and the scenes' fixed-point velocities.</summary>
    internal static class Motion
    {
        /// <summary>The zoom that ends the initial Ceres zoom: two and a half floor pixels per screen pixel.</summary>
        internal static FloorZoom CeresZoomLimit { get; } = FloorZoom.FromTwoHundredFiftySixths(640);
        /// <summary>The zoom the Ceres explosion's composition begins at: three floor pixels per screen pixel.</summary>
        internal static FloorZoom CeresExplosionZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(3);
        /// <summary>The smallest zoom the sequence keeps: a sixteenth of a floor pixel per screen pixel.</summary>
        internal static FloorZoom MinimumZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(16);
        /// <summary>The zoom that ends the near Zebes approach: four and a half floor pixels per screen pixel.</summary>
        internal static FloorZoom ZebesApproachZoomLimit { get; } = FloorZoom.FromTwoHundredFiftySixths(1152);
        /// <summary>The zoom that separates the far Zebes approach: thirty-two floor pixels per screen pixel.</summary>
        internal static FloorZoom FarZebesZoomLimit { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(32);
        /// <summary>The initial Ceres zoom's change each update, in 256ths of a floor pixel per screen pixel.</summary>
        internal const int CeresZoomStep = 1;
        /// <summary>The near Zebes approach's zoom change each update, in 256ths of a floor pixel per screen pixel.</summary>
        internal const int NearZebesZoomStep = 4;
        /// <summary>The slow per-update zoom change, in 256ths of a floor pixel per screen pixel.</summary>
        internal const int SlowZoomStep = 16;
        /// <summary>The fast per-update zoom change, in 256ths of a floor pixel per screen pixel.</summary>
        internal const int FastZoomStep = 32;
        /// <summary>One eighth of a pixel per update.</summary>
        internal static SignedSixteenSixteen EighthPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(2);
        /// <summary>Minus one half pixel per update.</summary>
        internal static SignedSixteenSixteen NegativeHalfPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);
        /// <summary>One sixteenth of a pixel per update.</summary>
        internal static SignedSixteenSixteen SixteenthPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(1);
        /// <summary>Minus one quarter pixel per update.</summary>
        internal static SignedSixteenSixteen NegativeQuarterPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-4);
        /// <summary>The fleeing gunship's horizontal speed: a quarter pixel per update, of which only the fraction moves the blasts.</summary>
        internal static SignedSixteenSixteen GunshipXSubvelocity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(4);
        /// <summary>The fleeing gunship's vertical velocity: one sixteenth of a pixel upward per update.</summary>
        internal static SignedSixteenSixteen GunshipYVelocity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-1);
        /// <summary>The floor position, in pixels, that completes the gunship's exit.</summary>
        internal const int FloorExitX = 62;
        /// <summary>The floor's rotation during the Zebes approach: an eighth of a turn.</summary>
        internal static RotationAngle ApproachAngle => RotationAngle.FromSteps(32);
    }

    /// <summary>The approach's starting scroll.</summary>
    internal static class Sprites
    {
        /// <summary>The Zebes approach's starting horizontal scroll, in pixels.</summary>
        internal const int ZebesInitialBackgroundX = 128;
    }
}
