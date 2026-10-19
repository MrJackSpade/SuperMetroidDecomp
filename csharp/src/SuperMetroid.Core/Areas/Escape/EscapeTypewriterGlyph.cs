namespace SuperMetroid.Core.Areas.Escape;

/// <summary>One glyph the escape typewriter typed (#1348): its tile, and the tilemap word its program places it at.</summary>
/// <param name="Destination">The program's VRAM tilemap word for the glyph.</param>
/// <param name="Tile">The glyph's tilemap word.</param>
public readonly record struct EscapeTypewriterGlyph(int Destination, SnesBgTilemapWord Tile);
