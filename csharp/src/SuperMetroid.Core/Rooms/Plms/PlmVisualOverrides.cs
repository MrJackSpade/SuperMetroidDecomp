using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One PLM family's validated visual-only overrides: the authored blocks of each compiled draw that differs from its
/// stock appearance, keyed by the draw. Collision, programs and run geometry stay compiled.
/// </summary>
internal sealed class PlmVisualOverrides
{
    /// <summary>The changed draws' appearances, flattened in run then block order; null when every draw is stock.</summary>
    private readonly Dictionary<PlmDraw, RoomBlockAppearance[]>? changed;

    /// <summary>
    /// Validates exactly one entry per compiled draw: its identity resolves through <paramref name="lookup"/>, its block
    /// count matches the draw's flattened run blocks, no identity repeats, and the entries cover all
    /// <paramref name="compiledCount"/> draws. Only entries that differ from the stock appearances are kept.
    /// </summary>
    /// <param name="entries">The authored entries.</param>
    /// <param name="family">The PLM family, naming it in a rejection.</param>
    /// <param name="lookup">Resolves an entry's visual identity to its compiled draw list.</param>
    /// <param name="compiledCount">The number of compiled draws the entries must cover.</param>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or coverage is incomplete.</exception>
    internal PlmVisualOverrides(IEnumerable<RoomPlmVisualEntry> entries, string family,
        ShotBlockPlmDraws.VisualLookup lookup, int compiledCount)
    {
        Ensure.NotNull(entries);
        Ensure.NotNull(lookup);
        var selected = new Dictionary<PlmDraw, RoomBlockAppearance[]>();
        var seen = new HashSet<PlmDraw>();
        foreach (RoomPlmVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null || !lookup(entry.Id, out RoomPlmShotBlockDrawDefinitions.DrawList draw) ||
                entry.Blocks.Length != StockBlockCount(draw))
                throw new InvalidDataException($"{family} visuals changed a draw identity or draw shape.");
            if (!seen.Add(draw.Draw))
                throw new InvalidDataException($"{family} visuals repeat draw {entry.Id}.");
            if (DiffersFromStock(entry.Blocks, draw))
                selected.Add(draw.Draw, entry.Blocks.ToArray());
        }
        if (seen.Count != compiledCount)
            throw new InvalidDataException($"{family} visuals do not cover all {compiledCount} compiled draws.");
        if (selected.Count != 0) changed = selected;
    }

    /// <summary>The authored blocks of a draw that differs from stock; false for a stock draw.</summary>
    internal bool TryGet(PlmDraw draw, [NotNullWhen(true)] out RoomBlockAppearance[]? blocks)
    {
        blocks = null;
        return changed is not null && changed.TryGetValue(draw, out blocks);
    }

    /// <summary>The number of blocks across a draw's runs.</summary>
    private static int StockBlockCount(RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        int count = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span) count += run.Count;
        return count;
    }

    /// <summary>Whether authored appearances differ from the appearances of the draw's stock terrain.</summary>
    private static bool DiffersFromStock(RoomBlockAppearance[] blocks, RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        int index = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
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
