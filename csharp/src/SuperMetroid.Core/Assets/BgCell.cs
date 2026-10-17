namespace SuperMetroid.Core.Assets;

/// <summary>One 8x8 cell of a background layer: the character it shows and how.</summary>
/// <param name="Character">Character in the layer's artwork.</param>
/// <param name="Palette">Palette row 0..7; a two-bpp row holds four colors, a four-bpp row sixteen. A logical row index,
/// not a register field: <see cref="BgLayer"/> rejects a row outside 0..7 when the cell is bound.</param>
/// <param name="Priority">Whether the cell is in front of the low-priority sprites of its layer.</param>
/// <param name="Flip">The cell's mirroring.</param>
public readonly record struct BgCell(int Character, int Palette, bool Priority, SnesTileFlipFlags Flip)
{
    /// <summary>The cell a native tilemap word names, its character relative to <paramref name="firstCharacter"/>.</summary>
    /// <param name="word">The tilemap word.</param>
    /// <param name="firstCharacter">The character number the layer's artwork starts at.</param>
    public static BgCell FromTilemapWord(SnesBgTilemapWord word, int firstCharacter = 0) =>
        new(word.CharacterIndex - firstCharacter, word.PaletteIndex, word.HasPriority, word.FlipFlags);
}
