namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>Tested with signed distance comparison.</summary>
    public const int SwoopTriggerDistance = 96;

    /// <summary>Seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(14);
    /// <summary>The falling wings' arc radius, used by detached-wing setup and both arcs.</summary>
    public const int DetachedWingRadius = 48;

    /// <summary>
    /// The random choice that takes one pixel per update off a grounded Ki-Hunter's upward jump speed: the sample modulo 2, added to
    /// the jump's eight pixels upward.
    /// </summary>
    internal static readonly RandomSampleField ShallowerGroundJump = new(0, 1);



    /// <summary>A rightward swoop's angular acceleration: its turning speed grows an eighth of a step each update.</summary>
    public static AngularSpeed RightSwoopAcceleration { get; } = AngularSpeed.FromSteps(1).DividedBy(8);

    /// <summary>A leftward swoop's angular acceleration: its turning speed grows an eighth of a step each update toward decreasing angles.</summary>
    public static AngularSpeed LeftSwoopAcceleration { get; } = -RightSwoopAcceleration;

    /// <summary>The fastest a rightward swoop turns: its whole steps are held to two steps an update.</summary>
    public static AngularSpeed RightSwoopMaximumAngularVelocity { get; } = AngularSpeed.FromSteps(2);

    /// <summary>The fastest a leftward swoop turns: its whole steps are held to two steps an update toward decreasing angles.</summary>
    public static AngularSpeed LeftSwoopMaximumAngularVelocity { get; } = -RightSwoopMaximumAngularVelocity;

    /// <summary>
    /// Where a rightward swoop starts: a half turn, the left end of its ellipse at the Ki-Hunter's height, from which its
    /// angle climbs through the bottom of the ellipse toward the full turn at its right end that ends the swoop.
    /// </summary>
    public static RotationAngle RightSwoopStartAngle { get; } = RotationAngle.HalfTurn;

    /// <summary>
    /// Where a leftward swoop starts: one step short of a full turn, as the game's does, near the right end of its
    /// ellipse, from which its angle falls through the bottom of the ellipse toward the half turn that ends the swoop.
    /// </summary>
    public static RotationAngle LeftSwoopStartAngle { get; } = RotationAngle.WrappedSteps(-1);

    /// <summary>The angle a rightward swoop changes to its swipe animation at: a sixteenth of a turn on from its start.</summary>
    public static RotationAngle RightSwoopAnimationChangeAngle { get; } = RotationAngle.HalfTurn.TurnedBySteps(RotationAngle.StepsPerTurn / 16);

    /// <summary>
    /// The angle a leftward swoop changes to its swipe animation once it falls below: a sixteenth of a turn short of a
    /// full turn.
    /// </summary>
    public static RotationAngle LeftSwoopAnimationChangeAngle { get; } = RotationAngle.WrappedSteps(-RotationAngle.StepsPerTurn / 16);

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
