using P = SuperMetroid.Core.Enemies.Corpses.DeadTorizoSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's stationary corpse pose as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class DeadTorizoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DeadTorizoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DeadTorizoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Stationary] =
        [
            Show(EnemyFrameId.DeadTorizoStationary, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DeadTorizoSegment, Animation<DeadTorizoSegment>> StartingAt { get; } = new Dictionary<DeadTorizoSegment, Animation<DeadTorizoSegment>>
    {
        [P.Stationary] = new([new(P.Stationary, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTorizoSegment> StartingWith(DeadTorizoSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
