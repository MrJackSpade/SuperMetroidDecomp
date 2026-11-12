namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's melt dissolve (#1652): which pixel column each step erases, and how much of the image it erases.</summary>
internal static class CrocomireMeltDissolveDefinitions
{
    /// <summary>
    /// The column each step of the dissolve erases, by step: its group of eight pixel columns is the column divided by eight.
    /// The pixel within the group the step erases is the step's place modulo eight, not the column's, so a step usually
    /// clears a different pixel than its column names; this silhouette is what the melt shows.
    /// </summary>
    /// <remarks>
    /// Retained under #1165's arbitrary-sequence exception: these are the fixed shuffled visitation identities of all columns
    /// 0..48, not samples of motion or geometry. Replacing this order with a new shuffle changes the dissolve.
    /// </remarks>
    internal static ReadOnlySpan<byte> ColumnOrder =>
    [
        43, 40, 33, 31, 44, 16, 22, 23,
        15, 0, 6, 7, 11, 8, 1, 42,
        12, 36, 46, 45, 26, 20, 29, 35,
        30, 41, 37, 34, 19, 25, 21, 18,
        48, 3, 9, 2, 27, 5, 24, 28,
        17, 10, 4, 13, 47, 14, 32, 38,
        39,
    ];

    /// <summary>Pixel columns in one group the dissolve erases within.</summary>
    internal const int GroupColumns = 8;

    /// <summary>The bands the dissolve erases: the six rows of eight pixels the erase runs down; a second pass's seventh band never dissolves.</summary>
    internal const int ErasedBands = 6;
}
