using P = SuperMetroid.Core.Enemies.Atomic.AtomicProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class AtomicAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<AtomicProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<AtomicProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.UpRight] =
        [
            Show(EnemyFrameId.AtomicUpRight0, 8),
            Show(EnemyFrameId.AtomicUpRight1, 8),
            Show(EnemyFrameId.AtomicUpRight2, 8),
            Show(EnemyFrameId.AtomicUpRight3, 8),
            Show(EnemyFrameId.AtomicUpRight4, 8),
            Show(EnemyFrameId.AtomicUpRight5, 8),
        ],
        [P.UpLeft] =
        [
            Show(EnemyFrameId.AtomicUpLeft0, 8),
            Show(EnemyFrameId.AtomicUpLeft1, 8),
            Show(EnemyFrameId.AtomicUpLeft2, 8),
            Show(EnemyFrameId.AtomicUpLeft3, 8),
            Show(EnemyFrameId.AtomicUpLeft4, 8),
            Show(EnemyFrameId.AtomicUpLeft5, 8),
        ],
        [P.DownLeft] =
        [
            Show(EnemyFrameId.AtomicUpRight5, 8),
            Show(EnemyFrameId.AtomicUpRight4, 8),
            Show(EnemyFrameId.AtomicUpRight3, 8),
            Show(EnemyFrameId.AtomicUpRight2, 8),
            Show(EnemyFrameId.AtomicUpRight1, 8),
            Show(EnemyFrameId.AtomicUpRight0, 8),
        ],
        [P.DownRight] =
        [
            Show(EnemyFrameId.AtomicUpLeft5, 8),
            Show(EnemyFrameId.AtomicUpLeft4, 8),
            Show(EnemyFrameId.AtomicUpLeft3, 8),
            Show(EnemyFrameId.AtomicUpLeft2, 8),
            Show(EnemyFrameId.AtomicUpLeft1, 8),
            Show(EnemyFrameId.AtomicUpLeft0, 8),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
