using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Literal movement for ordinary and Spring-Ball morph poses.
/// </summary>
/// <remarks>
/// The game does not treat the ball as a generic circular rigid body. Grounded,
/// falling, bouncing, and transition poses enter different wrappers which share only a few
/// steps. This class keeps those entry points separate while reusing their exact
/// 16.16 collision operations and their air/water/lava tables. Solid enemies stop the ball through the shared block
/// collision, conveyors carry it as inside-block reactions, and bomb-jump displacement lives
/// in <see cref="SamusBombJumpMovement"/>.
/// </remarks>
public static class SamusMorphBallMovement
{
    /// <summary>Executes movement type four.</summary>
    internal static MorphBallMovementResult StepGrounded(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        Validate(level, samus);
        bool ordinary = SamusPoseMetadata.IsGroundedMorphBallPose(samus.Pose);
        bool spring = SamusPoseMetadata.IsGroundedSpringBallPose(samus.Pose);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if ((!ordinary || movementType != SamusMovementType.MorphBallGround) &&
            (!spring || movementType != SamusMovementType.SpringBallGround))
        {
            throw new InvalidOperationException(
                $"Grounded ball movement requires a grounded morph-ball or Spring Ball pose, not {samus.Pose}/type {movementType}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));

        bool stationaryPose = samus.Pose is
            SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
            SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && stationaryPose)
        {
            // The rolling handler passes an explicit zero base magnitude through the ordinary
            // direction-aware displacement path. With no external displacement this is a
            // zero-pixel block scan, but it still publishes total X speed just like the game.
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else
        {
            // The rolling morph-ball poses, plus either stable pose carrying reversal mode one, enter
            // the full deceleration-allowed horizontal movement. Its run-momentum
            // call retains established run momentum and republishes boost contact damage.
            speed.HandleExtraRunSpeed(
                movementType, controllerInput: 0,
                speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
                liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) != SamusLiquidMedium.Air);
            UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        BlockMoveResult vertical;
        bool landed = false;
        bool hitCeiling;
        if (samus.Kinematics.YDirection != SamusVerticalDirection.None)
        {
            // The simple vertical movement handles a rebound already in progress through the
            // same falling check and one-frame-lag gravity step as airborne morph.
            vertical = MoveVerticallyWithGravity(

                level,
                samus,
                frameCounter,
                roomObjects,
                out landed,
                out hitCeiling);
        }
        else
        {
            vertical = RunNoSpeedCalculationGroundingProbe(

                level,
                samus,
                frameCounter,
                roomObjects,
                out hitCeiling);

            // Only the stable-pose branch performs the speed cleanup, and only
            // after the no-speed Y path. A moving ball retains its newly calculated speed.
            if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && stationaryPose)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>Executes movement type eight.</summary>
    internal static MorphBallMovementResult StepFalling(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        Validate(level, samus);
        bool ordinary = SamusPoseMetadata.IsAirborneMorphBallPose(samus.Pose);
        bool spring = samus.Pose is SamusPoseId.SpringBallFallingRightPose or
            SamusPoseId.SpringBallFallingLeftPose;
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if ((!ordinary || movementType != SamusMovementType.MorphBallFalling) &&
            (!spring || movementType != SamusMovementType.SpringBallFalling))
        {
            throw new InvalidOperationException(
                $"Falling ball movement requires a falling morph-ball or Spring Ball pose, not {samus.Pose}/type {movementType}.");
        }

        bool directionHeld = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != ControllerButton.None;
        if (!directionHeld && samus.HorizontalSpeed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            // The outer falling wrapper clears persistent motion before dispatching to
            // either the falling or bouncing step. The inner step still calculates
            // once, then erases base speed and moves horizontally by zero.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        return StepMorphedFallingOrBouncing(level, samus, controllerInput, frameCounter, roomObjects, movementType);
    }

    /// <summary>
    /// Executes the morph ball's fall or, while
    /// <see cref="SamusState.MorphBallBounceState"/> is nonzero,
    /// its bounce. Morph-ball falling
    /// and both Spring Ball air handlers dispatch here.
    /// </summary>
    private static MorphBallMovementResult StepMorphedFallingOrBouncing(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects,
        SamusMovementType movementType)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));

        bool directionHeld = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != ControllerButton.None;
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(movementType);
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

        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical;
        bool landed;
        bool hitCeiling;
        if (samus.MorphBallBounceState != SamusBallBounceState.None &&
            samus.KnockbackDirection == Movement.SamusKnockbackDirection.None &&
            samus.Kinematics.ExtraDisplacement.Y != SignedSixteenSixteen.Zero)
        {
            // The bounce gives an external producer priority over the rebound arc.
            // It cancels the stored bounce velocity, forces direction down, and executes
            // the downward move; a signed-negative producer can still make the actual scan upward.
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
            samus.Kinematics.YSpeed = default;
            SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(samus.Kinematics) ??
                throw new InvalidOperationException("A nonzero external Y displacement always yields a scan distance.");
            vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
            landed = !displacement.IsNegative && vertical.Collided;
            hitCeiling = displacement.IsNegative && vertical.Collided;
        }
        else
        {
            // With zero knockback and zero extra Y displacement, the bounce and the fall
            // rejoin at the same falling-check/gravity step. Bounce state changes only
            // when the later solid-vertical handler sees a downward collision.
            vertical = MoveVerticallyWithGravity(

                level,
                samus,
                frameCounter,
                roomObjects,
                out landed,
                out hitCeiling);
        }
        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>Executes <see cref="SamusMovementType.SpringBallInAir"/> movement.</summary>
    internal static MorphBallMovementResult StepSpringBallInAir(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        Validate(level, samus);
        if (samus.Pose is not (SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose) ||
            SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpringBallInAir)
        {
            throw new InvalidOperationException(
                $"Spring-Ball powered jump requires a rising Spring Ball jump pose, not {samus.Pose}.");
        }

        // A nonzero bounce state selects the bouncing step instead of the
        // powered jump, so a rebound's upward speed survives a released Jump button.
        if (samus.MorphBallBounceState != SamusBallBounceState.None)
        {
            return StepMorphedFallingOrBouncing(
level, samus, controllerInput, frameCounter, roomObjects, SamusMovementType.SpringBallInAir);
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));

        // The normal variable-height jump cutoff. Releasing Jump
        // while rising cancels the remaining magnitude; signed underflow at the apex takes
        // the same branch even if Jump is still held.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up &&
            (((controllerInput & ControllerButton.A) == ControllerButton.None) ||
             SamusSpeedMagnitude.HasPassedZero(samus.Kinematics.YSpeed.Whole)))
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(
                movementType: SamusMovementType.SpringBallInAir);
        bool directionHeld = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != ControllerButton.None;
        SignedSixteenSixteen requestedHorizontal;
        // Null when the game skips horizontal movement entirely: no probe runs.
        SamusCollisionDirection? collisionDirection = null;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !directionHeld)
        {
            // With no horizontal direction held the game clears the displacement and persistent base speed;
            // powered vertical motion continues unchanged.
            speed.BaseSpeed = default;
            // This branch bypasses the speed calculation; retain the previous total-speed pair.
            requestedHorizontal = SignedSixteenSixteen.Zero;
        }
        else
        {
            var calculated = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
            requestedHorizontal = calculated.Displacement;
            collisionDirection = calculated.CollisionDirection;
        }

