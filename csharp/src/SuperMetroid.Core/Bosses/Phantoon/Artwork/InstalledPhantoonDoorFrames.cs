using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The installed pictures of Phantoon's door frames (#1678), by frame: the fight's door and the restored door frame. The
/// frames' block shapes and the changes' timing stay with their draws and terrain changes.
/// </summary>
public sealed class InstalledPhantoonDoorFrames
{
    /// <summary>Holds the pictures the import made of every door frame.</summary>
    /// <param name="images">Every door frame's picture.</param>
    internal InstalledPhantoonDoorFrames(RoomObjectFrameImages<PhantoonDoorDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every door frame's picture.</summary>
    internal RoomObjectFrameImages<PhantoonDoorDraw> Images { get; }
}
