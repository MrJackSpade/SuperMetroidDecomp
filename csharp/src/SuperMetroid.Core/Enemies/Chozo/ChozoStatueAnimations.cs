using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Chozo.ChozoStatueCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoStatueSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair and Wrecked Ship Chozo statues' idle and activation animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="ChozoStatueCue"/> with the values it acts on.
/// </summary>
internal static class ChozoStatueAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChozoStatueSegment, EnemyFrameId, ChozoStatueCue> Segments { get; } = new(
        new Dictionary<ChozoStatueSegment, AnimationBeat<EnemyFrameId, ChozoStatueCue>[]>
    {
        [P.LowerNorfairIdle] =
        [
            Cue(new ChozoStatueCue.Signal(K.AwaitHand)),
            Hold(EnemyFrameId.ChozoStatueLowerNorfairHandsCupped),
        ],
        [P.LowerNorfairActivated] =
        [
            .. Grab(ChozoStatueHand.LowerNorfair, outstretchedUpdates: 48, openUpdates: 70, clenchedUpdates: 96),
            Cue(new ChozoStatueCue.Signal(K.StartLoweringAcid)),
        ],
        [P.LowerNorfairFlexing] = Flex(ChozoStatueHand.LowerNorfair),
        [P.LowerNorfairRelease] =
        [
            .. Release(ChozoStatueHand.LowerNorfair),
            Cue(new ChozoStatueCue.Signal(K.UnlockSamus)),
            Cue(new ChozoStatueCue.Signal(K.SetLoweredAcidPosition)),
        ],
        [P.WreckedShipIdle] =
        [
            Cue(new ChozoStatueCue.Signal(K.AwaitHand)),
            Hold(EnemyFrameId.ChozoStatueWreckedShipHandsCupped),
        ],
        [P.WreckedShipActivated] = Grab(ChozoStatueHand.WreckedShip, outstretchedUpdates: 80, openUpdates: 134, clenchedUpdates: 128),
        [P.WreckedShipFlexing] = Flex(ChozoStatueHand.WreckedShip),
        [P.WreckedShipWalkingWithSamus] =
        [
            .. Step(ChozoStatueHand.WreckedShip.Carries, ChozoStatueLeg.WreckedShipNear),
            .. Step(ChozoStatueHand.WreckedShip.Carries, ChozoStatueLeg.WreckedShipFar),
        ],
        [P.WreckedShipSettingSamusDown] =
        [
            .. QuietStep(ChozoStatueHand.WreckedShip.Carries, ChozoStatueLeg.WreckedShipNear),
            .. Release(ChozoStatueHand.WreckedShip),
            Cue(new ChozoStatueCue.Signal(K.ReleaseSamus)),
        ],
    });

    /// <summary>How many times the Lower Norfair statue flexes its fingers before it releases Samus.</summary>
    private const int LowerNorfairFlexes = 5;

    /// <summary>How many times the Wrecked Ship statue flexes its fingers before it walks.</summary>
    private const int WreckedShipFlexes = 4;

    /// <summary>How many times the Wrecked Ship statue takes its four strides with Samus in its hand.</summary>
    private const int WreckedShipStrideRuns = 16;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): each statue grabs Samus, flexes
    /// its fingers a counted number of times and lets her go; the Wrecked Ship statue walks its counted strides with her
    /// before it sets her down.
    /// </summary>
    private static IReadOnlyDictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>> StartingAt { get; } = new Dictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>>
    {
        [P.LowerNorfairIdle] = new([Segments.Whole(P.LowerNorfairIdle)], AnimationEnd.Holds),
        [P.LowerNorfairActivated] = new(
            [Segments.Whole(P.LowerNorfairActivated), Segments.Repeated(P.LowerNorfairFlexing, LowerNorfairFlexes), Segments.Whole(P.LowerNorfairRelease)],
            AnimationEnd.Holds),
        [P.WreckedShipIdle] = new([Segments.Whole(P.WreckedShipIdle)], AnimationEnd.Holds),
        [P.WreckedShipActivated] = new(
            [Segments.Whole(P.WreckedShipActivated), Segments.Repeated(P.WreckedShipFlexing, WreckedShipFlexes),
                Segments.Repeated(P.WreckedShipWalkingWithSamus, WreckedShipStrideRuns), Segments.Whole(P.WreckedShipSettingSamusDown)],
            AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoStatueSegment> StartingWith(ChozoStatueSegment segment) => StartingAt[segment];

    /// <summary>
    /// <paramref name="hand"/> taking Samus (#2477): reaching from its cupped hands, gripping her with its grab sound, holding
    /// her open-handed and clenching its fingers, moving her its way at each pose. Its hand shows outstretched for
    /// <paramref name="outstretchedUpdates"/> updates, open for <paramref name="openUpdates"/> and clenched for
    /// <paramref name="clenchedUpdates"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] Grab(ChozoStatueHand hand, int outstretchedUpdates, int openUpdates, int clenchedUpdates) =>
    [
        Carry(hand, ChozoCarryPose.Reaching),
        Show(hand.HandsCupped, 32),
        Carry(hand, ChozoCarryPose.Closing),
        Show(hand.HandReaching, 8),
        Carry(hand, ChozoCarryPose.Gripping),
        Show(hand.HandOutstretched, outstretchedUpdates),
        Cue(new ChozoStatueCue.Signal(K.PlayGrabSound)),
        Carry(hand, ChozoCarryPose.Holding),
        Show(hand.HandOpen, openUpdates),
        Show(hand.HandOpenEyeGlowing, 8),
        Show(hand.FingersCurling, 10),
        Show(hand.FingersCurled, 12),
        Show(hand.FingersClenched, clenchedUpdates),
    ];

    /// <summary>One flex of <paramref name="hand"/>'s fingers (#2477), its eye glowing: curling, curled, clenched and back.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] Flex(ChozoStatueHand hand) =>
    [
        Show(hand.HandOpenEyeGlowing, 11),
        Show(hand.FingersCurling, 8),
        Show(hand.FingersCurled, 6),
        Show(hand.FingersClenched, 8),
        Show(hand.FingersCurled, 6),
        Show(hand.FingersCurling, 8),
    ];

    /// <summary>
    /// <paramref name="hand"/> letting Samus go (#2477): the grab played back from open to cupped, moving her its way at each
    /// pose.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] Release(ChozoStatueHand hand) =>
    [
        Carry(hand, ChozoCarryPose.Holding),
        Show(hand.HandOpen, 128),
        Carry(hand, ChozoCarryPose.Gripping),
        Show(hand.HandOutstretched, 80),
        Carry(hand, ChozoCarryPose.Closing),
        Show(hand.HandReaching, 8),
        Carry(hand, ChozoCarryPose.Reaching),
        Show(hand.HandsCupped, 32),
    ];

    /// <summary>
    /// One step of <paramref name="leg"/> walking toward <paramref name="walking"/> (#2477): the leg lifting, the footstep's
    /// sound, and the leg striding and planting.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] Step(HorizontalDirection walking, ChozoStatueLeg leg) =>
        [.. Lifting(walking, leg), Cue(new ChozoStatueCue.Signal(K.PlayFootstepSound)), .. Planting(walking, leg)];

    /// <summary>
    /// One step of <paramref name="leg"/> walking toward <paramref name="walking"/> with no footstep's sound (#2477), as the
    /// statue's last stride lowers Samus.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] QuietStep(HorizontalDirection walking, ChozoStatueLeg leg) =>
        [.. Lifting(walking, leg), .. Planting(walking, leg)];

    /// <summary><paramref name="leg"/> lifting as the statue plants its weight, Samus and her footing moving with it.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] Lifting(HorizontalDirection walking, ChozoStatueLeg leg) =>
    [
        Cue(new ChozoStatueCue.Move(new(walking, ChozoCarryPose.StridePlant))),
        Cue(new ChozoStatueCue.Footstep(OffsetX: -8)),
        Show(leg.Lifting, 8),
    ];

    /// <summary><paramref name="leg"/> striding, planting and planted, Samus and her footing moving with each pose.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue>[] Planting(HorizontalDirection walking, ChozoStatueLeg leg) =>
    [
        Cue(new ChozoStatueCue.Move(new(walking, ChozoCarryPose.StrideWeightShift))),
        Cue(new ChozoStatueCue.Footstep(OffsetX: -20)),
        Show(leg.Striding, 11),
        Cue(new ChozoStatueCue.Move(new(walking, ChozoCarryPose.StrideLift))),
        Cue(new ChozoStatueCue.Footstep(OffsetX: -16)),
        Show(leg.Planting, 8),
        Cue(new ChozoStatueCue.Move(new(walking, ChozoCarryPose.StrideSwing))),
        Cue(new ChozoStatueCue.Footstep(OffsetX: 0)),
        Show(leg.Planted, 6),
    ];

    /// <summary>Moves Samus in <paramref name="hand"/> to <paramref name="pose"/>, the way the hand carries her.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Carry(ChozoStatueHand hand, ChozoCarryPose pose) =>
        Cue(new ChozoStatueCue.Move(new(hand.Carries, pose)));

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2369): a waiting statue rests on its cupped hands until Samus is in them.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Cue(ChozoStatueCue cue) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Event(cue);
}
