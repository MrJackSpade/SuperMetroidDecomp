using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the sixteen blue-door frames, whose closed caps each facing's closed cap drawn solid also
/// shows (#1557, #1994). The caps' terrain, animation timing and door handoff stay the game's.
/// </summary>
public sealed class InstalledRoomObjectBlueDoorVisuals
{
    /// <summary>Holds the pictures the import made of every blue-door frame.</summary>
    /// <param name="images">Every blue-door frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectBlueDoorVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every blue-door frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows; the closed cap drawn solid shows its facing's closed cap.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(BlueDoorCaps.VisualSource(draw));
}
