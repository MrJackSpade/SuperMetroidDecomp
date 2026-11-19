namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>How near Samus a Ki-Hunter swoops, how fast it accelerates and how its detached wings orbit.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>How near, in pixels, Samus must come horizontally for a flying Ki-Hunter to swoop at her.</summary>
    public const int SwoopTriggerDistance = 96;

    /// <summary>How far below a flying Ki-Hunter, in pixels, Samus must be at least for it to swoop at her.</summary>
    public const int SwoopLeastDepth = 32;

    /// <summary>Seven eighths of a pixel per frame squared, added by both falling and hopping.</summary>
    public static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromPixelFraction(7, 8);
    /// <summary>The way a flying Ki-Hunter first flies (#2338).</summary>
    public const HorizontalDirection PatrolStartHeading = HorizontalDirection.Left;

    /// <summary>
    /// How fast a flying Ki-Hunter flies along its heading, and climbs or descends on its patrol, in whole pixels an update:
    /// it starts descending at this speed, a swoop sets off and a swoop's recovery flies back at it (#2338).
    /// </summary>
    public const int PatrolSpeed = 1;

    /// <summary>How far above where it is placed a flying Ki-Hunter's patrol turns back down, in pixels.</summary>
    public const int PatrolRise = 16;

    /// <summary>How far below its upper turn a flying Ki-Hunter's patrol turns back up, in pixels.</summary>
    public const int PatrolDepth = 32;

    /// <summary>How fast a grounded Ki-Hunter's jump carries it toward or away from Samus, in whole pixels an update.</summary>
    public const int GroundJumpHorizontalSpeed = 2;

    /// <summary>
    /// The upward speed a grounded Ki-Hunter's jump starts at, in whole pixels an update, before
    /// <see cref="GroundJumpSpeedTakenOff"/> may take one off it.
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

    /// <summary>The downward velocity a Ki-Hunter that has lost its wings starts falling at: a pixel an update (#2339).</summary>
    public static SignedSixteenSixteen WinglessFallStartVelocity { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The vertical velocity a Ki-Hunter recovering from a swoop climbs at: a pixel an update upward (#2339).</summary>
    public static SignedSixteenSixteen SwoopRecoveryVerticalVelocity { get; } = SignedSixteenSixteen.FromWholePixels(-1);

    /// <summary>
    /// The whole downward speed a hopping Ki-Hunter turns to when it strikes the room above, in pixels an update; its
    /// fraction is kept (#2339).
    /// </summary>
    public const int HopReboundSpeed = 1;

    /// <summary>How long a landed Ki-Hunter waits before it decides whether to spit or hop again, in updates (#2339).</summary>
    public const int LandingWait = 12;

    /// <summary>How long a Ki-Hunter waits after spitting before it decides again, in updates (#2339).</summary>
    public const int SpitWait = 24;

    /// <summary>How near Samus must be sideways, in pixels, for a landed Ki-Hunter to spit at her rather than hop (#2339).</summary>
    public const int SpitRange = 96;

    /// <summary>How fast spat acid flies sideways, three pixels an update, toward the way it was spat (#2339).</summary>
    public static SignedEightEight AcidHorizontalSpeed { get; } = SignedEightEight.FromPixels(3);

    /// <summary>How much faster falling acid falls each update: a sixteenth of a pixel (#2339).</summary>
    public static SignedEightEight AcidGravity { get; } = SignedEightEight.FromPixelFraction(1, 16);

    /// <summary>The fastest acid falls: two pixels an update (#2339).</summary>
    public static SignedEightEight AcidFastestFall { get; } = SignedEightEight.FromPixels(2);

    /// <summary>
    /// The place along the speed curve, mark one, a slowing detached wing lands on when its next step would take it before
    /// the curve's start (#2339).
    /// </summary>
    internal static QuadraticSpeedCurvePlace SlowestDetachedWingPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(1);
    /// <summary>The falling wings' arc radius, used by detached-wing setup and both arcs.</summary>
    public const int DetachedWingRadius = 48;

    /// <summary>
    /// The whole pixels per update, none or one, the random sample takes off a grounded Ki-Hunter's upward jump speed
    /// (#2479): its lowest bit, added to the jump's eight pixels upward.
    /// </summary>
    internal static readonly RandomSampleField GroundJumpSpeedTakenOff = new(0, 1);



    /// <summary>
    /// A swoop's angular acceleration (#2338): its turning speed grows an eighth of a step each update, toward increasing
    /// angles for a rightward swoop and decreasing ones for a leftward one.
    /// </summary>
    public static AngularSpeed SwoopAcceleration { get; } = AngularSpeed.FromSteps(1).DividedBy(8);

    /// <summary>The fastest a swoop turns, either way: its whole steps are held to two steps an update (#2338).</summary>
    public static AngularSpeed SwoopMaximumAngularVelocity { get; } = AngularSpeed.FromSteps(2);

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

    /// <summary>The detached wing orbit's starting rotation angle, an eighth of a turn below zero.</summary>
    public static RotationAngle DetachedOrbitStartAngle { get; } = RotationAngle.WrappedSteps(-RotationAngle.StepsPerTurn / 8);

    /// <summary>The detached wing's falling-arc starting rotation angle, three eighths of a turn below zero.</summary>
    public static RotationAngle FallingArcStartAngle { get; } = RotationAngle.WrappedSteps(-3 * RotationAngle.StepsPerTurn / 8);

    /// <summary>
    /// The detached wing orbit's correction (#2474): the point at the orbit's starting angle, seven eighths of a turn
    /// (<see cref="DetachedOrbitStartAngle"/>), round a circle of <see cref="DetachedWingRadius"/>, which the orbit takes
    /// off its points so it starts where the wings stand (#2536).
    /// </summary>
    public static Offset16<RoomPixels> DetachedOrbitOffset { get; } =
        EnemyMath.CircleOffset(DetachedOrbitStartAngle.ToTrigonometryAngle(), DetachedWingRadius);

    /// <summary>
    /// The detached wing's falling-arc correction (#2474): the point at the arc's starting angle, five eighths of a turn
    /// (<see cref="FallingArcStartAngle"/>), round a circle of <see cref="DetachedWingRadius"/>, which the falling arc takes
    /// off its points so it starts where the wings stand (#2536).
    /// </summary>
    public static Offset16<RoomPixels> FallingArcOffset { get; } =
        EnemyMath.CircleOffset(FallingArcStartAngle.ToTrigonometryAngle(), DetachedWingRadius);

    /// <summary>
    /// The climb that fixes where the detached wings' speed resets on the speed curve: the place, stepping a mark and a
    /// half a pass, at which the coarse speeds passed sum to 32 pixels.
    /// </summary>
    internal static QuadraticCurveClimb DetachedWingClimb { get; } =
        new(SignedSixteenSixteen.FromWholePixels(32), QuadraticSpeedCurvePlace.FromMarkFraction(3, 2));
}
