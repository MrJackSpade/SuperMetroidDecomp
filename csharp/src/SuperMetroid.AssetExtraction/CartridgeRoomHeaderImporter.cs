using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads native bank-$8F room records for import and cartridge-reference checks.</summary>
public static class CartridgeRoomHeaderImporter
{
    // This is the import/reference decoder. Gameplay uses the compiled room branches;
    // importing native headers must remain independent of those generated definitions.
    private static ushort SelectImportedState(ISnesAddressSpace bus, ushort cursor,
        RoomStateSelectionContext selection)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        ushort Word(ushort pointer) => RomDataReader.ReadWordFixedBank(cartridge, 0x8f0000 | pointer);
        for (int commands = 0; commands < 256; commands++)
        {
            ushort code = Word(cursor);
            cursor = unchecked((ushort)(cursor + 2));
            if (code == RoomStateSelectorCodes.Finish) return cursor;
            bool match;
            switch (code)
            {
                case RoomStateSelectorCodes.EventHasBeenSet:
                    match = selection.IsEventSet(cartridge.ReadCartridgeByte(0x8f0000 | cursor));
                    cursor = unchecked((ushort)(cursor + 1));
                    break;
                case RoomStateSelectorCodes.BossIsDead:
                    match = selection.IsBossDead((BossBits)cartridge.ReadCartridgeByte(0x8f0000 | cursor));
                    cursor = unchecked((ushort)(cursor + 1));
                    break;
                case RoomStateSelectorCodes.MainAreaBossIsDead:
                    match = selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss);
                    break;
                case RoomStateSelectorCodes.MorphBallAndMissiles:
                    match = selection.HasMorphBallAndMissiles;
                    break;
                case RoomStateSelectorCodes.PowerBombs:
                    match = selection.HasPowerBombs;
                    break;
                case RoomStateSelectorCodes.UnusedDoor:
                case RoomStateSelectorCodes.UnusedMorphBall:
                    throw new NotSupportedException($"Unused native room selector $8F:{code:X4}.");
                default:
                    throw new InvalidDataException($"Unknown native room selector $8F:{code:X4}.");
            }
            ushort state = Word(cursor);
            if (match) return state;
            cursor = unchecked((ushort)(cursor + 2));
        }
        throw new InvalidDataException("Native room selector exceeds its bounded command count.");
    }
    public static CartridgeRoomHeader Load(ISnesAddressSpace bus, ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = 0x8f0000 | roomPointer;
        byte Byte(int offset) => bus.ReadCartridgeByte(address + offset);
        ushort doorList = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus), address + 9);
        ushort statePointer = SelectImportedState(bus, unchecked((ushort)(roomPointer + 11)), selection);
        return new CartridgeRoomHeader(roomPointer, Byte(0), (AreaId)Byte(1),
            Byte(2), Byte(3), Byte(4), Byte(5), Byte(6), Byte(7), Byte(8),
            doorList, CartridgeRoomStateImporter.Load(bus, statePointer));
    }
}

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
