using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's escape gate: a block's selected appearance, from the authored overrides or the
/// compiled terrain block, and the identity of the selected frames.
/// </summary>
internal static class EscapeGateArtwork
{
    /// <summary>Resolves one escape-gate block's selected appearance independently of its compiled solid collision.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported escape-gate frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row ordinal is outside the four-block run.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customBlocks, PlmDraw draw, int blockIndex)
    {
        if (!MotherBrainEscapeGateDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Escape-gate visuals lack frame {draw}.");
        if ((uint)blockIndex >= 4)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[blockIndex] : shape.BlockAt(blockIndex).Appearance;
    }
}
