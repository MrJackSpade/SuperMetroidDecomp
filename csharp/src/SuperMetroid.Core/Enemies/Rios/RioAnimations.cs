using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.RioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.RioProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="RioCue"/> with its typed operands.
/// </summary>
internal static class RioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RioProgram, EnemyFrameId, RioCue> Segments { get; } = new(
        new Dictionary<RioProgram, AnimationBeat<EnemyFrameId, RioCue>[]>
    {
        [P.Idle] =
        [
            Show(EnemyFrameId.RioPose0, 4),
            Show(EnemyFrameId.RioPose1, 4),
        ],
        [P.PostSwoopIdle] =
        [
            Show(EnemyFrameId.RioPose0, 4),
            Show(EnemyFrameId.RioPose1, 4),
            Show(EnemyFrameId.RioPose0, 4),
            Show(EnemyFrameId.RioPose1, 4),
            Show(EnemyFrameId.RioPose0, 4),
            Show(EnemyFrameId.RioPose1, 4),
            Show(EnemyFrameId.RioPose2, 4),
            Show(EnemyFrameId.RioPose3, 4),
            Show(EnemyFrameId.RioPose2, 4),
            Show(EnemyFrameId.RioPose3, 4),
        ],
        [P.SwoopingPart1] =
        [
            Show(EnemyFrameId.RioPose2, 3),
            Show(EnemyFrameId.RioPose3, 3),
            Show(EnemyFrameId.RioPose4, 3),
            Show(EnemyFrameId.RioPose5, 3),
            Show(EnemyFrameId.RioPose6, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.SwoopingPart2] =
        [
            Show(EnemyFrameId.RioPose7, 3),
            Show(EnemyFrameId.RioPose6, 3),
        ],
        [P.SwoopCooldown] =
        [
            Show(EnemyFrameId.RioPose6, 3),
            Show(EnemyFrameId.RioPose5, 3),
            Show(EnemyFrameId.RioPose4, 3),
            Show(EnemyFrameId.RioPose3, 3),
            Show(EnemyFrameId.RioPose2, 3),
            Cue(new(K.AnimationFinished)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RioProgram, Animation<RioProgram>> StartingAt { get; } = new Dictionary<RioProgram, Animation<RioProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 2, 1), new(P.PostSwoopIdle, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PostSwoopIdle] = new([new(P.PostSwoopIdle, 0, 10, 1), new(P.Idle, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopingPart1] = new([new(P.SwoopingPart1, 0, 6, 1)], AnimationEnd.Holds),
        [P.SwoopingPart2] = new([new(P.SwoopingPart2, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopCooldown] = new([new(P.SwoopCooldown, 0, 6, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RioProgram> StartingWith(RioProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Cue(RioCue cue) => new AnimationBeat<EnemyFrameId, RioCue>.Event(cue);
}
