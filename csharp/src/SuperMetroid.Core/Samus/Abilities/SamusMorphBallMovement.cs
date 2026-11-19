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
/// falling, bouncing, and transition poses enter different movements which share only a few
/// steps. This class keeps those movements separate while reusing their exact
/// collision moves and their air, water and lava speeds. Solid enemies stop the ball through the shared block
/// collision, conveyors carry it as inside-block reactions, and bomb-jump displacement lives
/// in <see cref="SamusBombJumpMovement"/>.
/// </remarks>
public static class SamusMorphBallMovement
{
    /// <summary>Executes <see cref="SamusMovementType.MorphBallGround"/> movement.</summary>
    internal static MorphBallMovementResult StepGrounded(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
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
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));

        bool stationaryPose = samus.Pose is
            SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
            SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && stationaryPose)
        {
            // The rolling movement passes an explicit zero base magnitude through the ordinary
            // direction-aware displacement path. With no external displacement this is a
            // zero-pixel block scan, but it still publishes total X speed just like the game.
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
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
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        BlockMoveResult vertical;
        bool landed = false;
        bool hitCeiling;
        if (!samus.Kinematics.VerticalVelocity.HasNoDirection)
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

            // Only a ball in a stable pose has its momentum cleared, and only after the vertical move with no speed. A
            // moving ball keeps its newly calculated speed.
            if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && stationaryPose)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        return new MorphBallMovementResult(vertical, landed, hitCeiling);
    }

    /// <summary>Executes <see cref="SamusMovementType.MorphBallFalling"/> movement.</summary>
    internal static MorphBallMovementResult StepFalling(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
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
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        if (!directionHeld && samus.HorizontalSpeed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            // The falling movement first clears persistent motion before handing over to
            // either the falling or bouncing step. The inner step still calculates
            // once, then erases base speed and moves horizontally by zero.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        return StepMorphedFallingOrBouncing(level, samus, controllerInput, frameCounter, roomObjects, movementType);
    }

    /// <summary>
    /// Executes the morph ball's fall or, while
    /// the ball is bouncing (<see cref="SamusState.MorphBallBounceState"/>),
    /// its bounce. Morph-ball falling
    /// and both Spring Ball air movements run here.
    /// </summary>
    private static MorphBallMovementResult StepMorphedFallingOrBouncing(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects,
        SamusMovementType movementType)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));

        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
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

        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
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
            // the downward move; an upward external displacement can still make the actual scan upward.
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
            SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(samus.Kinematics) ??
                throw new InvalidOperationException("A nonzero external Y displacement always yields a scan distance.");
            vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
            landed = !displacement.IsNegative && vertical.Collided;
            hitCeiling = displacement.IsNegative && vertical.Collided;
        }
        else
        {
            // With zero knockback and zero extra Y displacement, the bounce and the fall
            // rejoin at the same falling-check/gravity step. Bounce state changes only
            // when the later solid vertical collision sees a downward collision.
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
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Validate(level, samus);
        if (samus.Pose is not (SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose) ||
            SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpringBallInAir)
        {
            throw new InvalidOperationException(
                $"Spring-Ball powered jump requires a rising Spring Ball jump pose, not {samus.Pose}.");
        }

        // A ball that is bouncing takes the bouncing step instead of the
        // powered jump, so a rebound's upward speed survives a released Jump button.
        if (samus.MorphBallBounceState != SamusBallBounceState.None)
        {
            return StepMorphedFallingOrBouncing(
                level, samus, controllerInput, frameCounter, roomObjects, SamusMovementType.SpringBallInAir);
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
            speed.CalculateBaseSpeedDecelerationDisallowed(
                movementType: SamusMovementType.SpringBallInAir);
        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        SignedSixteenSixteen requestedHorizontal;
        // Null when the game skips horizontal movement entirely: no probe runs.
        SamusCollisionDirection? collisionDirection = null;
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
        BlockMoveResult horizontal = collisionDirection is null
            ? BlockMoveResult.NotMoved
            : SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal, CollisionMovementDirection = collisionDirection });
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
    /// the transition movement, including its airborne gravity branch.
    /// </summary>
    internal static MorphBallMovementResult StepTransition(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Validate(level, samus);
        if (!SamusPoseMetadata.IsMorphTransitionPose(samus.Pose) ||
            SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.PostureTransition)
        {
            throw new InvalidOperationException(
                $"Morph transition movement requires a morphing or unmorphing transition pose, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // The transition movement deliberately supplies zero base speed but still uses pose direction,
        // collision, and the total speed already worked out this update.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical;
        bool landed = false;
        if (samus.Kinematics.VerticalVelocity.HasNoDirection)
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
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects,
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
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

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
        ISamusRoomObjects roomObjects,
        out bool hitCeiling)
    {
        SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);
        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
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
