using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>WRAM scratch buffers used by the native X-ray tilemap transfers (#629).</summary>
[CpuBusBoundary("catalog: X-ray setup WRAM buffers")]
public static class XrayWorkRam
{
    /// <summary>
    /// <c>$7E:6000-$7E:6FFF</c>: both BG1 tilemap screens captured during X-ray setup. The first
    /// screen (<c>$7E:6000</c>) is read by <c>$91:CB57</c>; the second (<c>$7E:6800</c>) by <c>$91:CB1C</c>.
    /// </summary>
    public static readonly CpuBusCell SavedBg1 = CpuBusCell.WramWords(nameof(SavedBg1), 0x7e6000,
        XrayTilemapLayout.BufferWords, "BG1 tilemap words (u16[] little-endian)");

    /// <summary>
    /// <c>$7E:4000-$7E:4FFF</c>: the frozen reveal BG2 tilemap built by <c>$91:CB8E</c> and
    /// transferred by setup stages six and seven. X-ray reuses the library-background tilemap
    /// staging buffer for it.
    /// </summary>
    public static readonly CpuBusCell RevealTilemap = CpuBusCell.WramWords(nameof(RevealTilemap), 0x7e4000,
        XrayTilemapLayout.BufferWords, "BG2 tilemap words (u16[] little-endian)",
        aliasOf: LibraryBackgroundWorkRam.TilemapStaging);
}

/// <summary>Typed view of the X-ray setup WRAM buffers (#629).</summary>
[CpuBusBoundary("domain view: X-ray setup WRAM buffers")]
public readonly struct XrayMemory(ISnesMemoryBus bus)
{
    /// <summary>The two captured BG1 screens, first screen first.</summary>
    public WorkRamWordTable SavedBg1 => new(bus, XrayWorkRam.SavedBg1);

    /// <summary>The frozen reveal map; it is never rebuilt from later room or VRAM changes.</summary>
    public WorkRamWordTable RevealTilemap => new(bus, XrayWorkRam.RevealTilemap);
}
