using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp.Artwork;

/// <summary>
/// The presentation boundary of the Powamp spike's programs (#641): the projectile frame each
/// <see cref="PowampSpikeFrame"/> pose shows.
/// </summary>
internal static class PowampSpikeFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<PowampSpikeFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<PowampSpikeFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.PowampSpikeFrame00,
            [new(1)] = EnemyProjectileFrameId.PowampSpikeFrame01,
            [new(2)] = EnemyProjectileFrameId.PowampSpikeFrame02,
        };
}
