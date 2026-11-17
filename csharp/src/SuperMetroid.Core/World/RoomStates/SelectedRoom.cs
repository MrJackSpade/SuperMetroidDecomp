using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// A room and the state its selection chose: the room's fixed definition and the settings of that state.
/// </summary>
/// <remarks>
/// The game selects the room's state immediately after reading
/// the fixed definition. State selection is data-driven and can depend on events, bosses, or
/// inventory. A new game's Ceres start has none of them, so it takes each room's default state.
/// <see cref="UpScroller"/> and <see cref="DownScroller"/> are the camera's vertical scroller distances in pixels.
/// <see cref="UsesIsolatedDoorTransition"/> is the definition's flag that door transitions read: when the
/// source or destination room sets it, the transition hides the foreground below the HUD and does not keep the colours
/// of the door, item and block pictures every room shares while the source room fades.
/// </remarks>
public sealed record SelectedRoom(RoomDefinition Definition, RoomStateSettings State)
{
    /// <summary>The room's validated world area.</summary>
    public AreaId Area => Definition.Area;

    /// <summary>The room's top-left position on its area map.</summary>
    public Position16<MapTiles> MapPosition => Definition.MapPosition;

    /// <summary>The room's dimensions in screens.</summary>
    public Extent16<RoomScreens> Size => Definition.Size;

    /// <summary>The camera's upward scroller distance in pixels.</summary>
    public int UpScroller => Definition.UpScroller;

    /// <summary>The camera's downward scroller distance in pixels.</summary>
    public int DownScroller => Definition.DownScroller;

    /// <summary>Whether door transitions into or out of this room hide the foreground and drop the colours of the door, item and block pictures every room shares.</summary>
    public bool UsesIsolatedDoorTransition => Definition.UsesIsolatedDoorTransition;

    /// <summary>Selects <paramref name="room"/>'s state for <paramref name="selection"/> from the compiled room definitions.</summary>
    /// <param name="room">The room.</param>
    /// <param name="selection">The progress and equipment the room's state conditions read (#2290).</param>
    public static SelectedRoom LoadUsingCompiledSelection(
        RoomDefinition room,
        RoomStateSelectionContext selection) =>
        new(Ensure.NotNull(room), RoomStateSelection.Select(room, selection));
}
