using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The appearance of Crocomire's bridge and invisible-wall blocks: the installed picture a block of an arena frame shows
/// (#1557).
/// </summary>
internal static class CrocomireArenaArtwork
{
    /// <summary>The installed picture of one arena-frame block, translating run-local indices into the frame's cell order.</summary>
    /// <exception cref="InvalidDataException">The draw is not an arena frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectFramePictures pictures, RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!CrocomireArenaDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlocksPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return pictures.At(draw, runIndex * shape.BlocksPerRun + blockIndex);
    }
}
