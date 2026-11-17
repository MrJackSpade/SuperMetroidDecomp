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
        [P.LowerNorfairInitial] =
        [
            Cue(new ChozoStatueCue.Signal(K.AwaitHand)),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandsCupped, 1),
        ],
        [P.LowerNorfairActivated] =
        [
            Cue(new ChozoStatueCue.Signal(K.StopAwaitingHand)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandsCupped, 32),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandReaching, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOutstretched, 48),
            Cue(new ChozoStatueCue.Signal(K.PlayGrabSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpen, 64),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpen, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpenEyeGlowing, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurling, 10),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurled, 12),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersClenched, 96),
            Cue(new ChozoStatueCue.Signal(K.StartLoweringAcid)),
        ],
        [P.LowerNorfairFlexing] =
        [
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpenEyeGlowing, 11),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurling, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersClenched, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurling, 8),
        ],
        [P.LowerNorfairRelease] =
        [
            Cue(new ChozoStatueCue.Signal(K.StopAwaitingHand)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpen, 128),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOutstretched, 80),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandReaching, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandsCupped, 32),
            Cue(new ChozoStatueCue.Signal(K.UnlockSamus)),
            Cue(new ChozoStatueCue.Signal(K.SetLoweredAcidPosition)),
        ],
        [P.WreckedShipInitial] =
        [
            Cue(new ChozoStatueCue.Signal(K.AwaitHand)),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandsCupped, 1),
        ],
        [P.WreckedShipActivated] =
        [
            Cue(new ChozoStatueCue.Signal(K.StopAwaitingHand)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandsCupped, 32),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandReaching, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOutstretched, 80),
            Cue(new ChozoStatueCue.Signal(K.PlayGrabSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpen, 128),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpen, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpenEyeGlowing, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurling, 10),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurled, 12),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersClenched, 128),
        ],
        [P.WreckedShipFlexing] =
        [
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpenEyeGlowing, 11),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurling, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersClenched, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurling, 8),
        ],
        [P.WreckedShipFlexEnd] =
        [
            Cue(new ChozoStatueCue.Signal(K.StopAwaitingHand)),
        ],
        [P.WreckedShipWalkingWithSamus] =
        [
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StridePlant))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -8)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegLifting, 8),
            Cue(new ChozoStatueCue.Signal(K.PlayFootstepSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideWeightShift))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -20)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegStriding, 11),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -16)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegPlanting, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: 0)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegPlanted, 6),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StridePlant))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -8)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegLifting, 8),
            Cue(new ChozoStatueCue.Signal(K.PlayFootstepSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideWeightShift))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -20)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegStriding, 11),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -16)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegPlanting, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: 0)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegPlanted, 6),
        ],
        [P.WreckedShipSettingSamusDown] =
        [
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StridePlant))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -8)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegLifting, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideWeightShift))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -20)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegStriding, 11),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: -16)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegPlanting, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing))),
            Cue(new ChozoStatueCue.Footstep(OffsetX: 0)),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegPlanted, 6),
            Cue(new ChozoStatueCue.Signal(K.StopAwaitingHand)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpen, 128),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOutstretched, 80),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandReaching, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandsCupped, 32),
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
    internal static IReadOnlyDictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>> StartingAt { get; } = new Dictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>>
    {
        [P.LowerNorfairInitial] = new([Whole(P.LowerNorfairInitial)], AnimationEnd.Holds),
        [P.LowerNorfairActivated] = new(
            [Whole(P.LowerNorfairActivated), Repeated(P.LowerNorfairFlexing, LowerNorfairFlexes), Whole(P.LowerNorfairRelease)],
            AnimationEnd.Holds),
        [P.WreckedShipInitial] = new([Whole(P.WreckedShipInitial)], AnimationEnd.Holds),
        [P.WreckedShipActivated] = new(
            [Whole(P.WreckedShipActivated), Repeated(P.WreckedShipFlexing, WreckedShipFlexes), Whole(P.WreckedShipFlexEnd),
                Repeated(P.WreckedShipWalkingWithSamus, WreckedShipStrideRuns), Whole(P.WreckedShipSettingSamusDown)],
            AnimationEnd.Holds),
    };

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<ChozoStatueSegment> Whole(ChozoStatueSegment segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<ChozoStatueSegment> Repeated(ChozoStatueSegment segment, int plays) =>
        new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoStatueSegment> StartingWith(ChozoStatueSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Cue(ChozoStatueCue cue) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Event(cue);
}
