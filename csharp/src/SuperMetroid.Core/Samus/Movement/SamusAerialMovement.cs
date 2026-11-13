using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's ordinary jump, spin-jump, wall-jump, aerial-turn, and falling movement, including
/// their shared water/lava table selection.
/// </summary>
/// <remarks>
/// This deliberately retains the game's split 16.16 magnitudes and separate vertical
/// direction. It does not use floating point, host elapsed time, or a guessed gravity
/// curve. Live enemy production and external displacement remain explicit boundaries. Ordinary
/// Dash and equipped Speed Booster momentum are retained through environment-selected
/// jumps, including the equipped vertical bonus. Terrain and solid/frozen-enemy wall jumps
/// plus all three liquid launch-table entries are distinct results.
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
        // The two speeds remain independent. Speed Booster's fractional
        // bonus likewise uses an independent addition wrapping at 65536 and intentionally drops its overflow.
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
    /// Applies the Speed Booster jump bonus: when Speed Booster is equipped, the
    /// extra run speed's fraction is added directly to the vertical speed's fraction and half its whole pixels to the
    /// vertical speed's whole pixels. The two additions are independent: a fraction sum that passes a whole pixel keeps
    /// only its remainder and never raises the whole speed.
    /// </summary>
    public static void ApplyEquippedSpeedBoosterJumpBonus(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            return;

        samus.Kinematics.YSpeed = new UnsignedSixteenSixteen(
            samus.Kinematics.YSpeed.Whole + (samus.HorizontalSpeed.ExtraRunSpeed.Whole >> 1),
            (samus.Kinematics.YSpeed.Fraction + samus.HorizontalSpeed.ExtraRunSpeed.Fraction) % PixelFractionSteps);
    }

    /// <summary>How far the spin wall-jump probe looks for a wall behind Samus, in pixels.</summary>
    private const int WallJumpProbePixels = 8;

    /// <summary>The steps of a speed's fraction of a pixel; a fraction sum past them keeps only its remainder.</summary>
    private const int PixelFractionSteps = 65536;

    /// <summary>The whole pixels the space-jump window reads within a cycle of 256; the speed's higher pixels drop out.</summary>
    private const int SpaceJumpWindowWholeCycle = 256;

    /// <summary>
    /// Chooses Samus's vertical acceleration. The three adjacent
    /// accelerations are indexed by the exact bottom-boundary medium.
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
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.NormalJumping)
            throw new InvalidOperationException(
                $"Normal-jump movement requires type 2, not {SamusPoseMetadata.ReadMovementType(samus)}.");
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.NormalJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);

        // NeutralJumpTransitionRightPose, NeutralJumpTransitionLeftPose and NormalJumpTransitionAimUpRightPose through NormalJumpTransitionAimDiagonalDownLeftPose are genuine movement-type-2 poses, but the game treats them as
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
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
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
                vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
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
        ControllerButton controllerInput,
        int frameCounter,
        ControllerButton controllerNewInput = ControllerButton.None,
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.SpinJumping)
            throw new InvalidOperationException(
                $"Spin-jump movement requires type 3, not {SamusPoseMetadata.ReadMovementType(samus)}.");

        // Runs this before ordinary spin movement. Space Jump is not a
        // host-side double-jump: it only accepts a fresh Jump edge while descending and
        // while the split Y magnitude lies inside the game's dry-air 8.8 window.
        bool fullySubmergedWithoutGravity =
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            samus.LiquidPhysics.IsTopBoundarySubmerged(samus);
        if (!fullySubmergedWithoutGravity)
            TryRestartSpaceJump(samus, controllerNewInput);

        // Plays the underwater Space Jump sound only at the last tick of
        // animation frames zero and eight. It is movement-owned, so publish it before the
        // later animation call can advance the timer/frame pair.
        if (fullySubmergedWithoutGravity &&
            samus.AnimationFrameTimer.IsAt(1) &&
            samus.AnimationFrame is 0 or 8)
        {
            samus.LiquidPhysics.QueueMovementSound(SamusSound.UnderwaterSpaceJump, priority: SoundRequestPriority.Normal);
        }

        // Screw Attack's damaging body is republished every eligible dry-air spin frame.
        // A fully charged ordinary/Space-Jump spin instead publishes index four, but only
        // outside full liquid physics. The common runtime clears the damage mode before movement,
        // mirroring the gameplay tail, so neither stale damage mode can leak into another pose.
        if (!fullySubmergedWithoutGravity && SamusPoseMetadata.IsScrewAttackPose(samus.Pose))
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        else if (!fullySubmergedWithoutGravity && samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter))
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.PseudoScrew;

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
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(movementType: SamusMovementType.SpinJumping);

        // If acceleration did not overshoot the cap, spin jump retains motion only while
        // turning (mode 1) or while the input matching the pose's facing direction is held.
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
            // Mode two is not ordinary deceleration here. The aerial speed calculation tests only whether Samus is turning,
            // so mode two continues accelerating on subsequent airborne frames.
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
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
            // do not execute on the trigger frame. Runtime pose handling consumes command
            // five after movement and installs the wall-jump pose with the authored launch speed.
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
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.WallJumping ||
            !SamusPoseMetadata.IsWallJumpPose(samus.Pose))
            throw new InvalidOperationException($"Wall-jump movement requires a wall-jumping pose, not {samus.Pose}.");

        // Frames 23+ are the somersault portion and always use Screw-style index three.
        // Frames 3..22 use charge-beam index four only after the projectile flare counter
        // reaches 60. Frames 0..2 deliberately remain harmless even when fully charged.
        if (samus.AnimationFrame >= 23)
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        else if (samus.AnimationFrame >= 3 && samus.BeamCharge.HasCharged(BeamChargeDefinitions.FullyChargedCounter))
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.PseudoScrew;

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
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.DamageBoost ||
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
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        // Crouched aimed turns use the turning-while-jumping movement type even though they normally remain on
        // the floor. If a producer gives one nonzero Y direction, the turn handler immediately
        // executes the same X/simple-Y path as the ordinary jumping-turn records
        // (unaimed, aimed up, down and diagonally up). The pose name therefore cannot be used as an admission gate.
        bool isTranslatedTurnPose = SamusPoseMetadata.IsAerialTurnPose(samus.Pose) ||
            (movementType == SamusMovementType.TurningWhileJumping &&
             SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose));
        if (movementType is not (SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling) ||
            !isTranslatedTurnPose)
            throw new InvalidOperationException($"Aerial-turn movement requires a jumping or falling turn pose, not {samus.Pose}.");

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        // The turning wrapper enters the shared X mover, including its
        // extra-run-speed epilogue, before canceling boost later in this frame.
        // Turning cannot actively dash, but retained boost still publishes contact
        // damage for this final frame. Clearing the counter is not retroactive.
        speed.HandleExtraRunSpeed(movementType, controllerInput: 0,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) != SamusLiquidMedium.Air);
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(movementType);
        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // The simple vertical movement calls this before the shared gravity step. A signed
        // underflow at the apex becomes a stationary downward state on this same frame.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up && SamusSpeedMagnitude.HasPassedZero(samus.Kinematics.YSpeed.Whole))
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        // The vertical check-and-move selects the no-speed probe for direction zero even in
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
        // only publishes a ceiling result: this handler clears it before pose
        // processing can stop the jump. Keep gravity and physical clipping, but
        // preserve upward velocity even while the head is touching the ceiling.

        // Cancel speed boost and explicitly clear both extra speeds.
        speed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        speed.ExtraRunSpeed = default;
        return result with { Landed = false, HitCeiling = false };
    }

    /// <summary>Executes one movement-type-6 frame.</summary>
    internal static AerialMovementResult StepFalling(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Falling)
            throw new InvalidOperationException(
                $"Falling movement requires type 6, not {SamusPoseMetadata.ReadMovementType(samus)}.");
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

        // Converts an underflowed upward magnitude to a stationary downward state
        // before the common vertical movement can turn the negative speed into moonfall-like
        // motion. An ordinary walk-off already enters with direction two.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up &&
            SamusSpeedMagnitude.HasPassedZero(samus.Kinematics.YSpeed.Whole))
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        AerialMovementResult result = FinishVerticalMovement(level, samus, frameCounter, roomObjects: roomObjects);
        // The movement wrapper selects fast-fall art after physics, before Samus is animated.
        // Aimed falling poses have their own animations and do not enter this branch.
        if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose or
            SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose &&
            SamusSpeedMagnitude.IsAtLeast(samus.Kinematics.YSpeed.Whole, SamusMovementDefinitions.FastFallAnimationSpeed) &&
            samus.AnimationFrame < SamusMovementDefinitions.FastFallAnimationFrame)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, SamusMovementDefinitions.FastFallAnimationFrame,
                SamusMovementDefinitions.FastFallAnimationTimer);
        return result;
    }

    /// <summary>
    /// Executes one frame of Samus's flight after release from a grapple swing.
    /// </summary>
    /// <remarks>
    /// This is a movement handler, not movement type two. It begins on the same
    /// frame that the grapple release derives launch velocity, survives the beam's following-frame
    /// cleanup, uses the three standalone authored acceleration records,
    /// and restores the normal handler after upward-speed underflow, any downward pass, or
    /// a ceiling hit. Keeping that lifetime explicit prevents a grapple launch from silently
    /// acquiring ordinary jump input/caps one frame too early.
    /// </remarks>
    internal static AerialMovementResult StepReleasedFromGrapple(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        ValidateCommon(level, samus);
        if (!samus.Grapple.ReleasedMovementActive)
            throw new InvalidOperationException("Released-grapple movement handler is not active.");

        SamusKinematicsState state = samus.Kinematics;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool restoreNormalHandler = false;

        // Treats a signed-negative upward whole speed as the apex-wrap
        // sentinel. It clears the complete magnitude, flips downward, and installs normal
        // movement, but deliberately continues the remainder of this final special frame.
        if (state.YDirection == SamusVerticalDirection.Up && SamusSpeedMagnitude.HasPassedZero(state.YSpeed.Whole))
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
            restoreNormalHandler = true;
        }

        // The game writes mode two every frame, so the speed calculation always takes its deceleration
        // branch until it underflows and resets mode zero. Only then can an input-free frame
        // clear base speed instead of calling the horizontal mover.
        speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        SamusStandaloneSpeedRecord speedRecord = medium switch
        {
            SamusLiquidMedium.Water => SamusStandaloneSpeedRecord.GrappleReleaseWater,
            SamusLiquidMedium.LavaOrAcid => SamusStandaloneSpeedRecord.GrappleReleaseLavaAcid,
            SamusLiquidMedium.Air => SamusStandaloneSpeedRecord.GrappleReleaseAir,
            _ => throw new InvalidOperationException($"Undefined SamusLiquidMedium {medium}."),
        };
        UnsignedSixteenSixteen baseSpeed = speed.CalculateBaseSpeed(speedRecord);

        bool horizontalInput = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != ControllerButton.None;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !horizontalInput)
        {
            // Clears both persistent base speeds and the frame displacement.
            speed.BaseSpeed = default;
            // This branch bypasses the speed calculation; retain the previous total-speed pair.
            SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.Zero });
        }
        else
        {
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        AerialMovementResult result = FinishVerticalMovement(

            level,
            samus,
            frameCounter,
            roomObjects: roomObjects);
        // Reads the full collision result: any downward pass, or a ceiling hit, ends the
        // special handler. Only a free upward pass keeps it for another frame.
        if (samus.SolidVerticalCollisionResult != SamusVerticalCollisionResult.None)
            restoreNormalHandler = true;

        samus.Grapple.ReleasedMovementActive = !restoreNormalHandler;
        return result;
    }

    /// <summary>Applies the selected medium's base speed and wall collision, preserving the no-input bypass and wall-jump exception.</summary>
    private static BlockMoveResult MoveNormalAerialX(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        SamusMovementType movementType,
        ISamusRoomObjects? roomObjects)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(movementType);

        // A wall jump promotes zero acceleration mode to two
        // after calculating speed, then bypasses the held-direction admission test.
        if (movementType == SamusMovementType.WallJumping &&
            speed.AccelerationMode == SamusAccelerationMode.Accelerating)
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;

        bool directionHeld = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != ControllerButton.None;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !directionHeld)
        {
            // Clear both the DP displacement and persistent base speed.
            speed.BaseSpeed = default;
            // Then branches past horizontal movement (and the speed calculation, so the
            // previous total-speed pair is retained): no slope alignment runs either.
            return BlockMoveResult.NotMoved;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return horizontal;
    }

    /// <summary>Runs the shared Y mover and reduces its collision scan to landing and ceiling signals for the caller.</summary>
    private static AerialMovementResult FinishVerticalMovement(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null,
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
    /// Ordinary jump/fall handlers call this after their X pass, but the drained-Samus
    /// handler calls this directly. Keeping one implementation
    /// is important: the old-speed displacement, equality-only terminal-speed check, and
    /// ceiling response are observable game behavior, not general-purpose host physics.
    /// </remarks>
    internal static BlockMoveResult StepVerticalWithSpeedCalculations(SamusTerrain terrain, SamusState samus, int frameCounter, out bool hitCeiling, out bool downwardDisplacement, SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        ValidateCommon(terrain.Level, samus);
        SamusKinematicsState state = samus.Kinematics;

        // Snapshots the OLD speed into displacement before changing the stored
        // velocity. A newly initialized zero-speed fall therefore moves zero pixels on its
        // first handler call even though gravity has already accumulated for next frame.
        UnsignedSixteenSixteen oldSpeed = state.YSpeed;
        if (state.YDirection == SamusVerticalDirection.Down)
        {
            // Tests equality with whole speed five. Values above five are not
            // clamped; preserving that oddity matters for externally scripted velocities.
            if (state.YSpeed.Whole != 5)
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
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(terrain, state, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

        // (down) and the upward mover (up) publish this result after every move. A
        // downward pass is never zero: 1 on a landing, otherwise 2 unless a wall jump's 5 is retained.
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
            // Ordinary handlers consume the ceiling collision as a jump stop. A turn
            // instead clears that pending result, so its caller defers these writes.
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }

        return vertical;
    }

    /// <summary>Ends upward movement on Jump release or signed speed underflow.</summary>
    private static void ApplyVariableJumpCutoff(
        SamusKinematicsState state,
        ControllerButton controllerInput)
    {
        if (state.YDirection != SamusVerticalDirection.Up)
            return;

        bool jumpHeld = (controllerInput & ControllerButton.A) != ControllerButton.None;
        bool speedUnderflowed = SamusSpeedMagnitude.HasPassedZero(state.YSpeed.Whole);
        if (!jumpHeld || speedUnderflowed)
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }
    }

    /// <summary>Tests the directional button matching the current facing pose.</summary>
    private static bool IsForwardHeld(
        SamusState samus,
        ControllerButton controllerInput) =>
        SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
            ? (controllerInput & ControllerButton.Left) != ControllerButton.None
            : (controllerInput & ControllerButton.Right) != ControllerButton.None;

    /// <summary>
    /// Runs the wall-jump check's block and solid-enemy wall probes; returns whether the jump chord launched a wall jump.
    /// </summary>
    private static bool CheckBlockWallJump(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        ISamusRoomObjects? roomObjects)
    {
        // Rejects before either enemy or terrain observation. Probing
        // first is not equivalent: block contact can mutate real X subposition.
        if (!samus.PoseHistory.AllowsWallJumpProbe)
            return false;

        int firstEligibleFrame = SamusPoseMetadata.IsScrewAttackPose(samus.Pose)
            ? 27
            : 11;
        // Once the spin reaches the frames a launch can start on, the check withdraws an earlier launch's enemy shake
        // before it reads the held direction; earlier frames leave it waiting.
        bool launchFrameReached = samus.AnimationFrame >= firstEligibleFrame;
        if (launchFrameReached)
            samus.Kinematics.WithdrawWallJumpLaunchShake();

        bool heldLeft = (controllerInput & ControllerButton.Left) != ControllerButton.None;
        bool heldRight = (controllerInput & ControllerButton.Right) != ControllerButton.None;
        if (!heldLeft && !heldRight)
            return false;

        // Test Left first and probe RIGHT; the mirror Right chord probes
        // LEFT. That counter-intuitive direction is deliberate: the button points away from
        // the wall behind the newly facing spin pose.
        SignedSixteenSixteen requested = SignedSixteenSixteen.FromParts(heldLeft ? WallJumpProbePixels : -WallJumpProbePixels, 0);
        BlockMoveResult probe = SamusBlockCollision.ProbeWallHorizontal(

            level,
            samus.Kinematics, samus,
            requested,
            roomObjects);
        if (!probe.Collided)
            return false;

        // The accepted whole travel is signed, so its magnitude is at most 32768 and the
        // distance holds the exact unsigned value.
        int distance = Math.Abs(probe.AcceptedDisplacement.WholePixels);
        if (!launchFrameReached)
        {
            SamusAerialPoseTransitions.ApplyWallContactAnimationRewind(samus);
            return false;
        }

        bool jumpNew = (controllerNewInput & ControllerButton.A) != ControllerButton.None;
        bool triggered = jumpNew && distance < 8;
        if (triggered)
        {
            // Both successful exits converge on the same result: the solid-enemy
            // branch writes five, and the terrain branch repeats that write
            // too. This is not merely an internal boolean. The shared result
            // state, so retain the publication even though the host result below also tells
            // the runtime to install the wall-jump pose explicitly.
            samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.WallJump;
            // A launch off a solid or frozen enemy asks the next enemy pass to shake that enemy; a launch off
            // terrain asks nothing.
            if (probe.EnemyCollision is { EnemySlot: int launchedFrom })
                samus.Kinematics.RequestWallJumpLaunchShake(launchedFrom);
        }
        return triggered;
    }

    /// <summary>
    /// The air/partially-submerged Space Jump gate of spin-jumping movement.
    /// Screw Attack can use this physics when both item bits are
    /// equipped even though its visible pose is the Screw Attack pose.
    /// </summary>
    private static bool TryRestartSpaceJump(
        SamusState samus,
        ControllerButton controllerNewInput)
    {
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ||
            samus.Kinematics.YDirection != SamusVerticalDirection.Down)
        {
            return false;
        }

        // The window reads the falling speed as an 8.8 velocity without rounding, its whole pixels taken within
        // their 256-pixel cycle.
        int fallingVelocity8_8 = EightEight.FromSixteenSixteen(
            samus.Kinematics.YSpeed.Whole % SpaceJumpWindowWholeCycle, samus.Kinematics.YSpeed.Fraction);
        // The remembered medium is written by the preceding animation pass, not recomputed here. This
        // matters while crossing a surface: the top-boundary gate above and remembered
        // velocity window can intentionally describe different samples for one frame.
        int minimumVelocity = samus.LiquidPhysics.LiquidPhysicsType !=
            SamusLiquidMedium.Air
                ? 128
                : 640;
        if (fallingVelocity8_8 < minimumVelocity || fallingVelocity8_8 >= 1280 ||
            (controllerNewInput & ControllerButton.A) == ControllerButton.None)
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
