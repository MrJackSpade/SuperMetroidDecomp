using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The appearance of the Chozo statue hand and slope access: the installed picture a cell of one of their frames shows
/// (#1557).
/// </summary>
internal static class ChozoStatueArtwork
{
    /// <summary>The installed picture of one cell of a Chozo frame, translating run-local indices into the frame's cell order.</summary>
    /// <exception cref="InvalidDataException">The draw is not a Chozo frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectFramePictures pictures, RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!ChozoStatueDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Chozo statue visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.BlockCount(run);
        return pictures.At(draw, flatIndex);
    }
}
