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
public sealed partial record RoomDefinition(
    [InstalledKey] string Name,
    AreaId Area,
    Position16<MapTiles> MapPosition,
    Extent16<RoomScreens> Size)
{
    /// <summary>How far above Samus the camera aims its top as she falls and as she rises (#2482); the standard lead unless the room names its own (#2557).</summary>
    public RoomCameraLead CameraLead { get; init; } = RoomCameraLeads.Standard;

    /// <summary>How door transitions into or out of this room draw the screen; the whole screen unless the room says otherwise (#2557).</summary>
    public RoomDoorTransitionStyle DoorTransition { get; init; } = RoomDoorTransitionStyle.Standard;

    /// <summary>
    /// Whether the X-ray, where it keeps the room's backgrounds rather than revealing its blocks, also hides the room's
    /// background layer; only the Glass Tunnel does.
    /// </summary>
    public bool XrayHidesBackground { get; init; }

    /// <summary>
    /// Whether X-ray never reveals the room's blocks, whatever boss is loaded (#2485): only the Statues room and the Glass
    /// Tunnel.
    /// </summary>
    public bool XrayRevealsNoBlocks { get; init; }

    /// <summary>
    /// Whether the room's quakes are silent (#1415): entering it silences quake sounds as its room effect loads, and every
    /// other room keeps them.
    /// </summary>
    public bool QuakesAreSilent { get; init; }

    /// <summary>
    /// The ground Samus's running footsteps and landings go by in the room (#2416): its area's
    /// (<see cref="RoomGroundDefinitions.AreaGrounds"/>) unless the room declares its own.
    /// </summary>
    internal RoomGround Ground { get; init; } = Ensure.Listed(RoomGroundDefinitions.AreaGrounds, Area);
}
