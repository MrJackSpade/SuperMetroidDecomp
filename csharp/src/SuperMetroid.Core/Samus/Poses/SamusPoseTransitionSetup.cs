using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The setup the pose update runs for an input transition (#2506): each transition of a pose's input list names it, so the
/// update installs the chosen pose by it directly.
/// Each setup runs itself through the pose phase (#2571), so no switch over setup identities picks the routine.
/// </summary>
internal sealed class SamusPoseTransitionSetup
{
    /// <summary>The setup's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs the setup.</summary>
    private readonly Action<SamusPoseTransitionPhase, SamusControlFrame, SamusState, SamusPoseId, SamusTerrain> _run;

    /// <summary>A setup named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The setup's name, for diagnostics.</param>
    /// <param name="run">Runs the setup.</param>
    private SamusPoseTransitionSetup(string name, Action<SamusPoseTransitionPhase, SamusControlFrame, SamusState, SamusPoseId, SamusTerrain> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>The transition keeps the pose Samus is in, so her input chooses no new pose and nothing is set up.</summary>
    internal static SamusPoseTransitionSetup KeepPose { get; } = new(nameof(KeepPose), (_, _, _, _, _) => { });

    /// <summary>She jumps from standing, a landing, a wall stop, running, a ground turn or a moonwalk turn.</summary>
    internal static SamusPoseTransitionSetup StartJump { get; } = new(nameof(StartJump),
        (owner, _, samus, targetPose, _) => owner.SetUpStartJump(samus, targetPose));

    /// <summary>She jumps from a crouch, raising the crouched body to the jump's.</summary>
    internal static SamusPoseTransitionSetup JumpFromCrouch { get; } = new(nameof(JumpFromCrouch),
        (owner, _, samus, targetPose, terrain) => owner.SetUpJumpFromCrouch(samus, targetPose, terrain));

    /// <summary>A knockback pose's escape chord starts the damage boost the other way.</summary>
    internal static SamusPoseTransitionSetup StartDamageBoost { get; } = new(nameof(StartDamageBoost),
        (owner, frame, samus, targetPose, _) => owner.SetUpStartDamageBoost(frame, samus, targetPose));

    /// <summary>She leaves a damage boost for the neutral or forward normal jump.</summary>
    internal static SamusPoseTransitionSetup JumpFromDamageBoost { get; } = new(nameof(JumpFromDamageBoost),
        (owner, _, samus, targetPose, _) => owner.SetUpJumpFromDamageBoost(samus, targetPose));

    /// <summary>A moonwalk reverses while Jump is held, starting the moonwalk turn that jumps.</summary>
    internal static SamusPoseTransitionSetup StartMoonwalkTurnJump { get; } = new(nameof(StartMoonwalkTurnJump),
        (owner, _, samus, targetPose, _) => owner.SetUpStartMoonwalkTurnJump(samus, targetPose));

    /// <summary>She starts, aims or leaves a moonwalk, or turns instead when moonwalking is switched off.</summary>
    internal static SamusPoseTransitionSetup ChangeMoonwalkPose { get; } = new(nameof(ChangeMoonwalkPose),
        (owner, _, samus, targetPose, _) => owner.SetUpChangeMoonwalkPose(samus, targetPose));

    /// <summary>She starts running the way she faces from standing, a landing or a wall stop.</summary>
    internal static SamusPoseTransitionSetup StartRunning { get; } = new(nameof(StartRunning),
        (owner, _, samus, targetPose, _) => owner.SetUpStartRunning(samus, targetPose));

    /// <summary>She aims or fires differently on the ground, standing, running, at a wall or crouched.</summary>
    internal static SamusPoseTransitionSetup ChangeGroundedAim { get; } = new(nameof(ChangeGroundedAim),
        (owner, _, samus, targetPose, _) => owner.SetUpChangeGroundedAim(samus, targetPose));

    /// <summary>She turns around on the ground, standing, running, moonwalking, at a wall, crouched or facing forward.</summary>
    internal static SamusPoseTransitionSetup TurnOnGround { get; } = new(nameof(TurnOnGround),
        (owner, _, samus, targetPose, _) => owner.SetUpTurnOnGround(samus, targetPose));

    /// <summary>She starts to crouch or to stand up through the posture change's poses.</summary>
    internal static SamusPoseTransitionSetup CrouchOrStand { get; } = new(nameof(CrouchOrStand),
        (owner, _, samus, targetPose, terrain) => owner.SetUpCrouchOrStand(samus, targetPose, terrain));

    /// <summary>She stands straight up from a crouch.</summary>
    internal static SamusPoseTransitionSetup StandFromCrouch { get; } = new(nameof(StandFromCrouch),
        (owner, _, samus, targetPose, terrain) => owner.SetUpStandFromCrouch(samus, targetPose, terrain));

    /// <summary>She starts to morph into the ball or to unmorph from it.</summary>
    internal static SamusPoseTransitionSetup MorphOrUnmorph { get; } = new(nameof(MorphOrUnmorph),
        (owner, _, samus, targetPose, terrain) => owner.SetUpMorphOrUnmorph(samus, targetPose, terrain));

    /// <summary>The ball rolls, stops or turns without leaving its shared animation.</summary>
    internal static SamusPoseTransitionSetup ChangeBallPose { get; } = new(nameof(ChangeBallPose),
        (owner, _, samus, targetPose, _) => owner.SetUpChangeBallPose(samus, targetPose));

    /// <summary>The Spring Ball jumps from the ground.</summary>
    internal static SamusPoseTransitionSetup JumpSpringBall { get; } = new(nameof(JumpSpringBall),
        (owner, _, samus, targetPose, _) => owner.SetUpJumpSpringBall(samus, targetPose));

    /// <summary>Unmorphing in the air gives way to a falling pose, keeping her fall.</summary>
    internal static SamusPoseTransitionSetup FallFromUnmorph { get; } = new(nameof(FallFromUnmorph),
        (owner, _, samus, targetPose, terrain) => owner.SetUpFallFromUnmorph(samus, targetPose, terrain));

    /// <summary>She turns around in a normal jump or a fall.</summary>
    internal static SamusPoseTransitionSetup TurnInAir { get; } = new(nameof(TurnInAir),
        (owner, _, samus, targetPose, terrain) => owner.SetUpTurnInAir(samus, targetPose, terrain));

    /// <summary>A spin or wall jump spins the other way, or keeps spinning.</summary>
    internal static SamusPoseTransitionSetup ChangeSpinDirection { get; } = new(nameof(ChangeSpinDirection),
        (owner, _, samus, targetPose, _) => owner.SetUpChangeSpinDirection(samus, targetPose));

    /// <summary>Aiming or firing ends a spin or wall jump in a normal jump, keeping its arc.</summary>
    internal static SamusPoseTransitionSetup AimFromSpin { get; } = new(nameof(AimFromSpin),
        (owner, _, samus, targetPose, terrain) => owner.SetUpAimFromSpin(samus, targetPose, terrain));

    /// <summary>She aims or fires differently in a normal jump or a fall, facing the same way.</summary>
    internal static SamusPoseTransitionSetup ChangeAerialAim { get; } = new(nameof(ChangeAerialAim),
        (owner, _, samus, targetPose, _) => owner.SetUpChangeAerialAim(samus, targetPose));

    /// <summary>A change to or from the compact straight-down aim in the air, making room for the larger body.</summary>
    internal static SamusPoseTransitionSetup ChangeCompactAerialPose { get; } = new(nameof(ChangeCompactAerialPose),
        (owner, _, samus, targetPose, terrain) => owner.SetUpChangeCompactAerialPose(samus, targetPose, terrain));

    /// <summary>A shinespark windup's direction launches the shinespark.</summary>
    internal static SamusPoseTransitionSetup AimShinespark { get; } = new(nameof(AimShinespark),
        (owner, _, samus, targetPose, _) => owner.SetUpAimShinespark(samus, targetPose));

    /// <summary>She aims, fires or struggles differently while Draygon holds her.</summary>
    internal static SamusPoseTransitionSetup ChangeDraygonGrabbedPose { get; } = new(nameof(ChangeDraygonGrabbedPose),
        (owner, _, samus, targetPose, _) => owner.SetUpChangeDraygonGrabbedPose(samus, targetPose));

    /// <summary>
    /// The stand-up Up chooses from the ball the Hyper Beam's freeze can leave in Draygon's hold (#2516): the game makes
    /// room for the larger body and starts the stand-up's animation, with no setup of the stand-up's own, and no other
    /// transition shares that change.
    /// </summary>
    internal static SamusPoseTransitionSetup StandUpFromDraygonHold { get; } = new(nameof(StandUpFromDraygonHold),
        (owner, _, samus, targetPose, terrain) => owner.SetUpStandUpFromDraygonHold(samus, targetPose, terrain));

    /// <summary>Installs the transition's target pose through the setup.</summary>
    /// <param name="owner">The phase that runs the setup.</param>
    /// <param name="frame">The frame the setup reads.</param>
    /// <param name="samus">The samus the setup reads.</param>
    /// <param name="targetPose">The targetPose the setup reads.</param>
    /// <param name="terrain">The terrain the setup reads.</param>
    internal void Apply(SamusPoseTransitionPhase owner, SamusControlFrame frame, SamusState samus, SamusPoseId targetPose, SamusTerrain terrain) => _run(owner, frame, samus, targetPose, terrain);

    /// <summary>Names the setup.</summary>
    public override string ToString() => _name;
}
