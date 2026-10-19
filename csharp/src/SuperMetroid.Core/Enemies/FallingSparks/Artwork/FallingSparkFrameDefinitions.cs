using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks.Artwork;

/// <summary>
/// The presentation boundary of the falling spark's programs (#641): the projectile frame each
/// <see cref="FallingSparkFrame"/> pose shows.
/// </summary>
internal static class FallingSparkFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<FallingSparkFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<FallingSparkFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.FallingSparkFrame00,
            [new(1)] = EnemyProjectileFrameId.FallingSparkFrame01,
            [new(2)] = EnemyProjectileFrameId.FallingSparkFrame02,
            [new(3)] = EnemyProjectileFrameId.FallingSparkFrame03,
            [new(4)] = EnemyProjectileFrameId.FallingSparkFrame04,
            [new(5)] = EnemyProjectileFrameId.FallingSparkFrame05,
            [new(6)] = EnemyProjectileFrameId.FallingSparkFrame06,
            [new(7)] = EnemyProjectileFrameId.FallingSparkFrame07,
            [new(8)] = EnemyProjectileFrameId.FallingSparkFrame08,
            [new(9)] = EnemyProjectileFrameId.FallingSparkFrame09,
            [new(10)] = EnemyProjectileFrameId.FallingSparkFrame10,
            [new(11)] = EnemyProjectileFrameId.FallingSparkFrame11,
            [new(12)] = EnemyProjectileFrameId.FallingSparkFrame12,
            [new(13)] = EnemyProjectileFrameId.FallingSparkFrame13,
        };
}
