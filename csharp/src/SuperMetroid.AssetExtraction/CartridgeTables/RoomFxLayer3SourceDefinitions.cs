using SuperMetroid.Core.Hardware;
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores the room-effect tables the effect extractors read.</summary>
internal static class RoomFxLayer3SourceDefinitions
{
    /// <summary>$83:ABF0, table of layer-three effect tilemap pointers.</summary>
    internal static readonly SnesAddress Layer3TilemapPointers = new(0x83, 0xabf0);
}
