using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge banks whose bank-relative pointers the room-effect extractors resolve (#1415).</summary>
[CpuBusBoundary("catalog: room-effect cartridge banks")]
internal static class RoomFxCartridgeBanks
{
    /// <summary>Bank $8A, which holds the layer-three effect tilemaps.</summary>
    internal const int Tilemaps = 0x8a0000;

    /// <summary>Bank $8D, which holds the palette-effect objects, programs and colour records.</summary>
    internal const int PaletteFx = 0x8d0000;
}
