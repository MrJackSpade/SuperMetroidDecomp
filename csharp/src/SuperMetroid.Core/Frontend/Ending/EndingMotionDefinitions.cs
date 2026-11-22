namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's floor zoom and angle thresholds, the gunship's motion and the credits' scroll (#1416).
/// </summary>
public static partial class EndingMotionDefinitions
{
    /// <summary>The floor point, in floor pixels, the flyaway's perspective floor turns about.</summary>
    public static Offset16<FloorPixels> FloorCenter => new(56, 24);

    /// <summary>The floor point, in floor pixels, the atmospheric and explosion scenes' floor turns about.</summary>
    public static Offset16<FloorPixels> AtmosphericFloorCenter => new(128, 128);

    /// <summary>The escape scenes' initial floor angle.</summary>
    public static TrigonometryAngle EscapeInitialAngle { get; } = new(32);

    /// <summary>The escape and explosion scenes' initial zoom.</summary>
    public static FloorZoom EscapeInitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(1, 4);

    /// <summary>The zoom at which the side-cloud and top-and-bottom-cloud escapes begin fading out.</summary>
    public static FloorZoom EscapeEndZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(3, 2);

    /// <summary>The side-cloud escape's zoom change each update.</summary>
    public static FloorZoomStep SideCloudEscapeZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 128);

    /// <summary>The top-and-bottom-cloud escape's zoom change each update.</summary>
    public static FloorZoomStep TopAndBottomCloudEscapeZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(3, 256);

    /// <summary>The zoom enabling the side clouds' motion in the side-cloud escape.</summary>
    public static FloorZoom CloudMotionStartZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(3, 8);

    /// <summary>The zoom below which the top-and-bottom-cloud escape's clouds keep moving.</summary>
    public static FloorZoom TopAndBottomCloudZoomLimit { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(11, 16);

    /// <summary>The space view's initial zoom for the gunship's emergence.</summary>
    public static FloorZoom PlanetEscapeInitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(12);

    /// <summary>The zoom below which fast spinning gives way to the slower emergence.</summary>
    public static FloorZoom PlanetFastEndZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(91, 16);

    /// <summary>The zoom below which the accelerating fly-to-camera phase starts.</summary>
    public static FloorZoom PlanetSlowEndZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(37, 8);

    /// <summary>The fly-to-camera zoom below which the ship turns toward its exit angle at its turn cadence.</summary>
    public static FloorZoom PlanetTurnStartZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(3, 2);

    /// <summary>The fly-to-camera zoom below which gunship colours are restored and the operation-success message begins.</summary>
    public static FloorZoom PlanetExitZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(1, 8);

    /// <summary>The space view's floor scroll as the gunship's emergence starts: horizontally, in pixels.</summary>
    public const int PlanetEscapeFloorX = -72;

    /// <summary>The space view's floor scroll as the gunship's emergence starts: vertically, in pixels.</summary>
    public const int PlanetEscapeFloorY = -104;

    /// <summary>The space view's floor angle as the gunship's emergence starts.</summary>
    public static RotationAngle PlanetEscapeInitialAngle => RotationAngle.WrappedSteps(-112);

    /// <summary>How far the cloud escapes turn the floor on each of their turning updates, in steps: one step clockwise.</summary>
    public const int AtmosphereTurnSteps = -1;

    /// <summary>
    /// How far the slow emergence turns the floor each update until it reaches
    /// <see cref="PlanetSlowTargetAngle"/>, in steps: one step clockwise.
    /// </summary>
    public const int PlanetSlowTurnSteps = -1;

    /// <summary>How far the fast emergence turns the floor each update, in steps: four steps clockwise.</summary>
    public const int PlanetFastTurnSteps = -4;

    /// <summary>How far the fly-to-camera turn toward the exit angle turns the floor on each of its updates, in steps.</summary>
    public const int PlanetExitTurnSteps = 2;

    /// <summary>The fast emergence's zoom change each update.</summary>
    public static FloorZoomStep PlanetFastZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 32);

    /// <summary>The slow emergence's zoom change each update.</summary>
    public static FloorZoomStep PlanetSlowZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 128);

    /// <summary>The fly-to-camera phase's zoom change each update.</summary>
    public static FloorZoomStep PlanetFlyToCameraZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 64);

    /// <summary>The fly-to-camera phase's initial horizontal velocity.</summary>
    public static SignedSixteenSixteen PlanetInitialVelocity => SignedSixteenSixteen.FromPixelFraction(1, 2);

    /// <summary>The fly-to-camera phase's velocity decrease each update.</summary>
    public static SignedSixteenSixteen PlanetAcceleration => SignedSixteenSixteen.FromPixelFraction(1, 256);

    /// <summary>The starfield's flyaway speed to the right when the fast planet escape starts it.</summary>
    public static SignedSixteenSixteen StarsFlyawayInitialVelocity => SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>The starfield's flyaway slowing each update, applied before it moves.</summary>
    public static SignedSixteenSixteen StarsFlyawayDeceleration => SignedSixteenSixteen.FromPixelFraction(1, 2048);

    /// <summary>The credits' vertical scroll per update.</summary>
    public static SignedSixteenSixteen CreditsScrollVelocity => SignedSixteenSixteen.FromPixelFraction(1, 2);

    /// <summary>The angle the slow-spinning gunship approaches by <see cref="PlanetSlowTurnSteps"/> each update.</summary>
    public static RotationAngle PlanetSlowTargetAngle => RotationAngle.FromSteps(224);

    /// <summary>The angle the final fly-to-camera turn approaches, separate from the slow-spin target.</summary>
    public static RotationAngle PlanetExitTargetAngle => RotationAngle.FromSteps(16);


    /// <summary>How far a right side cloud of the escape drifts each update: one pixel left and two up.</summary>
    public static Offset16<ScreenPixels> RightSideCloudDrift { get; } = new(-1, -2);

    /// <summary>How far a left side cloud of the escape drifts each update: one pixel right and two down.</summary>
    public static Offset16<ScreenPixels> LeftSideCloudDrift { get; } = new(1, 2);
}
