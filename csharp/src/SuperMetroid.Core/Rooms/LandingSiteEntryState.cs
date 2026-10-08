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
}
