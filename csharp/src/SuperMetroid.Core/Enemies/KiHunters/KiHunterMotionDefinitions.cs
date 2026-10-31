namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Native KiHunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>$A8:F180, XProximityToActivateSwoop, tested with native signed distance comparison.</summary>
    public const int SwoopTriggerDistance = 96;
    /// <summary>One eighth, in the 16.16 units these accelerations are added in.</summary>
    private const int OneEighth = 8192;

    /// <summary>$A8:F182/$A8:F184: seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromRaw(7 * OneEighth);
    /// <summary>$A8:F186 low byte, fallingWingsArcRadius, used by detached-wing setup and both arcs.</summary>
    public const int DetachedWingRadius = 48;



    /// <summary>A leftward swoop's angular acceleration: minus an eighth of a table step per frame.</summary>
    public static SignedSixteenSixteen LeftSwoopAcceleration { get; } = SignedSixteenSixteen.FromRaw(-OneEighth);

    /// <summary>A rightward swoop's angular acceleration: an eighth of a table step per frame.</summary>
    public static SignedSixteenSixteen RightSwoopAcceleration { get; } = SignedSixteenSixteen.FromRaw(OneEighth);

    /// <summary>The detached wing orbit's offset angle, seven-eighths of a byte-angle turn.</summary>
    public static ByteAngle DetachedOrbitOffsetAngle { get; } = new(0xe0);

    /// <summary>The detached wing's falling-arc offset angle, five-eighths of a byte-angle turn.</summary>
    public static ByteAngle FallingArcOffsetAngle { get; } = new(0xa0);

    /// <summary>The detached wing orbit's starting 16-bit angle, an eighth of a turn below zero.</summary>
    public static RotationAngle DetachedOrbitStartAngle { get; } = RotationAngle.NormalizeRaw(-0x2000);

    /// <summary>The detached wing's falling-arc starting 16-bit angle, three eighths of a turn below zero.</summary>
    public static RotationAngle FallingArcStartAngle { get; } = RotationAngle.NormalizeRaw(-0x6000);

    /// <summary>The detached speed accumulator stops summing once it reaches this.</summary>
    public const int DetachedSpeedThreshold = 0x2000;
}
