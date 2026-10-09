using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$90 bomb-jump movement handler `$E025-$E094`.
/// </summary>
/// <remarks>
/// A bomb explosion does not substitute a normal jump. Bank $A0 first publishes direction
/// left/straight/right from bomb-versus-Samus X, bank $91 locks pose input, and this special
/// handler owns only the rising part of the arc. Input returns shortly before the apex;
/// normal movement returns on a direction cancellation, downward turn, or collision.
/// The retained pose can be morphed, humanoid, or damaged rather than always a ball.
/// </remarks>
public static class SamusBombJumpMovement
{
    /// <summary>Runs `$90:E025`, which initializes velocity but deliberately moves zero pixels.</summary>
    public static BombJumpMovementResult Start(ISnesAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        byte direction = unchecked((byte)samus.BombJumpDirection);
        if (direction is < 1 or > 3 || (samus.BombJumpDirection & 0x0800) == 0)
            throw new InvalidOperationException($"Bomb-jump start requires armed direction $0801-$0803, got ${samus.BombJumpDirection:X4}.");

        // `$90:9A2C` uses the same bottom-edge air/water/lava classification as ordinary
        // launch, indexing adjacent table words by zero/two/four. Gravity is refreshed by
        // frame-handler alpha, so this routine replaces only the launch speed/direction.
        (samus.Kinematics.YSpeed, samus.Kinematics.YSubspeed) =
            SamusVerticalMotionDefinitions.BombJump(samus.LiquidPhysics.DetermineMovementMedium(samus));
        samus.Kinematics.YDirection = 1;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = true;
        return new BombJumpMovementResult(null);
    }

