using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileSegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The ceiling spore spawner's animations (#641, #2011, #2554): its closed pose, held (#2235), and its release, which spawns a spore after its second pose.</summary>
internal static class SporeSpawnSpawnerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnSpawnerCue> Segments { get; } = new(
        new Dictionary<SporeSpawnProjectileSegment, AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue>[]>
    {
        [P.SpawnerClosed] =
        [
            Hold(EnemyProjectileFrameId.SporeSpawnSpawnerClosed),
        ],
        [P.SpawnerRelease] =
        [
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerClosed, 1),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerOpening, 6),
            Cue(SporeSpawnSpawnerCue.SpawnSpore),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerOpen, 16),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerOpening, 6),
            Show(EnemyProjectileFrameId.SporeSpawnSpawnerClosed, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt { get; } = new Dictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>>
    {
        [P.SpawnerClosed] = new([Segments.Whole(P.SpawnerClosed)], AnimationEnd.Holds),
        [P.SpawnerRelease] = new([Segments.Whole(P.SpawnerRelease)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnProjectileSegment> StartingWith(SporeSpawnProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue> Cue(SporeSpawnSpawnerCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, SporeSpawnSpawnerCue>.Event(cue);
}
