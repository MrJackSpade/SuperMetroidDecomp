namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for a grey-door cap or shared door-clear frame.</summary>
public sealed record RoomPlmGreyDoorVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable grey caps and door-clear appearances. Native PLM level words,
/// collision, condition gates, timing, sound, and persistence remain unchanged.
/// </summary>
public sealed class RoomPlmGreyDoorVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmGreyDoorVisualCatalog(IEnumerable<RoomPlmGreyDoorVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmGreyDoorVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !GreyDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Grey-door visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Grey-door visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != GreyDoorPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Grey-door visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmGreyDoorVisualCatalog Stock() => new(
        GreyDoorPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmGreyDoorVisualEntry(
                GreyDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())));

    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words))
            throw new InvalidDataException($"Grey-door visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= (uint)words.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
