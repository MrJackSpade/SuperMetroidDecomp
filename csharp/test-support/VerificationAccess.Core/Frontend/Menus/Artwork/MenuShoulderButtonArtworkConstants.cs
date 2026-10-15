namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Cartridge values of <see cref="MenuShoulderButtonArtwork"/> that only verification reads.</summary>
internal static class MenuShoulderButtonArtworkConstants
{
    /// <summary>Two 16-bit tile words stored for each menu shoulder-button glyph.</summary>
    public const int StoredGlyphByteCount = 2 * sizeof(ushort);

    /// <summary>Provides the stored glyph byte count used to check menu shoulder button art extraction.</summary>
    extension(MenuShoulderButtonArtwork)
    {
        /// <inheritdoc cref="MenuShoulderButtonArtworkConstants.StoredGlyphByteCount"/>
        internal static int StoredGlyphByteCount => MenuShoulderButtonArtworkConstants.StoredGlyphByteCount;
    }
}
