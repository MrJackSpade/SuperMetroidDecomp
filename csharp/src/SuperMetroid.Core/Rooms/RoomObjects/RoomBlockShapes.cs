using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The forms room objects' block shapes share (#1862): one block at the object's block, a line of blocks along its row or
/// down its column, a filled rectangle, and a parent block with the blocks linked to it.
/// </summary>
internal static class RoomBlockShapes
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
