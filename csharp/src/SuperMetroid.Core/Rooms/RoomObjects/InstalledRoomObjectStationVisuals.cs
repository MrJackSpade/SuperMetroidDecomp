namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the map, refill and save stations' frames and their access frames (#1557). The stations'
/// terrain, animation and refills stay the game's.
/// </summary>
public sealed class InstalledRoomObjectStationVisuals
{
    /// <summary>Holds the pictures the import made of every station frame.</summary>
    /// <param name="images">Every station frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectStationVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every station frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        StationObjectDraws.TryDescribe(draw, out _)
            ? Images.Image(draw)
            : throw new InvalidDataException($"Station visuals lack frame {draw}.");
}
