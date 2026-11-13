using P = SuperMetroid.Core.Enemies.Boulder.BoulderSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class BoulderAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoulderSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BoulderSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyFrameId.BoulderLeftFirst, 8),
            Show(EnemyFrameId.BoulderLeftSecond, 8),
            Show(EnemyFrameId.BoulderLeftThird, 8),
            Show(EnemyFrameId.BoulderLeftFourth, 8),
            Show(EnemyFrameId.BoulderLeftFifth, 8),
            Show(EnemyFrameId.BoulderLeftSixth, 8),
            Show(EnemyFrameId.BoulderLeftSeventh, 8),
            Show(EnemyFrameId.BoulderLeftEighth, 8),
        ],
        [P.Right] =
        [
            Show(EnemyFrameId.BoulderLeftFirst, 8),
            Show(EnemyFrameId.BoulderLeftEighth, 8),
            Show(EnemyFrameId.BoulderLeftSeventh, 8),
            Show(EnemyFrameId.BoulderLeftSixth, 8),
            Show(EnemyFrameId.BoulderLeftFifth, 8),
            Show(EnemyFrameId.BoulderLeftFourth, 8),
            Show(EnemyFrameId.BoulderLeftThird, 8),
            Show(EnemyFrameId.BoulderLeftSecond, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BoulderSegment, Animation<BoulderSegment>> StartingAt { get; } = new Dictionary<BoulderSegment, Animation<BoulderSegment>>
    {
        [P.Left] = new([new(P.Left, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([new(P.Right, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoulderSegment> StartingWith(BoulderSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
