using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One family of room-object frames and its drawing (#1862, #2034): the block shape each frame draws, each cell colliding
/// as the shape says, and the installed picture (#1557) each frame shows. A family's room objects draw their frames
/// through it, and a draw names the family it belongs to (<see cref="RoomObjectFrameDraw{TFrame}"/>).
/// </summary>
/// <typeparam name="TFrame">The family's frame: an enum of its frames, or a record of what varies between them.</typeparam>
internal class RoomObjectDrawFamily<TFrame> where TFrame : struct
{
    /// <summary>The block shape of each frame.</summary>
    private readonly Func<TFrame, RoomBlockShape> _shape;

    /// <summary>The installed picture of each draw, from the room's installed room-object pictures.</summary>
    private readonly Func<IRoomObjectDrawing, RoomObjectFrameDraw<TFrame>, RoomObjectFrameImage> _image;

    /// <summary>A family of frames shaped by <paramref name="shape"/> and pictured by <paramref name="image"/>.</summary>
    /// <param name="name">The family's name in diagnostics.</param>
    /// <param name="shape">The block shape of each frame.</param>
    /// <param name="image">The installed picture of each draw.</param>
    internal RoomObjectDrawFamily(string name, Func<TFrame, RoomBlockShape> shape,
        Func<IRoomObjectDrawing, RoomObjectFrameDraw<TFrame>, RoomObjectFrameImage> image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        _shape = Ensure.NotNull(shape);
        _image = Ensure.NotNull(image);
    }

    /// <summary>The family's name in diagnostics.</summary>
    internal string Name { get; }

    /// <summary>
    /// A family whose pictures are the installed catalog <typeparamref name="TCatalog"/>: <paramref name="image"/> picks
    /// each draw's picture from it.
    /// </summary>
    /// <typeparam name="TCatalog">The family's installed pictures.</typeparam>
    /// <param name="name">The family's name in diagnostics, which also names the catalog when it is not installed.</param>
    /// <param name="shape">The block shape of each frame.</param>
    /// <param name="image">The picture of a draw in the installed catalog.</param>
    internal static RoomObjectDrawFamily<TFrame> Pictured<TCatalog>(string name, Func<TFrame, RoomBlockShape> shape,
        Func<TCatalog, RoomObjectFrameDraw<TFrame>, RoomObjectFrameImage> image) where TCatalog : class
    {
        Ensure.NotNull(image);
        return new(name, shape, (drawing, draw) => image(drawing.Installed<TCatalog>(), draw));
    }

    /// <summary>The shape <paramref name="shapes"/> give <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidOperationException">The frame has no shape.</exception>
    internal static RoomBlockShape Defined(IReadOnlyDictionary<TFrame, RoomBlockShape> shapes, TFrame frame) =>
        Ensure.NotNull(shapes).TryGetValue(frame, out RoomBlockShape? shape)
            ? shape
            : throw new InvalidOperationException($"Undefined {typeof(TFrame).Name} {frame}.");

    /// <summary>The draw of <paramref name="frame"/>.</summary>
    internal RoomObjectFrameDraw<TFrame> Of(TFrame frame) => new(this, frame);

    /// <summary>The block shape <paramref name="frame"/> draws.</summary>
    /// <exception cref="InvalidOperationException">The frame has no shape.</exception>
    internal RoomBlockShape Shape(TFrame frame) => _shape(frame);

    /// <summary>The installed picture <paramref name="draw"/> shows.</summary>
    /// <exception cref="InvalidDataException">The family's pictures lack the frame.</exception>
    internal RoomObjectFrameImage Image(IRoomObjectDrawing drawing, RoomObjectFrameDraw<TFrame> draw) =>
        _image(Ensure.NotNull(drawing), Ensure.NotNull(draw));

    /// <summary>Draws <paramref name="frame"/> with its block origin at <paramref name="origin"/>.</summary>
    internal void Draw(RoomLevelData level, Position16<RoomBlocks> origin, IRoomObjectDrawing drawing, TFrame frame) =>
        Draw(level, origin, drawing, Of(frame));

    /// <summary>
    /// Draws <paramref name="draw"/> with its block origin at <paramref name="origin"/>, cell by cell: each cell of the
    /// frame's shape collides as the shape says and shows what the frame's installed picture shows there. No two cells
    /// share a block, so the order they are written in is unobservable.
    /// </summary>
    internal virtual void Draw(RoomLevelData level, Position16<RoomBlocks> origin, IRoomObjectDrawing drawing, RoomObjectFrameDraw<TFrame> draw)
    {
        Ensure.NotNull(draw);
        RoomObjectFrameImage image = Image(drawing, draw);
        foreach (RoomBlockShapeCell cell in Shape(draw.Frame).Cells)
            DrawTerrainAt(level, draw, origin, cell.Cell, new RoomTerrainBlock(image.At(cell.Cell), cell.Collision));
    }

    /// <summary>
    /// Writes a draw's terrain at the cell <paramref name="cell"/> from <paramref name="origin"/>, reached through the room's
    /// reading order (<see cref="RoomReadingOrder"/>): a block of the grid, or, past its last row, one of the terrain beyond
    /// the room (<see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">The cell lies before the room's first block or past the terrain beyond the room.</exception>
    private protected static void DrawTerrainAt(RoomLevelData level, RoomObjectDraw draw, Position16<RoomBlocks> origin,
        Offset16<RoomBlocks> cell, RoomTerrainBlock terrain)
    {
        try
        {
            RoomObjectLevelWrites.DrawTerrain(level, level.ObjectBlockStep(origin, cell), terrain);
        }
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException(
                $"Room-object draw {draw} targets cell ({cell.X},{cell.Y}) of block ({origin.X},{origin.Y}) outside the room's " +
                "room-object-reachable blocks.", error);
        }
    }
}
