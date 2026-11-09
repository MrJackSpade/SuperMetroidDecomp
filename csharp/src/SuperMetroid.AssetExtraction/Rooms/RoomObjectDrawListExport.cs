using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Builds the run-by-run draw lists the import lays room-object frames out from (#1557), from the shapes the game draws.
/// The game draws from the shapes directly and never builds these lists.
/// </summary>
internal static class RoomObjectDrawListExport
{
    /// <summary>The direction of every run of a draw that writes its runs along their rows.</summary>
    internal static readonly Func<int, RoomObjectRunDirection> Horizontal = static _ => RoomObjectRunDirection.Horizontal;

    /// <summary>Where each of a draw's stacked rows starts: its row number of blocks below the origin.</summary>
    internal static readonly Func<int, Offset16<RoomBlocks>> Stacked = static run => new(0, run);

    /// <summary>
    /// Builds the draw list of a draw whose runs are described one by one, each placed where it starts (#1546); the
    /// cartridge's continuation after each run is derived by <see cref="RoomObjectRunContinuationCodec"/>.
    /// </summary>
    /// <param name="draw">The draw the list exports.</param>
    /// <param name="runCount">The number of runs the draw writes.</param>
    /// <param name="blockCount">The number of blocks in a run.</param>
    /// <param name="blockAt">The terrain at a cell of a run.</param>
    /// <param name="direction">The direction a run writes its blocks in.</param>
    /// <param name="start">Where a run starts, in blocks from the draw's origin.</param>
    /// <exception cref="InvalidDataException">The draw has no run, or its first run does not start at its origin.</exception>
    internal static DrawList Runs(RoomObjectDraw draw, int runCount, Func<int, int> blockCount, Func<int, int, RoomTerrainBlock> blockAt,
        Func<int, RoomObjectRunDirection> direction, Func<int, Offset16<RoomBlocks>> start)
    {
        if (runCount < 1 || start(0) != Offset16<RoomBlocks>.Zero)
            throw new InvalidDataException($"Draw {draw} does not start its first run at its origin.");
        var runs = new Run[runCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var blocks = new RoomTerrainBlock[blockCount(run)];
            for (int cell = 0; cell < blocks.Length; cell++) blocks[cell] = blockAt(run, cell);
            runs[run] = new(direction(run), blocks, start(run));
        }
        return new(draw, runs);
    }

    /// <summary>Builds the draw list of a draw of horizontal strips, each placed where it starts (#1546).</summary>
    /// <param name="draw">The draw the list exports.</param>
    /// <param name="stripCount">The number of strips the draw writes.</param>
    /// <param name="blockCount">The number of blocks in a strip.</param>
    /// <param name="blockAt">The terrain at a cell of a strip.</param>
    /// <param name="start">Where a strip starts, in blocks from the draw's origin.</param>
    /// <exception cref="InvalidDataException">The draw has no strip, or its first strip does not start at its origin.</exception>
    internal static DrawList Strips(RoomObjectDraw draw, int stripCount, Func<int, int> blockCount, Func<int, int, RoomTerrainBlock> blockAt,
        Func<int, Offset16<RoomBlocks>> start) =>
        Runs(draw, stripCount, blockCount, blockAt, Horizontal, start);

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
}
