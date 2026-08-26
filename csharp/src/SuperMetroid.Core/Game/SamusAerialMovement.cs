using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal dry-air ports of Samus's ordinary jump, spin-jump, wall-jump, aerial-turn, and
/// falling movement routines in bank $90.
/// </summary>
/// <remarks>
/// This deliberately retains the cartridge's split 16.16 magnitudes and separate vertical
/// direction word. It does not use floating point, host elapsed time, or a guessed gravity
/// curve. Enemy collision, liquid physics, run-button acceleration, and external displacement
/// remain explicit boundaries. Wall-jump BLOCK collision and dry equipment launch tables are
/// translated; solid-enemy wall jumps are not silently treated as blocks.
/// </remarks>
public static class SamusAerialMovement
{
    private const int InitialYSpeedJumpingAddress = 0x909eb9;
    private const int InitialYSubspeedJumpingAddress = 0x909ebf;
    private const int YSubaccelerationInAirAddress = 0x909ea1;
    private const int YAccelerationInAirAddress = 0x909ea7;

    /// <summary>
    /// Ports the dry-air, no-hi-jump, no-speed-booster path through
    /// <c>Make_Samus_Jump</c> at <c>$90:98BC</c> and the normal-air branch of
    /// <c>Determine_Samus_YAcceleration</c> at <c>$90:9C5B</c>.
    /// </summary>
    public static void InitializeDryAirJump(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);

