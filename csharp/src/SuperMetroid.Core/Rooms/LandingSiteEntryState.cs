using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The cartridge-defined parts of one door entry into Landing Site that are needed before
/// the first gameplay frame: camera screen and door-selected scrolling-sky transfer.
/// </summary>
public sealed record LandingSiteEntryState(
    ushort DoorPointer,
    byte Direction,
    byte DoorCapXBlock,
    byte DoorCapYBlock,
    byte ScreenX,
    byte ScreenY,
    ushort SpawnDistance,
    ushort DoorAsmPointer,
    int SkySourceAddress,
    ushort SkyVramDestination,
    ushort SkyByteCount,
    RoomIdentity RoomIdentity,
    byte RoomMapX,
    byte RoomMapY,
    byte RoomWidthInScreens,
    byte RoomHeightInScreens,
    byte UpScroller,
    byte DownScroller,
    ushort RoomStatePointer,
    ushort EnemyPopulationPointer,
    ushort EnemyTilesetPointer)
{
    /// <summary>Validated area projected from the logical room identity.</summary>
    public AreaId AreaIndex => RoomIdentity.Area;

    /// <summary>Initial layer-1 X position encoded by the door's screen-X byte.</summary>
    public ushort CameraX => (ushort)(ScreenX << 8);

    /// <summary>Initial layer-1 Y position encoded by the door's screen-Y byte.</summary>
    public ushort CameraY => (ushort)(ScreenY << 8);

    /// <summary>Parses the intro landing-cutscene door and its matching command-E record.</summary>
    public static LandingSiteEntryState LoadLandingCutscene(ISnesAddressSpace bus) =>
        Load(bus, LandingSiteRomData.LandingCutsceneDoorPointer);

    /// <summary>
    /// Uses the compiled door/room definitions and selects its transfer from
    /// <c>LibBG_ScrollingSky_Tilemaps_LandingSite</c> at <c>$8F:B76A</c>.
    /// </summary>
    public static LandingSiteEntryState Load(ISnesAddressSpace bus, ushort doorPointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        CartridgeDoorHeader door = DoorDefinitions.Get(doorPointer);

        // The destination is a bank-$8F room pointer. Rejecting any other room
        // is important: the library-background list below is specific to Landing Site and
        // silently applying it to an arbitrary door would manufacture a plausible image.
        if (door.DestinationRoomPointer != RoomHeaderPointers.LandingSite)
        {
            throw new InvalidDataException(
                $"Door $83:{doorPointer:X4} targets room ${door.DestinationRoomPointer:X4}, " +
                "not Landing Site $91F8.");
        }

        RoomHeaderDefinition room = RoomHeaderDefinitions.Get(RoomHeaderPointers.LandingSite);
        RoomIdentity roomIdentity = new(room.AreaIndex, room.RoomIndex);
        if (roomIdentity != RoomIdentities.LandingSite)
        {
            throw new InvalidDataException(
                $"Landing Site room definition has logical identity {roomIdentity}, expected " +
                $"{RoomIdentities.LandingSite}.");
        }

        ushort statePointer = RoomStateSelectionDefinitions.Select(
            RoomHeaderPointers.LandingSite, default);
        CartridgeRoomState state = RoomStateDefinitions.Get(statePointer);
        SkyTransfer transfer = FindSkyTransfer(bus, doorPointer);
        return new LandingSiteEntryState(
            doorPointer,
            Direction: door.Orientation,
            DoorCapXBlock: door.PlmX,
            DoorCapYBlock: door.PlmY,
            ScreenX: door.DestinationScreenX,
            ScreenY: door.DestinationScreenY,
            SpawnDistance: door.SamusDistance,
            DoorAsmPointer: door.SetupCodePointer,
            transfer.SourceAddress,
            transfer.VramDestination,
            transfer.ByteCount,
            RoomIdentity: roomIdentity,
            RoomMapX: room.MapX,
            RoomMapY: room.MapY,
            RoomWidthInScreens: room.WidthInScreens,
            RoomHeightInScreens: room.HeightInScreens,
            UpScroller: room.UpScroller,
            DownScroller: room.DownScroller,
            RoomStatePointer: state.Pointer,
            EnemyPopulationPointer: state.EnemyPopulationPointer,
            EnemyTilesetPointer: state.EnemyTilesetPointer);
    }

    private static SkyTransfer FindSkyTransfer(ISnesAddressSpace bus, ushort doorPointer)
    {
        int cursor = LandingSiteRomData.LibraryBackgroundListAddress;
        while (true)
        {
            ushort command = ReadWord(bus, cursor);
            if (command == 0)
                break;

            // Landing Site's list consists solely of command Eh records. $82:E9E7 compares
            // DoorPointer with the following word; a match falls through to command 2 and
            // DMAs the subsequent 24-bit source, VRAM word destination, and byte count.
            if (command != 0x000e)
            {
                throw new InvalidDataException(
                    $"Unexpected library-background command ${command:X4} at ${cursor:X6}.");
            }

            ushort candidateDoor = ReadWord(bus, cursor + 2);
            if (candidateDoor == doorPointer)
            {
                int source = ReadLong(bus, cursor + 4);
                ushort destination = ReadWord(bus, cursor + 7);
                ushort byteCount = ReadWord(bus, cursor + 9);
                return new SkyTransfer(source, destination, byteCount);
            }

            // 2-byte command + 2-byte door + 3-byte source + 2-byte destination +
            // 2-byte size. The native nonmatching command-E path likewise advances nine
            // parameter bytes after its command word.
            cursor += 11;
        }

        throw new InvalidDataException(
            $"Landing Site library background has no command-E record for door $83:{doorPointer:X4}.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8));

    private static int ReadLong(ISnesAddressSpace bus, int address) =>
        bus.ReadByte(address) |
        (bus.ReadByte(address + 1) << 8) |
        (bus.ReadByte(address + 2) << 16);

    private readonly record struct SkyTransfer(
        int SourceAddress,
        ushort VramDestination,
        ushort ByteCount);
}
