namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the collectibles' frames (#1557): every frame but a block pair's, which lies under the item's
/// own picture and writes only its terrain (#1538). Pickup and collision stay the game's.
/// </summary>
public sealed class InstalledRoomObjectCollectibleVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every collectible frame.</summary>
    /// <param name="images">Every collectible frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectCollectibleVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every collectible frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
