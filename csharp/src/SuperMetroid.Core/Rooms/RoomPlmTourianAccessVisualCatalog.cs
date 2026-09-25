namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Tourian access-floor frame in cartridge draw-run order.</summary>
public sealed record RoomPlmTourianAccessVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only block selections for the four crumble frames and six-row clear.
/// The compiled draw lists retain every physical level word and run offset.
/// </summary>
public sealed class RoomPlmTourianAccessVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmTourianAccessVisualCatalog(
        IEnumerable<RoomPlmTourianAccessVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmTourianAccessVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !TourianAccessPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Tourian access visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Tourian access visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != TourianAccessPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Tourian access visuals do not cover all five compiled layouts.");
        blocks = selected;
    }

    public static RoomPlmTourianAccessVisualCatalog Stock() => new(
        TourianAccessPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmTourianAccessVisualEntry(
                TourianAccessPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !TourianAccessPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Tourian access visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
