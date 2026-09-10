using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>
    /// Runs UpdateSamusPose's shared expansion check before the interrupted hurt
    /// pose is installed. Crouch-to-hurt must keep the larger body above its floor.
    /// </summary>
    internal bool TryResolveKnockbackPoseCollision(
        ISnesAddressSpace bus, RoomLevelData level, byte targetPose,
        ushort nmiFrameCounter, RoomPlmSystem? plms)
    {
        byte sourcePose = Pose;
        LargerPoseCollisionOutcome outcome = ResolveLargerPoseCollision(
            bus, level, targetPose, nmiFrameCounter, plms, out int adjustment);
        if (outcome != LargerPoseCollisionOutcome.Allowed)
        {
            if (outcome == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + adjustment));
        return true;
    }
}
