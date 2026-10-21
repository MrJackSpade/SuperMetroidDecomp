using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// Presentation-only elevator-platform block selection. The native four-step
/// instruction loop, draw geometry, and physical collision words stay compiled.
/// </summary>
public sealed class RoomPlmElevatorPlatformVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmElevatorPlatformVisualCatalog), content =>
    {
        Span<ushort> buffer = stackalloc ushort[4];
        foreach (var frame in ElevatorPlatformPlmDefinitions.DrawLists.OrderBy(item => ElevatorPlatformPlmDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            ElevatorPlatformPlmDefinitions.TryDescribe(frame.Draw, out var shape);
            content.AppendName("frame", ElevatorPlatformPlmDefinitions.VisualId(frame.Draw));
            content.Append("runs", shape.RunCount);
            for (int run = 0; run < shape.RunCount; run++)
            {
                int count = shape.WordCount(run);
                for (int cell = 0; cell < count; cell++) buffer[cell] = GetWord(frame.Draw, run, cell);
                content.AppendWords("words", buffer[..count]);
            }
        }
    });

    /// <summary>Only frames whose visual words differ from the compiled source are stored here.</summary>
    private readonly Dictionary<PlmDraw, ushort[][]>? customWords;

    /// <summary>Validates all three elevator-platform draw layouts and copies visual differences while retaining native run geometry, collision words, and animation cadence.</summary>
    /// <remarks>The four-step instruction loop selects first, second, third, then second layout; the repeated second layout has only one entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled frame, with its exact native run and word counts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    public RoomPlmElevatorPlatformVisualCatalog(
        IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<PlmDraw, ushort[][]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
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

            if (!seen.Add(definition.Draw))
                throw new InvalidDataException(
                    $"Elevator-platform visuals repeat frame {entry.Id}.");
            ElevatorPlatformPlmDefinitions.TryDescribe(definition.Draw, out var shape);
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
                for (int cell = 0; cell < shape.WordCount(run); cell++)
                    changed |= entry.Runs[run][cell] != new RoomLevelWord(shape.WordAt(run, cell)).VisualWord;
            if (changed) selected.Add(definition.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != ElevatorPlatformPlmDefinitions.DrawLists.Count())
            throw new InvalidDataException(
                "Elevator-platform visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Resolves one elevator-platform visual block without replacing its compiled physical word or draw offset.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the native draw run.</param>
    /// <param name="wordIndex">Zero-based word ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected metatile/flip word, derived from the compiled stock frame when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int wordIndex)
    {
        if (!ElevatorPlatformPlmDefinitions.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)wordIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(draw, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(shape.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
