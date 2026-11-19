namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the downward gates' column frames and coloured triggers (#1557). Their terrain, shot
/// triggers and timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectDownwardGateVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every downward-gate frame.</summary>
    /// <param name="images">Every downward-gate frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectDownwardGateVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every downward-gate frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
