using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="GameplayVramLayout"/> that only verification reads.</summary>
internal static class GameplayVramLayoutConstants
{
    /// <summary>BG1SC/BG2SC size bit 0 for rooms: two 32-by-32 screens side by side.</summary>
    public static Extent16<TilemapCells> RoomTilemapSize =>
        new(2 * SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles);
    /// <summary>VMADD $5000-$57FF: BG1's two horizontal 32x32 tilemap screens (BG1SC=$51), the level ring.</summary>
    public static readonly PpuLayoutCell Bg1Tilemap = new(nameof(Bg1Tilemap), PpuLayoutRegion.Vram,
        GameplayVramLayout.Bg1TilemapWord, 0x0800, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);
    /// <summary>VMADD of <see cref="HudTilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort HudTilemapWord = 0x5800;
    /// <summary>VMADD $5800-$5FFF: BG3's tilemap (BG3SC=$5A): the four HUD rows followed by the layer-3 FX screen.</summary>
    public static readonly PpuLayoutCell HudTilemap = new(nameof(HudTilemap), PpuLayoutRegion.Vram,
        HudTilemapWord, 0x0800, "BG3 tilemap words (32x64)", PpuMemoryLayout.Vram);
}
