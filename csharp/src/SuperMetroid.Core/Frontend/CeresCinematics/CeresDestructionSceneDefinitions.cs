using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The Ceres station's destruction and the approach to Zebes: their timing, motion, floor views and actor places.</summary>
internal static class CeresDestructionSceneDefinitions
{
    /// <summary>Updates the scene waits before it sets up the station.</summary>
    internal const int InitialFrameWaits = 9;

    /// <summary>
    /// The gunship engine's flickering colours, started when the approach to Zebes is set up, which ends the station's
    /// navigation lights.
    /// </summary>
    internal const ColourEffectId GunshipEngineFlicker = ColourEffectId.CutsceneGunshipEngineFlicker;

    /// <summary>The station's blinking navigation lights on the turning floor, started as the station is set up (#1733).</summary>
    internal const ColourEffectId NavigationLights = ColourEffectId.CutsceneCeresNavigationLightsBackground;

    /// <summary>The PLANET ZEBES caption's fade in from black, started as the caption first shows (#1733).</summary>
    internal const ColourEffectId ZebesTextFadeIn = ColourEffectId.FadeInPlanetZebesText;

    /// <summary>The PLANET ZEBES caption's fade out to black, started as its hold ends; it stays black once faded (#1733).</summary>
    internal const ColourEffectId ZebesTextFadeOut = ColourEffectId.FadeOutPlanetZebesText;

    /// <summary>
    /// The colour groups the turning floor shows the records of (#1733): the station's navigation lights over Ceres, and the
    /// gunship engine's flicker on the approach to Zebes.
    /// </summary>
    internal static IReadOnlyList<ColourEffectColours> FloorColours { get; } =
        [ColourEffectColours.CeresNavigationLightsBackground, ColourEffectColours.CutsceneGunshipEngine];

    /// <summary>
    /// The colour group of each actor colouring a colour effect animates (#1733): an actor drawn in the colouring shows the
    /// record the effect last showed over its drawn colours.
    /// </summary>
    internal static IReadOnlyDictionary<CeresSpriteColouring, ColourEffectColours> FadedColours { get; } =
        new Dictionary<CeresSpriteColouring, ColourEffectColours>
        {
            [CeresSpriteColouring.ZebesTitle] = ColourEffectColours.PlanetZebesText,
        };

    /// <summary>
    /// The floor point, in floor pixels, the Ceres explosion's floor turns about before its fade-out; the station's
    /// explosions are also placed relative to it.
    /// </summary>
    internal static Offset16<FloorPixels> CeresFloorCenter => new(52, 48);

    /// <summary>The floor point, in floor pixels, the following Zebes approach's floor turns about.</summary>
    internal static Offset16<FloorPixels> ZebesFloorCenter => new(56, 24);

    /// <summary>
    /// The width, in pixels, of the loop the Ceres scenes' drifting asteroids and station travel round (#2418): the screen
    /// and an equally wide stretch of space left of it. An actor that drifts off the screen's right edge comes round to the
    /// far end of that stretch and drifts back into view across the screen's left edge.
    /// </summary>
    internal const int DriftLoopWidth = 512;

    /// <summary>The tracks the scene plays.</summary>
    internal static class Music
    {
        /// <summary>The station's destruction track when Samus has escaped.</summary>
        internal const MusicTrack CeresTrack = MusicTrack.CeresExplosion;
        /// <summary>The station's destruction track when Samus is still aboard.</summary>
        internal const MusicTrack CeresWithSamusTrack = MusicTrack.CeresExplosionWithSamusAboard;
        /// <summary>How long the destruction track waits on the music queue: fourteen passes.</summary>
        internal static MusicQueueDelay QueueDelay { get; } = new(14);
    }

    /// <summary>Authored holds, in updates.</summary>
    internal static class Timing
    {
        /// <summary>Updates the completed Ceres explosion stays shown.</summary>
        internal const int ExplosionHoldFrames = 192;
        /// <summary>Updates the completed Zebes approach stays shown.</summary>
        internal const int ZebesHoldFrames = 64;
        /// <summary>Updates each step of the cinematic's slow fades takes: one level every second update.</summary>
        internal const int FadeUpdatesPerStep = 2;
    }

