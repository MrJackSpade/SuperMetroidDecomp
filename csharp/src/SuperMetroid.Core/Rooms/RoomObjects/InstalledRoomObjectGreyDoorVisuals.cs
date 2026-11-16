using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the grey caps' frames and their shared clear frames (#1557). The caps' terrain, waits and
/// timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectGreyDoorVisuals
{
    /// <summary>Holds the pictures the import made of every grey-cap frame.</summary>
    /// <param name="images">Every grey-cap frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectGreyDoorVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every grey-cap frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
