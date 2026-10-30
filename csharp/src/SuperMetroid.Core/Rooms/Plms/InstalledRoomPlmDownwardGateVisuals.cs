using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Presentation-only selection for downward-gate PLM blocks. Physical level appearances,
/// gate collision, shot filters, and animation timing remain compiled.
/// </summary>
public sealed class InstalledRoomPlmDownwardGateVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmDownwardGateVisuals), content =>
    {
        Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[5];
        foreach (var frame in DownwardGatePlmDraws.All.OrderBy(item => DownwardGatePlmDraws.VisualId(item.Draw), StringComparer.Ordinal))
        {
            DownwardGatePlmDraws.TryDescribe(frame.Draw, out var draw);
            content.AppendName("frame", DownwardGatePlmDraws.VisualId(frame.Draw));
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                int count = draw.BlockCount(run);
                for (int cell = 0; cell < count; cell++) buffer[cell] = GetAppearance(frame.Draw, run, cell);
                content.AppendAppearances("blocks", buffer[..count]);
            }
        }
    });

    /// <summary>
    /// Deep-copied visual overrides keyed by draw; null means every
    /// gate frame still takes its appearance from the compiled physical appearances.
    /// </summary>
    private readonly Dictionary<PlmDraw, RoomBlockAppearance[][]>? customBlocks;

    /// <summary>Validates complete downward-gate and shot-trigger coverage and copies visual differences without changing collision, shot filtering, or animation mechanics.</summary>
    /// <param name="entries">One entry per compiled layout, retaining its exact run count and block counts with visual-only payloads.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public InstalledRoomPlmDownwardGateVisuals(IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<PlmDraw, RoomBlockAppearance[][]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !DownwardGatePlmDraws.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.Runs.Length)
                throw new InvalidDataException(
                    "Downward gate visuals changed a compiled frame identity or draw shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                RoomBlockAppearance[] appearances = entry.Runs[run];
                if (appearances is null ||
                    appearances.Length != definition.Runs.Span[run].Count)
                    throw new InvalidDataException(
                        $"Downward gate visuals {entry.Id} change a draw shape.");
            }

            if (!seen.Add(definition.Draw))
                throw new InvalidDataException($"Downward gate visuals repeat frame {entry.Id}.");
            DownwardGatePlmDraws.TryDescribe(definition.Draw, out var draw);
            bool changed = false;
            for (int run = 0; run < entry.Runs.Length; run++)
                for (int cell = 0; cell < entry.Runs[run].Length; cell++)
                    changed |= entry.Runs[run][cell] != draw.BlockAt(run, cell).Appearance;
            if (changed) selected.Add(definition.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != DownwardGatePlmDraws.All.Count())
            throw new InvalidDataException("Downward gate visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves one gate or shot-trigger appearance using the compiled layout's native run structure.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the native draw run.</param>
    /// <param name="blockIndex">Zero-based block ordinal within that run, not a room coordinate.</param>
    /// <returns>The selected appearance, or the compiled terrain's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported gate or shot-trigger layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!DownwardGatePlmDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException($"Downward gate visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(draw, out var runs)
            ? runs[runIndex][blockIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
