using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where the cartridge keeps each graphics set's room artwork sources.</summary>
internal static class RoomTilesetSourceDefinitions
{
    /// <summary>$8F:E7A7: one bank-$8F pointer per graphics set to its tileset entry.</summary>
    internal static readonly SnesAddress TilesetTable = new(0x8f, 0xe7a7);

    /// <summary>Bytes into a tileset entry of the long pointer to its compressed block table.</summary>
    internal const int BlocksPointerOffset = 0;

    /// <summary>Bytes into a tileset entry of the long pointer to its compressed character sheet.</summary>
    internal const int CharactersPointerOffset = 3;

    /// <summary>Bytes into a tileset entry of the long pointer to its compressed palette.</summary>
    internal const int PalettePointerOffset = 6;
}
