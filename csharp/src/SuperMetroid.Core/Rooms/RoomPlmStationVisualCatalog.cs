using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable metatile references for one station animation frame.</summary>
/// <param name="Id">Case-sensitive compiled frame identity for map, energy, missile, save-pod, or left/right station-access artwork.</param>
/// <param name="Runs">Mutable input arrays in native draw-run/cell order, with the compiled run lengths; words contain only ten-bit 16-by-16 metatile indices and two parent flip bits, not collision data or run offsets.</param>
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

    /// <summary>Deep-copied visual run overrides for changed frames, keyed by draw-list pointer; unchanged frames use compiled visual bits.</summary>
    private readonly Dictionary<ushort, ushort[][]>? customWords;

    /// <summary>Validates and captures the complete twenty-frame station artwork selection while retaining native run geometry, collision, activation, and reward behavior.</summary>
    /// <param name="entries">Every compiled map, energy, missile, save, and access frame exactly once, with its original run/cell counts and valid visual-only words.</param>
    /// <remarks>Changed frames are deep-copied; stock-identical frames resolve from compiled definitions. Later caller-array edits do not affect either path.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, frame identity, array shape, or presentation-only word bits are invalid.</exception>
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

    /// <summary>Resolves a station cell's selected appearance, using compiled visual bits for an unchanged frame.</summary>
    /// <param name="drawPointer">Bank-$84 pointer to one of the twenty compiled station draw lists, not an animation instruction-list pointer.</param>
    /// <param name="runIndex">Zero-based native horizontal draw-run ordinal; geometry is frame-specific, with save-pod runs ordered from floor to cap rather than screen-top downward.</param>
    /// <param name="wordIndex">Zero-based left-to-right cell within the selected run's compiled length.</param>
    /// <returns>The twelve-bit metatile/parent-flip reference, without the separately applied physical collision nibble.</returns>
    /// <exception cref="InvalidDataException"><paramref name="drawPointer"/> does not identify a compiled station frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside that frame's shape; the reported parameter is <paramref name="wordIndex"/>.</exception>
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
