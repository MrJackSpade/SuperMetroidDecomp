namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only block references for one colored-door orientation and frame.</summary>
public sealed record RoomPlmColoredDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable yellow, green, and red door-cap appearances. Physical level words,
/// projectile filters, opening timing, sound, and persistence remain native logic.
/// </summary>
public sealed class RoomPlmColoredDoorVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmColoredDoorVisualCatalog(
        IEnumerable<RoomPlmColoredDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmColoredDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !ColoredDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Colored-door visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Colored-door visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != ColoredDoorPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Colored-door visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmColoredDoorVisualCatalog Stock() => new(
        ColoredDoorPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmColoredDoorVisualEntry(
                ColoredDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())));

    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words))
            throw new InvalidDataException(
                $"Colored-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= (uint)words.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
