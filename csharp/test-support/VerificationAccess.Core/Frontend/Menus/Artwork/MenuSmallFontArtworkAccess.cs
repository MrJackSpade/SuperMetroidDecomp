using SuperMetroid.AssetExtraction.Frontend.Menus;
namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Verification access to <see cref="MenuSmallFontArtwork"/> members production does not use.</summary>
internal static class MenuSmallFontArtworkAccess
{
    /// <summary>Inspects small-font face bytes and sparse pixel edits.</summary>
    extension(MenuSmallFontArtwork self)
    {
        /// <summary>Measures the stored small-font face payload.</summary>
        internal int StoredFaceByteCount => self.faces.Length;

        /// <summary>Counts explicit small-font pixel edits.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;

        /// <summary>Checks the sparse edit key relative to the small font's first tile.</summary>
        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuSmallFontArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return self.edits is { } edits && edits.ContainsKey((tile - MenuSmallFontArtwork.FirstTile) * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}
