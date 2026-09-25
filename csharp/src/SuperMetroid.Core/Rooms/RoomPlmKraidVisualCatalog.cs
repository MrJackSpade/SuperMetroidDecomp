namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Kraid ceiling or spike draw in native block order.</summary>
public sealed record RoomPlmKraidVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only Kraid ceiling and floor-spike selections. Native block words,
/// animation timing and movement callbacks remain compiled and immutable.
/// </summary>
public sealed class RoomPlmKraidVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmKraidVisualCatalog(IEnumerable<RoomPlmKraidVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmKraidVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !KraidRoomPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Kraid room visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Kraid room visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != KraidRoomPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Kraid room visuals do not cover all ten compiled draws.");
        blocks = selected;
    }

    public static RoomPlmKraidVisualCatalog Stock() => new(
        KraidRoomPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmKraidVisualEntry(
                KraidRoomPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !KraidRoomPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Kraid room visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
