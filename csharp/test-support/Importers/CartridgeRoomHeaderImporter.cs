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
            ushort word = Word(cursor);
            cursor = unchecked((ushort)(cursor + 2));
            RoomStateSelectorCode code = ClosedNativeWords.Decode<RoomStateSelectorCode>(word, "native room selector");
            bool match;
            switch (code)
            {
                case RoomStateSelectorCode.Finish:
                    return cursor;
                case RoomStateSelectorCode.EventHasBeenSet:
                    match = selection.IsEventSet(cartridge.ReadCartridgeByte(0x8f0000 | cursor));
                    cursor = unchecked((ushort)(cursor + 1));
                    break;
                case RoomStateSelectorCode.BossIsDead:
                    match = selection.IsBossDead((BossBits)cartridge.ReadCartridgeByte(0x8f0000 | cursor));
                    cursor = unchecked((ushort)(cursor + 1));
                    break;
                case RoomStateSelectorCode.MainAreaBossIsDead:
                    match = selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss);
                    break;
                case RoomStateSelectorCode.MorphBallAndMissiles:
                    match = selection.HasMorphBallAndMissiles;
                    break;
                case RoomStateSelectorCode.PowerBombs:
                    match = selection.HasPowerBombs;
                    break;
                case RoomStateSelectorCode.UnusedDoor:
                case RoomStateSelectorCode.UnusedMorphBall:
                    throw new NotSupportedException($"Unused native room selector $8F:{word:X4}.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(RoomStateSelectorCode)} {word:X4}.");
            }
            ushort state = Word(cursor);
            if (match) return state;
            cursor = unchecked((ushort)(cursor + 2));
        }
        throw new InvalidDataException("Native room selector exceeds its bounded command count.");
    }
    /// <summary>Decodes a native bank-$8F room header and evaluates its bounded state-selection command list independently of compiled gameplay definitions.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <param name="selection">Event, boss, and equipment conditions used to select the first matching native state, or the inline default state.</param>
    /// <returns>The decoded room header and selected room-state payload; no gameplay state is initialized.</returns>
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
