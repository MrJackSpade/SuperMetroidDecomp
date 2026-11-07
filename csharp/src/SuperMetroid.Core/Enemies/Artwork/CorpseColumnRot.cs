using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// How one character column of a rotting corpse's picture shows the corpse's rot (#1531): the band of the corpse's rows
/// the column's flesh begins at, which no move of a row above it reaches, and the bands of the corpse's rows the column
/// shows rotting, eight pixel rows to a band. Every other row of the column shows itself, as where a later look of its
/// encounter shows other artwork over the corpse's.
/// </summary>
public sealed class CorpseColumnRot
{
    /// <summary>The column whose flesh begins at <paramref name="firstBand"/> and shows <paramref name="bands"/> rotting.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The first band is negative, or the bands are empty, out of order, repeated or above the flesh.
    /// </exception>
    public CorpseColumnRot(int firstBand, IEnumerable<int> bands)
    {
        FirstBand = Ensure.AtLeastZero(firstBand);
        Bands = [.. Ensure.NotNull(bands)];
        if (Bands.IsEmpty)
            throw new ArgumentOutOfRangeException(nameof(bands), "A rotting column shows at least one band rotting.");
        for (int index = 0; index < Bands.Length; index++)
        {
            if (Bands[index] < firstBand || index > 0 && Bands[index] <= Bands[index - 1])
                throw new ArgumentOutOfRangeException(nameof(bands), Bands[index], $"A rotting column's bands rise from its flesh's band {firstBand}, each once.");
        }
    }

    /// <summary>The band the column's flesh begins at.</summary>
    public int FirstBand { get; }

    /// <summary>The bands the column shows rotting, in order.</summary>
    public ImmutableArray<int> Bands { get; }

    /// <summary>Whether the column shows band <paramref name="band"/> rotting.</summary>
    public bool Rots(int band) => Bands.BinarySearch(band) >= 0;
}
