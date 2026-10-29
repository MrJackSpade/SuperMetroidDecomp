using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusChargeColours"/> by the document's sequence and suit numbering.</summary>
internal static class SamusChargeColoursAccess
{
    /// <summary>Reads the pseudo-screw or charged-beam suit palette for one phase.</summary>
    extension(SamusChargeColours self)
    {
        /// <summary>Selects the appropriate charge sequence, suit by its list place, phase, and color.</summary>
        internal RgbColor ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            self.Resolve(new SamusChargePalette(pseudo ? SamusChargeKind.PseudoScrew : SamusChargeKind.ChargedBeam,
                SamusSuits.AtListPlace(suit), phase), color);
    }
}
