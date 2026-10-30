namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// Consecutive floor cells showing characters of the floor's atlas: each cell shows the character
/// <c>Step</c> after the previous one's, so a contiguous atlas run steps by one and a repeated character by
/// zero.
/// </summary>
/// <param name="FirstTile">The character the first cell shows.</param>
/// <param name="Length">The number of cells.</param>
/// <param name="Step">The character advance from one cell to the next.</param>
internal readonly record struct CeresFloorAtlasRun(int FirstTile, int Length, int Step = 1);
