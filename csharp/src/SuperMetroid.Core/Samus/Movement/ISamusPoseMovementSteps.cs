namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The steps the pose that owns Samus's movement can take on one update (#2291, #2571), which the update that runs her
/// movement carries out against the loaded room; each movement type's <see cref="SamusPoseMovement"/> calls its own.
/// </summary>
internal interface ISamusPoseMovementSteps
{
    /// <summary>The standing movement: the forward-facing poses' and the landings' own steps, otherwise standing's.</summary>
    void StepStanding();

    /// <summary>The run.</summary>
    void StepRunning();

    /// <summary>The moonwalk.</summary>
    void StepMoonwalking();

    /// <summary>The stop against a wall she ran into.</summary>
    void StepRanIntoWall();

    /// <summary>The turn on the ground.</summary>
    void StepTurningOnGround();

    /// <summary>The jumping turns: an aimed crouched turn with no vertical direction turns on the ground, every other one in the air.</summary>
    void StepTurningWhileJumping();

    /// <summary>The turn in the air.</summary>
    void StepTurningInAir();

    /// <summary>The Morph Ball's or the Spring Ball's movement on the ground, the one <paramref name="movement"/> is.</summary>
    /// <param name="movement">The grounded ball's movement its movement type selects.</param>
    void StepBallGrounded(SamusMovement.GroundedBallMovement movement);

    /// <summary>The Morph Ball's or the Spring Ball's fall, the one <paramref name="movement"/> is.</summary>
    /// <param name="movement">The falling ball's movement its movement type selects.</param>
    void StepBallFalling(SamusMovement.FallingBallMovement movement);

    /// <summary>The Spring Ball's jump.</summary>
    void StepSpringBallInAir();

    /// <summary>The normal jump.</summary>
    void StepNormalJump();

    /// <summary>The spin jump.</summary>
    void StepSpinJump();

    /// <summary>The wall jump.</summary>
    void StepWallJump();

    /// <summary>The damage boost.</summary>
    void StepDamageBoost();

    /// <summary>The fall.</summary>
    void StepFalling();

    /// <summary>The crouch.</summary>
    void StepCrouching();

    /// <summary>The knockback movement type's normal movement: the one-frame grounding probe before the knockback ending's target.</summary>
    void StepKnockbackEnding();

    /// <summary>The posture transitions: the morph transitions' own step, the Draygon-held stand-up's hold, otherwise the crouch and stand transitions'.</summary>
    void StepPostureTransition();

    /// <summary>The glitch ball's normal movement, which only clears the vertical solid-collision result.</summary>
    void HoldWhereInstalled();

    /// <summary>The Draygon-held ball's normal step, which only clears the vertical solid-collision result; the actual boss grab keeps Samus still instead.</summary>
    void StepDraygonHeld();

    /// <summary>The glitch-ball knockback's movement once its knockback ends: Samus holds still in it (#1535).</summary>
    void HoldStill();
}
