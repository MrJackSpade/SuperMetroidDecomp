namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The explored maps the game keeps beside the map of the area being played, and what the camera reads from them when a
/// scroll check runs past the room's fifty scroll cells (#1336, #1500). The game marks exploration on one live map. It
/// stores that live map as the map of the area being left when Samus passes a door that exchanges maps, and as the map of
/// the current area when the game is saved; it loads the live map from an area's stored map when such a door enters the
/// area, when a file is selected at file select, and when Samus lands on Zebes after Ceres. Loading a file sets every
/// Zebes area's stored map from the file and clears Ceres's; a new file clears every area's; a cleared file and a soft
/// reset clear every area's and the live map; an attract demo clears the Zebes areas'. Nothing else changes the maps, so
/// the live map can carry exploration from one game or demo into the next, and a map stored after a door that exchanges
/// maps without a matching load (or the reverse) is the live map as it was.
/// <para>
/// A scroll check past the scroll cells reads the stored maps in the order the game keeps them: area by area, each area
/// as runs of eight neighbouring cells of a row, the 32 rows of the left half of the map first and then the right half,
/// four runs to a row. A run with no explored cell reads as a closed screen, a run whose only explored cell is its last
/// (rightmost) one reads as a screen open with the camera aligned to its top, and any other run reads as an open screen
/// (the original's red, blue and green scrolls). The stored map after the seventh
/// area's is never stored and reads as unexplored.
/// </para>
/// </summary>
/// <remarks>
/// Marked by the live map's exploration marks; stored and loaded at the door, save, file and landing points named above;
/// consulted by the camera's scroll-boundary checks whenever one indexes past the scroll cells, during Samus's scrolling.
/// Evidence: the camera's scroll steps (<c>$80:A802</c>, <c>$80:A823</c>, <c>$80:A903</c>) index the scroll cells at
/// <c>$7E:CD20</c> with a row times the room width plus a column, and the eight stored maps (<c>$7E:CD52..D551</c>)
/// follow the fifty cells; the maps are written only by the store and load routines (<c>$80:858C</c>, <c>$80:85C6</c>,
/// the door transition's <c>$82:DF99</c>/<c>$82:DFB6</c> for doors whose elevator properties carry <c>$40</c>), the
/// save (<c>$81:8000</c>), the file load (<c>$81:82E4</c>), the new or cleared file (<c>$81:B3B9</c>) and the demo load
/// (<c>$82:8679</c>); boot and soft reset clear all work memory (<c>$80:8482</c>). The map bits are laid out most
/// significant bit first by <c>$90:A8A6</c>. The 100% movie reads past the cells at update 101214. The stored-map check
/// compares this service's answers with that byte layout for constructed explorations.
/// </remarks>
/// <typeparam name="TArea">An area of the game.</typeparam>
[CompatibilityService("a camera scroll check past the room's scroll cells reads the explored maps the game last stored")]
public sealed class StoredExploredMaps<TArea>
    where TArea : struct, Enum
{
    /// <summary>The camera's reading of a run of eight cells of a stored map.</summary>
    public enum RunReading
    {
        /// <summary>No cell of the run is explored: the camera reads a closed screen.</summary>
        NothingExplored,
        /// <summary>Only the run's last cell is explored: the camera reads a screen open with the camera aligned to its top.</summary>
        OnlyLastCellExplored,
        /// <summary>Any other exploration: the camera reads an open screen.</summary>
        OtherwiseExplored,
    }

    /// <summary>The stored maps the game keeps, of which only the first seven are ever stored.</summary>
    private const int KeptMapCount = 8;

    /// <summary>Neighbouring cells of a row that one run holds.</summary>
    private const int RunCells = 8;

    /// <summary>Runs per row of one half of the map.</summary>
    private const int RunsPerHalfRow = 4;

    /// <summary>Rows of the map.</summary>
    private const int Rows = 32;

    /// <summary>Cells across one half of the map.</summary>
    private const int HalfWidth = RunCells * RunsPerHalfRow;

    /// <summary>Cells across the whole map.</summary>
    private const int Width = HalfWidth * 2;

    /// <summary>Cells of one map.</summary>
    private const int MapCells = Width * Rows;

    /// <summary>Runs one half of the map holds.</summary>
    private const int RunsPerHalf = RunsPerHalfRow * Rows;

    /// <summary>Runs one area's stored map holds.</summary>
    private const int RunsPerMap = RunsPerHalf * 2;

    /// <summary>The areas whose maps are stored, in the order the game keeps them.</summary>
    private readonly IReadOnlyList<TArea> _storedOrder;

    /// <summary>Each area's stored map, in the order the game keeps them: whether each cell, row by row, is explored.</summary>
    private readonly bool[][] _stored;

    /// <summary>The live map: whether each cell, row by row, is explored.</summary>
    private bool[] _live = new bool[MapCells];

    /// <summary>Starts with every map unexplored, as power-on leaves them.</summary>
    /// <param name="storedOrder">The areas whose maps are stored, in the order the game keeps them; at most eight.</param>
    /// <exception cref="ArgumentException">More than eight areas, or an area twice.</exception>
    public StoredExploredMaps(IReadOnlyList<TArea> storedOrder)
    {
        Ensure.NotNull(storedOrder);
        if (storedOrder.Count > KeptMapCount || storedOrder.Distinct().Count() != storedOrder.Count)
            throw new ArgumentException("The game keeps at most eight stored maps, one per area.", nameof(storedOrder));
        _storedOrder = storedOrder;
        _stored = [.. storedOrder.Select(_ => new bool[MapCells])];
    }

    /// <summary>Marks <paramref name="cell"/> explored on the live map.</summary>
    /// <param name="cell">The map cell.</param>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the 64-by-32 map.</exception>
    public void MarkLiveMapCell(Position16<MapTiles> cell) => _live[CellNumber(cell)] = true;

    /// <summary>Stores the live map as <paramref name="area"/>'s map.</summary>
    /// <param name="area">The area the game stores the live map for.</param>
    public void StoreLiveMap(TArea area) => _stored[IndexOf(area)] = [.. _live];

    /// <summary>Loads the live map from <paramref name="area"/>'s stored map.</summary>
    /// <param name="area">The area whose stored map becomes the live map.</param>
    public void LoadLiveMap(TArea area) => _live = [.. _stored[IndexOf(area)]];

    /// <summary>Replaces <paramref name="area"/>'s stored map with <paramref name="explored"/>, as loading a file does.</summary>
    /// <param name="area">The area.</param>
    /// <param name="explored">The explored cells the file holds for the area.</param>
    /// <exception cref="ArgumentOutOfRangeException">A cell lies outside the 64-by-32 map.</exception>
    public void LoadStoredMap(TArea area, IEnumerable<Position16<MapTiles>> explored)
    {
        var map = new bool[MapCells];
        foreach (Position16<MapTiles> cell in Ensure.NotNull(explored))
            map[CellNumber(cell)] = true;
        _stored[IndexOf(area)] = map;
    }

    /// <summary>Clears <paramref name="area"/>'s stored map.</summary>
    /// <param name="area">The area.</param>
    public void ClearStoredMap(TArea area) => Array.Clear(_stored[IndexOf(area)]);

    /// <summary>Clears the live map.</summary>
    public void ClearLiveMap() => Array.Clear(_live);

    /// <summary>
    /// What a scroll check reads <paramref name="cellsPast"/> cells past the room's scroll cells: the run of the stored
    /// maps it lands on.
    /// </summary>
    /// <param name="cellsPast">How far past the last scroll cell the check indexes, zero for the first.</param>
    /// <exception cref="ArgumentOutOfRangeException">The check indexes past every stored map.</exception>
    public RunReading ReadPastScrollCells(int cellsPast)
    {
        int map = Ensure.AtLeastZero(cellsPast) / RunsPerMap;
        if (map >= KeptMapCount)
        {
            // Rejected, not read: past the eighth stored map the check reads unrelated work memory, which only a camera
            // above a room nine or more screens wide reaches. Reproducing it needs the whole of the game's work memory,
            // which this simulation does not keep (emulation scope limits, #1500).
            throw new ArgumentOutOfRangeException(nameof(cellsPast), cellsPast,
                "A camera scroll check indexes past every stored explored map, into memory the port does not reproduce.");
        }
        if (map >= _storedOrder.Count)
            return RunReading.NothingExplored;

        int run = cellsPast % RunsPerMap;
        int firstX = run / RunsPerHalf * HalfWidth + run % RunsPerHalfRow * RunCells;
        int y = run % RunsPerHalf / RunsPerHalfRow;
        bool[] stored = _stored[map];
        int explored = 0;
        for (int x = firstX; x < firstX + RunCells; x++)
        {
            if (stored[y * Width + x])
                explored++;
        }
        return explored == 0
            ? RunReading.NothingExplored
            : explored == 1 && stored[y * Width + firstX + RunCells - 1]
                ? RunReading.OnlyLastCellExplored
                : RunReading.OtherwiseExplored;
    }

    /// <summary>The number of <paramref name="cell"/>, row by row.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the 64-by-32 map.</exception>
    private static int CellNumber(Position16<MapTiles> cell) =>
        cell.X < Width && cell.Y < Rows
            ? cell.Y * Width + cell.X
            : throw new ArgumentOutOfRangeException(nameof(cell), cell, "A map cell lies within the 64-by-32 map.");

    /// <summary>The place of <paramref name="area"/>'s stored map.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The game stores no map for the area.</exception>
    private int IndexOf(TArea area)
    {
        for (int index = 0; index < _storedOrder.Count; index++)
        {
            if (EqualityComparer<TArea>.Default.Equals(_storedOrder[index], area))
                return index;
        }
        throw new ArgumentOutOfRangeException(nameof(area), area, "The game stores no explored map for this area.");
    }
}
