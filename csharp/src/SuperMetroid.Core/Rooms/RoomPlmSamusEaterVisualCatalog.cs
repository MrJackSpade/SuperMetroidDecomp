using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One replaceable Samus Eater block appearance in native run order.</summary>
/// <param name="Id">Compiled floor/ceiling idle or chewing-pose identity.</param>
/// <param name="Blocks">Eight metatile/flip words flattened by native run then word order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmSamusEaterVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only level words for all four floor and four ceiling plant poses.
/// The compiled draw definitions retain their physical collision nibble and
/// three-run geometry even when a player replaces these visible block IDs.
/// </summary>
public sealed class RoomPlmSamusEaterVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmSamusEaterVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[8];
        foreach (var draw in SamusEaterPlmDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            int index = 0;
            for (int run = 0; run < 3; run++)
            for (int block = 0; block < SamusEaterPlmDrawDefinitions.Draw.Count(run); block++)
                words[index++] = GetWord(draw.Pointer, run, block);
            content.Append("frame", draw.Pointer);
            // Preserve the original flattened-frame hash, not the three physical runs.
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    /// <summary>Copied flattened visual-word overrides keyed by pose draw pointer; unchanged poses use compiled stock words.</summary>
    private readonly Dictionary<ushort, ushort[]>? customWords;

    /// <summary>Validates all eight three-run plant poses and copies authored visual differences without changing collision, run geometry, chewing timing, or Samus interaction.</summary>
    /// <param name="entries">Exactly one eight-word visual-only entry for each compiled floor and ceiling pose.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    public RoomPlmSamusEaterVisualCatalog(
        IEnumerable<RoomPlmSamusEaterVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmSamusEaterVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !SamusEaterPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Samus Eater visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Samus Eater visuals repeat frame {entry.Id}.");
            bool differs = false;
            int index = 0;
            foreach (var run in draw.Runs.Span)
            foreach (ushort word in run.LevelWords.Span)
                differs |= entry.Blocks[index++] != new RoomLevelWord(word).VisualWord;
            if (differs) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != SamusEaterPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Samus Eater visuals do not cover all eight compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Resolves one selected plant block, translating native run-local indices into the flattened authored payload.</summary>
    /// <param name="drawPointer">Bank-$84 identity of a supported Samus Eater pose.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal 0..2.</param>
    /// <param name="blockIndex">Zero-based word ordinal within the selected run.</param>
    /// <returns>The authored metatile/flip word, or the compiled physical word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported pose.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled shape.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!SamusEaterPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= 3 ||
            (uint)blockIndex >= (uint)SamusEaterPlmDrawDefinitions.Draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += SamusEaterPlmDrawDefinitions.Draw.Count(run);
        return customWords is not null && customWords.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
