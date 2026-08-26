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
    byte AreaIndex,
    byte RoomMapX,
    byte RoomMapY,
    byte RoomWidthInScreens,
    byte RoomHeightInScreens,
    byte UpScroller,
    byte DownScroller)
{
    // Door headers contain 16-bit bank-$83 pointers. $88FE is the synthetic entry used by
    // the intro landing cutscene and is also one of the command-E comparisons at $8F:B76A.
    public const ushort LandingCutsceneDoorPointer = 0x88fe;

    private const int DoorBank = 0x830000;
    private const int LandingSiteRoomHeaderPointer = 0x91f8;
    private const int LandingSiteRoomHeaderAddress = 0x8f91f8;
    private const int LibraryBackgroundListAddress = 0x8fb76a;

    /// <summary>Initial layer-1 X position encoded by the door's screen-X byte.</summary>
    public ushort CameraX => (ushort)(ScreenX << 8);

    /// <summary>Initial layer-1 Y position encoded by the door's screen-Y byte.</summary>
    public ushort CameraY => (ushort)(ScreenY << 8);

    /// <summary>Parses the intro landing-cutscene door and its matching command-E record.</summary>
    public static LandingSiteEntryState LoadLandingCutscene(ISnesAddressSpace bus) =>
        Load(bus, LandingCutsceneDoorPointer);

    /// <summary>
    /// Parses a bank-$83 door header and selects its transfer from
    /// <c>LibBG_ScrollingSky_Tilemaps_LandingSite</c> at <c>$8F:B76A</c>.
    /// </summary>
    public static LandingSiteEntryState Load(ISnesAddressSpace bus, ushort doorPointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int doorAddress = DoorBank | doorPointer;

        // The first word is a bank-$8F destination-room pointer. Rejecting any other room
        // is important: the library-background list below is specific to Landing Site and
        // silently applying it to an arbitrary door would manufacture a plausible image.
        ushort destinationRoom = ReadWord(bus, doorAddress);
        if (destinationRoom != LandingSiteRoomHeaderPointer)
        {
            throw new InvalidDataException(
                $"Door $83:{doorPointer:X4} targets room ${destinationRoom:X4}, not Landing Site $91F8.");
        }

        SkyTransfer transfer = FindSkyTransfer(bus, doorPointer);
        return new LandingSiteEntryState(
            doorPointer,
            Direction: bus.ReadByte(doorAddress + 3),
            DoorCapXBlock: bus.ReadByte(doorAddress + 4),
            DoorCapYBlock: bus.ReadByte(doorAddress + 5),
            ScreenX: bus.ReadByte(doorAddress + 6),
            ScreenY: bus.ReadByte(doorAddress + 7),
            SpawnDistance: ReadWord(bus, doorAddress + 8),
            DoorAsmPointer: ReadWord(bus, doorAddress + 10),
            transfer.SourceAddress,
            transfer.VramDestination,
            transfer.ByteCount,
            // RoomHeader_LandingSite begins with room index, area, map X/Y, dimensions,
            // then the upward/downward camera-scroller distances. Reading these bytes here
            // keeps minimap and camera integration tied to the selected ROM room instead of
            // duplicating visually plausible host constants in the runtime.
            AreaIndex: bus.ReadByte(LandingSiteRoomHeaderAddress + 1),
            RoomMapX: bus.ReadByte(LandingSiteRoomHeaderAddress + 2),
            RoomMapY: bus.ReadByte(LandingSiteRoomHeaderAddress + 3),
            RoomWidthInScreens: bus.ReadByte(LandingSiteRoomHeaderAddress + 4),
            RoomHeightInScreens: bus.ReadByte(LandingSiteRoomHeaderAddress + 5),
            UpScroller: bus.ReadByte(LandingSiteRoomHeaderAddress + 6),
            DownScroller: bus.ReadByte(LandingSiteRoomHeaderAddress + 7));
    }

    private static SkyTransfer FindSkyTransfer(ISnesAddressSpace bus, ushort doorPointer)
    {
        int cursor = LibraryBackgroundListAddress;
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
