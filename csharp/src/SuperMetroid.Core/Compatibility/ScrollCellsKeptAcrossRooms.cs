namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The scroll cells the game keeps for the room being played, which outlive every room (#1551). The game keeps one set
/// of fifty cells for every room. A room whose scroll data is an explicit table loads all fifty: its own cells, and past
/// a table shorter than fifty cells the data the game stores after it, read as cells. A room whose scroll data is the
/// implicit layout writes only its own width-by-height cells, so every cell past them keeps what an earlier room, its
/// scroll triggers, doors or enemies left there. A scroll check past the fifty cells reads the stored explored maps
/// (<see cref="StoredExploredMaps{TArea}"/>). The camera's scroll-boundary checks read a cell past the room's own at the
/// room's bottom and right edges, so a cell an earlier room left decides whether the camera keeps following Samus or
/// stops at the edge.
/// </summary>
/// <remarks>
/// Loaded when each room's scroll data is set up during the room load; written by scroll triggers, door setups, enemies
/// and scripted events whenever they change a cell; consulted by the camera's scroll-boundary checks during Samus's
/// scrolling on every update, and by enemies that read a cell. The session's first room finds every cell cleared.
/// Evidence: the scroll loaders (<c>$82:E878</c> copies fifty bytes from an explicit table whatever the room's size;
/// <c>$82:E88D</c> writes only the room's cells for an implicit layout) and the camera's scroll steps (<c>$80:A641</c>,
/// <c>$80:A6BB</c>, <c>$80:A893</c>, <c>$80:A936</c>), which index the cells at <c>$7E:CD20</c> by a row times the room
/// width plus a column and tell a cleared cell, a blue cell and every other value apart; in the 13% movie Samus falls
/// into a three-by-two implicit room whose cell below the bottom row still holds the room before's blue, so the camera
/// keeps following her. The explicit tables' data after a short table is decoded once into cells.
/// </remarks>
/// <typeparam name="TCell">A scroll cell's state.</typeparam>
/// <typeparam name="TArea">An area of the game, by which the stored explored maps are kept.</typeparam>
[CompatibilityService("a room's scroll cells past its own keep what earlier rooms left there, and checks past every cell read the stored explored maps")]
public sealed class ScrollCellsKeptAcrossRooms<TCell, TArea>
    where TCell : struct, Enum
    where TArea : struct, Enum
{
    /// <summary>Every kept cell, by its place in the rows of the room being played.</summary>
    private readonly TCell[] _cells;

    /// <summary>The session's stored explored maps, which a check past every cell reads.</summary>
    private readonly StoredExploredMaps<TArea> _storedExploredMaps;

    /// <summary>What a run of a stored map with no explored cell reads as: a cleared cell.</summary>
    private readonly TCell _cleared;

    /// <summary>What a run of a stored map whose only explored cell is its last reads as.</summary>
    private readonly TCell _onlyLastCellExplored;

    /// <summary>What a run of a stored map with any other exploration reads as.</summary>
    private readonly TCell _otherwiseExplored;

    /// <summary>Starts with every cell cleared, as power-on leaves them.</summary>
    /// <param name="count">How many cells the game keeps.</param>
    /// <param name="cleared">The state of a cleared cell, which a stored-map run with no explored cell also reads as.</param>
    /// <param name="storedExploredMaps">The session's stored explored maps, which a check past every cell reads.</param>
    /// <param name="onlyLastCellExplored">What a stored-map run whose only explored cell is its last reads as.</param>
    /// <param name="otherwiseExplored">What a stored-map run with any other exploration reads as.</param>
    public ScrollCellsKeptAcrossRooms(int count, TCell cleared, StoredExploredMaps<TArea> storedExploredMaps,
        TCell onlyLastCellExplored, TCell otherwiseExplored)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "The game keeps at least one scroll cell.");
        _cells = new TCell[count];
        Array.Fill(_cells, cleared);
        _cleared = cleared;
        _storedExploredMaps = Ensure.NotNull(storedExploredMaps);
        _onlyLastCellExplored = onlyLastCellExplored;
        _otherwiseExplored = otherwiseExplored;
    }

    /// <summary>How many cells the game keeps.</summary>
    public int Count => _cells.Length;

    /// <summary>Loads every kept cell, as an explicit scroll table's load does.</summary>
    /// <param name="cells">One state for every kept cell, in order.</param>
    /// <exception cref="ArgumentException">The table does not hold one state for every kept cell.</exception>
    public void LoadAll(IReadOnlyList<TCell> cells)
    {
        Ensure.NotNull(cells);
        if (cells.Count != _cells.Length)
            throw new ArgumentException($"An explicit scroll table holds all {_cells.Length} kept cells.", nameof(cells));
        for (int cell = 0; cell < _cells.Length; cell++)
            _cells[cell] = cells[cell];
    }

    /// <summary>The state of kept cell <paramref name="cell"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the kept cells.</exception>
    public TCell Read(int cell) => _cells[KeptCell(cell)];

    /// <summary>Sets kept cell <paramref name="cell"/> to <paramref name="state"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the kept cells.</exception>
    public void Write(int cell, TCell state) => _cells[KeptCell(cell)] = state;

    /// <summary>
    /// What a camera check reads at <paramref name="cell"/>: a kept cell, or, past every kept cell, the run of the stored
    /// explored maps it lands on.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell is negative, or lies past every stored map.</exception>
    public TCell ReadForCamera(int cell)
    {
        if (Ensure.AtLeastZero(cell) < _cells.Length)
            return _cells[cell];
        return _storedExploredMaps.ReadPastScrollCells(cell - _cells.Length) switch
        {
            StoredExploredMaps<TArea>.RunReading.NothingExplored => _cleared,
            StoredExploredMaps<TArea>.RunReading.OnlyLastCellExplored => _onlyLastCellExplored,
            StoredExploredMaps<TArea>.RunReading.OtherwiseExplored => _otherwiseExplored,
            var reading => throw new ArgumentOutOfRangeException(nameof(cell), reading, "Unknown stored-map reading."),
        };
    }

    /// <summary>Requires <paramref name="cell"/> to be a kept cell.</summary>
    private int KeptCell(int cell) =>
        (uint)cell < (uint)_cells.Length
            ? cell
            : throw new ArgumentOutOfRangeException(nameof(cell), cell, $"The game keeps {_cells.Length} scroll cells.");
}
