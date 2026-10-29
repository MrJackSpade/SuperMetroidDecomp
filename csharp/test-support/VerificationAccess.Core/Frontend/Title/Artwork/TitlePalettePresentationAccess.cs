using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Verification access to <see cref="TitlePalettePresentation"/> members production does not use.</summary>
internal static class TitlePalettePresentationAccess
{
    /// <summary>Exposes installed title colors without re-reading the cartridge.</summary>
    extension(TitlePalettePresentation self)
    {
        /// <summary>The selected initial colours in the cartridge's colour order, the native boundary checks compare at.</summary>
        internal ReadOnlySpan<RgbColor> Colors => ScreenColourImage.Encode(self.colours);
    }
}
