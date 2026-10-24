using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The pause screen's VRAM allocation (#630). Pause keeps its own PPU image; its BG3 plane
/// retains the gameplay HUD rows at the gameplay HUD tilemap's location.
/// </summary>
[PpuLayoutBoundary("catalog: pause screen VRAM")]
[NumberedIdentities("BG1, BG2 and BG3 are the PPU's distinct background layers, each with its own pause-screen role and size: the 64x32 map or equipment page, the 32x32 frame and the 32x64 HUD plane. The number names the hardware layer, not a position.")]
internal static class PauseVramLayout
{
    /// <summary>VMADD $0000-$0FFF: the shared map characters.</summary>
    public static readonly PpuLayoutCell MapCharacters = new(nameof(MapCharacters), PpuLayoutRegion.Vram,
        0x0000, SuperMetroid.Core.Map.Artwork.MapTileAtlasFormat.ByteCount / sizeof(ushort), "4-bpp BG characters (map atlas)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $1000-$1FFF: the pause interface characters uploaded from the pause UI atlas.</summary>
    public static readonly PpuLayoutCell InterfaceCharacters = new(nameof(InterfaceCharacters), PpuLayoutRegion.Vram,
        SuperMetroid.Core.Frontend.Pause.Artwork.PauseTileAtlasFormatConstants.DestinationByte / sizeof(ushort), SuperMetroid.Core.Frontend.Pause.Artwork.PauseTileAtlasFormat.ByteCount / sizeof(ushort),
        "4-bpp BG characters (pause interface)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $2000-$2FFF (byte $4000): the map-object OBJ characters uploaded from <c>$B6:C000</c>.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        MapSpriteByteAddress / sizeof(ushort), SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteFormat.ByteCount / sizeof(ushort),
        "4-bpp OBJ characters (map objects)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $3000-$37FF: the map and equipment pages' BG1 64x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg1Tilemap = new(nameof(Bg1Tilemap), PpuLayoutRegion.Vram,
        PauseMenuLayoutConstants.Bg1TilemapWord, 0x0800, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $3800-$3BFF: the pause frame's BG2 32x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg2Tilemap = new(nameof(Bg2Tilemap), PpuLayoutRegion.Vram,
        PauseMenuLayoutConstants.Bg2TilemapWord, 0x0400, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>
    /// VMADD $5800-$5FFF: the BG3 tilemap (SetupPPUForPauseMenu sets BG3SC=$58): gameplay's retained HUD rows, then the FX
    /// rows <c>QueueClearingOfFxTilemap</c> ($80:A211) fills.
    /// </summary>
    public static readonly PpuLayoutCell Bg3Tilemap = new(nameof(Bg3Tilemap), PpuLayoutRegion.Vram,
        GameplayVramLayoutConstants.HudTilemapWord, 0x0800, "BG3 tilemap words (32x64)", PpuMemoryLayout.Vram);

    /// <summary>Pause OBSEL=$01 places the shared menu OBJ characters at VRAM byte $4000.</summary>
    internal const int MapSpriteByteAddress = 0x4000;
}
