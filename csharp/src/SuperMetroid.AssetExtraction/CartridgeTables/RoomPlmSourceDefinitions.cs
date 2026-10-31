using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers room-PLM visual codecs resolve.</summary>
[EncodingOwner("cartridge region of the bank-$84 PLM data")]
internal static class RoomPlmSourceDefinitions
{
    /// <summary>
    /// $84:8000-$84:FFFF: PLM headers, instruction lists and block-draw lists. PLM draw-list,
    /// instruction and selector pointers are bank-local words resolved in this bank.
    /// </summary>
    internal static readonly CpuBusCell PlmBank = CpuBusCell.CartridgeBytes(nameof(PlmBank), 0x848000, 0x8000,
        "PLM headers, instruction lists and draw lists (u16 bank-local pointers)");
}
