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
        ClearHorizontalMomentum(
            samus.HorizontalSpeed,
            samus.ReadPoseXDirection(bus));
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

        // `$90:9348` does exactly two stores before tail-calling the ordinary horizontal
        // mover: it zeros the *base* displacement pair `$12.$14`. It emphatically does
        // not require extra run speed to be zero. `MoveSamus_Horizontally` subsequently
        // calls `$90:E4E6`, which adds `$0B42.$0B44` to that zero base. This is the retail
        // behavior that lets a stage-four run continue sliding during crouch transition
        // `$35/$36`; bank $91 samples and stores the shine before normal crouching clears
        // that momentum. The old exception at this seam prevented that native route.
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

        // `$90:923F` uses the external-Y replacement when present. Otherwise it couples
        // the downward floor probe to the total X speed calculated by the horizontal pass;
        // this matters for a Speed-Booster slide through `$35/$36`.
        int verticalDisplacement = SamusExtraDisplacement
            .CalculateNoSpeedVerticalDisplacement(samus.Kinematics, speed);
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(
            bus,
            level,
            samus.Kinematics,
            displacement: verticalDisplacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
        return new GroundedMovementResult(horizontal, vertical);
    }

    private static void ClearHorizontalMomentum(
        SamusHorizontalSpeedState speed,
        byte poseXDirection)
    {
        // Every posture route that reaches this full clear is modeling a native momentum
        // cancellation seam, so the `$0B3C/$0B3E` control words must not outlive the
        // numeric X-speed words below.
        speed.CancelRunningMomentum(poseXDirection);
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
