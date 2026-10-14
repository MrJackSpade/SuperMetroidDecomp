namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The shared menu VRAM allocation installed by <c>LoadInitialMenuTiles</c> ($81:8DDB) and used
/// by file select, options and game over (#630).
/// </summary>
[PpuLayoutBoundary("catalog: shared menu VRAM")]
public static class MenuVramLayout
{
    /// <summary>VMADD of <see cref="Bg1Tilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg1TilemapWord = 0x5000;

    /// <summary>VMADD of <see cref="Bg2Tilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg2TilemapWord = 0x5800;

    /// <summary>VMADD $5000-$53FF: the menu BG1 32x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg1Tilemap = new(nameof(Bg1Tilemap), PpuLayoutRegion.Vram,
        Bg1TilemapWord, SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5000-$57FF: the file-select room map's 64x32 area-map tilemap, spilling past the menu BG1 page.</summary>
    public static readonly PpuLayoutCell AreaMapTilemap = new(nameof(AreaMapTilemap), PpuLayoutRegion.Vram,
        Bg1TilemapWord, 2 * SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $3000-$3FFF: the shared map characters file select and the room map draw with.</summary>
    public static readonly PpuLayoutCell MapCharacters = new(nameof(MapCharacters), PpuLayoutRegion.Vram,
        FileSelectMapRomData.RoomCharacters, Assets.MapTileAtlasFormat.ByteCount / sizeof(ushort), "4-bpp BG characters (map atlas)",
        PpuMemoryLayout.Vram);

    /// <summary>VMADD $5C00-$5FFF (menu BG3SC=$5C): the world map's area-background tilemap.</summary>
    public static readonly PpuLayoutCell AreaBackgroundTilemap = new(nameof(AreaBackgroundTilemap), PpuLayoutRegion.Vram,
        FileSelectMapRomData.AreaBackgroundVram, SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $6000-$6FFF (byte $C000): the map-object OBJ characters uploaded from <c>$B6:C000</c>.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        Assets.MapSpriteFormat.FileSelectDestination / sizeof(ushort), Assets.MapSpriteFormat.ByteCount / sizeof(ushort),
        "4-bpp OBJ characters (map objects)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5800-$5BFF: the menu BG2 32x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg2Tilemap = new(nameof(Bg2Tilemap), PpuLayoutRegion.Vram,
        Bg2TilemapWord, SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);
}
