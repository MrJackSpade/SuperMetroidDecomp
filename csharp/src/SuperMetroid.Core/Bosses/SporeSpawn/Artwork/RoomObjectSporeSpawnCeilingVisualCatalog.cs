using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// The installed pictures of Spore Spawn's ceiling frames: the three crumble frames and the final clear (#1557). The
/// frames' terrain and two-by-two geometry stay the game's.
/// </summary>
public sealed class RoomObjectSporeSpawnCeilingVisualCatalog
{
    /// <summary>Holds the pictures the import made of every ceiling frame.</summary>
    /// <param name="images">Every ceiling frame's picture, keyed by its draw.</param>
    internal RoomObjectSporeSpawnCeilingVisualCatalog(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every ceiling frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        SporeSpawnCeilingDraws.TryGetBlock(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Spore Spawn ceiling visuals lack frame {draw}.");
}
