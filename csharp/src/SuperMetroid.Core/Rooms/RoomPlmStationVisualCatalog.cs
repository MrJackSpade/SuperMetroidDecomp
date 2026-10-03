using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable metatile references for one station animation frame.</summary>
public sealed record RoomPlmStationVisualEntry(string Id, ushort[][] Runs);

/// <summary>
/// Complete station presentation selection. Native
/// animation, activation, rewards, save behavior and collision stay compiled.
/// </summary>
public sealed class RoomPlmStationVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmStationVisualCatalog), content =>
    {
        Span<ushort> buffer = stackalloc ushort[2];
        foreach (var frame in RoomPlmStationDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            RoomPlmStationDrawDefinitions.TryDescribe(frame.Pointer, out var shape);
            content.Append("frame", frame.Pointer);
            content.Append("runs", shape.RunCount);
            for (int run = 0; run < shape.RunCount; run++)
            {
                int count = shape.WordCount(run);
                for (int cell = 0; cell < count; cell++) buffer[cell] = GetWord(frame.Pointer, run, cell);
                content.AppendWords("words", buffer[..count]);
            }
        }
    });

    private readonly Dictionary<ushort, ushort[][]>? customWords;

    private RoomPlmStationVisualCatalog() { }

    public RoomPlmStationVisualCatalog(
        IEnumerable<RoomPlmStationVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        var seen = new HashSet<ushort>();
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

            if (!seen.Add(definition.Pointer))
                throw new InvalidDataException(
                    $"Station visuals repeat frame {entry.Id}.");
            RoomPlmStationDrawDefinitions.TryDescribe(definition.Pointer, out var shape);
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
            for (int cell = 0; cell < shape.WordCount(run); cell++)
                changed |= entry.Runs[run][cell] != new RoomLevelWord(shape.WordAt(run, cell)).VisualWord;
            if (changed) selected.Add(definition.Pointer, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != RoomPlmStationDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Station visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Calculate stock appearances directly; retain only customized frames.</summary>
    public static RoomPlmStationVisualCatalog Stock() => new();

    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!RoomPlmStationDrawDefinitions.TryDescribe(drawPointer, out var shape))
            throw new InvalidDataException(
                $"Station visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)wordIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(shape.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
