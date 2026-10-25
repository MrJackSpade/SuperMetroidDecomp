namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's floor zoom and angle thresholds, the gunship's motion and the credits' scroll (#1416). Zooms are 8.8
/// scales, where $0100 is unity.
/// </summary>
public static class EndingMotionDefinitions
{
    /// <summary>The floor point, in floor pixels, the flyaway's perspective floor turns about.</summary>
    public static Offset16<Mode7Pixels> FloorCenter => new(56, 24);

    /// <summary>The floor point, in floor pixels, the atmospheric and explosion scenes' floor turns about.</summary>
    public static Offset16<Mode7Pixels> AtmosphericFloorCenter => new(128, 128);

    /// <summary>The unity zoom.</summary>
    public const ushort IdentityScale = 0x0100;

    /// <summary>The escape scenes' initial floor angle.</summary>
    public static ByteAngle EscapeInitialAngle { get; } = new(0x20);

    /// <summary>The escape and explosion scenes' initial zoom.</summary>
    public const ushort EscapeInitialScale = 0x0040;

    /// <summary>The zoom at which escape scenes A and B begin fading out.</summary>
    public const ushort EscapeEndScale = 0x0180;

    /// <summary>The zoom enabling side-cloud motion in escape scene A.</summary>
    public const ushort CloudMotionStartScale = 0x0060;

    /// <summary>The zoom below which scene B's top and bottom clouds keep moving.</summary>
    public const ushort CloudSceneBScaleLimit = 0x00b0;

    /// <summary>The space view's initial zoom for the gunship's emergence.</summary>
    public const ushort PlanetEscapeInitialScale = 0x0c00;

    /// <summary>The zoom below which fast spinning gives way to the slower emergence.</summary>
    public const ushort PlanetFastEndScale = 0x05b0;

    /// <summary>The zoom below which the accelerating fly-to-camera phase starts.</summary>
    public const ushort PlanetSlowEndScale = 0x04a0;

    /// <summary>The fly-to-camera zoom below which the ship turns toward its exit angle on every fourth update.</summary>
    public const ushort PlanetTurnStartScale = 0x0180;

    /// <summary>The fly-to-camera zoom below which gunship colours are restored and the operation-success message begins.</summary>
    public const ushort PlanetExitScale = 0x0020;

    /// <summary>The fly-to-camera phase's initial horizontal velocity: half a pixel per update.</summary>
    public static SignedSixteenSixteen PlanetInitialVelocity => SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The fly-to-camera phase's velocity decrease per update: 1/256 pixel per update squared.</summary>
    public static SignedSixteenSixteen PlanetAcceleration => SignedSixteenSixteen.FromRaw(0x0100);

    /// <summary>The credits' vertical scroll per update: half a pixel.</summary>
    public static SignedSixteenSixteen CreditsScrollVelocity => SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>The angle the slow-spinning gunship approaches one unit per update.</summary>
    public static RotationAngle PlanetSlowTargetAngle => RotationAngle.FromTableIndex(0xe0);

    /// <summary>The angle the final fly-to-camera turn approaches, separate from the slow-spin target.</summary>
    public static RotationAngle PlanetExitTargetAngle => RotationAngle.FromTableIndex(0x10);

    /// <summary>The steps of the fast ship-shake pattern.</summary>
    public const int PlanetFastPatternLength = 16;

    /// <summary>The steps of the slow ship-shake pattern.</summary>
    public const int PlanetSlowPatternLength = 8;
}
