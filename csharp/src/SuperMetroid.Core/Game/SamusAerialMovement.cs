using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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
    // `$90:9C21` selects one standalone 12-byte record for the movement handler installed
    // when Samus releases a grapple swing. These are not movement-type-indexed table bases.
    /// <summary>
    /// Ports the dry-air, no-hi-jump path through
    /// <c>Make_Samus_Jump</c> at <c>$90:98BC</c> and the normal-air branch of
    /// <c>Determine_Samus_YAcceleration</c> at <c>$90:9C5B</c>.
    /// </summary>
    public static void InitializeDryAirJump(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // Table offset zero is the dry-air entry. Reading the user's ROM, rather than
        // embedding 4.E000 and 0.2800, keeps regional timing and ROM provenance visible.
        samus.Kinematics.YSpeed = ReadWord(bus, SamusMovementRomData.VerticalMotion.NormalJumpSpeeds);
        samus.Kinematics.YSubspeed = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.NormalJumpSubspeeds);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureDryAirGravity(bus, samus);
        samus.Kinematics.YDirection = 1;
    }

    /// <summary>
    /// Ports all environment/equipment paths through <c>Make_Samus_Jump</c> at
    /// <c>$90:98BC</c>. Air/water/lava select word offsets zero/two/four in the normal or
    /// Hi-Jump tables; Gravity Suit forces offset zero before that equipment choice.
    /// </summary>
    public static void InitializeJump(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        ushort medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        int tableOffset = medium * 2;
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        int wholeTable = hiJumpEquipped
            ? SamusMovementRomData.VerticalMotion.HiJumpSpeeds
            : SamusMovementRomData.VerticalMotion.NormalJumpSpeeds;
        int fractionalTable = hiJumpEquipped
            ? SamusMovementRomData.VerticalMotion.HiJumpSubspeeds
            : SamusMovementRomData.VerticalMotion.NormalJumpSubspeeds;

        // The two words are loaded independently from ROM. Speed Booster's fractional
        // bonus likewise uses an independent 16-bit ADC and intentionally drops its carry.
        samus.Kinematics.YSpeed = ReadWord(bus, wholeTable + tableOffset);
        samus.Kinematics.YSubspeed = ReadWord(bus, fractionalTable + tableOffset);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(bus, samus);
        samus.Kinematics.YDirection = 1;
    }

    /// <summary>
    /// Ports <c>Make_Samus_WallJump</c> at <c>$90:9949</c> without duplicating its three
    /// liquid entries in pose code. Extra-run bonus and 16-bit arithmetic match normal jump.
    /// </summary>
    public static void InitializeWallJump(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        ushort medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        int tableOffset = medium * 2;
        bool hiJumpEquipped = samus.EquippedItems.HasAny(SamusEquipmentFlags.HiJumpBoots);
        int wholeTable = hiJumpEquipped
            ? SamusMovementRomData.VerticalMotion.HiWallJumpSpeeds
            : SamusMovementRomData.VerticalMotion.WallJumpSpeeds;
        int fractionalTable = hiJumpEquipped
            ? SamusMovementRomData.VerticalMotion.HiWallJumpSubspeeds
            : SamusMovementRomData.VerticalMotion.WallJumpSubspeeds;

        samus.Kinematics.YSpeed = ReadWord(bus, wholeTable + tableOffset);
        samus.Kinematics.YSubspeed = ReadWord(bus, fractionalTable + tableOffset);
        ApplyEquippedSpeedBoosterJumpBonus(samus);
        ConfigureEnvironmentGravity(bus, samus);
        samus.Kinematics.YDirection = 1;
    }

    /// <summary>
    /// Applies `$90:9905-$991F` / `$90:9992-$99AC`: when Speed Booster is equipped, the
    /// fractional extra-X word is added directly to Y subspeed and half the whole extra-X
    /// word is added to Y speed. These are independent 16-bit additions; the fractional
    /// overflow deliberately does not carry into the whole word.
    /// </summary>
    public static void ApplyEquippedSpeedBoosterJumpBonus(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            return;

        samus.Kinematics.YSubspeed = unchecked((ushort)(
            samus.Kinematics.YSubspeed + samus.HorizontalSpeed.ExtraRunSubspeed));
        samus.Kinematics.YSpeed = unchecked((ushort)(
            samus.Kinematics.YSpeed + (samus.HorizontalSpeed.ExtraRunSpeed >> 1)));
    }

    /// <summary>
    /// Publishes the normal-air gravity pair selected by <c>$90:9C5B</c>. The normal frame
    /// pipeline refreshes these environment-dependent words before movement even when the
    /// pose change was a walk-off rather than <c>Make_Samus_Jump</c>.
    /// </summary>
    public static void ConfigureDryAirGravity(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        samus.Kinematics.YSubacceleration = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.GravitySubaccelerations);
        samus.Kinematics.YAcceleration = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.GravityAccelerations);
    }

    /// <summary>
    /// Ports <c>Determine_Samus_YAcceleration</c> at <c>$90:9C5B</c>. The three adjacent
    /// ROM words at `$90:9EA1/$9EA7` are indexed by the exact bottom-boundary medium.
    /// </summary>
    public static void ConfigureEnvironmentGravity(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        int tableOffset = samus.LiquidPhysics.DetermineMovementMedium(samus) * 2;
        samus.Kinematics.YSubacceleration = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.GravitySubaccelerations + tableOffset);
        samus.Kinematics.YAcceleration = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.GravityAccelerations + tableOffset);
    }

    /// <summary>
    /// Executes one movement-type-2 frame from <c>Samus_Jumping_Movement</c> at
    /// <c>$90:8FB3</c>.
    /// </summary>
    public static AerialMovementResult StepNormalJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementKind(bus) != SamusMovementType.NormalJumping)
            throw new InvalidOperationException(
                $"Normal-jump movement requires type 2, not ${(byte)samus.ReadMovementType(bus):X2}.");
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.NormalJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            bus,
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidPhysicsState.Air);

        // Poses `$4B/$4C/$55-$5A` are genuine movement-type-2 poses, but native treats them as
        // a transition: base X speed is forced to zero, only external X/Y displacement is
        // applied, and normal vertical speed does not move Samus on this frame.
        if (samus.Pose is SamusPoseIds.NeutralJumpTransitionRightPose or
            SamusPoseIds.NeutralJumpTransitionLeftPose or
            SamusPoseIds.NormalJumpTransitionAimUpRightPose or
            SamusPoseIds.NormalJumpTransitionAimUpLeftPose or
            SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseIds.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseIds.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusPoseIds.NormalJumpTransitionAimDiagonalDownLeftPose)
        {
            samus.HorizontalSpeed.AccelerationMode = 0;
            int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed: 0);
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                samus.Kinematics,
                requested,
                plms: plms);
            if (horizontal.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));

            // `$90:8FD6` is not an empty Y frame: it applies only the externally produced
            // displacement. Zero returns before collision, positive values gain the native
            // downward +1.0 bias, and negative values move upward exactly as supplied.
            int? externalY = SamusExtraDisplacement.CalculateExtraOnlyVerticalDisplacement(
                samus.Kinematics);
            BlockMoveResult? vertical = null;
            if (externalY is int displacement)
            {
                vertical = SamusBlockCollision.MoveVertical(
                    bus,
                    level,
                    samus.Kinematics,
                    displacement,
                    scanLeftToRight: (nmiFrameCounter & 1) == 0,
                    plms: plms);
            }
            return new AerialMovementResult(
                horizontal,
                vertical,
                Landed: externalY >= 0 && vertical is { Collided: true },
                HitCeiling: externalY < 0 && vertical is { Collided: true });
        }

        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        BlockMoveResult horizontalMove = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.NormalJumping,
            plms);
        return FinishVerticalMovement(bus, level, samus, horizontalMove, nmiFrameCounter, plms: plms);
    }

    /// <summary>
    /// Executes one movement-type-3 frame from <c>Samus_SpinJumping_Movement</c> at
    /// <c>$90:9040</c>, including `$90:9D35`'s block-wall contact and launch test.
    /// </summary>
    public static AerialMovementResult StepSpinJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter,
        ushort controllerNewInput = 0,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementKind(bus) != SamusMovementType.SpinJumping)
            throw new InvalidOperationException(
                $"Spin-jump movement requires type 3, not ${(byte)samus.ReadMovementType(bus):X2}.");

        // `$90:A436-$90:A4CB` runs this before ordinary spin movement. Space Jump is not a
        // host-side double-jump: it only accepts a fresh Jump edge while descending and
        // while the split Y magnitude lies inside the cartridge's dry-air 8.8 window.
        bool fullySubmergedWithoutGravity =
            !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
            samus.LiquidPhysics.IsTopBoundarySubmerged(samus);
        if (!fullySubmergedWithoutGravity)
            TryRestartSpaceJump(bus, samus, controllerNewInput);

        // `$90:A4D7-$A4F1` plays the underwater Space Jump sound only at the last tick of
        // animation frames zero and eight. It is movement-owned, so publish it before the
        // later animation call can advance the timer/frame pair.
        if (fullySubmergedWithoutGravity &&
            samus.AnimationFrameTimer == 1 &&
            samus.AnimationFrame is 0 or 8)
        {
            samus.LiquidPhysics.QueueMovementSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2f), maximumQueued: 6);
        }

        // Screw Attack's damaging body is republished every eligible dry-air spin frame.
        // A fully charged ordinary/Space-Jump spin instead publishes index four, but only
        // outside full liquid physics. The common runtime clears the word before movement,
        // mirroring `$90:E725`, so neither stale damage mode can leak into another pose.
        if (!fullySubmergedWithoutGravity && SamusState.IsScrewAttackPose(samus.Pose))
            samus.HorizontalSpeed.ContactDamageIndex = 3;
        else if (!fullySubmergedWithoutGravity && samus.ProjectileFlareCounter >= 0x003c)
            samus.HorizontalSpeed.ContactDamageIndex = 4;

        // Setup_Collision_RespawningBombBlock at `$84:CE83` admits either boost stage four
        // (`$0B3E & $0F00 == $0400`) or literal Screw Attack pose `$81/$82`. This gate is
        // independent of contact-damage publication above: liquid suppresses Samus damage,
        // but the bank-$84 setup itself still reads pose/boost and can break the terrain.
        bool canBreakCollisionBombBlocks =
            (samus.HorizontalSpeed.SpeedBoostCounter & 0x0f00) == 0x0400 ||
            SamusState.IsScrewAttackPose(samus.Pose);

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.SpinJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            bus,
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidPhysicsState.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType: SamusMovementType.SpinJumping);

        // If acceleration did not overshoot the cap, spin jump retains motion only while
        // turning (mode 1) or while the input matching the pose's facing direction is held.
        // The carry-set cap path bypasses this test exactly as $90:906C does.
        bool forwardHeld = IsForwardHeld(bus, samus, controllerInput);
        bool allowHorizontal = calculation.ReachedMaximum ||
            speed.AccelerationMode == 1 ||
            forwardHeld;
        if (!allowHorizontal)
        {
            // `$90:9078-$90:9089` stops only base motion when no forward input remains.
            // Dash momentum is intentionally retained for a later direction press/landing.
            ClearBaseHorizontalMotion(speed);
            calculation = new AerialBaseSpeedResult(0, ReachedMaximum: false);
        }
        else if (speed.AccelerationMode == 0)
        {
            // Mode two is not ordinary deceleration here. $90:9B1F tests only bit zero,
            // so mode two continues accelerating on subsequent airborne frames.
            speed.AccelerationMode = 2;
        }

        int requested = CalculateDirectedDisplacement(bus, samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requested,
            canBreakBombBlocks: canBreakCollisionBombBlocks,
            plms: plms);
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));

        WallJumpCheckResult wall = CheckBlockWallJump(
            bus,
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
                horizontal,
                Vertical: null,
                Landed: false,
                HitCeiling: false,
                WallJumpTriggered: true,
                WallContact: true,
                WallDistance: wall.Distance);
        }

        AerialMovementResult verticalResult = FinishVerticalMovement(
            bus,
            level,
            samus,
            horizontal,
            nmiFrameCounter,
            canBreakBombBlocks: canBreakCollisionBombBlocks,
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
    public static AerialMovementResult StepWallJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementKind(bus) != SamusMovementType.WallJumping ||
            !SamusState.IsWallJumpPose(samus.Pose))
            throw new InvalidOperationException($"Wall-jump movement requires type $14 pose, not ${samus.Pose:X2}.");

        // Frames 23+ are the somersault portion and always use Screw-style index three.
        // Frames 3..22 use charge-beam index four only after the projectile flare counter
        // reaches 60. Frames 0..2 deliberately remain harmless even when fully charged.
        if (samus.AnimationFrame >= 0x17)
            samus.HorizontalSpeed.ContactDamageIndex = 3;
        else if (samus.AnimationFrame >= 3 && samus.ProjectileFlareCounter >= 0x003c)
            samus.HorizontalSpeed.ContactDamageIndex = 4;

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.WallJumping,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            bus,
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidPhysicsState.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        BlockMoveResult horizontal = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.WallJumping,
            plms);
        return FinishVerticalMovement(bus, level, samus, horizontal, nmiFrameCounter, plms: plms);
    }

    /// <summary>
    /// Executes movement type `$19` at `$90:A7CA`. The native entry is a direct call to
    /// <c>Samus_Jumping_Movement</c>; damage boost therefore receives ordinary variable-
    /// height jump cutoff, type-indexed X physics, block collision, gravity, and landing.
    /// </summary>
    public static AerialMovementResult StepDamageBoost(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementKind(bus) != SamusMovementType.DamageBoost ||
            samus.Pose is not (SamusPoseIds.DamageBoostLeftPose or SamusPoseIds.DamageBoostRightPose))
        {
            throw new InvalidOperationException(
                $"Damage-boost movement requires type $19 pose, not ${samus.Pose:X2}.");
        }

        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.DamageBoost,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            bus,
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidPhysicsState.Air);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        BlockMoveResult horizontal = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.DamageBoost,
            plms);
        return FinishVerticalMovement(bus, level, samus, horizontal, nmiFrameCounter, plms: plms);
    }

    /// <summary>
    /// Executes `$90:A790/$90:A7AD` for movement types `$17/$18`. Unlike ordinary aerial
    /// motion this calls the deceleration-ALLOWED X routine and does not apply variable-jump
    /// release; the three-frame turn animation therefore preserves native reversal momentum.
    /// </summary>
    public static AerialMovementResult StepTurningInAir(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        SamusMovementType movementType = samus.ReadMovementType(bus);
        // Crouched aimed turns use movement type `$17` even though they normally remain on
        // the floor. If a producer gives one nonzero Y direction, `$90:A790` immediately
        // executes the same X/simple-Y path as the ordinary `$2F/$30/$8F-$92/$9E/$9F`
        // jumping-turn records. The pose name therefore cannot be used as an admission gate.
        bool isTranslatedTurnPose = SamusState.IsAerialTurnPose(samus.Pose) ||
            (movementType == SamusMovementType.TurningWhileJumping &&
             SamusState.IsAimedCrouchingTurnPose(samus.Pose));
        if (movementType is not (SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling) ||
            !isTranslatedTurnPose)
            throw new InvalidOperationException($"Aerial-turn movement requires type $17/$18 pose, not ${samus.Pose:X2}.");

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType);
        int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requested,
            plms: plms);
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));

        // Simple_Samus_Y_Movement calls this before the shared gravity routine. A signed
        // underflow at the apex becomes a stationary downward state on this same frame.
        if (samus.Kinematics.YDirection == 1 && unchecked((short)samus.Kinematics.YSpeed) < 0)
        {
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YDirection = 2;
        }

        // CheckAndMoveY selects the no-speed probe for direction zero even in
        // airborne turn art. Moonfall therefore pauses gravity during its turn;
        // this is the same branch used by grounded aimed crouching turns.
        AerialMovementResult result = samus.Kinematics.YDirection == 0
            ? new AerialMovementResult(horizontal,
                SamusGroundedMovement.RunNoSpeedCalculationGroundingProbe(bus, level, samus, nmiFrameCounter, plms),
                false, false)
            : FinishVerticalMovement(bus, level, samus, horizontal, nmiFrameCounter, plms: plms);
        // Turning clears the collision-to-pose request after movement. A floor
        // collision clamps position but does not reset accumulated falling speed
        // or trigger landing presentation. The shared upward mover still owns
        // its immediate ceiling-stop velocity writes.

        // `$90:A79E/$90:A7BB` cancel speed boost and explicitly clear both extra words.
        speed.CancelRunningMomentum(samus.ReadPoseXDirection(bus));
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        return result with { Landed = false, HitCeiling = false };
    }

    /// <summary>Executes one movement-type-6 frame from <c>$90:9168</c>.</summary>
    public static AerialMovementResult StepFalling(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementKind(bus) != SamusMovementType.Falling)
            throw new InvalidOperationException(
                $"Falling movement requires type 6, not ${(byte)samus.ReadMovementType(bus):X2}.");
        samus.HorizontalSpeed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Falling,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            bus,
            liquidImpeded: samus.LiquidPhysics.DetermineMovementMedium(samus) !=
                SamusLiquidPhysicsState.Air);

        BlockMoveResult horizontal = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: SamusMovementType.Falling,
            plms);

        // $90:90C4 converts an underflowed upward magnitude to a stationary downward state
        // before the common vertical routine can turn the negative word into moonfall-like
        // motion. An ordinary walk-off already enters with direction two.
        if (samus.Kinematics.YDirection == 1 &&
            unchecked((short)samus.Kinematics.YSpeed) < 0)
        {
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YDirection = 2;
        }

        AerialMovementResult result = FinishVerticalMovement(bus, level, samus, horizontal, nmiFrameCounter, plms: plms);
        // The movement wrapper selects fast-fall art after physics, before AnimateSamus.
        // Aimed falling poses have their own lists and do not enter this branch.
        if (samus.Pose is SamusPoseIds.FallingRightPose or SamusPoseIds.FallingLeftPose or
            SamusPoseIds.FallingGunExtendedRightPose or SamusPoseIds.FallingGunExtendedLeftPose &&
            unchecked((short)(samus.Kinematics.YSpeed - SamusMovementRomData.FastFallAnimationSpeed)) >= 0 &&
            unchecked((short)(samus.AnimationFrame - SamusMovementRomData.FastFallAnimationFrame)) < 0)
            samus.SetAnimationFrameFromSpecialHandler(SamusMovementRomData.FastFallAnimationFrame,
                SamusMovementRomData.FastFallAnimationTimer);
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
    /// and restores the normal handler only after upward-speed underflow or vertical
    /// collision. Keeping that lifetime explicit prevents a grapple launch from silently
    /// acquiring ordinary jump input/caps one frame too early.
    /// </remarks>
    public static AerialMovementResult StepReleasedFromGrapple(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        if (!samus.Grapple.ReleasedMovementActive)
            throw new InvalidOperationException("Released-grapple movement handler is not active.");

        SamusKinematicsState state = samus.Kinematics;
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool restoreNormalHandler = false;

        // `$90:946E-$948A` treats a signed-negative upward whole speed as the apex-wrap
        // sentinel. It clears the complete magnitude, flips downward, and installs normal
        // movement, but deliberately continues the remainder of this final special frame.
        if (state.YDirection == 1 && unchecked((short)state.YSpeed) < 0)
        {
            state.YSpeed = 0;
            state.YSubspeed = 0;
            state.YDirection = 2;
            restoreNormalHandler = true;
        }

        // Native writes mode two every frame, so `$90:9A7E` always takes its deceleration
        // branch until it underflows and resets mode zero. Only then can an input-free frame
        // clear base speed instead of calling the horizontal mover.
        speed.AccelerationMode = 2;
        ushort medium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        int speedRecordAddress = medium switch
        {
            SamusLiquidPhysicsState.Water =>
                SamusMovementRomData.VerticalMotion.GrappleReleaseWaterSpeed,
            SamusLiquidPhysicsState.LavaAcid =>
                SamusMovementRomData.VerticalMotion.GrappleReleaseLavaAcidSpeed,
            _ => SamusMovementRomData.VerticalMotion.GrappleReleaseAirSpeed,
        };
        uint baseSpeed = speed.CalculateBaseSpeedAtAddress(bus, speedRecordAddress);

        bool horizontalInput = (controllerInput &
            ((ushort)SnesButton.Left | (ushort)SnesButton.Right)) != 0;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == 0 && !horizontalInput)
        {
            // `$90:94AA-$94B4` clears both persistent base words and the frame displacement.
            speed.BaseSpeed = 0;
            speed.BaseSubspeed = 0;
            speed.CalculateTotalSpeed(0);
            horizontal = SamusBlockCollision.MoveHorizontal(bus, level, state, 0, plms: plms);
        }
        else
        {
            int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(bus, level, state, requested, plms: plms);
            if (horizontal.Collided)
                speed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));
        }

        AerialMovementResult result = FinishVerticalMovement(
            bus,
            level,
            samus,
            horizontal,
            nmiFrameCounter,
            plms: plms);
        if (result.Vertical is { Collided: true })
            restoreNormalHandler = true;

        samus.Grapple.ReleasedMovementActive = !restoreNormalHandler;
        return result;
    }

    private static BlockMoveResult MoveNormalAerialX(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        SamusMovementType movementType,
        RoomPlmSystem? plms)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType);

        // $90:8FEF-$8FFB: a wall jump promotes zero acceleration mode to two
        // after calculating speed, then bypasses the held-direction admission test.
        if (movementType == SamusMovementType.WallJumping &&
            speed.AccelerationMode == SamusHorizontalAccelerationModes.Accelerating)
            speed.AccelerationMode = SamusHorizontalAccelerationModes.Decelerating;

        bool directionHeld = (controllerInput &
            ((ushort)SnesButton.Left | (ushort)SnesButton.Right)) != 0;
        if (speed.AccelerationMode == 0 && !directionHeld)
        {
            // $90:901E/$90:9185 clear both the DP displacement and persistent base speed.
            speed.BaseSpeed = 0;
            speed.BaseSubspeed = 0;
            speed.CalculateTotalSpeed(0);
            return SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, 0, plms: plms);
        }

        int requested = CalculateDirectedDisplacement(bus, samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requested,
            plms: plms);
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));
        return horizontal;
    }

    private static AerialMovementResult FinishVerticalMovement(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        BlockMoveResult horizontal,
        ushort nmiFrameCounter,
        bool canBreakBombBlocks = false,
        RoomPlmSystem? plms = null)
    {
        BlockMoveResult vertical = StepVerticalWithSpeedCalculations(
            bus,
            level,
            samus,
            nmiFrameCounter,
            out bool hitCeiling,
            out bool downwardDisplacement,
            canBreakBombBlocks,
            plms);
        bool landed = downwardDisplacement && vertical.Collided;

        return new AerialMovementResult(horizontal, vertical, landed, hitCeiling);
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
    internal static BlockMoveResult StepVerticalWithSpeedCalculations(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        out bool hitCeiling,
        out bool downwardDisplacement,
        bool canBreakBombBlocks = false,
        RoomPlmSystem? plms = null)
    {
        ValidateCommon(bus, level, samus);
        SamusKinematicsState state = samus.Kinematics;

        // `$90:90E5` snapshots the OLD speed into displacement before changing the stored
        // velocity. A newly initialized zero-speed fall therefore moves zero pixels on its
        // first handler call even though gravity has already accumulated for next frame.
        uint oldSpeed = state.VerticalSpeedFixed;
        if (state.YDirection == 2)
        {
            // `$90:9112` tests equality with whole speed five. Values above five are not
            // clamped; preserving that oddity matters for externally scripted velocities.
            if (state.YSpeed != 5)
            {
                SetVerticalSpeed(state, unchecked(state.VerticalSpeedFixed + Compose(
                    state.YAcceleration,
                    state.YSubacceleration)));
            }
        }
        else
        {
            SetVerticalSpeed(state, unchecked(state.VerticalSpeedFixed - Compose(
                state.YAcceleration,
                state.YSubacceleration)));
        }

        int displacement = state.YDirection == 2
            ? unchecked((int)oldSpeed)
            : unchecked(-(int)oldSpeed);
        displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            displacement);
        downwardDisplacement = displacement >= 0;
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(
            bus,
            level,
            state,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            canBreakBombBlocks: canBreakBombBlocks,
            plms: plms);

        hitCeiling = displacement < 0 && vertical.Collided;
        if (hitCeiling)
        {
            // The shared routine's ceiling branch zeroes both velocity halves and changes
            // the magnitude direction to down; it does not select a landing pose itself.
            state.YSpeed = 0;
            state.YSubspeed = 0;
            state.YDirection = 2;
        }

        return vertical;
    }

    private static void ApplyVariableJumpCutoff(
        SamusKinematicsState state,
        ushort controllerInput)
    {
        if (state.YDirection != 1)
            return;

        bool jumpHeld = (controllerInput & (ushort)SnesButton.A) != 0;
        bool speedUnderflowed = unchecked((short)state.YSpeed) < 0;
        if (!jumpHeld || speedUnderflowed)
        {
            state.YSpeed = 0;
            state.YSubspeed = 0;
            state.YDirection = 2;
        }
    }

    private static bool IsForwardHeld(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerInput) =>
        samus.IsFacingLeft(bus)
            ? (controllerInput & (ushort)SnesButton.Left) != 0
            : (controllerInput & (ushort)SnesButton.Right) != 0;

    /// <summary>
    /// Ports `$90:9D35`'s block and solid-enemy wall probes, including enemy-shake publication.
    /// </summary>
    private static WallJumpCheckResult CheckBlockWallJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput,
        RoomPlmSystem? plms)
    {
        // $90:9D35 rejects before either enemy or terrain observation. Probing
        // first is not equivalent: block contact can mutate real X subposition.
        if (!samus.PoseHistory.AllowsWallJumpProbe)
            return default;

        bool heldLeft = (controllerInput & (ushort)SnesButton.Left) != 0;
        bool heldRight = (controllerInput & (ushort)SnesButton.Right) != 0;
        if (!heldLeft && !heldRight)
            return default;

        // `$90:9D6B/$90:9DE3` test Left first and probe RIGHT; the mirror Right chord probes
        // LEFT. That counter-intuitive direction is deliberate: the button points away from
        // the wall behind the newly facing spin pose.
        int requested = heldLeft ? 8 << 16 : -(8 << 16);
        BlockMoveResult probe = SamusBlockCollision.ProbeWallHorizontal(
            bus,
            level,
            samus.Kinematics,
            requested,
            plms);
        if (!probe.Collided)
            return default;

        ushort distance = unchecked((ushort)Math.Abs(probe.AcceptedDisplacement >> 16));
        ushort firstEligibleFrame = SamusState.IsScrewAttackPose(samus.Pose)
            ? (ushort)0x1b
            : (ushort)0x0b;
        if (samus.AnimationFrame < firstEligibleFrame)
        {
            samus.ApplyWallContactAnimationRewind();
            return new WallJumpCheckResult(Triggered: false, Contact: true, Distance: distance);
        }

        bool jumpNew = (controllerNewInput & (ushort)SnesButton.A) != 0;
        bool triggered = jumpNew && distance < 8;
        if (triggered)
        {
            // Both successful exits converge on the same native result: the solid-enemy
            // branch writes five at `$90:9E5E`, and the terrain branch repeats that write at
            // `$90:9E7F`. This is not merely an internal boolean. `$0DC6` is shared WRAM
            // state, so retain the publication even though the host result below also tells
            // the runtime to install the wall-jump pose explicitly.
            samus.SolidVerticalCollisionResult = 5;

            if (probe.EnemyCollision is { EnemyIndex: ushort enemyIndex })
            {
                // `$90:9E64-$90:9E66` publishes only enemy-backed launches. Terrain wall
                // jumps intentionally leave the previous `$0E18` value alone.
                samus.EnemyIndexToShake = enemyIndex;
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
        ISnesAddressSpace bus,
        SamusState samus,
        ushort controllerNewInput)
    {
        if (!samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ||
            samus.Kinematics.YDirection != 2)
        {
            return false;
        }

        // The 65816 reads a deliberately unaligned word beginning at `$0B2D`: the high
        // byte of Y subspeed followed by the low byte of whole Y speed. In host terms that
        // is the magnitude converted from 16.16 to 8.8 without rounding.
        ushort fallingVelocity8_8 = unchecked((ushort)(
            (samus.Kinematics.YSpeed << 8) |
            (samus.Kinematics.YSubspeed >> 8)));
        // `$0AD2` is written by the preceding animation pass, not recomputed here. This
        // matters while crossing a surface: the top-boundary gate above and remembered
        // velocity window can intentionally describe different samples for one frame.
        ushort minimumVelocity = samus.LiquidPhysics.LiquidMedium !=
            SamusLiquidMedium.Air
                ? (ushort)0x0080
                : (ushort)0x0280;
        bool atOrAboveMinimum = unchecked((short)(fallingVelocity8_8 - minimumVelocity)) >= 0;
        bool belowMaximum = unchecked((short)(fallingVelocity8_8 - 0x0500)) < 0;
        if (!atOrAboveMinimum || !belowMaximum ||
            (controllerNewInput & (ushort)SnesButton.A) == 0)
        {
            return false;
        }

        // This is the same Samus_InitJump used by a grounded launch: ROM-authored initial
        // magnitude, Speed Booster's split-word bonus, dry-air gravity, and upward direction.
        SamusAerialMovement.InitializeJump(bus, samus);
        return true;
    }

    private static int CalculateDirectedDisplacement(
        ISnesAddressSpace bus,
        SamusState samus,
        uint baseSpeed)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        byte direction = samus.ReadPoseXDirection(bus);

        // $90:8EA9 reverses the pose's direction only in mode one. Modes zero and two use
        // the pose direction normally; this is why mode two is safe for aerial carry.
        bool movesLeft = speed.AccelerationMode == 1
            ? direction == 8
            : direction == 4;
        return movesLeft
            ? speed.CalculateLeftDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed)
            : speed.CalculateRightDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed);
    }

    private static void ClearBaseHorizontalMotion(SamusHorizontalSpeedState speed)
    {
        speed.BaseSpeed = 0;
        speed.BaseSubspeed = 0;
        speed.AccelerationMode = 0;
    }

    private static void ValidateCommon(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
    }

    private static void SetVerticalSpeed(SamusKinematicsState state, uint speed)
    {
        state.YSpeed = unchecked((ushort)(speed >> 16));
        state.YSubspeed = unchecked((ushort)speed);
    }

    private static uint Compose(ushort high, ushort low) => ((uint)high << 16) | low;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Both collision scans and collision state produced by one aerial frame.</summary>
public readonly record struct AerialMovementResult(
    BlockMoveResult Horizontal,
    BlockMoveResult? Vertical,
    bool Landed,
    bool HitCeiling,
    bool WallJumpTriggered = false,
    bool WallContact = false,
    ushort WallDistance = 0);

internal readonly record struct WallJumpCheckResult(
    bool Triggered,
    bool Contact,
    ushort Distance);
