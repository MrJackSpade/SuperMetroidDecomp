using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The cells of a boss body layer something covers (#1466): from a first row, one mask per row, each bit one of the row's
/// thirty-two cells, the lowest the leftmost. Immutable; an empty footprint covers nothing.
/// </summary>
public sealed class BossBodyFootprint
{
    /// <summary>A footprint covering nothing.</summary>
    public static BossBodyFootprint Empty { get; } = new(0, []);

    /// <summary>A row mask covering every cell of its row.</summary>
    private const uint WholeRow = uint.MaxValue;

    /// <summary>Holds <paramref name="rows"/> from row <paramref name="top"/>, trimmed of uncovered rows at either end.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="top"/> is negative.</exception>
    public BossBodyFootprint(int top, ImmutableArray<uint> rows)
    {
        Ensure.AtLeastZero(top);
        int first = 0, end = rows.IsDefault ? 0 : rows.Length;
        while (first < end && rows[first] == 0)
            first++;
        while (end > first && rows[end - 1] == 0)
            end--;
        Top = first == end ? 0 : top + first;
        Rows = first == end ? [] : rows[first..end];
    }

    /// <summary>The first row covered; zero when nothing is.</summary>
    public int Top { get; }

    /// <summary>The cells covered in each row from <see cref="Top"/>; empty when nothing is.</summary>
    public ImmutableArray<uint> Rows { get; }

    /// <summary>The row after the last covered.</summary>
    public int End => Top + Rows.Length;

    /// <summary>Whether no cell is covered.</summary>
    public bool IsEmpty => Rows.IsEmpty;

    /// <summary>The cells of row <paramref name="row"/> covered; none outside the footprint.</summary>
    public uint Row(int row) => row >= Top && row < End ? Rows[row - Top] : 0;

    /// <summary>
    /// The <paramref name="count"/> cells from <paramref name="firstCell"/> in reading order: by row, then by column.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The first cell or the count is negative.</exception>
    public static BossBodyFootprint OfCells(int firstCell, int count)
    {
        Ensure.AtLeastZero(firstCell);
        Ensure.AtLeastZero(count);
        if (count == 0)
            return Empty;
        int top = firstCell / BossBodyDefinitions.ColumnCount, end = firstCell + count;
        int bottom = (end - 1) / BossBodyDefinitions.ColumnCount;
        var rows = new uint[bottom - top + 1];
        for (int row = top; row <= bottom; row++)
        {
            int from = Math.Max(firstCell, row * BossBodyDefinitions.ColumnCount) - row * BossBodyDefinitions.ColumnCount;
            int to = Math.Min(end, (row + 1) * BossBodyDefinitions.ColumnCount) - row * BossBodyDefinitions.ColumnCount;
            rows[row - top] = Span(from, to);
        }
        return new BossBodyFootprint(top, [.. rows]);
    }

    /// <summary>Every cell of <paramref name="rowCount"/> rows from <paramref name="top"/>.</summary>
    public static BossBodyFootprint OfRows(int top, int rowCount) =>
        new(top, [.. Enumerable.Repeat(WholeRow, Math.Max(rowCount, 0))]);

    /// <summary>The cells covered here and not by <paramref name="other"/>.</summary>
    public BossBodyFootprint Without(BossBodyFootprint other) => Combine(other, (mine, theirs) => mine & ~theirs);

    /// <summary>The cells covered both here and by <paramref name="other"/>.</summary>
    public BossBodyFootprint Within(BossBodyFootprint other) => Combine(other, (mine, theirs) => mine & theirs);

    /// <summary>Each row of this footprint combined with <paramref name="other"/>'s same row.</summary>
    private BossBodyFootprint Combine(BossBodyFootprint other, Func<uint, uint, uint> combine)
    {
        Ensure.NotNull(other);
        if (IsEmpty)
            return this;
        var rows = new uint[Rows.Length];
        for (int row = 0; row < rows.Length; row++)
            rows[row] = combine(Rows[row], other.Row(Top + row));
        return new BossBodyFootprint(Top, [.. rows]);
    }

    /// <summary>The mask of the columns from <paramref name="from"/> up to, not including, <paramref name="to"/>.</summary>
    private static uint Span(int from, int to) =>
        to - from >= BossBodyDefinitions.ColumnCount ? WholeRow : ((1u << (to - from)) - 1) << from;
}
