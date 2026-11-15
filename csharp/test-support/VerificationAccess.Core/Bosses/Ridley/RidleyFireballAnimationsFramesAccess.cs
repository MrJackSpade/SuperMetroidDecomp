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
            EnemyProjectileFrameId.CeresRidleyLaunchFirst, EnemyProjectileFrameId.CeresRidleyLaunchSecond,
            EnemyProjectileFrameId.CeresRidleyFlightFirst, EnemyProjectileFrameId.CeresRidleyFlightSecond,
            EnemyProjectileFrameId.CeresRidleyFlightThird, EnemyProjectileFrameId.CeresRidleyFlightFourth,
            EnemyProjectileFrameId.CeresRidleyAfterburnFinalFirst, EnemyProjectileFrameId.CeresRidleyAfterburnFinalSecond,
            EnemyProjectileFrameId.CeresRidleyAfterburnFinalThird, EnemyProjectileFrameId.CeresRidleyAfterburnFinalFourth,
            EnemyProjectileFrameId.CeresRidleyAfterburnFinalFifth,
            EnemyProjectileFrameId.CeresRidleyHorizontalCenterFirst, EnemyProjectileFrameId.CeresRidleyHorizontalCenterSecond,
            EnemyProjectileFrameId.CeresRidleyHorizontalCenterThird, EnemyProjectileFrameId.CeresRidleyHorizontalCenterFourth,
            EnemyProjectileFrameId.CeresRidleyHorizontalCenterFifth,
            EnemyProjectileFrameId.CeresRidleyVerticalCenterFirst, EnemyProjectileFrameId.CeresRidleyVerticalCenterSecond,
            EnemyProjectileFrameId.CeresRidleyVerticalCenterThird, EnemyProjectileFrameId.CeresRidleyVerticalCenterFourth,
            EnemyProjectileFrameId.CeresRidleyVerticalCenterFifth,
            EnemyProjectileFrameId.CeresRidleyDirectionalFirst, EnemyProjectileFrameId.CeresRidleyDirectionalSecond,
            EnemyProjectileFrameId.CeresRidleyDirectionalThird, EnemyProjectileFrameId.CeresRidleyDirectionalFourth,
            EnemyProjectileFrameId.CeresRidleyDirectionalFifth,
        ];
    }
}
