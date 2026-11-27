using Engine.Content;
using Engine.Core;
using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's scroll cells: one set of fifty for every room, which outlive each room. A room whose layout is an
/// explicit table loads all fifty, its own screens and what the table's load writes after them; a room with none
/// writes only its own, so the cells past them keep what an earlier room, its triggers or its doors left. The camera
/// reads past a room's own screens at its bottom and right edges, so a cell an earlier room left can decide whether
/// the view goes on following Samus there.
/// </summary>
/// <remarks>
/// A cell is found by its row times the room's width plus its column, with no regard to the room's height. A read
/// past the fifty cells is, on the cartridge, a read of the copies of the explored map it keeps after them
/// (<see cref="KeptExploredMaps"/>), which are written only as an area is left by a door that exchanges maps, as
/// the game is saved and as a file is loaded: so the copy of the area being played can be behind what has been
/// explored there, or hold what another game explored.
/// </remarks>
public sealed class KeptScrollCells : IScrollCells
{
    /// <summary>The data file of what each explicit layout's load writes after the room's own screens, by the layout's name.</summary>
    private const string LayoutsFile = "rooms/scroll_layouts.json";

    /// <summary>How many cells the cartridge keeps.</summary>
    private const int Count = 50;

    /// <summary>The areas whose explored maps follow the cells, in the order the cartridge keeps them; the eighth map kept is never written.</summary>
    private static readonly string[] MapOrder = ["crateria", "brinstar", "norfair", "wrecked_ship", "maridia", "tourian", "ceres"];

    /// <summary>How many maps follow the cells: past the last of them a read is of memory this game does not keep.</summary>
    private const int MapsKept = 8;

    /// <summary>A map's cells are read in runs of eight along a row: four runs to a row of each half, the left half's 32 rows first.</summary>
    private const int RunCells = 8, RunsPerHalfRow = 4, MapRows = 32;

    private const int RunsPerHalf = RunsPerHalfRow * MapRows, RunsPerMap = RunsPerHalf * 2, HalfWidth = RunCells * RunsPerHalfRow;

    private readonly KeptExploredMaps _maps;
    private readonly ScreenScroll[] _cells = new ScreenScroll[Count];
    private int _columns = 1;

    /// <summary>The cells of <paramref name="session"/>, whose kept explored maps a read past them reads.</summary>
    public KeptScrollCells(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _maps = KeptExploredMaps.Of(session);
    }

    /// <inheritdoc/>
    public void Load(int columns, int rows, IReadOnlyList<ScreenScroll> screens, string layout, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(screens);
        ArgumentNullException.ThrowIfNull(content);
        if (screens.Count != columns * rows || screens.Count > Count)
            throw new ArgumentException($"A room of {columns} by {rows} screens was given {screens.Count} scroll states; the game keeps {Count}.", nameof(screens));
        _columns = columns;
        for (int cell = 0; cell < screens.Count; cell++)
            _cells[cell] = screens[cell];

        // Only an explicit table writes past the room's own screens: every cell to the fiftieth.
        DataNode layouts = content.ReadData(LayoutsFile);
        if (layout.Length == 0 || !layouts.Has(layout))
            return;
        List<ScreenScroll> past = [.. layouts.List(layout).Select(state => state.AsEnum<ScreenScroll>())];
        if (screens.Count + past.Count != Count)
            throw new InvalidDataException($"The scroll layout \"{layout}\" gives {past.Count} cells after a room of {screens.Count} screens; together they must be {Count}.");
        for (int cell = 0; cell < past.Count; cell++)
            _cells[screens.Count + cell] = past[cell];
    }

    /// <inheritdoc/>
    public ScreenScroll Read(int column, int row)
    {
        // A screen's column and row are each a byte, as the cartridge takes them from the view's place.
        int cell = (row & 0xFF) * _columns + (column & 0xFF);
        return cell < Count ? _cells[cell] : ReadPast(cell - Count, column, row);
    }

    /// <summary>
    /// What a read <paramref name="cellsPast"/> cells past the fifty finds: the kept copies of the explored map, area
    /// by area, each run of eight cells as one screen. A run with nothing explored is a closed screen, one whose only
    /// explored cell is its last an open one aligned to its top, and any other an open one.
    /// </summary>
    private ScreenScroll ReadPast(int cellsPast, int column, int row)
    {
        int map = cellsPast / RunsPerMap;
        if (map >= MapsKept)
        {
            throw new InvalidOperationException(
                $"The view asked about the screen at column {column}, row {row}, past every explored map the cartridge keeps after its scroll cells: memory this game does not keep.");
        }

        if (map >= MapOrder.Length)
            return ScreenScroll.Closed;
        int run = cellsPast % RunsPerMap;
        int firstColumn = run / RunsPerHalf * HalfWidth + run % RunsPerHalfRow * RunCells;
        int mapRow = run % RunsPerHalf / RunsPerHalfRow;
        int explored = 0;
        for (int mapColumn = firstColumn; mapColumn < firstColumn + RunCells; mapColumn++)
        {
            if (_maps.IsExploredInCopy(MapOrder[map], mapColumn, mapRow))
                explored++;
        }

        if (explored == 0)
            return ScreenScroll.Closed;
        return explored == 1 && _maps.IsExploredInCopy(MapOrder[map], firstColumn + RunCells - 1, mapRow)
            ? ScreenScroll.OpenAlignedToTop
            : ScreenScroll.Open;
    }

    /// <inheritdoc/>
    public void Write(int column, int row, ScreenScroll state)
    {
        if (column < 0 || column >= _columns)
            throw new ArgumentOutOfRangeException(nameof(column), column, "The screen's column lies outside the room.");
        int cell = row * _columns + column;
        if (row < 0 || cell >= Count)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"The screen lies outside the {Count} scroll cells the game keeps.");
        _cells[cell] = state;
    }
}
