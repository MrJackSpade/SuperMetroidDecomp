using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileProgram;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The presentation boundary of Bomb and Golden Torizo's projectiles (#641): the bank-$86 list entries the projectile
/// definitions name, and each typed frame's installed artwork key, which is its native spritemap operand's address.
/// </summary>
internal static class TorizoProjectileVisualDefinitions
{
    /// <summary>The native list ExplosionLowHealthInitial starts, at $86:A3CB, which projectile definitions name.</summary>
    internal const ushort ExplosionLowHealthInitialList = 0xa3cb;
    /// <summary>The native list ExplosionDeathInitial starts, at $86:A3FA, which projectile definitions name.</summary>
    internal const ushort ExplosionDeathInitialList = 0xa3fa;
    /// <summary>The native list DroolNoDelay starts, at $86:A472, which projectile definitions name.</summary>
    internal const ushort DroolNoDelayList = 0xa472;
    /// <summary>The native list SwipeInitial starts, at $86:A4AA, which projectile definitions name.</summary>
    internal const ushort SwipeInitialList = 0xa4aa;
    /// <summary>The native list StatueFragment8 starts, at $86:A54B, which projectile definitions name.</summary>
    internal const ushort StatueFragment8List = 0xa54b;
    /// <summary>The native list ChozoOrbMovingLeft starts, at $86:AB15, which projectile definitions name.</summary>
    internal const ushort ChozoOrbMovingLeftList = 0xab15;
    /// <summary>The native list ChozoOrbWallImpact starts, at $86:AB25, which projectile definitions name.</summary>
    internal const ushort ChozoOrbWallImpactList = 0xab25;
    /// <summary>The native list ChozoOrbShot starts, at $86:AB68, which projectile definitions name.</summary>
    internal const ushort ChozoOrbShotList = 0xab68;
    /// <summary>The native list SonicBoomFiredLeft starts, at $86:ADBF, which projectile definitions name.</summary>
    internal const ushort SonicBoomFiredLeftList = 0xadbf;
    /// <summary>The native list LandingDustRightFoot starts, at $86:AF9D, which projectile definitions name.</summary>
    internal const ushort LandingDustRightFootList = 0xaf9d;
    /// <summary>The native list LandingDustLeftFoot starts, at $86:AFB5, which projectile definitions name.</summary>
    internal const ushort LandingDustLeftFootList = 0xafb5;
    /// <summary>The native list EggBouncingLeft starts, at $86:B104, which projectile definitions name.</summary>
    internal const ushort EggBouncingLeftList = 0xb104;
    /// <summary>The native list SuperMissileRightInitial starts, at $86:B293, which projectile definitions name.</summary>
    internal const ushort SuperMissileRightInitialList = 0xb293;
    /// <summary>The native list SuperMissileImpact starts, at $86:B2EF, which projectile definitions name.</summary>
    internal const ushort SuperMissileImpactList = 0xb2ef;
    /// <summary>The native list EyeBeamNormal starts, at $86:B410, which projectile definitions name.</summary>
    internal const ushort EyeBeamNormalList = 0xb410;

