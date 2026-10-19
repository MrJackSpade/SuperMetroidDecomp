namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scroll data a room state selects: one native 50-byte allocation, or the implicit layout that marks every cell
/// green except the bottom row.
/// </summary>
public sealed class RoomScrollDefinition
{
    /// <summary>Copies a decoded native scroll allocation.</summary>
    /// <param name="name">The allocation's name, the disassembly's label for it without its prefix.</param>
    /// <param name="storage">Raw allocation bytes, exactly 50, including bytes beyond the room's logical screen cells; state validation belongs to grid consumers.</param>
    /// <exception cref="ArgumentNullException">The name or storage is null.</exception>
    /// <exception cref="ArgumentException">The storage is not exactly 50 bytes.</exception>
    public RoomScrollDefinition(string name, byte[] storage)
    {
        Name = Ensure.NotNull(name);
        Ensure.NotNull(storage);
        Ensure.LengthEqual<byte>(storage, RoomScrollGrid.StorageByteCount);
        Storage = storage.ToArray();
    }

    /// <summary>Describes an implicit layout whose bottom row is <paramref name="bottomRow"/>.</summary>
    private RoomScrollDefinition(string name, RoomScrollState bottomRow)
    {
        Name = name;
        RoomScrollStates.Validate(bottomRow, nameof(bottomRow));
        ImplicitBottomRow = bottomRow;
    }

    /// <summary>An implicit layout: every cell green except the bottom row, which is <paramref name="bottomRow"/>.</summary>
    public static RoomScrollDefinition Implicit(string name, RoomScrollState bottomRow) => new(Ensure.NotNull(name), bottomRow);

    /// <summary>The scroll data's name.</summary>
    public string Name { get; }

    /// <summary>Read-only view of an explicit allocation's raw bytes, empty for an implicit layout; logical cells use red/blue/green states, while retained trailing bytes need not be valid states.</summary>
    public ReadOnlyMemory<byte> Storage { get; }

    /// <summary>An implicit layout's bottom-row state, or null for an explicit allocation.</summary>
    public RoomScrollState? ImplicitBottomRow { get; }
}
