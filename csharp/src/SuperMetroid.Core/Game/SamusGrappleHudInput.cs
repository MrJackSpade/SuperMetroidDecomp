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

        SamusHudHandler handler = SamusHudDefinitions.MovementHandler(samus.ReadMovementType(bus));
        return handler switch
        {
            SamusHudHandler.Standard or SamusHudHandler.Grapple => true,
            SamusHudHandler.DraygonHeld => samus.ReadMovementType(bus) == SamusMovementType.DraygonHeld,
            SamusHudHandler.Turning => samus.PoseTransitionShotDirection != 0,
            SamusHudHandler.Transition => SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose, grappleActive: false),
            SamusHudHandler.MorphBall or SamusHudHandler.Jump => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusHudHandler)} {(int)handler}."),
        };
    }
}
