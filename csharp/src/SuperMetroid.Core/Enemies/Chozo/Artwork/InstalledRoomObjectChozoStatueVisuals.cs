using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The installed pictures of the Chozo hand and slope-access frames (#1557). The frames' draw geometry and terrain stay
/// the game's.
/// </summary>
public sealed class InstalledRoomObjectChozoStatueVisuals
{
    /// <summary>Holds the pictures the import made of every Chozo statue frame.</summary>
    /// <param name="images">Every Chozo statue frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectChozoStatueVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Chozo statue frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        ChozoStatueDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Chozo statue visuals lack frame {draw}.");
}
