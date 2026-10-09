using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One Draygon cannon frame's visible blocks in cartridge run order.</summary>
/// <param name="Id">Compiled identity of a reachable Draygon cannon draw frame.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order, then word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmDraygonCannonVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable appearance for the cannon PLM. The compiled draw geometry, full
/// level words, hit thresholds, damage transitions, and control writes remain fixed.
/// </summary>
public sealed class RoomPlmDraygonCannonVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmDraygonCannonVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[4];
        foreach (var frame in DraygonCannonPlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            DraygonCannonPlmDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                words[count++] = GetWord(frame.Pointer, run, word);
            content.Append("frame", frame.Pointer);
            // Preserve the flattened single-run framing of installed cannon artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    /// <summary>Validates complete reachable cannon-frame coverage and copies visual differences without changing shield damage, hit thresholds, control writes, or draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled frame with the exact flattened count of all its run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves reachable frame coverage incomplete.</exception>
    public RoomPlmDraygonCannonVisualCatalog(
        IEnumerable<RoomPlmDraygonCannonVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmDraygonCannonVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !DraygonCannonPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Draygon cannon visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"Draygon cannon visuals repeat frame {entry.Id}.");
            DraygonCannonPlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            int index = 0;
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                changed |= entry.Blocks[index++] != new RoomLevelWord(shape.WordAt(run, word)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != DraygonCannonPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Draygon cannon visuals do not cover all reachable compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves a cannon block's selected appearance by projecting native run-local indices into the flattened authored frame.</summary>
    /// <param name="drawPointer">Bank-$84 identity of a supported cannon draw frame, not its PLM header or instruction-list pointer.</param>
    /// <param name="runIndex">Zero-based ordinal of the frame's native draw run.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index.</param>
    /// <returns>The selected metatile/flip word, or visual bits from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported cannon frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!DraygonCannonPlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Draygon cannon visuals lack frame ${drawPointer:X4}.");
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
