namespace SuperMetroid.Core.Rooms;

/// <summary>One Bomb Torizo hand frame's visible blocks in cartridge run order.</summary>
public sealed record RoomPlmBombTorizoHandVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable appearance for the hand PLM. The compiled draw geometry, full
/// level words, Bombs gate, DMA, debris cadence, and music remain fixed.
/// </summary>
public sealed class RoomPlmBombTorizoHandVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmBombTorizoHandVisualCatalog(
        IEnumerable<RoomPlmBombTorizoHandVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmBombTorizoHandVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !BombTorizoHandPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Bomb Torizo hand visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Bomb Torizo hand visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != BombTorizoHandPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Bomb Torizo hand visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmBombTorizoHandVisualCatalog Stock() => new(
        BombTorizoHandPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmBombTorizoHandVisualEntry(
                BombTorizoHandPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !BombTorizoHandPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Bomb Torizo hand visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
