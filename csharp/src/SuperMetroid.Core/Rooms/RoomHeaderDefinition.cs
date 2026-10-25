using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Application-owned copy of the fixed eleven-byte metadata preceding a retail room's
/// native state-selection program.
/// </summary>
/// <param name="Name">The room's name.</param>
/// <param name="RoomIndex">The room's index within its area, 0 through 255.</param>
/// <param name="AreaIndex">The room's area.</param>
/// <param name="MapPosition">The room's top-left map tile.</param>
/// <param name="Size">The room's size in scroll screens.</param>
/// <param name="UpScroller">The camera's vertical target offset while Samus moves down, in pixels, 0 through 255.</param>
/// <param name="DownScroller">The camera's vertical target offset while Samus moves up, in pixels, 0 through 255.</param>
/// <param name="CreBitset">The room's common-element settings bits, 0 through 255 (<see cref="LevelData.RoomCreBitsets"/>).</param>
public sealed record RoomHeaderDefinition(
    string Name,
    int RoomIndex,
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size,
    int UpScroller,
    int DownScroller,
    int CreBitset);
