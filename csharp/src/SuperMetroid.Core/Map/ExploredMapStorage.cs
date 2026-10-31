using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// Which explored map cells a save keeps (#1322, #1421, #1475). A save keeps the six Zebes areas' exploration, not
/// Ceres's, and keeps it in runs of eight neighbouring cells of one map row, starting at every eighth column: a run is kept
/// whole when the area's stock map has a cell that can be explored in it, and lost whole otherwise. Exploring a cell
/// whose whole run lies in blank map therefore does not survive saving the game.
/// </summary>
/// <remarks>
/// Evidence: <c>SaveMap</c> and <c>LoadMap</c> in bank $81 copy each area's exploration byte by byte through a list of the
/// bytes the area's map occupies, a byte being eight cells of a row; the save-map check compares the occupied runs with
/// those lists. The stock topology decides it even when installed artwork overrides the picture.
/// </remarks>
public static class ExploredMapStorage
{
    /// <summary>The neighbouring cells of a map row a save keeps or loses together.</summary>
    public const int RunCellCount = 8;

    /// <summary>
    /// The exploration a save of <paramref name="progress"/> keeps: the Zebes areas' explored cells in runs the stock map
    /// occupies, and nothing of Ceres. Every other part of the progress is kept as it is.
    /// </summary>
    /// <param name="progress">The progress being saved; it is not changed.</param>
    /// <param name="maps">Installed stock map rules; the runs they occupy decide what is kept.</param>
    /// <returns>A detached copy of the progress as the save keeps it.</returns>
    /// <exception cref="InvalidOperationException">No map rules are bound.</exception>
    public static PersistentProgress Retained(PersistentProgress progress, IAreaMapSource? maps)
    {
        Ensure.NotNull(progress);
        IAreaMapSource stock = maps ?? throw new InvalidOperationException("Saving explored maps requires bound immutable stock map rules.");
        PersistentProgress kept = progress.Copy();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            IAreaMapView? view = AreaIds.IsZebes(area) ? stock.Get(area) : null;
            foreach (Position16<MapTiles> cell in progress.ExploredCells(area))
                kept.SetExplored(area, cell, view is not null && IsKept(view, cell));
        }
        return kept;
    }

    /// <summary>Whether a save keeps <paramref name="cell"/>'s exploration: its run has a cell the stock map can explore.</summary>
    /// <param name="stockRules">The area's stock map rules.</param>
    /// <param name="cell">The map cell.</param>
    public static bool IsKept(IAreaMapView stockRules, Position16<MapTiles> cell)
    {
        Ensure.NotNull(stockRules);
        int runStart = cell.X / RunCellCount * RunCellCount;
        for (int x = runStart; x < runStart + RunCellCount; x++)
        {
            if (stockRules.IsDiscoverable(new Position16<MapTiles>(x, cell.Y)))
                return true;
        }
        return false;
    }
}
