using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$90 movement-type admission to the inactive Grapple HUD handler.</summary>
internal static class SamusGrappleHudInput
{
    public static bool IsSelectedAndAdmitted(ISnesAddressSpace bus, SamusState samus)
    {
        if (samus.InputLocked || SamusState.IsForwardFacingPose(samus.Pose) ||
            samus.SelectedHudItem != SamusGrappleHudRomData.SelectedItem)
            return false;

        int entry = SamusGrappleHudRomData.MovementHandlers + 2 * (int)samus.ReadMovementType(bus);
        ushort handler = (ushort)(bus.ReadByte(entry) | bus.ReadByte(entry + 1) << 8);
        if (handler is SamusGrappleHudRomData.StandardHandler or SamusGrappleHudRomData.GrappleHandler)
            return true;
        if (handler == SamusGrappleHudRomData.DraygonHeldHandler)
            return samus.ReadMovementType(bus) == SamusMovementType.DraygonHeld;
        if (handler == SamusGrappleHudRomData.TurningHandler)
            return samus.PoseTransitionShotDirection != 0;
        if (handler != SamusGrappleHudRomData.TransitionHandler)
            return false;
        if (samus.Pose >= SamusGrappleHudRomData.StandardTransitionStart)
            return true;
        if (samus.Pose >= SamusGrappleHudRomData.NonFiringTransitionStart)
            return false;
        return bus.ReadByte(SamusGrappleHudRomData.TransitionFlags + samus.Pose -
            SamusGrappleHudRomData.FirstTransitionPose) == 0;
    }
}
