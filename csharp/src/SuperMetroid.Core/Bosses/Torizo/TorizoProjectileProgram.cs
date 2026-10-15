namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's typed projectile programs (#641), one per bank-$86 list a spawn, pre-instruction, branch
/// or resumed sleep enters, in authored order. Generated from the verified native lists; see
/// <see cref="TorizoProjectileAnimations"/>.
/// </summary>
internal enum TorizoProjectileProgram
{
    /// <summary>The native list at $86:A3CB.</summary>
    ExplosionLowHealthInitial,
    /// <summary>The loop at $86:A3D5.</summary>
    ExplosionLowHealthInitialLoop,
    /// <summary>The native list at $86:A3FA.</summary>
    ExplosionDeathInitial,
    /// <summary>The loop at $86:A408.</summary>
    ExplosionDeathInitialLoop,
    /// <summary>The native list at $86:A431.</summary>
    ExplosionDeathSmokeSetup,
    /// <summary>The loop at $86:A435.</summary>
    ExplosionDeathSmokeSetupLoop,
    /// <summary>The native list at $86:A46A.</summary>
    DroolFourFrameDelay,
    /// <summary>The native list at $86:A46E.</summary>
    DroolTwoFrameDelay,
    /// <summary>The native list at $86:A472.</summary>
    DroolNoDelay,
    /// <summary>The native list at $86:A482.</summary>
    DroolFallingLoop,
    /// <summary>The native list at $86:A48A.</summary>
    DroolWallImpact,
    /// <summary>The native list at $86:A48E.</summary>
    DroolFloorImpact,
    /// <summary>The native list at $86:A4AA.</summary>
    SwipeInitial,
    /// <summary>The native list at $86:A4C3.</summary>
    StatueFragment0,
    /// <summary>The native list at $86:A4D4.</summary>
    StatueFragment1,
    /// <summary>The native list at $86:A4E5.</summary>
    StatueFragment2,
    /// <summary>The native list at $86:A4F6.</summary>
    StatueFragment3,
    /// <summary>The native list at $86:A507.</summary>
    StatueFragment4,
    /// <summary>The native list at $86:A518.</summary>
    StatueFragment5,
    /// <summary>The native list at $86:A529.</summary>
    StatueFragment6,
    /// <summary>The native list at $86:A53A.</summary>
    StatueFragment7,
    /// <summary>The native list at $86:A54B.</summary>
    StatueFragment8,
    /// <summary>The native list at $86:A55C.</summary>
    StatueFragment9,
    /// <summary>The native list at $86:A56D.</summary>
    StatueFragment10,
    /// <summary>The native list at $86:A57E.</summary>
    StatueFragment11,
    /// <summary>The native list at $86:A58F.</summary>
    StatueFragment12,
    /// <summary>The native list at $86:A5A0.</summary>
    StatueFragment13,
    /// <summary>The native list at $86:A5B1.</summary>
    StatueFragment14,
    /// <summary>The native list at $86:A5C2.</summary>
    StatueFragment15,
    /// <summary>The native list at $86:AB15.</summary>
    ChozoOrbMovingLeft,
    /// <summary>The native list at $86:AB1D.</summary>
    ChozoOrbMovingRight,
    /// <summary>The native list at $86:AB25.</summary>
    ChozoOrbWallImpact,
    /// <summary>The native list at $86:AB41.</summary>
    ChozoOrbFloorImpact,
    /// <summary>The native list at $86:AB68.</summary>
    ChozoOrbShot,
    /// <summary>The native list at $86:ADBF.</summary>
    SonicBoomFiredLeft,
    /// <summary>The native list at $86:ADCA.</summary>
    SonicBoomMovingLeft,
    /// <summary>The native list at $86:ADD2.</summary>
    SonicBoomFiredRight,
    /// <summary>The native list at $86:ADDD.</summary>
    SonicBoomMovingRight,
    /// <summary>The native list at $86:ADE5.</summary>
    SonicBoomWallImpact,
    /// <summary>The native list at $86:ADF1.</summary>
    SonicBoomWallImpactLoop,
    /// <summary>The native list at $86:AF9D.</summary>
    LandingDustRightFoot,
    /// <summary>The native list at $86:AFB5.</summary>
    LandingDustLeftFoot,
    /// <summary>The native list at $86:B104.</summary>
    EggBouncingLeft,
    /// <summary>The native list at $86:B10A.</summary>
    EggBouncingLeftHatching,
    /// <summary>The native list at $86:B11C.</summary>
    EggBouncingRight,
    /// <summary>The native list at $86:B122.</summary>
    EggBouncingRightHatching,
    /// <summary>The native list at $86:B134.</summary>
    EggHatch,
    /// <summary>The native list at $86:B14B.</summary>
    EggHatchedLeft,
    /// <summary>The loop at $86:B152.</summary>
    EggHatchedLeftLoop,
    /// <summary>The native list at $86:B166.</summary>
    EggHatchedRight,
    /// <summary>The loop at $86:B16D.</summary>
    EggHatchedRightLoop,
    /// <summary>The native list at $86:B190.</summary>
    EggBreakLeft,
    /// <summary>The native list at $86:B1A8.</summary>
    EggBreakRight,
    /// <summary>The native list at $86:B293.</summary>
    SuperMissileRightInitial,
    /// <summary>The native list at $86:B29D.</summary>
    SuperMissileRightLoop,
    /// <summary>The native list at $86:B2C1.</summary>
    SuperMissileLeftInitial,
    /// <summary>The native list at $86:B2CB.</summary>
    SuperMissileLeftLoop,
    /// <summary>The native list at $86:B2EF.</summary>
    SuperMissileImpact,
    /// <summary>The native list at $86:B3CD.</summary>
    EyeBeamWallImpact,
    /// <summary>The native list at $86:B3E5.</summary>
    EyeBeamFloorImpact,
    /// <summary>The native list at $86:B3E7.</summary>
    EyeBeamFloorImpactLoop,
    /// <summary>The native list at $86:B410.</summary>
    EyeBeamNormal,
}
