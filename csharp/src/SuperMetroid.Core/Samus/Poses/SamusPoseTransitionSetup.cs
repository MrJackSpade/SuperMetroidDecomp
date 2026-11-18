namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The setup the pose update runs for an input transition (#2506): each transition of a pose's input list names it, so the
/// update installs the chosen pose by it directly.
/// </summary>
public enum SamusPoseTransitionSetup
{
    /// <summary>The transition keeps the pose Samus is in, so her input chooses no new pose and nothing is set up.</summary>
    KeepPose,

    /// <summary>She jumps from standing, a landing, a wall stop, running, a ground turn or a moonwalk turn.</summary>
    StartJump,

    /// <summary>She jumps from a crouch, raising the crouched body to the jump's.</summary>
    JumpFromCrouch,

    /// <summary>A knockback pose's escape chord starts the damage boost the other way.</summary>
    StartDamageBoost,

    /// <summary>She leaves a damage boost for the neutral or forward normal jump.</summary>
    JumpFromDamageBoost,

    /// <summary>A moonwalk reverses while Jump is held, starting the moonwalk turn that jumps.</summary>
    StartMoonwalkTurnJump,

    /// <summary>She starts, aims or leaves a moonwalk, or turns instead when moonwalking is switched off.</summary>
    ChangeMoonwalkPose,

    /// <summary>She starts running the way she faces from standing, a landing or a wall stop.</summary>
    StartRunning,

    /// <summary>She aims or fires differently on the ground, standing, running, at a wall or crouched.</summary>
    ChangeGroundedAim,

    /// <summary>She turns around on the ground, standing, running, moonwalking, at a wall, crouched or facing forward.</summary>
    TurnOnGround,

    /// <summary>She starts to crouch or to stand up through the posture change's poses.</summary>
    CrouchOrStand,

    /// <summary>She stands straight up from a crouch.</summary>
    StandFromCrouch,

    /// <summary>She starts to morph into the ball or to unmorph from it.</summary>
    MorphOrUnmorph,

    /// <summary>The ball rolls, stops or turns without leaving its shared animation.</summary>
    ChangeBallPose,

    /// <summary>The Spring Ball jumps from the ground.</summary>
    JumpSpringBall,

    /// <summary>Unmorphing in the air gives way to a falling pose, keeping her fall.</summary>
    FallFromUnmorph,

    /// <summary>She turns around in a normal jump or a fall.</summary>
    TurnInAir,

    /// <summary>A spin or wall jump spins the other way, or keeps spinning.</summary>
    ChangeSpinDirection,

    /// <summary>Aiming or firing ends a spin or wall jump in a normal jump, keeping its arc.</summary>
    AimFromSpin,

    /// <summary>She aims or fires differently in a normal jump or a fall, facing the same way.</summary>
    ChangeAerialAim,

    /// <summary>A change to or from the compact straight-down aim in the air, making room for the larger body.</summary>
    ChangeCompactAerialPose,

    /// <summary>A shinespark windup's direction launches the shinespark.</summary>
    AimShinespark,

    /// <summary>She aims, fires or struggles differently while Draygon holds her.</summary>
    ChangeDraygonGrabbedPose,

    /// <summary>
    /// The stand-up Up chooses from the ball the Hyper Beam's freeze can leave in Draygon's hold. Its setup is not
    /// reproduced: the game makes room for the larger body and starts the stand-up's animation, with no setup of the
    /// stand-up's own, and no other transition shares that change.
    /// </summary>
    StandUpFromDraygonHold,
}
