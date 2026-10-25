namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of the graphics sets' tileset table that only verification reads.</summary>
internal static class RoomTilesetTableConstants
{
    /// <summary>Offset of the compressed 16x16 block-definition address.</summary>
    public const int BlockDefinitionsAddressOffset = 0;
    /// <summary>Offset of the compressed 4bpp character address.</summary>
    public const int CharacterAddressOffset = 3;
    /// <summary>Bank containing both <see cref="PointerTableAddress"/> and its records.</summary>
    public const int DefinitionBank = 0x8f0000;
    /// <summary>Offset of the compressed palette address.</summary>
    public const int PaletteAddressOffset = 6;
    /// <summary><c>$8F:E7A7 tileset_table</c>, a table of bank-$8F word pointers.</summary>
    public const int PointerTableAddress = 0x8fe7a7;
}
