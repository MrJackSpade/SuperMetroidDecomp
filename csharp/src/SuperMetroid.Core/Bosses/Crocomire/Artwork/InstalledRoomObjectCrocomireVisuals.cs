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
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        CrocomireArenaDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Crocomire visuals lack frame {draw}.");
}
