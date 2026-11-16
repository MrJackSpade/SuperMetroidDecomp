using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoProjectileAnimations"/> that only verification reads.</summary>
internal static class TorizoProjectileAnimationsConstants
{
    /// <summary>The family whose projectiles play <paramref name="animation"/>.</summary>
    internal static TorizoProjectileFamily Family(TorizoProjectileSegment animation) => animation switch
    {
        P.LowHealthExplosion or P.LowHealthExplosionBurst or P.DeathExplosion or P.DeathExplosionBurst or P.DeathSmoke or P.Deletion => TorizoProjectileFamily.Explosion,
        P.DroolFourFrameDelay or P.DroolTwoFrameDelay or P.DroolNoDelay or P.DroolFalling or P.DroolWallImpact or P.DroolFloorImpact => TorizoProjectileFamily.Drool,
        P.ExplosiveSwipe => TorizoProjectileFamily.Swipe,
        P.StatueFragmentTopMiddle or P.StatueFragmentTopRight or P.StatueFragmentMiddleLeft or P.StatueFragmentCentre or P.StatueFragmentMiddleRight or P.StatueFragmentBottomLeft or P.StatueFragmentBottomMiddle or P.StatueFragmentBottomRight => TorizoProjectileFamily.StatueFragment,
        P.ChozoOrbMovingLeft or P.ChozoOrbMovingRight or P.ChozoOrbWallImpact or P.ChozoOrbFloorImpact or P.ChozoOrbShot => TorizoProjectileFamily.ChozoOrb,
        P.SonicBoomFiredLeft or P.SonicBoomFlyingLeft or P.SonicBoomFiredRight or P.SonicBoomFlyingRight or P.SonicBoomWallImpact or P.SonicBoomWallImpactBurst => TorizoProjectileFamily.SonicBoom,
        P.LandingDustRightFoot or P.LandingDustLeftFoot => TorizoProjectileFamily.LandingDust,
        P.EggBouncingLeft or P.EggHatchingLeft or P.EggBouncingRight or P.EggHatchingRight or P.EggHatch or P.EggHatchedLeft or P.HatchlingChargingLeft or P.EggHatchedRight or P.HatchlingChargingRight or P.EggBreakLeft or P.EggBreakRight => TorizoProjectileFamily.Egg,
        P.SuperMissileRight or P.SuperMissileSpinningRight or P.SuperMissileLeft or P.SuperMissileSpinningLeft or P.SuperMissileImpact => TorizoProjectileFamily.SuperMissile,
        P.EyeBeamWallImpact or P.EyeBeamFloorImpact or P.EyeBeamFloorImpactFlames or P.EyeBeam => TorizoProjectileFamily.EyeBeam,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Torizo projectile animation."),
    };
}
