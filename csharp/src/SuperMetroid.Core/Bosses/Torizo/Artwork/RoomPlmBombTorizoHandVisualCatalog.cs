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
        foreach (var frame in BombTorizoHandPlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            BombTorizoHandPlmDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                words[count++] = GetWord(frame.Pointer, run, word);
            content.Append("frame", frame.Pointer);
            // Preserve the flattened single-run framing of installed hand artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Copied visual-only edits keyed by compiled hand-frame pointer; stock frames use calculated words.</summary>
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
    /// <param name="drawPointer">Compiled bank-$84 Bomb Torizo hand draw identity.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not the flattened entry index or a room coordinate.</param>
    /// <returns>The metatile reference and parent flips, excluding the compiled physical collision nibble.</returns>
    /// <exception cref="InvalidDataException">The draw identity is not a supported hand frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled shape.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!BombTorizoHandPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Bomb Torizo hand visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)blockIndex >= (uint)draw.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.WordCount(run);
        return customBlocks.TryGet(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
