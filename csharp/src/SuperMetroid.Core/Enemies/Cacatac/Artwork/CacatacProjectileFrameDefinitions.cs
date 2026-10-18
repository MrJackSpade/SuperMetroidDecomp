using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac.Artwork;

/// <summary>
/// The presentation boundary of the Cacatac's spike programs (#641): the projectile frame each
/// <see cref="CacatacProjectileFrame"/> pose shows.
/// </summary>
internal static class CacatacProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 10;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<CacatacProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<CacatacProjectileFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.CacatacSpikeFrame00,
            [new(1)] = EnemyProjectileFrameId.CacatacSpikeFrame01,
            [new(2)] = EnemyProjectileFrameId.CacatacSpikeFrame02,
            [new(3)] = EnemyProjectileFrameId.CacatacSpikeFrame03,
            [new(4)] = EnemyProjectileFrameId.CacatacSpikeFrame04,
            [new(5)] = EnemyProjectileFrameId.CacatacSpikeFrame05,
            [new(6)] = EnemyProjectileFrameId.CacatacSpikeFrame06,
            [new(7)] = EnemyProjectileFrameId.CacatacSpikeFrame07,
            [new(8)] = EnemyProjectileFrameId.CacatacSpikeFrame08,
            [new(9)] = EnemyProjectileFrameId.CacatacSpikeFrame09,
        };
}
