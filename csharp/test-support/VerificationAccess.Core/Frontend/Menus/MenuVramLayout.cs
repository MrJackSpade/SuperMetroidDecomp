using SuperMetroid.Core.Frontend.FileSelect;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The shared menu VRAM allocation installed by <c>LoadInitialMenuTiles</c> ($81:8DDB) and used
/// by file select, options and game over (#630).
/// </summary>
internal static class MenuVramLayout
{
    /// <summary>VMADD of the menu BG1 32x32 tilemap, for render packets and the PPU backend.</summary>
    public const ushort Bg1TilemapWord = 0x5000;

    /// <summary>VMADD of the menu BG2 32x32 tilemap, for render packets and the PPU backend.</summary>
    public const ushort Bg2TilemapWord = 0x5800;

    /// <summary>VMADD $3000-$3FFF: the shared map characters file select and the room map draw with.</summary>
    public static readonly PpuLayoutCell MapCharacters = new(nameof(MapCharacters), PpuLayoutRegion.Vram,
        FileSelectMapNativeSources.RoomCharacters, SuperMetroid.Core.Map.Artwork.MapTileAtlasFormat.ByteCount / sizeof(ushort), "4-bpp BG characters (map atlas)",
        PpuMemoryLayout.Vram);

    /// <summary>VMADD $6000-$6FFF (byte $C000): the map-object OBJ characters uploaded from <c>$B6:C000</c>.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        MapSpriteByteAddress / sizeof(ushort), SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteFormat.ByteCount / sizeof(ushort),
        "4-bpp OBJ characters (map objects)", PpuMemoryLayout.Vram);

    /// <summary>File-select OBSEL=$03 places the shared menu OBJ characters at VRAM byte $C000.</summary>
    internal const int MapSpriteByteAddress = 0xc000;
}
