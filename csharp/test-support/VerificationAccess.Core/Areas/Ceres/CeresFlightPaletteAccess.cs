using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Verification's reading of the Ceres flight palette's background half (#1544): only checks lay it out alone.</summary>
internal static class CeresFlightPaletteAccess
{
    /// <summary>The Ceres flight palette's halves.</summary>
    extension(CeresFlightPalette palette)
    {
        /// <summary>The selected background colours, which the Ceres scenes' floors, caption and planet are pictured in.</summary>
        internal ImmutableArray<RgbColor> BackgroundColours => palette.Colours[..CeresFlightPalette.OwnerColours];
    }
}
