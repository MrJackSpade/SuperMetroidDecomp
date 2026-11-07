using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The appearance of Crocomire's bridge and invisible-wall blocks: a block's selected appearance, from the authored
/// overrides or the compiled terrain block, and the identity of the selected frames.
/// </summary>
internal static class CrocomireArenaArtwork
{
    /// <summary>Resolves one arena-layout block, translating run-local indices into the flattened visual override.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectVisualOverrides customAppearances, RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!CrocomireArenaDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlocksPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = runIndex * shape.BlocksPerRun + blockIndex;
        return customAppearances.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
