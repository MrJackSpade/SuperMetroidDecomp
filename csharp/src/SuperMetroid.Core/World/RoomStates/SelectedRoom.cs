
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// A retail room and the state its selection chose: the room's fixed definition and the settings of that state.
/// </summary>
/// <remarks>
/// The game selects the room's state immediately after reading
/// the fixed definition. State selection is data-driven and can depend on events, bosses, or
/// inventory. This first reusable loader accepts those facts explicitly; a fresh Ceres
/// start supplies an all-clear context and therefore follows the exact default branch.
/// <c>UpScroller</c> and <c>DownScroller</c> are the camera's vertical scroller distances in pixels, 0 through 255.
/// <c>UsesIsolatedDoorTransition</c> is the definition's common-room-element flag that door transitions read: when the
/// source or destination room sets it, the transition hides the foreground below the HUD and does not keep the common
/// room elements' colours while the source palette fades.
/// </remarks>
public sealed record SelectedRoom(RoomDefinition Definition, RoomStateSettings State)
{
    /// <summary>The room's per-area index, 0 through 255.</summary>
    public int RoomIndex => Definition.RoomIndex;

    /// <summary>The room's validated retail world area.</summary>
    public AreaId Area => Definition.Area;

    /// <summary>The room's top-left position on its area map.</summary>
    public Position16<MapTiles> MapPosition => Definition.MapPosition;

    /// <summary>The room's dimensions in screens.</summary>
    public Extent16<RoomScreens> Size => Definition.Size;

    /// <summary>The camera's upward scroller distance in pixels.</summary>
    public int UpScroller => Definition.UpScroller;

    /// <summary>The camera's downward scroller distance in pixels.</summary>
    public int DownScroller => Definition.DownScroller;

    /// <summary>Whether door transitions into or out of this room hide the foreground and drop the common room elements' colours.</summary>
    public bool UsesIsolatedDoorTransition => Definition.UsesIsolatedDoorTransition;

    /// <summary>The entries the room's door blocks select by index.</summary>
    public IReadOnlyList<DoorListEntry> Doors => RoomDoorListDefinitions.ByRoom[Definition];

    /// <summary>Logical area/room pair.</summary>
    public RoomIdentity Identity => new(Area, RoomIndex);

    /// <summary>Selects <paramref name="room"/>'s state for <paramref name="selection"/> from the compiled room definitions.</summary>
    public static SelectedRoom LoadUsingCompiledSelection(
        RoomDefinition room,
        RoomStateSelectionContext selection = default) =>
        new(Ensure.NotNull(room), RoomStateSelection.Select(room, selection));
}
