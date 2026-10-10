using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One Mother Brain glass frame's visible blocks in cartridge run order.</summary>
/// <param name="Id">Compiled identity of an initial, damaged, shifted, shattering, or cleared glass layout.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order and word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmMotherBrainGlassVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable appearance for the glass PLM. The compiled draw geometry, full
/// level words, hit thresholds, shards, and event timing remain fixed.
/// </summary>
public sealed class RoomPlmMotherBrainGlassVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainGlassVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[16];
        foreach (var frame in MotherBrainGlassPlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            MotherBrainGlassPlmDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                words[count++] = GetWord(frame.Pointer, run, word);
            content.Append("frame", frame.Pointer);
            // Preserve the flattened single-run framing of installed glass artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    /// <summary>Visual-word overrides keyed by native draw-frame pointer, with each array flattened in run order.</summary>
    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    /// <summary>Validates all eleven glass layouts and copies visual differences without changing damage thresholds, shards, event timing, physical words, or signed draw offsets.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled glass identity, with the full flattened count of its native run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmMotherBrainGlassVisualCatalog(
        IEnumerable<RoomPlmMotherBrainGlassVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmMotherBrainGlassVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !MotherBrainGlassPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Mother Brain glass visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Mother Brain glass visuals repeat frame {entry.Id}.");
            MotherBrainGlassPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            int index = 0;
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                changed |= entry.Blocks[index++] != new RoomLevelWord(shape.WordAt(run, word)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != MotherBrainGlassPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Mother Brain glass visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves one glass-layout visual block by translating native run-local indices into the flattened authored frame.</summary>
    /// <param name="drawPointer">Bank-$84 identity of a supported glass draw layout.</param>
    /// <param name="runIndex">Zero-based native run ordinal; supported layouts contain one through four runs.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored metatile/flip word, or the compiled physical word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported glass layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!MotherBrainGlassPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Mother Brain glass visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)blockIndex >= (uint)draw.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.WordCount(run);
        return customBlocks is not null && customBlocks.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
