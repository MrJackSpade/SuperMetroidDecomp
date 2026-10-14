namespace SuperMetroid.Core.Assets;

/// <summary>Defines the map sprite document, atlas geometry, and native transfer sizes.</summary>
public static class MapSpriteFormat
{
    /// <summary>Supported map sprite schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON composition file for map markers and world labels.</summary>
    public const string JsonFile = "map-sprites.json";
    /// <summary>Indexed PNG file for shared map object characters.</summary>
    public const string PngFile = "map-objects.png";
    /// <summary>Sixteen eight-pixel character columns in the map object atlas.</summary>
    public const int TileColumns = 16;
    /// <summary>Sixteen eight-pixel character rows in the map object atlas.</summary>
    public const int TileRows = 16;
    /// <summary>Atlas width derived from its character columns.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Atlas height derived from its character rows.</summary>
    public const int Height = TileRows * 8;
    /// <summary>Largest supported number of ordered OBJ parts in one composition.</summary>
    public const int MaximumParts = 128;
    /// <summary>$B6:C000 shared menu object characters, transferred to the active menu's OBJ base.</summary>
    public const int SourceAddress = MapTileAtlasRomData.MenuObjectCharacterData;
    /// <summary>Compiled byte count of the 128-by-128-pixel 4-bpp atlas.</summary>
    public const int ByteCount = Width * Height / 2;
    /// <summary>File-select OBSEL=$03 places shared menu OBJ characters at VRAM byte $C000.</summary>
    public const int FileSelectDestination = 0xc000;
    /// <summary>Pause OBSEL=$01 places the same shared characters at VRAM byte $4000.</summary>
    public const int PauseDestination = 0x4000;
}
