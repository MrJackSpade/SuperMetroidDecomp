using P = SuperMetroid.Core.Enemies.Atomic.AtomicProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations as typed programs (#641). Each
/// frame shows a <see cref="AtomicFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class AtomicAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AtomicProgram, AtomicFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<AtomicProgram, AnimationBeat<AtomicFrame, NoAnimationCue>[]>
    {
        [P.UpRight] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
        ],
        [P.UpLeft] =
        [
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 8),
            Show(11, 8),
        ],
        [P.DownLeft] =
        [
            Show(5, 8),
            Show(4, 8),
            Show(3, 8),
            Show(2, 8),
            Show(1, 8),
            Show(0, 8),
        ],
        [P.DownRight] =
        [
            Show(11, 8),
            Show(10, 8),
            Show(9, 8),
            Show(8, 8),
            Show(7, 8),
            Show(6, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AtomicProgram, Animation<AtomicProgram>> StartingAt { get; } = new Dictionary<AtomicProgram, Animation<AtomicProgram>>
    {
        [P.UpRight] = new([new(P.UpRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLeft] = new([new(P.UpLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLeft] = new([new(P.DownLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownRight] = new([new(P.DownRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AtomicProgram> StartingWith(AtomicProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<AtomicFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<AtomicFrame, NoAnimationCue>.Frame(new(pose), duration);
}
