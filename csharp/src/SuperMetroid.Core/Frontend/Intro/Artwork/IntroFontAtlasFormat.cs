namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>PNG and native transfer geometry for the 144-tile opening font.</summary>
public static class IntroFontAtlasFormat
{
    /// <summary>Canonical opening-font PNG file name.</summary>
    public const string FileName = "intro-font.png";
    /// <summary>SNES planar bit depth of the opening narration font.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Required number of indexed PNG colors.</summary>
    public const int ColorCount = 4;
    /// <summary>Number of eight-by-eight glyph tiles in the atlas.</summary>
    public const int TileCount = 144;
    /// <summary>Number of glyph tiles in each atlas row.</summary>
    public const int TilesPerRow = 16;
    /// <summary>Width and height of one square glyph tile in pixels.</summary>
    public const int TileSize = 8;
    /// <summary>Required PNG width in pixels.</summary>
    public const int Width = TilesPerRow * TileSize;
    /// <summary>Required PNG height in pixels.</summary>
    public const int Height = TileCount / TilesPerRow * TileSize;
    /// <summary>Byte length of the compiled SNES two-bit planar transfer.</summary>
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
}
