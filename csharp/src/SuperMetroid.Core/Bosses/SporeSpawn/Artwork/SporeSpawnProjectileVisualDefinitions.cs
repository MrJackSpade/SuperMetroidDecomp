using SuperMetroid.Core.Enemies.Projectiles;

using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileProgram;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The presentation boundary of Spore Spawn's projectiles (#641): the bank-$86 list entries the projectile
/// definitions name, and the projectile frame each typed frame shows.
/// </summary>
internal static class SporeSpawnProjectileVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<SporeSpawnProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<SporeSpawnProjectileFrame, EnemyProjectileFrameId>
        {
            [new(SporeSpawnProjectileProgram.SpawnerClosed, 0)] = EnemyProjectileFrameId.SporeSpawnFrame00,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 0)] = EnemyProjectileFrameId.SporeSpawnFrame01,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 1)] = EnemyProjectileFrameId.SporeSpawnFrame02,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 2)] = EnemyProjectileFrameId.SporeSpawnFrame03,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 3)] = EnemyProjectileFrameId.SporeSpawnFrame04,
            [new(SporeSpawnProjectileProgram.SpawnerRelease, 4)] = EnemyProjectileFrameId.SporeSpawnFrame05,
            [new(SporeSpawnProjectileProgram.Spore, 0)] = EnemyProjectileFrameId.SporeSpawnFrame06,
            [new(SporeSpawnProjectileProgram.Spore, 1)] = EnemyProjectileFrameId.SporeSpawnFrame07,
            [new(SporeSpawnProjectileProgram.Spore, 2)] = EnemyProjectileFrameId.SporeSpawnFrame08,
            [new(SporeSpawnProjectileProgram.Stalk, 0)] = EnemyProjectileFrameId.SporeSpawnFrame09,
            [new(SporeSpawnProjectileProgram.SporeShot, 0)] = EnemyProjectileFrameId.SporeSpawnFrame10,
            [new(SporeSpawnProjectileProgram.SporeShot, 1)] = EnemyProjectileFrameId.SporeSpawnFrame11,
            [new(SporeSpawnProjectileProgram.SporeShot, 2)] = EnemyProjectileFrameId.SporeSpawnFrame12,
            [new(SporeSpawnProjectileProgram.SporeShot, 3)] = EnemyProjectileFrameId.SporeSpawnFrame13,
            [new(SporeSpawnProjectileProgram.SporeShot, 4)] = EnemyProjectileFrameId.SporeSpawnFrame14,
            [new(SporeSpawnProjectileProgram.SporeShot, 5)] = EnemyProjectileFrameId.SporeSpawnFrame15,
            [new(SporeSpawnProjectileProgram.SporeShot, 6)] = EnemyProjectileFrameId.SporeSpawnFrame16,
        };
}
