namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="GameplayVramLayout"/> that only verification reads.</summary>
internal static class GameplayVramLayoutConstants
{
    /// <summary>BG1SC/BG2SC size bit 0 for rooms: two 32-by-32 screens side by side.</summary>
    public static Extent16<TilemapCells> RoomTilemapSize =>
        new(2 * SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles);
}
