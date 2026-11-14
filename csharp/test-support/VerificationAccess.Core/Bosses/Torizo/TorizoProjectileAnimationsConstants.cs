using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoProjectileAnimations"/> that only verification reads.</summary>
internal static class TorizoProjectileAnimationsConstants
{
    /// <summary>The family whose projectiles play <paramref name="animation"/>.</summary>
    internal static TorizoProjectileFamily Family(TorizoProjectileSegment animation) => animation switch
    {
        P.LowHealthExplosion or P.DeathExplosion or P.DeathSmoke => TorizoProjectileFamily.Explosion,
        P.DroolFourFrameDelay or P.DroolTwoFrameDelay or P.DroolNoDelay or P.DroolWallImpact or P.DroolFloorImpact => TorizoProjectileFamily.Drool,
        P.ExplosiveSwipe => TorizoProjectileFamily.Swipe,
        P.FirstStatueFragment or P.SecondStatueFragment or P.ThirdStatueFragment or P.FourthStatueFragment or P.FifthStatueFragment or P.SixthStatueFragment or P.SeventhStatueFragment or P.EighthStatueFragment or P.NinthStatueFragment or P.TenthStatueFragment or P.EleventhStatueFragment or P.TwelfthStatueFragment or P.ThirteenthStatueFragment or P.FourteenthStatueFragment or P.FifteenthStatueFragment or P.SixteenthStatueFragment => TorizoProjectileFamily.StatueFragment,
        P.ChozoOrbMovingLeft or P.ChozoOrbMovingRight or P.ChozoOrbWallImpact or P.ChozoOrbFloorImpact or P.ChozoOrbShot => TorizoProjectileFamily.ChozoOrb,
        P.SonicBoomFiredLeft or P.SonicBoomFiredRight or P.SonicBoomWallImpact => TorizoProjectileFamily.SonicBoom,
        P.LandingDustRightFoot or P.LandingDustLeftFoot => TorizoProjectileFamily.LandingDust,
        P.EggBouncingLeft or P.EggHatchingLeft or P.EggBouncingRight or P.EggHatchingRight or P.EggHatchedLeft or P.EggHatchedRight or P.EggBreakLeft or P.EggBreakRight => TorizoProjectileFamily.Egg,
        P.SuperMissileRight or P.SuperMissileLeft or P.SuperMissileImpact => TorizoProjectileFamily.SuperMissile,
        P.EyeBeamWallImpact or P.EyeBeamFloorImpact or P.EyeBeam => TorizoProjectileFamily.EyeBeam,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Torizo projectile animation."),
    };
}
