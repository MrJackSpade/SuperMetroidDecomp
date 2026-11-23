namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Shared pause/file-select character atlas geometry and fixed installed filename.</summary>
public static partial class MapTileAtlasFormat
{
    /// <summary>Asset filename for the shared pause/file-select map character artwork.</summary>
    public const string FileName = "map-tiles.png";
    /// <summary>Thirty-two 8-pixel character columns; character indices advance left to right before proceeding to the next row.</summary>
    public const int TileColumns = 32;
    /// <summary>Required PNG width of 256 pixels, covering the thirty-two character columns.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Required PNG height of 64 pixels, covering eight character rows.</summary>
    public const int Height = 64;
    /// <summary>The eight 8-pixel character rows the atlas's height covers.</summary>
    public const int TileRows = Height / 8;
    /// <summary>The sixteen colour numbers the atlas's characters hold, which the import pre-colours the screen sheets from.</summary>
    public const int ColorCount = 16;
    /// <summary>8192 compiled bytes: 256 characters with 32 planar bytes per 8-by-8 4-bpp character.</summary>
    public const int ByteCount = Width * Height / 2;
}
