using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One run of a frame's draw list as the cartridge writes it (#1608): where the run starts, the direction it writes its
/// cells in and how many it writes. The game draws the frame's block shape and never these runs; the run form is the
/// import's private knowledge of how the cartridge stores the frame.
/// </summary>
/// <param name="Start">Where the run starts, in blocks from the frame's origin.</param>
/// <param name="Direction">The direction the run writes its cells in.</param>
/// <param name="Count">How many cells the run writes.</param>
internal readonly record struct RoomObjectShapeRun(Offset16<RoomBlocks> Start, RoomObjectRunDirection Direction, int Count)
{
    /// <summary>A run writing <paramref name="count"/> cells along a row from <paramref name="start"/>.</summary>
    internal static RoomObjectShapeRun Row(Offset16<RoomBlocks> start, int count) => new(start, RoomObjectRunDirection.Horizontal, count);

    /// <summary>A run writing <paramref name="count"/> cells down a column from <paramref name="start"/>.</summary>
    internal static RoomObjectShapeRun Column(Offset16<RoomBlocks> start, int count) => new(start, RoomObjectRunDirection.Vertical, count);

    /// <summary>Where the run's cell <paramref name="cell"/> lies, in blocks from the frame's origin.</summary>
    internal Offset16<RoomBlocks> CellAt(int cell) =>
        Start + (Direction == RoomObjectRunDirection.Vertical ? new Offset16<RoomBlocks>(0, cell) : new Offset16<RoomBlocks>(cell, 0));

    /// <summary>
    /// The layout the import checks and pictures a frame by: <paramref name="runs"/> in the cartridge's write order, each
    /// cell colliding as the game's <paramref name="shape"/> says.
    /// </summary>
    /// <param name="draw">The frame's draw.</param>
    /// <param name="shape">The block shape the game draws the frame as.</param>
    /// <param name="runs">The frame's runs as the cartridge writes them; the first starts at the origin.</param>
    /// <exception cref="InvalidOperationException">
    /// The runs write a cell outside the shape, write a cell twice, or leave one of the shape's cells unwritten: the game's
    /// shape is not exactly what the cartridge writes, or the write order would be observable.
    /// </exception>
    internal static RoomObjectFrameLayout Layout(RoomObjectDraw draw, RoomBlockShape shape, params RoomObjectShapeRun[] runs)
    {
        var written = new HashSet<Offset16<RoomBlocks>>();
        foreach (RoomObjectShapeRun run in runs)
        {
            for (int cell = 0; cell < run.Count; cell++)
            {
                Offset16<RoomBlocks> place = run.CellAt(cell);
                if (!shape.Covers(place))
                    throw new InvalidOperationException($"Frame {draw} writes cell {place}, which its block shape does not cover.");
                if (!written.Add(place))
                    throw new InvalidOperationException($"Frame {draw} writes cell {place} twice.");
            }
        }
        if (written.Count != shape.Cells.Count)
            throw new InvalidOperationException($"Frame {draw} leaves {shape.Cells.Count - written.Count} cells of its block shape unwritten.");
        return RoomObjectFrameLayout.Of(draw, runs.Length, run => runs[run].Count,
            (run, cell) => shape.CollisionAt(runs[run].CellAt(cell)), run => runs[run].Direction, run => runs[run].Start);
    }
}
