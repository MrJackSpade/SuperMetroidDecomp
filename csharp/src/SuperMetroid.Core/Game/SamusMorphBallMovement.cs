using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal movement for ordinary and Spring-Ball morph poses in bank $90.
/// </summary>
/// <remarks>
/// The cartridge does not treat the ball as a generic circular rigid body. Grounded,
/// falling, bouncing, and transition poses enter different wrappers which share only a few
/// subroutines. This class keeps those entry points separate while reusing their exact
/// 16.16 collision operations. Their air/water/lava tables are translated; bomb displacement,
/// enemies, conveyors, and Spring Ball projectiles remain explicit future branches. Bomb-
/// jump displacement itself lives
/// in <see cref="SamusBombJumpMovement"/> rather than being approximated here.
/// </remarks>
public static class SamusMorphBallMovement
{
    /// <summary>Executes movement type four at <c>$90:A521</c>.</summary>
    public static MorphBallMovementResult StepGrounded(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        Validate(bus, level, samus);
        bool ordinary = SamusState.IsGroundedMorphBallPose(samus.Pose);
        bool spring = SamusState.IsGroundedSpringBallPose(samus.Pose);
        byte movementType = samus.ReadMovementType(bus);
        if ((!ordinary || movementType != 4) && (!spring || movementType != 0x11))
        {
            throw new InvalidOperationException(
                $"Grounded ball movement requires type-$04 or type-$11 pose, not ${samus.Pose:X2}/type ${movementType:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        EnsureNoExtraRunSpeed(speed);

        bool stationaryPose = samus.Pose is
            SamusState.MorphBallGroundRightPose or SamusState.MorphBallGroundLeftPose or
            SamusState.SpringBallGroundRightPose or SamusState.SpringBallGroundLeftPose;
        BlockMoveResult horizontal;
        if (speed.AccelerationMode == 0 && stationaryPose)
        {
            // `$90:A546` passes an explicit zero base magnitude through the ordinary
            // direction-aware displacement path. With no external displacement this is a
            // zero-pixel block scan, but it still publishes total X speed just like SNES.
            int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed: 0);
            horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                samus.Kinematics,
                requested);
            if (horizontal.Collided)
                ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        }
        else
        {
            // Moving `$1E/$1F`, plus either stable pose carrying reversal mode one, enters
            // the full deceleration-allowed `Samus_X_Movement` route with table type four.
            uint baseSpeed = speed.CalculateBaseSpeed(bus, movementType);
            int requested = CalculateDirectedDisplacement(bus, samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                samus.Kinematics,
                requested);
            if (horizontal.Collided)
                ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        }

        BlockMoveResult vertical;
        bool landed = false;
        bool hitCeiling = false;
        if (samus.Kinematics.YDirection != 0)
        {
            // `Simple_Samus_Y_Movement` handles a rebound already in progress through the
            // same falling check and one-frame-lag gravity routine as airborne morph.
            vertical = MoveVerticallyWithGravity(
                bus,
                level,
                samus,
                nmiFrameCounter,
                out landed,
                out hitCeiling);
        }
        else
        {
            vertical = RunNoSpeedCalculationGroundingProbe(
                bus,
                level,
                samus,
                nmiFrameCounter);

            // Only the stable-pose branch performs `$90:A551-$90:A561` cleanup, and only
            // after the no-speed Y path. A moving ball retains its newly calculated speed.
            if (speed.AccelerationMode == 0 && stationaryPose)
                ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        }

        return new MorphBallMovementResult(horizontal, vertical, landed, hitCeiling);
    }

