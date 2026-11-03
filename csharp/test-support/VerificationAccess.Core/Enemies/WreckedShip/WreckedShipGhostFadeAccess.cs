using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Verification access to <see cref="WreckedShipGhostFade"/> members production does not use.</summary>
internal static class WreckedShipGhostFadeAccess
{
    /// <summary>Exposes <see cref="WreckedShipGhostFade"/> state that only verification reads.</summary>
    extension(WreckedShipGhostFade self)
    {
        /// <summary>The colours the current fade moves toward.</summary>
        public ReadOnlySpan<RgbColor> Target => self.target;
    }
}
