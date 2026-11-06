using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The cartridge's species columns in its crawler surface-program tables, which a population entry's second parameter
/// and the species initializers select by byte offset: the import decodes that offset to a surface animation family.
/// </summary>
internal static class CrawlerSurfaceTableOrder
{
    /// <summary>The family each two-byte column of a surface-program table selects, in column order.</summary>
    private static readonly CrawlerAnimationFamily[] Columns =
    [
        CrawlerAnimationFamily.Shared, CrawlerAnimationFamily.Shared, CrawlerAnimationFamily.Shared,
        CrawlerAnimationFamily.Viola, CrawlerAnimationFamily.Sciser, CrawlerAnimationFamily.Zero,
    ];

    /// <summary>The number of species columns in each surface-program table.</summary>
    internal static int ColumnCount => Columns.Length;

    /// <summary>The family the column at <paramref name="byteOffset"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="byteOffset"/> is not one of the six columns.</exception>
    internal static CrawlerAnimationFamily FamilyAt(int byteOffset) =>
        byteOffset % 2 == 0 && byteOffset / 2 is >= 0 and < 6
            ? Columns[byteOffset / 2]
            : throw new InvalidDataException($"Crawler surface-table offset ${byteOffset:X4} selects none of the six species columns.");

    /// <summary>The byte offset of the first column that selects <paramref name="family"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="family"/> has no surface-table column.</exception>
    internal static int ByteOffsetOf(CrawlerAnimationFamily family)
    {
        int column = Array.IndexOf(Columns, family);
        return column >= 0
            ? column * 2
            : throw new ArgumentOutOfRangeException(nameof(family), family, $"{family} has no crawler surface-table column.");
    }
}
