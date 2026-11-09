using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The installed pictures of the Tourian access floor's frames: the empty row, the crumble rows and the six-row clear
/// (#1557). The frames' terrain and run offsets stay the game's.
/// </summary>
public sealed class RoomObjectTourianAccessVisuals
{
    /// <summary>The installed pictures of every access-floor frame.</summary>
    private readonly RoomObjectFramePictures pictures;

    /// <summary>Holds the installed pictures of every access-floor frame.</summary>
    /// <param name="pictures">Each access-floor frame's cell pictures, row by row, keyed by frame.</param>
    internal RoomObjectTourianAccessVisuals(RoomObjectFramePictures pictures) =>
        this.pictures = Ensure.NotNull(pictures);

    /// <summary>The installed picture of one cell of a Tourian access-floor frame.</summary>
    /// <param name="draw">The empty row, a visible crumble row, or the six-row clear.</param>
    /// <param name="runIndex">Top-to-bottom row index, zero for single-row frames or zero through five for the six-row clear.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within the four-cell row.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or column is outside that frame's dimensions; the reported parameter is <paramref name="blockIndex"/>.</exception>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Tourian access frame, or the installation lacks it.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        TourianAccessDraws.Describe(RoomObjectDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"), out int rows, out _);
        if ((uint)runIndex >= (uint)rows ||
            (uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, runIndex * 4 + blockIndex);
    }
}
