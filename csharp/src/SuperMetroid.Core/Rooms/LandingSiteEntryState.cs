using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The cartridge-defined parts of one door entry into Landing Site that are needed before
/// the first gameplay frame: camera screen and door-selected scrolling-sky transfer.
/// </summary>
public sealed record LandingSiteEntryState(
    RoomIdentity RoomIdentity,
    byte RoomMapX,
    byte RoomMapY,
    byte UpScroller,
    byte DownScroller)
{
    /// <summary>Validated area projected from the logical room identity.</summary>
    public AreaId AreaIndex => RoomIdentity.Area;
}
