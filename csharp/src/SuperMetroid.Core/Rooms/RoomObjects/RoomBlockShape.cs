using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block shape a room object's frame changes the room by: a set of cells around the object's origin block, each with
/// how it collides once drawn (#1608). What each cell shows is the frame's installed picture at that cell (#1557), so a
/// shape carries no appearance.
/// </summary>
/// <remarks>
/// A shape is a set: no two cells share a place, so drawing it writes each block once and the order the cells are
/// written in is unobservable. The cells keep the order they were listed in only so a shape reads as it was authored;
/// nothing depends on it. The room-object draw path (<see cref="IRoomObjectDrawing.Draw"/>) draws a shape cell by cell at
/// a place's block; a cell that reaches past a row's last block runs into the next row, as every room-object draw does.
/// </remarks>
internal sealed class RoomBlockShape
{
    /// <summary>The shape's cells, by place from the origin.</summary>
    private readonly Dictionary<Offset16<RoomBlocks>, RoomCollisionType> _collisions = [];

    /// <summary>The shape covering <paramref name="cells"/>.</summary>
    /// <param name="cells">The shape's cells; at least one, each at its own place.</param>
    /// <exception cref="ArgumentException">No cell is given, or two cells share a place.</exception>
    internal RoomBlockShape(params ReadOnlySpan<RoomBlockShapeCell> cells)
    {
        if (cells.IsEmpty)
            throw new ArgumentException("A block shape covers at least one cell.", nameof(cells));
        foreach (RoomBlockShapeCell cell in cells)
        {
            if (!_collisions.TryAdd(cell.Cell, cell.Collision))
                throw new ArgumentException($"A block shape covers cell {cell.Cell} twice.", nameof(cells));
        }
        Cells = cells.ToArray();
    }

    /// <summary>The shape's cells, each at its own place, in the order they were listed.</summary>
    internal IReadOnlyList<RoomBlockShapeCell> Cells { get; }

    /// <summary>Whether the shape covers <paramref name="cell"/>.</summary>
    internal bool Covers(Offset16<RoomBlocks> cell) => _collisions.ContainsKey(cell);

    /// <summary>How the shape's cell at <paramref name="cell"/> collides.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shape does not cover <paramref name="cell"/>.</exception>
    internal RoomCollisionType CollisionAt(Offset16<RoomBlocks> cell) =>
        _collisions.TryGetValue(cell, out RoomCollisionType collision)
            ? collision
            : throw new ArgumentOutOfRangeException(nameof(cell), cell, "The block shape does not cover this cell.");
}