    /// <summary>Runs one `$90:E032` main-handler frame.</summary>
    public static BombJumpMovementResult Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem plms)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(plms);
        if (!samus.BombJumpActive)
            throw new InvalidOperationException("Bomb-jump main handler requires an active jump.");

        if (samus.BombJumpDirection == 0)
            return End(samus, null, null);

        byte direction = unchecked((byte)samus.BombJumpDirection);
        if (direction is < 1 or > 3)
        {
            // Native doubles this value into a four-entry pointer table whose zero entry
            // deliberately crashes. Surface corrupted/impossible state instead of turning
            // it into a made-up graceful termination.
            throw new InvalidOperationException(
                $"Active bomb jump has invalid native direction ${samus.BombJumpDirection:X4}.");
        }

        BlockMoveResult? horizontal = null;
        if (direction != 2)
        {
            // `$90:8EF4` bypasses the normal movement-type pointer and feeds `$90:9F25`
            // directly to the deceleration-allowed calculator. Direction one is left;
            // direction three is right. Existing base words intentionally participate.
            uint baseSpeed = samus.HorizontalSpeed.CalculateBaseSpeedAtAddress(
                bus,
            SamusMovementRomData.VerticalMotion.DiagonalBombJumpHorizontalSpeed);
            var displacement = SamusHorizontalDisplacement.Toward(direction == 1, samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                samus.Kinematics,
                displacement.Displacement,
                plms: plms,
                collisionMovementDirection: displacement.CollisionDirection);
            // The native direction-aware X mover clears momentum on a wall hit
            // before the later Y scan replaces its collision result. This must
            // not end the bomb ascent, but it must stop horizontal acceleration.
            if (horizontal.Value.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(samus.ReadFacingDirection(bus));
        }

        // `$90:8F1B` changes signed underflow to the falling direction. A diagonal jump
        // additionally selects mode two so normal horizontal movement decelerates after
        // this special handler relinquishes control.
        if (samus.Kinematics.YDirection == 1 &&
            unchecked((short)samus.Kinematics.YSpeed) < 0)
        {
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YSpeed = 0;
            samus.Kinematics.YDirection = 2;
            if (direction != 2)
                samus.HorizontalSpeed.AccelerationMode = 2;
        }
        else if (samus.Kinematics.YDirection == 1 && samus.Kinematics.YSpeed == 0)
        {
            // The rising helper restores input while speed is still fractional and
            // positive, BEFORE the apex or a collision restores normal movement.
            // A next-frame table match can therefore cancel the remaining bomb rise.
            samus.BombJumpPoseInputLocked = false;
        }

        // Native ends immediately once direction becomes down; it does not spend one
        // special-handler frame moving by a zero/negative vertical magnitude.
        if (samus.Kinematics.YDirection == 2)
            return End(samus, horizontal, null);

        (BlockMoveResult vertical, bool movedDown) = MoveUpWithGravity(
            bus,
            level,
            samus,
            nmiFrameCounter,
            plms);
        // The native handler tests samus_collision_flag after moving Y. That move
        // replaces the horizontal result, so a shaft wall alone must not cancel the
        // upward bomb arc or return control to ordinary Morph Ball movement early.
        if (vertical.Collided)
            return End(samus, horizontal, vertical);

        // At the apex the negated speed is non-negative, so `$90:915E` moves down; finding
        // no floor, `$90:E639` publishes the falling result for the pose pass.
        return new BombJumpMovementResult(vertical, FellWithoutFloor: movedDown);
    }

    /// <summary>Applies one gravity step and moves along the vertical arc, switching to falling when displacement is nonnegative.</summary>
    /// <param name="bus">Address space used by collision and moving-platform checks.</param>
    /// <param name="level">Room geometry against which Samus moves.</param>
    /// <param name="samus">State whose vertical speed and position are advanced.</param>
    /// <param name="nmiFrameCounter">Frame counter used to preserve the native alternating collision scan order.</param>
    /// <param name="plms">Room objects considered during vertical collision resolution.</param>
    /// <returns>The vertical collision result and whether this displacement follows the downward path.</returns>
    private static (BlockMoveResult Result, bool MovedDown) MoveUpWithGravity(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter,
        RoomPlmSystem plms)
    {
        SamusKinematicsState state = samus.Kinematics;
        uint oldSpeed = state.VerticalSpeedFixed;
        uint acceleration = ((uint)state.YAcceleration << 16) | state.YSubacceleration;
        uint nextSpeed = unchecked(oldSpeed - acceleration);
        state.YSpeed = unchecked((ushort)(nextSpeed >> 16));
        state.YSubspeed = unchecked((ushort)nextSpeed);
        int displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            unchecked(-(int)oldSpeed));
        BlockMoveResult result = SamusBlockCollision.MoveVertical(
            bus,
            level,
            state,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            plms: plms);
        if (result.Collided)
        {
            state.YSpeed = 0;
            state.YSubspeed = 0;
            state.YDirection = 2;
        }
        // $90:915C: a non-negative displacement, including the apex's zero, takes MoveSamus_Down.
        return (result, displacement >= 0);
    }

    /// <summary>Clears bomb-jump ownership and returns the vertical collision result that ended the special handler.</summary>
    /// <param name="samus">State whose bomb-jump and pose-input flags are reset.</param>
    /// <param name="horizontal">Horizontal collision result from this frame; termination reports only the vertical result.</param>
    /// <param name="vertical">Vertical collision result to expose in the returned frame result, if one exists.</param>
    /// <returns>A frame result carrying the optional vertical collision.</returns>
    private static BombJumpMovementResult End(
        SamusState samus,
        BlockMoveResult? horizontal,
        BlockMoveResult? vertical)
    {
        samus.BombJumpDirection = 0;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = false;
        samus.BombJumpPoseInputLocked = false;
        return new BombJumpMovementResult(vertical);
    }

}

/// <summary>Observable output of one special bomb-jump handler frame.</summary>
/// <param name="Vertical">Vertical block-movement result produced during the frame, when vertical movement ran.</param>
/// <param name="FellWithoutFloor">Whether the vertical displacement selected downward movement without reporting a floor collision.</param>
public readonly record struct BombJumpMovementResult(
    BlockMoveResult? Vertical,
    bool FellWithoutFloor = false);
