using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Complete, immutable presentation selection for the 19 ordinary shot-block lists.</summary>
public sealed class InstalledRoomPlmShotBlockVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmShotBlockVisuals), content =>
    {
        Span<RoomBlockAppearance> row = stackalloc RoomBlockAppearance[2];
        foreach (var draw in ShotBlockPlmDraws.Calculated.OrderBy(ShotBlockPlmDraws.VisualId, StringComparer.Ordinal))
        {
            content.AppendName("frame", ShotBlockPlmDraws.VisualId(draw));
            content.Append("runs", draw.RunCount);
            for (int run = 0; run < draw.RunCount; run++)
            {
                for (int block = 0; block < draw.BlocksPerRun; block++)
                    row[block] = GetAppearance(PlmDraw.Of(draw), run, block);
                content.AppendAppearances("blocks", row[..draw.BlocksPerRun]);
            }
        }
    });

    /// <summary>Deep-copied edited run appearances; stock-identical draw lists stay calculated.</summary>
    private readonly Dictionary<ShotBlockPlmDraws.Draw, RoomBlockAppearance[][]>? customBlocks;

    /// <summary>Validates visual references for all nineteen compiled breakup/restoration draw lists, deep-copying edited runs while retaining calculated stock visuals where supplied appearances match; compiled collision and run placement are never replaced.</summary>
    /// <param name="entries">Complete, duplicate-free draw identity set with unchanged run counts and lengths.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">An entry/run is null, a draw identity is unknown/duplicated/missing, the run shape differs, or a block count differs.</exception>
    public InstalledRoomPlmShotBlockVisuals(IEnumerable<RoomPlmVisualRunEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<ShotBlockPlmDraws.Draw, RoomBlockAppearance[][]>();
        var seen = new HashSet<ShotBlockPlmDraws.Draw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null ||
                !ShotBlockPlmDraws.TryGetByVisualId(entry.Id, out var definition) ||
                entry.Runs.Length != definition.RunCount)
                throw new InvalidDataException("Shot-block visuals changed a compiled draw-list identity or shape.");
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                RoomBlockAppearance[] appearances = entry.Runs[run];
                if (appearances is null ||
                    appearances.Length != definition.BlocksPerRun)
                    throw new InvalidDataException(
                        $"Shot-block visuals {entry.Id} change a draw shape.");
            }

            if (!seen.Add(definition))
                throw new InvalidDataException($"Shot-block visuals repeat draw {entry.Id}.");
            bool stock = true;
            for (int run = 0; run < definition.RunCount; run++)
                for (int block = 0; block < definition.BlocksPerRun; block++)
                    stock &= entry.Runs[run][block] == definition.BlockAt(run, block).Appearance;
            if (!stock) selected.Add(definition, entry.Runs.Select(run => run.ToArray()).ToArray());
        }

        if (seen.Count != RoomPlmShotBlockDrawDefinitions.DrawCount)
            throw new InvalidDataException("Shot-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Returns one visual reference without exposing the catalog's mutable backing arrays.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (draw is not PlmFrameDraw<ShotBlockPlmDraws.Draw> { Frame: var shape })
            throw new InvalidDataException($"Shot-block visuals lack draw {draw}.");
        if ((uint)runIndex >= (uint)shape.RunCount ||
            (uint)blockIndex >= (uint)shape.BlocksPerRun)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return customBlocks is not null && customBlocks.TryGetValue(shape, out var runs)
            ? runs[runIndex][blockIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
