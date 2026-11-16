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
            EnemyProjectileFrameId.CeresRidleyFireballTurned0, EnemyProjectileFrameId.CeresRidleyFireballTurned0Twin,
            EnemyProjectileFrameId.CeresRidleyFireballTurned0TwinB, EnemyProjectileFrameId.CeresRidleyFireballTurned90,
            EnemyProjectileFrameId.CeresRidleyFireballTurned180, EnemyProjectileFrameId.CeresRidleyFireballTurned270,
            EnemyProjectileFrameId.CeresRidleyFireballSpark, EnemyProjectileFrameId.CeresRidleyFireballFireCloud,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreading, EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreaking,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbers,
            EnemyProjectileFrameId.CeresRidleyFireballSparkTwin, EnemyProjectileFrameId.CeresRidleyFireballFireCloudTwin,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreadingTwin, EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreakingTwin,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbersTwin,
            EnemyProjectileFrameId.CeresRidleyFireballSparkTwinB, EnemyProjectileFrameId.CeresRidleyFireballFireCloudTwinB,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreadingTwinB, EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreakingTwinB,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbersTwinB,
            EnemyProjectileFrameId.CeresRidleyFireballSparkTwinC, EnemyProjectileFrameId.CeresRidleyFireballFireCloudTwinC,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudSpreadingTwinC, EnemyProjectileFrameId.CeresRidleyFireballFireCloudBreakingTwinC,
            EnemyProjectileFrameId.CeresRidleyFireballFireCloudEmbersTwinC,
        ];
    }
}
