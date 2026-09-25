namespace SuperMetroid.Core.Rooms;

/// <summary>One visual n00b-tube frame's blocks in native run order.</summary>
public sealed record RoomPlmNoobTubeVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable appearance for the n00b-tube PLM. Native run geometry, collision
/// words, power-bomb gating, shard release and event timing remain compiled.
/// </summary>
public sealed class RoomPlmNoobTubeVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmNoobTubeVisualCatalog(IEnumerable<RoomPlmNoobTubeVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmNoobTubeVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !NoobTubePlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "N00b-tube visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"N00b-tube visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != NoobTubePlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "N00b-tube visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmNoobTubeVisualCatalog Stock() => new(
        NoobTubePlmDrawDefinitions.All.Select(draw =>
            new RoomPlmNoobTubeVisualEntry(
                NoobTubePlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !NoobTubePlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"N00b-tube visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
