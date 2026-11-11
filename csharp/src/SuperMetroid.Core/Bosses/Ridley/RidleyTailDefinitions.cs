namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Initial articulated-tail angular geometry from the tail's initialization.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>Tail initialization writes this bound independently of facing: sixteen steps clockwise of straight down.</summary>
    internal static RidleyTailAngle InitialMinimumClockwise { get; } = RidleyTailAngle.StraightDown.TurnedClockwise(16);
    /// <summary>Tail initialization writes this bound before the live tail controller adjusts it: a quarter turn counterclockwise of straight down.</summary>
    internal static RidleyTailAngle InitialMaximumCounterClockwise { get; } = RidleyTailAngle.StraightDown.TurnedCounterclockwise(64);

    /// <summary>The carry's release sets the ideal tail spacing while releasing Samus.</summary>
    internal const int CarryReleaseInterSegmentSpacing = 8;
    /// <summary>
    /// The tail's usual extension speed, fifteen sixteenths of a pixel an update: set at initialization and restored by
    /// the ground attack and on carry phase expiry.
    /// </summary>
    internal static SignedEightEight StandardExtensionSpeed { get; } = SignedEightEight.FromParts(0, 240);
    /// <summary>The tail's extension speed while Ridley pogoes: one and a half pixels an update.</summary>
    internal static SignedEightEight PogoExtensionSpeed { get; } = SignedEightEight.FromParts(1, 128);
    /// <summary>How far a link longer than its rest length shrinks each update: half a pixel.</summary>
    internal static SignedEightEight ShrinkStep { get; } = SignedEightEight.FromParts(0, 128);
    /// <summary>Straight-down target angle.</summary>
    internal static RidleyTailAngle DownAngle { get; } = RidleyTailAngle.StraightDown;
    /// <summary>Pogo clockwise whip target: straight down a whole turn clockwise, so the whip swings the tail round once.</summary>
    internal static RidleyTailAngle PogoWhipAngle { get; } = RidleyTailAngle.StraightDown.TurnedByWholeTurns(-1);
    /// <summary>The whip's extension request while a segment chases a whip target: twelve pixels.</summary>
    internal static SignedEightEight WhipDistance { get; } = SignedEightEight.FromParts(12, 0);
    /// <summary>The first direction of the forward interval a whip may not aim through Ridley's body.</summary>
    internal static TrigonometryAngle ForbiddenForwardStart { get; } = new(24);
    /// <summary>The direction past the forbidden forward interval; a left-facing target clamps here.</summary>
    internal static TrigonometryAngle ForbiddenForwardEnd { get; } = new(232);
    /// <summary>
    /// The angle a left-facing (clockwise) whip target's direction is counted from: straight down a whole turn
    /// clockwise, so the target lies in the turn that ends at straight down.
    /// </summary>
    internal static RidleyTailAngle LeftWhipTargetBase { get; } = RidleyTailAngle.StraightDown.TurnedByWholeTurns(-1);
    /// <summary>The angle a right-facing (counterclockwise) whip target's direction is counted from: straight down, so the target lies in the turn that starts there.</summary>
    internal static RidleyTailAngle RightWhipTargetBase { get; } = RidleyTailAngle.StraightDown;

    /// <summary>The longest each segment may extend while whipping, base through tip.</summary>
    internal static readonly SignedEightEight[] MaximumDistances =
    [
        SignedEightEight.Zero, SignedEightEight.FromParts(24, 0), SignedEightEight.FromParts(24, 0), SignedEightEight.FromParts(22, 0),
        SignedEightEight.FromParts(22, 0), SignedEightEight.FromParts(18, 0), SignedEightEight.FromParts(5, 0),
    ];
    /// <summary>Descending stab extension request: ten pixels.</summary>
    internal static SignedEightEight StabDistance { get; } = SignedEightEight.FromParts(10, 0);
    /// <summary>Bounce extension request: twelve pixels.</summary>
    internal static SignedEightEight BounceDistance { get; } = SignedEightEight.FromParts(12, 0);
    /// <summary>The tail's minimum clockwise angle for left/front/right facing: sixteen steps clockwise of straight down, or a quarter turn facing right.</summary>
    internal static readonly Dictionary<RidleyFacing, RidleyTailAngle> MinimumClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RidleyTailAngle.StraightDown.TurnedClockwise(16),
        [RidleyFacing.Turning] = RidleyTailAngle.StraightDown.TurnedClockwise(16),
        [RidleyFacing.Right] = RidleyTailAngle.StraightDown.TurnedClockwise(64),
    };
    /// <summary>The tail's maximum counter-clockwise angle for left/front/right facing: a quarter turn counterclockwise of straight down, or sixteen steps facing right.</summary>
    internal static readonly Dictionary<RidleyFacing, RidleyTailAngle> MaximumCounterClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RidleyTailAngle.StraightDown.TurnedCounterclockwise(64),
        [RidleyFacing.Turning] = RidleyTailAngle.StraightDown.TurnedCounterclockwise(64),
        [RidleyFacing.Right] = RidleyTailAngle.StraightDown.TurnedCounterclockwise(16),
    };

    /// <summary>Tail initialization sets Ridley's ideal inter-segment tail angle to 16; the initial angles advance by that separation.</summary>
    internal const int IdealInterSegmentSpacing = 16;
    /// <summary>The initial base separation: two pixels. The length is an authored scalar (see the residual scalar inputs review).</summary>
    private static readonly SignedEightEight BaseRestDistance = SignedEightEight.FromParts(2, 0);
    /// <summary>The initial shaft separation and its shrink thresholds: eight pixels. The length is an authored scalar.</summary>
    private static readonly SignedEightEight ShaftRestDistance = SignedEightEight.FromParts(8, 0);
    /// <summary>The initial tip separation and its shrink threshold: five pixels. The length is an authored scalar.</summary>
    private static readonly SignedEightEight TipRestDistance = SignedEightEight.FromParts(5, 0);

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static readonly SignedEightEight[] RestDistances =
        [BaseRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, TipRestDistance];

    /// <summary>The seven successive initial link angles, base through tip: straight down, then each one ideal separation further counterclockwise.</summary>
    internal static readonly RidleyTailAngle[] InitialAngles =
    [
        RidleyTailAngle.StraightDown,
        RidleyTailAngle.StraightDown.TurnedCounterclockwise(IdealInterSegmentSpacing),
        RidleyTailAngle.StraightDown.TurnedCounterclockwise(2 * IdealInterSegmentSpacing),
        RidleyTailAngle.StraightDown.TurnedCounterclockwise(3 * IdealInterSegmentSpacing),
        RidleyTailAngle.StraightDown.TurnedCounterclockwise(4 * IdealInterSegmentSpacing),
        RidleyTailAngle.StraightDown.TurnedCounterclockwise(5 * IdealInterSegmentSpacing),
        RidleyTailAngle.StraightDown.TurnedCounterclockwise(6 * IdealInterSegmentSpacing),
    ];
}
