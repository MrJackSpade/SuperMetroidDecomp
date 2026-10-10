using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$90 movement-type admission to the inactive Grapple HUD handler.</summary>
internal static class SamusGrappleHudInput
{
    /// <summary>Checks whether the selected Grapple HUD item is admitted by the current bank-$90 movement handler.</summary>
    /// <param name="bus">The address space used to read Samus's current movement type.</param>
    /// <param name="samus">Samus's input-lock, pose, selected-item, and transition state.</param>
    /// <returns><see langword="true"/> when the current posture and handler allow the Grapple HUD action.</returns>
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
        return SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose, grappleActive: false);
    }
}
