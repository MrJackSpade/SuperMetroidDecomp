using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed pictures of the room changes of Mother Brain's fake death (#1557), by frame: the wall fill, the escape
/// door, the twelve mural rows, the two fill rows and the six tube clears. Their geometry, collision and timing stay
/// with the draws and the encounter.
/// </summary>
public sealed class InstalledMotherBrainFakeDeathFrames
{
    /// <summary>Holds the pictures the import made of every fake-death room frame.</summary>
    /// <param name="images">Every fake-death room frame's picture.</param>
    internal InstalledMotherBrainFakeDeathFrames(RoomObjectFrameImages<MotherBrainFakeDeathDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every fake-death room frame's picture.</summary>
    internal RoomObjectFrameImages<MotherBrainFakeDeathDraw> Images { get; }
}
