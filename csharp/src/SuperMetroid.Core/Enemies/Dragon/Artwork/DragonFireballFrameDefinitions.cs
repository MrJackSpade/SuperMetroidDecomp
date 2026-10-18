using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon.Artwork;

/// <summary>
/// The presentation boundary of Dragon's fireball programs (#641): the projectile frame each
/// <see cref="DragonFireballFrame"/> pose shows.
/// </summary>
internal static class DragonFireballFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<DragonFireballFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<DragonFireballFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.DragonFireballFrame00,
            [new(1)] = EnemyProjectileFrameId.DragonFireballFrame01,
            [new(2)] = EnemyProjectileFrameId.DragonFireballFrame02,
            [new(3)] = EnemyProjectileFrameId.DragonFireballFrame03,
            [new(4)] = EnemyProjectileFrameId.DragonFireballFrame04,
            [new(5)] = EnemyProjectileFrameId.DragonFireballFrame05,
            [new(6)] = EnemyProjectileFrameId.DragonFireballFrame06,
            [new(7)] = EnemyProjectileFrameId.DragonFireballFrame07,
        };
}
