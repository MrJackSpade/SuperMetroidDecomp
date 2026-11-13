using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Owtch.OwtchCueKind;
using P = SuperMetroid.Core.Enemies.Owtch.OwtchSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="OwtchCue"/> with its typed operands.
/// </summary>
internal static class OwtchAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<OwtchSegment, EnemyFrameId, OwtchCue> Segments { get; } = new(
        new Dictionary<OwtchSegment, AnimationBeat<EnemyFrameId, OwtchCue>[]>
    {
        [P.MovingLeft] =
        [
            Cue(new(K.SetMovingLeft)),
        ],
        [P.MovingLeftLoop] =
        [
            Show(EnemyFrameId.OwtchMovingLeftLoopFirst, 8),
            Show(EnemyFrameId.OwtchMovingLeftLoopSecond, 8),
            Show(EnemyFrameId.OwtchMovingLeftLoopThird, 8),
        ],
        [P.MovingRight] =
        [
            Cue(new(K.SetMovingRight)),
        ],
        [P.MovingRightLoop] =
        [
            Show(EnemyFrameId.OwtchMovingLeftLoopThird, 8),
            Show(EnemyFrameId.OwtchMovingLeftLoopSecond, 8),
            Show(EnemyFrameId.OwtchMovingLeftLoopFirst, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<OwtchSegment, Animation<OwtchSegment>> StartingAt { get; } = new Dictionary<OwtchSegment, Animation<OwtchSegment>>
    {
        [P.MovingLeft] = new([new(P.MovingLeft, 0, 1, 1), new(P.MovingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MovingLeftLoop] = new([new(P.MovingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MovingRight] = new([new(P.MovingRight, 0, 1, 1), new(P.MovingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MovingRightLoop] = new([new(P.MovingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<OwtchSegment> StartingWith(OwtchSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, OwtchCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, OwtchCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, OwtchCue> Cue(OwtchCue cue) => new AnimationBeat<EnemyFrameId, OwtchCue>.Event(cue);
}
