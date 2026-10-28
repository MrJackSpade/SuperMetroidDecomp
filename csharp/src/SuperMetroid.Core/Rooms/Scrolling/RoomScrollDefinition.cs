namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scroll data a room state selects: one native 50-byte allocation, or the implicit layout that marks every cell
/// green except the bottom row. <see cref="RoomScrollGrid"/> checks the allocation's length and the bottom row's
/// state when it loads them.
/// </summary>
/// <param name="Name">The scroll data's name, the disassembly's label for it without its prefix.</param>
/// <param name="Storage">An explicit allocation's raw bytes, including bytes beyond the room's logical screen cells; empty
/// for an implicit layout. Logical cells use red/blue/green states, while retained trailing bytes need not be valid states.</param>
/// <param name="ImplicitBottomRow">An implicit layout's bottom-row state, or null for an explicit allocation.</param>
public sealed record RoomScrollDefinition(string Name, ReadOnlyMemory<byte> Storage, RoomScrollState? ImplicitBottomRow = null)
{
    /// <summary>An implicit layout: every cell green except the bottom row, which is <paramref name="bottomRow"/>.</summary>
    public static RoomScrollDefinition Implicit(string name, RoomScrollState bottomRow) => new(name, ReadOnlyMemory<byte>.Empty, bottomRow);
}
