namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The cartridge-defined parts of one door entry into Landing Site that are needed before
/// the first gameplay frame: camera screen and door-selected scrolling-sky transfer.
/// </summary>
[RoomLifetime]
public sealed record LandingSiteEntryState(
    RoomIdentity RoomIdentity,
    Position16<MapTiles> RoomMapPosition,
    int UpScroller,
    int DownScroller)
{
    /// <summary>The room's up-scroller distance in pixels, 0 through 255.</summary>
    public int UpScroller { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, UpScroller);

    /// <summary>The room's down-scroller distance in pixels, 0 through 255.</summary>
    public int DownScroller { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, DownScroller);

    /// <summary>Validated area projected from the logical room identity.</summary>
    public AreaId AreaIndex => RoomIdentity.Area;
}
