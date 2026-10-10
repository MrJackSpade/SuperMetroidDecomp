using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads one native twelve-byte bank-$83 door record during import.</summary>
public static class CartridgeDoorHeaderImporter
{
    /// <summary>Decodes the destination, orientation, entry coordinates, distance, and setup-code pointer from one native bank-$83 door record.</summary>
    /// <param name="bus">Address space implementing the import-only cartridge-source capability.</param>
    /// <param name="pointer">Sixteen-bit door-record address within bank $83.</param>
    /// <returns>The decoded door header retaining its native pointer identity; no runtime state is changed.</returns>
    public static CartridgeDoorHeader Load(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = DoorHeaderRomDataTooling.BankAddress | pointer;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte Byte(int offset) => cartridge.ReadCartridgeByte(address + offset);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge, address + offset);
        return new CartridgeDoorHeader(pointer, Word(0), CartridgeDoorOrientation.Decode(Byte(3)), Byte(4),
            Byte(5), Byte(6), Byte(7), Word(8),
            ClosedNativeWords.Decode<DoorSetupCode>(Word(10), "door setup routine"));
    }
}
