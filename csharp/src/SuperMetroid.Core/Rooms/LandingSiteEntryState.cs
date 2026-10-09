using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The cartridge-defined parts of one door entry into Landing Site that are needed before
/// the first gameplay frame: camera screen and door-selected scrolling-sky transfer.
/// </summary>
/// <param name="RoomIdentity">Logical identity of the Landing Site room being entered.</param>
/// <param name="RoomMapX">Room's horizontal map coordinate used to position the camera screen.</param>
/// <param name="RoomMapY">Room's vertical map coordinate used to position the camera screen.</param>
/// <param name="UpScroller">Scrolling-sky transfer value selected for upward movement through the entry door.</param>
/// <param name="DownScroller">Scrolling-sky transfer value selected for downward movement through the entry door.</param>
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
