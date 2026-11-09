namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the ordinary shot blocks' breakup and restoration frames (#1557). Their collision,
/// respawn and timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectShotBlockVisuals
{
    /// <summary>Holds the pictures the import made of every shot-block frame.</summary>
    /// <param name="images">Every shot-block frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectShotBlockVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every shot-block frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        draw is RoomObjectFrameDraw<ShotBlockObjectDraws.Draw>
            ? Images.Image(draw)
            : throw new InvalidDataException($"Shot-block visuals lack frame {draw}.");
}
