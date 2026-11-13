namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed picture of the Botwoon wall's nine-block clear (#1557). The clear's air terrain and the crumble sequence
/// stay the game's; the crumble frames are shot-block frames.
/// </summary>
public sealed class InstalledRoomObjectBotwoonWallVisuals
{
    /// <summary>Holds the pictures the import made of every Botwoon wall frame.</summary>
    /// <param name="images">Every Botwoon wall frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectBotwoonWallVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Botwoon wall frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The draw is not one of the family's frames, or the installation lacks it.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) =>
        draw == BotwoonWallObjectDrawDefinitions.Clear
            ? Images.Image(draw)
            : throw new InvalidDataException($"Botwoon wall visuals lack frame {draw}.");
}