    /// <summary>Each family's frames in installed order.</summary>
    internal static IReadOnlyList<TorizoProjectileFrame> Frames(TorizoProjectileFamily family) => family switch
    {
        TorizoProjectileFamily.Explosion => [new(P.ExplosionLowHealthInitialLoop, 0), new(P.ExplosionLowHealthInitialLoop, 1), new(P.ExplosionLowHealthInitialLoop, 2), new(P.ExplosionLowHealthInitialLoop, 3), new(P.ExplosionLowHealthInitialLoop, 4), new(P.ExplosionDeathInitialLoop, 0), new(P.ExplosionDeathInitialLoop, 1), new(P.ExplosionDeathInitialLoop, 2), new(P.ExplosionDeathInitialLoop, 3), new(P.ExplosionDeathInitialLoop, 4), new(P.ExplosionDeathInitialLoop, 5), new(P.ExplosionDeathSmokeSetupLoop, 0), new(P.ExplosionDeathSmokeSetupLoop, 1), new(P.ExplosionDeathSmokeSetupLoop, 2), new(P.ExplosionDeathSmokeSetupLoop, 3)],
        TorizoProjectileFamily.Drool => [new(P.DroolFourFrameDelay, 0), new(P.DroolTwoFrameDelay, 0), new(P.DroolNoDelay, 0), new(P.DroolFallingLoop, 0), new(P.DroolFloorImpact, 0), new(P.DroolFloorImpact, 1), new(P.DroolFloorImpact, 2)],
        TorizoProjectileFamily.Swipe => [new(P.SwipeInitial, 0), new(P.SwipeInitial, 1), new(P.SwipeInitial, 2), new(P.SwipeInitial, 3), new(P.SwipeInitial, 4)],
        TorizoProjectileFamily.StatueFragment => [new(P.StatueFragment0, 0), new(P.StatueFragment0, 1), new(P.StatueFragment1, 0), new(P.StatueFragment1, 1), new(P.StatueFragment2, 0), new(P.StatueFragment2, 1), new(P.StatueFragment3, 0), new(P.StatueFragment3, 1), new(P.StatueFragment4, 0), new(P.StatueFragment4, 1), new(P.StatueFragment5, 0), new(P.StatueFragment5, 1), new(P.StatueFragment6, 0), new(P.StatueFragment6, 1), new(P.StatueFragment7, 0), new(P.StatueFragment7, 1), new(P.StatueFragment8, 0), new(P.StatueFragment8, 1), new(P.StatueFragment9, 0), new(P.StatueFragment9, 1), new(P.StatueFragment10, 0), new(P.StatueFragment10, 1), new(P.StatueFragment11, 0), new(P.StatueFragment11, 1), new(P.StatueFragment12, 0), new(P.StatueFragment12, 1), new(P.StatueFragment13, 0), new(P.StatueFragment13, 1), new(P.StatueFragment14, 0), new(P.StatueFragment14, 1), new(P.StatueFragment15, 0), new(P.StatueFragment15, 1)],
        TorizoProjectileFamily.ChozoOrb => [new(P.ChozoOrbMovingLeft, 0), new(P.ChozoOrbMovingRight, 0), new(P.ChozoOrbWallImpact, 0), new(P.ChozoOrbWallImpact, 1), new(P.ChozoOrbWallImpact, 2), new(P.ChozoOrbWallImpact, 3), new(P.ChozoOrbWallImpact, 4), new(P.ChozoOrbFloorImpact, 0), new(P.ChozoOrbFloorImpact, 1), new(P.ChozoOrbFloorImpact, 2), new(P.ChozoOrbFloorImpact, 3), new(P.ChozoOrbFloorImpact, 4), new(P.ChozoOrbFloorImpact, 5), new(P.ChozoOrbShot, 0), new(P.ChozoOrbShot, 1), new(P.ChozoOrbShot, 2), new(P.ChozoOrbShot, 3), new(P.ChozoOrbShot, 4)],
        TorizoProjectileFamily.SonicBoom => [new(P.SonicBoomFiredLeft, 0), new(P.SonicBoomFiredLeft, 1), new(P.SonicBoomMovingLeft, 0), new(P.SonicBoomFiredRight, 0), new(P.SonicBoomFiredRight, 1), new(P.SonicBoomMovingRight, 0), new(P.SonicBoomWallImpactLoop, 0), new(P.SonicBoomWallImpactLoop, 1), new(P.SonicBoomWallImpactLoop, 2), new(P.SonicBoomWallImpactLoop, 3), new(P.SonicBoomWallImpactLoop, 4)],
        TorizoProjectileFamily.LandingDust => [new(P.LandingDustRightFoot, 0), new(P.LandingDustRightFoot, 1), new(P.LandingDustRightFoot, 2), new(P.LandingDustRightFoot, 3), new(P.LandingDustLeftFoot, 0), new(P.LandingDustLeftFoot, 1), new(P.LandingDustLeftFoot, 2), new(P.LandingDustLeftFoot, 3)],
        TorizoProjectileFamily.Egg => [new(P.EggBouncingLeft, 0), new(P.EggBouncingLeftHatching, 0), new(P.EggBouncingLeftHatching, 1), new(P.EggBouncingLeftHatching, 2), new(P.EggBouncingRight, 0), new(P.EggBouncingRightHatching, 0), new(P.EggBouncingRightHatching, 1), new(P.EggBouncingRightHatching, 2), new(P.EggHatchedLeftLoop, 0), new(P.EggHatchedLeftLoop, 1), new(P.EggHatchedLeftLoop, 2), new(P.EggHatchedLeftLoop, 3), new(P.EggHatchedRightLoop, 0), new(P.EggHatchedRightLoop, 1), new(P.EggHatchedRightLoop, 2), new(P.EggHatchedRightLoop, 3), new(P.EggBreakLeft, 0), new(P.EggBreakLeft, 1), new(P.EggBreakLeft, 2), new(P.EggBreakLeft, 3), new(P.EggBreakLeft, 4), new(P.EggBreakRight, 0), new(P.EggBreakRight, 1), new(P.EggBreakRight, 2), new(P.EggBreakRight, 3), new(P.EggBreakRight, 4)],
        TorizoProjectileFamily.SuperMissile => [new(P.SuperMissileRightInitial, 0), new(P.SuperMissileRightLoop, 0), new(P.SuperMissileRightLoop, 1), new(P.SuperMissileRightLoop, 2), new(P.SuperMissileRightLoop, 3), new(P.SuperMissileRightLoop, 4), new(P.SuperMissileRightLoop, 5), new(P.SuperMissileRightLoop, 6), new(P.SuperMissileRightLoop, 7), new(P.SuperMissileLeftInitial, 0), new(P.SuperMissileLeftLoop, 0), new(P.SuperMissileLeftLoop, 1), new(P.SuperMissileLeftLoop, 2), new(P.SuperMissileLeftLoop, 3), new(P.SuperMissileLeftLoop, 4), new(P.SuperMissileLeftLoop, 5), new(P.SuperMissileLeftLoop, 6), new(P.SuperMissileLeftLoop, 7), new(P.SuperMissileImpact, 0), new(P.SuperMissileImpact, 1), new(P.SuperMissileImpact, 2), new(P.SuperMissileImpact, 3), new(P.SuperMissileImpact, 4), new(P.SuperMissileImpact, 5)],
        TorizoProjectileFamily.EyeBeam => [new(P.EyeBeamWallImpact, 0), new(P.EyeBeamWallImpact, 1), new(P.EyeBeamWallImpact, 2), new(P.EyeBeamWallImpact, 3), new(P.EyeBeamWallImpact, 4), new(P.EyeBeamFloorImpactLoop, 0), new(P.EyeBeamFloorImpactLoop, 1), new(P.EyeBeamFloorImpactLoop, 2), new(P.EyeBeamFloorImpactLoop, 3), new(P.EyeBeamFloorImpactLoop, 4), new(P.EyeBeamFloorImpactLoop, 5), new(P.EyeBeamFloorImpactLoop, 6), new(P.EyeBeamNormal, 0), new(P.EyeBeamNormal, 1), new(P.EyeBeamNormal, 2), new(P.EyeBeamNormal, 3), new(P.EyeBeamNormal, 4)],
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined Torizo projectile family."),
    };

