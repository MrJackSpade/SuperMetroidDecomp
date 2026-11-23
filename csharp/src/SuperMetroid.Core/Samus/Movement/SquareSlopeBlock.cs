using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// A square slope block as a collision reads it (#2554): the solid quarters of its shape and the reflections its behaviour
/// gives them. Only a block whose slope shape is square has one (<see cref="Of"/>).
/// </summary>
/// <param name="Shape">The solid quarters of the block's square shape, before its reflections.</param>
/// <param name="Behavior">The block's behaviour, which holds its reflections.</param>
internal readonly partial record struct SquareSlopeBlock(SquareSlopeShape Shape, RoomBlockBehavior Behavior)
{
    /// <summary>The square slope <paramref name="behavior"/> selects, or none for a slope shape read as a height profile.</summary>
    /// <param name="behavior">The slope block's behaviour.</param>
    internal static SquareSlopeBlock? Of(RoomBlockBehavior behavior) =>
        behavior.Slope.Shape.SquareQuarters is { } shape ? new SquareSlopeBlock(shape, behavior) : null;

    /// <summary>The quarter of the shape the block's halves <paramref name="column"/> and <paramref name="row"/> show.</summary>
    internal SquareSlopeQuadrant Showing(SquareSlopeColumnHalf column, SquareSlopeRowHalf row) =>
        SquareSlopeQuadrant.Showing(Behavior, column, row);

    /// <summary>Whether <paramref name="quadrant"/> of the shape is solid.</summary>
    internal bool IsSolid(SquareSlopeQuadrant quadrant) => SquareSlopes.IsSolid(Shape, quadrant);
}
