using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The installed pictures of the elevator platform's frames (#1557). The four-step loop, draw geometry and terrain stay
/// the game's.
/// </summary>
public sealed class InstalledRoomObjectElevatorPlatformVisuals
{
    /// <summary>Holds the pictures the import made of every platform frame.</summary>
    /// <param name="images">Every platform frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectElevatorPlatformVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every platform frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        ElevatorPlatformDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Elevator platform visuals lack frame {draw}.");
}
