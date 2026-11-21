using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The geometry of one room-object frame as the import checks it against the cartridge and pictures it (#1557): its
/// runs in write order, built from the shapes and collision kinds the game draws by. What each cell shows is read from
/// the cartridge, never from the game.
/// </summary>
/// <param name="Draw">The frame's draw.</param>
/// <param name="Runs">The frame's runs in write order; the first starts at the origin.</param>
internal readonly partial record struct RoomObjectFrameLayout(RoomObjectDraw Draw, ReadOnlyMemory<RoomObjectDrawLists.Run> Runs)
{
    /// <summary>The layout of a frame written as <paramref name="runs"/>, each cell colliding as the game's <paramref name="shape"/> says.</summary>
    /// <param name="draw">The frame's draw.</param>
    /// <param name="shape">The block shape the game draws the frame as.</param>
    /// <param name="runs">The frame's runs in the cartridge's write order; the first starts at the origin.</param>
    /// <exception cref="InvalidDataException">The frame has no run, or its first run does not start at its origin.</exception>
    internal static RoomObjectFrameLayout Of(RoomObjectDraw draw, RoomBlockShape shape, IReadOnlyList<RoomObjectShapeRun> runs) =>
        Of(RoomObjectDrawListExport.Runs(draw, shape, runs));

    /// <summary>The layout of a frame the game draws as <paramref name="list"/>: the same runs, directions, starts and collision.</summary>
    /// <param name="list">The frame's runs as the game draws them.</param>
    /// <exception cref="InvalidDataException">The frame has no run, or its first run does not start at its origin.</exception>
    internal static RoomObjectFrameLayout Of(RoomObjectDrawLists.DrawList list)
    {
        if (list.Runs.Length < 1 || list.Runs.Span[0].Start != Offset16<RoomBlocks>.Zero)
            throw new InvalidDataException($"Frame {list.Draw} does not start its first run at its origin.");
        return new(list.Draw, list.Runs);
    }

    /// <summary>Where the run after <paramref name="run"/> starts, or the origin after the last run, which ends the list.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the frame's runs.</exception>
    internal Offset16<RoomBlocks> NextStart(int run)
    {
        ReadOnlySpan<RoomObjectDrawLists.Run> runs = Runs.Span;
        if ((uint)run >= (uint)runs.Length) throw new ArgumentOutOfRangeException(nameof(run));
        return run + 1 < runs.Length ? runs[run + 1].Start : Offset16<RoomBlocks>.Zero;
    }

    /// <summary>Every cell the frame covers, by its place from the origin; a cell written twice is listed once.</summary>
    internal HashSet<Offset16<RoomBlocks>> Cells()
    {
        var cells = new HashSet<Offset16<RoomBlocks>>();
        foreach (RoomObjectDrawLists.Run run in Runs.Span)
            for (int cell = 0; cell < run.Count; cell++) cells.Add(run.CellAt(cell));
        return cells;
    }
}
