using P = SuperMetroid.Core.Enemies.Metroids.MochtroidSegment;
using SuperMetroid.Core.Programs;
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
            Show(EnemyFrameId.MochtroidFreeFlightFirst, 14),
            Show(EnemyFrameId.MochtroidFreeFlightSecond, 14),
            Show(EnemyFrameId.MochtroidFreeFlightThird, 14),
            Show(EnemyFrameId.MochtroidFreeFlightSecond, 14),
        ],
        [P.Attached] =
        [
            Show(EnemyFrameId.MochtroidAttachedFirst, 5),
            Show(EnemyFrameId.MochtroidAttachedSecond, 5),
            Show(EnemyFrameId.MochtroidAttachedThird, 5),
            Show(EnemyFrameId.MochtroidAttachedSecond, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MochtroidSegment, Animation<MochtroidSegment>> StartingAt { get; } = new Dictionary<MochtroidSegment, Animation<MochtroidSegment>>
    {
        [P.FreeFlight] = new([new(P.FreeFlight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Attached] = new([new(P.Attached, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MochtroidSegment> StartingWith(MochtroidSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
