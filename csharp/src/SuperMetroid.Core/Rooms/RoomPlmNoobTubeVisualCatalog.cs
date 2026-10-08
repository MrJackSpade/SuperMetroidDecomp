using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One N00b tube frame's visible blocks in cartridge run order.</summary>
public sealed record RoomPlmNoobTubeVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable appearance for the tube PLM. The compiled draw geometry, full
/// level words, power-bomb gate, debris, event timing, and water physics remain fixed.
/// </summary>
public sealed class RoomPlmNoobTubeVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmNoobTubeVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[48];
        foreach (var frame in NoobTubePlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            NoobTubePlmDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            int count = 0;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                words[count++] = GetWord(frame.Pointer, run, word);
            content.Append("frame", frame.Pointer);
            // Preserve the flattened single-run framing of installed tube artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    public RoomPlmNoobTubeVisualCatalog(
        IEnumerable<RoomPlmNoobTubeVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmNoobTubeVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !NoobTubePlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "N00b tube visuals changed a frame identity, draw shape, or visual word.");
            if (!seen.Add(draw.Pointer))
                throw new InvalidDataException(
                    $"N00b tube visuals repeat frame {entry.Id}.");
            NoobTubePlmDrawDefinitions.TryDescribe(draw.Pointer, out var shape);
            int index = 0;
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
            for (int word = 0; word < shape.WordCount(run); word++)
                changed |= entry.Blocks[index++] != new RoomLevelWord(shape.WordAt(run, word)).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != NoobTubePlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "N00b tube visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!NoobTubePlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException(
                $"N00b tube visuals lack frame ${drawPointer:X4}.");
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
