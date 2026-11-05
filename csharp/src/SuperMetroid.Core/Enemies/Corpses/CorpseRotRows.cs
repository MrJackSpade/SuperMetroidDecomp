namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// A rotting corpse's pixel rows as its rot has left them (#1531), for each band of eight rows a column of the corpse can
/// start rotting at: a column whose flesh begins at that band skips every move of a row above it, so the rot leaves its
/// rows differently from a column whose flesh begins higher. For each such column, each of the corpse's rows shows one of
/// the column's own rows, or nothing once the rot has cleared it. Immutable: the corpse's frames are drawn through it.
/// </summary>
internal sealed class CorpseRotRows
{
    /// <summary>For each band a column can start rotting at, the row each row shows, or none.</summary>
    private readonly int?[][] bands;

    /// <summary>Keeps copies of each band's rows.</summary>
    /// <exception cref="ArgumentException">The bands are missing, or not each as many rows as there are bands' rows.</exception>
    private CorpseRotRows(int?[][] bands)
    {
        if (bands.Length == 0)
            throw new ArgumentException("A rotting corpse has at least one band of rows.", nameof(bands));
        int count = bands.Length * CorpseRowMoveDefinitions.BandRows;
        foreach (int?[] rows in bands)
        {
            if (rows.Length != count)
                throw new ArgumentException($"A rotting corpse of {bands.Length} bands has {count} rows, not {rows.Length}.", nameof(bands));
            foreach (int? row in rows)
            {
                if (row is int shown && (uint)shown >= (uint)count)
                    throw new ArgumentException($"A rotting corpse's row shows row {shown} of {count}.", nameof(bands));
            }
        }
        this.bands = [.. bands.Select(rows => (int?[])[.. rows])];
    }

    /// <summary>The corpse's pixel rows.</summary>
    internal int Count => bands.Length * CorpseRowMoveDefinitions.BandRows;

    /// <summary>
    /// The row that row <paramref name="row"/> of a column whose flesh begins at band <paramref name="firstBand"/> shows,
    /// or none where the rot has cleared it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such band or row.</exception>
    internal int? Shown(int firstBand, int row)
    {
        int?[] rows = (uint)firstBand < (uint)bands.Length ? bands[firstBand]
            : throw new ArgumentOutOfRangeException(nameof(firstBand), firstBand, $"The rotting corpse has {bands.Length} bands.");
        return (uint)row < (uint)rows.Length ? rows[row]
            : throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse has {rows.Length} rows.");
    }

    /// <summary>The rows of every band of a corpse <paramref name="rowCount"/> rows tall, each showing itself.</summary>
    /// <exception cref="ArgumentException">The corpse is not a whole number of bands tall.</exception>
    internal static CorpseRotRows Unrotted(int rowCount) => new([.. Enumerable.Range(0, Bands(rowCount))
        .Select(_ => Enumerable.Range(0, rowCount).Select(row => (int?)row).ToArray())]);

    /// <summary>The rows each band shows in <paramref name="shown"/>, by band then row.</summary>
    internal static CorpseRotRows Of(int?[][] shown) => new(Ensure.NotNull(shown));

    /// <summary>
    /// A corpse <paramref name="rowCount"/> rows tall each of whose rows shows itself where <paramref name="shows"/> says
    /// it does and nothing elsewhere, in every band.
    /// </summary>
    internal static CorpseRotRows Showing(int rowCount, Func<int, bool> shows)
    {
        Ensure.NotNull(shows);
        int?[] rows = [.. Enumerable.Range(0, rowCount).Select(row => shows(row) ? row : (int?)null)];
        return new([.. Enumerable.Range(0, Bands(rowCount)).Select(_ => rows)]);
    }

    /// <summary>
    /// The rows <paramref name="upper"/> shows above row <paramref name="lowerFirstRow"/> and <paramref name="lower"/>
    /// shows from it down, as a corpse shows rows it uploads in two halves, each as it last uploaded it.
    /// </summary>
    /// <exception cref="ArgumentException">The two are not the same corpse's rows.</exception>
    internal static CorpseRotRows Joined(CorpseRotRows upper, CorpseRotRows lower, int lowerFirstRow)
    {
        Ensure.NotNull(upper);
        Ensure.NotNull(lower);
        if (upper.Count != lower.Count)
            throw new ArgumentException($"Rows of a corpse {upper.Count} rows tall cannot join rows of one {lower.Count} rows tall.", nameof(lower));
        return new([.. Enumerable.Range(0, upper.bands.Length).Select(band => Enumerable.Range(0, upper.Count)
            .Select(row => row < lowerFirstRow ? upper.bands[band][row] : lower.bands[band][row]).ToArray())]);
    }

    /// <summary>The bands of a corpse <paramref name="rowCount"/> rows tall.</summary>
    /// <exception cref="ArgumentException">The corpse is not a whole number of bands tall.</exception>
    internal static int Bands(int rowCount) => rowCount > 0 && rowCount % CorpseRowMoveDefinitions.BandRows == 0
        ? rowCount / CorpseRowMoveDefinitions.BandRows
        : throw new ArgumentException($"A rotting corpse {rowCount} rows tall is not a whole number of bands.", nameof(rowCount));
}
