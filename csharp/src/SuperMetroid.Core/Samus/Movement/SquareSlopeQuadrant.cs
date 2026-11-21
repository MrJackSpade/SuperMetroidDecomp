using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One quarter of a square slope block, in the slope shape's own unmirrored frame: its column half and row half.</summary>
public readonly record struct SquareSlopeQuadrant
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="column">The quarter's column half.</param>
    /// <param name="row">The quarter's row half.</param>
    private SquareSlopeQuadrant(SquareSlopeColumnHalf column, SquareSlopeRowHalf row)
    {
        Column = column;
        Row = row;
    }

    /// <summary>The quarter's column half.</summary>
    public SquareSlopeColumnHalf Column { get; init; }

    /// <summary>The quarter's row half.</summary>
    public SquareSlopeRowHalf Row { get; init; }

    /// <summary>Whether the quarter is in the shape's right column half; otherwise the left.</summary>
    public bool Right => Column == SquareSlopeColumnHalf.Right;

    /// <summary>Whether the quarter is in the shape's bottom row half; otherwise the top.</summary>
    public bool Bottom => Row == SquareSlopeRowHalf.Bottom;

    /// <summary>The quarter beside this one in the same row.</summary>
    public SquareSlopeQuadrant OtherColumn => this with { Column = Opposite(Column) };

    /// <summary>The quarter above or below this one in the same column.</summary>
    public SquareSlopeQuadrant OtherRow => this with { Row = Opposite(Row) };

    /// <summary>The column half room pixel <paramref name="x"/> lies in: the right one in the block's second half.</summary>
    public static SquareSlopeColumnHalf ColumnFor(int x) =>
        RoomGridConversions.InSecondHalfOfBlock(x) ? SquareSlopeColumnHalf.Right : SquareSlopeColumnHalf.Left;

    /// <summary>The row half room pixel <paramref name="y"/> lies in: the bottom one in the block's second half.</summary>
    public static SquareSlopeRowHalf RowFor(int y) =>
        RoomGridConversions.InSecondHalfOfBlock(y) ? SquareSlopeRowHalf.Bottom : SquareSlopeRowHalf.Top;

    /// <summary>The column half <paramref name="half"/>, a half-block number, is: the right one for a block's second half.</summary>
    public static SquareSlopeColumnHalf ColumnOfHalf(int half) =>
        RoomGridConversions.IsBlockSecondHalf(half) ? SquareSlopeColumnHalf.Right : SquareSlopeColumnHalf.Left;

    /// <summary>The row half <paramref name="half"/>, a half-block number, is: the bottom one for a block's second half.</summary>
    public static SquareSlopeRowHalf RowOfHalf(int half) =>
        RoomGridConversions.IsBlockSecondHalf(half) ? SquareSlopeRowHalf.Bottom : SquareSlopeRowHalf.Top;

    /// <summary>
    /// The quarter of <paramref name="behavior"/>'s slope shape that a block's halves show: the block's horizontal and
    /// vertical reflections swap the halves they mirror.
    /// </summary>
    /// <param name="behavior">The square slope block's behaviour, which holds its reflections.</param>
    /// <param name="column">The block column half the point lies in.</param>
    /// <param name="row">The block row half the point lies in.</param>
    public static SquareSlopeQuadrant Showing(RoomBlockBehavior behavior, SquareSlopeColumnHalf column, SquareSlopeRowHalf row) =>
        new(behavior.Slope.FlipsHorizontally ? Opposite(column) : column, behavior.Slope.FlipsVertically ? Opposite(row) : row);

    /// <summary>The opposite of each column.</summary>
    private static IdentityTable<SquareSlopeColumnHalf, SquareSlopeColumnHalf> OppositeByColumn => field ??= new(new Dictionary<SquareSlopeColumnHalf, SquareSlopeColumnHalf>
    {
        [SquareSlopeColumnHalf.Left] = SquareSlopeColumnHalf.Right,
        [SquareSlopeColumnHalf.Right] = SquareSlopeColumnHalf.Left,
    });

    /// <summary>The other column half.</summary>
    private static SquareSlopeColumnHalf Opposite(SquareSlopeColumnHalf column) => OppositeByColumn[column];

    /// <summary>The opposite of each row.</summary>
    private static IdentityTable<SquareSlopeRowHalf, SquareSlopeRowHalf> OppositeByRow => field ??= new(new Dictionary<SquareSlopeRowHalf, SquareSlopeRowHalf>
    {
        [SquareSlopeRowHalf.Top] = SquareSlopeRowHalf.Bottom,
        [SquareSlopeRowHalf.Bottom] = SquareSlopeRowHalf.Top,
    });

    /// <summary>The other row half.</summary>
    private static SquareSlopeRowHalf Opposite(SquareSlopeRowHalf row) => OppositeByRow[row];
}
