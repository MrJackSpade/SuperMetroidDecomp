namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of one room-object family's frames, keyed by each frame's draw (#1557, #2557): the blocks,
/// stations, doors, gates, platforms, tubes, walls and boss-room pieces whose pictures the room draws as installed. Each
/// family's loader builds it from the family's files, which picture every frame the family draws and no other; the family's
/// terrain, timing and mutations stay the game's.
/// </summary>
public sealed class InstalledRoomObjectPictures : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every frame of a family.</summary>
    /// <param name="images">Every frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectPictures(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures => Images.All;
}
