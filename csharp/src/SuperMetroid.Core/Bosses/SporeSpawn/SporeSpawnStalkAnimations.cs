using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileSegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A stalk link's animation (#641, #2011, #2554): its pose, held; it publishes no cues.</summary>
internal static class SporeSpawnStalkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SporeSpawnProjectileSegment, AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>[]>
    {
        [P.Stalk] =
        [
            Hold(EnemyProjectileFrameId.SporeSpawnStalk),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt { get; } = new Dictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>>
    {
        [P.Stalk] = new([Segments.Whole(P.Stalk)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnProjectileSegment> StartingWith(SporeSpawnProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, NoAnimationCue> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.HeldFrame(frame);
}
