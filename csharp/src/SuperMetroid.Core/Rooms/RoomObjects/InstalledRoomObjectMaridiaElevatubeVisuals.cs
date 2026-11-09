namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of the Maridia elevatube's block (#1557). Its physical block, hold, sound and deletion stay the
/// game's.
/// </summary>
public sealed class InstalledRoomObjectMaridiaElevatubeVisuals
{
    /// <summary>Holds the pictures the import made of every elevatube frame.</summary>
    /// <param name="images">Every elevatube frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectMaridiaElevatubeVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every elevatube frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        draw == MaridiaElevatubeObjectDefinitions.BlockDraw
            ? Images.Image(draw)
            : throw new InvalidDataException($"Maridia elevatube visuals lack frame {draw}.");
}
