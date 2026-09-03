using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$90 bomb-jump movement handler `$E025-$E094`.
/// </summary>
/// <remarks>
/// A bomb explosion does not substitute a normal jump. Bank $A0 first publishes direction
/// left/straight/right from bomb-versus-Samus X, bank $91 locks pose input, and this special
/// handler owns only the rising part of the arc. At the apex it restores normal movement;
/// the still-active ball pose then performs the downward half through type $08/$12/$13.
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
        int liquidOffset = samus.LiquidPhysics.DetermineMovementMedium(samus) * 2;
        samus.Kinematics.YSpeed = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.BombJumpSpeeds + liquidOffset);
        samus.Kinematics.YSubspeed = ReadWord(
            bus,
            SamusMovementRomData.VerticalMotion.BombJumpSubspeeds + liquidOffset);
        samus.Kinematics.YDirection = 1;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = true;
        return new BombJumpMovementResult(null, null, Started: true, Ended: false);
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
            int displacement = direction == 1
                ? samus.HorizontalSpeed.CalculateLeftDisplacement(
                    baseSpeed,
                    samus.Kinematics.ExtraXFixed)
                : samus.HorizontalSpeed.CalculateRightDisplacement(
                    baseSpeed,
                    samus.Kinematics.ExtraXFixed);
            horizontal = SamusBlockCollision.MoveHorizontal(
                bus,
                level,
                samus.Kinematics,
                displacement,
                plms: plms);
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

        // Native ends immediately once direction becomes down; it does not spend one
        // special-handler frame moving by a zero/negative vertical magnitude.
        if (samus.Kinematics.YDirection == 2)
            return End(samus, horizontal, null);

        BlockMoveResult vertical = MoveUpWithGravity(
            bus,
            level,
            samus,
            nmiFrameCounter,
            plms);
        if (vertical.Collided || horizontal is { Collided: true })
            return End(samus, horizontal, vertical);

        return new BombJumpMovementResult(horizontal, vertical, Started: false, Ended: false);
    }

    private static BlockMoveResult MoveUpWithGravity(
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
        return result;
    }

    private static BombJumpMovementResult End(
        SamusState samus,
        BlockMoveResult? horizontal,
        BlockMoveResult? vertical)
    {
        samus.BombJumpDirection = 0;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = false;
        return new BombJumpMovementResult(horizontal, vertical, Started: false, Ended: true);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Observable output of one special bomb-jump handler frame.</summary>
public readonly record struct BombJumpMovementResult(
    BlockMoveResult? Horizontal,
    BlockMoveResult? Vertical,
    bool Started,
    bool Ended);
