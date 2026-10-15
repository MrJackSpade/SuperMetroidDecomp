
namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Application-owned copy of the fixed eleven-byte metadata preceding a retail room's
/// native state-selection program.
/// </summary>
public sealed record RoomHeaderDefinition(
    ushort Pointer,
    byte RoomIndex,
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size,
    byte UpScroller,
    byte DownScroller,
    byte CreBitset,
    ushort DoorListPointer);
