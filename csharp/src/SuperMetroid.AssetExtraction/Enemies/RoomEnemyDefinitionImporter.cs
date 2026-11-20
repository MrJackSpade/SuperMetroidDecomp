using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads native 64-byte bank-$A0 enemy headers during cartridge import.</summary>
[EncodingOwner("cartridge enemy header words")]
public static class RoomEnemyDefinitionImporter
{

    /// <summary>The tile set the bank-$A0 header at <paramref name="headerPointer"/> names.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="headerPointer">The header's bank-$A0 pointer; a verification fixture may seed one for an identity no member has.</param>
    internal static EnemyHeaderTileSet TileSetAt(ISnesAddressSpace bus, int headerPointer)
    {
        Ensure.NotNull(bus);
        int tileSize = ReadWord(bus, headerPointer, 0);
        return new EnemyHeaderTileSet(tileSize & EnemySourceDefinitions.EnemyHeaderTileByteCountBits);
    }

    /// <summary>The long address of the planar tile data the bank-$A0 header at <paramref name="headerPointer"/> names.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="headerPointer">The header's bank-$A0 pointer.</param>
    internal static int TileDataSourceAt(ISnesAddressSpace bus, int headerPointer)
    {
        Ensure.NotNull(bus);
        return RomDataReader.ReadLongFixedBank(bus, HeaderAddress(headerPointer, TileDataOffset));
    }

    /// <summary>Header offset of the 24-bit tile data source.</summary>
    private const int TileDataOffset = 54;

    /// <summary>The long address of the sixteen-color palette the bank-$A0 header at <paramref name="headerPointer"/> names.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="headerPointer">The header's bank-$A0 pointer.</param>
    internal static int PaletteAddressAt(ISnesAddressSpace bus, int headerPointer)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = bus;
        return (int)new SnesAddress(ReadByte(cartridge, headerPointer, BankOffset), ReadWord(cartridge, headerPointer, PaletteOffset));
    }

    /// <summary>Header offset of the bank byte that qualifies the header's code and palette words.</summary>
    private const int BankOffset = 12;

    /// <summary>Header offset of the palette word.</summary>
    private const int PaletteOffset = 2;

    /// <summary>The long address of the byte <paramref name="offset"/> bytes into the bank-$A0 header at <paramref name="header"/>.</summary>
    private static int HeaderAddress(int header, int offset) =>
        EnemySourceDefinitions.EnemyHeaderBank | unchecked((ushort)(header + offset));

    /// <summary>Reads the little-endian header word <paramref name="offset"/> bytes into the header at <paramref name="header"/>.</summary>
    private static ushort ReadWord(IImportCartridgeSource cartridge, int header, int offset) =>
        RomDataReader.ReadWordFixedBank(cartridge, HeaderAddress(header, offset));

    /// <summary>Reads the header byte <paramref name="offset"/> bytes into the header at <paramref name="header"/>.</summary>
    private static byte ReadByte(IImportCartridgeSource cartridge, int header, int offset) =>
        cartridge.ReadCartridgeByte(HeaderAddress(header, offset));
}
