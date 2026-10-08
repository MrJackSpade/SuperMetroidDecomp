using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual block references for one downward-gate frame or shot trigger.</summary>
public sealed record RoomPlmDownwardGateVisualEntry(string Id, ushort[][] Runs);

/// <summary>
/// Presentation-only selection for downward-gate PLM blocks. Physical level words,
/// gate collision, shot filters, and animation timing remain compiled.
/// </summary>
public sealed class RoomPlmDownwardGateVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmDownwardGateVisualCatalog), content =>
    {
        Span<ushort> buffer = stackalloc ushort[5];
        foreach (var frame in DownwardGatePlmDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            DownwardGatePlmDrawDefinitions.TryDescribe(frame.Pointer, out var draw);
            content.Append("frame", frame.Pointer);
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                int count = draw.WordCount(run);
                for (int word = 0; word < count; word++) buffer[word] = GetWord(frame.Pointer, run, word);
                content.AppendWords("words", buffer[..count]);
            }
        }
    });

    private readonly Dictionary<ushort, ushort[][]>? customWords;

    public RoomPlmDownwardGateVisualCatalog(IEnumerable<RoomPlmDownwardGateVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        var seen = new HashSet<ushort>();
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

            if (!seen.Add(definition.Pointer))
                throw new InvalidDataException($"Downward gate visuals repeat frame {entry.Id}.");
            DownwardGatePlmDrawDefinitions.TryDescribe(definition.Pointer, out var draw);
            bool changed = false;
            for (int run = 0; run < entry.Runs.Length; run++)
            for (int word = 0; word < entry.Runs[run].Length; word++)
                changed |= entry.Runs[run][word] != new RoomLevelWord(draw.WordAt(run, word)).VisualWord;
            if (changed) selected.Add(definition.Pointer, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != DownwardGatePlmDrawDefinitions.All.Count())
            throw new InvalidDataException("Downward gate visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!DownwardGatePlmDrawDefinitions.TryDescribe(drawPointer, out var draw))
            throw new InvalidDataException($"Downward gate visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.RunCount ||
            (uint)wordIndex >= (uint)draw.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(draw.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
