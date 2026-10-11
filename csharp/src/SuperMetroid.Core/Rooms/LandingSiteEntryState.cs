using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The cartridge-defined parts of one door entry into Landing Site that are needed before
/// the first gameplay frame: camera screen and door-selected scrolling-sky transfer.
/// </summary>
public sealed record LandingSiteEntryState(
    RoomIdentity RoomIdentity,
    Position16<MapTiles> RoomMapPosition,
    byte UpScroller,
    byte DownScroller)
{
    /// <summary>Validated area projected from the logical room identity.</summary>
    public AreaId AreaIndex => RoomIdentity.Area;
}
