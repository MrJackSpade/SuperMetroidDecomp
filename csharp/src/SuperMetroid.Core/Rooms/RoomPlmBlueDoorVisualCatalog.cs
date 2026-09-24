namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only tile references for one blue-door orientation and frame.</summary>
public sealed record RoomPlmBlueDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable blue-door cap appearance. The compiled PLM draw definitions retain the
/// physical level words; door collision, opening cadence, and sound are unaffected.
/// </summary>
public sealed class RoomPlmBlueDoorVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmBlueDoorVisualCatalog(IEnumerable<RoomPlmBlueDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmBlueDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !BlueDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Blue-door visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException($"Blue-door visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != BlueDoorPlmDrawDefinitions.All.Count())
            throw new InvalidDataException("Blue-door visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmBlueDoorVisualCatalog Stock() => new(
        BlueDoorPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmBlueDoorVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())));

    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words))
            throw new InvalidDataException($"Blue-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= (uint)words.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
