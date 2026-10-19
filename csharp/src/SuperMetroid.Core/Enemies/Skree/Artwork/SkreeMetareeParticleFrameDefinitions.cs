using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree.Artwork;

/// <summary>
/// The presentation boundary of the Skree's and Metaree's death particles programs (#641): the projectile frame each
/// <see cref="SkreeMetareeParticleFrame"/> pose shows.
/// </summary>
internal static class SkreeMetareeParticleFrameDefinitions
{
    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeParticleFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<SkreeMetareeParticleFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.SkreeDebris,
            [new(1)] = EnemyProjectileFrameId.MetareeDebris,
        };
}
