using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads one native twelve-byte bank-$83 door record during import.</summary>
public static class CartridgeDoorHeaderImporter
{
    public static CartridgeDoorHeader Load(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = DoorHeaderRomData.BankAddress | pointer;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte Byte(int offset) => cartridge.ReadCartridgeByte(address + offset);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge, address + offset);
        return new CartridgeDoorHeader(pointer, Word(0), Byte(2), Byte(3), Byte(4),
            Byte(5), Byte(6), Byte(7), Word(8), Word(10));
    }
}
