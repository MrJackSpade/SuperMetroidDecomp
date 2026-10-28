using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The appearance of Mother Brain's glass: a block's selected visual word, from the authored overrides or the compiled
/// physical word, and the identity of the selected frames.
/// </summary>
internal static class MotherBrainGlassArtwork
{
    /// <summary>Hashes every glass layout's selected visual words as one flattened frame, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmMotherBrainGlassVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainGlassVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[16];
            foreach (var frame in MotherBrainGlassDraws.All.OrderBy(item => MotherBrainGlassDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                MotherBrainGlassDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int word = 0; word < shape.WordCount(run); word++)
                        words[count++] = visuals.GetWord(frame.Draw, run, word);
                content.AppendName("frame", MotherBrainGlassDraws.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed glass artwork.
                content.Append("runs", 1);
                content.AppendWords("words", words[..count]);
            }
        });

    /// <summary>Resolves one glass-layout visual block by translating native run-local indices into the flattened authored frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported glass layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal static ushort Word(PlmVisualOverrides customBlocks, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!MotherBrainGlassDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Mother Brain glass visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.WordCount(run);
        return customBlocks.TryGet(draw, out var words)
            ? words[flatIndex] : new RoomLevelWord(shape.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
