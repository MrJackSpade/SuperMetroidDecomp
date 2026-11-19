using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The installed pictures of the Chozo hand and slope-access frames (#1557). The frames' draw geometry and terrain stay
/// the game's.
/// </summary>
public sealed class InstalledRoomObjectChozoStatueVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every Chozo statue frame.</summary>
    /// <param name="images">Every Chozo statue frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectChozoStatueVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Chozo statue frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
