namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual block references for one downward-gate frame or shot trigger.</summary>
public sealed record RoomPlmDownwardGateVisualEntry(string Id, ushort[][] Runs);

/// <summary>
/// Presentation-only selection for downward-gate PLM blocks. Physical level words,
/// gate collision, shot filters, and animation timing remain compiled.
/// </summary>
public sealed class RoomPlmDownwardGateVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[][]> words;

    public RoomPlmDownwardGateVisualCatalog(IEnumerable<RoomPlmDownwardGateVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        foreach (RoomPlmDownwardGateVisualEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !DownwardGatePlmDrawDefinitions.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.Runs.Length)
                throw new InvalidDataException(
                    "Downward gate visuals changed a compiled frame identity or draw shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.Runs.Span[run].LevelWords.Length ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Downward gate visuals {entry.Id} contain an invalid visual word.");
            }

            if (!selected.TryAdd(definition.Pointer,
                    entry.Runs.Select(run => run.ToArray()).ToArray()))
                throw new InvalidDataException($"Downward gate visuals repeat frame {entry.Id}.");
        }

        if (selected.Count != DownwardGatePlmDrawDefinitions.All.Count())
            throw new InvalidDataException("Downward gate visuals do not cover all compiled frames.");
        words = selected;
    }

    public static RoomPlmDownwardGateVisualCatalog Stock() => new(
        DownwardGatePlmDrawDefinitions.All.Select(definition =>
            new RoomPlmDownwardGateVisualEntry(
                DownwardGatePlmDrawDefinitions.VisualId(definition.Pointer),
                definition.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray())
                    .ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!words.TryGetValue(drawPointer, out ushort[][]? runs))
            throw new InvalidDataException($"Downward gate visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)runs.Length ||
            (uint)wordIndex >= (uint)runs[runIndex].Length)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return runs[runIndex][wordIndex];
    }
}
