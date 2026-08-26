using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Movement for ordinary/aimed crouching and the crouch/stand animation poses.
/// </summary>
/// <remarks>
/// These routines look stationary but are not host no-ops: the cartridge still publishes
/// total X speed, runs horizontal block/slope collision, and performs its downward grounding
/// probe every frame. Keeping those calls makes ledges, slopes, and future conveyors meet
/// the same bank-$94 seam as standing movement.
/// </remarks>
public static class SamusPostureMovement
{
    /// <summary>
    /// Ports movement type five at <c>$90:A573</c> for the complete equal-radius
    /// `$27/$28/$71-$74/$85/$86` crouch family.
    /// </summary>
    public static GroundedMovementResult StepCrouching(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        Validate(bus, level, samus);
        if (!SamusState.IsRightFacingCrouchingPose(samus.Pose) &&
            !SamusState.IsLeftFacingCrouchingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Crouching movement requires pose $27/$28/$71-$74/$85/$86, not ${samus.Pose:X2}.");
        }

        GroundedMovementResult result = MoveWithZeroBaseSpeed(
            bus,
            level,
            samus,
            nmiFrameCounter);

        // $90:A57C-$90:A588 performs this cleanup after both collision passes. It is not
        // inferred from being stationary; these are literal observable WRAM writes.
        ClearHorizontalMomentum(samus.HorizontalSpeed);
        return result;
    }

    /// <summary>
    /// Ports the `$35/$36/$3B/$3C/$F1-$FC` crouch/stand subset of movement type
    /// `$0F` at <c>$90:A61C</c>.
    /// </summary>
    public static GroundedMovementResult StepCrouchStandTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        Validate(bus, level, samus);
        if (!SamusState.IsCrouchStandTransitionPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Crouch/stand transition movement requires pose $35/$36/$3B/$3C/$F1-$FC, not ${samus.Pose:X2}.");
        }

        // All four corresponding entries in $90:A659 are RTS. Unlike movement type five,
        // the wrapper does not clear stored base momentum after moving. That distinction is
        // retained even though ordinary stand/crouch input reaches this code at zero speed.
        return MoveWithZeroBaseSpeed(bus, level, samus, nmiFrameCounter);
    }

    private static GroundedMovementResult MoveWithZeroBaseSpeed(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort nmiFrameCounter)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        if (speed.ExtraRunSpeed != 0 || speed.ExtraRunSubspeed != 0)
        {
            throw new NotSupportedException(
                "Posture transitions with extra run speed require the unported run-button/momentum branch.");
        }

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

        // With zero external displacement and total X speed zero, Simple Samus Y Movement
        // reaches the same +1.0 collision probe used by the grounded standing slice.
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            displacement: 0x00010000,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
        return new GroundedMovementResult(horizontal, vertical);
    }

    private static void ClearHorizontalMomentum(SamusHorizontalSpeedState speed)
    {
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.BaseSpeed = 0;
        speed.BaseSubspeed = 0;
        speed.AccelerationMode = 0;
    }

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
