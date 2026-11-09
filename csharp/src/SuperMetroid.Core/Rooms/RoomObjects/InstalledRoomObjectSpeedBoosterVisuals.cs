namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of the bomb-revealed Speed Booster block (#1557). Its type-B collision and timing stay the
/// game's.
/// </summary>
public sealed class InstalledRoomObjectSpeedBoosterVisuals
{
    /// <summary>Holds the pictures the import made of every Speed Booster reveal frame.</summary>
    /// <param name="images">Every Speed Booster reveal frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectSpeedBoosterVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Speed Booster reveal frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        draw == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw
            ? Images.Image(draw)
            : throw new InvalidDataException($"Speed Booster visuals lack frame {draw}.");
}
