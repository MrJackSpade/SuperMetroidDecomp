namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The room scroll-zone buffer in WRAM (#629).</summary>
[CpuBusBoundary("catalog: room scroll-zone WRAM")]
public static class RoomScrollWorkRam
{
    /// <summary>
    /// <c>$7E:CD20-$7E:CD51</c> <c>Scrolls</c>: one scroll-zone byte per 256-pixel room screen,
    /// row-major, in the fixed 50-byte native allocation.
    /// </summary>
    public static readonly CpuBusCell Zones = CpuBusCell.WramBytes(nameof(Zones), 0x7ecd20,
        RoomScrollGrid.StorageByteCount, "RoomScrollState[] (u8)");
}
