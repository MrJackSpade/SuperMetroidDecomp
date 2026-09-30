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
    public string ContentIdentity { get; }

    private readonly Dictionary<ushort, ushort[][]> words;

    public RoomPlmElevatorPlatformVisualCatalog(
        IEnumerable<RoomPlmElevatorPlatformVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[][]>();
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

            if (!selected.TryAdd(definition.Pointer,
                    entry.Runs.Select(run => run.ToArray()).ToArray()))
                throw new InvalidDataException(
                    $"Elevator-platform visuals repeat frame {entry.Id}.");
        }

        if (selected.Count != ElevatorPlatformPlmDefinitions.DrawLists.Count())
            throw new InvalidDataException(
                "Elevator-platform visuals do not cover all compiled frames.");
        words = selected;
        ContentIdentity = SelectedPresentationHash.FromWordFrames(nameof(RoomPlmElevatorPlatformVisualCatalog), words);
    }

    public static RoomPlmElevatorPlatformVisualCatalog Stock() => new(
        ElevatorPlatformPlmDefinitions.DrawLists.Select(definition =>
            new RoomPlmElevatorPlatformVisualEntry(
                ElevatorPlatformPlmDefinitions.VisualId(definition.Pointer),
                definition.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray())
                    .ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int wordIndex)
    {
        if (!words.TryGetValue(drawPointer, out ushort[][]? runs))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)runs.Length ||
            (uint)wordIndex >= (uint)runs[runIndex].Length)
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return runs[runIndex][wordIndex];
    }
}
