namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a rotting corpse's picture shows as its own rot has left it (#1539): the picture row each of its pixel rows shows,
/// across the whole picture, or none where the rot has cleared it. The corpse's sheet holds each of its frames once, and the
/// frame is drawn through these rows, so the rot moves the picture's rows as it is drawn.
/// </summary>
public sealed class CorpseRotPicture
{
    /// <summary>The picture row each pixel row shows, or none.</summary>
    private readonly int?[] rows;

    /// <summary>Keeps a copy of the rows each pixel row shows.</summary>
    /// <exception cref="ArgumentException">There are no rows, or one shows a row outside the picture.</exception>
    internal CorpseRotPicture(int?[] rows)
    {
        Ensure.NotNull(rows);
        if (rows.Length == 0)
            throw new ArgumentException("A rotting corpse's picture has at least one pixel row.", nameof(rows));
        foreach (int? row in rows)
        {
            if (row is int source && (uint)source >= (uint)rows.Length)
                throw new ArgumentException($"A rotting corpse's picture row shows row {source} of {rows.Length}.", nameof(rows));
        }
        this.rows = [.. rows];
    }

    /// <summary>The picture's pixel rows.</summary>
    public int Rows => rows.Length;

    /// <summary>The picture row that row <paramref name="row"/> shows, or none where the rot has cleared it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no such row.</exception>
    public int? Shown(int row) => (uint)row < (uint)rows.Length ? rows[row]
        : throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse's picture has {rows.Length} rows.");
}
