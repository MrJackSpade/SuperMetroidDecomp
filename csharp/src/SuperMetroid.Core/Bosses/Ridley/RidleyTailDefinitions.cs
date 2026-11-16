namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's tail as it is made, and the swings, spacings, reaches and extensions its controllers set (#2238).</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>Tail initialization writes this bound independently of facing: sixteen steps clockwise of straight down.</summary>
    internal static RidleyTailAngle InitialMinimumClockwise { get; } = RidleyTailAngle.StraightDown.TurnedClockwise(16);
    /// <summary>Tail initialization writes this bound before the live tail controller adjusts it: a quarter turn counterclockwise of straight down.</summary>
    internal static RidleyTailAngle InitialMaximumCounterClockwise { get; } = RidleyTailAngle.StraightDown.TurnedCounterclockwise(64);

    /// <summary>
    /// The spacing the tail's links keep as it is made and through most of the fight: each link starts the next once it
    /// has swung sixteen steps, and each link starts sixteen steps counterclockwise of the one before.
    /// </summary>
    internal static RidleyTailTurn StandardLinkSpacing { get; } = RidleyTailTurn.OfSteps(16);
    /// <summary>The tighter spacing the wall sweep curls the tail with: eleven steps.</summary>
    internal static RidleyTailTurn WallSweepLinkSpacing { get; } = RidleyTailTurn.OfSteps(11);
    /// <summary>The spacing the carry's release sets while releasing Samus: eight steps.</summary>
    internal static RidleyTailTurn CarryReleaseLinkSpacing { get; } = RidleyTailTurn.OfSteps(8);
    /// <summary>The spacing Ceres Ridley's least curly tail keeps: eight steps, to which his curliness roll adds up to fifteen.</summary>
    internal static RidleyTailTurn LeastCurlyLinkSpacing { get; } = RidleyTailTurn.OfSteps(8);
    /// <summary>How far each link has already swung as the tail is made, toward starting the next: seventeen steps.</summary>
    internal static RidleyTailTurn InitialStagger { get; } = RidleyTailTurn.OfSteps(17);

    /// <summary>
    /// The neutral tail's slow swing, a step an update: while Samus stands left of <see cref="FastNeutralSwingFromX"/>,
    /// as the tail is made, and as the pogo, a grab and a Power Bomb's lunge give the tail back to its neutral swing.
    /// </summary>
    internal static RidleyTailTurn SlowNeutralSwing { get; } = RidleyTailTurn.OfSteps(1);
    /// <summary>The neutral tail's fast swing, two steps an update, while Samus stands at or right of <see cref="FastNeutralSwingFromX"/>.</summary>
    internal static RidleyTailTurn FastNeutralSwing { get; } = RidleyTailTurn.OfSteps(2);
    /// <summary>The room X from which a new neutral wave swings fast.</summary>
    internal const int FastNeutralSwingFromX = 112;
    /// <summary>The swing of an aimed whip, and of the tail pointing down for the pogo: eight steps an update.</summary>
    internal static RidleyTailTurn WhipSwing { get; } = RidleyTailTurn.OfSteps(8);
    /// <summary>The swing of the downward stab, slower than its setup: three steps an update.</summary>
    internal static RidleyTailTurn StabSwing { get; } = RidleyTailTurn.OfSteps(3);
    /// <summary>
    /// The pogo tail's swing as its controller sets it up: none. Its activation timer is made at zero and never
    /// given another value, so both pogo tail setups write zero.
    /// </summary>
    internal static RidleyTailTurn PogoSwing => RidleyTailTurn.None;

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
    /// <summary>The whip's extension request while a link chases a whip target: twelve pixels.</summary>
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

    /// <summary>How many shaft links, counted from the tip inward, the pogo's floor check probes after the tip.</summary>
    internal const int FloorProbingShaftLinks = 4;

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

    /// <summary>The base, two pixels from the hip, pointing straight down; it holds its length.</summary>
    internal static RidleyTailLinkDefinition BaseLink { get; } =
        new(SignedEightEight.FromParts(2, 0), SignedEightEight.FromParts(2, 0), RidleyTailAngle.StraightDown);

    /// <summary>
    /// The five shaft links from the body out, each eight pixels from the link before and starting one standard spacing
    /// further counterclockwise; the two nearest the body stretch to 24 pixels, the next two to 22 and the last to 18.
    /// </summary>
    internal static IReadOnlyList<RidleyTailLinkDefinition> ShaftLinks { get; } =
    [
        new(SignedEightEight.FromParts(8, 0), SignedEightEight.FromParts(24, 0), RidleyTailAngle.StraightDown.TurnedCounterclockwise(16)),
        new(SignedEightEight.FromParts(8, 0), SignedEightEight.FromParts(24, 0), RidleyTailAngle.StraightDown.TurnedCounterclockwise(32)),
        new(SignedEightEight.FromParts(8, 0), SignedEightEight.FromParts(22, 0), RidleyTailAngle.StraightDown.TurnedCounterclockwise(48)),
        new(SignedEightEight.FromParts(8, 0), SignedEightEight.FromParts(22, 0), RidleyTailAngle.StraightDown.TurnedCounterclockwise(64)),
        new(SignedEightEight.FromParts(8, 0), SignedEightEight.FromParts(18, 0), RidleyTailAngle.StraightDown.TurnedCounterclockwise(80)),
    ];

    /// <summary>The tip, five pixels from the last shaft link, starting one standard spacing further round; it never stretches.</summary>
    internal static RidleyTailLinkDefinition TipLink { get; } =
        new(SignedEightEight.FromParts(5, 0), SignedEightEight.FromParts(5, 0), RidleyTailAngle.StraightDown.TurnedCounterclockwise(96));
}
