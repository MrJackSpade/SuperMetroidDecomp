using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The presentation boundary of the Wrecked Ship Chozo's and Tourian statue's dust programs (#641): the projectile frame each
/// <see cref="ChozoTourianDustFrame"/> pose shows.
/// </summary>
internal static class ChozoTourianDustFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 14;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<ChozoTourianDustFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<ChozoTourianDustFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.ChozoTourianDustFrame00,
            [new(1)] = EnemyProjectileFrameId.ChozoTourianDustFrame01,
            [new(2)] = EnemyProjectileFrameId.ChozoTourianDustFrame02,
            [new(3)] = EnemyProjectileFrameId.ChozoTourianDustFrame03,
            [new(4)] = EnemyProjectileFrameId.ChozoTourianDustFrame04,
            [new(5)] = EnemyProjectileFrameId.ChozoTourianDustFrame05,
            [new(6)] = EnemyProjectileFrameId.ChozoTourianDustFrame06,
            [new(7)] = EnemyProjectileFrameId.ChozoTourianDustFrame07,
            [new(8)] = EnemyProjectileFrameId.ChozoTourianDustFrame08,
            [new(9)] = EnemyProjectileFrameId.ChozoTourianDustFrame09,
            [new(10)] = EnemyProjectileFrameId.ChozoTourianDustFrame10,
            [new(11)] = EnemyProjectileFrameId.ChozoTourianDustFrame11,
            [new(12)] = EnemyProjectileFrameId.ChozoTourianDustFrame12,
            [new(13)] = EnemyProjectileFrameId.ChozoTourianDustFrame13,
        };
}
