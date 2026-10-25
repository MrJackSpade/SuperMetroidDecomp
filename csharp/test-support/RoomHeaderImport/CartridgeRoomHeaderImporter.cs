using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.TestSupport.RoomHeaderImport;

/// <summary>Reads native bank-$8F room records for import and cartridge-reference checks.</summary>
[CpuBusBoundary("codec: ROM bank-$8F room header and state-selection records → SelectedRoom")]
public static class CartridgeRoomHeaderImporter
{
    /// <summary>Evaluates the native room selector commands for import and reference checks, independently of compiled gameplay branches.</summary>
    private static ushort SelectImportedState(ISnesAddressSpace bus, ushort cursor,
        RoomStateSelectionContext selection)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        ushort Word(ushort pointer) => RomDataReader.ReadWordFixedBank(cartridge, 0x8f0000 | pointer);
        for (int commands = 0; commands < 256; commands++)
        {
            ushort word = Word(cursor);
            cursor = unchecked((ushort)(cursor + 2));
            RoomStateSelectorCode code = ClosedNativeWordDecoding.Decode<RoomStateSelectorCode>(word, "native room selector");
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
    /// <summary>Evaluates the native selector program following the room header at <paramref name="roomPointer"/>, independently of compiled gameplay branches.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <param name="selection">Event, boss, and equipment conditions the selector program tests.</param>
    /// <returns>The bank-$8F pointer of the first matching state, or of the inline default state.</returns>
    public static ushort SelectStatePointer(ISnesAddressSpace bus, ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        Ensure.NotNull(bus);
        return SelectImportedState(bus, unchecked((ushort)(roomPointer + 11)), selection);
    }

    /// <summary>Decodes a native bank-$8F room header and evaluates its bounded state-selection command list independently of compiled gameplay definitions.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <param name="selection">Event, boss, and equipment conditions used to select the first matching native state, or the inline default state.</param>
    /// <returns>The decoded room header and selected room-state payload; no gameplay state is initialized.</returns>
    public static SelectedRoom Load(ISnesAddressSpace bus, ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        Ensure.NotNull(bus);
        int address = 0x8f0000 | roomPointer;
        byte Byte(int offset) => bus.ReadCartridgeByte(address + offset);
        ushort statePointer = SelectStatePointer(bus, roomPointer, selection);
        var definition = new RoomHeaderDefinition(RoomHeaderDefinitions.Get(roomPointer).Name, Byte(0), (AreaId)Byte(1),
            new(Byte(2), Byte(3)), new(Byte(4), Byte(5)), Byte(6), Byte(7), Byte(8));
        return new SelectedRoom(definition, CartridgeRoomStateImporter.Load(bus, statePointer));
    }
}
