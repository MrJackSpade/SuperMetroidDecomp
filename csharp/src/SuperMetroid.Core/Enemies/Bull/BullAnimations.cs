using P = SuperMetroid.Core.Enemies.Bull.BullSegment;
using SuperMetroid.Core.Animation;
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
            Show(EnemyFrameId.BullTentaclesUpright, 10),
            Show(EnemyFrameId.BullTentaclesTilted, 10),
            Show(EnemyFrameId.BullTentaclesUpright, 10),
            Show(EnemyFrameId.BullTentaclesTiltedFurther, 10),
        ],
        [P.Shot] =
        [
            Show(EnemyFrameId.BullTentaclesUpright, 3),
            Show(EnemyFrameId.BullTentaclesTilted, 3),
            Show(EnemyFrameId.BullTentaclesUpright, 3),
            Show(EnemyFrameId.BullTentaclesTiltedFurther, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BullSegment, Animation<BullSegment>> StartingAt { get; } = new Dictionary<BullSegment, Animation<BullSegment>>
    {
        [P.Normal] = new([Whole(P.Normal)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>How many quick cycles of its poses an immune shot sets off before its normal cycle resumes.</summary>
    private const int ShotCycles = 5;

    /// <summary>The reaction to an immune shot (#2061): five quick cycles of its poses, then its normal cycle for good.</summary>
    internal static Animation<BullSegment> Shot { get; } = new([Repeated(P.Shot, ShotCycles), Whole(P.Normal)], AnimationEnd.LoopsFrom(1));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BullSegment> StartingWith(BullSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<BullSegment> Whole(BullSegment segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<BullSegment> Repeated(BullSegment segment, int plays) => new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
