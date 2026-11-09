namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scroll data a room state selects: an explicit table of every kept scroll cell, or the implicit layout that marks
/// every cell green except the bottom row. <see cref="RoomScrollGrid"/> checks the table's length and the bottom row's
/// state when it loads them.
/// </summary>
/// <param name="Name">The scroll data's name.</param>
/// <param name="Cells">An explicit table's <see cref="RoomScrollGrid.KeptCellCount"/> cells, in order: the room's own
/// screens row by row, then, past a room with fewer screens, what its load writes over the kept cells after them (for a
/// table shorter than the kept cells, the data stored after it, read as cells); empty for an implicit layout.</param>
/// <param name="ImplicitBottomRow">An implicit layout's bottom-row state, or null for an explicit table.</param>
public sealed record RoomScrollDefinition(string Name, IReadOnlyList<RoomScrollState> Cells, RoomScrollState? ImplicitBottomRow = null)
{
    /// <summary>An implicit layout: every cell green except the bottom row, which is <paramref name="bottomRow"/>.</summary>
    public static RoomScrollDefinition Implicit(string name, RoomScrollState bottomRow) => new(name, [], bottomRow);
}
