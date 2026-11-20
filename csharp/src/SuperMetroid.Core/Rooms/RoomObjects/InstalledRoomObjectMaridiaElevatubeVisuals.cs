namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of the Maridia elevatube's block (#1557). Its physical block, hold, sound and deletion stay the
/// game's.
/// </summary>
public sealed class InstalledRoomObjectMaridiaElevatubeVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every elevatube frame.</summary>
    /// <param name="images">Every elevatube frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectMaridiaElevatubeVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every elevatube frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
