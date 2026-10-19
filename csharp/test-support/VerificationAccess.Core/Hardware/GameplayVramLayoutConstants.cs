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
}
