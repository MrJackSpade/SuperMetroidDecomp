namespace SuperMetroid.Core.Assets;

/// <summary>Compiled placement and tile words for one ending-text character.</summary>
/// <param name="Row">Zero-based destination tile row.</param>
/// <param name="Column">Zero-based destination tile column.</param>
/// <param name="TopWord">Tilemap word for the glyph's top or only tile.</param>
/// <param name="BottomWord">Optional tilemap word for the glyph's bottom half.</param>
public readonly record struct EndingTextCharacter(
    int Row, int Column, ushort TopWord, ushort? BottomWord);
