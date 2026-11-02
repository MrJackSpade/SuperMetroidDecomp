using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmShotBlockDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Validates one PLM family's authored visual entries against its compiled draws before the game's visuals are built:
/// every identity resolves to a compiled draw, every draw keeps its shape, no identity repeats and every compiled draw is
/// covered. Only the draws whose authored appearances differ from stock are kept.
/// </summary>
internal static class RoomPlmVisualOverrideReader
{
    /// <summary>Validates entries whose blocks are flattened in run then block order.</summary>
    /// <param name="entries">The authored entries.</param>
    /// <param name="family">The PLM family, naming it in a rejection.</param>
    /// <param name="lookup">Resolves an entry's visual identity to its compiled draw list.</param>
    /// <param name="compiledCount">The number of compiled draws the entries must cover.</param>
    /// <returns>Owned copies of the changed draws' appearances.</returns>
    /// <exception cref="ArgumentNullException">The entry sequence or lookup is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or coverage is incomplete.</exception>
    internal static PlmVisualOverrides Flattened(IEnumerable<RoomPlmVisualEntry> entries, string family,
        RoomPlmDrawListExport.VisualLookup lookup, int compiledCount)
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(lookup);
        var selected = new Dictionary<PlmDraw, RoomBlockAppearance[]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null || !lookup(entry.Id, out DrawList draw) ||
                entry.Blocks.Length != StockBlockCount(draw))
                throw new InvalidDataException($"{family} visuals changed a draw identity or draw shape.");
            if (!seen.Add(draw.Draw))
                throw new InvalidDataException($"{family} visuals repeat draw {entry.Id}.");
            if (DiffersFromStock(entry.Blocks, draw))
                selected.Add(draw.Draw, entry.Blocks.ToArray());
        }
        if (seen.Count != compiledCount)
            throw new InvalidDataException($"{family} visuals do not cover all {compiledCount} compiled draws.");
        return new PlmVisualOverrides(selected);
    }

    /// <summary>Validates entries whose blocks keep each compiled run separate.</summary>
    /// <param name="entries">The authored entries.</param>
    /// <param name="family">The PLM family, naming it in a rejection.</param>
    /// <param name="lookup">Resolves an entry's visual identity to its compiled draw list.</param>
    /// <param name="compiledCount">The number of compiled draws the entries must cover.</param>
    /// <returns>Owned run-by-run copies of the changed draws' appearances, keyed by draw.</returns>
    /// <exception cref="ArgumentNullException">The entry sequence or lookup is null.</exception>
    /// <exception cref="InvalidDataException">An entry or run is invalid, an identity repeats, a run shape differs, or coverage is incomplete.</exception>
    internal static Dictionary<PlmDraw, RoomBlockAppearance[][]> RunByRun(IEnumerable<RoomPlmVisualRunEntry> entries, string family,
        RoomPlmDrawListExport.VisualLookup lookup, int compiledCount)
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(lookup);
        var selected = new Dictionary<PlmDraw, RoomBlockAppearance[][]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualRunEntry entry in entries)
        {
            if (entry is null || entry.Runs is null || !lookup(entry.Id, out DrawList draw) ||
                entry.Runs.Length != draw.Runs.Length)
                throw new InvalidDataException($"{family} visuals changed a compiled frame identity or draw shape.");
            bool changed = false;
            for (int run = 0; run < entry.Runs.Length; run++)
            {
                RoomBlockAppearance[] appearances = entry.Runs[run];
                ReadOnlySpan<RoomTerrainBlock> stock = draw.Runs.Span[run].Blocks.Span;
                if (appearances is null || appearances.Length != stock.Length)
                    throw new InvalidDataException($"{family} visuals {entry.Id} change a draw shape.");
                for (int cell = 0; cell < appearances.Length; cell++)
                    changed |= appearances[cell] != stock[cell].Appearance;
            }
            if (!seen.Add(draw.Draw))
                throw new InvalidDataException($"{family} visuals repeat frame {entry.Id}.");
            if (changed) selected.Add(draw.Draw, entry.Runs.Select(run => run.ToArray()).ToArray());
        }
        if (seen.Count != compiledCount)
            throw new InvalidDataException($"{family} visuals do not cover all compiled frames.");
        return selected;
    }

    /// <summary>The number of blocks across a draw's runs.</summary>
    private static int StockBlockCount(DrawList draw)
    {
        int count = 0;
        foreach (Run run in draw.Runs.Span) count += run.Count;
        return count;
    }

    /// <summary>Whether authored appearances differ from the appearances of the draw's stock terrain.</summary>
    private static bool DiffersFromStock(RoomBlockAppearance[] blocks, DrawList draw)
    {
        int index = 0;
        foreach (Run run in draw.Runs.Span)
        {
            foreach (RoomTerrainBlock block in run.Blocks.Span)
            {
                if (blocks[index++] != block.Appearance)
                    return true;
            }
        }
        return false;
    }
}
