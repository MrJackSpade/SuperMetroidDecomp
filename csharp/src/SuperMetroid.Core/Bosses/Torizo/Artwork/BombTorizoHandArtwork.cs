using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The appearance of Bomb Torizo's hand: a cell's selected appearance, from the authored overrides or the compiled
/// terrain block, and the identity of the selected frames.
/// </summary>
internal static class BombTorizoHandArtwork
{
    /// <summary>Resolves one selected appearance in a hand draw run, using compiled stock art when no independent edit was supplied.</summary>
    /// <exception cref="InvalidDataException">The draw identity is not a supported hand frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled shape.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customBlocks, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!BombTorizoHandDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Bomb Torizo hand visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.BlockCount(run);
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
