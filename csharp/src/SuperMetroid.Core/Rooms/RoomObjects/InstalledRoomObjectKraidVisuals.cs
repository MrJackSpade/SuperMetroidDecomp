namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of Kraid's room's ten ceiling and floor-spike frames (#1557). Their terrain, animation timing
/// and movement behaviours stay the game's.
/// </summary>
public sealed class InstalledRoomObjectKraidVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every Kraid room frame.</summary>
    /// <param name="images">Every Kraid room frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectKraidVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Kraid room frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
