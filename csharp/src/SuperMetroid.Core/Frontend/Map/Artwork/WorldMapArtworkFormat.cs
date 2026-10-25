namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The world map's installed character atlases: their file names and geometry.</summary>
public static class WorldMapArtworkFormat
{
    /// <summary>Indexed PNG containing the shared 4-bpp world-map foreground characters.</summary>
    public const string ForegroundFile = "world-map-foreground.png";
    /// <summary>Indexed PNG containing the shared 2-bpp world-map background characters.</summary>
    public const string BackgroundFile = "world-map-background.png";
    /// <summary>Number of eight-pixel character columns in each exported atlas.</summary>
    public const int TileColumns = 16;
    /// <summary>Atlas width in pixels.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Size in bytes of the native 4-bpp foreground character transfer.</summary>
    public const int ForegroundBytes = 22016;
    /// <summary>Number of 32-byte 4-bpp characters in the foreground atlas.</summary>
    public const int ForegroundTileCount = ForegroundBytes / 32;
    /// <summary>Foreground atlas height in pixels.</summary>
    public const int ForegroundHeight = ForegroundTileCount / TileColumns * 8;
    /// <summary>Size in bytes of the native 2-bpp background character transfer.</summary>
    public const int BackgroundBytes = 1536;
    /// <summary>Number of 16-byte 2-bpp characters in the background atlas.</summary>
    public const int BackgroundTileCount = BackgroundBytes / 16;
    /// <summary>Background atlas height in pixels.</summary>
    public const int BackgroundHeight = BackgroundTileCount / TileColumns * 8;
}
