namespace SuperMetroid.Core.Rooms;

/// <summary>One Chozo statue PLM layout's visual blocks in native run order.</summary>
public sealed record RoomPlmChozoStatueVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable Chozo hand and slope-access appearances. Run geometry and physical
/// collision words are retained in the compiled draw definitions.
/// </summary>
public sealed class RoomPlmChozoStatueVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmChozoStatueVisualCatalog(
        IEnumerable<RoomPlmChozoStatueVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmChozoStatueVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !ChozoStatuePlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Chozo statue visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Chozo statue visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != ChozoStatuePlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Chozo statue visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmChozoStatueVisualCatalog Stock() => new(
        ChozoStatuePlmDrawDefinitions.All.Select(draw =>
            new RoomPlmChozoStatueVisualEntry(
                ChozoStatuePlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !ChozoStatuePlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Chozo statue visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
