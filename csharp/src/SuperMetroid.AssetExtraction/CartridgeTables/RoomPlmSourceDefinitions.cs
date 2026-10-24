using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers room-PLM visual codecs resolve.</summary>
[CpuBusBoundary("catalog: room PLM cartridge tables")]
internal static class RoomPlmSourceDefinitions
{
    /// <summary>
    /// $84:8000-$84:FFFF: PLM headers, instruction lists and block-draw lists. PLM draw-list,
    /// instruction and selector pointers are bank-local words resolved in this bank.
    /// </summary>
    internal static readonly CpuBusCell PlmBank = CpuBusCell.CartridgeBytes(nameof(PlmBank), 0x848000, 0x8000,
        "PLM headers, instruction lists and draw lists (u16 bank-local pointers)");

    /// <summary>
    /// $89:8000-$89:FFFF: item PLM graphics. A dynamic collectible's graphics pointer addresses its
    /// 4-bpp tiles in this bank.
    /// </summary>
    internal static readonly CpuBusCell ItemGraphicsBank = CpuBusCell.CartridgeBytes(nameof(ItemGraphicsBank), 0x898000, 0x8000,
        "item PLM 4-bpp tiles (u8[])");

    /// <summary>$89:8000 ItemPLMGFX_Bombs: the first item artwork block; every block owns eight 32-byte tiles.</summary>
    internal const ushort FirstItemGraphic = 0x8000;
    /// <summary>Bytes in one item artwork block.</summary>
    internal const int ItemGraphicStride = 0x100;
}
