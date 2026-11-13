using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's projectiles (#641): the list entries the projectile
/// definitions name, and the projectile frame each typed frame shows.
/// </summary>
internal static class SporeSpawnProjectileVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<SporeSpawnProjectileFrame, EnemyProjectileFrameId>
        {
            [new(SporeSpawnProjectileProgram.SpawnerClosed, 0)] = EnemyProjectileFrameId.SporeSpawnSpawnerClosed,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 0)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseFirst,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 1)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseSecond,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 2)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseThird,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 3)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseFourth,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 4)] = EnemyProjectileFrameId.SporeSpawnSpawnerReleaseFifth,
            [new(SporeSpawnProjectileProgram.Spore, 0)] = EnemyProjectileFrameId.SporeSpawnSporeFirst,
            [new(SporeSpawnProjectileProgram.Spore, 1)] = EnemyProjectileFrameId.SporeSpawnSporeSecond,
            [new(SporeSpawnProjectileProgram.Spore, 2)] = EnemyProjectileFrameId.SporeSpawnSporeThird,
            [new(SporeSpawnProjectileProgram.Stalk, 0)] = EnemyProjectileFrameId.SporeSpawnStalk,
            [new(SporeSpawnProjectileProgram.SporeShot, 0)] = EnemyProjectileFrameId.SporeSpawnSporeShotFirst,
            [new(SporeSpawnProjectileProgram.SporeShot, 1)] = EnemyProjectileFrameId.SporeSpawnSporeShotSecond,
            [new(SporeSpawnProjectileProgram.SporeShot, 2)] = EnemyProjectileFrameId.SporeSpawnSporeShotThird,
            [new(SporeSpawnProjectileProgram.SporeShot, 3)] = EnemyProjectileFrameId.SporeSpawnSporeShotFourth,
            [new(SporeSpawnProjectileProgram.SporeShot, 4)] = EnemyProjectileFrameId.SporeSpawnSporeShotFifth,
            [new(SporeSpawnProjectileProgram.SporeShot, 5)] = EnemyProjectileFrameId.SporeSpawnSporeShotSixth,
            [new(SporeSpawnProjectileProgram.SporeShot, 6)] = EnemyProjectileFrameId.SporeSpawnSporeShotSeventh,
        };
}
