using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The installed pictures of Draygon's wall cannons' twelve frames (#1557), by frame. The cannons' geometry, collision,
/// hit counts and control writes stay with their typed objects and draws.
/// </summary>
public sealed class InstalledDraygonCannonFrames : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every cannon frame.</summary>
    /// <param name="images">Every cannon frame's picture.</param>
    internal InstalledDraygonCannonFrames(RoomObjectFrameImages<DraygonCannonDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every cannon frame's picture.</summary>
    internal RoomObjectFrameImages<DraygonCannonDraw> Images { get; }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> IRoomObjectFamilyPictures.Pictures =>
        Images.All.Select(picture => KeyValuePair.Create<RoomObjectDraw, RoomObjectFrameImage>(DraygonCannonDraws.Family.Of(picture.Key), picture.Value));
}
