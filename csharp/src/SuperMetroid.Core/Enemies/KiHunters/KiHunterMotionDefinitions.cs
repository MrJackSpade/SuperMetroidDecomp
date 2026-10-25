namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Native KiHunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>$A8:F180, XProximityToActivateSwoop, tested with native signed distance comparison.</summary>
    public const ushort SwoopTriggerDistance = 96;
    /// <summary>One eighth, in the 16.16 units these accelerations are added in.</summary>
    private const int OneEighth = 8192;

    /// <summary>$A8:F182/$A8:F184: seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SnesSignedSixteenSixteen Gravity { get; } = SnesSignedSixteenSixteen.FromRaw(7 * OneEighth);
    /// <summary>$A8:F186 low byte, fallingWingsArcRadius, used by detached-wing setup and both arcs.</summary>
    public const byte DetachedWingRadius = 48;

    /// <summary>The bit of parameter one that makes the Ki-Hunter start without wings; its wing record starts deleted.</summary>
    public const int WinglessParameterBit = 15;

    /// <summary>A leftward swoop's angular acceleration: minus an eighth of a table step per frame.</summary>
    public static SnesSignedSixteenSixteen LeftSwoopAcceleration { get; } = SnesSignedSixteenSixteen.FromRaw(-OneEighth);

    /// <summary>A rightward swoop's angular acceleration: an eighth of a table step per frame.</summary>
    public static SnesSignedSixteenSixteen RightSwoopAcceleration { get; } = SnesSignedSixteenSixteen.FromRaw(OneEighth);

    /// <summary>The detached wing orbit's offset angle, seven-eighths of a byte-angle turn.</summary>
    public static SnesByteAngle DetachedOrbitOffsetAngle { get; } = new(0xe0);

    /// <summary>The detached wing's falling-arc offset angle, five-eighths of a byte-angle turn.</summary>
    public static SnesByteAngle FallingArcOffsetAngle { get; } = new(0xa0);

    /// <summary>The detached wing orbit's starting 16-bit angle, an eighth of a turn below zero.</summary>
    public static SnesAngle DetachedOrbitStartAngle { get; } = SnesAngle.NormalizeRaw(-0x2000);

    /// <summary>The detached wing's falling-arc starting 16-bit angle, three eighths of a turn below zero.</summary>
    public static SnesAngle FallingArcStartAngle { get; } = SnesAngle.NormalizeRaw(-0x6000);

    /// <summary>The detached speed accumulator stops summing once it reaches this.</summary>
    public const ushort DetachedSpeedThreshold = 0x2000;
}
