using SuperMetroid.Core.Assets;

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
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmShotBlockVisualCatalog), content =>
    {
        Span<ushort> row = stackalloc ushort[2];
        foreach (var draw in RoomPlmShotBlockDrawDefinitions.Calculated.OrderBy(draw => draw.Pointer))
        {
            content.Append("frame", draw.Pointer);
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                for (int block = 0; block < draw.WordsPerRun; block++)
                    row[block] = GetWord(draw.Pointer, run, block);
                content.AppendWords("words", row[..draw.WordsPerRun]);
            }
        }
    });

    private readonly Dictionary<ushort, ushort[][]>? customWords;

    private RoomPlmShotBlockVisualCatalog() { }

    public RoomPlmShotBlockVisualCatalog(IEnumerable<RoomPlmShotBlockVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmShotBlockVisualEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !RoomPlmShotBlockDrawDefinitions.TryGet(entry.DrawPointer, out var definition) ||
                entry.Runs.Length != definition.RunCount)
                throw new InvalidDataException("Shot-block visuals changed a compiled draw-list identity or shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.WordsPerRun ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Shot-block visuals ${entry.DrawPointer:X4} contain an invalid visual word.");
            }

            if (!seen.Add(entry.DrawPointer))
                throw new InvalidDataException(
                    $"Shot-block visuals repeat draw list ${entry.DrawPointer:X4}.");
            bool stock = true;
            for (int run = 0; run < definition.RunCount; run++)
            for (int block = 0; block < definition.WordsPerRun; block++)
                stock &= entry.Runs[run][block] == new RoomLevelWord(definition.WordAt(run, block)).VisualWord;
            if (!stock) selected.Add(entry.DrawPointer, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != RoomPlmShotBlockDrawDefinitions.DrawCount)
            throw new InvalidDataException("Shot-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Native visual selections, useful when no installed override is present.</summary>
    public static RoomPlmShotBlockVisualCatalog Stock() => new();

    /// <summary>Returns one visual reference without exposing the catalog's mutable backing arrays.</summary>
    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!RoomPlmShotBlockDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException($"Shot-block visuals lack draw list ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)wordIndex >= (uint)draw.WordsPerRun)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(draw.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
