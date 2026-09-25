namespace SuperMetroid.Core.Rooms;

/// <summary>One linked-block restoration image in cartridge run order.</summary>
public sealed record RoomPlmLinkedRestoreVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable visual block selection for bomb and contact-crumble restoration.
/// The compiled level words retain linked parent/child collision ownership.
/// </summary>
public sealed class RoomPlmLinkedRestoreVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmLinkedRestoreVisualCatalog(
        IEnumerable<RoomPlmLinkedRestoreVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmLinkedRestoreVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !RoomPlmLinkedRestoreDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Linked restoration visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Linked restoration visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != RoomPlmLinkedRestoreDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Linked restoration visuals do not cover all six compiled layouts.");
        blocks = selected;
    }

    public static RoomPlmLinkedRestoreVisualCatalog Stock() => new(
        RoomPlmLinkedRestoreDrawDefinitions.All.Select(draw =>
            new RoomPlmLinkedRestoreVisualEntry(
                RoomPlmLinkedRestoreDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !RoomPlmLinkedRestoreDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Linked restoration visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
