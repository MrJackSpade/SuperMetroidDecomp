namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Compiled placement and tilemap entries for one ending-text character.</summary>
/// <param name="Row">Zero-based destination tile row.</param>
/// <param name="Column">Zero-based destination tile column.</param>
/// <param name="TopWord">Tilemap entry for the glyph's top or only tile.</param>
/// <param name="BottomWord">Tilemap entry for the glyph's bottom half, present only for two-row glyphs.</param>
public readonly record struct EndingTextCharacter(
    int Row, int Column, SnesBgTilemapWord TopWord, SnesBgTilemapWord? BottomWord);
