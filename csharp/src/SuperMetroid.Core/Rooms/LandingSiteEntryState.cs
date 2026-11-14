using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The authored parts of one door entry into Landing Site that are needed before
/// the first gameplay frame: camera screen and door-selected scrolling sky.
/// </summary>
[RoomLifetime]
public sealed record LandingSiteEntryState(
    RoomDefinition Room,
    Position16<MapTiles> RoomMapPosition,
    int UpScroller,
    int DownScroller)
{
    /// <summary>The room's up-scroller distance in pixels; a distance, so never negative.</summary>
    public int UpScroller { get; } = Ensure.AtLeastZero(UpScroller);

    /// <summary>The room's down-scroller distance in pixels; a distance, so never negative.</summary>
    public int DownScroller { get; } = Ensure.AtLeastZero(DownScroller);

    /// <summary>The area of <see cref="Room"/>.</summary>
    public AreaId Area => Room.Area;
}
