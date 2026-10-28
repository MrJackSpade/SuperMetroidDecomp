using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The elevator platform's appearance: the authored visual words that differ from the compiled frames, a cell's
/// selected visual word, and the identity of the selected frames.
/// </summary>
internal static class ElevatorPlatformArtwork
{
    /// <summary>Hashes each frame's selected visual words run by run, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmElevatorPlatformVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmElevatorPlatformVisualCatalog), content =>
        {
            Span<ushort> buffer = stackalloc ushort[4];
            foreach (var frame in ElevatorPlatformDraws.DrawLists.OrderBy(item => ElevatorPlatformDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                ElevatorPlatformDraws.TryDescribe(frame.Draw, out var shape);
                content.AppendName("frame", ElevatorPlatformDraws.VisualId(frame.Draw));
                content.Append("runs", shape.RunCount);
                for (int run = 0; run < shape.RunCount; run++)
                {
                    int count = shape.WordCount(run);
                    for (int cell = 0; cell < count; cell++) buffer[cell] = visuals.GetWord(frame.Draw, run, cell);
                    content.AppendWords("words", buffer[..count]);
                }
            }
        });

    /// <summary>
    /// Validates all three elevator-platform draw layouts and copies the frames whose visual words differ from the
    /// compiled source; null when none differ.
    /// </summary>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static Dictionary<PlmDraw, ushort[][]>? Overrides(IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<PlmDraw, ushort[][]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !ElevatorPlatformDraws.TryGetByVisualId(entry.Id, out var definition) ||
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
            ElevatorPlatformDraws.TryDescribe(definition.Draw, out var shape);
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
                for (int cell = 0; cell < shape.WordCount(run); cell++)
                    changed |= entry.Runs[run][cell] != new RoomLevelWord(shape.WordAt(run, cell)).VisualWord;
            if (changed) selected.Add(definition.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != ElevatorPlatformDraws.DrawLists.Count())
            throw new InvalidDataException(
                "Elevator-platform visuals do not cover all compiled frames.");
        return selected.Count != 0 ? selected : null;
    }

    /// <summary>Resolves one elevator-platform visual block without replacing its compiled physical word or draw offset.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal static ushort Word(Dictionary<PlmDraw, ushort[][]>? customWords, PlmDraw draw, int runIndex, int wordIndex)
    {
        if (!ElevatorPlatformDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)wordIndex >= (uint)shape.WordCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(wordIndex));
        return customWords is not null && customWords.TryGetValue(draw, out var runs)
            ? runs[runIndex][wordIndex] : new RoomLevelWord(shape.WordAt(runIndex, wordIndex)).VisualWord;
    }
}
