namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>Tested with signed distance comparison.</summary>
    public const int SwoopTriggerDistance = 96;
    /// <summary>An eighth of a table step, in the 65536ths of a step a swoop's angular speed counts below its whole steps.</summary>
    private const int EighthOfAStep = 65536 / 8;

    /// <summary>Seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(14);
    /// <summary>The falling wings' arc radius, used by detached-wing setup and both arcs.</summary>
    public const int DetachedWingRadius = 48;

    /// <summary>
    /// The random choice that takes one pixel per update off a grounded Ki-Hunter's upward jump speed: the sample modulo 2, added to
    /// the jump's eight pixels upward.
    /// </summary>
    internal static readonly RandomSampleField ShallowerGroundJump = new(0, 1);



    /// <summary>A rightward swoop's angular acceleration: an eighth of a table step per frame.</summary>
    public static SignedSixteenSixteen RightSwoopAcceleration { get; } = SignedSixteenSixteen.FromParts(0, EighthOfAStep);

    /// <summary>A leftward swoop's angular acceleration: minus an eighth of a table step per frame.</summary>
    public static SignedSixteenSixteen LeftSwoopAcceleration { get; } = -RightSwoopAcceleration;

    /// <summary>The detached wing orbit's offset angle, seven-eighths of a turn.</summary>
    public static TrigonometryAngle DetachedOrbitOffsetAngle { get; } = new(224);

    /// <summary>The detached wing's falling-arc offset angle, five-eighths of a turn.</summary>
    public static TrigonometryAngle FallingArcOffsetAngle { get; } = new(160);

    /// <summary>The detached wing orbit's starting rotation angle, an eighth of a turn below zero.</summary>
    public static RotationAngle DetachedOrbitStartAngle { get; } = RotationAngle.WrappedSteps(-RotationAngle.StepsPerTurn / 8);

    /// <summary>The detached wing's falling-arc starting rotation angle, three eighths of a turn below zero.</summary>
    public static RotationAngle FallingArcStartAngle { get; } = RotationAngle.WrappedSteps(-3 * RotationAngle.StepsPerTurn / 8);

    /// <summary>The detached speed accumulator stops summing once it reaches this.</summary>
    public const int DetachedSpeedThreshold = 8192;
}
