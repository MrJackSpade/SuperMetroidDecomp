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
/// This is deliberately not a generic platformer controller. The movements below
/// correspond only to the standing, running, turning-on-ground, moonwalking and ran-into-wall movement types, with
/// their speeds in air, water and lava (<see cref="SamusHorizontalSpeedDefinitions"/>), submerged Dash gating, and ordinary Dash and equipped-Speed-Booster accumulation and
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
    /// <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/> and <see cref="SamusPoseId.ForwardFacingSuitedPose"/> share <see cref="SamusMovementType.Standing"/> movement with ordinary standing, but the game
    /// returns before horizontal movement, speed cleanup, and the one-pixel grounding probe.
    /// It clears her solid vertical collision result (<see cref="SamusState"/>) every update; while an elevator moves, it first
    /// moves her one pixel down through the downward block collision. The elevator actor remains a separate producer of the status,
    /// and of the platform art; this method holds the complete Samus-side consumer.
    /// </remarks>
    /// <param name="level">The room's level the elevator carries her through.</param>
    /// <param name="samus">Samus, in a front-facing pose.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    /// <param name="elevatorIsMoving">Whether an elevator carries her this update.</param>
    /// <param name="roomObjects">The room's room objects and door contacts her scan reaches.</param>
    /// <param name="doorBlocks">What the door blocks the elevator carries her through do to her (#2545).</param>
    internal static void StepFacingForward(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        bool elevatorIsMoving,
        ISamusRoomObjects roomObjects,
        DoorBlockMeeting doorBlocks)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsForwardFacingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Forward-facing movement requires a forward-facing pose, not {samus.Pose}.");
        }

        if (elevatorIsMoving)
        {
            // A moving elevator carries Samus down one whole pixel through the downward mover; the collision direction
            // the game sets first is read by nothing before the next move sets its own. That move deliberately skips
            // solid-enemy collision but retains ordinary room-block and behaviour scanning and the
            // alternating left/right order selected from the frame counter's parity.
            _ = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = SamusMovementDefinitions.ElevatorCarry, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false, DoorBlocks = doorBlocks });
        }

        // Clearing the vertical collision result is all front-view movement does without a moving elevator. In
        // particular, stale base/extra X speed is retained; facing forward clears those speeds at
        // setup time, not every frame in this movement step.
        samus.ForgetVerticalCollisionResult();
    }

    /// <summary>
    /// Runs the movement-relevant portion of standing movement for either facing:
    /// the right-facing standing family or the left-facing one, unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepStanding(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        GameplayAction controllerInput,
        ISamusRoomObjects roomObjects)
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

        // Standing moves Samus with no base speed, which still passes zero through
        // horizontal movement and the pose direction's displacement calculation. That
        // calculation publishes total-X speed before collision; do not skip it merely because
        // a freshly spawned standing Samus normally has zero base/extra speed. A left-facing
        // pose's direction is left, so the displacement is leftward even though its
        // base speed is zero, which matters once extra displacement contributes.
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft ? HorizontalDirection.Left : HorizontalDirection.Right, samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            level,
            samus,
            frameCounter,
            roomObjects);

        // Standing then cancels the speed boost and clears extra speed, base speed, and
        // acceleration mode after both moves, whichever way Samus faces.
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>Restarts neutral standing's shot pose on held Shoot even when no projectile was admitted; angled poses keep their own animation.</summary>
    private static void ResetStandingShotAnimation(SamusState samus, GameplayAction controllerInput)
    {
        if (samus.Pose is SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose &&
            (controllerInput & GameplayAction.Shoot) != GameplayAction.None)
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationDelays.AnimationForPose(samus.AnimationPose).Start, SamusMovementDefinitions.StandingShotAnimationTimer);
    }

    /// <summary>
    /// Runs running movement in dry air, including ordinary Dash, for either
    /// facing: the right-moving running family or the left-moving one, unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepRunning(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        GameplayAction controllerInput,
        ISamusRoomObjects roomObjects)
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

        // The liquid Samus's bottom boundary lies in can replace her ordinary speeds with the water or lava
        // speeds.
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // Running handles Dash before calculating base speed. With no Speed Booster equipped, Dash establishes momentum, adds
        // exactly a sixteenth of a pixel per frame, and caps the extra component at two pixels. The flag retains that
        // component after Dash is released.
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // The horizontal speed calculation advances the base speed. The following
        // direction selector is shared by every horizontal movement family:
        // accelerating and decelerating obey the pose's facing, while turning
        // reverses it. Turning is therefore not an invalid running state; it is the
        // one-frame-old momentum that can survive a pose transition into this body. Do not
        // special-case it, even if another input-side transition has already changed the
        // visible running pose.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType: SamusMovementType.Running);
        bool movesLeft = (speed.AccelerationMode is SamusAccelerationMode.Accelerating or SamusAccelerationMode.Decelerating) == facingLeft;
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(movesLeft ? HorizontalDirection.Left : HorizontalDirection.Right, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });

        // A wall clears X speed immediately after block collision. Total
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
    /// Moves Samus as she turns around on the ground
    /// and as she turns around while jumping.
    /// The latter is misleading for the aimed crouching turns: crouched entry leaves no vertical
    /// direction, so the game runs its no-speed grounding probe rather than airborne gravity.
    /// </summary>
    internal static GroundedMovementResult StepTurningOnGround(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
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
                $"Grounded-turn movement requires a standing, crouched, or moonwalk turn pose, not {samus.Pose}.");
        }

        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        if (movementType is not (SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping))
            throw new InvalidOperationException(
                $"Grounded turn pose {samus.Pose} has movement type {movementType}.");
        if (movementType == SamusMovementType.TurningWhileJumping && !SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose))
            throw new InvalidOperationException($"Grounded turning-while-jumping admission requires an aimed crouched turn, not {samus.Pose}.");
        if (movementType == SamusMovementType.TurningWhileJumping && !samus.Kinematics.VerticalVelocity.HasNoDirection)
        {
            throw new InvalidOperationException(
                $"Airborne turning-while-jumping pose {samus.Pose} must execute through SamusAerialMovement.StepTurningInAir.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // The turn runs the complete X movement before ending her boost: the turn gains no Dash speed, and her extra
        // speed and momentum last only through this update's movement before they are cleared.
        speed.HandleExtraRunSpeed(
            movementType,
            controllerInput: GameplayAction.None,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // The turning-on-ground or turning-while-jumping speeds supply the
        // deceleration. If it takes the speed below zero, the speed stops and the
        // acceleration mode returns to accelerating before the direction is chosen.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);

        // The turn's direction comes from the pose's facing rather than its name; the acceleration mode then chooses
        // whether she moves the way she faces or against it.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            level,
            samus,
            frameCounter,
            roomObjects);

        // The turn cancels the speed boost on every turn frame. This matters even
        // without the Speed Booster item: the turn setup folded the numeric extra component but
        // deliberately left the running-momentum flag set until this movement. Then the movement
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
        ISamusRoomObjects roomObjects)
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

        // The extra run speed survives moonwalking while she has running momentum and is cleared otherwise; moonwalking
        // never gains Dash speed.
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Moonwalking,
            controllerInput: GameplayAction.None,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // Moonwalking has its own speeds. Its poses' facings intentionally
        // produce travel opposite the visible facing; turning reverses that direction just as it
        // does for every other family. Do not derive either direction from the pose's name.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType: SamusMovementType.Moonwalking);
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // Moonwalking then runs the same no-speed-calculation grounding probe as running.
        // The probe deliberately scales the downward probe by the total horizontal magnitude.
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
        ISamusRoomObjects roomObjects)
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

        // Ran-into-wall movement uses the same no-base-speed X movement as standing. Direction still
        // comes from the pose's own facing. Extra speed is included in the requested move
        // before the movement's unconditional cleanup, in the game's order.
        var requestedHorizontal = SamusHorizontalDisplacement.TowardFacing(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
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

        // Ran-into-wall movement then cancels the speed boost and clears both run/base components plus
        // acceleration mode on every frame.
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs standing movement for every landing pose: the ordinary
    /// and spin landings, plus the aimed and horizontally firing landings.
    /// Landing animation is cosmetic with respect to movement: every member uses standing's
    /// zero-base-speed horizontal pass, grounding probe, and momentum cleanup until the animation
    /// cue's pose transition returns to <see cref="SamusPoseId.FacingRightNormalPose"/> and <see cref="SamusPoseId.FacingLeftNormalPose"/>.
    /// </summary>
    internal static GroundedMovementResult StepLanding(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        // Do not duplicate the pose list here. The predicates are the single audited definition
        // of the standing movement type's landing family and deliberately include the two firing
        // landings. Keeping the movement selection and this defensive guard on the same predicates prevents a
        // landing the selection admits from being rejected here.
        if (!SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) &&
            !SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Landing movement requires a landing pose, not {samus.Pose}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        var requestedHorizontal = SamusHorizontalDisplacement.TowardFacing(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
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
        // a downward direction while this pose remains active, and that value must survive.
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Moves Samus at the end of a knockback, after its owner (damage or Ceres Ridley's ejection) has restored her
    /// normal step while a knockback pose still shows: her vertical collision result is cleared and she probes for
    /// ground, without moving.
    /// </summary>
    internal static BlockMoveResult StepKnockbackEnding(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
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
        ISamusRoomObjects roomObjects)
    {
        SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);

        // The probe alternates its left-to-right and right-to-left horizontal scan order on
        // the frame counter's parity. That parity can affect which simultaneous
        // collision side effect wins.
        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }
}
