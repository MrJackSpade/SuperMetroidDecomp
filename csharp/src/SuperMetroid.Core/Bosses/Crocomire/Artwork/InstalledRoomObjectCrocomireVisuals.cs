using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The installed pictures of Crocomire's bridge and invisible-wall frames (#1557). The frames' terrain, animations and draw
/// geometry stay the game's.
/// </summary>
public sealed class InstalledRoomObjectCrocomireVisuals
{
    /// <summary>Holds the pictures the import made of every arena frame.</summary>
    /// <param name="images">Every arena frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectCrocomireVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every arena frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
