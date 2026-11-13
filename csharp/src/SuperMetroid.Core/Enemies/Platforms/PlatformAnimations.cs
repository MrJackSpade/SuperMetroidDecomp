using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Platforms.PlatformCueKind;
using P = SuperMetroid.Core.Enemies.Platforms.PlatformSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and Kamer's vertically moving and still platform loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="PlatformCue"/> with its typed operands.
/// </summary>
internal static class PlatformAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PlatformSegment, EnemyFrameId, PlatformCue> Segments { get; } = new(
        new Dictionary<PlatformSegment, AnimationBeat<EnemyFrameId, PlatformCue>[]>
    {
        [P.KamerMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.KamerMovingLeftLoop] =
        [
            Show(EnemyFrameId.KamerMovingLeftLoopFirst, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopSecond, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopThird, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopFourth, 10),
        ],
        [P.KamerMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.KamerMovingRightLoop] =
        [
            Show(EnemyFrameId.KamerMovingLeftLoopFirst, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopSecond, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopThird, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopFourth, 10),
        ],
        [P.KamerStillLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.KamerStillLeftLoop] =
        [
            Show(EnemyFrameId.KamerMovingLeftLoopFirst, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopSecond, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopThird, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopFourth, 10),
        ],
        [P.KamerStillRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.KamerStillRightLoop] =
        [
            Show(EnemyFrameId.KamerMovingLeftLoopFirst, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopSecond, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopThird, 10),
            Show(EnemyFrameId.KamerMovingLeftLoopFourth, 10),
        ],
        [P.TripperMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.TripperMovingLeftLoop] =
        [
            Show(EnemyFrameId.TripperMovingLeftLoopFirst, 7),
            Show(EnemyFrameId.TripperMovingLeftLoopSecond, 8),
            Show(EnemyFrameId.TripperMovingLeftLoopFirst, 7),
            Show(EnemyFrameId.TripperMovingLeftLoopThird, 8),
        ],
        [P.TripperMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperMovingRightLoop] =
        [
            Show(EnemyFrameId.TripperMovingRightLoopFirst, 7),
            Show(EnemyFrameId.TripperMovingRightLoopSecond, 8),
            Show(EnemyFrameId.TripperMovingRightLoopFirst, 7),
            Show(EnemyFrameId.TripperMovingRightLoopThird, 8),
        ],
        [P.TripperStillMovingLeft] =
        [
            Cue(new(K.MoveLeft)),
        ],
        [P.TripperStillMovingLeftLoop] =
        [
            Show(EnemyFrameId.TripperStillMovingLeftLoopFirst, 7),
            Show(EnemyFrameId.TripperStillMovingLeftLoopSecond, 8),
            Show(EnemyFrameId.TripperStillMovingLeftLoopFirst, 7),
            Show(EnemyFrameId.TripperStillMovingLeftLoopThird, 8),
        ],
        [P.TripperStillMovingRight] =
        [
            Cue(new(K.MoveRight)),
        ],
        [P.TripperStillMovingRightLoop] =
        [
            Show(EnemyFrameId.TripperStillMovingRightLoopFirst, 7),
            Show(EnemyFrameId.TripperStillMovingRightLoopSecond, 8),
            Show(EnemyFrameId.TripperStillMovingRightLoopFirst, 7),
            Show(EnemyFrameId.TripperStillMovingRightLoopThird, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PlatformSegment, Animation<PlatformSegment>> StartingAt { get; } = new Dictionary<PlatformSegment, Animation<PlatformSegment>>
    {
        [P.KamerMovingLeft] = new([new(P.KamerMovingLeft, 0, 1, 1), new(P.KamerMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerMovingLeftLoop] = new([new(P.KamerMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KamerMovingRight] = new([new(P.KamerMovingRight, 0, 1, 1), new(P.KamerMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerMovingRightLoop] = new([new(P.KamerMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KamerStillLeft] = new([new(P.KamerStillLeft, 0, 1, 1), new(P.KamerStillLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerStillLeftLoop] = new([new(P.KamerStillLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KamerStillRight] = new([new(P.KamerStillRight, 0, 1, 1), new(P.KamerStillRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.KamerStillRightLoop] = new([new(P.KamerStillRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperMovingLeft] = new([new(P.TripperMovingLeft, 0, 1, 1), new(P.TripperMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingLeftLoop] = new([new(P.TripperMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperMovingRight] = new([new(P.TripperMovingRight, 0, 1, 1), new(P.TripperMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperMovingRightLoop] = new([new(P.TripperMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperStillMovingLeft] = new([new(P.TripperStillMovingLeft, 0, 1, 1), new(P.TripperStillMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingLeftLoop] = new([new(P.TripperStillMovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TripperStillMovingRight] = new([new(P.TripperStillMovingRight, 0, 1, 1), new(P.TripperStillMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.TripperStillMovingRightLoop] = new([new(P.TripperStillMovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PlatformSegment> StartingWith(PlatformSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, PlatformCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, PlatformCue> Cue(PlatformCue cue) => new AnimationBeat<EnemyFrameId, PlatformCue>.Event(cue);
}
