using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The installed frames of <see cref="RidleyFireballAnimations"/> in installed order, which only verification reads.</summary>
internal static class RidleyFireballAnimationsFramesAccess
{
    /// <summary>The members, read as members of <see cref="RidleyFireballAnimations"/>.</summary>
    extension(RidleyFireballAnimations)
    {
        /// <summary>Every fireball and afterburn frame in installed order: launch, flight, then each afterburn run's five poses.</summary>
        internal static IReadOnlyList<EnemyProjectileFrameId> ProjectileFrames =>
        [
            EnemyProjectileFrameId.RidleyFireballTurned0, EnemyProjectileFrameId.RidleyFireballTurned0Twin,
            EnemyProjectileFrameId.RidleyFireballTurned0TwinB, EnemyProjectileFrameId.RidleyFireballTurned90,
            EnemyProjectileFrameId.RidleyFireballTurned180, EnemyProjectileFrameId.RidleyFireballTurned270,
            EnemyProjectileFrameId.RidleyFireballSpark, EnemyProjectileFrameId.RidleyFireballFireCloud,
            EnemyProjectileFrameId.RidleyFireballFireCloudSpreading, EnemyProjectileFrameId.RidleyFireballFireCloudBreaking,
            EnemyProjectileFrameId.RidleyFireballFireCloudEmbers,
            EnemyProjectileFrameId.RidleyFireballSparkTwin, EnemyProjectileFrameId.RidleyFireballFireCloudTwin,
            EnemyProjectileFrameId.RidleyFireballFireCloudSpreadingTwin, EnemyProjectileFrameId.RidleyFireballFireCloudBreakingTwin,
            EnemyProjectileFrameId.RidleyFireballFireCloudEmbersTwin,
            EnemyProjectileFrameId.RidleyFireballSparkTwinB, EnemyProjectileFrameId.RidleyFireballFireCloudTwinB,
            EnemyProjectileFrameId.RidleyFireballFireCloudSpreadingTwinB, EnemyProjectileFrameId.RidleyFireballFireCloudBreakingTwinB,
            EnemyProjectileFrameId.RidleyFireballFireCloudEmbersTwinB,
            EnemyProjectileFrameId.RidleyFireballSparkTwinC, EnemyProjectileFrameId.RidleyFireballFireCloudTwinC,
            EnemyProjectileFrameId.RidleyFireballFireCloudSpreadingTwinC, EnemyProjectileFrameId.RidleyFireballFireCloudBreakingTwinC,
            EnemyProjectileFrameId.RidleyFireballFireCloudEmbersTwinC,
        ];
    }
}
