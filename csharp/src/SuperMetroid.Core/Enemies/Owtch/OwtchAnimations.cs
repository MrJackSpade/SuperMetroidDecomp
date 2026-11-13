using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Owtch.OwtchCueKind;
using P = SuperMetroid.Core.Enemies.Owtch.OwtchProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="OwtchCue"/> with its typed operands.
/// </summary>
internal static class OwtchAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<OwtchProgram, EnemyFrameId, OwtchCue> Segments { get; } = new(
        new Dictionary<OwtchProgram, AnimationBeat<EnemyFrameId, OwtchCue>[]>
    {
        [P.MovingLeft] =
        [
            Cue(new(K.SetMovingLeft)),
        ],
        [P.MovingLeftLoop] =
        [
            Show(EnemyFrameId.OwtchLeft0, 8),
            Show(EnemyFrameId.OwtchLeft1, 8),
            Show(EnemyFrameId.OwtchLeft2, 8),
        ],
        [P.MovingRight] =
        [
            Cue(new(K.SetMovingRight)),
        ],
        [P.MovingRightLoop] =
        [
            Show(EnemyFrameId.OwtchLeft2, 8),
            Show(EnemyFrameId.OwtchLeft1, 8),
            Show(EnemyFrameId.OwtchLeft0, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<OwtchProgram, Animation<OwtchProgram>> StartingAt { get; } = new Dictionary<OwtchProgram, Animation<OwtchProgram>>
    {
        [P.MovingLeft] = new([new(P.MovingLeft, 0, 1, 1), new(P.MovingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MovingLeftLoop] = new([new(P.MovingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MovingRight] = new([new(P.MovingRight, 0, 1, 1), new(P.MovingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MovingRightLoop] = new([new(P.MovingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<OwtchProgram> StartingWith(OwtchProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, OwtchCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, OwtchCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, OwtchCue> Cue(OwtchCue cue) => new AnimationBeat<EnemyFrameId, OwtchCue>.Event(cue);
}
