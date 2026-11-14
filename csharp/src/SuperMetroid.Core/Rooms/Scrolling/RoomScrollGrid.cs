using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// One room's scroll grid: a <see cref="RoomScrollState"/> for each of its screens, row by row, kept among the session's
/// scroll cells (<see cref="ScrollCellsKeptAcrossRooms{TCell, TArea}"/>), whose cells past the room's own keep what earlier
/// rooms left there.
/// </summary>
/// <remarks>
/// Cells are screen-sized (256x256 pixels), not 16x16 room blocks. Closed cells form a camera boundary; open ones are
/// traversable, and those open with the camera aligned to the screen's top align the camera vertically differently. Scroll
/// triggers, doors, enemies and scripted events change cells during play.
/// </remarks>
public sealed class RoomScrollGrid
{
    /// <summary>How many scroll cells the game keeps: no room has more screens.</summary>
    public const int KeptCellCount = 50;

    /// <summary>The session's kept scroll cells, of which this room's are the first.</summary>
    private readonly ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> _cells;

    /// <summary>Validates the room's screen dimensions within the kept cells.</summary>
    private RoomScrollGrid(ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> cells, Extent16<RoomScreens> size)
    {
        _cells = Ensure.NotNull(cells);
        if (size.X == 0 || size.Y == 0 || size.X * size.Y > cells.Count)
            throw new ArgumentOutOfRangeException(nameof(size));

        Size = size;
    }

    /// <summary>Gets the room size in 256-pixel scroll cells.</summary>
    public Extent16<RoomScreens> Size { get; }

    /// <summary>Gets the number of the room's own cells.</summary>
    public int LogicalCellCount => Size.X * Size.Y;

    /// <summary>
    /// The session's scroll cells, cleared as power-on leaves them, with a check past them reading
    /// <paramref name="storedExploredMaps"/>: a run with nothing explored as closed, one whose only explored cell is its last
    /// as open with the camera aligned to the screen's top, and any other as open.
    /// </summary>
    public static ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> KeepCellsAcrossRooms(StoredExploredMaps<AreaId> storedExploredMaps) =>
        new(KeptCellCount, RoomScrollState.Closed, storedExploredMaps, RoomScrollState.OpenAlignedToTop, RoomScrollState.Open);

    /// <summary>Loads the scroll data <paramref name="scroll"/> selects into <paramref name="cells"/>, as the room loader does.</summary>
    /// <exception cref="ArgumentException">An explicit table does not hold one state for every kept cell.</exception>
    public static RoomScrollGrid FromDefinition(
        ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> cells,
        RoomScrollDefinition scroll,
        Extent16<RoomScreens> size)
    {
        Ensure.NotNull(scroll);
        if (scroll.ImplicitBottomRow is { } bottomRow)
            return CreateImplicit(cells, size, bottomRow);

        foreach (RoomScrollState state in scroll.Cells)
            Validate(state, nameof(scroll));
        var grid = new RoomScrollGrid(cells, size);
        cells.LoadAll(scroll.Cells);
        return grid;
    }

    /// <summary>
    /// Builds the implicit layout a room state selects when it names no explicit table: every row open except the last,
    /// which is <paramref name="lastRowState"/>.
    /// </summary>
    /// <remarks>
    /// Only the room's width x height cells are written; every kept cell past them keeps what the previous room left
    /// there, and the camera reads them at the room's edges.
    /// </remarks>
    public static RoomScrollGrid CreateImplicit(
        ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> cells,
        Extent16<RoomScreens> size,
        RoomScrollState lastRowState)
    {
        Validate(lastRowState, nameof(lastRowState));
        var grid = new RoomScrollGrid(cells, size);
        for (int index = 0; index < grid.LogicalCellCount; index++)
            cells.Write(index, index / size.X == size.Y - 1 ? lastRowState : RoomScrollState.Open);
        return grid;
    }

    /// <summary>The state of kept cell <paramref name="index"/>, the room's own or one past it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index lies outside the kept cells.</exception>
    public RoomScrollState ReadState(int index) => _cells.Read(index);

    /// <summary>
    /// The scroll state a camera check reads at the cell a scroll calculation selects.
    /// </summary>
    /// <remarks>
    /// Most reads stay in the room's own cells, but the bottom row and right column can select the kept cells past them,
    /// and a camera above the room selects a row past every kept cell; such a read observes the session's stored
    /// explored maps (<see cref="ScrollCellsKeptAcrossRooms{TCell, TArea}.ReadForCamera"/>) rather than throwing or
    /// clamping to a cell.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The index is negative, or past every stored map.</exception>
    public RoomScrollState ReadCameraState(int index) => _cells.ReadForCamera(index);

    /// <summary>
    /// Sets kept cell <paramref name="index"/>. Scroll triggers, doors and enemies name cells by their place in the kept
    /// cells rather than by screen, so a write may reach a cell past the room's own.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The index lies outside the kept cells, or the state is not a scroll state.</exception>
    public void SetCell(int index, RoomScrollState state)
    {
        Validate(state, nameof(state));
        _cells.Write(index, state);
    }

    /// <summary>
    /// Sets the screen at <paramref name="screen"/>, in one of the room's columns and in one of its rows or a row past them,
    /// as a door setup or a scroll trigger does: a row past the room's own names the kept cells after them, row by row at
    /// the room's width.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The column lies outside the room, the screen outside the kept cells, or the state is not a scroll state.
    /// </exception>
    public void SetScreen(Position16<RoomScreens> screen, RoomScrollState state)
    {
        if (screen.X >= Size.X)
            throw new ArgumentOutOfRangeException(nameof(screen), screen, "The screen's column lies outside the room.");
        SetCell(screen.Y * Size.X + screen.X, state);
    }

    /// <summary>Updates one of the room's screens as a scroll trigger would.</summary>
    public void SetLogicalState(Position16<RoomScreens> screen, RoomScrollState state)
    {
        if (screen.X >= Size.X || screen.Y >= Size.Y)
            throw new ArgumentOutOfRangeException(nameof(screen));
        Validate(state, nameof(state));
        _cells.Write(screen.Y * Size.X + screen.X, state);
    }

    /// <summary>Rejects a value that is not one of the three scroll states.</summary>
    private static void Validate(RoomScrollState state, string parameterName)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                state,
                "Known room scroll states are closed, open, and open with the camera aligned to the screen's top.");
        }
    }
}
