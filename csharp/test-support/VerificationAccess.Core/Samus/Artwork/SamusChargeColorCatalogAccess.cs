using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusChargeColorCatalog"/> members production does not use.</summary>
internal static class SamusChargeColorCatalogAccess
{
    /// <summary>Reads the pseudo-screw or charged-beam suit palette for one phase.</summary>
    extension(SamusChargeColorCatalog self)
    {
        /// <summary>Selects the appropriate charge branch, suit, phase, and color.</summary>
        internal RgbColor ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            (pseudo ? self.pseudoScrew : self.chargedBeam).Resolve(suit, phase, color);
    }
}
