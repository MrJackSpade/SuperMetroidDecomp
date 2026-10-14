namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Shared pause/file-select character atlas geometry and fixed installed filename.</summary>
public static class MapTileAtlasFormat
{
    /// <summary>Asset filename for the shared pause/file-select map character artwork.</summary>
    public const string FileName = "map-tiles.png";
    /// <summary>Thirty-two 8-pixel character columns; character indices advance left to right before proceeding to the next row.</summary>
    public const int TileColumns = 32;
    /// <summary>Required PNG width of 256 pixels, covering the thirty-two character columns.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Required PNG height of 64 pixels, covering eight character rows.</summary>
    public const int Height = 64;
    /// <summary>Sixteen permitted pixel indices for the 4-bpp characters; runtime CGRAM palettes are installed separately.</summary>
    public const int ColorCount = 16;
    /// <summary>8192 compiled bytes: 256 characters with 32 planar bytes per 8-by-8 4-bpp character.</summary>
    public const int ByteCount = Width * Height / 2;
    /// <summary>$B6:8000: first 256 shared map/menu 4-bpp characters, imported once rather than reread at runtime.</summary>
    public const int SourceAddress = MapTileAtlasRomData.CharacterData;
}
