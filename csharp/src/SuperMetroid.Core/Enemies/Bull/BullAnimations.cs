using P = SuperMetroid.Core.Enemies.Bull.BullSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's idle and immune-shot animations (#641, #2477). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class BullAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BullSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BullSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Idle] = Sway(10),
        [P.Shot] = Sway(3),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BullSegment, Animation<BullSegment>> StartingAt { get; } = new Dictionary<BullSegment, Animation<BullSegment>>
    {
        [P.Idle] = Animation<BullSegment>.Looping([Segments.Whole(P.Idle)]),
    };

    /// <summary>How many quick cycles of its poses an immune shot sets off before its idle sway resumes.</summary>
    private const int ShotCycles = 5;

    /// <summary>The reaction to an immune shot (#2061): five quick cycles of its poses, then its idle sway for good.</summary>
    internal static Animation<BullSegment> Shot { get; } = Animation<BullSegment>.LeadingInto([Segments.Repeated(P.Shot, ShotCycles)], [Segments.Whole(P.Idle)]);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BullSegment> StartingWith(BullSegment segment) => StartingAt[segment];

    /// <summary>
    /// One sway of its tentacles (#2477), each pose shown for <paramref name="updates"/> updates: upright, tilted, upright and
    /// tilted further. Its idle sway and an immune shot's quick cycles are the same sway at two paces.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Sway(int updates) =>
    [
        Show(EnemyFrameId.BullTentaclesUpright, updates),
        Show(EnemyFrameId.BullTentaclesTilted, updates),
        Show(EnemyFrameId.BullTentaclesUpright, updates),
        Show(EnemyFrameId.BullTentaclesTiltedFurther, updates),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
