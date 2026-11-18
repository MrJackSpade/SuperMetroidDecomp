using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Rooms;

/// <summary>
/// A room of the game: its name, its area and place on the map, its size, the camera's scroll offsets
/// and how door transitions into or out of it draw the screen. Its states are selected by the room's own state choice.
/// The room is its own identity: gameplay names the rooms it treats specially through <see cref="RoomDefinitions"/>,
/// never by a number.
/// </summary>
/// <param name="Name">The room's name.</param>
/// <param name="Area">The room's area.</param>
/// <param name="MapPosition">The area-map cell of the room's top-left screen; each of its screens shows one cell further right or down.</param>
/// <param name="Size">The room's size in scroll screens.</param>
/// <param name="FallingCameraOffset">How many pixels above Samus the camera aims its top while she moves any way but up, leading the view below her as she falls (#2482); zero or more.</param>
/// <param name="RisingCameraOffset">How many pixels above Samus the camera aims its top while she moves up, leading the view above her as she rises (#2482); zero or more.</param>
/// <param name="DoorTransition">How door transitions into or out of this room draw the screen.</param>
public sealed record RoomDefinition(
    string Name,
    AreaId Area,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size,
    int FallingCameraOffset,
    int RisingCameraOffset,
    RoomDoorTransitionStyle DoorTransition);