    /// <summary>The installed artwork key of <paramref name="frame"/>: its native operand's address.</summary>
    internal static ushort PresentationKey(TorizoProjectileFrame frame) => (frame.Program, frame.Phase) switch
    {
        (P.ExplosionLowHealthInitialLoop, 0) => 0xa3e2,
        (P.ExplosionLowHealthInitialLoop, 1) => 0xa3e6,
        (P.ExplosionLowHealthInitialLoop, 2) => 0xa3ea,
        (P.ExplosionLowHealthInitialLoop, 3) => 0xa3ee,
        (P.ExplosionLowHealthInitialLoop, 4) => 0xa3f2,
        (P.ExplosionDeathInitialLoop, 0) => 0xa415,
        (P.ExplosionDeathInitialLoop, 1) => 0xa419,
        (P.ExplosionDeathInitialLoop, 2) => 0xa41d,
        (P.ExplosionDeathInitialLoop, 3) => 0xa421,
        (P.ExplosionDeathInitialLoop, 4) => 0xa425,
        (P.ExplosionDeathInitialLoop, 5) => 0xa429,
        (P.ExplosionDeathSmokeSetupLoop, 0) => 0xa442,
        (P.ExplosionDeathSmokeSetupLoop, 1) => 0xa446,
        (P.ExplosionDeathSmokeSetupLoop, 2) => 0xa44a,
        (P.ExplosionDeathSmokeSetupLoop, 3) => 0xa44e,
        (P.DroolFourFrameDelay, 0) => 0xa46c,
        (P.DroolTwoFrameDelay, 0) => 0xa470,
        (P.DroolNoDelay, 0) => 0xa47c,
        (P.DroolFallingLoop, 0) => 0xa484,
        (P.DroolFloorImpact, 0) => 0xa492,
        (P.DroolFloorImpact, 1) => 0xa496,
        (P.DroolFloorImpact, 2) => 0xa49a,
        (P.SwipeInitial, 0) => 0xa4af,
        (P.SwipeInitial, 1) => 0xa4b3,
        (P.SwipeInitial, 2) => 0xa4b7,
        (P.SwipeInitial, 3) => 0xa4bb,
        (P.SwipeInitial, 4) => 0xa4bf,
        (P.StatueFragment0, 0) => 0xa4c5,
        (P.StatueFragment0, 1) => 0xa4d0,
        (P.StatueFragment1, 0) => 0xa4d6,
        (P.StatueFragment1, 1) => 0xa4e1,
        (P.StatueFragment2, 0) => 0xa4e7,
        (P.StatueFragment2, 1) => 0xa4f2,
        (P.StatueFragment3, 0) => 0xa4f8,
        (P.StatueFragment3, 1) => 0xa503,
        (P.StatueFragment4, 0) => 0xa509,
        (P.StatueFragment4, 1) => 0xa514,
        (P.StatueFragment5, 0) => 0xa51a,
        (P.StatueFragment5, 1) => 0xa525,
        (P.StatueFragment6, 0) => 0xa52b,
        (P.StatueFragment6, 1) => 0xa536,
        (P.StatueFragment7, 0) => 0xa53c,
        (P.StatueFragment7, 1) => 0xa547,
        (P.StatueFragment8, 0) => 0xa54d,
        (P.StatueFragment8, 1) => 0xa558,
        (P.StatueFragment9, 0) => 0xa55e,
        (P.StatueFragment9, 1) => 0xa569,
        (P.StatueFragment10, 0) => 0xa56f,
        (P.StatueFragment10, 1) => 0xa57a,
        (P.StatueFragment11, 0) => 0xa580,
        (P.StatueFragment11, 1) => 0xa58b,
        (P.StatueFragment12, 0) => 0xa591,
        (P.StatueFragment12, 1) => 0xa59c,
        (P.StatueFragment13, 0) => 0xa5a2,
        (P.StatueFragment13, 1) => 0xa5ad,
        (P.StatueFragment14, 0) => 0xa5b3,
        (P.StatueFragment14, 1) => 0xa5be,
        (P.StatueFragment15, 0) => 0xa5c4,
        (P.StatueFragment15, 1) => 0xa5cf,
        (P.ChozoOrbMovingLeft, 0) => 0xab17,
        (P.ChozoOrbMovingRight, 0) => 0xab1f,
        (P.ChozoOrbWallImpact, 0) => 0xab2d,
        (P.ChozoOrbWallImpact, 1) => 0xab31,
        (P.ChozoOrbWallImpact, 2) => 0xab35,
        (P.ChozoOrbWallImpact, 3) => 0xab39,
        (P.ChozoOrbWallImpact, 4) => 0xab3d,
        (P.ChozoOrbFloorImpact, 0) => 0xab50,
        (P.ChozoOrbFloorImpact, 1) => 0xab54,
        (P.ChozoOrbFloorImpact, 2) => 0xab58,
        (P.ChozoOrbFloorImpact, 3) => 0xab5c,
        (P.ChozoOrbFloorImpact, 4) => 0xab60,
        (P.ChozoOrbFloorImpact, 5) => 0xab64,
        (P.ChozoOrbShot, 0) => 0xab70,
        (P.ChozoOrbShot, 1) => 0xab74,
        (P.ChozoOrbShot, 2) => 0xab78,
        (P.ChozoOrbShot, 3) => 0xab7c,
        (P.ChozoOrbShot, 4) => 0xab80,
        (P.SonicBoomFiredLeft, 0) => 0xadc4,
        (P.SonicBoomFiredLeft, 1) => 0xadc8,
        (P.SonicBoomMovingLeft, 0) => 0xadcc,
        (P.SonicBoomFiredRight, 0) => 0xadd7,
        (P.SonicBoomFiredRight, 1) => 0xaddb,
        (P.SonicBoomMovingRight, 0) => 0xaddf,
        (P.SonicBoomWallImpactLoop, 0) => 0xadfb,
        (P.SonicBoomWallImpactLoop, 1) => 0xadff,
        (P.SonicBoomWallImpactLoop, 2) => 0xae03,
        (P.SonicBoomWallImpactLoop, 3) => 0xae07,
        (P.SonicBoomWallImpactLoop, 4) => 0xae0b,
        (P.LandingDustRightFoot, 0) => 0xaf9f,
        (P.LandingDustRightFoot, 1) => 0xafa5,
        (P.LandingDustRightFoot, 2) => 0xafab,
        (P.LandingDustRightFoot, 3) => 0xafb1,
        (P.LandingDustLeftFoot, 0) => 0xafb7,
        (P.LandingDustLeftFoot, 1) => 0xafbd,
        (P.LandingDustLeftFoot, 2) => 0xafc3,
        (P.LandingDustLeftFoot, 3) => 0xafc9,
        (P.EggBouncingLeft, 0) => 0xb106,
        (P.EggBouncingLeftHatching, 0) => 0xb10e,
        (P.EggBouncingLeftHatching, 1) => 0xb112,
        (P.EggBouncingLeftHatching, 2) => 0xb116,
        (P.EggBouncingRight, 0) => 0xb11e,
        (P.EggBouncingRightHatching, 0) => 0xb126,
        (P.EggBouncingRightHatching, 1) => 0xb12a,
        (P.EggBouncingRightHatching, 2) => 0xb12e,
        (P.EggHatchedLeftLoop, 0) => 0xb154,
        (P.EggHatchedLeftLoop, 1) => 0xb158,
        (P.EggHatchedLeftLoop, 2) => 0xb15c,
        (P.EggHatchedLeftLoop, 3) => 0xb160,
        (P.EggHatchedRightLoop, 0) => 0xb16f,
        (P.EggHatchedRightLoop, 1) => 0xb173,
        (P.EggHatchedRightLoop, 2) => 0xb177,
        (P.EggHatchedRightLoop, 3) => 0xb17b,
        (P.EggBreakLeft, 0) => 0xb194,
        (P.EggBreakLeft, 1) => 0xb198,
        (P.EggBreakLeft, 2) => 0xb19c,
        (P.EggBreakLeft, 3) => 0xb1a0,
        (P.EggBreakLeft, 4) => 0xb1a4,
        (P.EggBreakRight, 0) => 0xb1ac,
        (P.EggBreakRight, 1) => 0xb1b0,
        (P.EggBreakRight, 2) => 0xb1b4,
        (P.EggBreakRight, 3) => 0xb1b8,
        (P.EggBreakRight, 4) => 0xb1bc,
        (P.SuperMissileRightInitial, 0) => 0xb295,
        (P.SuperMissileRightLoop, 0) => 0xb29f,
        (P.SuperMissileRightLoop, 1) => 0xb2a3,
        (P.SuperMissileRightLoop, 2) => 0xb2a7,
        (P.SuperMissileRightLoop, 3) => 0xb2ab,
        (P.SuperMissileRightLoop, 4) => 0xb2af,
        (P.SuperMissileRightLoop, 5) => 0xb2b3,
        (P.SuperMissileRightLoop, 6) => 0xb2b7,
        (P.SuperMissileRightLoop, 7) => 0xb2bb,
        (P.SuperMissileLeftInitial, 0) => 0xb2c3,
        (P.SuperMissileLeftLoop, 0) => 0xb2cd,
        (P.SuperMissileLeftLoop, 1) => 0xb2d1,
        (P.SuperMissileLeftLoop, 2) => 0xb2d5,
        (P.SuperMissileLeftLoop, 3) => 0xb2d9,
        (P.SuperMissileLeftLoop, 4) => 0xb2dd,
        (P.SuperMissileLeftLoop, 5) => 0xb2e1,
        (P.SuperMissileLeftLoop, 6) => 0xb2e5,
        (P.SuperMissileLeftLoop, 7) => 0xb2e9,
        (P.SuperMissileImpact, 0) => 0xb302,
        (P.SuperMissileImpact, 1) => 0xb306,
        (P.SuperMissileImpact, 2) => 0xb30a,
        (P.SuperMissileImpact, 3) => 0xb30e,
        (P.SuperMissileImpact, 4) => 0xb312,
        (P.SuperMissileImpact, 5) => 0xb316,
        (P.EyeBeamWallImpact, 0) => 0xb3d1,
        (P.EyeBeamWallImpact, 1) => 0xb3d5,
        (P.EyeBeamWallImpact, 2) => 0xb3d9,
        (P.EyeBeamWallImpact, 3) => 0xb3dd,
        (P.EyeBeamWallImpact, 4) => 0xb3e1,
        (P.EyeBeamFloorImpactLoop, 0) => 0xb3e9,
        (P.EyeBeamFloorImpactLoop, 1) => 0xb3f4,
        (P.EyeBeamFloorImpactLoop, 2) => 0xb3f8,
        (P.EyeBeamFloorImpactLoop, 3) => 0xb400,
        (P.EyeBeamFloorImpactLoop, 4) => 0xb404,
        (P.EyeBeamFloorImpactLoop, 5) => 0xb408,
        (P.EyeBeamFloorImpactLoop, 6) => 0xb40c,
        (P.EyeBeamNormal, 0) => 0xb412,
        (P.EyeBeamNormal, 1) => 0xb416,
        (P.EyeBeamNormal, 2) => 0xb41a,
        (P.EyeBeamNormal, 3) => 0xb41e,
        (P.EyeBeamNormal, 4) => 0xb422,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Torizo projectile frame."),
    };
}
