namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Spore Spawn ceiling frame in native draw-run order.</summary>
public sealed record RoomPlmSporeSpawnCeilingVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only block selections for the three crumble frames and final clear.
/// Compiled draw definitions retain their physical words and 2×2 geometry.
/// </summary>
public sealed class RoomPlmSporeSpawnCeilingVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmSporeSpawnCeilingVisualCatalog(
        IEnumerable<RoomPlmSporeSpawnCeilingVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmSporeSpawnCeilingVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !SporeSpawnCeilingPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Spore Spawn ceiling visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Spore Spawn ceiling visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != SporeSpawnCeilingPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Spore Spawn ceiling visuals do not cover all four compiled frames.");
        blocks = selected;
    }

    public static RoomPlmSporeSpawnCeilingVisualCatalog Stock() => new(
        SporeSpawnCeilingPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmSporeSpawnCeilingVisualEntry(
                SporeSpawnCeilingPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !SporeSpawnCeilingPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Spore Spawn ceiling visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
