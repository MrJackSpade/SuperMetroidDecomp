using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters.Artwork;

/// <summary>
/// The presentation boundary of the Ki-Hunter's acid spit programs (#641): the projectile frame each
/// <see cref="KiHunterAcidSpitFrame"/> pose shows.
/// </summary>
internal static class KiHunterAcidSpitFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 19;

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<KiHunterAcidSpitFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<KiHunterAcidSpitFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame00,
            [new(1)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame01,
            [new(2)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame02,
            [new(3)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame03,
            [new(4)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame04,
            [new(5)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame05,
            [new(6)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame06,
            [new(7)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame07,
            [new(8)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame08,
            [new(9)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame09,
            [new(10)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame10,
            [new(11)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame11,
            [new(12)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame12,
            [new(13)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame13,
            [new(14)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame14,
            [new(15)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame15,
            [new(16)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame16,
            [new(17)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame17,
            [new(18)] = EnemyProjectileFrameId.KiHunterAcidSpitFrame18,
        };
}
