namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a rotting corpse's picture shows as its rot has left it (#1531): for each of the picture's eight-pixel character
/// columns, the picture row each of its pixel rows shows, or none where the rot has cleared it. The corpse's sheet holds
/// each of its frames once, and the frame is drawn through these rows, so the rot moves the picture's rows as it is drawn.
/// Columns that show alike share one table of rows.
/// </summary>
public sealed class CorpseRotPicture
{
    /// <summary>The pixels of one character column.</summary>
    public const int ColumnPixels = 8;

    /// <summary>Each distinct table: the picture row each pixel row shows, or none.</summary>
    private readonly int?[][] tables;

    /// <summary>The table each character column shows its rows by.</summary>
    private readonly int[] columnTables;

    /// <summary>Keeps each character column's rows, sharing one table between columns that show alike.</summary>
    /// <exception cref="ArgumentException">There are no columns, or they are not all as tall, or one shows a row outside the picture.</exception>
    internal CorpseRotPicture(IReadOnlyList<int?[]> columns)
    {
        Ensure.NotNull(columns);
        if (columns.Count == 0)
            throw new ArgumentException("A rotting corpse's picture has at least one character column.", nameof(columns));
        int rows = columns[0].Length;
        var distinct = new List<int?[]>();
        columnTables = new int[columns.Count];
        for (int column = 0; column < columns.Count; column++)
        {
            int?[] shown = columns[column];
            if (shown.Length != rows)
                throw new ArgumentException($"Character column {column} of a rotting corpse's picture is {shown.Length} rows tall, not {rows}.", nameof(columns));
            foreach (int? row in shown)
            {
                if (row is int source && (uint)source >= (uint)rows)
                    throw new ArgumentException($"Character column {column} of a rotting corpse's picture shows row {source} of {rows}.", nameof(columns));
            }
            int table = distinct.FindIndex(earlier => earlier.SequenceEqual(shown));
            if (table < 0)
            {
                table = distinct.Count;
                distinct.Add([.. shown]);
            }
            columnTables[column] = table;
        }
        tables = [.. distinct];
    }

    /// <summary>The picture's character columns.</summary>
    public int Columns => columnTables.Length;

    /// <summary>The picture's pixel rows.</summary>
    public int Rows => tables[0].Length;

    /// <summary>The distinct tables of rows the columns show by.</summary>
    public int Tables => tables.Length;

    /// <summary>The table character column <paramref name="column"/> shows its rows by.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no such column.</exception>
    public int TableOf(int column) => (uint)column < (uint)columnTables.Length ? columnTables[column]
        : throw new ArgumentOutOfRangeException(nameof(column), column, $"The rotting corpse's picture has {columnTables.Length} character columns.");

    /// <summary>The picture row that row <paramref name="row"/> shows by table <paramref name="table"/>, or none where the rot has cleared it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such table or row.</exception>
    public int? Shown(int table, int row)
    {
        int?[] shown = (uint)table < (uint)tables.Length ? tables[table]
            : throw new ArgumentOutOfRangeException(nameof(table), table, $"The rotting corpse's picture has {tables.Length} tables.");
        return (uint)row < (uint)shown.Length ? shown[row]
            : throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse's picture has {shown.Length} rows.");
    }
}
