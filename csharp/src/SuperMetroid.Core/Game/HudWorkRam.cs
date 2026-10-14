namespace SuperMetroid.Core.Game;

/// <summary>The gameplay HUD's WRAM tilemap buffer (#629).</summary>
[CpuBusBoundary("catalog: gameplay HUD WRAM")]
public static class HudWorkRam
{
    /// <summary>
    /// <c>$7E:C608</c>, the first byte of <see cref="MutableTilemap"/>. The queued VRAM DMA names it
    /// as a compile-time constant so the static DMA-source audit can prove the transfer's owner.
    /// </summary>
    internal const int MutableTilemapAddress = 0x7ec608;

    /// <summary>
    /// <c>$7E:C608-$7E:C6C7</c>: the three mutable HUD tilemap rows written before the
    /// <c>$80:9CC3-$80:9CE9</c> VRAM queue entry uploads them.
    /// </summary>
    public static readonly CpuBusCell MutableTilemap = CpuBusCell.WramWords(nameof(MutableTilemap), MutableTilemapAddress,
        HudState.MutableTileCount, "BG3 tilemap words (u16[] little-endian)");
}
