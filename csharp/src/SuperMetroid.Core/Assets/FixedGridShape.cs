using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The fixed dimensions of an editable row-major document grid, such as animation frames of
/// palette colors: an exact number of rows, each holding an exact number of entries.
/// </summary>
/// <param name="Rows">Required number of rows.</param>
/// <param name="Columns">Required number of entries in every row.</param>
internal readonly record struct FixedGridShape(int Rows, int Columns)
{
    /// <summary>Whether <paramref name="rows"/> is present with exactly <see cref="Rows"/> rows.</summary>
    internal bool HasRows<T>([NotNullWhen(true)] T[]? rows) => rows is not null && rows.Length == Rows;

    /// <summary>Whether <paramref name="row"/> is present with exactly <see cref="Columns"/> entries.</summary>
    internal bool HasColumns<T>([NotNullWhen(true)] T[]? row) => row is not null && row.Length == Columns;

    /// <summary>Whether <paramref name="grid"/> has exactly <see cref="Rows"/> rows of exactly <see cref="Columns"/> entries.</summary>
    internal bool Fits<T>([NotNullWhen(true)] T[]?[]? grid)
    {
        if (!HasRows(grid))
            return false;
        foreach (T[]? row in grid)
        {
            if (!HasColumns(row))
                return false;
        }
        return true;
    }
}
