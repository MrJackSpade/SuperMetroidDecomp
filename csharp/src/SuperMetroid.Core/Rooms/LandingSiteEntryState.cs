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

    /// <summary>Selects the intro landing-cutscene door and its compiled command-E record.</summary>
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
        LibraryBackgroundInstruction transfer =
            LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                doorPointer);
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
            transfer.Destination,
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

}
