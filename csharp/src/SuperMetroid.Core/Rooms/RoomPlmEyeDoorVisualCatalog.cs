namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual blocks for one eye-door draw frame.</summary>
public sealed record RoomPlmEyeDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only choices for the mirrored eye, middle and bottom components.
/// Door collision, attack logic, hit counters, timers and persistence stay compiled.
/// </summary>
public sealed class RoomPlmEyeDoorVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmEyeDoorVisualCatalog(IEnumerable<RoomPlmEyeDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmEyeDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !EyeDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Eye-door visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Eye-door visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != EyeDoorPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Eye-door visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmEyeDoorVisualCatalog Stock() => new(
        EyeDoorPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmEyeDoorVisualEntry(
                EyeDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())));

    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words))
            throw new InvalidDataException($"Eye-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= (uint)words.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
