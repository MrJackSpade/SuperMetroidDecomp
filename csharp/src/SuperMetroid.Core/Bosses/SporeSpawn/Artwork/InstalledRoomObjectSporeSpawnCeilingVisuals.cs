using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The installed pictures of Spore Spawn's ceiling frames: the three crumble frames and the final clear (#1557). The
/// frames' terrain and two-by-two geometry stay the game's.
/// </summary>
public sealed class InstalledRoomObjectSporeSpawnCeilingVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every ceiling frame.</summary>
    /// <param name="images">Every ceiling frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectSporeSpawnCeilingVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every ceiling frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IEnumerable<KeyValuePair<RoomObjectDraw, RoomObjectFrameImage>> IRoomObjectFamilyPictures.Pictures =>
        Images.All;

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
