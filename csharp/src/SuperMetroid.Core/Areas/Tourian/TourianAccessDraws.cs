namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Describes the Tourian access floor's draws (<see cref="TourianAccessObjectDefinitions"/>, <see cref="TourianAccessObjectDrawDefinitions"/>):
/// each draw's rows of four air cells.
/// </summary>
internal static class TourianAccessDraws
{
    /// <summary>The rows one Tourian access draw writes (<see cref="TourianAccessObjectDrawDefinitions.Rows"/>).</summary>
    /// <exception cref="InvalidOperationException"><paramref name="list"/> is not a defined Tourian access draw.</exception>
    internal static int Rows(TourianAccessDraw list) =>
        TourianAccessObjectDrawDefinitions.Rows.TryGetValue(list, out int rows)
            ? rows : throw new InvalidOperationException($"Undefined TourianAccessDraw {list}.");
}
