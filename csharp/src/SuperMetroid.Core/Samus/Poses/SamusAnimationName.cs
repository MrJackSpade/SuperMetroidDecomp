namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Which animation Samus animates by (#2048): every animation of hers has its own name, whatever poses animate by it, so
/// what a frame read past an animation reaches names the later animation itself rather than a pose kept to name it
/// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>). The installation keys each
/// animation's outcomes by this name.
/// </summary>
internal enum SamusAnimationName
{
    /// <summary>
    /// The animation <see cref="SamusPoseId.MovingRightNormalPose"/>, <see cref="SamusPoseId.MovingLeftNormalPose"/> and
    /// six more poses play: its run of ten frames, its slow stride of ten frames and its stride hold of one frame (<see
    /// cref="SamusAnimationDelayDefinitions.RunningAnimation"/>).
    /// </summary>
    Running,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingAimUpRightPose"/>, <see cref="SamusPoseId.StandingAimUpLeftPose"/>,
    /// <see cref="SamusPoseId.CrouchingAimUpRightPose"/> and <see cref="SamusPoseId.CrouchingAimUpLeftPose"/> play: its
    /// two frames (<see cref="SamusAnimationDelayDefinitions.AimingUpAnimation"/>).
    /// </summary>
    AimingUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkFacingLeftPose"/>, <see cref="SamusPoseId.MoonwalkFacingRightPose"/>
    /// and four more poses play: its six frames (<see cref="SamusAnimationDelayDefinitions.MoonwalkAnimation"/>).
    /// </summary>
    Moonwalk,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FiringLandingRightPose"/> and <see
    /// cref="SamusPoseId.NormalLandingRightPose"/> play: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.NormalLandingRightAnimation"/>).
    /// </summary>
    NormalLandingRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FiringLandingLeftPose"/> and <see cref="SamusPoseId.NormalLandingLeftPose"/>
    /// play: its two frames (<see cref="SamusAnimationDelayDefinitions.NormalLandingLeftAnimation"/>).
    /// </summary>
    NormalLandingLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.SpinLandingRightPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.SpinLandingRightAnimation"/>).
    /// </summary>
    SpinLandingRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.SpinLandingLeftPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.SpinLandingLeftAnimation"/>).
    /// </summary>
    SpinLandingLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.LandingAimUpRightPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.LandingAimUpRightAnimation"/>).
    /// </summary>
    LandingAimUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.LandingAimUpLeftPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.LandingAimUpLeftAnimation"/>).
    /// </summary>
    LandingAimUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.LandingAimDiagonalUpRightPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.LandingAimDiagonalUpRightAnimation"/>).
    /// </summary>
    LandingAimDiagonalUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.LandingAimDiagonalUpLeftPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.LandingAimDiagonalUpLeftAnimation"/>).
    /// </summary>
    LandingAimDiagonalUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.LandingAimDiagonalDownRightPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.LandingAimDiagonalDownRightAnimation"/>).
    /// </summary>
    LandingAimDiagonalDownRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.LandingAimDiagonalDownLeftPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.LandingAimDiagonalDownLeftAnimation"/>).
    /// </summary>
    LandingAimDiagonalDownLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DrainedCrouchingRightPose"/> plays: its collapse of four frames, its fall of
    /// one frame, its held of four frames and its stand up of one frame (<see
    /// cref="SamusAnimationDelayDefinitions.DrainedCrouchingRightAnimation"/>).
    /// </summary>
    DrainedCrouchingRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DrainedCrouchingLeftPose"/> plays: its collapse of three frames, its fall of
    /// one frame, its held of five frames, its stand up of three frames, its failed stand of five frames, its failed
    /// stand end of one frame and its frozen of one frame (<see
    /// cref="SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation"/>).
    /// </summary>
    DrainedCrouchingLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DrainedStandingRightPose"/> plays: its breathe of four frames and its stand
    /// up of one frame (<see cref="SamusAnimationDelayDefinitions.DrainedStandingRightAnimation"/>).
    /// </summary>
    DrainedStandingRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DrainedStandingLeftPose"/> plays: its breathe of four frames and its stand up
    /// of one frame (<see cref="SamusAnimationDelayDefinitions.DrainedStandingLeftAnimation"/>).
    /// </summary>
    DrainedStandingLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FacingRightNormalPose"/> and <see cref="SamusPoseId.FacingLeftNormalPose"/>
    /// play: its breathe of four frames and its low energy breathe of four frames (<see
    /// cref="SamusAnimationDelayDefinitions.StandingAnimation"/>).
    /// </summary>
    Standing,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingRightPose"/> and <see cref="SamusPoseId.CrouchingLeftPose"/> play:
    /// its breathe of four frames and its low energy breathe of four frames (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingAnimation"/>).
    /// </summary>
    Crouching,

    /// <summary>
    /// The animation <see cref="SamusPoseId.XrayingStandingRightPose"/>, <see
    /// cref="SamusPoseId.XrayingStandingLeftPose"/>, <see cref="SamusPoseId.XrayingCrouchingRightPose"/> and <see
    /// cref="SamusPoseId.XrayingCrouchingLeftPose"/> play: its five frames (<see
    /// cref="SamusAnimationDelayDefinitions.XrayingAnimation"/>).
    /// </summary>
    Xraying,

    /// <summary>
    /// The animation <see cref="SamusPoseId.RanIntoWallRightPose"/>, <see cref="SamusPoseId.RanIntoWallLeftPose"/> and 26
    /// more poses play: its one frame (<see cref="SamusAnimationDelayDefinitions.HeldStillAnimation"/>).
    /// </summary>
    HeldStill,

    /// <summary>
    /// The animation <see cref="SamusPoseId.GrappleStandingRightPose"/>, <see
    /// cref="SamusPoseId.GrappleStandingLeftPose"/>, <see cref="SamusPoseId.GrappleStandingDownRightPose"/> and <see
    /// cref="SamusPoseId.GrappleStandingDownLeftPose"/> play: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.GrappleStandingAnimation"/>).
    /// </summary>
    GrappleStanding,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.GrappleAirborneGunExtendedRightAnimation"/>).
    /// </summary>
    GrappleAirborneGunExtendedRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.GrappleAirborneAimDownRightAnimation"/>).
    /// </summary>
    GrappleAirborneAimDownRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.GrappleAirborneAimDiagonalDownRightAnimation"/>).
    /// </summary>
    GrappleAirborneAimDiagonalDownRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.GrappleSwingRightPose"/> and <see cref="SamusPoseId.GrappleSwingLeftPose"/>
    /// play: its 66 frames (<see cref="SamusAnimationDelayDefinitions.GrappleSwingAnimation"/>).
    /// </summary>
    GrappleSwing,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NeutralJumpTransitionRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NeutralJumpTransitionRightAnimation"/>).
    /// </summary>
    NeutralJumpTransitionRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NeutralJumpTransitionLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NeutralJumpTransitionLeftAnimation"/>).
    /// </summary>
    NeutralJumpTransitionLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpTransitionAimUpRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimUpRightAnimation"/>).
    /// </summary>
    NormalJumpTransitionAimUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpTransitionAimUpLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimUpLeftAnimation"/>).
    /// </summary>
    NormalJumpTransitionAimUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalUpRightAnimation"/>).
    /// </summary>
    NormalJumpTransitionAimDiagonalUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalUpLeftAnimation"/>).
    /// </summary>
    NormalJumpTransitionAimDiagonalUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalDownRightAnimation"/>).
    /// </summary>
    NormalJumpTransitionAimDiagonalDownRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpTransitionAimDiagonalDownLeftAnimation"/>).
    /// </summary>
    NormalJumpTransitionAimDiagonalDownLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.ShinesparkWindupRightPose"/>, <see
    /// cref="SamusPoseId.ShinesparkWindupLeftPose"/>, <see cref="SamusPoseId.NeutralJumpRightPose"/> and <see
    /// cref="SamusPoseId.NeutralJumpLeftPose"/> play: its six frames (<see
    /// cref="SamusAnimationDelayDefinitions.NeutralJumpAndWindupAnimation"/>).
    /// </summary>
    NeutralJumpAndWindup,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DamageBoostLeftPose"/> and <see cref="SamusPoseId.DamageBoostRightPose"/>
    /// play: its ten frames (<see cref="SamusAnimationDelayDefinitions.DamageBoostAnimation"/>).
    /// </summary>
    DamageBoost,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpAimUpRightPose"/> and <see
    /// cref="SamusPoseId.NormalJumpAimUpLeftPose"/> play: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpAimUpAnimation"/>).
    /// </summary>
    NormalJumpAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpAimDownRightPose"/> and <see
    /// cref="SamusPoseId.NormalJumpAimDownLeftPose"/> play: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpAimDownAnimation"/>).
    /// </summary>
    NormalJumpAimDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpForwardRightPose"/> and <see
    /// cref="SamusPoseId.NormalJumpForwardLeftPose"/> play: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpForwardAnimation"/>).
    /// </summary>
    NormalJumpForward,

    /// <summary>
    /// The animation <see cref="SamusPoseId.NormalJumpGunExtendedRightPose"/>, <see
    /// cref="SamusPoseId.NormalJumpGunExtendedLeftPose"/> and four more poses play: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.NormalJumpAimingAnimation"/>).
    /// </summary>
    NormalJumpAiming,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FallingRightPose"/> and <see cref="SamusPoseId.FallingLeftPose"/> play: its
    /// fall start of three frames and its fast fall of two frames (<see
    /// cref="SamusAnimationDelayDefinitions.FallingAnimation"/>).
    /// </summary>
    Falling,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FallingGunExtendedRightPose"/> and <see
    /// cref="SamusPoseId.FallingGunExtendedLeftPose"/> play: its fall start of three frames and its fast fall of two
    /// frames (<see cref="SamusAnimationDelayDefinitions.FallingGunExtendedAnimation"/>).
    /// </summary>
    FallingGunExtended,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FallingAimUpRightPose"/> and <see cref="SamusPoseId.FallingAimUpLeftPose"/>
    /// play: its three frames (<see cref="SamusAnimationDelayDefinitions.FallingAimUpAnimation"/>).
    /// </summary>
    FallingAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FallingAimDiagonalUpRightPose"/>, <see
    /// cref="SamusPoseId.FallingAimDiagonalUpLeftPose"/>, <see cref="SamusPoseId.FallingAimDiagonalDownRightPose"/> and
    /// <see cref="SamusPoseId.FallingAimDiagonalDownLeftPose"/> play: its aim of one frame and its aim held of one frame
    /// (<see cref="SamusAnimationDelayDefinitions.FallingAimDiagonalAnimation"/>).
    /// </summary>
    FallingAimDiagonal,

    /// <summary>
    /// The animation <see cref="SamusPoseId.FallingAimDownRightPose"/> and <see
    /// cref="SamusPoseId.FallingAimDownLeftPose"/> play: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.FallingAimDownAnimation"/>).
    /// </summary>
    FallingAimDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.KnockbackRightPose"/> and <see cref="SamusPoseId.KnockbackLeftPose"/> play:
    /// its knockback of two frames, its slow recoil of four frames, its slower recoil of two frames and its recoil hold
    /// of one frame (<see cref="SamusAnimationDelayDefinitions.KnockbackAnimation"/>).
    /// </summary>
    Knockback,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DraygonHeldBallLeftPose"/>, <see
    /// cref="SamusPoseId.MorphBallGroundRightPose"/> and seventeen more poses play: its roll of eight frames and its roll
    /// hold of one frame (<see cref="SamusAnimationDelayDefinitions.MorphBallAnimation"/>).
    /// </summary>
    MorphBall,

    /// <summary>
    /// The animation <see cref="SamusPoseId.SpinJumpRightPose"/> and <see cref="SamusPoseId.SpinJumpLeftPose"/> play: its
    /// spin of nine frames and its wall jump ready of one frame (<see
    /// cref="SamusAnimationDelayDefinitions.SpinJumpAnimation"/>).
    /// </summary>
    SpinJump,

    /// <summary>
    /// The animation <see cref="SamusPoseId.SpaceJumpRightPose"/> and <see cref="SamusPoseId.SpaceJumpLeftPose"/> play:
    /// its spin of nine frames and its wall jump ready of one frame (<see
    /// cref="SamusAnimationDelayDefinitions.SpaceJumpAnimation"/>).
    /// </summary>
    SpaceJump,

    /// <summary>
    /// The animation <see cref="SamusPoseId.ScrewAttackRightPose"/> and <see cref="SamusPoseId.ScrewAttackLeftPose"/>
    /// play: its spin of 25 frames and its wall jump ready of one frame (<see
    /// cref="SamusAnimationDelayDefinitions.ScrewAttackAnimation"/>).
    /// </summary>
    ScrewAttack,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftAnimation"/>).
    /// </summary>
    TurningRightToLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightAnimation"/>).
    /// </summary>
    TurningLeftToRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftJumpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAnimation"/>).
    /// </summary>
    TurningRightToLeftJump,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightJumpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAnimation"/>).
    /// </summary>
    TurningLeftToRightJump,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAnimation"/>).
    /// </summary>
    TurningRightToLeftCrouching,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAnimation"/>).
    /// </summary>
    TurningLeftToRightCrouching,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftFallingPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAnimation"/>).
    /// </summary>
    TurningRightToLeftFalling,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightFallingPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAnimation"/>).
    /// </summary>
    TurningLeftToRightFalling,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftAimUpAnimation"/>).
    /// </summary>
    TurningRightToLeftAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightAimUpAnimation"/>).
    /// </summary>
    TurningLeftToRightAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftAimDiagonalDownPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftAimDiagonalDownAnimation"/>).
    /// </summary>
    TurningRightToLeftAimDiagonalDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightAimDiagonalDownPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightAimDiagonalDownAnimation"/>).
    /// </summary>
    TurningLeftToRightAimDiagonalDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAimUpAnimation"/>).
    /// </summary>
    TurningRightToLeftJumpAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAimUpAnimation"/>).
    /// </summary>
    TurningLeftToRightJumpAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimDownPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAimDownAnimation"/>).
    /// </summary>
    TurningRightToLeftJumpAimDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimDownPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAimDownAnimation"/>).
    /// </summary>
    TurningLeftToRightJumpAimDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAimUpAnimation"/>).
    /// </summary>
    TurningRightToLeftFallingAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAimUpAnimation"/>).
    /// </summary>
    TurningLeftToRightFallingAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimDownPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAimDownAnimation"/>).
    /// </summary>
    TurningRightToLeftFallingAimDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimDownPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAimDownAnimation"/>).
    /// </summary>
    TurningLeftToRightFallingAimDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAimUpAnimation"/>).
    /// </summary>
    TurningRightToLeftCrouchingAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAimUpAnimation"/>).
    /// </summary>
    TurningLeftToRightCrouchingAimUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose"/> plays: its three frames
    /// (<see cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAimDiagonalDownAnimation"/>).
    /// </summary>
    TurningRightToLeftCrouchingAimDiagonalDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose"/> plays: its three frames
    /// (<see cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAimDiagonalDownAnimation"/>).
    /// </summary>
    TurningLeftToRightCrouchingAimDiagonalDown,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningRightToLeftAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningLeftToRightAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftJumpAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningRightToLeftJumpAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightJumpAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningLeftToRightJumpAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftFallingAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningRightToLeftFallingAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightFallingAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningLeftToRightFallingAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningRightToLeftCrouchingAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningRightToLeftCrouchingAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.TurningLeftToRightCrouchingAimDiagonalUpAnimation"/>).
    /// </summary>
    TurningLeftToRightCrouchingAimDiagonalUp,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkTurnJumpLeftPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpLeftAnimation"/>).
    /// </summary>
    MoonwalkTurnJumpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkTurnJumpRightPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpRightAnimation"/>).
    /// </summary>
    MoonwalkTurnJumpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimUpLeftPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimUpLeftAnimation"/>).
    /// </summary>
    MoonwalkTurnJumpAimUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimUpRightPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimUpRightAnimation"/>).
    /// </summary>
    MoonwalkTurnJumpAimUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimDownLeftPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimDownLeftAnimation"/>).
    /// </summary>
    MoonwalkTurnJumpAimDownLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimDownRightPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.MoonwalkTurnJumpAimDownRightAnimation"/>).
    /// </summary>
    MoonwalkTurnJumpAimDownRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.DraygonGrabbingLeftAnimation"/>).
    /// </summary>
    DraygonGrabbingLeft,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.CompactAirborneRightAnimation"/>).
    /// </summary>
    CompactAirborneRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.CompactAirborneLeftAnimation"/>).
    /// </summary>
    CompactAirborneLeft,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its nine frames (<see
    /// cref="SamusAnimationDelayDefinitions.CompactAirborneFallingAnimation"/>).
    /// </summary>
    CompactAirborneFalling,

    /// <summary>
    /// The animation <see cref="SamusPoseId.WallJumpRightPose"/> and <see cref="SamusPoseId.WallJumpLeftPose"/> play: its
    /// launch of two frames, its ordinary spin of eight frames, its space jump spin of eight frames and its screw attack
    /// spin of 24 frames (<see cref="SamusAnimationDelayDefinitions.WallJumpAnimation"/>).
    /// </summary>
    WallJump,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionRightAnimation"/>).
    /// </summary>
    CrouchingTransitionRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionLeftAnimation"/>).
    /// </summary>
    CrouchingTransitionLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MorphingTransitionRightPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.MorphingTransitionRightAnimation"/>).
    /// </summary>
    MorphingTransitionRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.MorphingTransitionLeftPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.MorphingTransitionLeftAnimation"/>).
    /// </summary>
    MorphingTransitionLeft,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.GlitchBallMorphingRightAnimation"/>).
    /// </summary>
    GlitchBallMorphingRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.GlitchBallMorphingLeftAnimation"/>).
    /// </summary>
    GlitchBallMorphingLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionRightAnimation"/>).
    /// </summary>
    StandingTransitionRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionLeftAnimation"/>).
    /// </summary>
    StandingTransitionLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.UnmorphingTransitionRightPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.UnmorphingTransitionRightAnimation"/>).
    /// </summary>
    UnmorphingTransitionRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.UnmorphingTransitionLeftPose"/> plays: its two frames (<see
    /// cref="SamusAnimationDelayDefinitions.UnmorphingTransitionLeftAnimation"/>).
    /// </summary>
    UnmorphingTransitionLeft,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.InstantMorphingRightAnimation"/>).
    /// </summary>
    InstantMorphingRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.InstantMorphingLeftAnimation"/>).
    /// </summary>
    InstantMorphingLeft,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.CompactMorphingRightAnimation"/>).
    /// </summary>
    CompactMorphingRight,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.DraygonHeldMorphingLeftAnimation"/>).
    /// </summary>
    DraygonHeldMorphingLeft,

    /// <summary>
    /// The animation no pose plays, which only a frame read past an earlier animation reaches: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.CompactStandingUpRightAnimation"/>).
    /// </summary>
    CompactStandingUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DraygonHeldStandingUpLeftPose"/> plays: its three frames (<see
    /// cref="SamusAnimationDelayDefinitions.DraygonHeldStandingUpLeftAnimation"/>).
    /// </summary>
    DraygonHeldStandingUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionAimUpRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimUpRightAnimation"/>).
    /// </summary>
    CrouchingTransitionAimUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionAimUpLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimUpLeftAnimation"/>).
    /// </summary>
    CrouchingTransitionAimUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalUpRightAnimation"/>).
    /// </summary>
    CrouchingTransitionAimDiagonalUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalUpLeftAnimation"/>).
    /// </summary>
    CrouchingTransitionAimDiagonalUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalDownRightAnimation"/>).
    /// </summary>
    CrouchingTransitionAimDiagonalDownRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.CrouchingTransitionAimDiagonalDownLeftAnimation"/>).
    /// </summary>
    CrouchingTransitionAimDiagonalDownLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionAimUpRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionAimUpRightAnimation"/>).
    /// </summary>
    StandingTransitionAimUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionAimUpLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionAimUpLeftAnimation"/>).
    /// </summary>
    StandingTransitionAimUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalUpRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalUpRightAnimation"/>).
    /// </summary>
    StandingTransitionAimDiagonalUpRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalUpLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalUpLeftAnimation"/>).
    /// </summary>
    StandingTransitionAimDiagonalUpLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalDownRightPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalDownRightAnimation"/>).
    /// </summary>
    StandingTransitionAimDiagonalDownRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalDownLeftPose"/> plays: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.StandingTransitionAimDiagonalDownLeftAnimation"/>).
    /// </summary>
    StandingTransitionAimDiagonalDownLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DraygonGrabbedMovingLeftPose"/> and <see
    /// cref="SamusPoseId.DraygonGrabbedMovingRightPose"/> play: its six frames (<see
    /// cref="SamusAnimationDelayDefinitions.DraygonGrabbedMovingAnimation"/>).
    /// </summary>
    DraygonGrabbedMoving,

    /// <summary>
    /// The animation <see cref="SamusPoseId.ShinesparkHorizontalRightPose"/>, <see
    /// cref="SamusPoseId.ShinesparkHorizontalLeftPose"/> and four more poses play: its one frame (<see
    /// cref="SamusAnimationDelayDefinitions.ShinesparkAnimation"/>).
    /// </summary>
    Shinespark,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrystalFlashRightPose"/> plays: its rise of four frames, its glow of four
    /// frames and its finish of three frames (<see cref="SamusAnimationDelayDefinitions.CrystalFlashRightAnimation"/>).
    /// </summary>
    CrystalFlashRight,

    /// <summary>
    /// The animation <see cref="SamusPoseId.CrystalFlashLeftPose"/> plays: its rise of four frames, its glow of four
    /// frames and its finish of three frames (<see cref="SamusAnimationDelayDefinitions.CrystalFlashLeftAnimation"/>).
    /// </summary>
    CrystalFlashLeft,

    /// <summary>
    /// The animation <see cref="SamusPoseId.DeathSequenceRightPose"/> and <see cref="SamusPoseId.DeathSequenceLeftPose"/>
    /// play: its six frames (<see cref="SamusAnimationDelayDefinitions.DeathSequenceAnimation"/>).
    /// </summary>
    DeathSequence,

    /// <summary>
    /// The animation <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/> and <see
    /// cref="SamusPoseId.ForwardFacingSuitedPose"/> play: its still of one frame and its appearance of 94 frames (<see
    /// cref="SamusAnimationDelayDefinitions.ForwardFacingAnimation"/>).
    /// </summary>
    ForwardFacing,
}
