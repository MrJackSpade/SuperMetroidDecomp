using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads one native 26-byte bank-$8F room-state payload during import.</summary>
public static class CartridgeRoomStateImporter
{
    /// <summary>Decodes one 26-byte bank-$8F room-state payload, including its 24-bit level-data address and native graphics, audio, population, and setup fields.</summary>
    /// <param name="bus">Address space implementing the import-only cartridge-source capability.</param>
    /// <param name="statePointer">Sixteen-bit payload address within bank $8F, not the room header or state-selection list.</param>
    /// <returns>The decoded definition retaining its native state pointer; no room selection or runtime setup is performed.</returns>
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
