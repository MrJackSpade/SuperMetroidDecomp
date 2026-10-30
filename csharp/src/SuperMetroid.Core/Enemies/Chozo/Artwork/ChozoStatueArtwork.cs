using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The appearance of the Chozo statue hand and slope access: a cell's selected appearance, from the authored overrides or the compiled
/// terrain block, and the identity of the selected frames.
/// </summary>
internal static class ChozoStatueArtwork
{
    /// <summary>Hashes every frame's selected appearances, flattened into one run, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmChozoStatueVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmChozoStatueVisualCatalog), content =>
        {
            var appearances = new RoomBlockAppearance[27];
            foreach (var frame in ChozoStatueDraws.All.OrderBy(item => ChozoStatueDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                ChozoStatueDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int cell = 0; cell < shape.BlockCount(run); cell++)
                        appearances[count++] = Appearance(visuals, frame.Draw, run, cell);
                content.AppendName("frame", ChozoStatueDraws.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed Chozo artwork.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances.AsSpan(0, count));
            }
        });

    /// <summary>The selected appearance of one cell of a statue draw.</summary>
    private static RoomBlockAppearance Appearance(RoomPlmChozoStatueVisualCatalog visuals, PlmDraw draw, int run, int cell) =>
        visuals.GetAppearance(draw, run, cell) ?? throw new InvalidDataException($"Chozo statue visuals lack frame {draw}.");

    /// <summary>Resolves one selected appearance in a draw run, using compiled stock art when no independent edit was supplied.</summary>
    /// <exception cref="InvalidDataException">The draw identity is not a supported frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled shape.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customBlocks, PlmDraw draw, int runIndex, int blockIndex)
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
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
