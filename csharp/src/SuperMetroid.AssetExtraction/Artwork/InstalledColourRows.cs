using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A complete installed colour table (#1615): every row of an authored colour set (a frame, health band, pose or step)
/// with every one of its colours, kept as installed. A table missing a row or a colour is rejected when built.
/// </summary>
internal sealed class InstalledColourRows
{
    /// <summary>The installed rows, each holding <see cref="ColoursPerRow"/> colours.</summary>
    private readonly RgbColor[][] rows;

    /// <summary>Keeps copies of the complete installed rows.</summary>
    /// <param name="rows">The installed rows, in order.</param>
    /// <param name="rowCount">The rows the table must hold.</param>
    /// <param name="coloursPerRow">The colours each row must hold.</param>
    /// <param name="label">What the table is, naming it in a rejection.</param>
    /// <exception cref="InvalidDataException">The table has another number of rows, or a row another number of colours.</exception>
    internal InstalledColourRows(RgbColor[][] rows, int rowCount, int coloursPerRow, string label)
    {
        if (rows.Length != rowCount)
            throw new InvalidDataException($"{label} requires {rowCount} rows of installed colours.");
        this.rows = new RgbColor[rowCount][];
        for (int row = 0; row < rowCount; row++)
        {
            if (rows[row].Length != coloursPerRow)
                throw new InvalidDataException($"{label} row {row} requires {coloursPerRow} installed colours.");
            this.rows[row] = (RgbColor[])rows[row].Clone();
        }
        ColoursPerRow = coloursPerRow;
    }

    /// <summary>Keeps a copy of a single complete installed row.</summary>
    /// <param name="row">The installed colours.</param>
    /// <param name="colours">The colours the row must hold.</param>
    /// <param name="label">What the row is, naming it in a rejection.</param>
    /// <exception cref="InvalidDataException">The row holds another number of colours.</exception>
    internal InstalledColourRows(RgbColor[] row, int colours, string label) : this([row], 1, colours, label)
    {
    }

    /// <summary>The rows the table holds.</summary>
    internal int RowCount => rows.Length;

    /// <summary>The colours each row holds.</summary>
    internal int ColoursPerRow { get; }

    /// <summary>The installed colour <paramref name="colour"/> of row <paramref name="row"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row or colour is outside the table.</exception>
    internal RgbColor Colour(int row, int colour)
    {
        if ((uint)row >= (uint)rows.Length) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)colour >= (uint)ColoursPerRow) throw new ArgumentOutOfRangeException(nameof(colour));
        return rows[row][colour];
    }

    /// <summary>The installed colour <paramref name="colour"/> of a single-row table.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colour is outside the row.</exception>
    internal RgbColor Colour(int colour) => Colour(0, colour);

    /// <summary>A copy of row <paramref name="row"/>'s installed colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the table.</exception>
    internal RgbColor[] Row(int row) => (uint)row < (uint)rows.Length
        ? (RgbColor[])rows[row].Clone()
        : throw new ArgumentOutOfRangeException(nameof(row));
}
