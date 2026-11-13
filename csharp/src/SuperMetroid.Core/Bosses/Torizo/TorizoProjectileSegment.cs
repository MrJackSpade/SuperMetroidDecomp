namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed projectile programs (#641), one per animation a spawn, frame behaviour, branch
/// or resumed sleep enters, in authored order. Generated from the verified animations; see
/// <see cref="TorizoProjectileAnimations"/>.
/// </summary>
internal enum TorizoProjectileSegment
{
    /// <summary>Where the animation starts.</summary>
    ExplosionLowHealthInitial,
    /// <summary>The animation's repeating part.</summary>
    ExplosionLowHealthInitialLoop,
    /// <summary>Where the animation starts.</summary>
    ExplosionDeathInitial,
    /// <summary>The animation's repeating part.</summary>
    ExplosionDeathInitialLoop,
    /// <summary>Where the animation starts.</summary>
    ExplosionDeathSmokeSetup,
    /// <summary>The animation's repeating part.</summary>
    ExplosionDeathSmokeSetupLoop,
    /// <summary>Where the animation starts.</summary>
    DroolFourFrameDelay,
    /// <summary>Where the animation starts.</summary>
    DroolTwoFrameDelay,
    /// <summary>Where the animation starts.</summary>
    DroolNoDelay,
    /// <summary>Where the animation starts.</summary>
    DroolFallingLoop,
    /// <summary>Where the animation starts.</summary>
    DroolWallImpact,
    /// <summary>Where the animation starts.</summary>
    DroolFloorImpact,
    /// <summary>Where the animation starts.</summary>
    SwipeInitial,
    /// <summary>Where the animation starts.</summary>
    StatueFragment0,
    /// <summary>Where the animation starts.</summary>
    StatueFragment1,
    /// <summary>Where the animation starts.</summary>
    StatueFragment2,
    /// <summary>Where the animation starts.</summary>
    StatueFragment3,
    /// <summary>Where the animation starts.</summary>
    StatueFragment4,
    /// <summary>Where the animation starts.</summary>
    StatueFragment5,
    /// <summary>Where the animation starts.</summary>
    StatueFragment6,
    /// <summary>Where the animation starts.</summary>
    StatueFragment7,
    /// <summary>Where the animation starts.</summary>
    StatueFragment8,
    /// <summary>Where the animation starts.</summary>
    StatueFragment9,
    /// <summary>Where the animation starts.</summary>
    StatueFragment10,
    /// <summary>Where the animation starts.</summary>
    StatueFragment11,
    /// <summary>Where the animation starts.</summary>
    StatueFragment12,
    /// <summary>Where the animation starts.</summary>
    StatueFragment13,
    /// <summary>Where the animation starts.</summary>
    StatueFragment14,
    /// <summary>Where the animation starts.</summary>
    StatueFragment15,
    /// <summary>Where the animation starts.</summary>
    ChozoOrbMovingLeft,
    /// <summary>Where the animation starts.</summary>
    ChozoOrbMovingRight,
    /// <summary>Where the animation starts.</summary>
    ChozoOrbWallImpact,
    /// <summary>Where the animation starts.</summary>
    ChozoOrbFloorImpact,
    /// <summary>Where the animation starts.</summary>
    ChozoOrbShot,
    /// <summary>Where the animation starts.</summary>
    SonicBoomFiredLeft,
    /// <summary>Where the animation starts.</summary>
    SonicBoomMovingLeft,
    /// <summary>Where the animation starts.</summary>
    SonicBoomFiredRight,
    /// <summary>Where the animation starts.</summary>
    SonicBoomMovingRight,
    /// <summary>Where the animation starts.</summary>
    SonicBoomWallImpact,
    /// <summary>Where the animation starts.</summary>
    SonicBoomWallImpactLoop,
    /// <summary>Where the animation starts.</summary>
    LandingDustRightFoot,
    /// <summary>Where the animation starts.</summary>
    LandingDustLeftFoot,
    /// <summary>Where the animation starts.</summary>
    EggBouncingLeft,
    /// <summary>Where the animation starts.</summary>
    EggBouncingLeftHatching,
    /// <summary>Where the animation starts.</summary>
    EggBouncingRight,
    /// <summary>Where the animation starts.</summary>
    EggBouncingRightHatching,
    /// <summary>Where the animation starts.</summary>
    EggHatch,
    /// <summary>Where the animation starts.</summary>
    EggHatchedLeft,
    /// <summary>The animation's repeating part.</summary>
    EggHatchedLeftLoop,
    /// <summary>Where the animation starts.</summary>
    EggHatchedRight,
    /// <summary>The animation's repeating part.</summary>
    EggHatchedRightLoop,
    /// <summary>Where the animation starts.</summary>
    EggBreakLeft,
    /// <summary>Where the animation starts.</summary>
    EggBreakRight,
    /// <summary>Where the animation starts.</summary>
    SuperMissileRightInitial,
    /// <summary>Where the animation starts.</summary>
    SuperMissileRightLoop,
    /// <summary>Where the animation starts.</summary>
    SuperMissileLeftInitial,
    /// <summary>Where the animation starts.</summary>
    SuperMissileLeftLoop,
    /// <summary>Where the animation starts.</summary>
    SuperMissileImpact,
    /// <summary>Where the animation starts.</summary>
    EyeBeamWallImpact,
    /// <summary>Where the animation starts.</summary>
    EyeBeamFloorImpact,
    /// <summary>Where the animation starts.</summary>
    EyeBeamFloorImpactLoop,
    /// <summary>Where the animation starts.</summary>
    EyeBeamNormal,
}
