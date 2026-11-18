using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's ordinary jump, spin-jump, wall-jump, aerial-turn, and falling movement, including
/// their shared choice of water or lava physics.
/// </summary>
/// <remarks>
/// This keeps the game's speed magnitudes, each with its fraction of a pixel, and separate vertical
/// direction. It does not use floating point, host elapsed time, or a guessed gravity
/// curve. Solid enemies stop Samus through the shared block collision, and the displacement enemies and blocks
/// add to her is the shared extra displacement. Ordinary
/// Dash and equipped Speed Booster momentum are retained through environment-selected
/// jumps, including the equipped vertical bonus. Terrain and solid/frozen-enemy wall jumps
/// plus all three liquid launches are distinct results.
/// </remarks>
public static class SamusAerialMovement
{
    /// <summary>
    /// Starts a jump through every environment/equipment path.
    /// Air, water and lava each select their jump speed from the normal or
    /// Hi-Jump speeds; Gravity Suit selects the air speed before that equipment choice.
    /// </summary>
    public static void InitializeJump(SamusState samus)
    {
        Ensure.NotNull(samus);

        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        // Speed Booster's bonus then raises the launch speed part by part (SpeedBoosterJumpBonus).
        samus.Kinematics.YSpeed =
            SamusVerticalMotion.Launch(medium, hiJumpEquipped, kind: SamusLaunchKind.Jump);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
    }

    /// <summary>
    /// Starts a wall jump without duplicating its three
    /// liquid entries in pose code. Extra-run bonus and wrapping arithmetic match normal jump.
    /// </summary>
    public static void InitializeWallJump(SamusState samus)
    {
        Ensure.NotNull(samus);

        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        samus.Kinematics.YSpeed =
            SamusVerticalMotion.Launch(medium, hiJumpEquipped, kind: SamusLaunchKind.WallJump);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
    }

    /// <summary>
    /// Applies the Speed Booster jump bonus when Speed Booster is equipped: the dash's extra run speed raises the launch
    /// speed part by part, as <see cref="SpeedBoosterJumpBonus"/> describes.
    /// </summary>
    public static void ApplyEquippedSpeedBoosterJumpBonus(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            return;

        samus.Kinematics.YSpeed = SpeedBoosterJumpBonus.Boosted(samus.Kinematics.YSpeed, samus.HorizontalSpeed.ExtraRunSpeed);
    }

/// <summary>
    /// Chooses Samus's vertical acceleration for the medium she moves in (<see cref="SamusLiquidMedium"/>).
    /// </summary>
    public static void ConfigureEnvironmentGravity(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Kinematics.YAcceleration =
            SamusVerticalMotion.Gravity(samus.LiquidPhysics.DetermineMovementMedium(samus));
    }

