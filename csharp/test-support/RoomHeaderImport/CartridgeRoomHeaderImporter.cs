using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.TestSupport.RoomHeaderImport;

/// <summary>Reads native bank-$8F room records for import and cartridge-reference checks.</summary>
public static class CartridgeRoomHeaderImporter
{
    /// <summary>
    /// The bit of header byte eight that the door transition tests in either adjacent room to hide BG1 below the HUD
    /// and show the objects alone. The byte's other bits select common-element tile reloads, which gameplay does not
    /// read.
    /// </summary>
    private const int IsolatedDoorTransitionBit = 1;

    /// <summary>Evaluates the native room selector commands for import and reference checks, independently of compiled gameplay branches.</summary>
    private static ushort SelectImportedState(ISnesAddressSpace bus, ushort cursor,
        RoomStateSelectionContext selection)
    {
        IImportCartridgeSource cartridge = bus;
        ushort Word(ushort pointer) => RomDataReader.ReadWordFixedBank(cartridge, 0x8f0000 | pointer);
        for (int commands = 0; commands < 256; commands++)
        {
            ushort word = Word(cursor);
            cursor = unchecked((ushort)(cursor + 2));
            RoomStateSelectorCode code = ClosedEnumWordDecoding.Decode<RoomStateSelectorCode>(word, "native room selector");
            bool match;
            switch (code)
            {
                case RoomStateSelectorCode.Finish:
                    return cursor;
                case RoomStateSelectorCode.EventHasBeenSet:
                    match = selection.IsEventSet(SuperMetroid.AssetExtraction.Progression.RoomPlacementDecoding.StoryEvent(cartridge.ReadCartridgeByte(0x8f0000 | cursor)));
                    cursor = unchecked((ushort)(cursor + 1));
                    break;
                case RoomStateSelectorCode.BossIsDead:
                    match = selection.IsBossDead(BossMaskCodec.Decode(cartridge.ReadCartridgeByte(0x8f0000 | cursor), "native room selector boss mask"));
                    cursor = unchecked((ushort)(cursor + 1));
                    break;
                case RoomStateSelectorCode.MainAreaBossIsDead:
                    match = selection.IsBossDead(SuperMetroid.Core.Progression.DefeatedBosses.AreaBoss);
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
    /// <summary>Evaluates the native selector program following the room header at <paramref name="roomPointer"/> for a game with no progress.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <returns>The bank-$8F pointer of the first matching state, or of the inline default state.</returns>
    public static ushort SelectStatePointer(ISnesAddressSpace bus, ushort roomPointer) =>
        SelectStatePointer(bus, roomPointer, RoomStateSelectionContexts.NoProgress);

    /// <summary>Evaluates the native selector program following the room header at <paramref name="roomPointer"/>, independently of compiled gameplay branches.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <param name="selection">Event, boss, and equipment conditions the selector program tests.</param>
    /// <returns>The bank-$8F pointer of the first matching state, or of the inline default state.</returns>
    public static ushort SelectStatePointer(ISnesAddressSpace bus, ushort roomPointer,
        RoomStateSelectionContext selection)
    {
        Ensure.NotNull(bus);
        return SelectImportedState(bus, unchecked((ushort)(roomPointer + 11)), selection);
    }

    /// <summary>
    /// The rows between a room header's map row and the area-map row of the room's top screen: the header names the row one
    /// above it, which the cartridge's map code corrected on every read.
    /// </summary>
    internal const int HeaderMapRowOffset = 1;

    /// <summary>Decodes a native bank-$8F room header and selects its state for a game with no progress.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <returns>The decoded room header and selected room-state payload; no gameplay state is initialized.</returns>
    public static SelectedRoom Load(ISnesAddressSpace bus, ushort roomPointer) =>
        Load(bus, roomPointer, RoomStateSelectionContexts.NoProgress);

    /// <summary>Decodes a native bank-$8F room header and evaluates its bounded state-selection command list independently of compiled gameplay definitions.</summary>
    /// <param name="bus">Address space providing the import-only cartridge source.</param>
    /// <param name="roomPointer">Sixteen-bit room-header address within bank $8F.</param>
    /// <param name="selection">Event, boss, and equipment conditions used to select the first matching native state, or the inline default state.</param>
    /// <returns>
    /// The catalog's room, whose header fields the cartridge's header matches, and the selected room-state payload; no
    /// gameplay state is initialized. Every room is a catalog room (<see cref="RoomDefinition"/>): installed content keyed by
    /// a room, such as its door blocks in each level, is found only for the catalog's, which also names what no header holds
    /// (the room's ground, its silent quakes and its X-ray rules).
    /// </returns>
    /// <exception cref="InvalidDataException">The cartridge's header differs from the catalog room's.</exception>
    public static SelectedRoom Load(ISnesAddressSpace bus, ushort roomPointer,
        RoomStateSelectionContext selection)
    {
        Ensure.NotNull(bus);
        int address = 0x8f0000 | roomPointer;
        byte Byte(int offset) => bus.ReadCartridgeByte(address + offset);
        ushort statePointer = SelectStatePointer(bus, roomPointer, selection);
        RoomDefinition room = RoomDefinitions.Get(roomPointer);
        RequireHeaderField(room, "area", room.Area, AreaNumberCodec.Decode(Byte(1), $"Room header {roomPointer:X4}"));
        RequireHeaderField(room, "map position", room.MapPosition, new Position16<MapTiles>(Byte(2), Byte(3) + HeaderMapRowOffset));
        RequireHeaderField(room, "size", room.Size, new Extent16<RoomScreens>(Byte(4), Byte(5)));
        RequireHeaderField(room, "camera lead", room.CameraLead, new RoomCameraLead(Byte(6), Byte(7)));
        RequireHeaderField(room, "door transition", room.DoorTransition,
            (Byte(8) & IsolatedDoorTransitionBit) != 0 ? RoomDoorTransitionStyle.ObjectsOnly : RoomDoorTransitionStyle.Standard);
        return new SelectedRoom(room, CartridgeRoomStateImporter.Load(bus, statePointer));
    }

    /// <summary>Requires the catalog room's <paramref name="field"/> to be the value its cartridge header decodes to.</summary>
    /// <exception cref="InvalidDataException">The values differ.</exception>
    private static void RequireHeaderField<T>(RoomDefinition room, string field, T compiled, T header)
    {
        if (!EqualityComparer<T>.Default.Equals(compiled, header))
            throw new InvalidDataException($"Room {room.Name}'s {field} is {compiled} in the catalog but {header} in its cartridge header.");
    }
}
