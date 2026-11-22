using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The forms room objects' block shapes share (#1862): one block at the object's block, a line of blocks along its row or
/// down its column, a filled rectangle, and a parent block with the blocks linked to it.
/// </summary>
internal static partial class RoomBlockShapes
{
    /// <summary>The one-block shape of each collision kind, made once.</summary>
    private static readonly Dictionary<RoomCollisionType, RoomBlockShape> Blocks = Enum.GetValues<RoomCollisionType>()
        .ToDictionary(collision => collision, collision => new RoomBlockShape([new(Offset16<RoomBlocks>.Zero, collision)]));

    /// <summary>The object's own block, colliding as <paramref name="collision"/>.</summary>
    /// <param name="collision">How the block collides.</param>
    internal static RoomBlockShape Block(RoomCollisionType collision) => Blocks[collision];

    /// <summary>
    /// A line of blocks from the object's block, one for each of <paramref name="collisions"/> in order: down its column
    /// along <paramref name="axis"/>: down its column, or rightward along its row.
    /// </summary>
    /// <param name="axis">The axis the line runs along (#2560).</param>
    /// <param name="collisions">How each block of the line collides, from the object's block on.</param>
    internal static RoomBlockShape Line(Axis axis, IReadOnlyList<RoomCollisionType> collisions)
    {
        var cells = new RoomBlockShapeCell[collisions.Count];
        for (int place = 0; place < cells.Length; place++)
        {
            cells[place] = new(axis switch
            {
                Axis.Vertical => new(0, place),
                Axis.Horizontal => new(place, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Undefined axis."),
            }, collisions[place]);
        }
        return new(cells);
    }

    /// <summary>
    /// A rectangle of <paramref name="size"/> blocks whose top-left block is the object's block, every block colliding as
    /// <paramref name="collision"/>.
    /// </summary>
    /// <param name="size">The rectangle's width and height in blocks.</param>
    /// <param name="collision">How every block collides.</param>
    internal static RoomBlockShape Rectangle(Extent16<RoomBlocks> size, RoomCollisionType collision)
    {
        var cells = new RoomBlockShapeCell[checked(size.X * size.Y)];
        for (int row = 0; row < size.Y; row++)
            for (int column = 0; column < size.X; column++)
                cells[row * size.X + column] = new(new(column, row), collision);
        return new(cells);
    }

    /// <summary>
    /// A rectangle of <paramref name="size"/> blocks whose top-left block lies <paramref name="corner"/> from the object's
    /// block, every block colliding as <paramref name="collision"/> (#2557).
    /// </summary>
    /// <param name="corner">Where the rectangle's top-left block lies from the object's block.</param>
    /// <param name="size">The rectangle's width and height in blocks.</param>
    /// <param name="collision">How every block collides.</param>
    internal static RoomBlockShape Rectangle(Offset16<RoomBlocks> corner, Extent16<RoomBlocks> size, RoomCollisionType collision) =>
        Moved(Rectangle(size, collision), corner);

    /// <summary>A row of <paramref name="length"/> blocks rightward from the object's block, each colliding as <paramref name="collision"/> (#2557).</summary>
    /// <param name="length">How many blocks the row covers.</param>
    /// <param name="collision">How every block collides.</param>
    internal static RoomBlockShape Row(int length, RoomCollisionType collision) =>
        Line(Axis.Horizontal, DefinitionRun.Repeated(collision, length));

    /// <summary>A column of <paramref name="length"/> blocks down from the object's block, each colliding as <paramref name="collision"/> (#2557).</summary>
    /// <param name="length">How many blocks the column covers.</param>
    /// <param name="collision">How every block collides.</param>
    internal static RoomBlockShape Column(int length, RoomCollisionType collision) =>
        Line(Axis.Vertical, DefinitionRun.Repeated(collision, length));

    /// <summary><paramref name="shape"/> with every cell moved <paramref name="by"/> blocks (#2557).</summary>
    /// <param name="shape">The shape moved.</param>
    /// <param name="by">How far each cell moves.</param>
    internal static RoomBlockShape Moved(RoomBlockShape shape, Offset16<RoomBlocks> by) =>
        new([.. shape.Cells.Select(cell => cell with { Cell = cell.Cell + by })]);

    /// <summary>The shape covering every cell of each of <paramref name="parts"/> (#2557); no two parts cover the same cell.</summary>
    /// <param name="parts">The shapes joined.</param>
    /// <exception cref="ArgumentException">Two parts cover the same cell.</exception>
    internal static RoomBlockShape Joined(params ReadOnlySpan<RoomBlockShape> parts)
    {
        var cells = new List<RoomBlockShapeCell>();
        foreach (RoomBlockShape part in parts)
            cells.AddRange(part.Cells);
        return new([.. cells]);
    }

    /// <summary>
    /// A parent block at the object's block colliding as <paramref name="parent"/>, with blocks linked to it at
    /// <paramref name="children"/>: a child on the parent's row extends it along the row, and a child on a row below
    /// extends it down the column.
    /// </summary>
    /// <param name="parent">How the parent block collides.</param>
    /// <param name="children">Where the linked blocks lie from the parent, each to its right or below it.</param>
    internal static RoomBlockShape Linked(RoomCollisionType parent, IReadOnlyList<Offset16<RoomBlocks>> children)
    {
        var cells = new RoomBlockShapeCell[children.Count + 1];
        cells[0] = new(Offset16<RoomBlocks>.Zero, parent);
        for (int child = 0; child < children.Count; child++)
            cells[child + 1] = new(children[child],
                children[child].Y == 0 ? RoomCollisionType.HorizontalExtension : RoomCollisionType.VerticalExtension);
        return new(cells);
    }
}
