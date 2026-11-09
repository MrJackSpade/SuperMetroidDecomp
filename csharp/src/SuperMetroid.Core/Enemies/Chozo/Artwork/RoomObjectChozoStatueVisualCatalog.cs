using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The installed pictures of the Chozo hand and slope-access frames (#1557). The frames' draw geometry and terrain stay
/// the game's.
/// </summary>
public sealed class RoomObjectChozoStatueVisualCatalog
{
    /// <summary>The installed pictures of every Chozo frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every Chozo frame.</summary>
    /// <param name="pictures">Each Chozo frame's cell pictures in run then cell order, keyed by frame.</param>
    internal RoomObjectChozoStatueVisualCatalog(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a Chozo frame.</summary>
    /// <param name="draw">The Chozo frame.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a room coordinate or flattened entry index.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a Chozo frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        ChozoStatueArtwork.Appearance(pictures, draw, runIndex, blockIndex);
}
