using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.World;

/// <summary>
/// One update's normal pose movement (#2291, #2571, #2553), as the movement type of her pose selects it: shared by
/// gameplay's movement phase and the intro demos, each giving it the terrain she collides with, the update's buttons and
/// frame counter, the elevator under her, and the update's Samus control, which keeps the movement's result.
/// </summary>
/// <param name="samus">Samus.</param>
/// <param name="terrain">The terrain her movement collides with and reacts to.</param>
/// <param name="frame">The update's Samus control, which keeps the movement's result for the rest of the update.</param>
/// <param name="held">The buttons held at the update's controller read.</param>
/// <param name="newlyPressed">The buttons newly pressed at it.</param>
/// <param name="frameCounter">The update's frame counter.</param>
/// <param name="elevator">The elevator under her, which a forward-facing pose reads.</param>
internal sealed partial class SamusPoseMovementRun(SamusState samus, SamusTerrain terrain, SamusControlFrame frame, GameplayAction held,
    GameplayAction newlyPressed, int frameCounter, SamusElevatorFooting elevator) : ISamusPoseMovementSteps
{
    /// <summary>Runs the normal movement of <paramref name="movement"/>, the movement type that selects Samus's mover.</summary>
    /// <exception cref="InvalidDataException">The movement type has no movement its pose owns.</exception>
    internal static void Step(SamusMovementType movement, SamusPoseMovementRun run)
    {
        // The special movement type returns before this, and grappling, a compact airborne pose and the unused movement
        // types are owned by their own movers; any other is corrupt pose state.
        SamusPoseMovement step = SamusMovementTypeDefinitions.Movements[movement].PoseMovement is { } owned
            ? owned
            : throw new InvalidDataException(
                $"Runtime movement step received pose {run.Samus.Pose}, whose movement type {movement} has no movement its pose owns.");
        step.Step(run);
    }

    /// <summary>Samus.</summary>
    private SamusState Samus { get; } = samus;

    /// <inheritdoc/>
    public void StepStanding()
    {
        // The standing movement type's movement: the forward-facing poses' and the landings' own steps, otherwise standing's.
        if (SamusPoseMetadata.IsForwardFacingPose(Samus.Pose))
        {
            // The two front-facing poses enter through the controller-locked demo and elevator setups: a forward-facing
            // Samus moves nothing while the elevator is at rest and scans one pixel down while it moves.
            SamusGroundedMovement.StepFacingForward(terrain, Samus, frameCounter, elevatorIsMoving: elevator.Moving,
                doorBlocks: elevator.DoorBlocks);
        }
        else if (SamusPoseMetadata.IsLandingPose(Samus.Pose))
            frame.Record(SamusGroundedMovement.StepLanding(terrain, Samus, frameCounter));
        else
            frame.Record(SamusGroundedMovement.StepStanding(terrain, Samus, frameCounter, held));
    }

    /// <inheritdoc/>
    public void StepRunning() => frame.Record(SamusGroundedMovement.StepRunning(terrain, Samus, frameCounter, held));

    /// <inheritdoc/>
    public void StepMoonwalking() => frame.Record(SamusGroundedMovement.StepMoonwalking(SamusMovement.Moonwalking, terrain, Samus, frameCounter));

    /// <inheritdoc/>
    public void StepRanIntoWall() => frame.Record(SamusGroundedMovement.StepRanIntoWall(terrain, Samus, frameCounter));

    /// <inheritdoc/>
    public void StepTurningOnGround() => StepTurningOnGround(SamusMovement.TurningOnGround);

    /// <inheritdoc/>
    public void StepTurningWhileJumping()
    {
        // An aimed crouched turn with no vertical direction turns on the ground, and every other jumping turn, an aimed
        // crouched one moving up or down among them, turns in the air. Only these turns read the vertical direction to
        // choose their movement. Elsewhere it is data the chosen movement reads: the plain crouched turns, which turn on
        // the ground, can retain a downward direction and still turn on the ground.
        if (SamusPoseMetadata.IsAimedCrouchingTurnPose(Samus.Pose) && Samus.Kinematics.VerticalVelocity.HasNoDirection)
            StepTurningOnGround(SamusMovement.AimedCrouchedTurningOnGround);
        else
            StepTurningInAir(SamusMovement.TurningRisingOrAimedCrouched);
    }

    /// <inheritdoc/>
    public void StepTurningInAir() => StepTurningInAir(SamusMovement.TurningWhileFalling);

    /// <inheritdoc/>
    public void StepBallGrounded(SamusMovement.GroundedBallMovement movement) => frame.Record(SamusMorphBallMovement.StepGrounded(movement, terrain, Samus, frameCounter));

    /// <inheritdoc/>
    public void StepBallFalling(SamusMovement.FallingBallMovement movement) => frame.Record(SamusMorphBallMovement.StepFalling(movement, terrain, Samus, held, frameCounter));

    /// <inheritdoc/>
    public void StepSpringBallInAir() => frame.Record(SamusMorphBallMovement.StepSpringBallInAir(SamusMovement.SpringBallInAir, terrain, Samus, held, frameCounter));

    /// <inheritdoc/>
    public void StepNormalJump() => frame.Record(SamusAerialMovement.StepNormalJump(SamusMovement.NormalJumping, terrain, Samus, held, frameCounter));

    /// <inheritdoc/>
    public void StepSpinJump() => frame.Record(SamusAerialMovement.StepSpinJump(SamusMovement.SpinJumping, terrain, Samus, held, frameCounter, newlyPressed));

    /// <inheritdoc/>
    public void StepWallJump() => frame.Record(SamusAerialMovement.StepWallJump(SamusMovement.WallJumping, terrain, Samus, held, frameCounter));

    /// <inheritdoc/>
    public void StepDamageBoost() => frame.Record(SamusAerialMovement.StepDamageBoost(SamusMovement.DamageBoost, terrain, Samus, held, frameCounter));

    /// <inheritdoc/>
    public void StepFalling() => frame.Record(SamusAerialMovement.StepFalling(SamusMovement.Falling, terrain, Samus, held, frameCounter));

    /// <inheritdoc/>
    public void StepCrouching() => frame.Record(SamusPostureMovement.StepCrouching(
        SamusPoseMetadata.IsXrayCrouchingPose(Samus.Pose) ? SamusMovement.XrayCrouching : SamusMovement.Crouching,
        terrain, Samus, frameCounter));

    /// <inheritdoc/>
    public void StepKnockbackEnding()
    {
        // The knockback movement type's normal movement is this one. Active knockback and Ceres ejection were intercepted
        // before it; this is the knockback movement type's one-frame normal movement, the grounding probe, before the pose
        // transition installs the input/no-input target chosen during the input phase.
        SignedSixteenSixteen groundingDisplacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            Samus.Kinematics, Samus.HorizontalSpeed);
        BlockMoveResult grounding = SamusGroundedMovement.RunNoSpeedCalculationGroundingProbe(terrain, Samus, frameCounter);
        frame.Record(new GroundedMovementResult(default, grounding));
        // When the knockback ending's downward probe hits the floor, Samus lands as on any landing.
        if (grounding.Collided && !groundingDisplacement.IsNegative)
            frame.Record(new AerialMovementResult(Landed: true, HitCeiling: false));
    }

    /// <inheritdoc/>
    public void StepPostureTransition()
    {
        // The morph transitions' own step, the Draygon-held stand-up's hold, otherwise the crouch and stand transitions'.
        if (SamusPoseMetadata.IsMorphTransitionPose(Samus.Pose))
            frame.Record(SamusMorphBallMovement.StepTransition(terrain, Samus, frameCounter));
        else if (SamusPoseTraits.IsIn(Samus.Pose, SamusPoseFamily.HeldInPlaceStandUp))
            HoldWhereInstalled();
        else
            frame.Record(SamusPostureMovement.StepCrouchStandTransition(terrain, Samus, frameCounter));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The glitch ball's normal movement, which only clears the vertical solid-collision result, as does the posture
    /// transition's for the compact Draygon-held stand-up, which skips its horizontal and vertical movement; nothing lands
    /// them, so the landed cleanup never runs. Both hold Samus where the over-read installed them (#1535).
    /// </remarks>
    public void HoldWhereInstalled() => Samus.ForgetVerticalCollisionResult();

    /// <inheritdoc/>
    public void StepDraygonHeld() => SamusDraygonGrabbedState.StepMovement(Samus);

    /// <inheritdoc/>
    public void HoldStill()
    {
    }

    /// <summary>The turn on the ground, by the turning movement type that turns it there.</summary>
    private void StepTurningOnGround(SamusMovement.GroundTurnMovement movement) =>
        frame.Record(SamusGroundedMovement.StepTurningOnGround(movement, terrain, Samus, frameCounter));

    /// <summary>The turn in the air, by the turning movement type that turns it there.</summary>
    private void StepTurningInAir(SamusMovement.AerialTurnMovement movement) =>
        frame.Record(SamusAerialMovement.StepTurningInAir(movement, terrain, Samus, frameCounter));
}
