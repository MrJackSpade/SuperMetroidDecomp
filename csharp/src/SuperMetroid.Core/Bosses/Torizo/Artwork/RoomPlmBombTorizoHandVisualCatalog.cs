using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Editable appearance for the hand PLM. The compiled draw geometry, full
/// level words, Bombs gate, DMA, debris cadence, and music remain fixed.
/// </summary>
public sealed class RoomPlmBombTorizoHandVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmBombTorizoHandVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[16];
        foreach (var frame in BombTorizoHandPlmDrawDefinitions.All.OrderBy(item => BombTorizoHandPlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            BombTorizoHandPlmDrawDefinitions.TryDescribe(frame.Draw, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
                for (int word = 0; word < shape.WordCount(run); word++)
                    words[count++] = GetWord(frame.Draw, run, word);
            content.AppendName("frame", BombTorizoHandPlmDrawDefinitions.VisualId(frame.Draw));
            // Preserve the flattened single-run framing of installed hand artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Copied visual-only edits keyed by draw; stock frames use calculated words.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete hand-frame coverage and copies only authored visual differences, leaving Bombs gating, collision, draw geometry and debris/music choreography fixed.</summary>
    /// <param name="entries">One entry for every compiled hand frame, with exactly the flattened native block count and no physical collision bits.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a frame, changes a compiled identity/shape, or leaves coverage incomplete.</exception>
    public RoomPlmBombTorizoHandVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Bomb Torizo hand", BombTorizoHandPlmDrawDefinitions.TryGetByVisualId,
            BombTorizoHandPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves one selected visual word in a hand draw run, using compiled stock art when no independent edit was supplied.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not the flattened entry index or a room coordinate.</param>
    /// <returns>The metatile reference and parent flips, excluding the compiled physical collision nibble.</returns>
    /// <exception cref="InvalidDataException">The draw identity is not a supported hand frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!BombTorizoHandPlmDrawDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Bomb Torizo hand visuals lack frame {draw}.");
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
