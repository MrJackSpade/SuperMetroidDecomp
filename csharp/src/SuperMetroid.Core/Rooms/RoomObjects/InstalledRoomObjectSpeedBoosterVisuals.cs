namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the bomb-revealed Speed Booster block's frames (#1557); the room object keeps the block's
/// collision and timing.
/// </summary>
public sealed class InstalledRoomObjectSpeedBoosterVisuals
{
    /// <summary>Holds the pictures the import made of every Speed Booster reveal frame.</summary>
    /// <param name="images">Every Speed Booster reveal frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectSpeedBoosterVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Speed Booster reveal frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
