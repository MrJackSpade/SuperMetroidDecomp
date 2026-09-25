namespace SuperMetroid.Core.Rooms;

/// <summary>Editable tile appearances for one Crocomire arena draw layout.</summary>
public sealed record RoomPlmCrocomireVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only Crocomire bridge and invisible-wall blocks. The compiled PLM
/// programs, complete physical level words, and draw geometry remain immutable.
/// </summary>
public sealed class RoomPlmCrocomireVisualCatalog
{
    private readonly Dictionary<ushort, ushort[]> blocks;

    public RoomPlmCrocomireVisualCatalog(
        IEnumerable<RoomPlmCrocomireVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        blocks = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmCrocomireVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !CrocomireArenaPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Crocomire visuals changed a draw identity, shape, or visual word.");
            if (!blocks.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Crocomire visuals repeat draw {entry.Id}.");
        }
        if (blocks.Count != CrocomireArenaPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Crocomire visuals do not cover all five compiled draws.");
    }

    public static RoomPlmCrocomireVisualCatalog Stock() => new(
        CrocomireArenaPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmCrocomireVisualEntry(
                CrocomireArenaPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !CrocomireArenaPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Crocomire visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
