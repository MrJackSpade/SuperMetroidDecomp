using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified grounded movement slices from Samus's ordinary bank-$90 movement dispatcher.
/// </summary>
/// <remarks>
/// This is deliberately not a generic platformer controller. The entry points below
/// correspond only to movement types zero, one, $0E, $10, and $15. Air/water/lava ROM X
/// tables and submerged Dash gating are translated; enemy collision, conveyor displacement,
/// and knockback remain separate. Ordinary Dash and equipped-Speed-Booster accumulation/
/// staging are translated; palette/echo rendering lives outside movement. Each
/// omitted system has observable native state and must be ported before its branch is
/// enabled; none is silently replaced with desktop physics.
/// </remarks>
public static class SamusGroundedMovement
{
    /// <summary>
    /// Ports front-view movement at `$90:A383-$90:A3AB`, including the active-elevator
    /// one-pixel downward block scan.
    /// </summary>
    /// <remarks>
    /// Poses `$00/$9B` share movement type zero with ordinary standing, but the cartridge
    /// returns before horizontal movement, speed cleanup, and the one-pixel grounding probe.
    /// It only clears <c>SamusSolidVerticalCollisionResult</c> while elevator status is zero.
    /// A nonzero status calls `$94:9763` with exactly `1.0000` downward displacement before
    /// clearing that result. The elevator actor remains a separate producer of the status
    /// word and platform art; this method translates the complete Samus-side consumer.
    /// </remarks>
    public static BlockMoveResult? StepFacingForward(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        bool elevatorIsMoving = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsForwardFacingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Forward-facing movement requires pose $00/$9B, not ${samus.Pose:X2}.");
        }

        BlockMoveResult? vertical = null;
        if (elevatorIsMoving)
        {
            // `$90:A397-$A3A4` publishes collision direction two and calls the bank-$94
            // downward mover with `$12.$14 = 0001.0000`. That entry deliberately skips
            // solid-enemy collision but retains ordinary room-block/BTS scanning and the
            // alternating left/right order selected from accepted-NMI parity.
            vertical = SamusBlockCollision.MoveVertical(
                bus,
                level,
                samus.Kinematics,
                displacement: 1 << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0,
                includeSolidEnemies: false);
        }