        // The game skips horizontal movement entirely, including its slope alignment.
        BlockMoveResult horizontal = collisionDirection is null
            ? BlockMoveResult.NotMoved
            : SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal, CollisionMovementDirection = collisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = MoveVerticallyWithGravity(

            level,
            samus,
            frameCounter,
            roomObjects,
            out bool landed,
            out bool hitCeiling);
        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>
    /// Executes the morphing and unmorphing subset of <see cref="SamusMovementType.PostureTransition"/> in
    /// the transition movement handler, including its airborne gravity branch.
    /// </summary>
    internal static MorphBallMovementResult StepTransition(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        Validate(level, samus);
        if (!SamusPoseMetadata.IsMorphTransitionPose(samus.Pose) ||
            SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.PostureTransition)
        {
            throw new InvalidOperationException(
                $"Morph transition movement requires a morphing or unmorphing transition pose, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // The transition handler deliberately supplies zero base speed but still uses pose direction,
        // collision, and the total speed already worked out this update.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical;
        bool landed = false;
        if (samus.Kinematics.YDirection == SamusVerticalDirection.None)
        {
            vertical = RunNoSpeedCalculationGroundingProbe(

                level,
                samus,
                frameCounter,
                roomObjects,
                out _);
        }
        else
        {
            vertical = MoveVerticallyWithGravity(

                level,
                samus,
                frameCounter,
                roomObjects,
                out landed,
                out _);
            if (landed)
            {
                // The transition handler recognizes the ordinary landed result and cancels every value
                // which could make the item-and-airborne transition cue choose the airborne endpoint afterward.
                samus.Kinematics.YSpeed = default;
                samus.Kinematics.YDirection = SamusVerticalDirection.None;
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

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects,
        out bool landed,
        out bool hitCeiling)
    {
        SamusKinematicsState state = samus.Kinematics;

        // The fall converts the signed underflow produced at the prior apex frame before
        // this frame snapshots displacement. Direction is a separate value;
        // do not infer it from the sign of the fixed-point magnitude.
        if (state.YDirection == SamusVerticalDirection.Up && SamusSpeedMagnitude.HasPassedZero(state.YSpeed.Whole))
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }

        // The bounce moves by the OLD magnitude, then updates stored speed for next frame.
        // This one-frame lag is observable on both bounce launch and the falling speed cap.
        UnsignedSixteenSixteen oldSpeed = state.YSpeed;
        if (state.YDirection == SamusVerticalDirection.Down)
        {
            if (state.YSpeed.Whole != 5)
                state.YSpeed = oldSpeed + state.YAcceleration;
        }
        else
        {
            state.YSpeed = oldSpeed - state.YAcceleration;
        }

        SignedSixteenSixteen displacement = state.YDirection == SamusVerticalDirection.Down
            ? oldSpeed.AsDisplacement()
            : -oldSpeed.AsDisplacement();
        displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            displacement);
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

        hitCeiling = displacement.IsNegative && vertical.Collided;
        landed = !displacement.IsNegative && vertical.Collided;
        // Movement publishes collision only. Samus's transitions stop vertical speed if
        // the ceiling transition wins AFTER animation/hurt/bomb interruption.
        // Clearing here would erase speed even when a new bomb launch wins.
        return vertical;
    }

    /// <summary>Scans for ground or ceiling contact using external displacement while leaving vertical speed unchanged.</summary>
    private static BlockMoveResult RunNoSpeedCalculationGroundingProbe(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects,
        out bool hitCeiling)
    {
        SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);
        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
        // No-speed Y movement can still go UP when a platform carries Samus. The
        // game publishes the same deferred ceiling hit as a velocity-driven
        // jump. Its speed/direction writes belong to the later winning transition.
        hitCeiling = displacement.IsNegative && result.Collided;
        return result;
    }

    /// <summary>Rejects missing game, level, or Samus state before entering a morph movement path.</summary>
    private static void Validate(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }
}
