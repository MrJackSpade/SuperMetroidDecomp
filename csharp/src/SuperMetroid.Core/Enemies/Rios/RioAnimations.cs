using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.RioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.RioProgram;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations as typed programs (#641). Each
/// frame shows a <see cref="RioFrame"/> pose and each callback becomes a <see cref="RioCue"/> with its typed operands.
/// </summary>
internal static class RioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RioProgram, RioFrame, RioCue> Segments { get; } = new(
        new Dictionary<RioProgram, AnimationBeat<RioFrame, RioCue>[]>
    {
        [P.Idle] =
        [
            Show(0, 4),
            Show(1, 4),
        ],
        [P.PostSwoopIdle] =
        [
            Show(0, 4),
            Show(1, 4),
            Show(0, 4),
            Show(1, 4),
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
            Show(3, 4),
            Show(2, 4),
            Show(3, 4),
        ],
        [P.SwoopingPart1] =
        [
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.SwoopingPart2] =
        [
            Show(7, 3),
            Show(6, 3),
        ],
        [P.SwoopCooldown] =
        [
            Show(6, 3),
            Show(5, 3),
            Show(4, 3),
            Show(3, 3),
            Show(2, 3),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RioFrame, RioCue> Show(int pose, int duration) => new AnimationBeat<RioFrame, RioCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<RioFrame, RioCue> Cue(RioCue cue) => new AnimationBeat<RioFrame, RioCue>.Event(cue);
}
