using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The installed pictures of Bomb Torizo's hand frames (#1557). The frames' draw geometry and terrain, the Bombs gate,
/// the debris cadence and the music stay the game's.
/// </summary>
public sealed class RoomObjectBombTorizoHandVisualCatalog
{
    /// <summary>The installed pictures of the intact and cleared hand frames.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every hand frame.</summary>
    /// <param name="pictures">Each hand frame's cell pictures in run then cell order, keyed by frame.</param>
    internal RoomObjectBombTorizoHandVisualCatalog(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a hand frame.</summary>
    /// <param name="draw">The hand frame.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not the flattened entry index or a room coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a hand frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        BombTorizoHandArtwork.Appearance(pictures, draw, runIndex, blockIndex);
}
