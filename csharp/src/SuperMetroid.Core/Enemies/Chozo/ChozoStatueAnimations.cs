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
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpening, 64),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpen, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpenEyeGlowing, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurling, 10),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurled, 12),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersClenched, 96),
            Cue(new ChozoStatueCue.Signal(K.StartLoweringAcid)),
        ],
        [P.LowerNorfairActivatedLoop] =
        [
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpenEyeGlowing, 11),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurling, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersClenched, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairFingersCurling, 8),
            Cue(new ChozoStatueCue.Signal(K.StopAwaitingHand)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairHandOpening, 128),
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
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpening, 128),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpen, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpenEyeGlowing, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurling, 10),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurled, 12),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersClenched, 128),
        ],
        [P.WreckedShipActivatedLoop] =
        [
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpenEyeGlowing, 11),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurling, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersClenched, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurled, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipFingersCurling, 8),
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
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOpening, 128),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandOutstretched, 80),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandReaching, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueWreckedShipHandsCupped, 32),
            Cue(new ChozoStatueCue.Signal(K.ReleaseSamus)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>> StartingAt { get; } = new Dictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>>
    {
        [P.LowerNorfairInitial] = new([new(P.LowerNorfairInitial, 0, 2, 1)], AnimationEnd.Holds),
        [P.LowerNorfairActivated] = new([new(P.LowerNorfairActivated, 0, 16, 1), new(P.LowerNorfairActivatedLoop, 0, 6, 5), new(P.LowerNorfairActivatedLoop, 6, 11, 1)], AnimationEnd.Holds),
        [P.WreckedShipInitial] = new([new(P.WreckedShipInitial, 0, 2, 1)], AnimationEnd.Holds),
        [P.WreckedShipActivated] = new([new(P.WreckedShipActivated, 0, 15, 1), new(P.WreckedShipActivatedLoop, 0, 6, 4), new(P.WreckedShipActivatedLoop, 6, 1, 1), new(P.WreckedShipWalkingWithSamus, 0, 26, 16), new(P.WreckedShipWalkingWithSamus, 26, 22, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoStatueSegment> StartingWith(ChozoStatueSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Cue(ChozoStatueCue cue) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Event(cue);
}
