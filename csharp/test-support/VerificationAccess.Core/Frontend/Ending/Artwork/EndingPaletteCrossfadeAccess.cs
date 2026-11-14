using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Verification access: the logo crossfade read at the installed resource's colour positions (#1464), the native
/// boundary at which checks compare it with the cartridge's crossfade words. The resource holds its steps in order, each
/// step's background colours before its sprite colours; production reads it by step and layer.
/// </summary>
internal static class EndingPaletteCrossfadeAccess
{
    /// <summary>The layers each crossfade step holds.</summary>
    private const int LayersPerStep = 2;

    /// <summary>The colours of the whole crossfade resource.</summary>
    private const int ColourCount = EndingLogoDefinitions.CrossfadeSteps * LayersPerStep * EndingLogoCrossfadeColours.ColoursPerLayer;

    /// <summary>An installed crossfade palette's colours by resource position.</summary>
    extension(EndingPalette self)
    {
        /// <summary>The logo crossfade's colour at resource position <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the crossfade.</exception>
        internal RgbColor CrossfadeColorAt(int index)
        {
            if ((uint)index >= ColourCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            int perStep = LayersPerStep * EndingLogoCrossfadeColours.ColoursPerLayer;
            EndingLogoLayer layer = index % perStep < EndingLogoCrossfadeColours.ColoursPerLayer
                ? EndingLogoLayer.Background : EndingLogoLayer.Sprites;
            return self.LogoCrossfadeColour(index / perStep, layer, index % EndingLogoCrossfadeColours.ColoursPerLayer);
        }
    }
}
