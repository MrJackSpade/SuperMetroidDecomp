using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Hud;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Whether a movement type admits the selected, inactive Grapple.</summary>
internal static class SamusGrappleHudInput
{
    /// <summary>Admits a selected Grapple input only for movement types that can fire it in the current pose transition.</summary>
    public static bool IsSelectedAndAdmitted(SamusState samus, PostureAdmissionBeyondTransitions<SamusPoseId> postureAdmission)
    {
        if (samus.Control.InputLocked || SamusPoseMetadata.IsForwardFacingPose(samus.Pose) ||
            samus.SelectedHudItem != HudItem.Grapple)
            return false;

        SamusHudSelectionMode mode = SamusHudObservations.SelectionMode(SamusPoseMetadata.ReadMovementType(samus));
        return mode switch
        {
            SamusHudSelectionMode.Standard or SamusHudSelectionMode.Grapple => true,
            SamusHudSelectionMode.DraygonHeld => SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.DraygonHeld,
            SamusHudSelectionMode.Turning => samus.PendingTransitionShot is not null,
            SamusHudSelectionMode.Transition => SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose, grappleActive: false, postureAdmission),
            SamusHudSelectionMode.MorphBall or SamusHudSelectionMode.Jump => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusHudSelectionMode)} {mode}."),
        };
    }
}
