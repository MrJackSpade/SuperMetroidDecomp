namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual block references for one compiled bank-$84 shot-block draw list.</summary>
/// <remarks>
/// Each value is only the low twelve bits of a level word: ten block-index bits and
/// two parent flips. Collision type, draw geometry, timers, and slot effects stay in
/// <see cref="RoomPlmShotBlockDrawDefinitions"/> and cannot be changed by this catalog.
/// </remarks>
public sealed record RoomPlmShotBlockVisualEntry(ushort DrawPointer, ushort[][] Runs);

/// <summary>Complete, immutable presentation selection for the 19 ordinary shot-block lists.</summary>
public sealed class RoomPlmShotBlockVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[][]> words;

    public RoomPlmShotBlockVisualCatalog(IEnumerable<RoomPlmShotBlockVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        foreach (RoomPlmShotBlockVisualEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !RoomPlmShotBlockDrawDefinitions.TryGet(entry.DrawPointer, out var definition) ||
                entry.Runs.Length != definition.Runs.Length)
                throw new InvalidDataException("Shot-block visuals changed a compiled draw-list identity or shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.Runs.Span[run].LevelWords.Length ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Shot-block visuals ${entry.DrawPointer:X4} contain an invalid visual word.");
            }

            if (!selected.TryAdd(entry.DrawPointer,
                    entry.Runs.Select(run => run.ToArray()).ToArray()))
                throw new InvalidDataException(
                    $"Shot-block visuals repeat draw list ${entry.DrawPointer:X4}.");
        }

        if (selected.Count != RoomPlmShotBlockDrawDefinitions.All.Count())
            throw new InvalidDataException("Shot-block visuals do not cover all compiled draw lists.");
        words = selected;
    }

    /// <summary>Native visual selections, useful when no installed override is present.</summary>
    public static RoomPlmShotBlockVisualCatalog Stock() => new(
        RoomPlmShotBlockDrawDefinitions.All.Select(definition =>
            new RoomPlmShotBlockVisualEntry(definition.Pointer,
                definition.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray())
                    .ToArray())));

    /// <summary>Returns one visual reference without exposing the catalog's mutable backing arrays.</summary>
    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!words.TryGetValue(drawPointer, out ushort[][]? runs))
            throw new InvalidDataException($"Shot-block visuals lack draw list ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)runs.Length ||
            (uint)wordIndex >= (uint)runs[runIndex].Length)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return runs[runIndex][wordIndex];
    }
}
