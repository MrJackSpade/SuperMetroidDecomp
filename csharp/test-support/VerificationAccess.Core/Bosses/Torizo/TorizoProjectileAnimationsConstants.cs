using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using X = SuperMetroid.Core.Bosses.Torizo.TorizoExplosionSegment;
using W = SuperMetroid.Core.Bosses.Torizo.GoldenTorizoWeaponSegment;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which install family each Torizo projectile segment's frames belong to; only verification reads it.</summary>
internal static class TorizoProjectileAnimationsConstants
{
    /// <summary>The install family of a drool, swipe, fragment, flying sonic boom or landing dust segment.</summary>
    internal static TorizoProjectileFamily Family(TorizoProjectileSegment animation) => animation switch
    {
        P.DroolForming or P.DroolNoDelay or P.DroolFalling or P.DroolWallImpact or P.DroolFloorImpact => TorizoProjectileFamily.Drool,
        P.ExplosiveSwipe => TorizoProjectileFamily.Swipe,
        P.StatueFragmentTopMiddle or P.StatueFragmentTopRight or P.StatueFragmentMiddleLeft or P.StatueFragmentCentre or P.StatueFragmentMiddleRight or P.StatueFragmentBottomLeft or P.StatueFragmentBottomMiddle or P.StatueFragmentBottomRight => TorizoProjectileFamily.StatueFragment,
        P.SonicBoomFiredLeft or P.SonicBoomFlyingLeft or P.SonicBoomFiredRight or P.SonicBoomFlyingRight => TorizoProjectileFamily.SonicBoom,
        P.LandingDustRightFoot or P.LandingDustLeftFoot => TorizoProjectileFamily.LandingDust,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Torizo projectile animation."),
    };

    /// <summary>The install family of an explosion segment: a struck sonic boom's bursts draw from the sonic boom's frames.</summary>
    internal static TorizoProjectileFamily Family(TorizoExplosionSegment animation) => animation switch
    {
        X.LowHealthExplosion or X.LowHealthExplosionBurst or X.DeathExplosion or X.DeathExplosionBurst or X.DeathSmoke or X.Deletion => TorizoProjectileFamily.Explosion,
        X.SonicBoomWallImpact or X.SonicBoomWallImpactBurst => TorizoProjectileFamily.SonicBoom,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Torizo explosion animation."),
    };

    /// <summary>The install family of a Chozo orb segment.</summary>
    internal static TorizoProjectileFamily Family(TorizoChozoOrbSegment animation) => Enum.IsDefined(animation)
        ? TorizoProjectileFamily.ChozoOrb
        : throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Torizo Chozo orb animation.");

    /// <summary>The install family of an egg segment: a shot egg's burst draws from the Chozo orb's frames.</summary>
    internal static TorizoProjectileFamily Family(GoldenTorizoEggSegment animation) => animation switch
    {
        GoldenTorizoEggSegment.Shot => TorizoProjectileFamily.ChozoOrb,
        _ when Enum.IsDefined(animation) => TorizoProjectileFamily.Egg,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Golden Torizo egg animation."),
    };

    /// <summary>The install family of a super missile or eye beam segment.</summary>
    internal static TorizoProjectileFamily Family(GoldenTorizoWeaponSegment animation) => animation switch
    {
        W.SuperMissileRight or W.SuperMissileSpinningRight or W.SuperMissileLeft or W.SuperMissileSpinningLeft or W.SuperMissileImpact => TorizoProjectileFamily.SuperMissile,
        W.EyeBeamWallImpact or W.EyeBeamFloorImpact or W.EyeBeamFloorImpactWaiting or W.EyeBeamFloorFlames or W.EyeBeam => TorizoProjectileFamily.EyeBeam,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined Golden Torizo weapon animation."),
    };
}
