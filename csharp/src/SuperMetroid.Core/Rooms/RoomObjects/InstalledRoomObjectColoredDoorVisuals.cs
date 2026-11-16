namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the coloured door caps' frames (#1557). The caps' terrain, hits and timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectColoredDoorVisuals
{
    /// <summary>Holds the pictures the import made of every coloured-cap frame.</summary>
    /// <param name="images">Every coloured-cap frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectColoredDoorVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every coloured-cap frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
