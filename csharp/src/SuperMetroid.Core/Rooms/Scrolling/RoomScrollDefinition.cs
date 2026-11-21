namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scroll data a room state selects (#2560): a closed choice of an explicit list of the room's screens, or the implicit
/// layout that lets the camera scroll over every screen except the bottom row. <see cref="RoomScrollGrid"/> checks the
/// list's length and the bottom row's state when it loads them.
/// </summary>
public abstract partial record RoomScrollDefinition
{
    /// <summary>Keeps the scroll data's name; only the two layouts below are scroll data.</summary>
    private RoomScrollDefinition(string name) => Name = Ensure.NotNull(name);

    /// <summary>
    /// The scroll data's name, by which the cells an explicit list's load writes past the room's screens are found
    /// (<see cref="ScrollCellsPastRoomScreens{TCell}"/>).
    /// </summary>
    public string Name { get; }

    /// <summary>An implicit layout: the camera scrolls over every cell except the bottom row, which is <paramref name="bottomRow"/>.</summary>
    public static RoomScrollDefinition Implicit([InstalledKey] string name, RoomScrollState bottomRow) => new ImplicitLayout(name, bottomRow);

    /// <summary>An explicit list of the room's screens.</summary>
    /// <param name="Name">The list's name.</param>
    /// <param name="Cells">The room's own screens, row by row.</param>
    public sealed partial record ExplicitLayout([InstalledKey] string Name, IReadOnlyList<RoomScrollState> Cells) : RoomScrollDefinition(Name);

    /// <summary>The implicit layout: every row of the room open except the bottom row.</summary>
    /// <param name="Name">The layout's name.</param>
    /// <param name="BottomRow">The bottom row's state.</param>
    public sealed partial record ImplicitLayout([InstalledKey] string Name, RoomScrollState BottomRow) : RoomScrollDefinition(Name);
}
