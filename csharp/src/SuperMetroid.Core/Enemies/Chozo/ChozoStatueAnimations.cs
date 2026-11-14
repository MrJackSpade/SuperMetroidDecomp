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
            Cue(new ChozoStatueCue.SetFrameBehavior(FrameBehavior: ChozoStatueFrameBehavior.WaitForLowerNorfairHandTrigger)),
            Show(EnemyFrameId.ChozoStatueLowerNorfairInitial, 1),
        ],
        [P.LowerNorfairActivated] =
        [
            Cue(new ChozoStatueCue.Signal(K.ClearFrameBehavior)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairInitial, 32),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSecond, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedThird, 48),
            Cue(new ChozoStatueCue.Signal(K.PlayGrabSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedFourth, 64),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedFifth, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSixth, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSeventh, 10),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedEighth, 12),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedNinth, 96),
            Cue(new ChozoStatueCue.Signal(K.StartLoweringAcid)),
        ],
        [P.LowerNorfairActivatedLoop] =
        [
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSixth, 11),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSeventh, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedEighth, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedNinth, 8),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedEighth, 6),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSeventh, 8),
            Cue(new ChozoStatueCue.Signal(K.ClearFrameBehavior)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedFourth, 128),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedThird, 80),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairActivatedSecond, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Right, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueLowerNorfairInitial, 32),
            Cue(new ChozoStatueCue.Signal(K.UnlockSamus)),
            Cue(new ChozoStatueCue.Signal(K.SetLoweredAcidPosition)),
        ],
        [P.WreckedShipInitial] =
        [
            Cue(new ChozoStatueCue.SetFrameBehavior(FrameBehavior: ChozoStatueFrameBehavior.WaitForWreckedShipHandTrigger)),
            Show(EnemyFrameId.ChozoStatueWreckedShipInitial, 1),
        ],
        [P.WreckedShipActivated] =
        [
            Cue(new ChozoStatueCue.Signal(K.ClearFrameBehavior)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueWreckedShipInitial, 32),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSecond, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedThird, 80),
            Cue(new ChozoStatueCue.Signal(K.PlayGrabSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedFourth, 128),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedFifth, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSixth, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSeventh, 10),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedEighth, 12),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedNinth, 128),
        ],
        [P.WreckedShipActivatedLoop] =
        [
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSixth, 11),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSeventh, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedEighth, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedNinth, 8),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedEighth, 6),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSeventh, 8),
            Cue(new ChozoStatueCue.Signal(K.ClearFrameBehavior)),
        ],
        [P.WreckedShipActivatedLoop2] =
        [
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StridePlant))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-8, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFirst, 8),
            Cue(new ChozoStatueCue.Signal(K.PlayFootstepSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideWeightShift))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-20, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingSecond, 11),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-16, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingThird, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(0, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFourth, 6),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StridePlant))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-8, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFifth, 8),
            Cue(new ChozoStatueCue.Signal(K.PlayFootstepSound)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideWeightShift))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-20, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingSixth, 11),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-16, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingSeventh, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(0, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingEighth, 6),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StridePlant))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-8, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFirst, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideWeightShift))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-20, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingSecond, 11),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-16, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingThird, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing))),
            Cue(new ChozoStatueCue.Footstep(Offset: new(0, 0))),
            Show(EnemyFrameId.ChozoStatueWreckedShipWalkingFourth, 6),
            Cue(new ChozoStatueCue.Signal(K.ClearFrameBehavior)),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Holding))),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedFourth, 128),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Gripping))),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedThird, 80),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Closing))),
            Show(EnemyFrameId.ChozoStatueWreckedShipActivatedSecond, 8),
            Cue(new ChozoStatueCue.Move(new(HorizontalDirection.Left, ChozoCarryPose.Reaching))),
            Show(EnemyFrameId.ChozoStatueWreckedShipInitial, 32),
            Cue(new ChozoStatueCue.Signal(K.ReleaseSamus)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>> StartingAt { get; } = new Dictionary<ChozoStatueSegment, Animation<ChozoStatueSegment>>
    {
        [P.LowerNorfairInitial] = new([new(P.LowerNorfairInitial, 0, 2, 1)], AnimationEnd.Holds),
        [P.LowerNorfairActivated] = new([new(P.LowerNorfairActivated, 0, 16, 1), new(P.LowerNorfairActivatedLoop, 0, 6, 5), new(P.LowerNorfairActivatedLoop, 6, 11, 1)], AnimationEnd.Holds),
        [P.WreckedShipInitial] = new([new(P.WreckedShipInitial, 0, 2, 1)], AnimationEnd.Holds),
        [P.WreckedShipActivated] = new([new(P.WreckedShipActivated, 0, 15, 1), new(P.WreckedShipActivatedLoop, 0, 6, 4), new(P.WreckedShipActivatedLoop, 6, 1, 1), new(P.WreckedShipActivatedLoop2, 0, 26, 16), new(P.WreckedShipActivatedLoop2, 26, 22, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoStatueSegment> StartingWith(ChozoStatueSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ChozoStatueCue> Cue(ChozoStatueCue cue) => new AnimationBeat<EnemyFrameId, ChozoStatueCue>.Event(cue);
}
