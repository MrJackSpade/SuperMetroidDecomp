namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scroll data a room state selects: an explicit list of the room's screens, or the implicit layout that lets the
/// camera scroll over every screen except the bottom row. <see cref="RoomScrollGrid"/> checks the list's length and the
/// bottom row's state when it loads them.
/// </summary>
/// <param name="Name">The scroll data's name, by which the cells an explicit list's load writes past the room's screens
/// are found (<see cref="ScrollCellsPastRoomScreens{TCell}"/>).</param>
/// <param name="Cells">An explicit list's cells: the room's own screens, row by row; empty for an implicit layout.</param>
/// <param name="ImplicitBottomRow">An implicit layout's bottom-row state, or null for an explicit list.</param>
public sealed record RoomScrollDefinition([InstalledKey] string Name, IReadOnlyList<RoomScrollState> Cells, RoomScrollState? ImplicitBottomRow = null)
{
    /// <summary>An implicit layout: the camera scrolls over every cell except the bottom row, which is <paramref name="bottomRow"/>.</summary>
    public static RoomScrollDefinition Implicit([InstalledKey] string name, RoomScrollState bottomRow) => new(name, [], bottomRow);
}
