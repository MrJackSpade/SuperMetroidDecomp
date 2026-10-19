namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// One room's view of Super Metroid's 50-byte room scroll-zone allocation (<see cref="RoomScrollStorage"/>).
/// </summary>
/// <remarks>
/// Logical cells are screen-sized (256x256 pixels), not 16x16 room blocks. Values zero,
/// one, and two are conventionally called red, blue, and green scrolls by the disassembly.
/// Zero forms a camera boundary; one and two are traversable but affect vertical alignment
/// differently. Scroll PLMs can mutate these bytes during play.
/// </remarks>
public sealed class RoomScrollGrid
{
    /// <summary>Size in bytes of the fixed native room scroll-zone allocation.</summary>
    public const int StorageByteCount = 50;

    /// <summary>The session's allocation, including bytes past this room's logical cells.</summary>
    internal readonly RoomScrollStorage _storage;

    /// <summary>Validates screen-cell dimensions within the 50-byte native allocation.</summary>
    private RoomScrollGrid(RoomScrollStorage storage, Extent16<RoomScreens> size)
    {
        _storage = Ensure.NotNull(storage);
        if (size.X == 0 || size.Y == 0 || size.X * size.Y > StorageByteCount)
            throw new ArgumentOutOfRangeException(nameof(size));

        Size = size;
    }

    /// <summary>Gets the room size in 256-pixel scroll-zone cells.</summary>
    public Extent16<RoomScreens> Size { get; }
    /// <summary>Gets the number of logical room cells within the fixed storage allocation.</summary>
    public int LogicalCellCount => Size.X * Size.Y;

    /// <summary>Copies a compiled 50-byte allocation into the session's scroll storage, as <c>$82:E878</c> does.</summary>
    public static RoomScrollGrid LoadCompiled(
        RoomScrollStorage storage,
        ReadOnlySpan<byte> compiled,
        Extent16<RoomScreens> size)
    {
        Ensure.LengthEqual(compiled, StorageByteCount);
        var grid = new RoomScrollGrid(storage, size);
        compiled.CopyTo(storage._bytes);
        return grid;
    }

    /// <summary>Creates the scroll allocation <paramref name="scroll"/> selects over <paramref name="storage"/>.</summary>
    public static RoomScrollGrid FromDefinition(RoomScrollStorage storage, RoomScrollDefinition scroll, Extent16<RoomScreens> size)
    {
        Ensure.NotNull(scroll);
        return scroll.ImplicitBottomRow is { } bottomRow
            ? CreateImplicit(storage, size, bottomRow)
            : LoadCompiled(storage, scroll.Storage.Span, size);
    }

    /// <summary>
    /// Builds the implicit scroll table used when a room state's scroll word is nonnegative.
    /// </summary>
    /// <remarks>
    /// This is the literal nested loop at $82:E88D: every row begins as green value two,
    /// while the final row receives <paramref name="lastRowState" />. The loop writes only
    /// the room's width x height cells; the rest of the 50-byte allocation keeps what the
    /// previous room left there, and the scroll routines read it at the room's edges.
    /// </remarks>
    public static RoomScrollGrid CreateImplicit(
        RoomScrollStorage storage,
        Extent16<RoomScreens> size,
        RoomScrollState lastRowState)
    {
        RoomScrollStates.Validate(lastRowState, nameof(lastRowState));
        var grid = new RoomScrollGrid(storage, size);
        for (int index = 0; index < grid.LogicalCellCount; index++)
            storage._bytes[index] = (byte)(index / size.X == size.Y - 1
                ? lastRowState
                : RoomScrollState.Green);
        return grid;
    }

    /// <summary>Reads one raw allocation index used by the assembly's multiplication arithmetic.</summary>
    public byte ReadStorage(int index)
    {
        if ((uint)index >= StorageByteCount)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Scroll routine indexed outside the 50-byte allocation.");
        return _storage._bytes[index];
    }

    /// <summary>Typed view of one byte inside the 50-byte scroll allocation.</summary>
    public RoomScrollState ReadState(int index) => (RoomScrollState)ReadStorage(index);

    /// <summary>
    /// Reads the native WRAM-relative byte selected by a bank-$80 scroll calculation.
    /// </summary>
    /// <remarks>
    /// Most accesses stay in <c>$CD20..CD51</c>, but the bottom row can legally select
    /// index $32, which is the first byte of <c>ExploredMapTiles</c> at $CD52. The 65C816
    /// does not know the C# array ended there, so camera code must observe adjacent WRAM
    /// rather than throw or manufacture a clamped scroll cell.
    /// </remarks>
    public byte ReadNativeStorage(int index)
    {
        if ((uint)index > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(index));
        return index < StorageByteCount ? _storage._bytes[index] : RoomScrollDefinitions.ExploredMapOverreadByte;
    }

    /// <summary>
    /// Typed view of a native-relative read, including legal adjacent-WRAM reads. Enum casts
    /// preserve unexpected byte values; callers can still distinguish every raw state.
    /// </summary>
    public RoomScrollState ReadNativeState(int index) =>
        (RoomScrollState)ReadNativeStorage(index);

    /// <summary>
    /// Writes one raw byte in the fixed 50-byte scroll allocation. Enemy and PLM scripts use
    /// literal WRAM indexes rather than logical room coordinates, so this seam preserves their
    /// overlapping word stores without reverse-engineering them into guessed screen cells.
    /// </summary>
    public void SetStorage(int index, RoomScrollState state)
    {
        if ((uint)index >= StorageByteCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        RoomScrollStates.Validate(state, nameof(state));
        _storage._bytes[index] = (byte)state;
    }

    /// <summary>Updates a logical cell as a scroll PLM would.</summary>
    public void SetLogicalState(Position16<RoomScreens> screen, RoomScrollState state)
    {
        if (screen.X >= Size.X || screen.Y >= Size.Y)
            throw new ArgumentOutOfRangeException(nameof(screen));
        RoomScrollStates.Validate(state, nameof(state));
        _storage._bytes[screen.Y * Size.X + screen.X] = (byte)state;
    }
}
