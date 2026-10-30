using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator.Artwork;

/// <summary>
/// The elevator platform's appearance: the authored appearances that differ from the compiled frames, a cell's
/// selected appearance, and the identity of the selected frames.
/// </summary>
internal static class ElevatorPlatformArtwork
{
    /// <summary>Hashes each frame's selected appearances run by run, in visual-identity order.</summary>
    internal static string ContentIdentity(RoomPlmElevatorPlatformVisualCatalog visuals) =>
        SelectedPresentationHash.Create(nameof(RoomPlmElevatorPlatformVisualCatalog), content =>
        {
            var buffer = new RoomBlockAppearance[4];
            foreach (var frame in ElevatorPlatformDraws.DrawLists.OrderBy(item => ElevatorPlatformDraws.VisualId(item.Draw), StringComparer.Ordinal))
            {
                ElevatorPlatformDraws.TryDescribe(frame.Draw, out var shape);
                content.AppendName("frame", ElevatorPlatformDraws.VisualId(frame.Draw));
                content.Append("runs", shape.RunCount);
                for (int run = 0; run < shape.RunCount; run++)
                {
                    int count = shape.BlockCount(run);
                    for (int cell = 0; cell < count; cell++)
                        buffer[cell] = visuals.GetAppearance(frame.Draw, run, cell) ??
                            throw new InvalidDataException($"Elevator-platform visuals lack frame {frame.Draw}.");
                    content.AppendAppearances("blocks", buffer.AsSpan(0, count));
                }
            }
        });

    /// <summary>
    /// Validates all three elevator-platform draw layouts and copies the frames whose appearances differ from the
    /// compiled source; null when none differ.
    /// </summary>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static Dictionary<PlmDraw, RoomBlockAppearance[][]>? Overrides(IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<PlmDraw, RoomBlockAppearance[][]>();
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
                RoomBlockAppearance[] appearances = entry.Runs[run];
                if (appearances is null ||
                    appearances.Length != definition.Runs.Span[run].Blocks.Length)
                    throw new InvalidDataException(
                        $"Elevator-platform visuals {entry.Id} change a run's block count.");
            }

            if (!seen.Add(definition.Draw))
                throw new InvalidDataException(
                    $"Elevator-platform visuals repeat frame {entry.Id}.");
            ElevatorPlatformDraws.TryDescribe(definition.Draw, out var shape);
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
                for (int cell = 0; cell < shape.BlockCount(run); cell++)
                    changed |= entry.Runs[run][cell] != shape.BlockAt(run, cell).Appearance;
            if (changed) selected.Add(definition.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != ElevatorPlatformDraws.DrawLists.Count())
            throw new InvalidDataException(
                "Elevator-platform visuals do not cover all compiled frames.");
        return selected.Count != 0 ? selected : null;
    }

    /// <summary>Resolves one elevator-platform block appearance without replacing its compiled terrain block or draw offset.</summary>
    /// <exception cref="InvalidDataException">The draw is not a supported platform frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal static RoomBlockAppearance Appearance(Dictionary<PlmDraw, RoomBlockAppearance[][]>? customAppearances, PlmDraw draw, int runIndex, int cellIndex)
    {
        if (!ElevatorPlatformDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Elevator-platform visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)cellIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(cellIndex));
        return customAppearances is not null && customAppearances.TryGetValue(draw, out var runs)
            ? runs[runIndex][cellIndex] : shape.BlockAt(runIndex, cellIndex).Appearance;
    }
}
