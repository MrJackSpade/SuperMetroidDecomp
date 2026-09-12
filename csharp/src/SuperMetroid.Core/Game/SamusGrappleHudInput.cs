using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$90 movement-type admission to the inactive Grapple HUD handler.</summary>
internal static class SamusGrappleHudInput
{
    public static bool IsSelectedAndAdmitted(ISnesAddressSpace bus, SamusState samus)
    {
        if (samus.InputLocked || SamusState.IsForwardFacingPose(samus.Pose) ||
            samus.SelectedHudItem != SamusHudRomData.GrappleSelectedItem)
            return false;

        ushort handler = SamusHudDefinitions.MovementHandler(samus.ReadMovementType(bus));
        if (handler is SamusHudRomData.StandardHandler or SamusHudRomData.GrappleHandler)
            return true;
        if (handler == SamusHudRomData.DraygonHeldHandler)
            return samus.ReadMovementType(bus) == SamusMovementType.DraygonHeld;
        if (handler == SamusHudRomData.TurningHandler)
            return samus.PoseTransitionShotDirection != 0;
        if (handler != SamusHudRomData.TransitionHandler)
            return false;
        return SamusHudInput.PostureTransitionAdmitsWeapons(bus, samus.Pose, grappleActive: false);
    }
}
