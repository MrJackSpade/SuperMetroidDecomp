using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// A room and the state its selection chose: the room's fixed definition and the settings of that state.
/// </summary>
/// <remarks>
/// A room's state is chosen from progress as the room loads: its events, bosses and inventory. A new game's Ceres start
/// has none of them, so it takes each room's default state.
/// <see cref="FallingCameraOffset"/> and <see cref="RisingCameraOffset"/> are how many pixels above Samus the camera aims its top as she falls and as she rises.
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

    /// <summary>How many pixels above Samus the camera aims its top while she moves any way but up, leading the view below her as she falls.</summary>
    public int FallingCameraOffset => Definition.FallingCameraOffset;

    /// <summary>How many pixels above Samus the camera aims its top while she moves up, leading the view above her as she rises.</summary>
    public int RisingCameraOffset => Definition.RisingCameraOffset;

    /// <summary>Whether door transitions into or out of this room hide the foreground and drop the colours of the door, item and block pictures every room shares.</summary>
    public bool UsesIsolatedDoorTransition => Definition.UsesIsolatedDoorTransition;

    /// <summary>Selects <paramref name="room"/>'s state for <paramref name="selection"/> from the room definitions.</summary>
    /// <param name="room">The room.</param>
    /// <param name="selection">The progress and equipment the room's state conditions read (#2290).</param>
    public static SelectedRoom ForProgress(
        RoomDefinition room,
        RoomStateSelectionContext selection) =>
        new(Ensure.NotNull(room), RoomStateSelection.Select(room, selection));
}
