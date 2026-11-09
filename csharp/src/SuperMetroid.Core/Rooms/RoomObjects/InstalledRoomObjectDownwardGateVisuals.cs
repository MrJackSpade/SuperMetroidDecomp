namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the downward gates' column frames and coloured triggers (#1557). Their terrain, shot
/// triggers and timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectDownwardGateVisuals
{
    /// <summary>Holds the pictures the import made of every downward-gate frame.</summary>
    /// <param name="images">Every downward-gate frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectDownwardGateVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every downward-gate frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        DownwardGateObjectDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Downward gate visuals lack frame {draw}.");
}
