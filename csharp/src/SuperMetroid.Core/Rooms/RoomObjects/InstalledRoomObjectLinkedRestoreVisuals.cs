namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the bomb and contact-crumble restoration frames (#1557). The frames' terrain keeps its
/// linked parent and child collision.
/// </summary>
public sealed class InstalledRoomObjectLinkedRestoreVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every restoration frame.</summary>
    /// <param name="images">Every restoration frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectLinkedRestoreVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every restoration frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
