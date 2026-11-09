namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>Tested with signed distance comparison.</summary>
    public const int SwoopTriggerDistance = 96;
    /// <summary>One eighth, in the 16.16 units these accelerations are added in.</summary>
    private const int OneEighth = 8192;

    /// <summary>Seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromRaw(7 * OneEighth);
    /// <summary>The falling wings' arc radius, used by detached-wing setup and both arcs.</summary>
    public const int DetachedWingRadius = 48;

    /// <summary>
    /// The random bit that takes one pixel per update off a grounded Ki-Hunter's upward jump speed: bit zero, added to
    /// the jump's eight pixels upward.
    /// </summary>
    internal static readonly RandomSampleField ShallowerGroundJump = new(0, 1);



    /// <summary>A leftward swoop's angular acceleration: minus an eighth of a table step per frame.</summary>
    public static SignedSixteenSixteen LeftSwoopAcceleration { get; } = SignedSixteenSixteen.FromRaw(-OneEighth);

    /// <summary>A rightward swoop's angular acceleration: an eighth of a table step per frame.</summary>
    public static SignedSixteenSixteen RightSwoopAcceleration { get; } = SignedSixteenSixteen.FromRaw(OneEighth);

    /// <summary>The detached wing orbit's offset angle, seven-eighths of a byte-angle turn.</summary>
    public static TrigonometryAngle DetachedOrbitOffsetAngle { get; } = new(224);

    /// <summary>The detached wing's falling-arc offset angle, five-eighths of a byte-angle turn.</summary>
    public static TrigonometryAngle FallingArcOffsetAngle { get; } = new(160);

    /// <summary>The detached wing orbit's starting rotation angle, an eighth of a turn below zero.</summary>
    public static RotationAngle DetachedOrbitStartAngle { get; } = RotationAngle.NormalizeRaw(-8192);

    /// <summary>The detached wing's falling-arc starting rotation angle, three eighths of a turn below zero.</summary>
    public static RotationAngle FallingArcStartAngle { get; } = RotationAngle.NormalizeRaw(-24576);

    /// <summary>The detached speed accumulator stops summing once it reaches this.</summary>
    public const int DetachedSpeedThreshold = 8192;
}
