using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="AcceptedFrameBoundary"/> members production does not use.</summary>
internal static class AcceptedFrameBoundaryAccess
{
    /// <summary>Exposes <see cref="AcceptedFrameBoundary"/> state that only verification reads.</summary>
    extension(AcceptedFrameBoundary self)
    {
        /// <summary>The effect overlay plane this frame boundary displays (#1348): the room effect it shows; the HUD is its own layer (#1466).</summary>
        internal FxOverlayPlane CaptureFxOverlayPlane() => self.room.FxOverlay?.EffectPlane ?? FxOverlayPlane.Cleared;
    }
}
