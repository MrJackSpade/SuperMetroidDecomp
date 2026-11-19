using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One quarter of a square slope block, in the slope shape's own unmirrored frame: its column half and row half.</summary>
/// <param name="Column">The quarter's column half.</param>
/// <param name="Row">The quarter's row half.</param>
public readonly record struct SquareSlopeQuadrant(SquareSlopeColumnHalf Column, SquareSlopeRowHalf Row)
{
    /// <summary>Whether the quarter is in the shape's right column half; otherwise the left.</summary>
    public bool Right => Column == SquareSlopeColumnHalf.Right;

    /// <summary>Whether the quarter is in the shape's bottom row half; otherwise the top.</summary>
    public bool Bottom => Row == SquareSlopeRowHalf.Bottom;

    /// <summary>The quarter beside this one in the same row.</summary>
    public SquareSlopeQuadrant OtherColumn => this with { Column = Opposite(Column) };

    /// <summary>The quarter above or below this one in the same column.</summary>
    public SquareSlopeQuadrant OtherRow => this with { Row = Opposite(Row) };

    /// <summary>The column half a point lies in: the right one when it lies in the block's second half.</summary>
    public static SquareSlopeColumnHalf ColumnFor(bool inSecondHalf) => inSecondHalf ? SquareSlopeColumnHalf.Right : SquareSlopeColumnHalf.Left;

    /// <summary>The row half a point lies in: the bottom one when it lies in the block's second half.</summary>
    public static SquareSlopeRowHalf RowFor(bool inSecondHalf) => inSecondHalf ? SquareSlopeRowHalf.Bottom : SquareSlopeRowHalf.Top;

    /// <summary>
    /// The quarter of <paramref name="behavior"/>'s slope shape that a block's halves show: the block's horizontal and
    /// vertical reflections swap the halves they mirror.
    /// </summary>
    /// <param name="behavior">The square slope block's behaviour, which holds its reflections.</param>
    /// <param name="column">The block column half the point lies in.</param>
    /// <param name="row">The block row half the point lies in.</param>
    public static SquareSlopeQuadrant Showing(RoomBlockBehavior behavior, SquareSlopeColumnHalf column, SquareSlopeRowHalf row) =>
        new(behavior.Slope.FlipsHorizontally ? Opposite(column) : column, behavior.Slope.FlipsVertically ? Opposite(row) : row);

    /// <summary>The other column half.</summary>
    private static SquareSlopeColumnHalf Opposite(SquareSlopeColumnHalf column) => column switch
    {
        SquareSlopeColumnHalf.Left => SquareSlopeColumnHalf.Right,
        SquareSlopeColumnHalf.Right => SquareSlopeColumnHalf.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Undefined column half."),
    };

    /// <summary>The other row half.</summary>
    private static SquareSlopeRowHalf Opposite(SquareSlopeRowHalf row) => row switch
    {
        SquareSlopeRowHalf.Top => SquareSlopeRowHalf.Bottom,
        SquareSlopeRowHalf.Bottom => SquareSlopeRowHalf.Top,
        _ => throw new ArgumentOutOfRangeException(nameof(row), row, "Undefined row half."),
    };
}
