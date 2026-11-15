using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The installed pictures of Bomb Torizo's hand frames (#1557). The frames' draw geometry and terrain, the Bombs gate,
/// the debris cadence and the music stay the game's.
/// </summary>
public sealed class InstalledRoomObjectBombTorizoHandVisuals
{
    /// <summary>Holds the pictures the import made of every hand frame.</summary>
    /// <param name="images">Every hand frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectBombTorizoHandVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every hand frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <summary>The picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
    internal RoomObjectFrameImage Image(RoomObjectDraw draw) => Images.Image(draw);
}
