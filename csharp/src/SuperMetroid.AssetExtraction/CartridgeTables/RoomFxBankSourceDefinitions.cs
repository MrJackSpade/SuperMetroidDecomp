using SuperMetroid.Core;


namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge banks whose bank-relative pointers the room-effect extractors resolve (#1415).</summary>
[EncodingOwner("cartridge banks of the room-effect tables")]
internal static class RoomFxBankSourceDefinitions
{
    /// <summary>Bank $8A, which holds the layer-three effect tilemaps.</summary>
    internal const int Tilemaps = 0x8a0000;

    /// <summary>Bank $8D, which holds the palette-effect objects, programs and colour records.</summary>
    internal const int PaletteFx = 0x8d0000;
}
