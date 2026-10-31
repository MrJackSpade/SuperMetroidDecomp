using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores the room-effect tables the effect extractors read.</summary>
[EncodingOwner("cartridge addresses of the layer-three effect tables")]
internal static class RoomFxLayer3SourceDefinitions
{
    /// <summary>$83:ABF0, table of layer-three effect tilemap pointers.</summary>
    internal static readonly SnesAddress Layer3TilemapPointers = new(0x83, 0xabf0);

    /// <summary>$8A:8000, the first of the six contiguous layer-three effect tilemap pages; the pages end at $8A:B17F.</summary>
    internal static readonly SnesAddress Layer3TilemapPages = new(0x8a, 0x8000);

    /// <summary>$89:AA02, the colour words blended into the temporary effect palette entries.</summary>
    internal static readonly SnesAddress PaletteBlendColors = new(0x89, 0xaa02);
}
