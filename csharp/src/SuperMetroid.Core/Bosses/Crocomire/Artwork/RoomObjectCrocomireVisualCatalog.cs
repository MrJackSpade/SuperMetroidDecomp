using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The installed pictures of Crocomire's bridge and invisible-wall frames (#1557). The frames' terrain, programs and draw
/// geometry stay the game's.
/// </summary>
public sealed class RoomObjectCrocomireVisualCatalog
{
    /// <summary>The installed pictures of the five arena frames.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every arena frame.</summary>
    /// <param name="pictures">Each arena frame's cell pictures, keyed by frame.</param>
    internal RoomObjectCrocomireVisualCatalog(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one arena-frame block.</summary>
    /// <param name="draw">The arena frame.</param>
    /// <param name="runIndex">Zero-based ordinal of the frame's run.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not an arena frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        CrocomireArenaArtwork.Appearance(pictures, draw, runIndex, blockIndex);
}
