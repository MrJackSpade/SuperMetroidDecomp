using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's tail as it is made, and the swings, spacings, reaches and extensions its actions set (#2238).</summary>
internal static class RidleyTailDefinitions
{
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
    /// The pogo tail's swing as its action sets it up: none. Both pogo tail setups leave the tail unswung.
    /// </summary>
    internal static RidleyTailTurn PogoSwing => RidleyTailTurn.None;

    /// <summary>
    /// The tail's usual extension speed, fifteen sixteenths of a pixel an update: set at initialization and restored by
    /// the ground attack and on carry phase expiry.
    /// </summary>
    internal static SignedEightEight StandardExtensionSpeed { get; } = SignedEightEight.FromPixelFraction(15, 16);
    /// <summary>The tail's extension speed while Ridley pogoes: one and a half pixels an update.</summary>
    internal static SignedEightEight PogoExtensionSpeed { get; } = SignedEightEight.FromPixelFraction(3, 2);
    /// <summary>How far a link longer than its rest length shrinks each update: half a pixel.</summary>
    internal static SignedEightEight ShrinkStep { get; } = SignedEightEight.FromPixelFraction(1, 2);
    /// <summary>Straight-down target angle.</summary>
    internal static RidleyTailAngle DownAngle { get; } = RidleyTailAngle.StraightDown;
    /// <summary>Pogo clockwise whip target: straight down a whole turn clockwise, so the whip swings the tail round once.</summary>
    internal static RidleyTailAngle PogoWhipAngle { get; } = RidleyTailAngle.StraightDown.TurnedByWholeTurns(-1);
    /// <summary>The whip's extension request while a link chases a whip target: twelve pixels.</summary>
    internal static SignedEightEight WhipDistance { get; } = SignedEightEight.FromPixels(12);
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

    /// <summary>How far below the tip the pogo's floor check probes.</summary>
    internal static readonly Offset16<RoomPixels> TipFloorProbe = new(0, 16);

    /// <summary>How far below each probed shaft link the pogo's floor check probes.</summary>
    internal static readonly Offset16<RoomPixels> ShaftFloorProbe = new(0, 18);

    /// <summary>The damage Ceres Ridley's tail tip does Samus on touch.</summary>
    internal const int CeresTipDamage = 15;

    /// <summary>The damage Lower Norfair Ridley's tail tip does Samus on touch.</summary>
    internal const int NorfairTipDamage = 120;

    /// <summary>Where the tail's hip lies below Ridley's body.</summary>
    internal static readonly Offset16<RoomPixels> HipBelowBody = new(0, 16);

    /// <summary>How far behind his body the hip lies while Ridley faces left; facing right mirrors it, and facing the screen centres it.</summary>
    internal static readonly Offset16<RoomPixels> HipBehindBodyFacingLeft = new(32, 0);

    /// <summary>How near Samus Ridley must be horizontally, exclusive, for his tail to whip at her unprompted.</summary>
    internal const int WhipRange = 128;

    /// <summary>Where on Samus an aimed whip aims, below her position.</summary>
    internal static readonly Offset16<RoomPixels> WhipAimOnSamus = new(0, 24);

    /// <summary>How near the tip a missile's edge must come, on each axis, exclusive, for the whip to aim at the missile instead.</summary>
    internal const int MissileWhipReach = 64;

    /// <summary>How far past Samus's own reach, on each axis, the tip touches her.</summary>
    internal const int TipTouchReach = 14;

    /// <summary>The reach of the tip's armour against Samus's shots.</summary>
    internal const int TipArmourReach = 14;

    /// <summary>The reach of the inner joint's armour against Samus's shots.</summary>
    internal const int InnerJointArmourReach = 10;

    /// <summary>Descending stab extension request: ten pixels.</summary>
    internal static SignedEightEight StabDistance { get; } = SignedEightEight.FromPixels(10);
    /// <summary>Bounce extension request: twelve pixels.</summary>
    internal static SignedEightEight BounceDistance { get; } = SignedEightEight.FromPixels(12);
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

    /// <summary>The base, two pixels from the hip, pointing straight down; it holds its length and is drawn as the base link.</summary>
    internal static RidleyDrawnTailLinkDefinition BaseLink { get; } =
        new(new(SignedEightEight.FromPixels(2), SignedEightEight.FromPixels(2), RidleyTailAngle.StraightDown), RidleySupplementalVisualDefinitions.BaseLink);

    /// <summary>
    /// The five shaft links from the body out, each eight pixels from the link before and starting one standard spacing
    /// further counterclockwise; the two nearest the body stretch to 24 pixels, the next two to 22 and the last to 18.
    /// The first is drawn as a base link, the next two as middle links and the last two as outer links.
    /// </summary>
    internal static IReadOnlyList<RidleyDrawnTailLinkDefinition> ShaftLinks { get; } =
    [
        new(new(SignedEightEight.FromPixels(8), SignedEightEight.FromPixels(24), RidleyTailAngle.StraightDown.TurnedCounterclockwise(16)), RidleySupplementalVisualDefinitions.BaseLink),
        new(new(SignedEightEight.FromPixels(8), SignedEightEight.FromPixels(24), RidleyTailAngle.StraightDown.TurnedCounterclockwise(32)), RidleySupplementalVisualDefinitions.MiddleLink),
        new(new(SignedEightEight.FromPixels(8), SignedEightEight.FromPixels(22), RidleyTailAngle.StraightDown.TurnedCounterclockwise(48)), RidleySupplementalVisualDefinitions.MiddleLink),
        new(new(SignedEightEight.FromPixels(8), SignedEightEight.FromPixels(22), RidleyTailAngle.StraightDown.TurnedCounterclockwise(64)), RidleySupplementalVisualDefinitions.OuterLink),
        new(new(SignedEightEight.FromPixels(8), SignedEightEight.FromPixels(18), RidleyTailAngle.StraightDown.TurnedCounterclockwise(80)), RidleySupplementalVisualDefinitions.OuterLink),
    ];

    /// <summary>The tip, five pixels from the last shaft link, starting one standard spacing further round; it never stretches.</summary>
    internal static RidleyTailLinkDefinition TipLink { get; } =
        new(SignedEightEight.FromPixels(5), SignedEightEight.FromPixels(5), RidleyTailAngle.StraightDown.TurnedCounterclockwise(96));
}
