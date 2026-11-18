namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Crocomire's melt dissolve as the cartridge erases his melt images' characters (#1652, #2478): which pixel column each
/// step erases, and how much of the image it erases. The import turns it into each melt picture's dissolve, which Core
/// shows; Core holds none of it.
/// </summary>
internal static class CrocomireMeltDissolveDefinitions
{
    /// <summary>
    /// The column each step of the dissolve erases, by step: the step erases a pixel of the group of eight pixel columns
    /// the column lies in. The pixel within the group is the step's place among each eight steps, not the column's, so a
    /// step usually clears a different pixel than its column names; this silhouette is what the melt shows.
    /// </summary>
    /// <remarks>
    /// The fixed shuffled order in which the dissolve visits all columns 0 through 48, not samples of motion or geometry.
    /// Replacing this order with a new shuffle changes the dissolve.
    /// </remarks>
    internal static ReadOnlySpan<int> ColumnOrder =>
    [
        43, 40, 33, 31, 44, 16, 22, 23,
        15, 0, 6, 7, 11, 8, 1, 42,
        12, 36, 46, 45, 26, 20, 29, 35,
        30, 41, 37, 34, 19, 25, 21, 18,
        48, 3, 9, 2, 27, 5, 24, 28,
        17, 10, 4, 13, 47, 14, 32, 38,
        39,
    ];

    /// <summary>Pixel columns in one group the dissolve erases within: one character's width.</summary>
    internal const int GroupColumns = 8;

    /// <summary>The bands the dissolve erases: the six rows of eight pixels the erase runs down; a second pass's seventh band never dissolves.</summary>
    internal const int ErasedBands = 6;
}