    /// <summary>
    /// The square blocks the Zebes reveal draws its planet picture in (#1774), in screen pixels, which also count the reveal
    /// down: each of its steps narrows them by one pixel.
    /// </summary>
    internal static class RevealPixelation
    {
        /// <summary>The blocks the planet picture shows in as the reveal is set up.</summary>
        internal const int StartingBlockWidth = 9;
        /// <summary>
        /// The coarsest blocks the reveal shows: a step from one-pixel blocks, an unpixelated picture, starts again from
        /// blocks this wide.
        /// </summary>
        internal const int CoarsestBlockWidth = 16;
    }

    /// <summary>The floor's zoom thresholds and the scenes' velocities.</summary>
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
        internal static FloorZoomStep CeresZoomStep { get; } = FloorZoomStep.FromTwoHundredFiftySixths(1);
        /// <summary>The near Zebes approach's zoom change each update, in 256ths of a floor pixel per screen pixel.</summary>
        internal static FloorZoomStep NearZebesZoomStep { get; } = FloorZoomStep.FromTwoHundredFiftySixths(4);
        /// <summary>The slow per-update zoom change, in 256ths of a floor pixel per screen pixel.</summary>
        internal static FloorZoomStep SlowZoomStep { get; } = FloorZoomStep.FromTwoHundredFiftySixths(16);
        /// <summary>The fast per-update zoom change, in 256ths of a floor pixel per screen pixel.</summary>
        internal static FloorZoomStep FastZoomStep { get; } = FloorZoomStep.FromTwoHundredFiftySixths(32);
        /// <summary>How far the floor scrolls right each update as the gunship flies away from the exploding station.</summary>
        internal static SignedSixteenSixteen FlyAwayScroll { get; } = SignedSixteenSixteen.FromWholePixels(2);
        /// <summary>How far the floor turns each update as the gunship flies away from the station, in steps: one step clockwise.</summary>
        internal const int FlyAwayTurnSteps = -1;
        /// <summary>How far the floor turns each update as the gunship turns toward Zebes, in steps: one step clockwise.</summary>
        internal const int TowardZebesTurnSteps = -1;
        /// <summary>One eighth of a pixel per update.</summary>
        internal static SignedSixteenSixteen EighthPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(2);
        /// <summary>Minus one half pixel per update.</summary>
        internal static SignedSixteenSixteen NegativeHalfPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);
        /// <summary>One sixteenth of a pixel per update.</summary>
        internal static SignedSixteenSixteen SixteenthPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(1);
        /// <summary>Minus one quarter pixel per update.</summary>
        internal static SignedSixteenSixteen NegativeQuarterPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-4);
        /// <summary>The fleeing gunship's horizontal velocity: a quarter pixel rightward per update.</summary>
        internal static SignedSixteenSixteen GunshipXVelocity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(4);
        /// <summary>The fleeing gunship's vertical velocity: one sixteenth of a pixel upward per update.</summary>
        internal static SignedSixteenSixteen GunshipYVelocity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-1);
        /// <summary>The floor position, in pixels, that completes the gunship's exit.</summary>
        internal const int FloorExitX = 62;
        /// <summary>The floor's rotation during the Zebes approach: an eighth of a turn.</summary>
        internal static RotationAngle ApproachAngle => RotationAngle.FromSteps(32);
    }

    /// <summary>The floor's scroll over the station and on the approach to Zebes, and where the reveal's actors leave.</summary>
    internal static class Sprites
    {
        /// <summary>The floor's starting horizontal scroll over Ceres, in pixels.</summary>
        internal const int CeresInitialBackgroundX = -44;
        /// <summary>The floor's vertical scroll over Ceres, and again as the gunship flies toward Zebes, in pixels.</summary>
        internal const int CeresAndFlightBackgroundY = -112;
        /// <summary>The Zebes approach's starting horizontal scroll, in pixels.</summary>
        internal const int ZebesInitialBackgroundX = 128;
        /// <summary>The Zebes approach's starting vertical scroll, in pixels.</summary>
        internal const int ZebesInitialBackgroundY = -104;
        /// <summary>The horizontal scroll, in pixels, the turn toward Zebes passes to close in on the planet.</summary>
        internal const int TurnTowardZebesEndX = -128;
        /// <summary>The screen row a sliding reveal actor rises above before it deletes itself.</summary>
        internal const int RevealActorExitY = -128;
    }
}
