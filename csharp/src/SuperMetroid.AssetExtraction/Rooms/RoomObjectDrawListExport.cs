using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Builds the run-by-run draw lists the PLM visual files export and check, from the shapes the game draws, and finds a
/// family's draw by its published visual identity. The game draws from the shapes directly and never builds these lists.
/// </summary>
internal static class RoomObjectDrawListExport
{
    /// <summary>Resolves a published visual identity to its compiled draw list.</summary>
    internal delegate bool VisualLookup(string id, out DrawList list);

    /// <summary>A lookup of a compiled draw list by its draw.</summary>
    internal delegate bool DrawListLookup(RoomObjectDraw draw, out DrawList list);

    /// <summary>The direction of every run of a draw that writes its runs along their rows.</summary>
    internal static readonly Func<int, RoomObjectRunDirection> Horizontal = static _ => RoomObjectRunDirection.Horizontal;

    /// <summary>Builds the draw list of a draw whose runs are described one by one.</summary>
    /// <param name="draw">The draw the list exports.</param>
    /// <param name="runCount">The number of runs the draw writes.</param>
    /// <param name="blockCount">The number of blocks in a run.</param>
    /// <param name="blockAt">The terrain at a cell of a run.</param>
    /// <param name="direction">The direction a run writes its blocks in.</param>
    /// <param name="nextX">The horizontal block offset after a run.</param>
    /// <param name="nextY">The vertical block offset after a run.</param>
    internal static DrawList Runs(RoomObjectDraw draw, int runCount, Func<int, int> blockCount, Func<int, int, RoomTerrainBlock> blockAt,
        Func<int, RoomObjectRunDirection> direction, Func<int, int> nextX, Func<int, int> nextY)
    {
        var runs = new Run[runCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[blockCount(run)];
            for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = blockAt(run, cell);
            runs[run] = new(direction(run), blocks, new(nextX(run), nextY(run)));
        }
        return new(draw, runs);
    }

    /// <summary>Builds the draw list of a draw that writes one run and ends there.</summary>
    /// <param name="draw">The draw the list exports.</param>
    /// <param name="direction">The direction the run writes its blocks in.</param>
    /// <param name="blocks">The terrain the run writes, one per block.</param>
    internal static DrawList SingleRun(RoomObjectDraw draw, RoomObjectRunDirection direction, RoomTerrainBlock[] blocks) =>
        new(draw, new Run[] { new(direction, blocks, new(0, 0)) });

    /// <summary>Builds the one four-block run of a door cap, down its column or along its row.</summary>
    /// <param name="draw">The cap's draw.</param>
    /// <param name="vertical">Whether the cap writes its blocks down a column.</param>
    /// <param name="blockAt">The terrain of one of the cap's four cells in run order.</param>
    internal static DrawList Cap(RoomObjectDraw draw, bool vertical, Func<int, RoomTerrainBlock> blockAt)
    {
        var blocks = new RoomTerrainBlock[4];
        for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = blockAt(cell);
        return SingleRun(draw, vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, blocks);
    }

    /// <summary>Finds the draw list among <paramref name="candidates"/> whose published visual identity is <paramref name="id"/>.</summary>
    internal static bool FindByVisualId(IEnumerable<DrawList> candidates, Func<RoomObjectDraw, string> visualId, string? id, out DrawList found) =>
        TryFindByVisualId(candidates, candidate => visualId(candidate.Draw), id, out found);

    /// <summary>Builds the draw list of the first of <paramref name="draws"/> whose visual identity is <paramref name="id"/>.</summary>
    internal static bool FindByVisualId(IEnumerable<RoomObjectDraw> draws, Func<RoomObjectDraw, string> visualId, DrawListLookup lookup, string? id, out DrawList found)
    {
        foreach (RoomObjectDraw draw in draws)
        {
            if (string.Equals(id, visualId(draw), StringComparison.Ordinal))
                return lookup(draw, out found);
        }
        found = default;
        return false;
    }

    /// <summary>The first of <paramref name="candidates"/> whose published visual identity is <paramref name="id"/>.</summary>
    internal static bool TryFindByVisualId<T>(IEnumerable<T> candidates, Func<T, string> visualId, string? id, out T found) where T : struct
    {
        foreach (T candidate in candidates)
        {
            if (string.Equals(visualId(candidate), id, StringComparison.Ordinal))
            {
                found = candidate;
                return true;
            }
        }
        found = default;
        return false;
    }
}
