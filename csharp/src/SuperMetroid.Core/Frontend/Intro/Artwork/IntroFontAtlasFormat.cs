namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Character geometry of the 144-tile opening font.</summary>
public static class IntroFontAtlasFormat
{
    /// <summary>Planar bit depth of the opening narration font.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Number of eight-by-eight glyph tiles in the atlas.</summary>
    public const int TileCount = 144;
    /// <summary>Width and height of one square glyph tile in pixels.</summary>
    public const int TileSize = 8;
    /// <summary>Byte length of the compiled two-bit planar font.</summary>
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
}
