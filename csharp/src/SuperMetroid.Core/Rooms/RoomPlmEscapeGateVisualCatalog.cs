namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual block references for one Mother Brain escape-gate frame.</summary>
public sealed record RoomPlmEscapeGateVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Presentation-only selection for the three escape-gate frames. The compiled
/// level words continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomPlmEscapeGateVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmEscapeGateVisualCatalog(IEnumerable<RoomPlmEscapeGateVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmEscapeGateVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !MotherBrainEscapeGatePlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                draw.Runs.Length != 1 ||
                entry.Blocks.Length != draw.Runs.Span[0].LevelWords.Length ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Escape-gate visuals changed a compiled frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException($"Escape-gate visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != MotherBrainEscapeGatePlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Escape-gate visuals do not cover all compiled frames.");
        blocks = selected;
    }

    public static RoomPlmEscapeGateVisualCatalog Stock() => new(
        MotherBrainEscapeGatePlmDrawDefinitions.All.Select(draw =>
            new RoomPlmEscapeGateVisualEntry(
                MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span.ToArray()
                    .Select(word => new RoomLevelWord(word).VisualWord).ToArray())));

    public ushort GetWord(ushort drawPointer, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words))
            throw new InvalidDataException(
                $"Escape-gate visuals lack frame ${drawPointer:X4}.");
        if ((uint)blockIndex >= (uint)words.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
