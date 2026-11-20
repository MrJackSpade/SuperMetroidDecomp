namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the map, refill and save stations' frames and their access frames (#1557). The stations'
/// terrain, animation and refills stay the game's.
/// </summary>
public sealed class InstalledRoomObjectStationVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every station frame.</summary>
    /// <param name="images">Every station frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectStationVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every station frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
