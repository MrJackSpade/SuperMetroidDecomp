using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's glass: a block's selected appearance, from the authored overrides or the compiled
/// terrain block, and the identity of the selected frames.
/// </summary>
internal static class MotherBrainGlassArtwork
{
    /// <summary>Hashes every glass layout's selected appearances as one flattened frame, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmMotherBrainGlassVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainGlassVisualCatalog), content =>
        {
            var appearances = new RoomBlockAppearance[16];
            foreach (var frame in MotherBrainGlassDraws.All.OrderBy(item => MotherBrainGlassDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                MotherBrainGlassDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int cell = 0; cell < shape.BlockCount(run); cell++)
                        appearances[count++] = visuals.GetAppearance(frame.Draw, run, cell) ??
                            throw new InvalidDataException($"{nameof(visuals)} lack frame {frame.Draw}.");
                content.AppendName("frame", MotherBrainGlassDraws.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed glass artwork.
                content.Append("runs", 1);
                content.AppendAppearances("blocks", appearances.AsSpan(0, count));
            }
        });

    /// <summary>Resolves one glass-layout visual block by translating native run-local indices into the flattened authored frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported glass layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(PlmVisualOverrides customBlocks, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!MotherBrainGlassDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Mother Brain glass visuals lack frame {draw}.");
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
