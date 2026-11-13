namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The pose input-transition lists selected by each Samus pose.</summary>
internal enum SamusPoseInputList
{
    /// <summary>The input-transition list of <see cref="SamusPoseId.TurningRightToLeftJumpPose"/>: diagnostic list identity.</summary>
    EmptyTransitionList,
    /// <summary>The input-transition list of the forward facing power suit poses.</summary>
    ForwardFacingPowerSuitPoseList,
    /// <summary>The input-transition list of the facing right normal poses.</summary>
    FacingRightNormalPoseList,
    /// <summary>The input-transition list of the facing left normal poses.</summary>
    FacingLeftNormalPoseList,
    /// <summary>The input-transition list of the moving right normal poses.</summary>
    MovingRightNormalPoseList,
    /// <summary>The input-transition list of the moving left normal poses.</summary>
    MovingLeftNormalPoseList,
    /// <summary>The input-transition list of the neutral jump transition right poses.</summary>
    NeutralJumpTransitionRightPoseList,
    /// <summary>The input-transition list of the neutral jump transition left poses.</summary>
    NeutralJumpTransitionLeftPoseList,
    /// <summary>The input-transition list of the normal jump aim up right poses.</summary>
    NormalJumpAimUpRightPoseList,
    /// <summary>The input-transition list of the normal jump aim up left poses.</summary>
    NormalJumpAimUpLeftPoseList,
    /// <summary>The input-transition list of the damage boost left poses.</summary>
    DamageBoostLeftPoseList,
    /// <summary>The input-transition list of the damage boost right poses.</summary>
    DamageBoostRightPoseList,
    /// <summary>The input-transition list of the spin jump right poses.</summary>
    SpinJumpRightPoseList,
    /// <summary>The input-transition list of the spin jump left poses.</summary>
    SpinJumpLeftPoseList,
    /// <summary>The input-transition list of the space jump right poses.</summary>
    SpaceJumpRightPoseList,
    /// <summary>The input-transition list of the space jump left poses.</summary>
    SpaceJumpLeftPoseList,
    /// <summary>The input-transition list of the screw attack right poses.</summary>
    ScrewAttackRightPoseList,
    /// <summary>The input-transition list of the screw attack left poses.</summary>
    ScrewAttackLeftPoseList,
    /// <summary>The input-transition list of the morph ball ground right poses.</summary>
    MorphBallGroundRightPoseList,
    /// <summary>The input-transition list of the morph ball moving right poses.</summary>
    MorphBallMovingRightPoseList,
    /// <summary>The input-transition list of the morph ball moving left poses.</summary>
    MorphBallMovingLeftPoseList,
    /// <summary>The input-transition list of the morph ball ground left poses.</summary>
    MorphBallGroundLeftPoseList,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallRightPose"/>: diagnostic list identity.</summary>
    GlitchBallRightPoseList,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallLeftPose"/>: diagnostic list identity.</summary>
    GlitchBallLeftPoseList,
    /// <summary>The input-transition list of the crouching right poses.</summary>
    CrouchingRightPoseList,
    /// <summary>The input-transition list of the crouching left poses.</summary>
    CrouchingLeftPoseList,
    /// <summary>The input-transition list of the falling right poses.</summary>
    FallingRightPoseList,
    /// <summary>The input-transition list of the falling left poses.</summary>
    FallingLeftPoseList,
    /// <summary>The input-transition list of the morph ball falling right poses.</summary>
    MorphBallFallingRightPoseList,
    /// <summary>The input-transition list of the morph ball falling left poses.</summary>
    MorphBallFallingLeftPoseList,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallKnockbackRightPose"/>: diagnostic list identity.</summary>
    GlitchBallKnockbackRightPoseList,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallKnockbackLeftPose"/>: diagnostic list identity.</summary>
    GlitchBallKnockbackLeftPoseList,
    /// <summary>The input-transition list of the moonwalk facing left poses.</summary>
    MoonwalkFacingLeftPoseList,
    /// <summary>The input-transition list of the moonwalk facing right poses.</summary>
    MoonwalkFacingRightPoseList,
    /// <summary>The input-transition list of the knockback right poses.</summary>
    KnockbackRightPoseList,
    /// <summary>The input-transition list of the knockback left poses.</summary>
    KnockbackLeftPoseList,
    /// <summary>The input-transition list of the spring ball ground right poses.</summary>
    SpringBallGroundRightPoseList,
    /// <summary>The input-transition list of the spring ball ground left poses.</summary>
    SpringBallGroundLeftPoseList,
    /// <summary>The input-transition list of the spring ball falling right poses.</summary>
    SpringBallFallingRightPoseList,
    /// <summary>The input-transition list of the spring ball falling left poses.</summary>
    SpringBallFallingLeftPoseList,
    /// <summary>The input-transition list of the spring ball jump right poses.</summary>
    SpringBallJumpRightPoseList,
    /// <summary>The input-transition list of the spring ball jump left poses.</summary>
    SpringBallJumpLeftPoseList,
    /// <summary>The unused input-transition list of the compact airborne right poses.</summary>
    CompactAirborneRightPoseList,
    /// <summary>The unused input-transition list of the compact airborne left poses.</summary>
    CompactAirborneLeftPoseList,
    /// <summary>The unused input-transition list of the compact airborne falling right poses.</summary>
    CompactAirborneFallingRightPoseList,
    /// <summary>The unused input-transition list of the compact airborne falling left poses.</summary>
    CompactAirborneFallingLeftPoseList,
    /// <summary>The input-transition list of the wall jump right poses.</summary>
    WallJumpRightPoseList,
    /// <summary>The input-transition list of the wall jump left poses.</summary>
    WallJumpLeftPoseList,
    /// <summary>The input-transition list of the ran into wall right poses.</summary>
    RanIntoWallRightPoseList,
    /// <summary>The input-transition list of the ran into wall left poses.</summary>
    RanIntoWallLeftPoseList,
    /// <summary>The input-transition list of the normal jump gun extended right poses.</summary>
    NormalJumpGunExtendedRightPoseList,
    /// <summary>The input-transition list of the normal jump gun extended left poses.</summary>
    NormalJumpGunExtendedLeftPoseList,
    /// <summary>The input-transition list of the normal jump aim down right poses.</summary>
    NormalJumpAimDownRightPoseList,
    /// <summary>The input-transition list of the normal jump aim down left poses.</summary>
    NormalJumpAimDownLeftPoseList,
    /// <summary>The input-transition list of the unmorphing transition right poses.</summary>
    UnmorphingTransitionRightPoseList,
    /// <summary>The input-transition list of the unmorphing transition left poses.</summary>
    UnmorphingTransitionLeftPoseList,
    /// <summary>The input-transition list of the turning right to left poses.</summary>
    TurningRightToLeftPoseList,
    /// <summary>The input-transition list of the turning left to right poses.</summary>
    TurningLeftToRightPoseList,
    /// <summary>The input-transition list of the turning right to left aim up poses.</summary>
    TurningRightToLeftAimUpPoseList,
    /// <summary>The input-transition list of the turning left to right aim up poses.</summary>
    TurningLeftToRightAimUpPoseList,
    /// <summary>The input-transition list of the turning right to left aim diagonal down poses.</summary>
    TurningRightToLeftAimDiagonalDownPoseList,
    /// <summary>The input-transition list of the turning left to right aim diagonal down poses.</summary>
    TurningLeftToRightAimDiagonalDownPoseList,
    /// <summary>The input-transition list of the shinespark windup right poses.</summary>
    ShinesparkWindupRightPoseList,
    /// <summary>The input-transition list of the shinespark windup left poses.</summary>
    ShinesparkWindupLeftPoseList,
    /// <summary>The input-transition list of the falling aim down right poses.</summary>
    FallingAimDownRightPoseList,
    /// <summary>The input-transition list of the falling aim down left poses.</summary>
    FallingAimDownLeftPoseList,
    /// <summary>The unused input-transition list of the draygon held ball left poses.</summary>
    DraygonHeldBallLeftPoseList,
    /// <summary>The input-transition list of the draygon grabbed neutral left poses.</summary>
    DraygonGrabbedNeutralLeftPoseList,
    /// <summary>The input-transition list of the draygon grabbed neutral right poses.</summary>
    DraygonGrabbedNeutralRightPoseList,
    /// <summary>The input-transition list of the moving right gun extended poses.</summary>
    MovingRightGunExtendedPoseList,
    /// <summary>The input-transition list of the moving left gun extended poses.</summary>
    MovingLeftGunExtendedPoseList,
    /// <summary>The input-transition list of the falling gun extended right poses.</summary>
    FallingGunExtendedRightPoseList,
    /// <summary>The input-transition list of the falling gun extended left poses.</summary>
    FallingGunExtendedLeftPoseList,
    /// <summary>The input-transition list of the moonwalk turn jump left poses.</summary>
    MoonwalkTurnJumpLeftPoseList,
    /// <summary>The input-transition list of the moonwalk turn jump right poses.</summary>
    MoonwalkTurnJumpRightPoseList,
    /// <summary>The input-transition list of the moonwalk turn jump aim up left poses.</summary>
    MoonwalkTurnJumpAimUpLeftPoseList,
    /// <summary>The input-transition list of the moonwalk turn jump aim up right poses.</summary>
    MoonwalkTurnJumpAimUpRightPoseList,
    /// <summary>The input-transition list of the moonwalk turn jump aim down left poses.</summary>
    MoonwalkTurnJumpAimDownLeftPoseList,
    /// <summary>The input-transition list of the moonwalk turn jump aim down right poses.</summary>
    MoonwalkTurnJumpAimDownRightPoseList,
}
