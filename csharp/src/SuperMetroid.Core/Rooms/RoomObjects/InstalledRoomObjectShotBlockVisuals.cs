namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the ordinary shot blocks' breakup and restoration frames (#1557). Their collision,
/// respawn and timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectShotBlockVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every shot-block frame.</summary>
    /// <param name="images">Every shot-block frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectShotBlockVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every shot-block frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
