namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Ki-Hunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>Tested with signed distance comparison.</summary>
    public const int SwoopTriggerDistance = 96;

    /// <summary>Seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(14);
    /// <summary>A flying Ki-Hunter's first velocity: a pixel an update left and a pixel an update down.</summary>
    public static Offset16<RoomPixels> PatrolStartVelocity { get; } = new(-1, 1);

    /// <summary>How far above where it is placed a flying Ki-Hunter's patrol turns back down, in pixels.</summary>
    public const int PatrolRise = 16;

    /// <summary>How far below its upper turn a flying Ki-Hunter's patrol turns back up, in pixels.</summary>
    public const int PatrolDepth = 32;

    /// <summary>How fast a grounded Ki-Hunter's jump carries it toward or away from Samus, in whole pixels an update.</summary>
    public const int GroundJumpHorizontalSpeed = 2;

    /// <summary>
    /// The upward speed a grounded Ki-Hunter's jump starts at, in whole pixels an update, before
    /// <see cref="ShallowerGroundJump"/> may take one off it.
    /// </summary>
    public const int GroundJumpUpwardSpeed = 8;

    /// <summary>The vertical velocity a grounded Ki-Hunter keeps once it lands: four pixels an update upward.</summary>
    public static SignedSixteenSixteen LandedVerticalVelocity { get; } = SignedSixteenSixteen.FromWholePixels(-4);

    /// <summary>How far ahead of the Ki-Hunter's body its acid spit appears, in pixels.</summary>
    public const int AcidSpitReach = 22;

    /// <summary>How far above the Ki-Hunter's body its acid spit appears, in pixels.</summary>
    public const int AcidSpitHeight = 16;

    /// <summary>How far ahead the acid spit jumps as its launch starts, in pixels.</summary>
    public const int AcidLaunchShift = 19;

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

    /// <summary>
    /// The climb that fixes where the detached wings' speed resets on the speed curve: the place, stepping a mark and a
    /// half a pass, at which the coarse speeds passed sum to 32 pixels.
    /// </summary>
    internal static QuadraticCurveClimb DetachedWingClimb { get; } =
        new(SignedSixteenSixteen.FromWholePixels(32), QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(384));
}
