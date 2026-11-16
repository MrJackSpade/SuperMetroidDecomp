using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The installed frames of <see cref="RidleyFireballAnimations"/> in installed order, which only verification reads.</summary>
internal static class RidleyFireballAnimationsFramesAccess
{
    /// <summary>The members, read as members of <see cref="RidleyFireballAnimations"/>.</summary>
    extension(RidleyFireballAnimations)
    {
        /// <summary>
        /// Every fireball and afterburn frame in installed order: the swirl of flame and its three turns, then the five poses
        /// every afterburn run shows (#2234).
        /// </summary>
        internal static IReadOnlyList<EnemyProjectileFrameId> ProjectileFrames =>
        [
            EnemyProjectileFrameId.RidleyFireballTurned0, EnemyProjectileFrameId.RidleyFireballTurned90,
            EnemyProjectileFrameId.RidleyFireballTurned180, EnemyProjectileFrameId.RidleyFireballTurned270,
            EnemyProjectileFrameId.RidleyFireballSpark, EnemyProjectileFrameId.RidleyFireballFireCloud,
            EnemyProjectileFrameId.RidleyFireballFireCloudSpreading, EnemyProjectileFrameId.RidleyFireballFireCloudBreaking,
            EnemyProjectileFrameId.RidleyFireballFireCloudEmbers,
        ];
    }
}
