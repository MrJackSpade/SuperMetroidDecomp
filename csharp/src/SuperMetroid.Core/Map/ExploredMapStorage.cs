using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// Which areas' explored map cells a save keeps (#1322, #1421, #1475): the six Zebes areas', in the runs
/// <see cref="SavedMapExplorationRuns"/> keeps, and nothing of Ceres's.
/// </summary>
/// <remarks>
/// Evidence: saving and loading the map copy only the Zebes areas' exploration; the save-map check compares the kept
/// cells with the save's packed lists.
/// </remarks>
public static class ExploredMapStorage
{
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
        foreach (AreaId area in AreaIds.All)
        {
            IAreaMapView? view = AreaIds.IsZebes(area) ? stock.Get(area) : null;
            foreach (Position16<MapTiles> cell in progress.ExploredCells(area))
                kept.SetExplored(area, cell, view is not null && SavedMapExplorationRuns.IsKept(view.IsDiscoverable, cell));
        }
        return kept;
    }
}
