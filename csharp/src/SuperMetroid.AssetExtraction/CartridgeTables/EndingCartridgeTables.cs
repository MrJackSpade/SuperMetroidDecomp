using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge regions whose bank-local pointers ending codecs resolve.</summary>
[CpuBusBoundary("catalog: ending cartridge tables")]
internal static class EndingCartridgeTables
{
    /// <summary>
    /// $8C:8000-$8C:FFFF: cinematic data including the ending logo crossfade palettes. Logo palette
    /// pointers address the last color of a sixteen-color BGR555 row in this bank.
    /// </summary>
    internal static readonly CpuBusCell LogoPaletteBank = CpuBusCell.CartridgeBytes(nameof(LogoPaletteBank), 0x8c8000, 0x8000,
        "BGR555 palette rows (u16 little-endian)");
}