        // Table offset zero is the dry-air entry. Reading the user's ROM, rather than
        // embedding 4.E000 and 0.2800, keeps regional timing and ROM provenance visible.
        samus.Kinematics.YSpeed = ReadWord(bus, InitialYSpeedJumpingAddress);
        samus.Kinematics.YSubspeed = ReadWord(bus, InitialYSubspeedJumpingAddress);
        ConfigureDryAirGravity(bus, samus);
        samus.Kinematics.YDirection = 1;
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
        samus.Kinematics.YSubacceleration = ReadWord(bus, YSubaccelerationInAirAddress);
        samus.Kinematics.YAcceleration = ReadWord(bus, YAccelerationInAirAddress);
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
        ushort nmiFrameCounter)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementType(bus) != 2)
            throw new InvalidOperationException($"Normal-jump movement requires type 2, not ${samus.ReadMovementType(bus):X2}.");
        EnsureNoExtraRunSpeed(samus.HorizontalSpeed);

        // Poses `$4B/$4C/$55-$5A` are genuine movement-type-2 poses, but native treats them as
        // a transition: base X speed is forced to zero, only external X/Y displacement is
        // applied, and normal vertical speed does not move Samus on this frame.
        if (samus.Pose is SamusState.NeutralJumpTransitionRightPose or
            SamusState.NeutralJumpTransitionLeftPose or
            SamusState.NormalJumpTransitionAimUpRightPose or
            SamusState.NormalJumpTransitionAimUpLeftPose or
            SamusState.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusState.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusState.NormalJumpTransitionAimDiagonalDownRightPose or
            SamusState.NormalJumpTransitionAimDiagonalDownLeftPose)
        {
            samus.HorizontalSpeed.AccelerationMode = 0;
            int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed: 0);
            BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                samus.Kinematics,
                requested);
            if (horizontal.Collided)
                ClearHorizontalMomentum(samus.HorizontalSpeed);
            return new AerialMovementResult(horizontal, Vertical: null, Landed: false, HitCeiling: false);
        }

        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        BlockMoveResult horizontalMove = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: 2);
        return FinishVerticalMovement(bus, level, samus, horizontalMove, nmiFrameCounter);
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
        ushort controllerNewInput = 0)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementType(bus) != 3)
            throw new InvalidOperationException($"Spin-jump movement requires type 3, not ${samus.ReadMovementType(bus):X2}.");
        EnsureNoExtraRunSpeed(samus.HorizontalSpeed);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectNormalAirSpeedTable();
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType: 3);

        // If acceleration did not overshoot the cap, spin jump retains motion only while
        // turning (mode 1) or while the input matching the pose's facing direction is held.
        // The carry-set cap path bypasses this test exactly as $90:906C does.
        bool forwardHeld = IsForwardHeld(bus, samus, controllerInput);
        bool allowHorizontal = calculation.ReachedMaximum ||
            speed.AccelerationMode == 1 ||
            forwardHeld;
        if (!allowHorizontal)
        {
            ClearHorizontalMomentum(speed);
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
            requested);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

        WallJumpCheckResult wall = CheckBlockWallJump(
            bus,
            level,
            samus,
            controllerInput,
            controllerNewInput);
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
            nmiFrameCounter);
        return verticalResult with
        {
            WallContact = wall.Contact,
            WallDistance = wall.Distance,
        };
    }

    /// <summary>
    /// Executes movement type `$14` at `$90:A734`. Contact-damage selection is unrelated to
    /// locomotion; the routine's movement branch is the ordinary jumping routine verbatim.
    /// </summary>
    public static AerialMovementResult StepWallJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementType(bus) != 0x14 || !SamusState.IsWallJumpPose(samus.Pose))
            throw new InvalidOperationException($"Wall-jump movement requires type $14 pose, not ${samus.Pose:X2}.");

        EnsureNoExtraRunSpeed(samus.HorizontalSpeed);
        ApplyVariableJumpCutoff(samus.Kinematics, controllerInput);
        BlockMoveResult horizontal = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: 0x14);
        return FinishVerticalMovement(bus, level, samus, horizontal, nmiFrameCounter);
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
        ushort nmiFrameCounter)
    {
        ValidateCommon(bus, level, samus);
        byte movementType = samus.ReadMovementType(bus);
        if (movementType is not (0x17 or 0x18) || !SamusState.IsAerialTurnPose(samus.Pose))
            throw new InvalidOperationException($"Aerial-turn movement requires type $17/$18 pose, not ${samus.Pose:X2}.");
        if (samus.Kinematics.YDirection == 0)
        {
            throw new InvalidOperationException(
                $"Airborne turn pose ${samus.Pose:X2} has no Y direction; grounded aimed turns use the separate no-speed path.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectNormalAirSpeedTable();
        uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType);
        int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requested);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);

        // Simple_Samus_Y_Movement calls this before the shared gravity routine. A signed
        // underflow at the apex becomes a stationary downward state on this same frame.
        if (samus.Kinematics.YDirection == 1 && unchecked((short)samus.Kinematics.YSpeed) < 0)
        {
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YDirection = 2;
        }

        AerialMovementResult result = FinishVerticalMovement(
            bus,
            level,
            samus,
            horizontal,
            nmiFrameCounter);
        if (result.Vertical is { Collided: true })
        {
            // Both upward and downward collision tables for types `$17/$18` select command
            // five's no-pose-change handler. It clears Y motion but leaves the turn animation
            // alive until its `$F8` operand supplies the final aimed jump/fall pose.
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YDirection = 2;
        }

        // `$90:A79E/$90:A7BB` cancel speed boost and explicitly clear both extra words.
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        return result;
    }

    /// <summary>Executes one movement-type-6 frame from <c>$90:9168</c>.</summary>
    public static AerialMovementResult StepFalling(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter)
    {
        ValidateCommon(bus, level, samus);
        if (samus.ReadMovementType(bus) != 6)
            throw new InvalidOperationException($"Falling movement requires type 6, not ${samus.ReadMovementType(bus):X2}.");
        EnsureNoExtraRunSpeed(samus.HorizontalSpeed);

        BlockMoveResult horizontal = MoveNormalAerialX(
            bus,
            level,
            samus,
            controllerInput,
            movementType: 6);

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

        return FinishVerticalMovement(bus, level, samus, horizontal, nmiFrameCounter);
    }

    private static BlockMoveResult MoveNormalAerialX(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        byte movementType)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectNormalAirSpeedTable();
        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType);

        bool directionHeld = (controllerInput &
            ((ushort)SnesButton.Left | (ushort)SnesButton.Right)) != 0;
        if (speed.AccelerationMode == 0 && !directionHeld)
        {
            // $90:901E/$90:9185 clear both the DP displacement and persistent base speed.
            speed.BaseSpeed = 0;
            speed.BaseSubspeed = 0;
            speed.CalculateTotalSpeed(0);
            return SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, 0);
        }

        int requested = CalculateDirectedDisplacement(bus, samus, calculation.Speed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requested);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed);
        return horizontal;
    }

    private static AerialMovementResult FinishVerticalMovement(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        BlockMoveResult horizontal,
        ushort nmiFrameCounter)
    {
        SamusKinematicsState state = samus.Kinematics;

        // $90:90E5 copies the OLD speed into $12.$14 before gravity changes the stored
        // words. This one-frame lag is observable at jump launch, apex, and terminal speed.
        uint oldSpeed = state.VerticalSpeedFixed;
        if (state.YDirection == 2)
        {
            // The native cap is a whole-word equality test, not >= and not a 16.16 clamp.
            if (state.YSpeed != 5)
                SetVerticalSpeed(state, unchecked(state.VerticalSpeedFixed + Compose(
                    state.YAcceleration,
                    state.YSubacceleration)));
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
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(
            bus,
            level,
            state,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);

        bool hitCeiling = displacement < 0 && vertical.Collided;
        bool landed = displacement >= 0 && vertical.Collided;
        if (hitCeiling)
        {
            // Prospective pose remains the current pose; command five supplies these four
            // writes without inventing a new animation or pose.
            state.YSpeed = 0;
            state.YSubspeed = 0;
            state.YDirection = 2;
        }

        return new AerialMovementResult(horizontal, vertical, landed, hitCeiling);
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
        samus.ReadPoseXDirection(bus) == 4
            ? (controllerInput & (ushort)SnesButton.Left) != 0
            : (controllerInput & (ushort)SnesButton.Right) != 0;

    /// <summary>Block-only port of `$90:9D35`; enemy wall collision remains explicit.</summary>
    private static WallJumpCheckResult CheckBlockWallJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort controllerNewInput)
    {
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
            requested);
        if (!probe.Collided)
            return default;

        ushort distance = unchecked((ushort)Math.Abs(probe.AcceptedDisplacement >> 16));
        if (samus.AnimationFrame < 0x0b)
        {
            samus.ApplyWallContactAnimationRewind();
            return new WallJumpCheckResult(Triggered: false, Contact: true, Distance: distance);
        }

        bool jumpNew = (controllerNewInput & (ushort)SnesButton.A) != 0;
        bool triggered = jumpNew && distance < 8;
        return new WallJumpCheckResult(triggered, Contact: true, distance);
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
            ? speed.CalculateLeftDisplacement(baseSpeed)
            : speed.CalculateRightDisplacement(baseSpeed);
    }

    private static void EnsureNoExtraRunSpeed(SamusHorizontalSpeedState speed)
    {
        if (speed.ExtraRunSpeed != 0 || speed.ExtraRunSubspeed != 0)
        {
            throw new NotSupportedException(
                "Aerial extra-run-speed handling requires the unported run button, speed booster, and momentum flags.");
        }
    }

    private static void ClearHorizontalMomentum(SamusHorizontalSpeedState speed)
    {
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
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
