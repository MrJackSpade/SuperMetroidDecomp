namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the Samus Eater plant's frames (#1557); the three-run terrain layout stays the game's.
/// </summary>
public sealed class InstalledRoomObjectSamusEaterVisuals
{
    /// <summary>Holds the pictures the import made of every plant frame.</summary>
    /// <param name="images">Every plant frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectSamusEaterVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every plant frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
