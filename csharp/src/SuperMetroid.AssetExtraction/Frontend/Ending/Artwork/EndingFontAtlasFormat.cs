using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>PNG and native transfer geometry for the 160-tile ending font resource.</summary>
public static class EndingFontAtlasFormat
{
    /// <summary>Installed indexed-PNG resource, <c>ending-font.png</c>, extracted from the native Font3 source at $97:E7DE.</summary>
    public const string FileName = "ending-font.png";
    /// <summary>Four SNES bit planes per font tile, giving a 32-byte transfer per eight-by-eight character.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>Sixteen permitted palette-index values, 0..15; these are glyph ink indices rather than supplied RGB palette colors.</summary>
    public const int ColorCount = 16;
    /// <summary>160 contiguous selected Font3 characters, including small/large letters, symbols, and post-shot subtitle artwork.</summary>
    public const int TileCount = 160;
    /// <summary>Sixteen eight-pixel tile columns per indexed-PNG row, preserving native character order.</summary>
    public const int TilesPerRow = 16;
    /// <summary>Eight pixels along each side of a native square font character; large glyphs occupy vertically adjacent characters.</summary>
    public const int TileSize = 8;
    /// <summary>Indexed-PNG width of 128 pixels, derived from sixteen eight-pixel character columns.</summary>
    public const int Width = TilesPerRow * TileSize;
    /// <summary>Indexed-PNG height of 80 pixels, derived from ten eight-pixel character rows.</summary>
    public const int Height = TileCount / TilesPerRow * TileSize;
    /// <summary>$1400 bytes (5120) in the complete selected 4-bpp transfer: 160 characters at 32 bytes each.</summary>
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
}
