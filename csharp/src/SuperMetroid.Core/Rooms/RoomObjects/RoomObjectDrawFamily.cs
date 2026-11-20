using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One family of room-object frames and its drawing (#1862, #2034): the block shape each frame draws, each cell colliding
/// as the shape says, and the installed picture (#1557) each frame shows, which the room's drawing holds under the draw
/// (#2571): the family's catalog lists its pictures (<see cref="IRoomObjectFamilyPictures"/>) when World composes the
/// room's drawing. A family's room objects draw their frames through it, and a draw names the family it belongs to
/// (<see cref="RoomObjectFrameDraw{TFrame}"/>).
/// </summary>
/// <typeparam name="TFrame">The family's frame: an enum of its frames, or a record of what varies between them.</typeparam>
internal class RoomObjectDrawFamily<TFrame> where TFrame : struct
{
    /// <summary>The block shape of each frame.</summary>
    private readonly Func<TFrame, RoomBlockShape> _shape;

    /// <summary>A family of frames shaped by <paramref name="shape"/>, each pictured as the room's drawing pictures its draw.</summary>
    /// <param name="name">The family's name in diagnostics.</param>
    /// <param name="shape">The block shape of each frame.</param>
    internal RoomObjectDrawFamily(string name, Func<TFrame, RoomBlockShape> shape)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        _shape = Ensure.NotNull(shape);
    }

    /// <summary>The family's name in diagnostics.</summary>
    internal string Name { get; }

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
        RoomObjectFrameImage image = Ensure.NotNull(drawing).Image(draw);
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
