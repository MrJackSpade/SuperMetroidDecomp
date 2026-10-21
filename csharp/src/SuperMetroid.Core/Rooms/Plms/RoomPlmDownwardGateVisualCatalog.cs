using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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
        foreach (var frame in DownwardGatePlmDrawDefinitions.All.OrderBy(item => DownwardGatePlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            DownwardGatePlmDrawDefinitions.TryDescribe(frame.Draw, out var draw);
            content.AppendName("frame", DownwardGatePlmDrawDefinitions.VisualId(frame.Draw));
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                int count = draw.WordCount(run);
                for (int word = 0; word < count; word++) buffer[word] = GetWord(frame.Draw, run, word);
                content.AppendWords("words", buffer[..count]);
            }
        }
    });

    /// <summary>
    /// Deep-copied visual overrides keyed by draw; null means every
    /// gate frame still takes its visual bits from the compiled physical words.
    /// </summary>
    private readonly Dictionary<PlmDraw, ushort[][]>? customWords;

    /// <summary>Validates complete downward-gate and shot-trigger coverage and copies visual differences without changing collision, shot filtering, or animation mechanics.</summary>
    /// <param name="entries">One entry per compiled layout, retaining its exact run count and word counts with visual-only payloads.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmDownwardGateVisualCatalog(IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<PlmDraw, ushort[][]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
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

            if (!seen.Add(definition.Draw))
                throw new InvalidDataException($"Downward gate visuals repeat frame {entry.Id}.");
            DownwardGatePlmDrawDefinitions.TryDescribe(definition.Draw, out var draw);
            bool changed = false;
            for (int run = 0; run < entry.Runs.Length; run++)
                for (int word = 0; word < entry.Runs[run].Length; word++)
                    changed |= entry.Runs[run][word] != new RoomLevelWord(draw.WordAt(run, word)).VisualWord;
            if (changed) selected.Add(definition.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != DownwardGatePlmDrawDefinitions.All.Count())
            throw new InvalidDataException("Downward gate visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Resolves one gate or shot-trigger visual word using the compiled layout's native run structure.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the native draw run.</param>
    /// <param name="wordIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected metatile/flip word, or the compiled physical word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported gate or shot-trigger layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int wordIndex)
    {
        if (!DownwardGatePlmDrawDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException($"Downward gate visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)wordIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(draw, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(shape.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
