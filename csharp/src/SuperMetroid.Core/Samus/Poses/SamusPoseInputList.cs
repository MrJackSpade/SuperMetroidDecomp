namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The input-transition lists Samus's poses select (#1929), each named for the poses it serves: poses that share their
/// transitions share a list.
/// </summary>
internal enum SamusPoseInputList
{
    /// <summary>
    /// The list with no transitions, which every pose whose input does not change it selects: turns in the air and while
    /// crouching or falling, posture and morph transitions, the Grapple's poses, the shinespark, Crystal Flash, the X-ray
    /// scope, death and the drained poses.
    /// </summary>
    NoInputTransitions,
    /// <summary>The input-transition list of both forward-facing poses, in the power suit and suited.</summary>
    ForwardFacing,
    /// <summary>The input-transition list of the right-facing standing poses and the landings, whatever their aim.</summary>
    StandingAndLandingRight,
    /// <summary>The input-transition list of the left-facing standing poses and the landings, whatever their aim.</summary>
    StandingAndLandingLeft,
    /// <summary>The input-transition list of the right-facing running poses, whatever their aim.</summary>
    RunningRight,
    /// <summary>The input-transition list of the left-facing running poses, whatever their aim.</summary>
    RunningLeft,
    /// <summary>The input-transition list of the right-facing jump transitions, whatever their aim.</summary>
    JumpTransitionRight,
    /// <summary>The input-transition list of the left-facing jump transitions, whatever their aim.</summary>
    JumpTransitionLeft,
    /// <summary>The input-transition list of the right-facing neutral, forward, aimed-up and diagonal normal jumps.</summary>
    NormalJumpRight,
    /// <summary>The input-transition list of the left-facing neutral, forward, aimed-up and diagonal normal jumps.</summary>
    NormalJumpLeft,
    /// <summary>The input-transition list of the damage boost left poses.</summary>
    DamageBoostLeft,
    /// <summary>The input-transition list of the damage boost right poses.</summary>
    DamageBoostRight,
    /// <summary>The input-transition list of the spin jump right poses.</summary>
    SpinJumpRight,
    /// <summary>The input-transition list of the spin jump left poses.</summary>
    SpinJumpLeft,
    /// <summary>The input-transition list of the space jump right poses.</summary>
    SpaceJumpRight,
    /// <summary>The input-transition list of the space jump left poses.</summary>
    SpaceJumpLeft,
    /// <summary>The input-transition list of the screw attack right poses.</summary>
    ScrewAttackRight,
    /// <summary>The input-transition list of the screw attack left poses.</summary>
    ScrewAttackLeft,
    /// <summary>The input-transition list of the morph ball ground right poses.</summary>
    MorphBallGroundRight,
    /// <summary>The input-transition list of the morph ball moving right poses.</summary>
    MorphBallMovingRight,
    /// <summary>The input-transition list of the morph ball moving left poses.</summary>
    MorphBallMovingLeft,
    /// <summary>The input-transition list of the morph ball ground left poses.</summary>
    MorphBallGroundLeft,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallRightPose"/>.</summary>
    GlitchBallRight,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallLeftPose"/>.</summary>
    GlitchBallLeft,
    /// <summary>The input-transition list of the crouching right poses.</summary>
    CrouchingRight,
    /// <summary>The input-transition list of the crouching left poses.</summary>
    CrouchingLeft,
    /// <summary>The input-transition list of the falling right poses.</summary>
    FallingRight,
    /// <summary>The input-transition list of the falling left poses.</summary>
    FallingLeft,
    /// <summary>The input-transition list of the morph ball falling right poses.</summary>
    MorphBallFallingRight,
    /// <summary>The input-transition list of the morph ball falling left poses.</summary>
    MorphBallFallingLeft,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallKnockbackRightPose"/>.</summary>
    GlitchBallKnockbackRight,
    /// <summary>The input-transition list of <see cref="SamusPoseId.GlitchBallKnockbackLeftPose"/>.</summary>
    GlitchBallKnockbackLeft,
    /// <summary>The input-transition list of the left-facing moonwalk poses, whatever their aim.</summary>
    MoonwalkLeft,
    /// <summary>The input-transition list of the right-facing moonwalk poses, whatever their aim.</summary>
    MoonwalkRight,
    /// <summary>The input-transition list of the knockback right poses.</summary>
    KnockbackRight,
    /// <summary>The input-transition list of the knockback left poses.</summary>
    KnockbackLeft,
    /// <summary>The input-transition list of the right-facing spring ball on the ground, still or moving.</summary>
    SpringBallGroundRight,
    /// <summary>The input-transition list of the left-facing spring ball on the ground, still or moving.</summary>
    SpringBallGroundLeft,
    /// <summary>The input-transition list of the spring ball falling right poses.</summary>
    SpringBallFallingRight,
    /// <summary>The input-transition list of the spring ball falling left poses.</summary>
    SpringBallFallingLeft,
    /// <summary>The input-transition list of the spring ball jump right poses.</summary>
    SpringBallJumpRight,
    /// <summary>The input-transition list of the spring ball jump left poses.</summary>
    SpringBallJumpLeft,
    /// <summary>The unused input-transition list of the compact airborne right poses.</summary>
    CompactAirborneRight,
    /// <summary>The unused input-transition list of the compact airborne left poses.</summary>
    CompactAirborneLeft,
    /// <summary>The unused input-transition list of the compact airborne falling right poses.</summary>
    CompactAirborneFallingRight,
    /// <summary>The unused input-transition list of the compact airborne falling left poses.</summary>
    CompactAirborneFallingLeft,
    /// <summary>The input-transition list of the wall jump right poses.</summary>
    WallJumpRight,
    /// <summary>The input-transition list of the wall jump left poses.</summary>
    WallJumpLeft,
    /// <summary>The input-transition list of the ran into wall right poses.</summary>
    RanIntoWallRight,
    /// <summary>The input-transition list of the ran into wall left poses.</summary>
    RanIntoWallLeft,
    /// <summary>The input-transition list of the normal jump gun extended right poses.</summary>
    NormalJumpGunExtendedRight,
    /// <summary>The input-transition list of the normal jump gun extended left poses.</summary>
    NormalJumpGunExtendedLeft,
    /// <summary>The input-transition list of the normal jump aim down right poses.</summary>
    NormalJumpAimDownRight,
    /// <summary>The input-transition list of the normal jump aim down left poses.</summary>
    NormalJumpAimDownLeft,
    /// <summary>The input-transition list of the unmorphing transition right poses.</summary>
    UnmorphingTransitionRight,
    /// <summary>The input-transition list of the unmorphing transition left poses.</summary>
    UnmorphingTransitionLeft,
    /// <summary>The input-transition list of the turning right to left poses.</summary>
    TurningRightToLeft,
    /// <summary>The input-transition list of the turning left to right poses.</summary>
    TurningLeftToRight,
    /// <summary>The input-transition list of the turning right to left aim up poses.</summary>
    TurningRightToLeftAimUp,
    /// <summary>The input-transition list of the turning left to right aim up poses.</summary>
    TurningLeftToRightAimUp,
    /// <summary>The input-transition list of the turning right to left aim diagonal down poses.</summary>
    TurningRightToLeftAimDiagonalDown,
    /// <summary>The input-transition list of the turning left to right aim diagonal down poses.</summary>
    TurningLeftToRightAimDiagonalDown,
    /// <summary>The input-transition list of the shinespark windup right poses.</summary>
    ShinesparkWindupRight,
    /// <summary>The input-transition list of the shinespark windup left poses.</summary>
    ShinesparkWindupLeft,
    /// <summary>The input-transition list of the falling aim down right poses.</summary>
    FallingAimDownRight,
    /// <summary>The input-transition list of the falling aim down left poses.</summary>
    FallingAimDownLeft,
    /// <summary>The unused input-transition list of the draygon held ball left poses.</summary>
    DraygonHeldBallLeft,
    /// <summary>The input-transition list of every left-facing pose Draygon's grab holds Samus in.</summary>
    DraygonGrabbedLeft,
    /// <summary>The input-transition list of every right-facing pose Draygon's grab holds Samus in.</summary>
    DraygonGrabbedRight,
    /// <summary>The input-transition list of the moving right gun extended poses.</summary>
    MovingRightGunExtended,
    /// <summary>The input-transition list of the moving left gun extended poses.</summary>
    MovingLeftGunExtended,
    /// <summary>The input-transition list of the falling gun extended right poses.</summary>
    FallingGunExtendedRight,
    /// <summary>The input-transition list of the falling gun extended left poses.</summary>
    FallingGunExtendedLeft,
    /// <summary>The input-transition list of the moonwalk turn jump left poses.</summary>
    MoonwalkTurnJumpLeft,
    /// <summary>The input-transition list of the moonwalk turn jump right poses.</summary>
    MoonwalkTurnJumpRight,
    /// <summary>The input-transition list of the moonwalk turn jump aim up left poses.</summary>
    MoonwalkTurnJumpAimUpLeft,
    /// <summary>The input-transition list of the moonwalk turn jump aim up right poses.</summary>
    MoonwalkTurnJumpAimUpRight,
    /// <summary>The input-transition list of the moonwalk turn jump aim down left poses.</summary>
    MoonwalkTurnJumpAimDownLeft,
    /// <summary>The input-transition list of the moonwalk turn jump aim down right poses.</summary>
    MoonwalkTurnJumpAimDownRight,
}
