using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The installed pictures of Bomb Torizo's hand frames (#1557). The frames' draw geometry and terrain, the Bombs gate,
/// the debris cadence and the music stay the game's.
/// </summary>
public sealed class RoomObjectBombTorizoHandVisualCatalog
{
    /// <summary>Holds the pictures the import made of every hand frame.</summary>
    /// <param name="images">Every hand frame's picture, keyed by its draw.</param>
    internal RoomObjectBombTorizoHandVisualCatalog(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every hand frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        BombTorizoHandDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Bomb Torizo hand visuals lack frame {draw}.");
}
