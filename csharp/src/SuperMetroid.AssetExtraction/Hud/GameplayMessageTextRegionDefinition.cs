namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>One stock text rectangle inside a gameplay notice's content rows.</summary>
/// <param name="Row">Zero-based content tile row, excluding the upper border.</param>
/// <param name="Column">Zero-based starting tile column within the 32-cell content row.</param>
/// <param name="Width">Region width in tile cells, including single interword spaces retained within the glyph run.</param>
/// <param name="Alignment">Text alignment; stock region derivation supplies <see cref="GameplayMessageTextAlignment.Left"/>.</param>
public readonly record struct GameplayMessageTextRegionDefinition(
    int Row,
    int Column,
    int Width,
    GameplayMessageTextAlignment Alignment);
