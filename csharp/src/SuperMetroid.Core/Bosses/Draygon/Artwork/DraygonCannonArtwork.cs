using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The appearance of Draygon's wall cannon: a block's selected visual word, from the authored overrides or the
/// compiled physical word, and the identity of the selected frames.
/// </summary>
internal static class DraygonCannonArtwork
{
    /// <summary>Hashes every cannon frame's selected visual words, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmDraygonCannonVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmDraygonCannonVisualCatalog), content =>
        {
            Span<ushort> words = stackalloc ushort[4];
            foreach (var frame in DraygonCannonDraws.All.OrderBy(item => DraygonCannonDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                DraygonCannonDraws.TryDescribe(frame.Draw, out var shape);
                int count = 0;
                for (int run = 0; run < shape.RunCount; run++)
                    for (int word = 0; word < shape.WordCount(run); word++)
                        words[count++] = visuals.GetWord(frame.Draw, run, word);
                content.AppendName("frame", DraygonCannonDraws.VisualId(frame.Draw));
                // Preserve the flattened single-run framing of installed cannon artwork.
                content.Append("runs", 1);
                content.AppendWords("words", words[..count]);
            }
        });

    /// <summary>Resolves a cannon block's selected appearance by projecting native run-local indices into the flattened authored frame.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported cannon frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal static ushort Word(PlmVisualOverrides customBlocks, PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!DraygonCannonDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Draygon cannon visuals lack frame {draw}.");
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