        // `$90:A3A8` is the only write in the ordinary no-elevator path. In particular,
        // stale base/extra X speed is retained; MakeSamusFaceForward clears those words at
        // setup time, not every frame in this movement dispatcher.
        samus.SolidVerticalCollisionResult = 0;
        return vertical;
    }

    /// <summary>
    /// Ports the movement-relevant portion of <c>Samus_Movement_00_Standing</c> at
    /// <c>$90:A383</c> for the right-facing $01/$03/$05/$07 family.
    /// </summary>
    public static GroundedMovementResult StepStandingRight(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsRightFacingStandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Standing-right movement requires pose $01/$03/$05/$07, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // $90:A39E calls Samus_Move_NoBaseSpeed_X, which still passes zero through
        // Samus_MoveX and Samus_CalcDisplacementMoveRight. That calculation publishes
        // total-X speed before collision; do not skip it merely because a freshly spawned
        // standing Samus normally has zero base/extra speed.
        int requestedHorizontal = speed.CalculateRightDisplacement(
            baseSpeed: 0,
            samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // $90:A3A7-$90:A3D7 cancels speed boost and clears extra speed, base speed, and
        // acceleration mode after both movement calls. Speed-booster bookkeeping itself is
        // outside this no-equipment slice, but these five WRAM words are exact and live.
        ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports the same <c>$90:A383</c> standing handler for the left-facing
    /// $02/$04/$06/$08 family.
    /// </summary>
    public static GroundedMovementResult StepStandingLeft(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsLeftFacingStandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Standing-left movement requires pose $02/$04/$06/$08, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Samus_Move_NoBaseSpeed_X still routes through $90:8EA9. Pose $02's direction
        // byte is $04, so the native displacement helper is the LEFT form even though its
        // base argument is zero. That distinction matters if a future translated source
        // contributes extra displacement; keep it correct now instead of aliasing right.
        int requestedHorizontal = speed.CalculateLeftDisplacement(
            baseSpeed: 0,
            samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // Standing's post-movement cleanup is direction-independent.
        ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>Samus_Movement_01_Running</c> at <c>$90:A3E5</c> for the
    /// $09/$0D/$0F/$11 right-moving family in dry air, including ordinary Dash.
    /// </summary>
    public static GroundedMovementResult StepRunningRight(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        ushort controllerInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsRightFacingRunningPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Running-right movement requires pose $09/$0D/$0F/$11, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // `$90:9BD1` can replace the block reaction's ordinary `$9F55` pointer with the
        // complete water/lava table after comparing Samus's bottom boundary.
        ushort liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectEnvironmentSpeedTable(liquidMedium);

        // `$90:8E64` handles Dash before calculating base speed. With no equipped Speed
        // Booster, B establishes momentum, adds exactly 0.1000 per frame, and caps the
        // extra component at 2.0000. The flag retains that component after B is released.
        speed.HandleExtraRunSpeed(
            movementType: 1,
            controllerInput,
            speedBoosterEquipped: (samus.EquippedItems & 0x2000) != 0,
            bus,
            liquidImpeded: liquidMedium != SamusLiquidPhysicsState.Air);

        // $90:8E64 -> $90:9A7E advances the split 16.16 base speed, then $90:8EA9 and
        // $90:E4AD publish total speed and construct a rightward displacement. Pose $09's
        // pose-X direction byte is four. Acceleration mode zero accelerates right; mode two
        // is the native no-button momentum state and decelerates while retaining direction.
        if (speed.AccelerationMode is not (0 or 2))
        {
            throw new NotSupportedException(
                "Running-right acceleration mode one requires the unported reversal path.");
        }
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType: 1);
        int requestedHorizontal = speed.CalculateRightDisplacement(
            baseSpeed,
            samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);

        // $90:93B1 invokes Samus_ClearXSpeedIfColl immediately after block collision. Total
        // speed deliberately remains published: the following grounding routine reads it
        // even if base speed was just cleared by a wall.
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>Samus_Movement_01_Running</c> for the $0A/$0E/$10/$12 left-moving family.
    /// </summary>
    public static GroundedMovementResult StepRunningLeft(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        ushort controllerInput = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsLeftFacingRunningPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Running-left movement requires pose $0A/$0E/$10/$12, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        ushort liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectEnvironmentSpeedTable(liquidMedium);

        speed.HandleExtraRunSpeed(
            movementType: 1,
            controllerInput,
            speedBoosterEquipped: (samus.EquippedItems & 0x2000) != 0,
            bus,
            liquidImpeded: liquidMedium != SamusLiquidPhysicsState.Air);

        // Modes zero and two use the pose's normal direction in $90:8EA9. Pose $0A stores
        // $04, selecting $90:E464's subtraction-based left displacement. Mode one belongs
        // exclusively to the separately translated grounded-turn handler below.
        if (speed.AccelerationMode is not (0 or 2))
        {
            throw new NotSupportedException(
                "Running-left acceleration mode one must execute through the grounded-turn pose.");
        }
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType: 1);
        int requestedHorizontal = speed.CalculateLeftDisplacement(
            baseSpeed,
            samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports the grounded branches of <c>SamusMovement_TurningAround_OnGround</c> at
    /// <c>$90:A67C</c> and <c>SamusMovement_TurningAround_Jumping</c> at <c>$90:A790</c>.
    /// The latter name is misleading for `$97-$9A/$A2/$A3`: crouched entry leaves Y direction
    /// zero, so native runs its no-speed grounding probe rather than airborne gravity.
    /// </summary>
    public static GroundedMovementResult StepTurningOnGround(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        bool turnsLeft = SamusState.IsRightToLeftGroundTurnPose(samus.Pose) ||
            SamusState.IsMoonwalkTurnJumpLeftPose(samus.Pose);
        bool turnsRight = SamusState.IsLeftToRightGroundTurnPose(samus.Pose) ||
            SamusState.IsMoonwalkTurnJumpRightPose(samus.Pose);
        if (!turnsLeft && !turnsRight)
        {
            throw new InvalidOperationException(
                $"Grounded-turn movement requires a verified standing, crouched, or moonwalk turn pose, not ${samus.Pose:X2}.");
        }

        byte movementType = samus.ReadMovementType(bus);
        if (movementType is not (0x0e or 0x17))
            throw new InvalidOperationException($"Grounded turn pose ${samus.Pose:X2} has movement type ${movementType:X2}.");
        if (movementType == 0x17 && !SamusState.IsAimedCrouchingTurnPose(samus.Pose))
            throw new InvalidOperationException($"Grounded type-$17 admission requires an aimed crouched turn, not ${samus.Pose:X2}.");
        if (movementType == 0x17 && samus.Kinematics.YDirection != 0)
        {
            throw new NotSupportedException(
                $"Crouched turn pose ${samus.Pose:X2} became airborne; the type-$17 aerial branch is not translated by this grounded slice.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));

        // `$91:F8D3` consumed the complete extra component when the turn pose was installed.
        // A nonzero pair here could therefore only have been introduced after that exact
        // transition seam and is not a valid ordinary grounded-turn state.
        if (speed.ExtraRunSpeed != 0 || speed.ExtraRunSubspeed != 0)
        {
            throw new NotSupportedException(
                "Grounded turning with newly introduced extra run speed is not translated.");
        }
        if (speed.AccelerationMode is not (0 or 1))
        {
            throw new NotSupportedException(
                $"Grounded turn reached unsupported acceleration mode {speed.AccelerationMode}.");
        }

        // The pose's literal `$0E` or `$17` twelve-byte speed-table record supplies the
        // deceleration. If subtraction crosses below zero,
        // $90:9B0A clears both speed halves AND mode before $90:8EA9 chooses direction.
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType);

        // $90:8EA9 inverts the pose direction only while mode is nonzero and not two:
        //   right-to-left records have direction $04, so mode 1 carries old RIGHTWARD momentum;
        //   left-to-right records have direction $08, so mode 1 carries old LEFTWARD momentum.
        // Once CalculateBaseSpeed clears mode at zero, these helpers switch to the new
        // facing direction, but the zero displacement makes that final switch invisible.
        bool movesLeft = speed.AccelerationMode == 1 ? turnsRight : turnsLeft;
        int requestedHorizontal = movesLeft
            ? speed.CalculateLeftDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed)
            : speed.CalculateRightDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // `$90:A685` calls `Samus_CancelSpeedBoost` on every turn frame. This matters even
        // without the Speed Booster item: `$91:F8D3` folded the numeric extra component but
        // deliberately left `$0B3C` set until this movement handler. Then `$90:A689-$A68C`
        // perform the separately observable zero writes to the two extra-speed words.
        speed.CancelRunningMomentum(samus.ReadPoseXDirection(bus));
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>SamusMovement_Moonwalking</c> at <c>$90:A694</c> for stable poses
    /// `$49/$4A/$75-$78` in dry air without run-button extra speed.
    /// </summary>
    public static GroundedMovementResult StepMoonwalking(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsMoonwalkingPose(samus.Pose) || samus.ReadMovementType(bus) != 0x10)
        {
            throw new InvalidOperationException(
                $"Moonwalking movement requires pose $49/$4A/$75-$78, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));

        // `$90:A697` calls the same complete X path as running. The no-run-button slice
        // cannot erase an extra component written by a future speed-booster implementation.
        if (speed.ExtraRunSpeed != 0 || speed.ExtraRunSubspeed != 0)
        {
            throw new NotSupportedException(
                "Moonwalking with extra run speed requires the untranslated $90:973E branch.");
        }
        if (speed.AccelerationMode is not (0 or 2))
        {
            throw new NotSupportedException(
                $"Stable moonwalking reached unsupported acceleration mode {speed.AccelerationMode}.");
        }

        // Type `$10` has its own twelve-byte speed record. The pose-X bytes are intentionally
        // opposite the visible facing: left-facing `$49/$75/$77` store eight and move right;
        // right-facing `$4A/$76/$78` store four and move left. Do not derive travel from names.
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType: 0x10);
        bool movesLeft = samus.ReadPoseXDirection(bus) == 4;
        int requestedHorizontal = movesLeft
            ? speed.CalculateLeftDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed)
            : speed.CalculateRightDisplacement(baseSpeed, samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        // `$90:A69A` is the same no-speed-calculation grounding probe used by running.
        // It deliberately scales the downward probe by the total horizontal magnitude.
        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>Samus_Movement_15_RanIntoWall</c> at <c>$90:A75F</c> for
    /// `$89/$8A/$CF-$D2` in the block-only dry-room slice.
    /// </summary>
    public static GroundedMovementResult StepRanIntoWall(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        if (!SamusState.IsRanIntoWallPose(samus.Pose) || samus.ReadMovementType(bus) != 0x15)
        {
            throw new InvalidOperationException(
                $"Ran-into-wall movement requires pose $89/$8A/$CF-$D2, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // `$90:A75F` calls the same no-base-speed X routine as standing. Direction still
        // comes from the literal pose definition: `$89/$CF/$D1` store eight (right), while
        // `$8A/$D0/$D2` store four (left). Extra speed is included in the requested move
        // before the handler's unconditional cleanup, matching the native call order.
        int requestedHorizontal = samus.ReadPoseXDirection(bus) == 4
            ? speed.CalculateLeftDisplacement(baseSpeed: 0, samus.Kinematics.ExtraXFixed)
            : speed.CalculateRightDisplacement(baseSpeed: 0, samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        // The one-or-more-pixel downward grounding probe runs before all X words are
        // cleared. This preserves the same ledge behavior as `$90:A762` even though the
        // ordinary wall-stop state normally begins with zero speed.
        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // `$90:A766-$A77F` cancels speed boost and clears both run/base components plus
        // acceleration mode on every frame. The speed-booster counters themselves remain
        // outside this slice; the five movement words are exact and debugger-visible.
        ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs movement type zero for every translated landing pose: `$A4-$A7` for the ordinary
    /// and spin families, plus `$E0-$E7` for aimed and horizontally firing landings. Native
    /// landing animation is cosmetic with respect to movement: every member uses standing's
    /// zero-base-speed horizontal pass, grounding probe, and momentum cleanup until animation
    /// command `$F8` returns to pose `$01/$02`.
    /// </summary>
    public static GroundedMovementResult StepLanding(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        // Do not duplicate the pose list here. The predicates are the single audited definition
        // of movement type zero's landing family and deliberately include firing landings `$E6`
        // and `$E7`. Keeping dispatch and this defensive guard on the same predicates prevents a
        // newly translated landing from reaching this real movement handler only to be rejected.
        if (!SamusState.IsRightFacingLandingPose(samus.Pose) &&
            !SamusState.IsLeftFacingLandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Landing movement requires pose $A4-$A7/$E0-$E7, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        int requestedHorizontal = facingLeft
            ? speed.CalculateLeftDisplacement(baseSpeed: 0, samus.Kinematics.ExtraXFixed)
            : speed.CalculateRightDisplacement(baseSpeed: 0, samus.Kinematics.ExtraXFixed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);
        ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = 0;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>Samus_Move_NoSpeedCalc_Y</c> at <c>$90:923F</c>, including the external-Y
    /// replacement path and its asymmetric positive one-pixel bias.
    /// </summary>
    private static BlockMoveResult RunNoSpeedCalculationGroundingProbe(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        int displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);

        // $94:9763 alternates its left-to-right and right-to-left horizontal scan order on
        // the low bit of the accepted-NMI word. That parity can affect which simultaneous
        // collision side effect wins, so it remains an explicit input even in this slice.
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
    }

    /// <summary>Exact speed-word subset cleared by <c>Samus_ClearXSpeedIfColl</c>.</summary>
    private static void ClearHorizontalMomentum(
        SamusHorizontalSpeedState speed,
        byte poseXDirection)
    {
        speed.CancelRunningMomentum(poseXDirection);
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.BaseSpeed = 0;
        speed.BaseSubspeed = 0;
        speed.AccelerationMode = 0;
    }
}

/// <summary>Both bank-$94 scans performed by one grounded bank-$90 movement handler.</summary>
public readonly record struct GroundedMovementResult(
    BlockMoveResult Horizontal,
    BlockMoveResult Vertical);
