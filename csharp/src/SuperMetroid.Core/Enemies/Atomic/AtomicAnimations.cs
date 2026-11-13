using P = SuperMetroid.Core.Enemies.Atomic.AtomicSegment;
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
    internal static AnimationSegments<AtomicSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<AtomicSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.UpRight] =
        [
            Show(EnemyFrameId.AtomicUpRightFirst, 8),
            Show(EnemyFrameId.AtomicUpRightSecond, 8),
            Show(EnemyFrameId.AtomicUpRightThird, 8),
            Show(EnemyFrameId.AtomicUpRightFourth, 8),
            Show(EnemyFrameId.AtomicUpRightFifth, 8),
            Show(EnemyFrameId.AtomicUpRightSixth, 8),
        ],
        [P.UpLeft] =
        [
            Show(EnemyFrameId.AtomicUpLeftFirst, 8),
            Show(EnemyFrameId.AtomicUpLeftSecond, 8),
            Show(EnemyFrameId.AtomicUpLeftThird, 8),
            Show(EnemyFrameId.AtomicUpLeftFourth, 8),
            Show(EnemyFrameId.AtomicUpLeftFifth, 8),
            Show(EnemyFrameId.AtomicUpLeftSixth, 8),
        ],
        [P.DownLeft] =
        [
            Show(EnemyFrameId.AtomicUpRightSixth, 8),
            Show(EnemyFrameId.AtomicUpRightFifth, 8),
            Show(EnemyFrameId.AtomicUpRightFourth, 8),
            Show(EnemyFrameId.AtomicUpRightThird, 8),
            Show(EnemyFrameId.AtomicUpRightSecond, 8),
            Show(EnemyFrameId.AtomicUpRightFirst, 8),
        ],
        [P.DownRight] =
        [
            Show(EnemyFrameId.AtomicUpLeftSixth, 8),
            Show(EnemyFrameId.AtomicUpLeftFifth, 8),
            Show(EnemyFrameId.AtomicUpLeftFourth, 8),
            Show(EnemyFrameId.AtomicUpLeftThird, 8),
            Show(EnemyFrameId.AtomicUpLeftSecond, 8),
            Show(EnemyFrameId.AtomicUpLeftFirst, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<AtomicSegment, Animation<AtomicSegment>> StartingAt { get; } = new Dictionary<AtomicSegment, Animation<AtomicSegment>>
    {
        [P.UpRight] = new([new(P.UpRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLeft] = new([new(P.UpLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLeft] = new([new(P.DownLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownRight] = new([new(P.DownRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AtomicSegment> StartingWith(AtomicSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
