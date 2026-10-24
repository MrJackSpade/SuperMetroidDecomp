namespace SuperMetroid.Core.Escape;

/// <summary>One glyph the escape typewriter typed (#1348): its tile, and the cell of the overlay its program places it at.</summary>
/// <param name="Cell">The overlay cell the glyph is typed over (#1406).</param>
/// <param name="Tile">The glyph's tilemap word.</param>
public readonly record struct EscapeTypewriterGlyph(int Cell, SnesBgTilemapWord Tile);
