using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public static partial class SamusGrappleMovement
{
    /// <summary>Firing branch of native $9B:C490/$B861, before the selected grapple function runs.</summary>
    private static GrappleMovementResult? HandleFiringPoseChange(
        ISnesAddressSpace bus, RoomLevelData level, SamusState samus, ushort controllerInput)
    {
        var grapple = samus.Grapple;
        if (grapple.PoseChangeAutoFireTimer != 0) grapple.PoseChangeAutoFireTimer--;
        byte direction = samus.ReadShotDirection(bus);
        bool banned = bus.ReadByte(SamusGrappleRomData.Firing.BannedMovementTypes +
            (int)samus.ReadMovementType(bus)) != 0;
        if (!banned && (direction & 0xf0) == 0)
        {
            if (direction == grapple.FireDirection) return null;
            if (grapple.PoseChangeAutoFireTimer != 0)
            {
                // Native changes the function pointer before dispatch: restart from
                // the current pose and hand position this frame, without extending.
                // Do not steer an old endpoint or override the pose from held aim keys.
                QueueGrappleSound(samus, SamusGrappleRomData.Sounds.RestartStop);
                grapple.Phase = GrapplePhase.Inactive;
                BeginFiring(bus, samus, controllerInput);
                return new GrappleMovementResult(grapple.Phase, Released: false,
                    ReleaseQueued: false, Fired: grapple.Phase == GrapplePhase.Firing,
                    OwnsMovement: false);
            }
        }
        // Unlike a cancellation requested inside the extension function, this
        // decision precedes dispatch, so the cancel function runs immediately.
        grapple.Phase = GrapplePhase.CancelPending;
        return CompleteFiringCancellation(bus, level, samus);
    }
}
