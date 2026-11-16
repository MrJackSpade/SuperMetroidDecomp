namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Which animation Samus animates by (#2048): every animation of hers has its own name, whatever poses animate by it, so
/// what a frame read past an animation reaches names the later animation itself rather than a pose kept to name it
/// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>). The installation keys each
/// animation's outcomes by this name.
/// </summary>
internal enum SamusAnimationName
{
    /// <summary>The running animation (<see cref="SamusAnimationDelayDefinitions.RunningAnimation"/>).</summary>
    Running,

    /// <summary>The aiming up animation (<see cref="SamusAnimationDelayDefinitions.AimingUpAnimation"/>).</summary>
    AimingUp,

    /// <summary>The moonwalk animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkAnimation"/>).</summary>
    Moonwalk,

    /// <summary>The normal landing right animation (<see cref="SamusAnimationDelayDefinitions.NormalLandingRightAnimation"/>).</summary>
    NormalLandingRight,

    /// <summary>The normal landing left animation (<see cref="SamusAnimationDelayDefinitions.NormalLandingLeftAnimation"/>).</summary>
    NormalLandingLeft,

    /// <summary>The spin landing right animation (<see cref="SamusAnimationDelayDefinitions.SpinLandingRightAnimation"/>).</summary>
    SpinLandingRight,

    /// <summary>The spin landing left animation (<see cref="SamusAnimationDelayDefinitions.SpinLandingLeftAnimation"/>).</summary>
    SpinLandingLeft,

    /// <summary>The landing aim up right animation (<see cref="SamusAnimationDelayDefinitions.LandingAimUpRightAnimation"/>).</summary>
    LandingAimUpRight,

    /// <summary>The landing aim up left animation (<see cref="SamusAnimationDelayDefinitions.LandingAimUpLeftAnimation"/>).</summary>
    LandingAimUpLeft,

    /// <summary>The landing aim diagonal up right animation (<see cref="SamusAnimationDelayDefinitions.LandingAimDiagonalUpRightAnimation"/>).</summary>
    LandingAimDiagonalUpRight,

    /// <summary>The landing aim diagonal up left animation (<see cref="SamusAnimationDelayDefinitions.LandingAimDiagonalUpLeftAnimation"/>).</summary>
    LandingAimDiagonalUpLeft,

    /// <summary>The landing aim diagonal down right animation (<see cref="SamusAnimationDelayDefinitions.LandingAimDiagonalDownRightAnimation"/>).</summary>
    LandingAimDiagonalDownRight,

    /// <summary>The landing aim diagonal down left animation (<see cref="SamusAnimationDelayDefinitions.LandingAimDiagonalDownLeftAnimation"/>).</summary>
    LandingAimDiagonalDownLeft,

    /// <summary>The drained crouching right animation (<see cref="SamusAnimationDelayDefinitions.DrainedCrouchingRightAnimation"/>).</summary>
    DrainedCrouchingRight,

    /// <summary>The drained crouching left animation (<see cref="SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation"/>).</summary>
    DrainedCrouchingLeft,

    /// <summary>The drained standing right animation (<see cref="SamusAnimationDelayDefinitions.DrainedStandingRightAnimation"/>).</summary>
    DrainedStandingRight,

    /// <summary>The drained standing left animation (<see cref="SamusAnimationDelayDefinitions.DrainedStandingLeftAnimation"/>).</summary>
    DrainedStandingLeft,

    /// <summary>The standing animation (<see cref="SamusAnimationDelayDefinitions.StandingAnimation"/>).</summary>
    Standing,

    /// <summary>The crouching animation (<see cref="SamusAnimationDelayDefinitions.CrouchingAnimation"/>).</summary>
    Crouching,

    /// <summary>The xraying animation (<see cref="SamusAnimationDelayDefinitions.XrayingAnimation"/>).</summary>
    Xraying,

    /// <summary>The held still animation (<see cref="SamusAnimationDelayDefinitions.HeldStillAnimation"/>).</summary>
    HeldStill,

    /// <summary>The grapple standing animation (<see cref="SamusAnimationDelayDefinitions.GrappleStandingAnimation"/>).</summary>
    GrappleStanding,

    /// <summary>The grapple airborne gun extended right animation (<see cref="SamusAnimationDelayDefinitions.GrappleAirborneGunExtendedRightAnimation"/>).</summary>
    GrappleAirborneGunExtendedRight,

    /// <summary>The grapple airborne aim down right animation (<see cref="SamusAnimationDelayDefinitions.GrappleAirborneAimDownRightAnimation"/>).</summary>
    GrappleAirborneAimDownRight,

    /// <summary>The grapple airborne aim diagonal down right animation (<see cref="SamusAnimationDelayDefinitions.GrappleAirborneAimDiagonalDownRightAnimation"/>).</summary>
    GrappleAirborneAimDiagonalDownRight,

    /// <summary>The grapple swing animation (<see cref="SamusAnimationDelayDefinitions.GrappleSwingAnimation"/>).</summary>
    GrappleSwing,

    /// <summary>The neutral jump transition right animation (<see cref="SamusAnimationDelayDefinitions.NeutralJumpTransitionRightAnimation"/>).</summary>
    NeutralJumpTransitionRight,

    /// <summary>The neutral jump transition left animation (<see cref="SamusAnimationDelayDefinitions.NeutralJumpTransitionLeftAnimation"/>).</summary>
    NeutralJumpTransitionLeft,

    /// <summary>The normal jump transition aim up right animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimUpRightAnimation"/>).</summary>
    NormalJumpTransitionAimUpRight,

    /// <summary>The normal jump transition aim up left animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimUpLeftAnimation"/>).</summary>
    NormalJumpTransitionAimUpLeft,

    /// <summary>The normal jump transition aim diagonal up right animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalUpRightAnimation"/>).</summary>
    NormalJumpTransitionAimDiagonalUpRight,

    /// <summary>The normal jump transition aim diagonal up left animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalUpLeftAnimation"/>).</summary>
    NormalJumpTransitionAimDiagonalUpLeft,

    /// <summary>The normal jump transition aim diagonal down right animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalDownRightAnimation"/>).</summary>
    NormalJumpTransitionAimDiagonalDownRight,

    /// <summary>The normal jump transition aim diagonal down left animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalDownLeftAnimation"/>).</summary>
    NormalJumpTransitionAimDiagonalDownLeft,

    /// <summary>The neutral jump and windup animation (<see cref="SamusAnimationDelayDefinitions.NeutralJumpAndWindupAnimation"/>).</summary>
    NeutralJumpAndWindup,

    /// <summary>The damage boost animation (<see cref="SamusAnimationDelayDefinitions.DamageBoostAnimation"/>).</summary>
    DamageBoost,

    /// <summary>The normal jump aim up animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpAimUpAnimation"/>).</summary>
    NormalJumpAimUp,

    /// <summary>The normal jump aim down animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpAimDownAnimation"/>).</summary>
    NormalJumpAimDown,

    /// <summary>The normal jump forward animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpForwardAnimation"/>).</summary>
    NormalJumpForward,

    /// <summary>The normal jump aiming animation (<see cref="SamusAnimationDelayDefinitions.NormalJumpAimingAnimation"/>).</summary>
    NormalJumpAiming,

    /// <summary>The falling animation (<see cref="SamusAnimationDelayDefinitions.FallingAnimation"/>).</summary>
    Falling,

    /// <summary>The falling gun extended animation (<see cref="SamusAnimationDelayDefinitions.FallingGunExtendedAnimation"/>).</summary>
    FallingGunExtended,

    /// <summary>The falling aim up animation (<see cref="SamusAnimationDelayDefinitions.FallingAimUpAnimation"/>).</summary>
    FallingAimUp,

    /// <summary>The falling aim diagonal animation (<see cref="SamusAnimationDelayDefinitions.FallingAimDiagonalAnimation"/>).</summary>
    FallingAimDiagonal,

    /// <summary>The falling aim down animation (<see cref="SamusAnimationDelayDefinitions.FallingAimDownAnimation"/>).</summary>
    FallingAimDown,

    /// <summary>The knockback animation (<see cref="SamusAnimationDelayDefinitions.KnockbackAnimation"/>).</summary>
    Knockback,

    /// <summary>The morph ball animation (<see cref="SamusAnimationDelayDefinitions.MorphBallAnimation"/>).</summary>
    MorphBall,

    /// <summary>The spin jump animation (<see cref="SamusAnimationDelayDefinitions.SpinJumpAnimation"/>).</summary>
    SpinJump,

    /// <summary>The space jump animation (<see cref="SamusAnimationDelayDefinitions.SpaceJumpAnimation"/>).</summary>
    SpaceJump,

    /// <summary>The screw attack animation (<see cref="SamusAnimationDelayDefinitions.ScrewAttackAnimation"/>).</summary>
    ScrewAttack,

    /// <summary>The turning right to left animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftAnimation"/>).</summary>
    TurningRightToLeft,

    /// <summary>The turning left to right animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightAnimation"/>).</summary>
    TurningLeftToRight,

    /// <summary>The turning right to left jump animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAnimation"/>).</summary>
    TurningRightToLeftJump,

    /// <summary>The turning left to right jump animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAnimation"/>).</summary>
    TurningLeftToRightJump,

    /// <summary>The turning right to left crouching animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAnimation"/>).</summary>
    TurningRightToLeftCrouching,

    /// <summary>The turning left to right crouching animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAnimation"/>).</summary>
    TurningLeftToRightCrouching,

    /// <summary>The turning right to left falling animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAnimation"/>).</summary>
    TurningRightToLeftFalling,

    /// <summary>The turning left to right falling animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAnimation"/>).</summary>
    TurningLeftToRightFalling,

    /// <summary>The turning right to left aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftAimUpAnimation"/>).</summary>
    TurningRightToLeftAimUp,

    /// <summary>The turning left to right aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightAimUpAnimation"/>).</summary>
    TurningLeftToRightAimUp,

    /// <summary>The turning right to left aim diagonal down animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftAimDiagonalDownAnimation"/>).</summary>
    TurningRightToLeftAimDiagonalDown,

    /// <summary>The turning left to right aim diagonal down animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightAimDiagonalDownAnimation"/>).</summary>
    TurningLeftToRightAimDiagonalDown,

    /// <summary>The turning right to left jump aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAimUpAnimation"/>).</summary>
    TurningRightToLeftJumpAimUp,

    /// <summary>The turning left to right jump aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAimUpAnimation"/>).</summary>
    TurningLeftToRightJumpAimUp,

    /// <summary>The turning right to left jump aim down animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAimDownAnimation"/>).</summary>
    TurningRightToLeftJumpAimDown,

    /// <summary>The turning left to right jump aim down animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAimDownAnimation"/>).</summary>
    TurningLeftToRightJumpAimDown,

    /// <summary>The turning right to left falling aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAimUpAnimation"/>).</summary>
    TurningRightToLeftFallingAimUp,

    /// <summary>The turning left to right falling aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAimUpAnimation"/>).</summary>
    TurningLeftToRightFallingAimUp,

    /// <summary>The turning right to left falling aim down animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAimDownAnimation"/>).</summary>
    TurningRightToLeftFallingAimDown,

    /// <summary>The turning left to right falling aim down animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAimDownAnimation"/>).</summary>
    TurningLeftToRightFallingAimDown,

    /// <summary>The turning right to left crouching aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAimUpAnimation"/>).</summary>
    TurningRightToLeftCrouchingAimUp,

    /// <summary>The turning left to right crouching aim up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAimUpAnimation"/>).</summary>
    TurningLeftToRightCrouchingAimUp,

    /// <summary>The turning right to left crouching aim diagonal down animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAimDiagonalDownAnimation"/>).</summary>
    TurningRightToLeftCrouchingAimDiagonalDown,

    /// <summary>The turning left to right crouching aim diagonal down animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAimDiagonalDownAnimation"/>).</summary>
    TurningLeftToRightCrouchingAimDiagonalDown,

    /// <summary>The turning right to left aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftAimDiagonalUpAnimation"/>).</summary>
    TurningRightToLeftAimDiagonalUp,

    /// <summary>The turning left to right aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightAimDiagonalUpAnimation"/>).</summary>
    TurningLeftToRightAimDiagonalUp,

    /// <summary>The turning right to left jump aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAimDiagonalUpAnimation"/>).</summary>
    TurningRightToLeftJumpAimDiagonalUp,

    /// <summary>The turning left to right jump aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAimDiagonalUpAnimation"/>).</summary>
    TurningLeftToRightJumpAimDiagonalUp,

    /// <summary>The turning right to left falling aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAimDiagonalUpAnimation"/>).</summary>
    TurningRightToLeftFallingAimDiagonalUp,

    /// <summary>The turning left to right falling aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAimDiagonalUpAnimation"/>).</summary>
    TurningLeftToRightFallingAimDiagonalUp,

    /// <summary>The turning right to left crouching aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAimDiagonalUpAnimation"/>).</summary>
    TurningRightToLeftCrouchingAimDiagonalUp,

    /// <summary>The turning left to right crouching aim diagonal up animation (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAimDiagonalUpAnimation"/>).</summary>
    TurningLeftToRightCrouchingAimDiagonalUp,

    /// <summary>The moonwalk turn jump left animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpLeftAnimation"/>).</summary>
    MoonwalkTurnJumpLeft,

    /// <summary>The moonwalk turn jump right animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpRightAnimation"/>).</summary>
    MoonwalkTurnJumpRight,

    /// <summary>The moonwalk turn jump aim up left animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimUpLeftAnimation"/>).</summary>
    MoonwalkTurnJumpAimUpLeft,

    /// <summary>The moonwalk turn jump aim up right animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimUpRightAnimation"/>).</summary>
    MoonwalkTurnJumpAimUpRight,

    /// <summary>The moonwalk turn jump aim down left animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimDownLeftAnimation"/>).</summary>
    MoonwalkTurnJumpAimDownLeft,

    /// <summary>The moonwalk turn jump aim down right animation (<see cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimDownRightAnimation"/>).</summary>
    MoonwalkTurnJumpAimDownRight,

    /// <summary>The draygon grabbing left animation (<see cref="SamusAnimationDelayDefinitions.DraygonGrabbingLeftAnimation"/>).</summary>
    DraygonGrabbingLeft,

    /// <summary>The compact airborne right animation (<see cref="SamusAnimationDelayDefinitions.CompactAirborneRightAnimation"/>).</summary>
    CompactAirborneRight,

    /// <summary>The compact airborne left animation (<see cref="SamusAnimationDelayDefinitions.CompactAirborneLeftAnimation"/>).</summary>
    CompactAirborneLeft,

    /// <summary>The compact airborne falling animation (<see cref="SamusAnimationDelayDefinitions.CompactAirborneFallingAnimation"/>).</summary>
    CompactAirborneFalling,

    /// <summary>The wall jump animation (<see cref="SamusAnimationDelayDefinitions.WallJumpAnimation"/>).</summary>
    WallJump,

    /// <summary>The crouching transition right animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionRightAnimation"/>).</summary>
    CrouchingTransitionRight,

    /// <summary>The crouching transition left animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionLeftAnimation"/>).</summary>
    CrouchingTransitionLeft,

    /// <summary>The morphing transition right animation (<see cref="SamusAnimationDelayDefinitions.MorphingTransitionRightAnimation"/>).</summary>
    MorphingTransitionRight,

    /// <summary>The morphing transition left animation (<see cref="SamusAnimationDelayDefinitions.MorphingTransitionLeftAnimation"/>).</summary>
    MorphingTransitionLeft,

    /// <summary>The glitch ball morphing right animation (<see cref="SamusAnimationDelayDefinitions.GlitchBallMorphingRightAnimation"/>).</summary>
    GlitchBallMorphingRight,

    /// <summary>The glitch ball morphing left animation (<see cref="SamusAnimationDelayDefinitions.GlitchBallMorphingLeftAnimation"/>).</summary>
    GlitchBallMorphingLeft,

    /// <summary>The standing transition right animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionRightAnimation"/>).</summary>
    StandingTransitionRight,

    /// <summary>The standing transition left animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionLeftAnimation"/>).</summary>
    StandingTransitionLeft,

    /// <summary>The unmorphing transition right animation (<see cref="SamusAnimationDelayDefinitions.UnmorphingTransitionRightAnimation"/>).</summary>
    UnmorphingTransitionRight,

    /// <summary>The unmorphing transition left animation (<see cref="SamusAnimationDelayDefinitions.UnmorphingTransitionLeftAnimation"/>).</summary>
    UnmorphingTransitionLeft,

    /// <summary>The instant morphing right animation (<see cref="SamusAnimationDelayDefinitions.InstantMorphingRightAnimation"/>).</summary>
    InstantMorphingRight,

    /// <summary>The instant morphing left animation (<see cref="SamusAnimationDelayDefinitions.InstantMorphingLeftAnimation"/>).</summary>
    InstantMorphingLeft,

    /// <summary>The compact morphing right animation (<see cref="SamusAnimationDelayDefinitions.CompactMorphingRightAnimation"/>).</summary>
    CompactMorphingRight,

    /// <summary>The draygon held morphing left animation (<see cref="SamusAnimationDelayDefinitions.DraygonHeldMorphingLeftAnimation"/>).</summary>
    DraygonHeldMorphingLeft,

    /// <summary>The compact standing up right animation (<see cref="SamusAnimationDelayDefinitions.CompactStandingUpRightAnimation"/>).</summary>
    CompactStandingUpRight,

    /// <summary>The draygon held standing up left animation (<see cref="SamusAnimationDelayDefinitions.DraygonHeldStandingUpLeftAnimation"/>).</summary>
    DraygonHeldStandingUpLeft,

    /// <summary>The crouching transition aim up right animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimUpRightAnimation"/>).</summary>
    CrouchingTransitionAimUpRight,

    /// <summary>The crouching transition aim up left animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimUpLeftAnimation"/>).</summary>
    CrouchingTransitionAimUpLeft,

    /// <summary>The crouching transition aim diagonal up right animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalUpRightAnimation"/>).</summary>
    CrouchingTransitionAimDiagonalUpRight,

    /// <summary>The crouching transition aim diagonal up left animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalUpLeftAnimation"/>).</summary>
    CrouchingTransitionAimDiagonalUpLeft,

    /// <summary>The crouching transition aim diagonal down right animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalDownRightAnimation"/>).</summary>
    CrouchingTransitionAimDiagonalDownRight,

    /// <summary>The crouching transition aim diagonal down left animation (<see cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalDownLeftAnimation"/>).</summary>
    CrouchingTransitionAimDiagonalDownLeft,

    /// <summary>The standing transition aim up right animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionAimUpRightAnimation"/>).</summary>
    StandingTransitionAimUpRight,

    /// <summary>The standing transition aim up left animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionAimUpLeftAnimation"/>).</summary>
    StandingTransitionAimUpLeft,

    /// <summary>The standing transition aim diagonal up right animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalUpRightAnimation"/>).</summary>
    StandingTransitionAimDiagonalUpRight,

    /// <summary>The standing transition aim diagonal up left animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalUpLeftAnimation"/>).</summary>
    StandingTransitionAimDiagonalUpLeft,

    /// <summary>The standing transition aim diagonal down right animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalDownRightAnimation"/>).</summary>
    StandingTransitionAimDiagonalDownRight,

    /// <summary>The standing transition aim diagonal down left animation (<see cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalDownLeftAnimation"/>).</summary>
    StandingTransitionAimDiagonalDownLeft,

    /// <summary>The draygon grabbed moving animation (<see cref="SamusAnimationDelayDefinitions.DraygonGrabbedMovingAnimation"/>).</summary>
    DraygonGrabbedMoving,

    /// <summary>The shinespark animation (<see cref="SamusAnimationDelayDefinitions.ShinesparkAnimation"/>).</summary>
    Shinespark,

    /// <summary>The crystal flash right animation (<see cref="SamusAnimationDelayDefinitions.CrystalFlashRightAnimation"/>).</summary>
    CrystalFlashRight,

    /// <summary>The crystal flash left animation (<see cref="SamusAnimationDelayDefinitions.CrystalFlashLeftAnimation"/>).</summary>
    CrystalFlashLeft,

    /// <summary>The death sequence animation (<see cref="SamusAnimationDelayDefinitions.DeathSequenceAnimation"/>).</summary>
    DeathSequence,

    /// <summary>The forward facing animation (<see cref="SamusAnimationDelayDefinitions.ForwardFacingAnimation"/>).</summary>
    ForwardFacing,
}
