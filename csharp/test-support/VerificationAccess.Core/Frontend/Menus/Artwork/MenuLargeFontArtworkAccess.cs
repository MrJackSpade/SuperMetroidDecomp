namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Verification access to <see cref="MenuLargeFontArtwork"/> members production does not use.</summary>
internal static class MenuLargeFontArtworkAccess
{
    /// <summary>Inspects large-font face bytes and sparse pixel edits.</summary>
    extension(MenuLargeFontArtwork self)
    {
        /// <summary>Measures the stored font-face payload before pixel overrides.</summary>
        internal int StoredFaceByteCount => self.faces.Length;

        /// <summary>Counts explicit large-font pixel edits.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;

        /// <summary>Checks the sparse edit key for a valid tile and eight-by-eight pixel coordinate.</summary>
        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuLargeFontArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return self.edits is { } edits && edits.ContainsKey(tile * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}
