using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified grounded movement slices from Samus's ordinary bank-$90 movement dispatcher.
/// </summary>
/// <remarks>
/// This is deliberately not a generic platformer controller. The entry points below
/// correspond only to movement types zero, one, and $0E, with no liquid, enemy collision,
/// run-button acceleration, conveyor displacement, knockback, or speed booster. Each
/// omitted system has observable native state and must be ported before its branch is
/// enabled; none is silently replaced with desktop physics.
/// </remarks>
public static class SamusGroundedMovement
{
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
        int requestedHorizontal = speed.CalculateRightDisplacement(baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // $90:A3A7-$90:A3D7 cancels speed boost and clears extra speed, base speed, and
        // acceleration mode after both movement calls. Speed-booster bookkeeping itself is
        // outside this no-equipment slice, but these five WRAM words are exact and live.
        ClearHorizontalMomentum(speed);
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
        int requestedHorizontal = speed.CalculateLeftDisplacement(baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // Standing's post-movement cleanup is direction-independent.
        ClearHorizontalMomentum(speed);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>Samus_Movement_01_Running</c> at <c>$90:A3E5</c> for the
    /// $09/$0D/$0F/$11 right-moving family in dry air without the run button.
    /// </summary>
    public static GroundedMovementResult StepRunningRight(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
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

        // BlockInsideReact_ShootableAir at $94:97D0 normally selects normal-air base
        // $9F55 earlier in the frame. The translated debug room has no water/acid or
        // special inside-block reaction, so make that real assignment visible here.
        speed.SelectNormalAirSpeedTable();

        // Samus_HandleExtraRunspeedX at $90:973E clears extra run speed when B is not held
        // and the momentum flag is clear. The grounded debug slice never introduces that
        // flag, equipment, or B input, so zero is the exact selected branch—not a tuning
        // choice. Assert the invariant instead of erasing unexpected future state.
        if (speed.ExtraRunSpeed != 0 || speed.ExtraRunSubspeed != 0)
        {
            throw new NotSupportedException(
                "Running with extra run speed requires the unported run-button/momentum branch at $90:973E.");
        }

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
        int requestedHorizontal = speed.CalculateRightDisplacement(baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);

        // $90:93B1 invokes Samus_ClearXSpeedIfColl immediately after block collision. Total
        // speed deliberately remains published: the following grounding routine reads it
        // even if base speed was just cleared by a wall.
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

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
        ushort nmiFrameCounter)
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
        speed.SelectNormalAirSpeedTable();

        // The no-run-button slice must not silently discard speed-booster state. This is
        // the same exact boundary enforced by StepRunningRight; direction does not alter
        // Samus_HandleExtraRunspeedX's unsupported equipment/input branch.
        if (speed.ExtraRunSpeed != 0 || speed.ExtraRunSubspeed != 0)
        {
            throw new NotSupportedException(
                "Running with extra run speed requires the unported run-button/momentum branch at $90:973E.");
        }

        // Modes zero and two use the pose's normal direction in $90:8EA9. Pose $0A stores
        // $04, selecting $90:E464's subtraction-based left displacement. Mode one belongs
        // exclusively to the separately translated grounded-turn handler below.
        if (speed.AccelerationMode is not (0 or 2))
        {
            throw new NotSupportedException(
                "Running-left acceleration mode one must execute through the grounded-turn pose.");
        }
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType: 1);
        int requestedHorizontal = speed.CalculateLeftDisplacement(baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

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
        if (!SamusState.IsRightToLeftGroundTurnPose(samus.Pose) &&
            !SamusState.IsLeftToRightGroundTurnPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Grounded-turn movement requires a verified standing or crouched turn pose, not ${samus.Pose:X2}.");
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
        speed.SelectNormalAirSpeedTable();

        // $91:F8D3 consumed extra run speed when the turn pose was installed. Seeing any
        // here means some unported system wrote it after that seam, so continuing would no
        // longer describe the cartridge's no-run-button path.
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
        bool movesLeft = speed.AccelerationMode == 1
            ? SamusState.IsLeftToRightGroundTurnPose(samus.Pose)
            : SamusState.IsRightToLeftGroundTurnPose(samus.Pose);
        int requestedHorizontal = movesLeft
            ? speed.CalculateLeftDisplacement(baseSpeed)
            : speed.CalculateRightDisplacement(baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // $90:A685 cancels speed boosting and $90:A689-$90:A68C clear extra run speed on
        // every turn frame. They should already be zero, but preserve the observable writes.
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs movement type zero for `$A4-$A7` and aimed normal-jump landings `$E0-$E5`. Native landing
    /// animation is cosmetic with respect to movement: it uses standing's zero-base-speed
    /// horizontal pass, grounding probe, and momentum cleanup until command $F8 returns to
    /// pose $01/$02.
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
        if (samus.Pose is not (
            SamusState.NormalLandingRightPose or SamusState.NormalLandingLeftPose or
            SamusState.SpinLandingRightPose or SamusState.SpinLandingLeftPose or
            SamusState.LandingAimUpRightPose or SamusState.LandingAimUpLeftPose or
            SamusState.LandingAimDiagonalUpRightPose or SamusState.LandingAimDiagonalUpLeftPose or
            SamusState.LandingAimDiagonalDownRightPose or SamusState.LandingAimDiagonalDownLeftPose))
        {
            throw new InvalidOperationException(
                $"Landing movement requires pose $A4-$A7/$E0-$E5, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        int requestedHorizontal = facingLeft
            ? speed.CalculateLeftDisplacement(baseSpeed: 0)
            : speed.CalculateRightDisplacement(baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(
            bus,
            level,
            samus,
            nmiFrameCounter);
        ClearHorizontalMomentum(speed);
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YDirection = 0;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports the zero-extra-Y branch of <c>Samus_Move_NoSpeedCalc_Y</c> at <c>$90:923F</c>.
    /// </summary>
    private static BlockMoveResult RunNoSpeedCalculationGroundingProbe(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // With no conveyor/quake displacement, native code uses total horizontal speed as
        // the downward probe unless post-X slope alignment already changed Y. It then adds
        // exactly 1.0 pixels. This is why a level-running Samus tests the floor by more than
        // one pixel while an aligned slope tests by exactly one.
        uint totalSpeed = ((uint)speed.TotalSpeed << 16) | speed.TotalSubspeed;
        int downwardDisplacement = samus.Kinematics.PositionAdjustedBySlope
            ? 0x00010000
            : unchecked((int)(totalSpeed + 0x00010000u));

        // $94:9763 alternates its left-to-right and right-to-left horizontal scan order on
        // the low bit of the accepted-NMI word. That parity can affect which simultaneous
        // collision side effect wins, so it remains an explicit input even in this slice.
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            downwardDisplacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
    }

    /// <summary>Exact speed-word subset cleared by <c>Samus_ClearXSpeedIfColl</c>.</summary>
    private static void ClearHorizontalMomentum(SamusHorizontalSpeedState speed)
    {
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
