using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete station presentation selection. Native
/// animation, activation, rewards, save behavior and collision stay compiled.
/// </summary>
public sealed class InstalledRoomPlmStationVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmStationVisuals), content =>
    {
        Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[2];
        foreach (var frame in StationPlmDraws.All.OrderBy(item => StationPlmDraws.VisualId(item.Draw), StringComparer.Ordinal))
        {
            var shape = StationPlmDraws.Describe(frame.Draw);
            content.AppendName("frame", StationPlmDraws.VisualId(frame.Draw));
            content.Append("runs", shape.RunCount);
            for (int run = 0; run < shape.RunCount; run++)
            {
                int count = shape.BlockCount(run);
                for (int cell = 0; cell < count; cell++) buffer[cell] = GetAppearance(frame.Draw, run, cell);
                content.AppendAppearances("blocks", buffer[..count]);
            }
        }
    });

    /// <summary>Deep-copied visual overrides keyed by draw; absent frames use stock artwork.</summary>
    private readonly Dictionary<PlmDraw, RoomBlockAppearance[][]>? customBlocks;

    /// <summary>Validates and captures the complete twenty-frame station artwork selection while retaining native run geometry, collision, activation, and reward behavior.</summary>
    /// <param name="entries">Every compiled map, energy, missile, save, and access frame exactly once, with its original run/cell counts and valid visual-only appearances.</param>
    /// <remarks>Changed frames are deep-copied; stock-identical frames resolve from compiled definitions. Later caller-array edits do not affect either path.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, frame identity, array shape, or appearances are invalid.</exception>
    public InstalledRoomPlmStationVisuals(
        IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<PlmDraw, RoomBlockAppearance[][]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !StationPlmDraws.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.Runs.Length)
                throw new InvalidDataException(
                    "Station visuals changed a compiled frame identity or draw shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                RoomBlockAppearance[] appearances = entry.Runs[run];
                if (appearances is null ||
                    appearances.Length != definition.Runs.Span[run].Count)
                    throw new InvalidDataException(
                        $"Station visuals {entry.Id} change a draw shape.");
            }

            if (!seen.Add(definition.Draw))
                throw new InvalidDataException(
                    $"Station visuals repeat frame {entry.Id}.");
            var shape = StationPlmDraws.Describe(definition.Draw);
            bool changed = false;
            for (int run = 0; run < shape.RunCount; run++)
                for (int cell = 0; cell < shape.BlockCount(run); cell++)
                    changed |= entry.Runs[run][cell] != shape.BlockAt(run, cell).Appearance;
            if (changed) selected.Add(definition.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != StationPlmDraws.All.Count())
            throw new InvalidDataException(
                "Station visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves a station cell's selected appearance, using compiled appearance for an unchanged frame.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based native horizontal draw-run ordinal; geometry is frame-specific, with save-pod runs ordered from floor to cap rather than screen-top downward.</param>
    /// <param name="blockIndex">Zero-based left-to-right cell within the selected run's compiled length.</param>
    /// <returns>The twelve-bit metatile/parent-flip reference, without the separately applied physical collision nibble.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> does not identify a compiled station frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside that frame's shape; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!StationPlmDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Station visuals lack frame {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlockCount(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(draw, out var runs)
            ? runs[runIndex][blockIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
