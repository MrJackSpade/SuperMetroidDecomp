using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed pictures of the room changes of Mother Brain's fake death (#1557), by draw: the wall fill, the escape
/// door, the twelve mural rows and the six tube clears. Their geometry, collision and timing stay
/// with the draws and the encounter.
/// </summary>
public sealed partial class InstalledMotherBrainFakeDeathFrames : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every fake-death room frame.</summary>
    /// <param name="images">Every fake-death room frame's picture, keyed by its draw.</param>
    internal InstalledMotherBrainFakeDeathFrames(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every fake-death room frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;
}
