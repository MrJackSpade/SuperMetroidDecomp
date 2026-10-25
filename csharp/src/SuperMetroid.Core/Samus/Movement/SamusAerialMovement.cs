using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Literal ports of Samus's ordinary jump, spin-jump, wall-jump, aerial-turn, and falling
/// movement routines in bank $90, including their shared water/lava table selection.
/// </summary>
/// <remarks>
/// This deliberately retains the cartridge's split 16.16 magnitudes and separate vertical
/// direction word. It does not use floating point, host elapsed time, or a guessed gravity
/// curve. Live enemy production and external displacement remain explicit boundaries. Ordinary
/// Dash and equipped Speed Booster momentum are retained through environment-selected
/// jumps, including the equipped vertical bonus. Terrain and solid/frozen-enemy wall jumps
/// plus all three liquid launch-table entries are translated as distinct native results.
/// </remarks>
public static class SamusAerialMovement
{
    /// <summary>
    /// Ports all environment/equipment paths through <c>Make_Samus_Jump</c> at
    /// <c>$90:98BC</c>. Air/water/lava select word offsets zero/two/four in the normal or
    /// Hi-Jump tables; Gravity Suit forces offset zero before that equipment choice.
    /// </summary>
    public static void InitializeJump(SamusState samus)
    {
        Ensure.NotNull(samus);

        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        // The two words remain independent. Speed Booster's fractional
        // bonus likewise uses an independent 16-bit ADC and intentionally drops its carry.
        samus.Kinematics.YSpeed =
            SamusVerticalMotionDefinitions.Launch(medium, hiJumpEquipped, kind: SamusLaunchKind.Jump);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
    }

    /// <summary>
    /// Ports <c>Make_Samus_WallJump</c> at <c>$90:9949</c> without duplicating its three
    /// liquid entries in pose code. Extra-run bonus and 16-bit arithmetic match normal jump.
    /// </summary>
    public static void InitializeWallJump(SamusState samus)
    {
        Ensure.NotNull(samus);

        SamusLiquidMedium medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        samus.Kinematics.YSpeed =
            SamusVerticalMotionDefinitions.Launch(medium, hiJumpEquipped, kind: SamusLaunchKind.WallJump);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
    }

    /// <summary>
    /// Applies `$90:9905-$991F` / `$90:9992-$99AC`: when Speed Booster is equipped, the
    /// fractional extra-X word is added directly to Y subspeed and half the whole extra-X
    /// word is added to Y speed. These are independent 16-bit additions; the fractional
    /// overflow deliberately does not carry into the whole word.
    /// </summary>
    public static void ApplyEquippedSpeedBoosterJumpBonus(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            return;

        samus.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(unchecked((ushort)(
            samus.Kinematics.YSpeed.Whole + (samus.HorizontalSpeed.ExtraRunSpeed.Whole >> 1))), unchecked((ushort)(
            samus.Kinematics.YSpeed.Fraction + samus.HorizontalSpeed.ExtraRunSpeed.Fraction)));
    }

    /// <summary>
    /// Ports <c>Determine_Samus_YAcceleration</c> at <c>$90:9C5B</c>. The three adjacent
    /// ROM words at `$90:9EA1/$9EA7` are indexed by the exact bottom-boundary medium.
    /// </summary>
    public static void ConfigureEnvironmentGravity(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Kinematics.YAcceleration =
            SamusVerticalMotionDefinitions.Gravity(samus.LiquidPhysics.DetermineMovementMedium(samus));
    }

