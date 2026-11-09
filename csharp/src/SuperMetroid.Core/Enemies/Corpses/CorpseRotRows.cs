namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// A rotting corpse's pixel rows as its own rot has left them (#1539): for each of the corpse's rows, the corpse row it
/// shows, or nothing once the rot has cleared it. Immutable: the corpse's frames are drawn whole through it.
/// </summary>
internal sealed class CorpseRotRows
{
    /// <summary>The corpse row each row shows, or none.</summary>
    private readonly int?[] rows;

    /// <summary>Keeps a copy of the rows.</summary>
    /// <exception cref="ArgumentException">There are no rows, or one shows a row the corpse does not have.</exception>
    private CorpseRotRows(int?[] rows)
    {
        if (rows.Length == 0)
            throw new ArgumentException("A rotting corpse has at least one pixel row.", nameof(rows));
        foreach (int? row in rows)
        {
            if (row is int shown && (uint)shown >= (uint)rows.Length)
                throw new ArgumentException($"A rotting corpse's row shows row {shown} of {rows.Length}.", nameof(rows));
        }
        this.rows = [.. rows];
    }

    /// <summary>The corpse's pixel rows.</summary>
    internal int Count => rows.Length;

    /// <summary>The corpse row that row <paramref name="row"/> shows, or none where the rot has cleared it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such row.</exception>
    internal int? Shown(int row) => (uint)row < (uint)rows.Length ? rows[row]
        : throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse has {rows.Length} rows.");

    /// <summary>The rows <paramref name="shown"/> says each row shows.</summary>
    internal static CorpseRotRows Of(int?[] shown) => new(Ensure.NotNull(shown));

    /// <summary>
    /// A corpse <paramref name="rowCount"/> rows tall each of whose rows shows itself where <paramref name="shows"/> says it
    /// does and nothing elsewhere.
    /// </summary>
    internal static CorpseRotRows Showing(int rowCount, Func<int, bool> shows)
    {
        Ensure.NotNull(shows);
        return new([.. Enumerable.Range(0, rowCount).Select(row => shows(row) ? row : (int?)null)]);
    }
}
