using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Rooms;

/// <summary>
/// A room of the game: its name, its place in its area and on the map, its size, the camera's scroll offsets
/// and how door transitions into or out of it draw the screen. Its states are selected by the room's own state choice.
/// </summary>
/// <param name="Name">The room's name.</param>
/// <param name="RoomIndex">The room's index within its area, 0 through 255.</param>
/// <param name="AreaIndex">The room's area.</param>
/// <param name="MapPosition">The room's top-left map tile.</param>
/// <param name="Size">The room's size in scroll screens.</param>
/// <param name="UpScroller">The camera's vertical target offset while Samus moves down, in pixels, 0 through 255.</param>
/// <param name="DownScroller">The camera's vertical target offset while Samus moves up, in pixels, 0 through 255.</param>
/// <param name="DoorTransition">How door transitions into or out of this room draw the screen.</param>
public sealed record RoomDefinition(
    string Name,
    int RoomIndex,
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size,
    int UpScroller,
    int DownScroller,
    RoomDoorTransitionStyle DoorTransition);
