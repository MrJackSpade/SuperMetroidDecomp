namespace SuperMetroid.Core.Rooms;

/// <summary>One visual glass frame's blocks in native run order.</summary>
public sealed record RoomPlmMotherBrainGlassVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable appearance for Mother Brain's glass PLM. The source's multi-run
/// layout, signed offsets, collision words, shatter timing and hit rules remain
/// compiled and cannot be changed by a visual override.
/// </summary>
public sealed class RoomPlmMotherBrainGlassVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmMotherBrainGlassVisualCatalog(
        IEnumerable<RoomPlmMotherBrainGlassVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmMotherBrainGlassVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !MotherBrainGlassPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out RoomPlmShotBlockDrawDefinitions.DrawList draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Mother Brain glass visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Mother Brain glass visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != MotherBrainGlassPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Mother Brain glass visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmMotherBrainGlassVisualCatalog Stock() => new(
        MotherBrainGlassPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmMotherBrainGlassVisualEntry(
                MotherBrainGlassPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !MotherBrainGlassPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Mother Brain glass visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