    /// <summary>
    /// Executes one movement-type-2 frame from <c>Samus_Jumping_Movement</c> at
    /// <c>$90:8FB3</c>.
    /// </summary>
    internal static AerialMovementResult StepNormalJump(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.NormalJumping)
            throw new InvalidOperationException(
                $"Normal-jump movement requires type 2, not ${(byte)SamusPoseMetadata.ReadMovementType(samus):X2}.");
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.NormalJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidMedium.Air);

        // Poses `$4B/$4C/$55-$5A` are genuine movement-type-2 poses, but native treats them as
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
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed: 0);
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

            // `$90:8FD6` is not an empty Y frame: it applies only the externally produced
            // displacement. Zero returns before collision, positive values gain the native
            // downward +1.0 bias, and negative values move upward exactly as supplied.
            int? externalY = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(
                samus.Kinematics);
            BlockMoveResult? vertical = null;
            if (externalY is int displacement)
            {
                vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = (nmiFrameCounter & 1) == 0 });
            }
            return new AerialMovementResult(
                Landed: externalY >= 0 && vertical is { Collided: true },
                HitCeiling: externalY < 0 && vertical is { Collided: true });
        }

        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        MoveNormalAerialX(

            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.NormalJumping,
            plms);
        return FinishVerticalMovement(level, samus, nmiFrameCounter, plms: plms);
    }

    /// <summary>
    /// Executes one movement-type-3 frame from <c>Samus_SpinJumping_Movement</c> at
    /// <c>$90:9040</c>, including `$90:9D35`'s block-wall contact and launch test.
    /// </summary>
    internal static AerialMovementResult StepSpinJump(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int nmiFrameCounter,
        ControllerButton controllerNewInput = ControllerButton.None,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.SpinJumping)
            throw new InvalidOperationException(
                $"Spin-jump movement requires type 3, not ${(byte)SamusPoseMetadata.ReadMovementType(samus):X2}.");

        // `$90:A436-$90:A4CB` runs this before ordinary spin movement. Space Jump is not a
        // host-side double-jump: it only accepts a fresh Jump edge while descending and
        // while the split Y magnitude lies inside the cartridge's dry-air 8.8 window.
        bool fullySubmergedWithoutGravity =
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            samus.LiquidPhysics.IsTopBoundarySubmerged(samus);
        if (!fullySubmergedWithoutGravity)
            TryRestartSpaceJump(samus, controllerNewInput);

        // `$90:A4D7-$A4F1` plays the underwater Space Jump sound only at the last tick of
        // animation frames zero and eight. It is movement-owned, so publish it before the
        // later animation call can advance the timer/frame pair.
        if (fullySubmergedWithoutGravity &&
            samus.AnimationFrameTimer.RawValue == 1 &&
            samus.AnimationFrame is 0 or 8)
        {
            samus.LiquidPhysics.QueueMovementSound(Library1Sound.UnderwaterSpaceJump, maximumQueued: SoundQueueLimit.Max6);
        }

        // Screw Attack's damaging body is republished every eligible dry-air spin frame.
        // A fully charged ordinary/Space-Jump spin instead publishes index four, but only
        // outside full liquid physics. The common runtime clears the word before movement,
        // mirroring `$90:E725`, so neither stale damage mode can leak into another pose.
        if (!fullySubmergedWithoutGravity && SamusPoseMetadata.IsScrewAttackPose(samus.Pose))
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        else if (!fullySubmergedWithoutGravity && samus.ProjectileFlareCounter >= 60)
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.PseudoScrew;

        // Bomb-block breaking is independent of contact-damage publication above: liquid
        // suppresses Samus damage, but $84:CE83 still reads pose/boost through the shared
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
        // The carry-set cap path bypasses this test exactly as $90:906C does.
        bool forwardHeld = IsForwardHeld(samus, controllerInput);
        bool allowHorizontal = calculation.ReachedMaximum ||
            speed.AccelerationMode == SamusAccelerationMode.Turning ||
            forwardHeld;
        if (!allowHorizontal)
        {
            // `$90:9078-$90:9089` stops only base motion when no forward input remains.
            // Dash momentum is intentionally retained for a later direction press/landing.
            ClearBaseHorizontalMotion(speed);
            calculation = new AerialBaseSpeedResult(0, ReachedMaximum: false);
        }
        else if (speed.AccelerationMode == SamusAccelerationMode.Accelerating)
        {
            // Mode two is not ordinary deceleration here. $90:9B1F tests only bit zero,
            // so mode two continues accelerating on subsequent airborne frames.
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        WallJumpCheckResult wall = CheckBlockWallJump(

            level,
            samus,
            controllerInput,
            controllerNewInput,
            plms);
        if (wall.Triggered)
        {
            // Carry set at `$90:90BD` skips `$90:90BF`, so gravity and vertical displacement
            // do not execute on the trigger frame. Runtime pose handling consumes command
            // five after movement and installs `$83/$84` with the ROM launch speed.
            return new AerialMovementResult(
                Landed: false,
                HitCeiling: false,
                WallJumpTriggered: true,
                WallContact: true,
                WallDistance: wall.Distance);
        }

        AerialMovementResult verticalResult = FinishVerticalMovement(

            level,
            samus,
            nmiFrameCounter,
            plms: plms);
        return verticalResult with
        {
            WallContact = wall.Contact,
            WallDistance = wall.Distance,
        };
    }

    /// <summary>
    /// Executes movement type `$14` at `$90:A734`, including its animation-frame/charge-
    /// selected contact damage before the ordinary jumping routine.
    /// </summary>
    internal static AerialMovementResult StepWallJump(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.WallJumping ||
            !SamusPoseMetadata.IsWallJumpPose(samus.Pose))
            throw new InvalidOperationException($"Wall-jump movement requires type $14 pose, not ${(int)samus.Pose:X2}.");

        // Frames 23+ are the somersault portion and always use Screw-style index three.
        // Frames 3..22 use charge-beam index four only after the projectile flare counter
        // reaches 60. Frames 0..2 deliberately remain harmless even when fully charged.
        if (samus.AnimationFrame >= 23)
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        else if (samus.AnimationFrame >= 3 && samus.ProjectileFlareCounter >= 60)
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
            plms);
        return FinishVerticalMovement(level, samus, nmiFrameCounter, plms: plms);
    }

    /// <summary>
    /// Executes movement type `$19` at `$90:A7CA`. The native entry is a direct call to
    /// <c>Samus_Jumping_Movement</c>; damage boost therefore receives ordinary variable-
    /// height jump cutoff, type-indexed X physics, block collision, gravity, and landing.
    /// </summary>
    internal static AerialMovementResult StepDamageBoost(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.DamageBoost ||
            samus.Pose is not (SamusPoseId.DamageBoostLeftPose or SamusPoseId.DamageBoostRightPose))
        {
            throw new InvalidOperationException(
                $"Damage-boost movement requires type $19 pose, not ${(int)samus.Pose:X2}.");
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
            plms);
        return FinishVerticalMovement(level, samus, nmiFrameCounter, plms: plms);
    }

    /// <summary>
    /// Executes `$90:A790/$90:A7AD` for movement types `$17/$18`. Unlike ordinary aerial
    /// motion this calls the deceleration-ALLOWED X routine and does not apply variable-jump
    /// release; the three-frame turn animation therefore preserves native reversal momentum.
    /// </summary>
    internal static AerialMovementResult StepTurningInAir(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);
        // Crouched aimed turns use movement type `$17` even though they normally remain on
        // the floor. If a producer gives one nonzero Y direction, `$90:A790` immediately
        // executes the same X/simple-Y path as the ordinary `$2F/$30/$8F-$92/$9E/$9F`
        // jumping-turn records. The pose name therefore cannot be used as an admission gate.
        bool isTranslatedTurnPose = SamusPoseMetadata.IsAerialTurnPose(samus.Pose) ||
            (movementType == SamusMovementType.TurningWhileJumping &&
             SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose));
        if (movementType is not (SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling) ||
            !isTranslatedTurnPose)
            throw new InvalidOperationException($"Aerial-turn movement requires type $17/$18 pose, not ${(int)samus.Pose:X2}.");

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        // The native turning wrapper enters the shared X mover, including its
        // extra-run-speed epilogue, before canceling boost later in this frame.
        // Turning cannot actively dash, but retained boost still publishes contact
        // damage for this final frame. Clearing the counter is not retroactive.
        speed.HandleExtraRunSpeed(movementType, controllerInput: 0,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) != SamusLiquidMedium.Air);
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        uint baseSpeed = speed.CalculateBaseSpeed(movementType);
        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // Simple_Samus_Y_Movement calls this before the shared gravity routine. A signed
        // underflow at the apex becomes a stationary downward state on this same frame.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up && unchecked((short)samus.Kinematics.YSpeed.Whole) < 0)
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        // CheckAndMoveY selects the no-speed probe for direction zero even in
        // airborne turn art. Moonfall therefore pauses gravity during its turn;
        // this is the same branch used by grounded aimed crouching turns.
        AerialMovementResult result;
        if (samus.Kinematics.YDirection == SamusVerticalDirection.None)
        {
            SamusGroundedMovement.RunNoSpeedCalculationGroundingProbe(level, samus, nmiFrameCounter, plms);
            result = new AerialMovementResult(false, false);
        }
        else
            result = FinishVerticalMovement(level, samus, nmiFrameCounter,
                plms: plms, ceilingResponse: SamusCeilingResponse.Deferred);
        // Turning clears the collision-to-pose request after movement. A floor
        // collision clamps position but does not reset accumulated falling speed
        // or trigger landing presentation. Likewise, the cartridge upward mover
        // only publishes a ceiling result: this handler clears it before pose
        // processing can stop the jump. Keep gravity and physical clipping, but
        // preserve upward velocity even while the head is touching the ceiling.

        // `$90:A79E/$90:A7BB` cancel speed boost and explicitly clear both extra words.
        speed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        speed.ExtraRunSpeed = default;
        return result with { Landed = false, HitCeiling = false };
    }

    /// <summary>Executes one movement-type-6 frame from <c>$90:9168</c>.</summary>
    internal static AerialMovementResult StepFalling(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Falling)
            throw new InvalidOperationException(
                $"Falling movement requires type 6, not ${(byte)SamusPoseMetadata.ReadMovementType(samus):X2}.");
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
            plms);

        // $90:90C4 converts an underflowed upward magnitude to a stationary downward state
        // before the common vertical routine can turn the negative word into moonfall-like
        // motion. An ordinary walk-off already enters with direction two.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up &&
            unchecked((short)samus.Kinematics.YSpeed.Whole) < 0)
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        }

        AerialMovementResult result = FinishVerticalMovement(level, samus, nmiFrameCounter, plms: plms);
        // The movement wrapper selects fast-fall art after physics, before AnimateSamus.
        // Aimed falling poses have their own lists and do not enter this branch.
        if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose or
            SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose &&
            unchecked((short)(samus.Kinematics.YSpeed.Whole - SamusMovementDefinitions.FastFallAnimationSpeed)) >= 0 &&
            unchecked((short)(samus.AnimationFrame - SamusMovementDefinitions.FastFallAnimationFrame)) < 0)
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, SamusMovementDefinitions.FastFallAnimationFrame,
                SamusMovementDefinitions.FastFallAnimationTimer);
        return result;
    }

    /// <summary>
    /// Executes <c>SamusMovementHandler_ReleasedFromGrappleSwing</c> at
    /// <c>$90:946E-$94CA</c> for one frame.
    /// </summary>
    /// <remarks>
    /// This is a movement-handler pointer, not movement type two. It begins on the same
    /// frame that `$9B:C7B8` derives launch velocity, survives the beam's following-frame
    /// cleanup, uses the three standalone ROM acceleration records at `$90:9F31-$9F54`,
    /// and restores the normal handler after upward-speed underflow, any downward pass, or
    /// a ceiling hit. Keeping that lifetime explicit prevents a grapple launch from silently
    /// acquiring ordinary jump input/caps one frame too early.
    /// </remarks>
    internal static AerialMovementResult StepReleasedFromGrapple(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null)
    {
        ValidateCommon(level, samus);
        if (!samus.Grapple.ReleasedMovementActive)
            throw new InvalidOperationException("Released-grapple movement handler is not active.");

        SamusKinematicsState state = samus.Kinematics;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool restoreNormalHandler = false;

        // `$90:946E-$948A` treats a signed-negative upward whole speed as the apex-wrap
        // sentinel. It clears the complete magnitude, flips downward, and installs normal
        // movement, but deliberately continues the remainder of this final special frame.
        if (state.YDirection == SamusVerticalDirection.Up && unchecked((short)state.YSpeed.Whole) < 0)
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
            restoreNormalHandler = true;
        }

        // Native writes mode two every frame, so `$90:9A7E` always takes its deceleration
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
        uint baseSpeed = speed.CalculateBaseSpeed(speedRecord);

        bool horizontalInput = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != 0;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !horizontalInput)
        {
            // `$90:94AA-$94B4` clears both persistent base words and the frame displacement.
            speed.BaseSpeed = default;
            // This branch bypasses $90:E4E6; retain the previous total-speed pair.
            SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, state, samus, new HorizontalBlockMove { Displacement = 0 });
        }
        else
        {
            var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, state, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        AerialMovementResult result = FinishVerticalMovement(

            level,
            samus,
            nmiFrameCounter,
            plms: plms);
        // `$90:94BF` reads the whole word: any downward pass, or a ceiling hit, ends the
        // special handler. Only a free upward pass keeps it for another frame.
        if (samus.SolidVerticalCollisionResult != SamusVerticalCollisionResult.None)
            restoreNormalHandler = true;

        samus.Grapple.ReleasedMovementActive = !restoreNormalHandler;
        return result;
    }

    /// <summary>Applies the selected medium's base speed and wall collision, preserving the native no-input bypass and wall-jump exception.</summary>
    private static BlockMoveResult MoveNormalAerialX(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        SamusMovementType movementType,
        ISamusRoomPlms? plms)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(movementType);

        // $90:8FEF-$8FFB: a wall jump promotes zero acceleration mode to two
        // after calculating speed, then bypasses the held-direction admission test.
        if (movementType == SamusMovementType.WallJumping &&
            speed.AccelerationMode == SamusAccelerationMode.Accelerating)
            speed.AccelerationMode = SamusAccelerationMode.Decelerating;

        bool directionHeld = (controllerInput &
            (ControllerButton.Left | ControllerButton.Right)) != 0;
        if (speed.AccelerationMode == SamusAccelerationMode.Accelerating && !directionHeld)
        {
            // $90:901E/$90:9185 clear both the DP displacement and persistent base speed.
            speed.BaseSpeed = default;
            // $90:902B then branches past MoveSamus_Horizontally (and $90:E4E6, so the
            // previous total-speed pair is retained): no slope alignment runs either.
            return BlockMoveResult.NotMoved;
        }

        var requested = SamusHorizontalDisplacement.ForPoseDirection(samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requested.Displacement, CollisionMovementDirection = requested.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        return horizontal;
    }

    /// <summary>Runs the shared Y mover and reduces its collision scan to landing and ceiling signals for the caller.</summary>
    private static AerialMovementResult FinishVerticalMovement(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        ISamusRoomPlms? plms = null,
        SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        BlockMoveResult vertical = StepVerticalWithSpeedCalculations(new SamusTerrain { Level = level, Plms = plms }, samus, nmiFrameCounter, out bool hitCeiling, out bool downwardDisplacement, ceilingResponse);
        bool landed = downwardDisplacement && vertical.Collided;

        return new AerialMovementResult(landed, hitCeiling);
    }

    /// <summary>
    /// Ports the shared <c>Samus_Y_Movement_WithSpeedCalculations</c> routine at
    /// <c>$90:90E2</c> without adding any horizontal movement.
    /// </summary>
    /// <remarks>
    /// Ordinary jump/fall handlers call this after their X pass, but the drained-Samus
    /// handler at <c>$90:94CB</c> calls this routine directly. Keeping one implementation
    /// is important: the old-speed displacement, equality-only terminal-speed check, and
    /// ceiling response are observable native behavior, not general-purpose host physics.
    /// </remarks>
    internal static BlockMoveResult StepVerticalWithSpeedCalculations(SamusTerrain terrain, SamusState samus, int nmiFrameCounter, out bool hitCeiling, out bool downwardDisplacement, SamusCeilingResponse ceilingResponse = SamusCeilingResponse.Immediate)
    {
        ValidateCommon(terrain.Level, samus);
        SamusKinematicsState state = samus.Kinematics;

        // `$90:90E5` snapshots the OLD speed into displacement before changing the stored
        // velocity. A newly initialized zero-speed fall therefore moves zero pixels on its
        // first handler call even though gravity has already accumulated for next frame.
        uint oldSpeed = state.VerticalSpeedFixed;
        if (state.YDirection == SamusVerticalDirection.Down)
        {
            // `$90:9112` tests equality with whole speed five. Values above five are not
            // clamped; preserving that oddity matters for externally scripted velocities.
            if (state.YSpeed.Whole != 5)
            {
                SetVerticalSpeed(state, unchecked(state.VerticalSpeedFixed + SnesSixteenSixteen.Compose(
                    state.YAcceleration.Whole,
                    state.YAcceleration.Fraction)));
            }
        }
        else
        {
            SetVerticalSpeed(state, unchecked(state.VerticalSpeedFixed - SnesSixteenSixteen.Compose(
                state.YAcceleration.Whole,
                state.YAcceleration.Fraction)));
        }

        int displacement = state.YDirection == SamusVerticalDirection.Down
            ? unchecked((int)oldSpeed)
            : unchecked(-(int)oldSpeed);
        displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            displacement);
        downwardDisplacement = displacement >= 0;
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(terrain, state, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = (nmiFrameCounter & 1) == 0 });

        // `$90:E61B` (down) and `$90:E606` (up) publish this word after every move. A
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

        hitCeiling = displacement < 0 && vertical.Collided;
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

        bool jumpHeld = (controllerInput & ControllerButton.A) != 0;
        bool speedUnderflowed = unchecked((short)state.YSpeed.Whole) < 0;
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
            ? (controllerInput & ControllerButton.Left) != 0
            : (controllerInput & ControllerButton.Right) != 0;

    /// <summary>
    /// Ports `$90:9D35`'s block and solid-enemy wall probes, including enemy-shake publication.
    /// </summary>
    private static WallJumpCheckResult CheckBlockWallJump(

        RoomLevelData level,
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton controllerNewInput,
        ISamusRoomPlms? plms)
    {
        // $90:9D35 rejects before either enemy or terrain observation. Probing
        // first is not equivalent: block contact can mutate real X subposition.
        if (!samus.PoseHistory.AllowsWallJumpProbe)
            return default;

        bool heldLeft = (controllerInput & ControllerButton.Left) != 0;
        bool heldRight = (controllerInput & ControllerButton.Right) != 0;
        if (!heldLeft && !heldRight)
            return default;

        // `$90:9D6B/$90:9DE3` test Left first and probe RIGHT; the mirror Right chord probes
        // LEFT. That counter-intuitive direction is deliberate: the button points away from
        // the wall behind the newly facing spin pose.
        int requested = heldLeft ? 8 << 16 : -(8 << 16);
        BlockMoveResult probe = SamusBlockCollision.ProbeWallHorizontal(

            level,
            samus.Kinematics, samus,
            requested,
            plms);
        if (!probe.Collided)
            return default;

        // The accepted whole travel is a signed word, so its magnitude is at most 32768 and the
        // distance holds the exact value the native unsigned word would.
        int distance = Math.Abs(SnesSixteenSixteen.WholeOf(probe.AcceptedDisplacement));
        ushort firstEligibleFrame = SamusPoseMetadata.IsScrewAttackPose(samus.Pose)
            ? (ushort)27
            : (ushort)11;
        if (samus.AnimationFrame < firstEligibleFrame)
        {
            SamusAerialPoseTransitions.ApplyWallContactAnimationRewind(samus);
            return new WallJumpCheckResult(Triggered: false, Contact: true, Distance: distance);
        }

        bool jumpNew = (controllerNewInput & ControllerButton.A) != 0;
        bool triggered = jumpNew && distance < 8;
        if (triggered)
        {
            // Both successful exits converge on the same native result: the solid-enemy
            // branch writes five at `$90:9E5E`, and the terrain branch repeats that write at
            // `$90:9E7F`. This is not merely an internal boolean. `$0DC6` is shared WRAM
            // state, so retain the publication even though the host result below also tells
            // the runtime to install the wall-jump pose explicitly.
            samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.WallJump;

            if (probe.EnemyCollision is { EnemySlot: int enemySlot })
            {
                // `$90:9E64-$90:9E66` publishes only enemy-backed launches. Terrain wall
                // jumps intentionally leave the previous `$0E18` value alone.
                samus.EnemySlotToShake = enemySlot;
            }
        }
        return new WallJumpCheckResult(triggered, Contact: true, distance);
    }

    /// <summary>
    /// Ports the air/partially-submerged Space Jump gate inside <c>Samus_Movement_03_SpinJumping</c> at
    /// <c>$90:A46B-$90:A4A2</c>. Screw Attack can use this physics when both item bits are
    /// equipped even though its visible pose is `$81/$82`.
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

        // The 65816 reads a deliberately unaligned word beginning at `$0B2D`: the high
        // byte of Y subspeed followed by the low byte of whole Y speed. In host terms that
        // is the magnitude converted from 16.16 to 8.8 without rounding.
        ushort fallingVelocity8_8 = unchecked((ushort)SnesEightEight.FromSixteenSixteen(
            samus.Kinematics.YSpeed.Whole, samus.Kinematics.YSpeed.Fraction));
        // `$0AD2` is written by the preceding animation pass, not recomputed here. This
        // matters while crossing a surface: the top-boundary gate above and remembered
        // velocity window can intentionally describe different samples for one frame.
        ushort minimumVelocity = samus.LiquidPhysics.LiquidPhysicsType !=
            SamusLiquidMedium.Air
                ? (ushort)128
                : (ushort)640;
        bool atOrAboveMinimum = unchecked((short)(fallingVelocity8_8 - minimumVelocity)) >= 0;
        bool belowMaximum = unchecked((short)(fallingVelocity8_8 - 1280)) < 0;
        if (!atOrAboveMinimum || !belowMaximum ||
            (controllerNewInput & ControllerButton.A) == 0)
        {
            return false;
        }

        // This is the same Samus_InitJump used by a grounded launch: ROM-authored initial
        // magnitude, Speed Booster's split-word bonus, dry-air gravity, and upward direction.
        SamusAerialMovement.InitializeJump(samus);
        return true;
    }

    /// <summary>Clears ordinary base X velocity and acceleration while leaving separate boost momentum intact.</summary>
    private static void ClearBaseHorizontalMotion(SamusHorizontalSpeedState speed)
    {
        speed.BaseSpeed = default;
        speed.AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Checks that terrain, address space and Samus state are available before an aerial step.</summary>
    private static void ValidateCommon(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }

    /// <summary>Splits a 16.16 vertical magnitude back into Samus's independent native words.</summary>
    private static void SetVerticalSpeed(SamusKinematicsState state, uint speed)
    {
        state.YSpeed = new SnesUnsignedSixteenSixteen(unchecked((ushort)(SnesSixteenSixteen.WholeOf(speed))), unchecked((ushort)speed));
    }

}
