using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's fake-death room blocks: a block's selected appearance, from the authored
/// overrides or the compiled terrain block, and the identity of the selected frames.
/// </summary>
internal static class MotherBrainFakeDeathArtwork
{
    /// <summary>Resolves a fake-death room block's selected appearance, projecting run-local indices into its flattened authored layout.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported fake-death layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectVisualOverrides customAppearances, RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (MotherBrainFakeDeathDraws.TryDescribeBackground(draw, out var background))
        {
            if (runIndex != 0 || (uint)blockIndex >= 13)
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customAppearances.TryGet(draw, out var selectedBackground)
                ? selectedBackground[blockIndex] : background.BlockAt(blockIndex).Appearance;
        }
        if (MotherBrainFakeDeathDraws.TryDescribeRegular(draw, out var regular))
        {
            if ((uint)runIndex >= regular.RunCount || (uint)blockIndex >= regular.Count(runIndex))
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customAppearances.TryGet(draw, out var selected)
                ? selected[(runIndex == 0 ? 0 : regular.Count(0)) + blockIndex]
                : regular.BlockAt(runIndex, blockIndex).Appearance;
        }
        if (!MotherBrainFakeDeathDraws.TryDescribeBoundary(draw, out var shape))
            throw new InvalidDataException(
                $"Mother Brain fake-death visuals lack draw {draw}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= (uint)shape.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.Count(run);
        return customAppearances.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
