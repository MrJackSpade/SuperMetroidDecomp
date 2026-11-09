using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the sixteen blue-door frames, four of which a closed cap shows under a second identity
/// (#1557). The caps' terrain, animation timing and door handoff stay the game's.
/// </summary>
public sealed class InstalledRoomObjectBlueDoorVisuals
{
    /// <summary>Holds the pictures the import made of every blue-door frame.</summary>
    /// <param name="images">Every blue-door frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectBlueDoorVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every blue-door frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows; a closed cap shows its facing's first opening frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a blue-door frame, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        BlueDoorCaps.TryDescribe(draw, out _)
            ? Images.Image(BlueDoorCaps.VisualSource(draw))
            : throw new InvalidDataException($"Blue-door visuals lack frame {draw}.");
}
