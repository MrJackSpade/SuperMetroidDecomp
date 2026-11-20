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
/// <param name="CameraLead">How far above Samus the camera aims its top as she falls and as she rises (#2482).</param>
/// <param name="DoorTransition">How door transitions into or out of this room draw the screen.</param>
public sealed record RoomDefinition(
    [InstalledKey] string Name,
    AreaId Area,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size,
    RoomCameraLead CameraLead,
    RoomDoorTransitionStyle DoorTransition)
{
    /// <summary>A room with the standard camera lead whose door transitions draw the whole screen.</summary>
    /// <param name="name">The room's name.</param>
    /// <param name="area">The room's area.</param>
    /// <param name="mapPosition">The area-map cell of the room's top-left screen.</param>
    /// <param name="size">The room's size in scroll screens.</param>
    public RoomDefinition([InstalledKey] string name, AreaId area, Position16<MapTiles> mapPosition, Extent16<RoomScreens> size)
        : this(name, area, mapPosition, size, RoomCameraLeads.Standard, RoomDoorTransitionStyle.Standard)
    {
    }

    /// <summary>
    /// Whether the X-ray, where it keeps the room's backgrounds rather than revealing its blocks, also hides the room's
    /// background layer; only the Glass Tunnel does.
    /// </summary>
    public bool XrayHidesBackground { get; init; }
}
