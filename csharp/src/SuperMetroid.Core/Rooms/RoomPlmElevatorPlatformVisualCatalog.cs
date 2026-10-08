using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable metatile references for one elevator-platform animation frame.</summary>
public sealed record RoomPlmElevatorPlatformVisualEntry(string Id, ushort[][] Runs);

/// <summary>
/// Presentation-only elevator-platform block selection. The native four-frame
/// instruction loop, draw geometry, and physical collision words stay compiled.
/// </summary>
public sealed class RoomPlmElevatorPlatformVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmElevatorPlatformVisualCatalog), content =>
    {
        Span<ushort> buffer = stackalloc ushort[4];
        foreach (var frame in ElevatorPlatformPlmDefinitions.DrawLists.OrderBy(frame => frame.Pointer))
        {
            ElevatorPlatformPlmDefinitions.TryDescribe(frame.Pointer, out var shape);
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

    public RoomPlmElevatorPlatformVisualCatalog(
        IEnumerable<RoomPlmElevatorPlatformVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmElevatorPlatformVisualEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !ElevatorPlatformPlmDefinitions.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.Runs.Length)
                throw new InvalidDataException(
                    "Elevator-platform visuals changed a compiled frame identity or draw shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                ushort[] visualWords = entry.Runs[run];
                if (visualWords is null ||
                    visualWords.Length != definition.Runs.Span[run].LevelWords.Length ||
                    visualWords.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                    throw new InvalidDataException(
                        $"Elevator-platform visuals {entry.Id} contain an invalid visual word.");
            }

            if (!seen.Add(definition.Pointer))
                throw new InvalidDataException(
                    $"Elevator-platform visuals repeat frame {entry.Id}.");
            ElevatorPlatformPlmDefinitions.TryDescribe(definition.Pointer, out var shape);
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
            for (int cell = 0; cell < shape.WordCount(run); cell++)
                changed |= entry.Runs[run][cell] != new RoomLevelWord(shape.WordAt(run, cell)).VisualWord;
            if (changed) selected.Add(definition.Pointer, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != ElevatorPlatformPlmDefinitions.DrawLists.Count())
            throw new InvalidDataException(
                "Elevator-platform visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!ElevatorPlatformPlmDefinitions.TryDescribe(drawPointer, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)wordIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(drawPointer, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(shape.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
