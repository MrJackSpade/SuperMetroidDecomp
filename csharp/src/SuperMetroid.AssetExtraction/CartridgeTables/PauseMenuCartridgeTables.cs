using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers pause-menu codecs resolve.</summary>
[CpuBusBoundary("catalog: pause menu cartridge tables")]
internal static class PauseMenuCartridgeTables
{
    /// <summary>
    /// $82:8000-$82:FFFF: pause-menu tilemap data. Equipment-label sources and reserve-screen label
    /// pointers are bank-local words resolved in this bank.
    /// </summary>
    internal static readonly CpuBusCell MenuBank = CpuBusCell.CartridgeBytes(nameof(MenuBank), 0x828000, 0x8000,
        "pause tilemap words (u16 bank-local pointers)");
}
