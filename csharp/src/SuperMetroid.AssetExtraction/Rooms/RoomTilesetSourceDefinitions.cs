using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where the cartridge keeps each graphics set's room artwork sources.</summary>
[EncodingOwner("cartridge addresses of the graphics sets' room artwork")]
internal static class RoomTilesetSourceDefinitions
{
    /// <summary><c>$B9:8000 Tiles_CRE</c>: the compressed common room element characters every Zebes room loads.</summary>
    internal static readonly SnesAddress CreCharacters = new(0xb9, 0x8000);

    /// <summary><c>$B9:A09D TileTable_CRE</c>: the compressed common room element block table.</summary>
    internal static readonly SnesAddress CreBlocks = new(0xb9, 0xa09d);

    /// <summary>$8F:E7A7: one bank-$8F pointer per graphics set to its tileset entry.</summary>
    internal static readonly SnesAddress TilesetTable = new(0x8f, 0xe7a7);

    /// <summary>Bytes into a tileset entry of the long pointer to its compressed block table.</summary>
    internal const int BlocksPointerOffset = 0;

    /// <summary>Bytes into a tileset entry of the long pointer to its compressed character sheet.</summary>
    internal const int CharactersPointerOffset = 3;

    /// <summary>Bytes into a tileset entry of the long pointer to its compressed palette.</summary>
    internal const int PalettePointerOffset = 6;
}
