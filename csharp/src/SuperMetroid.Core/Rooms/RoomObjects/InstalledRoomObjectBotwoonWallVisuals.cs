namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of the Botwoon wall's nine-block clear (#1557). The clear's air terrain and the crumble sequence
/// stay the game's; the crumble frames are shot-block frames.
/// </summary>
public sealed class InstalledRoomObjectBotwoonWallVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every Botwoon wall frame.</summary>
    /// <param name="images">Every Botwoon wall frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectBotwoonWallVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Botwoon wall frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
