using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Rooms;

/// <summary>
/// A room of the game: its name, its area and place on the map, its size, the camera's scroll offsets
/// and how door transitions into or out of it draw the screen. Its states are selected by the room's own state choice.
/// The room is its own identity: gameplay names the rooms it treats specially through <see cref="RoomDefinitions"/>,
/// never by a number.
/// </summary>
/// <remarks>
/// Only the catalog builds a room (<see cref="RoomDefinitions"/>, #2553): the constructor and the setters of the members a
/// room may name besides its place are the catalog's, so every room is one of <see cref="RoomDefinitions.All"/> or equal to
/// one, and each room's state choice is total (<see cref="World.RoomStates.RoomStateSelection"/>).
/// </remarks>
public sealed partial record RoomDefinition
{
    /// <summary>Builds one of the catalog's rooms, on its area's ground until the catalog names another.</summary>
    /// <param name="name">The room's name.</param>
    /// <param name="area">The room's area.</param>
    /// <param name="mapPosition">The area-map cell of the room's top-left screen.</param>
    /// <param name="size">The room's size in scroll screens.</param>
    internal RoomDefinition(string name, AreaId area, Position16<MapTiles> mapPosition, Extent16<RoomScreens> size) =>
        Place = new RoomPlace(name, area, mapPosition, size);

    /// <summary>The room's name.</summary>
    [InstalledKey]
    public string Name => Place.Name;

    /// <summary>The room's area.</summary>
    public AreaId Area => Place.Area;

    /// <summary>The area-map cell of the room's top-left screen; each of its screens shows one cell further right or down.</summary>
    public Position16<MapTiles> MapPosition => Place.MapPosition;

    /// <summary>The room's size in scroll screens.</summary>
    public Extent16<RoomScreens> Size => Place.Size;

    /// <summary>Where the room is and the ground it is on.</summary>
    private RoomPlace Place { get; init; }

    /// <summary>How far above Samus the camera aims its top as she falls and as she rises (#2482); the standard lead unless the room names its own (#2557).</summary>
    public RoomCameraLead CameraLead { get; internal init; } = RoomCameraLeads.Standard;

    /// <summary>How door transitions into or out of this room draw the screen; the whole screen unless the room says otherwise (#2557).</summary>
    public RoomDoorTransitionStyle DoorTransition { get; internal init; } = RoomDoorTransitionStyle.Standard;

    /// <summary>
    /// Whether the X-ray, where it keeps the room's backgrounds rather than revealing its blocks, also hides the room's
    /// background layer; only the Glass Tunnel does.
    /// </summary>
    public bool XrayHidesBackground { get; internal init; }

    /// <summary>
    /// Whether X-ray never reveals the room's blocks, whatever boss is loaded (#2485): only the Statues room and the Glass
    /// Tunnel.
    /// </summary>
    public bool XrayRevealsNoBlocks { get; internal init; }

    /// <summary>
    /// Whether the room's quakes are silent (#1415): entering it silences quake sounds as its room effect loads, and every
    /// other room keeps them.
    /// </summary>
    public bool QuakesAreSilent { get; internal init; }

    /// <summary>
    /// The ground Samus's running footsteps and landings go by in the room (#2416): its area's
    /// (<see cref="RoomGroundDefinitions.AreaGrounds"/>) unless the room declares its own.
    /// </summary>
    internal RoomGround Ground
    {
        get => Place.Ground;
        init => Place = Place with { Ground = value };
    }

    /// <summary>A room's name, its area and place on the map and its size, and the ground it is on.</summary>
    /// <param name="Name">The room's name.</param>
    /// <param name="Area">The room's area.</param>
    /// <param name="MapPosition">The area-map cell of the room's top-left screen.</param>
    /// <param name="Size">The room's size in scroll screens.</param>
    private sealed record RoomPlace(string Name, AreaId Area, Position16<MapTiles> MapPosition, Extent16<RoomScreens> Size)
    {
        /// <summary>The ground the room is on: its area's unless the room declares its own.</summary>
        internal RoomGround Ground { get; init; } = RoomGroundDefinitions.AreaGrounds[Area];
    }
}
