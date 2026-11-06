namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A save keeps a map's exploration in runs of eight neighbouring cells of one map row, starting at every eighth column
/// (#1322, #1421, #1475): a run in which the area's stock map has a cell that can be explored is kept whole, with every
/// explored cell in it, and any other run is lost whole. Exploring a cell whose whole run lies in blank stock map
/// therefore does not survive saving and loading the game.
/// <para>
/// What the player sees: after a save is loaded, the pause map shows exploration only in runs the stock map can be
/// explored in, even where the player's edited installed map picture shows cells elsewhere; and a camera scroll check
/// that reads past the room's scroll cells (<see cref="StoredExploredMaps{TArea}"/>) reads the loaded maps, so a run lost
/// on saving reads as unexplored to it.
/// </para>
/// </summary>
/// <remarks>
/// Consulted once per explored cell when a slot is saved, and by the save image's packing of the explored maps. The
/// stock map's topology decides it, never the installed picture. Evidence: the original game's save and load copy each
/// area's exploration byte by byte through a list of the bytes the area's stock map occupies, each byte holding eight
/// cells of a row; the explored-map packing check compares the runs this service keeps with those lists.
/// </remarks>
[CompatibilityService("a save keeps a map's exploration in whole runs of eight cells of a row, and loses every run the stock map cannot be explored in")]
public static class SavedMapExplorationRuns
{
    /// <summary>The neighbouring cells of a map row a save keeps or loses together.</summary>
    public const int RunCellCount = 8;

    /// <summary>Whether a save keeps <paramref name="cell"/>'s exploration: its run has a cell the stock map can explore.</summary>
    /// <param name="isExplorable">Whether the area's stock map can explore a cell.</param>
    /// <param name="cell">The map cell.</param>
    public static bool IsKept(Func<Position16<MapTiles>, bool> isExplorable, Position16<MapTiles> cell)
    {
        Ensure.NotNull(isExplorable);
        int runStart = cell.X / RunCellCount * RunCellCount;
        for (int x = runStart; x < runStart + RunCellCount; x++)
        {
            if (isExplorable(new Position16<MapTiles>(x, cell.Y)))
                return true;
        }
        return false;
    }
}
