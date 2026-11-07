using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The appearance of Draygon's wall cannon: a block's selected appearance, from the authored overrides or the
/// compiled terrain block, and the identity of the selected frames.
/// </summary>
internal static class DraygonCannonArtwork
{
    /// <summary>Resolves a cannon block's selected appearance by projecting run-local indices into the flattened authored frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported cannon frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(RoomObjectVisualOverrides customBlocks, RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!DraygonCannonDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Draygon cannon visuals lack frame {draw}.");
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
