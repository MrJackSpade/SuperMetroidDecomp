using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's projectiles (#641): the projectile frame each
/// typed frame shows.
/// </summary>
internal static class SporeSpawnProjectileVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<SporeSpawnProjectileFrame, EnemyProjectileFrameId>
        {
            [new(SporeSpawnProjectileSegment.SpawnerClosed, 0)] = EnemyProjectileFrameId.SporeSpawnSpawnerClosed,
            [new(SporeSpawnProjectileSegment.SpawnerRelease, 0)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseFirst,
            [new(SporeSpawnProjectileSegment.SpawnerRelease, 1)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseSecond,
            [new(SporeSpawnProjectileSegment.SpawnerRelease, 2)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseThird,
            [new(SporeSpawnProjectileSegment.SpawnerRelease, 3)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseFourth,
            [new(SporeSpawnProjectileSegment.SpawnerRelease, 4)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseFifth,
            [new(SporeSpawnProjectileSegment.Spore, 0)] = EnemyProjectileFrameId.SporeSpawnSporeFirst,
            [new(SporeSpawnProjectileSegment.Spore, 1)] = EnemyProjectileFrameId.SporeSpawnSporeSecond,
            [new(SporeSpawnProjectileSegment.Spore, 2)] = EnemyProjectileFrameId.SporeSpawnSporeThird,
            [new(SporeSpawnProjectileSegment.Stalk, 0)] = EnemyProjectileFrameId.SporeSpawnStalk,
            [new(SporeSpawnProjectileSegment.SporeShot, 0)] = EnemyProjectileFrameId.SporeSpawnSporeShotFirst,
            [new(SporeSpawnProjectileSegment.SporeShot, 1)] = EnemyProjectileFrameId.SporeSpawnSporeShotSecond,
            [new(SporeSpawnProjectileSegment.SporeShot, 2)] = EnemyProjectileFrameId.SporeSpawnSporeShotThird,
            [new(SporeSpawnProjectileSegment.SporeShot, 3)] = EnemyProjectileFrameId.SporeSpawnSporeShotFourth,
            [new(SporeSpawnProjectileSegment.SporeShot, 4)] = EnemyProjectileFrameId.SporeSpawnSporeShotFifth,
            [new(SporeSpawnProjectileSegment.SporeShot, 5)] = EnemyProjectileFrameId.SporeSpawnSporeShotSixth,
            [new(SporeSpawnProjectileSegment.SporeShot, 6)] = EnemyProjectileFrameId.SporeSpawnSporeShotSeventh,
        };
}
