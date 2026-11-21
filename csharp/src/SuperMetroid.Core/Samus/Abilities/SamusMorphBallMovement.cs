using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// How the Morph Ball and Spring Ball move: grounded, falling, bouncing and in transition.
/// </summary>
/// <remarks>
/// Each of those movements is its own, sharing a few steps: the collision moves and the air, water and lava
/// speeds. Solid enemies stop the ball through the shared block collision, conveyors carry it as inside-block
/// reactions, and bomb-jump displacement lives in <see cref="SamusBombJumpMovement"/>.
/// </remarks>
public static class SamusMorphBallMovement
{
    /// <summary>
    /// Executes <see cref="SamusMovementType.MorphBallGround"/> or <see cref="SamusMovementType.SpringBallGround"/>
    /// movement, the one <paramref name="movement"/> is.
    /// </summary>
    /// <param name="movement">The grounded ball's movement her pose selected, whose movement type it reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="samus">Samus, a grounded ball.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static MorphBallMovementResult StepGrounded(
        SamusMovement.GroundedBallMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(movement);
        Validate(terrain.Level, samus);
        SamusMovementType movementType = movement.Type;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));

        bool stationaryPose = SamusPoseTraits.IsIn(samus.Pose, SamusPoseFamily.StationaryBall);
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && stationaryPose)
        {
            // A resting ball moves only by outside displacement and stops on a wall; it still publishes its total X
            // speed.
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
            _ = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requested);
        }
        else
        {
            // The rolling morph-ball poses, plus either stable pose carrying the turning acceleration mode, enter
            // the full deceleration-allowed horizontal movement. That movement keeps established run momentum and
            // publishes boost contact damage again.
            speed.HandleExtraRunSpeed(
                movementType, controllerInput: GameplayAction.None,
                speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
                liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) != SamusLiquidMedium.Air);
            UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
            _ = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requested);
        }

        BlockMoveResult vertical;
        bool landed = false;
        bool hitCeiling;
        if (!samus.Kinematics.VerticalVelocity.HasNoDirection)
        {
            // The simple vertical movement handles a rebound already in progress through the
            // same falling check and one-frame-lag gravity step as airborne morph.
            vertical = MoveVerticallyWithGravity(
                terrain,
                samus,
                frameCounter,
                out landed,
                out hitCeiling);
        }
        else
        {
            vertical = RunNoSpeedCalculationGroundingProbe(
                terrain,
                samus,
                frameCounter,
                out hitCeiling);

            // Only a ball in a stable pose has its momentum cleared, and only after the vertical move with no speed. A
            // moving ball keeps its newly calculated speed.
            if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && stationaryPose)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>
    /// Executes <see cref="SamusMovementType.MorphBallFalling"/> or <see cref="SamusMovementType.SpringBallFalling"/>
    /// movement, the one <paramref name="movement"/> is.
    /// </summary>
    /// <param name="movement">The falling ball's movement her pose selected, whose movement type it reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="samus">Samus, a falling ball.</param>
    /// <param name="controllerInput">The update's held input.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static MorphBallMovementResult StepFalling(
        SamusMovement.FallingBallMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        Ensure.NotNull(movement);
        Validate(terrain.Level, samus);
        SamusMovementType movementType = movement.Type;
        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        if (!directionHeld && samus.HorizontalSpeed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            // The falling movement first clears persistent motion before handing over to
            // either the falling or bouncing step. The inner step still calculates
            // once, then erases base speed and moves horizontally by zero.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        return StepMorphedFallingOrBouncing(terrain, samus, controllerInput, frameCounter, movementType);
    }

    /// <summary>
    /// Executes the morph ball's fall or, while
    /// the ball is bouncing (<see cref="SamusState.MorphBallBounceState"/>),
    /// its bounce. Morph-ball falling
    /// and both Spring Ball air movements run here.
    /// </summary>
    private static MorphBallMovementResult StepMorphedFallingOrBouncing(
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        SamusMovementType movementType)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));

        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        AerialBaseSpeedResult calculation =
            speed.CalculateAerialBaseSpeed(movementType);
        SamusHorizontalDisplacement requestedHorizontal;
        if (!directionHeld && speed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            speed.BaseSpeed = default;
            requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
        }
        else
        {
            requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        }

        _ = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        BlockMoveResult vertical;
        bool landed;
        bool hitCeiling;
        if (samus.MorphBallBounceState != SamusBallBounceState.None &&
            samus.Knockback is Movement.SamusKnockback.NoKnockback &&
            samus.Kinematics.ExtraDisplacement.Y != SignedSixteenSixteen.Zero)
        {
            // The bounce gives an external producer priority over the rebound arc.
            // It cancels the stored bounce velocity, forces direction down, and executes
            // the downward move; an upward external displacement can still make the actual scan upward.
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
            SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(samus.Kinematics) ??
                throw new InvalidOperationException("A nonzero external Y displacement always yields a scan distance.");
            vertical = SamusTerrainMoves.MoveVertically(terrain, samus, displacement, frameCounter);
            landed = !displacement.IsNegative && vertical.Collided;
            hitCeiling = displacement.IsNegative && vertical.Collided;
        }
        else
        {
            // With zero knockback and zero extra Y displacement, the bounce and the fall
            // rejoin at the same falling-check/gravity step. Bounce state changes only
            // when the later solid vertical collision sees a downward collision.
            vertical = MoveVerticallyWithGravity(
                terrain,
                samus,
                frameCounter,
                out landed,
                out hitCeiling);
        }
        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>Executes <see cref="SamusMovementType.SpringBallInAir"/> movement, which only the rising Spring Ball jump poses move by.</summary>
    /// <param name="movement">The Spring Ball jump's movement her pose selected, whose movement type it reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="samus">Samus, a rising Spring Ball.</param>
    /// <param name="controllerInput">The update's held input.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static MorphBallMovementResult StepSpringBallInAir(
        SamusMovement.SpringBallInAirMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter)
    {
        Ensure.NotNull(movement);
        Validate(terrain.Level, samus);

        // A ball that is bouncing takes the bouncing step instead of the
        // powered jump, so a rebound's upward speed survives a released Jump button.
        if (samus.MorphBallBounceState != SamusBallBounceState.None)
        {
            return StepMorphedFallingOrBouncing(
                terrain, samus, controllerInput, frameCounter, movement.Type);
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));

        // The normal variable-height jump cutoff. Releasing Jump
        // while rising cancels the remaining magnitude; a speed passing zero at the apex takes
        // the same branch even if Jump is still held.
        if (samus.Kinematics.VerticalVelocity.IsRising &&
            (((controllerInput & GameplayAction.Jump) == GameplayAction.None) ||
             samus.Kinematics.VerticalVelocity.HasPassedApex))
        {
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }

        AerialBaseSpeedResult calculation =
            speed.CalculateAerialBaseSpeed(
                movementType: movement.Type);
        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        SignedSixteenSixteen requestedHorizontal;
        // Null when the game skips horizontal movement entirely: no probe runs.
        HorizontalDirection? collisionDirection = null;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !directionHeld)
        {
            // With no horizontal direction held the game clears the displacement and persistent base speed;
            // powered vertical motion continues unchanged.
            speed.BaseSpeed = default;
            // With no direction held the speed is not calculated, and the previous total speeds stay.
            requestedHorizontal = SignedSixteenSixteen.Zero;
        }
        else
        {
            var calculated = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
            requestedHorizontal = calculated.Displacement;
            collisionDirection = calculated.CollisionDirection;
        }

        // The game skips horizontal movement entirely, including its slope alignment.
        _ = collisionDirection is { } direction
            ? SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, new SamusHorizontalDisplacement(requestedHorizontal, direction))
            : BlockMoveResult.NotMoved;

        BlockMoveResult vertical = MoveVerticallyWithGravity(
            terrain,
            samus,
            frameCounter,
            out bool landed,
            out bool hitCeiling);
        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>
    /// Executes the morphing and unmorphing subset of <see cref="SamusMovementType.PostureTransition"/> in
    /// the transition movement, including its airborne gravity branch. It reads only the way the pose faces.
    /// </summary>
    internal static MorphBallMovementResult StepTransition(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Validate(terrain.Level, samus);

        // The transition movement deliberately supplies zero base speed but still uses pose direction,
        // collision, and the total speed already worked out this update.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
        _ = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        BlockMoveResult vertical;
        bool landed = false;
        if (samus.Kinematics.VerticalVelocity.HasNoDirection)
        {
            vertical = RunNoSpeedCalculationGroundingProbe(
                terrain,
                samus,
                frameCounter,
                out _);
        }
        else
        {
            vertical = MoveVerticallyWithGravity(
                terrain,
                samus,
                frameCounter,
                out landed,
                out _);
            if (landed)
            {
                // The transition movement recognizes the ordinary landed result and cancels every value
                // which could make the item-and-airborne transition cue choose the airborne endpoint afterward.
                samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
                samus.MorphBallBounceState = SamusBallBounceState.None;
            }
        }

        // It then clears the solid-vertical collision result, so the pose transition
        // never selects the ceiling hit for a transition pose: a blocked rise
        // keeps its upward speed, which the next frame's speed calculation keeps decaying.
        return new MorphBallMovementResult(vertical, landed, HitCeiling: false);
    }

    /// <summary>Moves the ball by its previous vertical speed, then advances gravity and reports collisions for the later transition.</summary>
    private static BlockMoveResult MoveVerticallyWithGravity(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter,
        out bool landed,
        out bool hitCeiling)
    {
        SamusKinematicsState state = samus.Kinematics;

        // The fall converts the rise that passed its apex on the prior frame before this frame snapshots displacement.
        if (state.VerticalVelocity.HasPassedApex)
        {
            state.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }

        // The bounce moves by the OLD velocity, then gravity updates it for next frame, a fall holding at the bounce's
        // falling speed. This one-frame lag is observable on both bounce launch and the falling speed cap.
        SignedSixteenSixteen displacement = state.VerticalVelocity.Displacement;
        state.VerticalVelocity = state.VerticalVelocity.AfterGravity(state.YAcceleration, SamusVerticalMotionDefinitions.BallBounceFallingSpeed);
        displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            displacement);
        BlockMoveResult vertical = SamusTerrainMoves.MoveVertically(terrain, samus, displacement, frameCounter);

        hitCeiling = displacement.IsNegative && vertical.Collided;
        landed = !displacement.IsNegative && vertical.Collided;
        // Movement publishes collision only. Samus's transitions stop vertical speed if
        // the ceiling transition wins AFTER animation/hurt/bomb interruption.
        // Clearing here would erase speed even when a new bomb launch wins.
        return vertical;
    }

    /// <summary>Scans for ground or ceiling contact using external displacement while leaving vertical speed unchanged.</summary>
    private static BlockMoveResult RunNoSpeedCalculationGroundingProbe(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter,
        out bool hitCeiling)
    {
        SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);
        BlockMoveResult result = SamusTerrainMoves.MoveVertically(terrain, samus, displacement, frameCounter);
        // No-speed Y movement can still go UP when a platform carries Samus. The
        // game publishes the same deferred ceiling hit as a velocity-driven
        // jump. Its speed/direction writes belong to the later winning transition.
        hitCeiling = displacement.IsNegative && result.Collided;
        return result;
    }

    /// <summary>Rejects a missing level or Samus state before entering a morph movement path.</summary>
    private static void Validate(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }
}
