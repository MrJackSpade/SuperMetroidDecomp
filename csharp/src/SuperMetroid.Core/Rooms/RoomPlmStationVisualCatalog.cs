namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual block references for one named station frame or access state.</summary>
public sealed record RoomPlmStationVisualEntry(string Id, ushort[][] Runs);

/// <summary>
/// Complete station presentation selection. Native level words, collision, animation
/// timing, station activation, rewards, and save behavior remain compiled.
/// </summary>
public sealed class RoomPlmStationVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[][]> words;

    public RoomPlmStationVisualCatalog(IEnumerable<RoomPlmStationVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        foreach (RoomPlmStationVisualEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !RoomPlmStationDrawDefinitions.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.Runs.Length)
                throw new InvalidDataException(
                    "Station visuals changed a compiled frame identity or draw shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.Runs.Span[run].LevelWords.Length ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Station visuals {entry.Id} contain an invalid visual word.");
            }

            if (!selected.TryAdd(definition.Pointer,
                    entry.Runs.Select(run => run.ToArray()).ToArray()))
                throw new InvalidDataException($"Station visuals repeat frame {entry.Id}.");
        }

        if (selected.Count != RoomPlmStationDrawDefinitions.All.Count())
            throw new InvalidDataException("Station visuals do not cover all compiled frames.");
        words = selected;
    }

    public static RoomPlmStationVisualCatalog Stock() => new(
        RoomPlmStationDrawDefinitions.All.Select(definition =>
            new RoomPlmStationVisualEntry(
                RoomPlmStationDrawDefinitions.VisualId(definition.Pointer),
                definition.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray())
                    .ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!words.TryGetValue(drawPointer, out ushort[][]? runs))
            throw new InvalidDataException($"Station visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)runs.Length ||
            (uint)wordIndex >= (uint)runs[runIndex].Length)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return runs[runIndex][wordIndex];
    }
}