    /// <summary>
    /// Executes one frame of normal-jumping movement.
    /// </summary>
    internal static AerialMovementResult StepNormalJump(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.NormalJumping)
            throw new InvalidOperationException(
                $"Normal-jump movement requires {nameof(SamusMovementType.NormalJumping)}, not {SamusPoseMetadata.ReadMovementType(samus)}.");
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.NormalJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);

        // NeutralJumpTransitionRightPose, NeutralJumpTransitionLeftPose and NormalJumpTransitionAimUpRightPose through NormalJumpTransitionAimDiagonalDownLeftPose are genuine normal-jumping poses, but the game treats them as
        // a transition: base X speed is forced to zero, only external X/Y displacement is
        // applied, and normal vertical speed does not move Samus on this frame.
        if (samus.Pose is SamusPoseId.NeutralJumpTransitionRightPose or
            SamusPoseId.NeutralJumpTransitionLeftPose or
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: default);
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

            // Is not an empty Y frame: it applies only the externally produced
            // displacement. Zero returns before collision, positive values gain the
            // downward +1.0 bias, and negative values move upward exactly as supplied.
            SignedSixteenSixteen? externalY = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(
                samus.Kinematics);
            BlockMoveResult? vertical = null;
            if (externalY is { } displacement)
            {
                vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
            }
            return new AerialMovementResult(
                Landed: externalY is { IsNegative: false } && vertical is { Collided: true },
                HitCeiling: externalY is { IsNegative: true } && vertical is { Collided: true });
        }

        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.NormalJumping,
            roomObjects);
        return FinishVerticalMovement(level, samus, frameCounter, roomObjects: roomObjects);
    }

    /// <summary>
    /// Executes one frame of spin-jumping movement,
    /// including the wall-jump block contact and launch test.
    /// </summary>
    internal static AerialMovementResult StepSpinJump(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        GameplayAction controllerNewInput,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpinJumping)
            throw new InvalidOperationException(
                $"Spin-jump movement requires {nameof(SamusMovementType.SpinJumping)}, not {SamusPoseMetadata.ReadMovementType(samus)}.");

        // Runs this before ordinary spin movement. Space Jump is not a
        // generic double-jump: it only accepts a fresh Jump edge while descending and
        // while the split Y magnitude lies inside the game's dry-air 1/256-pixel window.
        bool fullySubmergedWithoutGravity =
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            samus.LiquidPhysics.IsTopBoundarySubmerged(samus);
        if (!fullySubmergedWithoutGravity)
            TryRestartSpaceJump(samus, controllerNewInput);

        // Plays the underwater Space Jump sound only at the last tick of the spin's
        // start and middle frames. It is movement-owned, so publish it before the
        // later animation step can advance the timer/frame pair.
        if (fullySubmergedWithoutGravity &&
            samus.AnimationFrameTimer.IsAt(1) &&
            (samus.AnimationIsOn(SamusAnimationFrames.SpaceJumpSpinStart) || samus.AnimationIsOn(SamusAnimationFrames.SpaceJumpSpinMiddle)))
        {
            samus.AudioRequests.QueueSound(SamusSound.UnderwaterSpaceJump, priority: SoundRequestPriority.Normal);
        }

        // Screw Attack's damaging body is republished every eligible dry-air spin frame.
        // A fully charged ordinary/Space-Jump spin instead publishes the pseudo Screw Attack, but only
        // outside full liquid physics. The common runtime clears the damage mode before movement,
        // mirroring the gameplay tail, so neither stale damage mode can leak into another pose.
        if (!fullySubmergedWithoutGravity && SamusPoseMetadata.IsScrewAttackPose(samus.Pose))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.ScrewAttack;
        else if (!fullySubmergedWithoutGravity && samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.PseudoScrew;

        // Bomb-block breaking is independent of contact-damage publication above: liquid
        // suppresses Samus damage, but the bomb-block setup still reads pose/boost through the shared
        // collision gate (SamusState.BreaksCollisionBombBlocks) and can break the terrain.
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.SpinJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(movementType: SamusMovementType.SpinJumping);

        // If acceleration did not overshoot the cap, spin jump retains motion only while
        // turning (SamusAccelerationMode.Turning) or while the input matching the pose's facing direction is held.
        // A speed the maximum capped bypasses this test exactly as the game does.
        bool forwardHeld = IsForwardHeld(samus, controllerInput);
        bool allowHorizontal = calculation.ReachedMaximum ||
            speed.AccelerationMode == SamusAccelerationMode.Turning ||
            forwardHeld;
        if (!allowHorizontal)
        {
            // Stops only base motion when no forward input remains.
            // Dash momentum is intentionally retained for a later direction press/landing.
            ClearBaseHorizontalMotion(speed);
            calculation = new AerialBaseSpeedResult(default, ReachedMaximum: false);
        }
        else if (speed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            // SamusAccelerationMode.Decelerating is not ordinary deceleration here. The aerial speed calculation tests only whether Samus is turning,
            // so it continues accelerating on subsequent airborne frames.
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        bool wallJumpTriggered = CheckBlockWallJump(
            level,
            samus,
            controllerInput,
            controllerNewInput,
            roomObjects);
        if (wallJumpTriggered)
        {
            // A triggered wall-jump check skips gravity, so gravity and vertical displacement
            // do not execute on the trigger frame. Runtime pose handling consumes the
            // wall jump after movement and installs the wall-jump pose with the authored launch speed.
            return new AerialMovementResult(
                Landed: false,
                HitCeiling: false,
                WallJumpTriggered: true);
        }

        return FinishVerticalMovement(
            level,
            samus,
            frameCounter,
            roomObjects: roomObjects);
    }

    /// <summary>
    /// Executes wall-jumping movement, including its animation-frame/charge-
    /// selected contact damage before the ordinary jumping movement.
    /// </summary>
    internal static AerialMovementResult StepWallJump(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.WallJumping ||
            !SamusPoseMetadata.IsWallJumpPose(samus.Pose))
            throw new InvalidOperationException($"Wall-jump movement requires a wall-jumping pose, not {samus.Pose}.");

        // From the Screw Attack spin's start the somersault always attacks as the Screw Attack.
        // From the ordinary spin's start it attacks as the pseudo Screw Attack only after the projectile flare counter
        // reaches 60. The jump's frames before the spin remain harmless even when fully charged.
        if (samus.AnimationHasReached(SamusAnimationFrames.WallJumpScrewAttackSpinStart))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.ScrewAttack;
        else if (samus.AnimationHasReached(SamusAnimationFrames.WallJumpOrdinarySpinStart) && samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter))
            samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.PseudoScrew;

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.WallJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.WallJumping,
            roomObjects);
        return FinishVerticalMovement(level, samus, frameCounter, roomObjects: roomObjects);
    }

    /// <summary>
    /// Executes damage-boost movement. The entry is a direct call to
    /// normal-jumping movement; damage boost therefore receives ordinary variable-
    /// height jump cutoff, type-indexed X physics, block collision, gravity, and landing.
    /// </summary>
    internal static AerialMovementResult StepDamageBoost(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.DamageBoost ||
            samus.Pose is not (SamusPoseId.DamageBoostLeftPose or SamusPoseId.DamageBoostRightPose))
        {
            throw new InvalidOperationException(
                $"Damage-boost movement requires a damage-boost pose, not {samus.Pose}.");
        }

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.DamageBoost,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.DamageBoost,
            roomObjects);
        return FinishVerticalMovement(level, samus, frameCounter, roomObjects: roomObjects);
    }

    /// <summary>
    /// Executes the jumping and falling turns. Unlike ordinary aerial
    /// motion this calls the deceleration-ALLOWED X movement and does not apply variable-jump
    /// release; the three-frame turn animation therefore preserves reversal momentum.
    /// </summary>
    internal static AerialMovementResult StepTurningInAir(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        // Crouched aimed turns use the turning-while-jumping movement type even though they normally remain on
        // the floor. If a producer gives one a vertical direction, the turn movement immediately
        // executes the same X/simple-Y path as the ordinary jumping-turn records
        // (unaimed, aimed up, down and diagonally up). The pose name therefore cannot be used as an admission gate.
        bool runsAerialTurn = SamusPoseMetadata.IsAerialTurnPose(samus.Pose) ||
            (movementType == SamusMovementType.TurningWhileJumping &&
             SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose));
        if (movementType is not (SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling) ||
            !runsAerialTurn)
            throw new InvalidOperationException($"Aerial-turn movement requires a jumping or falling turn pose, not {samus.Pose}.");

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        // The turning movement enters the shared X mover, including its
        // extra-run-speed epilogue, before canceling boost later in this frame.
        // Turning cannot actively dash, but retained boost still publishes contact
        // damage for this final frame. Clearing the counter is not retroactive.
        speed.HandleExtraRunSpeed(movementType, controllerInput: GameplayAction.None,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) != SamusLiquidMedium.Air);
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);
        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // The simple vertical movement calls this before the shared gravity step. A speed
        // passing zero at the apex becomes a stationary downward state on this same frame.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up && SamusSpeedMagnitude.HasPassedZero(samus.Kinematics.YSpeed.Whole))
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        // The vertical check-and-move selects the no-speed probe for no vertical direction even in
        // airborne turn art. Moonfall therefore pauses gravity during its turn;
        // this is the same branch used by grounded aimed crouching turns.
        AerialMovementResult result;
        if (samus.Kinematics.YDirection == SamusVerticalDirection.None)
        {
            SamusGroundedMovement.RunNoSpeedCalculationGroundingProbe(level, samus, frameCounter, roomObjects);
            result = new AerialMovementResult(false, false);
        }
        else
            result = FinishVerticalMovement(level, samus, frameCounter,
                roomObjects: roomObjects, ceilingResponse: SamusCeilingResponse.Deferred);
        // Turning clears the collision-to-pose request after movement. A floor
        // collision clamps position but does not reset accumulated falling speed
        // or trigger landing presentation. Likewise, the game upward mover
        // only publishes a ceiling result: this movement clears it before pose
        // processing can stop the jump. Keep gravity and physical clipping, but
        // preserve upward velocity even while the head is touching the ceiling.

        // Cancel speed boost and explicitly clear both extra speeds.
        speed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        speed.ExtraRunSpeed = default;
        return result with { Landed = false, HitCeiling = false };
    }

    /// <summary>Executes one frame of <see cref="SamusMovementType.Falling"/> movement.</summary>
    internal static AerialMovementResult StepFalling(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Falling)
            throw new InvalidOperationException(
                $"Falling movement requires {nameof(SamusMovementType.Falling)}, not {SamusPoseMetadata.ReadMovementType(samus)}.");
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Falling,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);

        MoveNormalAerialX(
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.Falling,
            roomObjects);

        // Converts an upward magnitude that passed zero to a stationary downward state
        // before the common vertical movement can turn the negative speed into moonfall-like
        // motion. An ordinary walk-off already enters moving down.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up &&
            SamusSpeedMagnitude.HasPassedZero(samus.Kinematics.YSpeed.Whole))
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        AerialMovementResult result = FinishVerticalMovement(level, samus, frameCounter, roomObjects: roomObjects);
        // The movement selects fast-fall art after physics, before Samus is animated.
        // Aimed falling poses have their own animations and keep them.
        if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose or
            SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.YSpeed.Whole, SamusMovementDefinitions.FastFallAnimationSpeed) &&
            !samus.AnimationHasReached(SamusAnimationFrames.FastFallStart))
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationFrames.FastFall,
                SamusMovementDefinitions.FastFallAnimationTimer);
        return result;
    }

    /// <summary>
    /// Executes one frame of Samus's flight after release from a grapple swing.
    /// </summary>
    /// <remarks>
    /// This is a movement owner, not <see cref="SamusMovementType.NormalJumping"/> movement. It begins on the same
    /// frame that the grapple release derives launch velocity, survives the beam's following-frame
    /// cleanup, uses the three standalone authored acceleration records,
    /// and hands her movement back to her pose after the upward speed passes zero, any downward pass, or
    /// a ceiling hit. Keeping that lifetime explicit prevents a grapple launch from silently
    /// acquiring ordinary jump input/caps one frame too early.
    /// </remarks>
    internal static AerialMovementResult StepReleasedFromGrapple(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        ValidateCommon(level, samus);
        if (!samus.Grapple.ReleasedMovementActive)
            throw new InvalidOperationException("Released-grapple flight is not moving Samus.");

        SamusKinematicsState state = samus.Kinematics;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool releasedFlightEnds = false;

        // Treats a signed-negative upward whole speed as the apex-wrap
        // sentinel. It clears the complete magnitude, flips downward, and installs normal
        // movement, but deliberately continues the remainder of this final frame of the released flight.
        if (state.YDirection == SamusVerticalDirection.Up && SamusSpeedMagnitude.HasPassedZero(state.YSpeed.Whole))
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
            releasedFlightEnds = true;
        }

        // The game selects SamusAccelerationMode.Decelerating every frame, so the speed calculation always takes its deceleration
        // branch until it passes zero and resets to SamusAccelerationMode.Accelerating. Only then can an input-free frame
        // clear base speed instead of calling the horizontal mover.
        speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        // The release's speeds are the same in every medium, so the medium Samus is in does not choose them.
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(SamusHorizontalSpeedDefinitions.GrappleRelease);

        bool horizontalInput = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !horizontalInput)
        {
            // Clears both persistent base speeds and the frame displacement.
            speed.BaseSpeed = default;
            // With no direction held the speed is not calculated, and the previous total speeds stay.
            SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, samus.Pose, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.Zero });
        }
        else
        {
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        AerialMovementResult result = FinishVerticalMovement(
            level,
            samus,
            frameCounter,
            roomObjects: roomObjects);
        // Reads the full collision result: any downward pass, or a ceiling hit, ends the
        // released flight. Only a free upward pass keeps it for another frame.
        if (samus.SolidVerticalCollisionResult != SamusVerticalCollisionResult.None)
            releasedFlightEnds = true;

        samus.Grapple.ReleasedMovementActive = !releasedFlightEnds;
        return result;
    }

    /// <summary>Applies the selected medium's base speed and wall collision, preserving the no-input bypass and wall-jump exception.</summary>
    private static BlockMoveResult MoveNormalAerialX(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        SamusMovementType movementType,
        ISamusRoomObjects roomObjects)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectSpeedsFor(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(movementType);

        // A wall jump promotes accelerating to decelerating
        // after calculating speed, then bypasses the held-direction admission test.
        if (movementType == SamusMovementType.WallJumping &&
            speed.AccelerationMode == SamusAccelerationMode.Accelerating)
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;

        bool directionHeld = (controllerInput &
            (GameplayAction.Left | GameplayAction.Right)) != GameplayAction.None;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !directionHeld)
        {
            // Clear both the frame's horizontal displacement and persistent base speed.
            speed.BaseSpeed = default;
            // Then branches past horizontal movement (and the speed calculation, so the
            // previous total-speed pair is retained): no slope alignment runs either.
            return BlockMoveResult.NotMoved;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return horizontal;
    }

    /// <summary>Runs the shared Y mover and reduces its collision scan to the landing and ceiling signals her movement reads.</summary>
    private static AerialMovementResult FinishVerticalMovement(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects,
        SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        BlockMoveResult vertical = StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, frameCounter, out bool hitCeiling, out bool downwardDisplacement, ceilingResponse);
        bool landed = downwardDisplacement && vertical.Collided;

        return new AerialMovementResult(landed, hitCeiling);
    }

    /// <summary>
    /// Runs the shared vertical movement with its speed calculations
    /// without adding any horizontal movement.
    /// </summary>
    /// <remarks>
    /// Ordinary jumps and falls call this after their X pass, but the drained Samus's
    /// movement calls this directly. Keeping one implementation
    /// is important: the old-speed displacement, equality-only terminal-speed check, and
    /// ceiling response are observable game behavior, not general-purpose physics.
    /// </remarks>
    internal static BlockMoveResult StepVerticalWithSpeedCalculations(SamusTerrain terrain, SamusState samus, int frameCounter, out bool hitCeiling, out bool downwardDisplacement, SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        ValidateCommon(terrain.Level, samus);
        SamusKinematicsState state = samus.Kinematics;

        // Snapshots the OLD speed into displacement before changing the stored
        // velocity. A newly initialized zero-speed fall therefore moves zero pixels on its
        // first update even though gravity has already accumulated for next frame.
        UnsignedSixteenSixteen oldSpeed = state.YSpeed;
        if (state.YDirection == SamusVerticalDirection.Down)
        {
            // Tests equality with the terminal whole speed. Values above it are not
            // clamped; preserving that oddity matters for externally scripted velocities.
            if (state.YSpeed.Whole != SamusVerticalMotionDefinitions.TerminalFallWholeSpeed)
                state.YSpeed += state.YAcceleration;
        }
        else
        {
            state.YSpeed -= state.YAcceleration;
        }

        SignedSixteenSixteen displacement = state.YDirection == SamusVerticalDirection.Down
            ? oldSpeed.AsDisplacement()
            : -oldSpeed.AsDisplacement();
        displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            displacement);
        downwardDisplacement = !displacement.IsNegative;
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(terrain, state, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

        // Both vertical movers, downward and upward, publish this result after every move. A downward pass always
        // publishes one: Landed on a landing, otherwise Falling unless a wall jump's WallJump is retained.
        if (downwardDisplacement)
        {
            if (vertical.Collided)
                samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.Landed;
            else if (samus.SolidVerticalCollisionResult != SamusVerticalCollisionResult.WallJump)
                samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.Falling;
        }
        else
        {
            samus.SolidVerticalCollisionResult = vertical.Collided
                ? SamusVerticalCollisionResult.HitCeiling
                : SamusVerticalCollisionResult.None;
        }

        hitCeiling = displacement.IsNegative && vertical.Collided;
        if (hitCeiling && ceilingResponse != SamusCeilingResponse.Deferred)
        {
            // Ordinary movements consume the ceiling collision as a jump stop. A turn
            // instead clears that pending result, so the turn defers these writes.
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }

        return vertical;
    }

    /// <summary>Ends upward movement on Jump release or the speed passing zero.</summary>
    private static void ApplyVariableJumpCutoff(
        SamusKinematicsState state,
        GameplayAction controllerInput)
    {
        if (state.YDirection != SamusVerticalDirection.Up)
            return;

        bool jumpHeld = (controllerInput & GameplayAction.Jump) != GameplayAction.None;
        bool speedPassedZero = SamusSpeedMagnitude.HasPassedZero(state.YSpeed.Whole);
        if (!jumpHeld || speedPassedZero)
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }
    }

    /// <summary>Tests the directional button matching the current facing pose.</summary>
    private static bool IsForwardHeld(
        SamusState samus,
        GameplayAction controllerInput) =>
        SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? (controllerInput & GameplayAction.Left) != GameplayAction.None
            : (controllerInput & GameplayAction.Right) != GameplayAction.None;

    /// <summary>
    /// Runs the wall-jump check's block and solid-enemy wall probes; returns whether the jump chord launched a wall jump.
    /// </summary>
    private static bool CheckBlockWallJump(
        RoomLevelData level,
        SamusState samus,
        GameplayAction controllerInput,
        GameplayAction controllerNewInput,
        ISamusRoomObjects roomObjects)
    {
        // The check stops before she probes enemies or terrain: touching a block can move her by a fraction of a pixel.
        if (!samus.PoseHistory.AllowsWallJumpProbe)
            return false;

        SamusAnimationPlace firstEligibleFrame = SamusPoseMetadata.IsScrewAttackPose(samus.Pose)
            ? SamusAnimationFrames.ScrewAttackWallJumpReady
            : SamusAnimationFrames.SpinJumpWallJumpReady;
        // Once the spin reaches the frames a launch can start on, the check withdraws an earlier launch's enemy shake
        // before it reads the held direction; earlier frames leave it waiting.
        bool launchFrameReached = samus.AnimationHasReached(firstEligibleFrame);
        if (launchFrameReached)
            samus.Kinematics.WithdrawWallJumpLaunchShake();

        bool heldLeft = (controllerInput & GameplayAction.Left) != GameplayAction.None;
        bool heldRight = (controllerInput & GameplayAction.Right) != GameplayAction.None;
        if (!heldLeft && !heldRight)
            return false;

        // Test Left first and probe RIGHT; the mirror Right chord probes
        // LEFT. That counter-intuitive direction is deliberate: the button points away from
        // the wall behind the newly facing spin pose.
        SignedSixteenSixteen requested = SignedSixteenSixteen.FromParts(heldLeft ? SamusMovementDefinitions.WallJumpReachPixels : -SamusMovementDefinitions.WallJumpReachPixels, 0);
        BlockMoveResult probe = SamusBlockCollision.ProbeWallHorizontal(
            level,
            samus.Kinematics, samus, samus.Pose,
            requested,
            roomObjects);
        if (!probe.Collided)
            return false;

        // The accepted travel is a displacement either way; the distance is its whole pixels without their sign.
        int distance = Math.Abs(probe.AcceptedDisplacement.WholePixels);
        if (!launchFrameReached)
        {
            SamusAerialPoseTransitions.ApplyWallContactAnimationRewind(samus);
            return false;
        }

        bool jumpNew = (controllerNewInput & GameplayAction.Jump) != GameplayAction.None;
        bool triggered = jumpNew && distance < SamusMovementDefinitions.WallJumpReachPixels;
        if (triggered)
        {
            // A wall jump off a solid enemy and one off terrain both record the wall jump as her vertical collision
            // result, which others read, as well as returning it; the result below also tells
            // the runtime to install the wall-jump pose explicitly.
            samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.WallJump;
            // A launch off a solid or frozen enemy asks the next enemy pass to shake that enemy; a launch off
            // terrain asks nothing.
            if (probe.EnemyCollision is { Enemy: { } launchedFrom })
                samus.Kinematics.RequestWallJumpLaunchShake(launchedFrom);
        }
        return triggered;
    }

    /// <summary>
    /// The air/partially-submerged Space Jump gate of spin-jumping movement.
    /// Screw Attack can use this physics when both items are
    /// equipped even though its visible pose is the Screw Attack pose.
    /// </summary>
    private static bool TryRestartSpaceJump(
        SamusState samus,
        GameplayAction controllerNewInput)
    {
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ||
            samus.Kinematics.YDirection != SamusVerticalDirection.Down)
        {
            return false;
        }

        // The window compares the falling speed in 256ths of a pixel, so its bounds, each a whole number of 256ths, hold
        // exactly for the full speed. The game read only the speed's low 256 whole pixels, but a falling speed never
        // reaches 256: gravity adds under a pixel per update, so it would take over two thousand updates of falling,
        // past any room's height, to get there.
        UnsignedSixteenSixteen fallingSpeed = samus.Kinematics.YSpeed;
        // The remembered medium is written by the preceding animation pass, not recomputed here. This
        // matters while crossing a surface: the top-boundary gate above and remembered
        // velocity window can intentionally describe different samples for one frame.
        UnsignedSixteenSixteen minimumSpeed = samus.LiquidPhysics.LiquidPhysicsType != SamusLiquidMedium.Air
            ? SamusVerticalMotionDefinitions.SpaceJumpLiquidMinimumFallingSpeed
            : SamusVerticalMotionDefinitions.SpaceJumpAirMinimumFallingSpeed;
        if (fallingSpeed.IsSlowerThan(minimumSpeed) ||
            !fallingSpeed.IsSlowerThan(SamusVerticalMotionDefinitions.SpaceJumpFallingSpeedLimit) ||
            (controllerNewInput & GameplayAction.Jump) == GameplayAction.None)
        {
            return false;
        }

        // This is the same jump start used by a grounded launch: authored initial
        // magnitude, Speed Booster's split bonus, dry-air gravity, and upward direction.
        SamusAerialMovement.InitializeJump(samus);
        return true;
    }

    /// <summary>Clears ordinary base X velocity and acceleration while leaving separate boost momentum intact.</summary>
    private static void ClearBaseHorizontalMotion(SamusHorizontalSpeedState speed)
    {
        speed.BaseSpeed = default;
        speed.AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Checks that terrain and Samus state are available before an aerial step.</summary>
    private static void ValidateCommon(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }
}
