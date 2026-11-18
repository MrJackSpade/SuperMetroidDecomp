using System.Collections;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// One menu page's rows from the top (#2225, #2359): the options pages' (<see cref="Options.GameOptionsPageRows"/>) and file
/// select's (<see cref="FileSelect.FileSelectItemRows"/>). The cursor moves up and down through them, on the options pages and
/// the file-select main page wrapping from either end to the other.
/// </summary>
/// <typeparam name="TRow">The page's rows.</typeparam>
internal sealed class MenuRowOrder<TRow> : IReadOnlyList<TRow>
{
    /// <summary>The rows from the top.</summary>
    private readonly ImmutableArray<TRow> rows;

    /// <summary>Orders a page's <paramref name="rows"/> from the top.</summary>
    /// <param name="rows">The rows from the top, each once; a page has at least one.</param>
    /// <exception cref="ArgumentException">The page has no row, or a row stands twice.</exception>
    internal MenuRowOrder(ImmutableArray<TRow> rows)
    {
        if (rows.IsDefaultOrEmpty)
            throw new ArgumentException("A menu page has at least one row.", nameof(rows));
        if (rows.Distinct().Count() != rows.Length)
            throw new ArgumentException("A menu page shows each row once.", nameof(rows));
        this.rows = rows;
    }

    /// <summary>The top row, which the cursor starts on whenever the page loads.</summary>
    internal TRow Top => rows[0];

    /// <summary>How many rows the page shows.</summary>
    public int Count => rows.Length;

    /// <summary>The row <paramref name="place"/> rows from the top.</summary>
    /// <exception cref="IndexOutOfRangeException"><paramref name="place"/> is not a place on the page.</exception>
    public TRow this[int place] => rows[place];

    /// <summary>How many rows stand above <paramref name="row"/>.</summary>
    /// <exception cref="InvalidOperationException">The row is not on the page.</exception>
    internal int PlaceOf(TRow row)
    {
        int place = rows.IndexOf(row);
        return place >= 0 ? place : throw new InvalidOperationException($"Menu row {row} is not on its page.");
    }

    /// <summary>
    /// The row above <paramref name="row"/>, or from the top row the bottom one, which <paramref name="wrapped"/> reports.
    /// </summary>
    /// <exception cref="InvalidOperationException">The row is not on the page.</exception>
    internal TRow Above(TRow row, out bool wrapped)
    {
        int place = PlaceOf(row);
        wrapped = place == 0;
        return rows[wrapped ? rows.Length - 1 : place - 1];
    }

    /// <summary>
    /// The row below <paramref name="row"/>, or from the bottom row the top one, which <paramref name="wrapped"/> reports.
    /// </summary>
    /// <exception cref="InvalidOperationException">The row is not on the page.</exception>
    internal TRow Below(TRow row, out bool wrapped)
    {
        int place = PlaceOf(row);
        wrapped = place == rows.Length - 1;
        return rows[wrapped ? 0 : place + 1];
    }

    /// <inheritdoc/>
    public IEnumerator<TRow> GetEnumerator() => ((IEnumerable<TRow>)rows).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
