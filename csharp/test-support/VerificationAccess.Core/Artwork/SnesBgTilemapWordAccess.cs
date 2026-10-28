namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Verification access to <see cref="SnesBgTilemapWord"/> operations production does not use.</summary>
internal static class SnesBgTilemapWordAccess
{
    /// <summary>Field replacements only verification makes; the word's fields cover all sixteen bits.</summary>
    extension(SnesBgTilemapWord self)
    {
        /// <summary>
        /// Toggles only the requested flip fields, as the XOR performed while expanding a flipped 16x16 room block into
        /// its four 8x8 entries.
        /// </summary>
        public SnesBgTilemapWord ToggleFlips(TileFlip flags) =>
            SnesBgTilemapWord.Create(self.CharacterIndex, self.PaletteIndex, self.HasPriority, self.FlipFlags ^ flags);

        /// <summary>Replaces only the priority bit.</summary>
        public SnesBgTilemapWord WithPriority(bool priority) =>
            SnesBgTilemapWord.Create(self.CharacterIndex, self.PaletteIndex, priority, self.FlipFlags);

        /// <summary>Replaces only the three-bit palette field, preserving character, priority and flips.</summary>
        public SnesBgTilemapWord WithPaletteIndex(int paletteIndex) =>
            SnesBgTilemapWord.Create(self.CharacterIndex, paletteIndex, self.HasPriority, self.FlipFlags);
    }
}
