using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Reads one native twelve-byte bank-$83 door record during import.</summary>
public static class CartridgeDoorHeaderImporter
{
    /// <summary>Decodes the destination, orientation, entry coordinates, distance, and setup-code pointer from one native bank-$83 door record.</summary>
    /// <param name="bus">Address space implementing the import-only cartridge-source capability.</param>
    /// <param name="pointer">Sixteen-bit door-record address within bank $83.</param>
    /// <returns>The decoded door header, named like its compiled door; no runtime state is changed.</returns>
    public static DoorDefinition Load(ISnesAddressSpace bus, ushort pointer)
    {
        Ensure.NotNull(bus);
        int address = DoorHeaderRomDataTooling.BankAddress | pointer;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte Byte(int offset) => cartridge.ReadCartridgeByte(address + offset);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge, address + offset);
        return new DoorDefinition(DoorHeaderSources.Door(pointer).Name, RoomHeaderSources.Room(Word(0)), DoorRecordCodec.DecodeOrientation(Byte(3)),
            new(Byte(4), Byte(5)), new(Byte(6), Byte(7)), DoorRecordCodec.DecodeSamusSpeed(Word(8)),
            DoorIdentityWords.SetupRoutines.Decode(Word(10), "door setup routine"));
    }
}
