namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The bank-$7E staging buffer the bank-$82 library-background commands fill (#629).</summary>
[CpuBusBoundary("catalog: library-background WRAM staging")]
public static class LibraryBackgroundWorkRamAddresses
{
    /// <summary>
    /// <c>$7E:4000-$7E:4FFF</c>: the BG tilemap staging buffer that library-background command
    /// <c>DecompressToWorkRam</c> unpacks into and the clear-BG2/clear-FX commands fill, before a
    /// queued DMA uploads it.
    /// </summary>
    public static readonly CpuBusCell TilemapStaging = CpuBusCell.WramBytes(nameof(TilemapStaging), 0x7e4000, 0x1000,
        "BG tilemap words (u8[] little-endian)");
}
