using P = SuperMetroid.Core.Enemies.Metroids.MochtroidSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Mochtroid's free-flight and attached pulse loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class MochtroidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MochtroidSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MochtroidSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.FreeFlight] =
        [
            Show(EnemyFrameId.MochtroidFloatingWide, 14),
            Show(EnemyFrameId.MochtroidFloatingSqueezing, 14),
            Show(EnemyFrameId.MochtroidFloatingSqueezed, 14),
            Show(EnemyFrameId.MochtroidFloatingSqueezing, 14),
        ],
        [P.Attached] =
        [
            Show(EnemyFrameId.MochtroidLatchedWide, 5),
            Show(EnemyFrameId.MochtroidLatchedSqueezing, 5),
            Show(EnemyFrameId.MochtroidLatchedSqueezed, 5),
            Show(EnemyFrameId.MochtroidLatchedSqueezing, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static IReadOnlyDictionary<MochtroidSegment, Animation<MochtroidSegment>> StartingAt { get; } = new Dictionary<MochtroidSegment, Animation<MochtroidSegment>>
    {
        [P.FreeFlight] = Animation<MochtroidSegment>.Looping([Segments.Whole(P.FreeFlight)]),
        [P.Attached] = Animation<MochtroidSegment>.Looping([Segments.Whole(P.Attached)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MochtroidSegment> StartingWith(MochtroidSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