    /// <summary>Executes movement type eight at <c>$90:A5CA</c>.</summary>
    public static MorphBallMovementResult StepFalling(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter)
    {
        Validate(bus, level, samus);
        bool ordinary = SamusState.IsAirborneMorphBallPose(samus.Pose);
        bool spring = samus.Pose is SamusState.SpringBallFallingRightPose or
            SamusState.SpringBallFallingLeftPose;
        byte movementType = samus.ReadMovementType(bus);
        if ((!ordinary || movementType != 8) && (!spring || movementType != 0x13))
        {
            throw new InvalidOperationException(
                $"Falling ball movement requires type-$08 or type-$13 pose, not ${samus.Pose:X2}/type ${movementType:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        EnsureNoExtraRunSpeed(speed);

        bool directionHeld = (controllerInput &
            ((ushort)SnesButton.Left | (ushort)SnesButton.Right)) != 0;
        if (!directionHeld && speed.AccelerationMode == 0)
        {
            // The outer `$90:A5CD` wrapper clears persistent motion before dispatching to
            // either the falling or bouncing subroutine. The inner routine still calculates
            // once, then erases base speed and moves horizontally by zero.
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));
        }

        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType);
        int requestedHorizontal;
        if (!directionHeld && speed.AccelerationMode == 0)
        {
            speed.BaseSpeed = 0;
            speed.BaseSubspeed = 0;
            requestedHorizontal = CalculateDirectedDisplacement(bus, samus, baseSpeed: 0);
        }
        else
        {
            requestedHorizontal = CalculateDirectedDisplacement(bus, samus, calculation.Speed);
        }

        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        // With zero knockback and zero extra Y displacement, `$90:919F` and `$90:91D1`
        // rejoin at the same falling-check/gravity routine. Bounce state changes only when
        // the later bank-$91 solid-vertical collision handler sees a downward collision.
        BlockMoveResult vertical = MoveVerticallyWithGravity(
            bus,
            level,
            samus,
            nmiFrameCounter,
            out bool landed,
            out bool hitCeiling);
        return new MorphBallMovementResult(horizontal, vertical, landed, hitCeiling);
    }

    /// <summary>Executes Spring Ball movement type $12 at <c>$90:A6F1</c>.</summary>
    public static MorphBallMovementResult StepSpringBallInAir(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort controllerInput,
        ushort nmiFrameCounter)
    {
        Validate(bus, level, samus);
        if (samus.Pose is not (SamusState.SpringBallJumpRightPose or SamusState.SpringBallJumpLeftPose) ||
            samus.ReadMovementType(bus) != 0x12)
        {
            throw new InvalidOperationException(
                $"Spring-Ball powered jump requires type-$12 pose $7F/$80, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        speed.SelectEnvironmentSpeedTable(samus.LiquidPhysics.DetermineMovementMedium(samus));
        EnsureNoExtraRunSpeed(speed);

        // `$90:8FDC-$90:8FF9` is the normal variable-height jump cutoff. Releasing Jump
        // while rising cancels the remaining magnitude; signed underflow at the apex takes
        // the same branch even if Jump is still held.
        if (samus.Kinematics.YDirection == 1 &&
            (((controllerInput & (ushort)SnesButton.A) == 0) ||
             unchecked((short)samus.Kinematics.YSpeed) < 0))
        {
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YDirection = 2;
        }

        AerialBaseSpeedResult calculation =
            speed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType: 0x12);
        bool directionHeld = (controllerInput &
            ((ushort)SnesButton.Left | (ushort)SnesButton.Right)) != 0;
        int requestedHorizontal;
        if (speed.AccelerationMode == 0 && !directionHeld)
        {
            // `$90:901E` clears the displacement and persistent base words when no
            // horizontal direction is held; powered vertical motion continues unchanged.
            speed.BaseSpeed = 0;
            speed.BaseSubspeed = 0;
            speed.CalculateTotalSpeed(0);
            requestedHorizontal = 0;
        }
        else
        {
            requestedHorizontal = CalculateDirectedDisplacement(bus, samus, calculation.Speed);
        }

        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical = MoveVerticallyWithGravity(
            bus,
            level,
            samus,
            nmiFrameCounter,
            out bool landed,
            out bool hitCeiling);
        return new MorphBallMovementResult(horizontal, vertical, landed, hitCeiling);
    }

    /// <summary>
    /// Executes the `$37/$38/$3D/$3E` subset of transition movement type $0F at
    /// <c>$90:A61C</c>, including its airborne gravity branch.
    /// </summary>
    public static MorphBallMovementResult StepTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        Validate(bus, level, samus);
        if (!SamusState.IsMorphTransitionPose(samus.Pose) || samus.ReadMovementType(bus) != 0x0f)
        {
            throw new InvalidOperationException(
                $"Morph transition movement requires type-$0F pose $37/$38/$3D/$3E, not ${samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        EnsureNoExtraRunSpeed(speed);

        // `$90:A635` deliberately supplies zero base speed but still uses pose direction,
        // collision, and the already-published total-speed words.
        int requestedHorizontal = CalculateDirectedDisplacement(bus, samus, baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            samus.Kinematics,
            requestedHorizontal);
        if (horizontal.Collided)
            ClearHorizontalMomentum(speed, samus.ReadPoseXDirection(bus));

        BlockMoveResult vertical;
        bool landed = false;
        bool hitCeiling = false;
        if (samus.Kinematics.YDirection == 0)
        {
            vertical = RunNoSpeedCalculationGroundingProbe(
                bus,
                level,
                samus,
                nmiFrameCounter);
        }
        else
        {
            vertical = MoveVerticallyWithGravity(
                bus,
                level,
                samus,
                nmiFrameCounter,
                out landed,
                out hitCeiling);
            if (landed)
            {
                // `$90:A640` recognizes the ordinary landed result and cancels every word
                // which could make command $F9 choose the airborne endpoint afterward.
                samus.Kinematics.YSubspeed = 0;
                samus.Kinematics.YSpeed = 0;
                samus.Kinematics.YDirection = 0;
                samus.MorphBallBounceState = 0;
            }
        }

        return new MorphBallMovementResult(horizontal, vertical, landed, hitCeiling);
    }

    private static BlockMoveResult MoveVerticallyWithGravity(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        out bool landed,
        out bool hitCeiling)
    {
        SamusKinematicsState state = samus.Kinematics;

        // `$90:90C4` converts the signed underflow produced at the prior apex frame before
        // this frame snapshots displacement. Direction is a separate word in the original;
        // do not infer it from the sign of the fixed-point magnitude.
        if (state.YDirection == 1 && unchecked((short)state.YSpeed) < 0)
        {
            state.YSubspeed = 0;
            state.YSpeed = 0;
            state.YDirection = 2;
        }

        // `$90:90E5` moves by the OLD magnitude, then updates stored speed for next frame.
        // This one-frame lag is observable on both bounce launch and the falling speed cap.
        uint oldSpeed = Compose(state.YSpeed, state.YSubspeed);
        uint acceleration = Compose(state.YAcceleration, state.YSubacceleration);
        uint newSpeed;
        if (state.YDirection == 2)
        {
            newSpeed = state.YSpeed == 5
                ? oldSpeed
                : unchecked(oldSpeed + acceleration);
        }
        else
        {
            newSpeed = unchecked(oldSpeed - acceleration);
        }
        state.YSpeed = unchecked((ushort)(newSpeed >> 16));
        state.YSubspeed = unchecked((ushort)newSpeed);

        int displacement = state.YDirection == 2
            ? unchecked((int)oldSpeed)
            : unchecked(-(int)oldSpeed);
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(
            bus,
            level,
            state,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);

        hitCeiling = displacement < 0 && vertical.Collided;
        landed = displacement >= 0 && vertical.Collided;
        if (hitCeiling)
        {
            state.YSpeed = 0;
            state.YSubspeed = 0;
            state.YDirection = 2;
        }
        return vertical;
    }

    private static BlockMoveResult RunNoSpeedCalculationGroundingProbe(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        uint totalSpeed = Compose(speed.TotalSpeed, speed.TotalSubspeed);
        int displacement = samus.Kinematics.PositionAdjustedBySlope
            ? 0x00010000
            : unchecked((int)(totalSpeed + 0x00010000u));
        return SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
    }

    private static int CalculateDirectedDisplacement(
        ISnesAddressSpace bus,
        SamusState samus,
        uint baseSpeed)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        byte direction = samus.ReadPoseXDirection(bus);

        // `$90:8EA9` reverses pose direction only for mode one. That is what preserves old
        // travel during a `$1E <-> $1F` turn; mode two uses the new facing normally.
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
                "Ordinary Morph-Ball movement with newly introduced extra run speed requires the run-button/speed-booster branch.");
        }
    }

    private static void ClearHorizontalMomentum(
        SamusHorizontalSpeedState speed,
        byte poseXDirection)
    {
        // Block collision and grounded stop paths call the native full momentum cleanup;
        // keep its flag/counter writes adjacent to the five visible X-motion words.
        speed.CancelRunningMomentum(poseXDirection);
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.BaseSpeed = 0;
        speed.BaseSubspeed = 0;
        speed.AccelerationMode = 0;
    }

    private static uint Compose(ushort high, ushort low) => ((uint)high << 16) | low;

    private static void Validate(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
    }
}

/// <summary>Collision and direction result produced by one ordinary Morph-Ball frame.</summary>
public readonly record struct MorphBallMovementResult(
    BlockMoveResult Horizontal,
    BlockMoveResult Vertical,
    bool Landed,
    bool HitCeiling);
