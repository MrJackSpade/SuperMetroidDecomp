namespace SuperMetroid.Core.Rooms;

/// <summary>Editable tile appearance for one Mother Brain fake-death draw.</summary>
public sealed record RoomPlmMotherBrainFakeDeathVisualEntry(
    string Id, ushort[] Blocks);

/// <summary>
/// Visual-only Mother Brain fake-death wall, room background, door, and tube
/// appearances. Native collision words, draw geometry, and PLM timing remain
/// immutable compiled data, independent of these replacement tile indexes.
/// </summary>
public sealed class RoomPlmMotherBrainFakeDeathVisualCatalog
{
    private readonly Dictionary<ushort, ushort[]> blocks;

    public RoomPlmMotherBrainFakeDeathVisualCatalog(
        IEnumerable<RoomPlmMotherBrainFakeDeathVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        blocks = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmMotherBrainFakeDeathVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !MotherBrainFakeDeathPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Mother Brain fake-death visuals changed a draw identity, shape, or visual word.");
            if (!blocks.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Mother Brain fake-death visuals repeat draw {entry.Id}.");
        }
        if (blocks.Count != MotherBrainFakeDeathPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Mother Brain fake-death visuals do not cover all twenty-two draws.");
    }

    public static RoomPlmMotherBrainFakeDeathVisualCatalog Stock() => new(
        MotherBrainFakeDeathPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmMotherBrainFakeDeathVisualEntry(
                MotherBrainFakeDeathPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !MotherBrainFakeDeathPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Mother Brain fake-death visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
