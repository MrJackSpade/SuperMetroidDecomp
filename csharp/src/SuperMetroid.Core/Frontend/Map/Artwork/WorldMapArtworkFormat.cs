namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Map-world PNG identities, native source transfers, and compiled PPU destinations.</summary>
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
    /// <summary>$81:8DDB LoadInitialMenuTiles transfers $5600 bytes from $8E:8000.</summary>
    public const int ForegroundSource = SuperMetroid.Core.Frontend.FileSelect.FileSelectMapRomData.InitialMenuForegroundCharacters;
    /// <summary>Size in bytes of the native 4-bpp foreground character transfer.</summary>
    public const int ForegroundBytes = 0x5600;
    /// <summary>Number of 32-byte 4-bpp characters in the foreground atlas.</summary>
    public const int ForegroundTileCount = ForegroundBytes / 32;
    /// <summary>Foreground atlas height in pixels.</summary>
    public const int ForegroundHeight = ForegroundTileCount / TileColumns * 8;
    /// <summary>$81:8DDB also installs $600 bytes of 2-bpp BG3 characters from $8E:D600.</summary>
    public const int BackgroundSource = SuperMetroid.Core.Frontend.FileSelect.FileSelectMapRomData.InitialMenuBackgroundCharacters;
    /// <summary>Size in bytes of the native 2-bpp background character transfer.</summary>
    public const int BackgroundBytes = 0x600;
    /// <summary>Number of 16-byte 2-bpp characters in the background atlas.</summary>
    public const int BackgroundTileCount = BackgroundBytes / 16;
    /// <summary>Background atlas height in pixels.</summary>
    public const int BackgroundHeight = BackgroundTileCount / TileColumns * 8;
    /// <summary>Menu BG1 character base in byte-addressed VRAM.</summary>
    public const int ForegroundDestination = 0;
    /// <summary>Menu BG3 character base in byte-addressed VRAM, BG34NBA=$04.</summary>
    public const int BackgroundDestination = 0x8000;
}
