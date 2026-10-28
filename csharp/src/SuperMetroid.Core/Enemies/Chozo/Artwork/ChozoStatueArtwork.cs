using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The appearance of the Chozo statue hand and slope access: a cell's selected visual word, from the authored overrides or the compiled physical
/// word, and the identity of the selected frames.
/// </summary>
internal static class ChozoStatueArtwork
{
    /// <summary>Hashes every frame's selected visual words, flattened into one run, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmChozoStatueVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmChozoStatueVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[27];
            foreach (var frame in ChozoStatueDraws.All.OrderBy(item => ChozoStatueDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                ChozoStatueDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int word = 0; word < shape.WordCount(run); word++)
                        words[count++] = visuals.GetWord(frame.Draw, run, word);
                content.AppendName("frame", ChozoStatueDraws.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed Chozo artwork.
                content.Append("runs", 1);
                content.AppendWords("words", words[..count]);
            }
        });

    /// <summary>Resolves one selected visual word in a draw run, using compiled stock art when no independent edit was supplied.</summary>
    /// <exception cref="InvalidDataException">The draw identity is not a supported frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled shape.</exception>
    internal static ushort Word(PlmVisualOverrides customBlocks, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!ChozoStatueDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Chozo statue visuals lack frame {draw}.");
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
