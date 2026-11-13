using P = SuperMetroid.Core.Enemies.Polyp.PolypSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class PolypAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PolypSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PolypSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Stationary] =
        [
            Show(EnemyFrameId.PolypStationary, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PolypSegment, Animation<PolypSegment>> StartingAt { get; } = new Dictionary<PolypSegment, Animation<PolypSegment>>
    {
        [P.Stationary] = new([new(P.Stationary, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PolypSegment> StartingWith(PolypSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
