using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Verification access to <see cref="TitlePalettePresentation"/> members production does not use.</summary>
internal static class TitlePalettePresentationAccess
{
    /// <summary>Exposes installed title colors without re-reading the cartridge.</summary>
    extension(TitlePalettePresentation self)
    {
        /// <summary>Reads the stored RGB5 color span used by the title presentation.</summary>
        internal ReadOnlySpan<RgbColor> Colors => self.colors;
    }
}
