using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's grounded movement types from her ordinary movement step.
/// </summary>
/// <remarks>
/// This is deliberately not a generic platformer controller. The entry points below
/// correspond only to the standing, running, turning-on-ground, moonwalking and ran-into-wall movement types, with
/// their air/water/lava X tables, submerged Dash gating, and ordinary Dash and equipped-Speed-Booster accumulation and
/// staging. Solid enemies stop Samus through the shared block collision, conveyors carry her as inside-block reactions,
/// knockback is <see cref="SamusKnockbackMovement"/>'s, and the boost's colours and echoes are drawn outside movement.
/// </remarks>
public static class SamusGroundedMovement
{
    /// <summary>
    /// Runs front-view movement, including the active-elevator
    /// one-pixel downward block scan.
    /// </summary>
    /// <remarks>
    /// <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/> and <see cref="SamusPoseId.ForwardFacingSuitedPose"/> share movement type zero with ordinary standing, but the game
    /// returns before horizontal movement, speed cleanup, and the one-pixel grounding probe.
    /// It only clears <see cref="SamusState.SolidVerticalCollisionResult"/> while elevator status is zero.
    /// A nonzero status calls the downward block collision with exactly one pixel of downward displacement before
    /// clearing that result. The elevator actor remains a separate producer of the status,
    /// and of the platform art; this method holds the complete Samus-side consumer.
    /// </remarks>
    [IndependentFlag("Elevator motion and door side-effect publication are independent states of the frame.")]
    internal static BlockMoveResult? StepFacingForward(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        bool elevatorIsMoving = false,
        ISamusRoomObjects? roomObjects = null,
        bool publishDoorSideEffects = true)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsForwardFacingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Forward-facing movement requires a forward-facing pose, not {samus.Pose}.");
        }

        BlockMoveResult? vertical = null;
        if (elevatorIsMoving)
        {
            // Publishes collision direction two and calls the
            // downward mover with one whole pixel. That entry deliberately skips
            // solid-enemy collision but retains ordinary room-block and behaviour scanning and the
            // alternating left/right order selected from the frame counter's parity.
            vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = SignedSixteenSixteen.FromParts(1, 0), ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false, PublishDoorSideEffects = publishDoorSideEffects });
        }

        // Is the only write in the ordinary no-elevator path. In particular,
        // stale base/extra X speed is retained; facing forward clears those speeds at
        // setup time, not every frame in this movement step.
        samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.None;
        return vertical;
    }

    /// <summary>
    /// Runs the movement-relevant portion of standing movement for either facing:
    /// the right-facing standing family or the left-facing one, unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepStanding(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ControllerButton controllerInput,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose);
        if (!facingLeft && !SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Standing movement requires a standing pose, not {samus.Pose}.");
        }

        ResetStandingShotAnimation(samus, controllerInput);
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Moves Samus with no base speed, which still passes zero through
        // horizontal movement and the pose direction's displacement calculator. That
        // calculation publishes total-X speed before collision; do not skip it merely because
        // a freshly spawned standing Samus normally has zero base/extra speed. A left-facing
        // pose's direction is left, so the helper is the LEFT form even though its
        // base argument is zero, which matters once extra displacement contributes.
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft, samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);

        // Cancels speed boost and clears extra speed, base speed, and
        // acceleration mode after both movement calls, whichever way Samus faces.
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>Restarts neutral standing's shot pose on held Shoot even when no projectile was admitted; angled poses keep their own animation.</summary>
    private static void ResetStandingShotAnimation(SamusState samus, ControllerButton controllerInput)
    {
        if (samus.Pose is SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose &&
            (controllerInput & ControllerButton.X) != ControllerButton.None)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 0, SamusMovementDefinitions.StandingShotAnimationTimer);
    }

    /// <summary>
    /// Runs running movement in dry air, including ordinary Dash, for either
    /// facing: the right-moving running family or the left-moving one, unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepRunning(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ControllerButton controllerInput = ControllerButton.None,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose);
        if (!facingLeft && !SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Running movement requires a pose of the running families, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Can replace the block reaction's ordinary speed table with the
        // complete water/lava table after comparing Samus's bottom boundary.
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // Handles Dash before calculating base speed. With no equipped Speed
        // Booster, B establishes momentum, adds exactly a sixteenth of a pixel per frame, and caps the
        // extra component at two pixels. The flag retains that component after B is released.
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // The horizontal speed calculation advances the split 1/65536-pixel base speed. The following
        // direction selector is shared by every horizontal movement family:
        // modes zero and two obey the pose's facing, while every other nonzero
        // value reverses it. Mode one is therefore not an invalid running state; it is the
        // one-frame-old momentum that can survive a pose transition into this body. Do not
        // special-case it, even if another input-side transition has already changed the
        // visible running pose.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType: SamusMovementType.Running);
        bool movesLeft = (speed.AccelerationMode is SamusAccelerationMode.Accelerating or SamusAccelerationMode.Decelerating) == facingLeft;
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(movesLeft, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });

        // Clears X speed on a collision immediately after block collision. Total
        // speed deliberately remains published: the following grounding probe reads it
        // even if base speed was just cleared by a wall.
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs the grounded branches of turning around on the ground
    /// and of turning around while jumping.
    /// The latter is misleading for the aimed crouching turns: crouched entry leaves Y direction
    /// zero, so the game runs its no-speed grounding probe rather than airborne gravity.
    /// </summary>
    internal static GroundedMovementResult StepTurningOnGround(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        bool turnsLeft = SamusPoseMetadata.IsRightToLeftGroundTurnPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpLeftPose(samus.Pose);
        bool turnsRight = SamusPoseMetadata.IsLeftToRightGroundTurnPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpRightPose(samus.Pose);
        if (!turnsLeft && !turnsRight)
        {
            throw new InvalidOperationException(
                $"Grounded-turn movement requires a verified standing, crouched, or moonwalk turn pose, not {samus.Pose}.");
        }

        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if (movementType is not (SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping))
            throw new InvalidOperationException(
                $"Grounded turn pose {samus.Pose} has movement type {movementType}.");
        if (movementType == SamusMovementType.TurningWhileJumping && !SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose))
            throw new InvalidOperationException($"Grounded turning-while-jumping admission requires an aimed crouched turn, not {samus.Pose}.");
        if (movementType == SamusMovementType.TurningWhileJumping && samus.Kinematics.YDirection != SamusVerticalDirection.None)
        {
            throw new InvalidOperationException(
                $"Airborne turning-while-jumping pose {samus.Pose} must execute through SamusAerialMovement.StepTurningInAir.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // The turn runs the complete X movement before
        // cancelling boost. Because this movement type is not running, the Dash acceleration cannot
        // add speed; it either clears an unowned pair or preserves an already established
        // momentum pair for this final movement frame. The unconditional cancellation and
        // zero writes below then retire it in the game's original order.
        speed.HandleExtraRunSpeed(
            movementType,
            controllerInput: 0,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // The pose's own turning-on-ground or turning-while-jumping speed-table record supplies the
        // deceleration. If subtraction crosses below zero,
        // the calculation clears both speed halves AND mode before the direction selector chooses direction.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);

        // Reads the pose's facing rather than the English-facing name. Keeping
        // the pose's facing also preserves the game's behavior for unexpected
        // nonzero acceleration-mode values instead of manufacturing another unsupported arm.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);

        // Cancels the speed boost on every turn frame. This matters even
        // without the Speed Booster item: the turn initializer folded the numeric extra component but
        // deliberately left the running-momentum flag set until this movement handler. Then the handler
        // cancels that momentum and zeroes the extra run speed, each separately observable.
        speed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        speed.ExtraRunSpeed = default;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs moonwalking movement for the stable moonwalk poses, unaimed or aimed,
    /// including the complete shared extra-run-speed prepass.
    /// </summary>
    internal static GroundedMovementResult StepMoonwalking(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsMoonwalkingPose(samus.Pose) ||
            SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Moonwalking)
        {
            throw new InvalidOperationException(
                $"Moonwalking movement requires a stable moonwalk pose, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // Runs horizontal movement, whose first operation is the Dash acceleration.
        // Moonwalking can never enter that step's Dash-acceleration arm, but an
        // existing momentum flag deliberately carries the extra pair through moonwalking;
        // without the flag the pair is cleared before displacement is calculated.
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Moonwalking,
            controllerInput: 0,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // Moonwalking has its own speed record. Its pose X directions intentionally
        // produce travel opposite the visible facing; mode one reverses that direction just as it
        // does for every other family. Do not derive either direction from the pose's name.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType: SamusMovementType.Moonwalking);
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // Is the same no-speed-calculation grounding probe used by running.
        // It deliberately scales the downward probe by the total horizontal magnitude.
        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs ran-into-wall movement for the ran-into-wall poses, unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepRanIntoWall(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsRanIntoWallPose(samus.Pose) ||
            SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.RanIntoWall)
        {
            throw new InvalidOperationException(
                $"Ran-into-wall movement requires a ran-into-wall pose, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Calls the same no-base-speed X movement as standing. Direction still
        // comes from the literal pose definition: the right-facing poses store eight (right), while
        // the left-facing poses store four (left). Extra speed is included in the requested move
        // before the handler's unconditional cleanup, matching the call order.
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left), samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // The one-or-more-pixel downward grounding probe runs before all X values are
        // cleared. This preserves the same ledge behavior as ordinary wall stops even though the
        // ordinary wall-stop state normally begins with zero speed.
        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);

        // Cancels speed boost and clears both run/base components plus
        // acceleration mode on every frame.
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs standing movement for every modelled landing pose: the ordinary
    /// and spin landings, plus the aimed and horizontally firing landings.
    /// Landing animation is cosmetic with respect to movement: every member uses standing's
    /// zero-base-speed horizontal pass, grounding probe, and momentum cleanup until the animation
    /// cue's pose transition returns to <see cref="SamusPoseId.FacingRightNormalPose"/> and <see cref="SamusPoseId.FacingLeftNormalPose"/>.
    /// </summary>
    internal static GroundedMovementResult StepLanding(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        // Do not duplicate the pose list here. The predicates are the single audited definition
        // of the standing movement type's landing family and deliberately include the two firing
        // landings. Keeping dispatch and this defensive guard on the same predicates prevents a
        // newly modelled landing from reaching this real movement handler only to be rejected.
        if (!SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) &&
            !SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Landing movement requires a landing pose, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft, samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        // Landing art still uses standing movement: it does not reset Y values.
        // Settling the accepted landing owns its cleanup; a later hurt expiry may write
        // direction two while this pose remains active, and that value must survive.
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Executes the knockback movement type's ordinary handler after a
    /// special owner (normal damage knockback or Ceres Ridley's ejection) has restored the
    /// normal step while leaving a knockback pose current. The game clears the
    /// vertical-collision result and performs only the shared no-speed Y probe; it does not
    /// invent another knockback arc or silently reinterpret the hurt body as standing.
    /// </summary>
    internal static BlockMoveResult StepKnockbackOrCrystalFlashEnding(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Knockback)
        {
            throw new InvalidOperationException(
                $"Knockback-ending movement requires the knockback movement type, not pose {samus.Pose}.");
        }

        return RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            frameCounter,
            roomObjects);
    }

    /// <summary>
    /// Moves Samus vertically without a speed calculation, including the external-Y
    /// replacement path and its asymmetric positive one-pixel bias.
    /// </summary>
    internal static BlockMoveResult RunNoSpeedCalculationGroundingProbe(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects)
    {
        SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);

        // Alternates its left-to-right and right-to-left horizontal scan order on
        // the frame counter's parity. That parity can affect which simultaneous
        // collision side effect wins.
        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }

}
