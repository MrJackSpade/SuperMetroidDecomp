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
/// The movements below are the standing, running, turning-on-ground, moonwalking and ran-into-wall movement types, with
/// their speeds in air, water and lava (<see cref="SamusHorizontalSpeedDefinitions"/>), submerged Dash gating, and
/// ordinary Dash and equipped-Speed-Booster accumulation and
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
    /// <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/> and <see cref="SamusPoseId.ForwardFacingSuitedPose"/> share
    /// <see cref="SamusMovementType.Standing"/> movement with ordinary standing, but move neither horizontally nor by
    /// the one-pixel grounding probe, and keep their speeds. Her solid vertical collision result
    /// (<see cref="SamusState"/>) is cleared every update; while an elevator moves, she first moves one pixel down
    /// through the downward block collision. The elevator itself says whether it moves and draws the platform.
    /// </remarks>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="samus">Samus, in a front-facing pose.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    /// <param name="elevatorIsMoving">Whether an elevator carries her this update.</param>
    /// <param name="doorBlocks">What the door blocks the elevator carries her through do to her (#2545).</param>
    internal static void StepFacingForward(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter,
        bool elevatorIsMoving,
        DoorBlockMeeting doorBlocks)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        if (elevatorIsMoving)
        {
            // A moving elevator carries Samus down one whole pixel; that move meets no solid enemy but does meet the
            // room's blocks and their behaviours, in the left or right order the update's parity picks.
            _ = SamusBlockCollision.MoveVertical(terrain, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = SamusMovementDefinitions.ElevatorCarry, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false, DoorBlocks = doorBlocks });
        }

        // Clearing the vertical collision result is all front-view movement does without a moving elevator. In
        // particular, stale base/extra X speed is retained; facing forward clears those speeds at
        // setup time, not every frame in this movement step.
        samus.ForgetVerticalCollisionResult();
    }

    /// <summary>
    /// Runs the movement-relevant portion of standing movement for the poses of the standing movement type that neither
    /// face forward nor land: the standing poses, unaimed or aimed, and the X-ray scope's standing poses. It reads only the
    /// way the moving pose faces.
    /// </summary>
    internal static GroundedMovementResult StepStanding(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter,
        GameplayAction controllerInput)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacingLeft(samus.Pose);
        ResetStandingShotAnimation(samus, controllerInput);
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Standing moves Samus with no base speed, which still passes zero through
        // horizontal movement and the pose direction's displacement calculation. That
        // calculation publishes total-X speed before collision; do not skip it merely because
        // a freshly spawned standing Samus normally has zero base/extra speed. A left-facing
        // pose's direction is left, so the displacement is leftward even though its
        // base speed is zero, which matters once extra displacement contributes.
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft ? HorizontalDirection.Left : HorizontalDirection.Right, samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);

        // Standing then cancels the speed boost and clears extra speed, base speed, and
        // acceleration mode after both moves, whichever way Samus faces.
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>Restarts neutral standing's shot pose on held Shoot even when no projectile was admitted; angled poses keep their own animation.</summary>
    private static void ResetStandingShotAnimation(SamusState samus, GameplayAction controllerInput)
    {
        if (SamusPoseTraits.IsIn(samus.Pose, SamusPoseFamily.NeutralStanding) &&
            (controllerInput & GameplayAction.Shoot) != GameplayAction.None)
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationDelays.AnimationForPose(samus.Pose).Start, SamusMovementDefinitions.StandingShotAnimationTimer);
    }

    /// <summary>
    /// Runs running movement in dry air, including ordinary Dash, for either
    /// facing: the right-moving running family or the left-moving one, unaimed or aimed. As the game's running movement
    /// does, it reads only the way the pose faces.
    /// </summary>
    internal static GroundedMovementResult StepRunning(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter,
        GameplayAction controllerInput)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsFacingLeft(samus.Pose);
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // The liquid Samus's bottom boundary lies in can replace her ordinary speeds with the water or lava
        // speeds.
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // Running handles Dash before calculating base speed. With no Speed Booster equipped, Dash establishes momentum, adds
        // its acceleration each frame, and caps the extra component at its unboosted cap. The flag retains that
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
        // A wall clears X speed immediately after block collision. Total
        // speed deliberately remains published: the following grounding probe reads it
        // even if base speed was just cleared by a wall.
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Moves Samus as she turns around on the ground
    /// and as she turns around while jumping.
    /// The latter is misleading for the aimed crouching turns: crouched entry leaves no vertical
    /// direction, so the game runs its no-speed grounding probe rather than airborne gravity.
    /// </summary>
    /// <param name="movement">The turn on the ground the movement dispatch selected, whose movement type the turn reads.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="samus">Samus, turning.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static GroundedMovementResult StepTurningOnGround(
        SamusMovement.GroundTurnMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        SamusMovementType movementType = movement.Type;
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

        // The turning-on-ground or rising-or-aimed-crouched turn speeds supply the
        // deceleration. If it takes the speed below zero, the speed stops and the
        // acceleration mode returns to accelerating before the direction is chosen.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);

        // The turn's direction comes from the pose's facing rather than its name; the acceleration mode then chooses
        // whether she moves the way she faces or against it.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);

        // The turn cancels the speed boost on every turn update, even without the Speed Booster: the turn's setup
        // folded in her extra speed but left her running momentum until this movement, which cancels the momentum and
        // stops the extra run speed, each separately.
        speed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        speed.ExtraRunSpeed = default;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs moonwalking movement for the stable moonwalk poses, unaimed or aimed,
    /// including the complete shared extra-run-speed prepass.
    /// </summary>
    internal static GroundedMovementResult StepMoonwalking(
        SamusMovement.MoonwalkingMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectSpeedsFor(liquidMedium);

        // The extra run speed survives moonwalking while she has running momentum and is cleared otherwise; moonwalking
        // never gains Dash speed.
        speed.HandleExtraRunSpeed(
            movementType: movement.Type,
            controllerInput: GameplayAction.None,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // Moonwalking has its own speeds. Its poses' facings intentionally
        // produce travel opposite the visible facing; turning reverses that direction just as it
        // does for every other family. Do not derive either direction from the pose's name.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType: movement.Type);
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        // Moonwalking then runs the same no-speed-calculation grounding probe as running.
        // The probe deliberately scales the downward probe by the total horizontal magnitude.
        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs ran-into-wall movement for the ran-into-wall poses, unaimed or aimed. As the game's movement does, it reads only
    /// the way the pose faces.
    /// </summary>
    internal static GroundedMovementResult StepRanIntoWall(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Ran-into-wall movement uses the same no-base-speed X movement as standing. Direction still
        // comes from the pose's own facing. Extra speed is included in the requested move
        // before the movement's unconditional cleanup, in the game's order.
        var requestedHorizontal = SamusHorizontalDisplacement.TowardFacing(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        // The one-or-more-pixel downward grounding probe runs before all X values are
        // cleared. This preserves the same ledge behavior as ordinary wall stops even though the
        // ordinary wall-stop state normally begins with zero speed.
        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);

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
    /// It reads only the way the pose faces.
    /// </summary>
    internal static GroundedMovementResult StepLanding(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        var requestedHorizontal = SamusHorizontalDisplacement.TowardFacing(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);
        speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        // Landing art still uses standing movement: it does not reset Y values.
        // Settling the accepted landing owns its cleanup; a later hurt expiry may write
        // a downward direction while this pose remains active, and that value must survive.
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Moves Samus vertically without a speed calculation, including the external-Y
    /// replacement path and its asymmetric positive one-pixel bias.
    /// </summary>
    internal static BlockMoveResult RunNoSpeedCalculationGroundingProbe(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        SignedSixteenSixteen displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);

        // The probe alternates its left-to-right and right-to-left horizontal scan order on
        // the frame counter's parity. That parity can affect which simultaneous
        // collision side effect wins.
        return SamusBlockCollision.MoveVertical(terrain, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }
}
