using P = SuperMetroid.Core.Enemies.Bull.BullSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class BullAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BullSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BullSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Normal] =
        [
            Show(EnemyFrameId.BullNormalFirst, 10),
            Show(EnemyFrameId.BullNormalSecond, 10),
            Show(EnemyFrameId.BullNormalFirst, 10),
            Show(EnemyFrameId.BullNormalThird, 10),
        ],
        [P.Shot] =
        [
        ],
        [P.ShotLoop] =
        [
            Show(EnemyFrameId.BullNormalFirst, 3),
            Show(EnemyFrameId.BullNormalSecond, 3),
            Show(EnemyFrameId.BullNormalFirst, 3),
            Show(EnemyFrameId.BullNormalThird, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BullSegment, Animation<BullSegment>> StartingAt { get; } = new Dictionary<BullSegment, Animation<BullSegment>>
    {
        [P.Normal] = new([new(P.Normal, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Shot] = new([new(P.ShotLoop, 0, 4, 5), new(P.Normal, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BullSegment> StartingWith(BullSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
