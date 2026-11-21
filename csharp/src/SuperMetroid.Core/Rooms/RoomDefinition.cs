using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Rooms;

/// <summary>
/// A room of the game: its name, its area and place on the map, its size, the camera's scroll offsets
/// and how door transitions into or out of it draw the screen. Its states are selected by the room's own state choice.
/// The room is its own identity: gameplay names the rooms it treats specially through <see cref="RoomDefinitions"/>,
/// never by a number.
/// </summary>
/// <remarks>
/// Only the catalog builds a room (<see cref="RoomDefinitions"/>, #2553): the constructor is the catalog's, and no member
/// can be changed by a copy, so every room is one of <see cref="RoomDefinitions.All"/> or equal to one, and each room's
/// state choice is total (<see cref="World.RoomStates.RoomStateSelection"/>).
/// </remarks>
public sealed record RoomDefinition
{
    /// <summary>Builds one of the catalog's rooms.</summary>
    /// <param name="name">The room's name.</param>
    /// <param name="area">The room's area.</param>
    /// <param name="mapPosition">The area-map cell of the room's top-left screen.</param>
    /// <param name="size">The room's size in scroll screens.</param>
    /// <param name="fallingCameraOffset">How many pixels above Samus the camera aims its top while she moves any way but up.</param>
    /// <param name="risingCameraOffset">How many pixels above Samus the camera aims its top while she moves up.</param>
    /// <param name="doorTransition">How door transitions into or out of this room draw the screen.</param>
    /// <param name="xrayHidesBackground">Whether the X-ray, keeping the room's backgrounds, also hides its background layer.</param>
    internal RoomDefinition(string name, AreaId area, Position16<MapTiles> mapPosition, Extent16<RoomScreens> size,
        int fallingCameraOffset, int risingCameraOffset, RoomDoorTransitionStyle doorTransition, bool xrayHidesBackground = false)
    {
        Name = name;
        Area = area;
        MapPosition = mapPosition;
        Size = size;
        FallingCameraOffset = fallingCameraOffset;
        RisingCameraOffset = risingCameraOffset;
        DoorTransition = doorTransition;
        XrayHidesBackground = xrayHidesBackground;
    }

    /// <summary>The room's name.</summary>
    [InstalledKey]
    public string Name { get; }

    /// <summary>The room's area.</summary>
    public AreaId Area { get; }

    /// <summary>The area-map cell of the room's top-left screen; each of its screens shows one cell further right or down.</summary>
    public Position16<MapTiles> MapPosition { get; }

    /// <summary>The room's size in scroll screens.</summary>
    public Extent16<RoomScreens> Size { get; }

    /// <summary>
    /// How many pixels above Samus the camera aims its top while she moves any way but up, leading the view below her as she
    /// falls (#2482); zero or more.
    /// </summary>
    public int FallingCameraOffset { get; }

    /// <summary>
    /// How many pixels above Samus the camera aims its top while she moves up, leading the view above her as she rises
    /// (#2482); zero or more.
    /// </summary>
    public int RisingCameraOffset { get; }

    /// <summary>How door transitions into or out of this room draw the screen.</summary>
    public RoomDoorTransitionStyle DoorTransition { get; }

    /// <summary>
    /// Whether the X-ray, where it keeps the room's backgrounds rather than revealing its blocks, also hides the room's
    /// background layer; only the Glass Tunnel does.
    /// </summary>
    public bool XrayHidesBackground { get; }
}
