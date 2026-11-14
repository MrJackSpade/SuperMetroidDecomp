namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's floor zoom and angle thresholds, the gunship's motion and the credits' scroll (#1416).
/// </summary>
public static class EndingMotionDefinitions
{
    /// <summary>The floor point, in floor pixels, the flyaway's perspective floor turns about.</summary>
    public static Offset16<FloorPixels> FloorCenter => new(56, 24);

    /// <summary>The floor point, in floor pixels, the atmospheric and explosion scenes' floor turns about.</summary>
    public static Offset16<FloorPixels> AtmosphericFloorCenter => new(128, 128);

    /// <summary>The escape scenes' initial floor angle.</summary>
    public static TrigonometryAngle EscapeInitialAngle { get; } = new(32);

    /// <summary>The escape and explosion scenes' initial zoom: a quarter floor pixel per screen pixel.</summary>
    public static FloorZoom EscapeInitialZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(64);

    /// <summary>The zoom at which escape scenes A and B begin fading out: one and a half floor pixels per screen pixel.</summary>
    public static FloorZoom EscapeEndZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(384);

    /// <summary>The zoom enabling the side clouds' motion in the side-cloud escape.</summary>
    public static FloorZoom CloudMotionStartZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(96);

    /// <summary>The zoom below which the top-and-bottom-cloud escape's clouds keep moving.</summary>
    public static FloorZoom TopAndBottomCloudZoomLimit { get; } = FloorZoom.FromTwoHundredFiftySixths(176);

    /// <summary>The space view's initial zoom for the gunship's emergence: twelve floor pixels per screen pixel.</summary>
    public static FloorZoom PlanetEscapeInitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(12);

    /// <summary>The zoom below which fast spinning gives way to the slower emergence.</summary>
    public static FloorZoom PlanetFastEndZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(1456);

    /// <summary>The zoom below which the accelerating fly-to-camera phase starts.</summary>
    public static FloorZoom PlanetSlowEndZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(1184);

    /// <summary>The fly-to-camera zoom below which the ship turns toward its exit angle on every fourth update.</summary>
    public static FloorZoom PlanetTurnStartZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(384);

    /// <summary>The fly-to-camera zoom below which gunship colours are restored and the operation-success message begins.</summary>
    public static FloorZoom PlanetExitZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(32);

    /// <summary>The fast emergence's zoom change each update, in 256ths of a floor pixel per screen pixel.</summary>
    public const int PlanetFastZoomStep = 8;

    /// <summary>The slow emergence's zoom change each update, in 256ths of a floor pixel per screen pixel.</summary>
    public const int PlanetSlowZoomStep = 2;

    /// <summary>The fly-to-camera phase's zoom change each update, in 256ths of a floor pixel per screen pixel.</summary>
    public const int PlanetFlyToCameraZoomStep = 4;

    /// <summary>The fly-to-camera phase's initial horizontal velocity: half a pixel per update.</summary>
    public static SignedSixteenSixteen PlanetInitialVelocity => SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The fly-to-camera phase's velocity decrease per update: 1/256 pixel per update squared.</summary>
    public static SignedSixteenSixteen PlanetAcceleration => SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(1);

    /// <summary>The starfield's flyaway speed when the fast planet escape starts it: a quarter pixel an update to the right.</summary>
    public static SignedSixteenSixteen StarsFlyawayInitialVelocity => SignedSixteenSixteen.FromSixteenthsOfPixel(4);

    /// <summary>The starfield's flyaway slowing each update, applied before it moves: 1/2048 of a pixel an update.</summary>
    public static SignedSixteenSixteen StarsFlyawayDeceleration => SignedSixteenSixteen.FromParts(0, 32);

    /// <summary>The credits' vertical scroll per update: half a pixel.</summary>
    public static SignedSixteenSixteen CreditsScrollVelocity => SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The angle the slow-spinning gunship approaches one unit per update.</summary>
    public static RotationAngle PlanetSlowTargetAngle => RotationAngle.FromSteps(224);

    /// <summary>The angle the final fly-to-camera turn approaches, separate from the slow-spin target.</summary>
    public static RotationAngle PlanetExitTargetAngle => RotationAngle.FromSteps(16);

    /// <summary>The steps of the fast ship-shake pattern.</summary>
    public const int PlanetFastPatternLength = 16;

    /// <summary>The steps of the slow ship-shake pattern.</summary>
    public const int PlanetSlowPatternLength = 8;
}
