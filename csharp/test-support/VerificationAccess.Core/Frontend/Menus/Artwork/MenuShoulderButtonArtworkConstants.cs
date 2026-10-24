using SuperMetroid.AssetExtraction.Frontend.Menus;
namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Cartridge values of <see cref="MenuShoulderButtonArtwork"/> that only verification reads.</summary>
internal static class MenuShoulderButtonArtworkConstants
{
    /// <summary>Authored glyph pixels retained: the four-by-four L and R drawings.</summary>
    public const int StoredGlyphPixelCount = 2 * MenuShoulderButtonArtwork.GlyphSize * MenuShoulderButtonArtwork.GlyphSize;

    /// <summary>Provides the stored glyph pixel count used to check menu shoulder button art extraction.</summary>
    extension(MenuShoulderButtonArtwork)
    {
        /// <inheritdoc cref="MenuShoulderButtonArtworkConstants.StoredGlyphPixelCount"/>
        internal static int StoredGlyphPixelCount => MenuShoulderButtonArtworkConstants.StoredGlyphPixelCount;
    }
}
