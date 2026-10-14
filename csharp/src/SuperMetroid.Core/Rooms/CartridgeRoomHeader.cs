using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Lossless cartridge view of bank $8F's eleven-byte room header and its selected
/// twenty-six-byte room-state record.
/// </summary>
/// <remarks>
/// The engine calls <c>HandleRoomDefStateSelect</c> at $8F:E5D2 immediately after reading
/// the fixed header. State selection is data-driven and can depend on events, bosses, or
/// inventory. This first reusable loader accepts those facts explicitly; a fresh Ceres
/// start supplies an all-clear context and therefore follows the exact default branch.
/// </remarks>
public sealed record CartridgeRoomHeader(
    ushort Pointer,
    byte RoomIndex,
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size,
    byte UpScroller,
    byte DownScroller,
    byte CreBitset,
    ushort DoorListPointer,
    CartridgeRoomState State)
{
    /// <summary>Logical area/room pair, kept separate from <see cref="Pointer"/>.</summary>
    public RoomIdentity Identity => new(AreaIndex, RoomIndex);

    /// <summary>Builds a room entirely from compiled fixed metadata, selection, and state payloads.</summary>
    public static CartridgeRoomHeader LoadUsingCompiledSelection(
        ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        RoomHeaderDefinition header = RoomHeaderDefinitions.Get(roomPointer);
        ushort statePointer = RoomStateSelectionDefinitions.Select(roomPointer, selection);
        return new CartridgeRoomHeader(
            header.Pointer,
            header.RoomIndex,
            header.AreaIndex,
            header.MapPosition,
            header.Size,
            header.UpScroller,
            header.DownScroller,
            header.CreBitset,
            header.DoorListPointer,
            RoomStateDefinitions.Get(statePointer));
    }

    /// <summary>Returns a compiled fixed header's area without reading native room data.</summary>
    public static AreaId ReadAreaIndex(ushort roomPointer) =>
        RoomHeaderDefinitions.Get(roomPointer).AreaIndex;

}
