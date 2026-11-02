using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The elevator platform's appearance: a cell's selected appearance, from the authored appearances that differ from
/// the compiled frames, and the identity of the selected frames.
/// </summary>
internal static class ElevatorPlatformArtwork
{
    /// <summary>Resolves one elevator-platform block appearance without replacing its compiled terrain block or draw offset.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(IReadOnlyDictionary<PlmDraw, RoomBlockAppearance[][]> customAppearances, PlmDraw draw, int runIndex, int cellIndex)
    {
        if (!ElevatorPlatformDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)cellIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(cellIndex));
        return customAppearances.TryGetValue(draw, out var runs)
            ? runs[runIndex][cellIndex] : shape.BlockAt(runIndex, cellIndex).Appearance;
    }
}
