using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The geometry of one room-object frame as the import checks it against the cartridge and pictures it (#1557): its
/// runs in write order, built from the shapes and collision kinds the game draws by.
/// </summary>
/// <param name="Draw">The frame's draw.</param>
/// <param name="Runs">The frame's runs in write order; the first starts at the origin.</param>
internal readonly record struct RoomObjectFrameLayout(RoomObjectDraw Draw, ReadOnlyMemory<RoomObjectFrameRun> Runs)
{
    /// <summary>The layout of a frame whose runs are described one by one, each placed where it starts.</summary>
    /// <param name="draw">The frame's draw.</param>
    /// <param name="runCount">The number of runs the frame writes.</param>
    /// <param name="count">The number of cells in a run.</param>
    /// <param name="collisionAt">How a cell of a run collides.</param>
    /// <param name="direction">The direction a run writes its cells in.</param>
    /// <param name="start">Where a run starts, in blocks from the origin.</param>
    /// <exception cref="InvalidDataException">The frame has no run, or its first run does not start at its origin.</exception>
    internal static RoomObjectFrameLayout Of(RoomObjectDraw draw, int runCount, Func<int, int> count,
        Func<int, int, RoomCollisionType> collisionAt, Func<int, RoomObjectRunDirection> direction, Func<int, Offset16<RoomBlocks>> start)
    {
        if (runCount < 1 || start(0) != Offset16<RoomBlocks>.Zero)
            throw new InvalidDataException($"Frame {draw} does not start its first run at its origin.");
        var runs = new RoomObjectFrameRun[runCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var collisions = new RoomCollisionType[count(run)];
            for (int cell = 0; cell < collisions.Length; cell++) collisions[cell] = collisionAt(run, cell);
            runs[run] = new(direction(run), collisions, start(run));
        }
        return new(draw, runs);
    }

    /// <summary>Where the run after <paramref name="run"/> starts, or the origin after the last run, which ends the list.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the frame's runs.</exception>
    internal Offset16<RoomBlocks> NextStart(int run)
    {
        ReadOnlySpan<RoomObjectFrameRun> runs = Runs.Span;
        if ((uint)run >= (uint)runs.Length) throw new ArgumentOutOfRangeException(nameof(run));
        return run + 1 < runs.Length ? runs[run + 1].Start : Offset16<RoomBlocks>.Zero;
    }

    /// <summary>Every cell the frame covers, by its place from the origin; a cell written twice is listed once.</summary>
    internal HashSet<Offset16<RoomBlocks>> Cells()
    {
        var cells = new HashSet<Offset16<RoomBlocks>>();
        foreach (RoomObjectFrameRun run in Runs.Span)
            for (int cell = 0; cell < run.Count; cell++) cells.Add(run.CellAt(cell));
        return cells;
    }
}
