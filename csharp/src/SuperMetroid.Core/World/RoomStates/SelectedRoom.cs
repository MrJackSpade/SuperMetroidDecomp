
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// A retail room and the state its selection chose: the room's fixed header and the settings of that state.
/// </summary>
/// <remarks>
/// The engine calls <c>HandleRoomDefStateSelect</c> at $8F:E5D2 immediately after reading
/// the fixed header. State selection is data-driven and can depend on events, bosses, or
/// inventory. This first reusable loader accepts those facts explicitly; a fresh Ceres
/// start supplies an all-clear context and therefore follows the exact default branch.
/// <c>UpScroller</c> and <c>DownScroller</c> are the camera's vertical scroller distances in pixels, 0 through 255.
/// <c>UsesIsolatedDoorTransition</c> is the header's common-room-element bit that door transitions read: when the
/// source or destination room sets it, the transition hides BG1 below the HUD and does not keep the common
/// room elements' colours while the source palette fades.
/// </remarks>
public sealed record SelectedRoom(RoomHeaderDefinition Definition, RoomStateSettings State)
{
    /// <summary>The room's native per-area index, 0 through 255.</summary>
    public int RoomIndex => Definition.RoomIndex;

    /// <summary>The room's validated retail world area.</summary>
    public AreaId AreaIndex => Definition.AreaIndex;

    /// <summary>The room's top-left position on its area map.</summary>
    public Position16<MapTiles> MapPosition => Definition.MapPosition;

    /// <summary>The room's dimensions in screens.</summary>
    public Extent16<RoomScreens> Size => Definition.Size;

    /// <summary>The camera's upward scroller distance in pixels.</summary>
    public int UpScroller => Definition.UpScroller;

    /// <summary>The camera's downward scroller distance in pixels.</summary>
    public int DownScroller => Definition.DownScroller;

    /// <summary>Whether door transitions into or out of this room hide BG1 and drop the common room elements' colours.</summary>
    public bool UsesIsolatedDoorTransition => Definition.UsesIsolatedDoorTransition;

    /// <summary>The entries the room's type-$9 blocks select by index.</summary>
    public IReadOnlyList<DoorListEntry> Doors => RoomDoorListDefinitions.ByRoom[Definition];

    /// <summary>Logical area/room pair.</summary>
    public RoomIdentity Identity => new(AreaIndex, RoomIndex);

    /// <summary>Builds a room entirely from compiled fixed metadata, selection, and state payloads.</summary>
    public static SelectedRoom LoadUsingCompiledSelection(
        RoomHeaderDefinition room,
        RoomStateSelectionContext selection = default) =>
        new(Ensure.NotNull(room), RoomStateSelection.Select(room, selection));
}
