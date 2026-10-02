using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One Mother Brain glass frame's visible blocks in cartridge run order.</summary>
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
            MotherBrainGlassPlmDrawDefinitions.TryGet(frame.Pointer, out var shape);
            int count = 0;
            for (int run = 0; run < shape.Runs.Length; run++)
            for (int word = 0; word < shape.Runs.Span[run].LevelWords.Length; word++)
                words[count++] = GetWord(frame.Pointer, run, word);
            content.Append("frame", frame.Pointer);
            // Preserve the flattened single-run framing of installed glass artwork.
            content.Append("runs", 1);
            content.AppendWords("words", words[..count]);
        }
    });

    private readonly Dictionary<ushort, ushort[]>? customBlocks;

    private RoomPlmMotherBrainGlassVisualCatalog() { }

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
            MotherBrainGlassPlmDrawDefinitions.TryGet(draw.Pointer, out var shape);
            int index = 0;
            bool changed = false;
            for (int run = 0; run < shape.Runs.Length; run++)
            for (int word = 0; word < shape.Runs.Span[run].LevelWords.Length; word++)
                changed |= entry.Blocks[index++] != new RoomLevelWord(shape.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (changed) selected.Add(draw.Pointer, entry.Blocks.ToArray());
        }
        if (seen.Count != MotherBrainGlassPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Mother Brain glass visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Calculate original appearances directly; store only customized frames.</summary>
    public static RoomPlmMotherBrainGlassVisualCatalog Stock() => new();

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!MotherBrainGlassPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Mother Brain glass visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return customBlocks is not null && customBlocks.TryGetValue(drawPointer, out var words)
            ? words[flatIndex] : new RoomLevelWord(draw.Runs.Span[runIndex].LevelWords.Span[blockIndex]).VisualWord;
    }
}
