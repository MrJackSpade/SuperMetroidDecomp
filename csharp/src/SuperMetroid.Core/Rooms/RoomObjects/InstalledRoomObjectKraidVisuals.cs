namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of Kraid's room's ten ceiling and floor-spike frames (#1557). Their terrain, animation timing
/// and movement behaviours stay the game's.
/// </summary>
public sealed class InstalledRoomObjectKraidVisuals
{
    /// <summary>Holds the pictures the import made of every Kraid room frame.</summary>
    /// <param name="images">Every Kraid room frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectKraidVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Kraid room frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        KraidRoomObjectDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Kraid room visuals lack frame {draw}.");
}
