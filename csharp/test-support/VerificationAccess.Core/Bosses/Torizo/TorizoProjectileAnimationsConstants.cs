using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileProgram;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoProjectileAnimations"/> that only verification reads.</summary>
internal static class TorizoProjectileAnimationsConstants
{
    /// <summary>The family whose projectiles run <paramref name="program"/>.</summary>
    internal static TorizoProjectileFamily Family(TorizoProjectileProgram program) => program switch
    {
        P.ExplosionLowHealthInitial or P.ExplosionLowHealthInitialLoop or P.ExplosionDeathInitial or P.ExplosionDeathInitialLoop or P.ExplosionDeathSmokeSetup or P.ExplosionDeathSmokeSetupLoop => TorizoProjectileFamily.Explosion,
        P.DroolFourFrameDelay or P.DroolTwoFrameDelay or P.DroolNoDelay or P.DroolFallingLoop or P.DroolWallImpact or P.DroolFloorImpact => TorizoProjectileFamily.Drool,
        P.SwipeInitial => TorizoProjectileFamily.Swipe,
        P.StatueFragment0 or P.StatueFragment1 or P.StatueFragment2 or P.StatueFragment3 or P.StatueFragment4 or P.StatueFragment5 or P.StatueFragment6 or P.StatueFragment7 or P.StatueFragment8 or P.StatueFragment9 or P.StatueFragment10 or P.StatueFragment11 or P.StatueFragment12 or P.StatueFragment13 or P.StatueFragment14 or P.StatueFragment15 => TorizoProjectileFamily.StatueFragment,
        P.ChozoOrbMovingLeft or P.ChozoOrbMovingRight or P.ChozoOrbWallImpact or P.ChozoOrbFloorImpact or P.ChozoOrbShot => TorizoProjectileFamily.ChozoOrb,
        P.SonicBoomFiredLeft or P.SonicBoomMovingLeft or P.SonicBoomFiredRight or P.SonicBoomMovingRight or P.SonicBoomWallImpact or P.SonicBoomWallImpactLoop => TorizoProjectileFamily.SonicBoom,
        P.LandingDustRightFoot or P.LandingDustLeftFoot => TorizoProjectileFamily.LandingDust,
        P.EggBouncingLeft or P.EggBouncingLeftHatching or P.EggBouncingRight or P.EggBouncingRightHatching or P.EggHatch or P.EggHatchedLeft or P.EggHatchedLeftLoop or P.EggHatchedRight or P.EggHatchedRightLoop or P.EggBreakLeft or P.EggBreakRight => TorizoProjectileFamily.Egg,
        P.SuperMissileRightInitial or P.SuperMissileRightLoop or P.SuperMissileLeftInitial or P.SuperMissileLeftLoop or P.SuperMissileImpact => TorizoProjectileFamily.SuperMissile,
        P.EyeBeamWallImpact or P.EyeBeamFloorImpact or P.EyeBeamFloorImpactLoop or P.EyeBeamNormal => TorizoProjectileFamily.EyeBeam,
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Torizo projectile program."),
    };
}
