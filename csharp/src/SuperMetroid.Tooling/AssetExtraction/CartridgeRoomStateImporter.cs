using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads one native 26-byte bank-$8F room-state payload during import.</summary>
public static class CartridgeRoomStateImporter
{
    public static CartridgeRoomState Load(ISnesAddressSpace bus, ushort statePointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = 0x8f0000 | statePointer;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte Byte(int offset) => cartridge.ReadCartridgeByte(address + offset);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge, address + offset);
        int Long(int offset) => RomDataReader.ReadLongFixedBank(cartridge, address + offset);
        return new CartridgeRoomState(statePointer, Long(0), Byte(3), Byte(4), Byte(5),
            Word(6), Word(8), Word(10), Byte(12), Byte(13), Word(14), Word(16),
            Word(18), Word(20), Word(22), Word(24));
    }
}
