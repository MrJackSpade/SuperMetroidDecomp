namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Compiled physical column order and bitplane masks for Crocomire's melt.</summary>
internal static class CrocomireMeltingDefinitions
{
    /// <summary>
    /// <c>CrocomireMeltingXOffsetTable</c> at <c>$A4:9697-$A4:96C7</c>.
    /// The cartridge uses this permutation to choose the next physical X column.
    /// </summary>
    /// <remarks>
    /// Retained under #1165's arbitrary-sequence exception: these are the fixed shuffled
    /// visitation identities of all columns 0..48, not samples of motion or geometry.
    /// The erase routine consumes their chronological order and keeps height per selected
    /// column; its separate pixel mask uses cursor modulo eight. The pinned C reference
    /// names this kCrocoVlineRandomPos. Replacing this order with a new shuffle changes the
    /// dissolve; a switch or fitted polynomial would only encode the same arbitrary choices.
    /// Full native bytes, consumers, bounded-generator checks and the distinction from
    /// an impossibility claim are recorded in crocomireColumnOrderRetentionReview in the
    /// #1165 inventory. This exception does not cover the separately computed masks.
    /// </remarks>
    private static ReadOnlySpan<byte> ColumnOrder =>
    [
        43, 40, 33, 31, 44, 16, 22, 23,
        15, 0, 6, 7, 11, 8, 1, 42,
        12, 36, 46, 45, 26, 20, 29, 35,
        30, 41, 37, 34, 19, 25, 21, 18,
        48, 3, 9, 2, 27, 5, 24, 28,
        17, 10, 4, 13, 47, 14, 32, 38,
        39,
    ];

    /// <summary>The number of authored physical columns in the melt permutation.</summary>
    public const int ColumnCount = 49;

    /// <summary>The pixels each dissolve step erases down its column, and the height at which a column is erased whole.</summary>
    public const int ColumnPixels = 48;

    /// <summary>Returns the physical X column selected by one chronological cursor.</summary>
    public static byte SelectColumn(int cursor)
    {
        if ((uint)cursor >= ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(cursor));
        return ColumnOrder[cursor];
    }

    /// <summary>
    /// Returns the native clear mask for a chronological cursor, preserving the shipped
    /// bug that uses <c>cursor &amp; 7</c> instead of the selected column's low bits.
    /// </summary>
    public static byte SelectMask(int cursor)
    {
        if ((uint)cursor >= ColumnCount)
            throw new ArgumentOutOfRangeException(nameof(cursor));
        return (byte)~(128 >> (cursor & 7));
    }

    /// <summary>VRAM word $4000: where the melting copies' destination words begin.</summary>
    public const int DestinationWordBase = 0x4000;
}
