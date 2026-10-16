using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Hud;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Bank-$90 movement-type admission to the inactive Grapple HUD handler.</summary>
internal static class SamusGrappleHudInput
{
    /// <summary>Admits a selected Grapple input only for native movement handlers that can fire it in the current pose transition.</summary>
    public static bool IsSelectedAndAdmitted(SamusState samus)
    {
        if (samus.InputLocked || SamusPoseMetadata.IsForwardFacingPose(samus.Pose) ||
            samus.SelectedHudItem != HudItem.Grapple)
            return false;

        SamusHudHandler handler = SamusHudDefinitions.MovementHandler(samus.PoseMetadata.ReadMovementType());
        return handler switch
        {
            SamusHudHandler.Standard or SamusHudHandler.Grapple => true,
            SamusHudHandler.DraygonHeld => samus.PoseMetadata.ReadMovementType() == SamusMovementType.DraygonHeld,
            SamusHudHandler.Turning => samus.PoseTransitionShotDirection != 0,
            SamusHudHandler.Transition => SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose, grappleActive: false),
            SamusHudHandler.MorphBall or SamusHudHandler.Jump => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusHudHandler)} {(int)handler}."),
        };
    }
}
