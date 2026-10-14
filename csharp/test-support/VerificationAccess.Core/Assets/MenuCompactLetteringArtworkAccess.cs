using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="MenuCompactLetteringArtwork"/> members production does not use.</summary>
internal static class MenuCompactLetteringArtworkAccess
{
    /// <summary>Inspects glyph ink storage and sparse compact-letter pixel edits.</summary>
    extension(MenuCompactLetteringArtwork self)
    {
        /// <summary>Measures one 32-bit ink value per stored glyph.</summary>
        internal int StoredInkByteCount => self.glyphs.Count * sizeof(uint);

        /// <summary>Counts explicit compact-letter pixel edits.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;

        /// <summary>Checks the sparse edit key for a valid tile and eight-by-eight pixel coordinate.</summary>
        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuCompactLetteringArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return self.edits is { } edits && edits.ContainsKey(tile * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}
