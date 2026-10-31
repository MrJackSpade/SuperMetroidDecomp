namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed projectile programs (#641), one per bank-$86 list a spawn, pre-instruction, branch
/// or resumed sleep enters, in authored order. Generated from the verified native lists; see
/// <see cref="TorizoProjectileAnimations"/>.
/// </summary>
internal enum TorizoProjectileProgram
{
    /// <summary>The native list.</summary>
    ExplosionLowHealthInitial,
    /// <summary>The loop.</summary>
    ExplosionLowHealthInitialLoop,
    /// <summary>The native list.</summary>
    ExplosionDeathInitial,
    /// <summary>The loop.</summary>
    ExplosionDeathInitialLoop,
    /// <summary>The native list.</summary>
    ExplosionDeathSmokeSetup,
    /// <summary>The loop.</summary>
    ExplosionDeathSmokeSetupLoop,
    /// <summary>The native list.</summary>
    DroolFourFrameDelay,
    /// <summary>The native list.</summary>
    DroolTwoFrameDelay,
    /// <summary>The native list.</summary>
    DroolNoDelay,
    /// <summary>The native list.</summary>
    DroolFallingLoop,
    /// <summary>The native list.</summary>
    DroolWallImpact,
    /// <summary>The native list.</summary>
    DroolFloorImpact,
    /// <summary>The native list.</summary>
    SwipeInitial,
    /// <summary>The native list.</summary>
    StatueFragment0,
    /// <summary>The native list.</summary>
    StatueFragment1,
    /// <summary>The native list.</summary>
    StatueFragment2,
    /// <summary>The native list.</summary>
    StatueFragment3,
    /// <summary>The native list.</summary>
    StatueFragment4,
    /// <summary>The native list.</summary>
    StatueFragment5,
    /// <summary>The native list.</summary>
    StatueFragment6,
    /// <summary>The native list.</summary>
    StatueFragment7,
    /// <summary>The native list.</summary>
    StatueFragment8,
    /// <summary>The native list.</summary>
    StatueFragment9,
    /// <summary>The native list.</summary>
    StatueFragment10,
    /// <summary>The native list.</summary>
    StatueFragment11,
    /// <summary>The native list.</summary>
    StatueFragment12,
    /// <summary>The native list.</summary>
    StatueFragment13,
    /// <summary>The native list.</summary>
    StatueFragment14,
    /// <summary>The native list.</summary>
    StatueFragment15,
    /// <summary>The native list.</summary>
    ChozoOrbMovingLeft,
    /// <summary>The native list.</summary>
    ChozoOrbMovingRight,
    /// <summary>The native list.</summary>
    ChozoOrbWallImpact,
    /// <summary>The native list.</summary>
    ChozoOrbFloorImpact,
    /// <summary>The native list.</summary>
    ChozoOrbShot,
    /// <summary>The native list.</summary>
    SonicBoomFiredLeft,
    /// <summary>The native list.</summary>
    SonicBoomMovingLeft,
    /// <summary>The native list.</summary>
    SonicBoomFiredRight,
    /// <summary>The native list.</summary>
    SonicBoomMovingRight,
    /// <summary>The native list.</summary>
    SonicBoomWallImpact,
    /// <summary>The native list.</summary>
    SonicBoomWallImpactLoop,
    /// <summary>The native list.</summary>
    LandingDustRightFoot,
    /// <summary>The native list.</summary>
    LandingDustLeftFoot,
    /// <summary>The native list.</summary>
    EggBouncingLeft,
    /// <summary>The native list.</summary>
    EggBouncingLeftHatching,
    /// <summary>The native list.</summary>
    EggBouncingRight,
    /// <summary>The native list.</summary>
    EggBouncingRightHatching,
    /// <summary>The native list.</summary>
    EggHatch,
    /// <summary>The native list.</summary>
    EggHatchedLeft,
    /// <summary>The loop.</summary>
    EggHatchedLeftLoop,
    /// <summary>The native list.</summary>
    EggHatchedRight,
    /// <summary>The loop.</summary>
    EggHatchedRightLoop,
    /// <summary>The native list.</summary>
    EggBreakLeft,
    /// <summary>The native list.</summary>
    EggBreakRight,
    /// <summary>The native list.</summary>
    SuperMissileRightInitial,
    /// <summary>The native list.</summary>
    SuperMissileRightLoop,
    /// <summary>The native list.</summary>
    SuperMissileLeftInitial,
    /// <summary>The native list.</summary>
    SuperMissileLeftLoop,
    /// <summary>The native list.</summary>
    SuperMissileImpact,
    /// <summary>The native list.</summary>
    EyeBeamWallImpact,
    /// <summary>The native list.</summary>
    EyeBeamFloorImpact,
    /// <summary>The native list.</summary>
    EyeBeamFloorImpactLoop,
    /// <summary>The native list.</summary>
    EyeBeamNormal,
}
