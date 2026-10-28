using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Compiled placement and tiles for one ending-text character.</summary>
/// <param name="Row">Zero-based destination tile row.</param>
/// <param name="Column">Zero-based destination tile column.</param>
/// <param name="Top">The glyph's top or only tile.</param>
/// <param name="Bottom">The glyph's bottom half, present only for two-row glyphs.</param>
public readonly record struct EndingTextCharacter(
    int Row, int Column, BackgroundTile Top, BackgroundTile? Bottom);
