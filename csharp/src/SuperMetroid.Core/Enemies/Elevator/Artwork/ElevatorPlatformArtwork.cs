using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>The elevator platform's appearance: the installed picture a cell of a platform frame shows (#1557).</summary>
internal static class ElevatorPlatformArtwork
{
    /// <summary>The installed picture of one cell of a platform frame, translating run-local indices into the frame's cell order.</summary>
    /// <exception cref="InvalidDataException">The draw is not a platform frame, or the installation lacks it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the frame's shape.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectFramePictures pictures, RoomObjectDraw draw, int runIndex, int cellIndex)
    {
        if (!ElevatorPlatformDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)cellIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(cellIndex));
        int flatIndex = cellIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.BlockCount(run);
        return pictures.At(draw, flatIndex);
    }
}
