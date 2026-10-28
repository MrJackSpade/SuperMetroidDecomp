using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Hud;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Bank-$90 movement-type admission to the inactive Grapple HUD handler.</summary>
internal static class SamusGrappleHudInput
{
    /// <summary>Admits a selected Grapple input only for native movement handlers that can fire it in the current pose transition.</summary>
    public static bool IsSelectedAndAdmitted(SamusState samus, InstalledSamusOverreads? overreads)
    {
        if (samus.Control.AlphaLocked || SamusPoseMetadata.IsForwardFacingPose(samus.Pose) ||
            samus.SelectedHudItem != HudItem.Grapple)
            return false;

        SamusHudHandler handler = SamusHudObservations.MovementHandler(SamusPoseMetadata.ReadMovementType(samus));
        return handler switch
        {
            SamusHudHandler.Standard or SamusHudHandler.Grapple => true,
            SamusHudHandler.DraygonHeld => SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.DraygonHeld,
            SamusHudHandler.Turning => samus.PoseTransitionShotDirection != 0,
            SamusHudHandler.Transition => SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose, grappleActive: false, overreads),
            SamusHudHandler.MorphBall or SamusHudHandler.Jump => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusHudHandler)} {handler}."),
        };
    }
}
