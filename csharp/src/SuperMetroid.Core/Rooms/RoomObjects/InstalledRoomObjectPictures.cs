namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of one room-object family's frames, keyed by each frame's draw (#1557, #2557): the blocks,
/// stations, doors, gates, platforms, tubes, walls and boss-room pieces whose pictures the room draws as installed. Each
/// family's loader builds it from the family's files, which picture every frame the family draws and no other, and adds at
/// load each draw that shows another frame's picture (a blue cap drawn solid shows its facing's closed cap, the eye door's
/// opening clear the shared clear mirrored); the family's terrain, timing and mutations stay the game's.
/// </summary>
public sealed class InstalledRoomObjectPictures : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures of every draw of a family.</summary>
    /// <param name="images">Every draw's picture, keyed by the draw.</param>
    internal InstalledRoomObjectPictures(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every draw's picture, keyed by the draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures => Images.All;
}
