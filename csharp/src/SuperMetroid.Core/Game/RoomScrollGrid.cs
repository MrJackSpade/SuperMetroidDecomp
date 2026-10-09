using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Super Metroid's 50-byte room scroll-zone buffer at WRAM <c>$7E:CD20-$7E:CD51</c>.
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
    public const int StorageByteCount = 0x32;
    /// <summary>24-bit WRAM address of the first room scroll-zone byte.</summary>
    public const int WorkRamAddress = 0x7ecd20;

    private readonly byte[] _cells = new byte[StorageByteCount];
    private readonly ISnesAddressSpace _bus;

    private RoomScrollGrid(ISnesAddressSpace bus, int widthInScreens, int heightInScreens)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        if (widthInScreens <= 0 || heightInScreens <= 0 || widthInScreens * heightInScreens > StorageByteCount)
            throw new ArgumentOutOfRangeException(nameof(widthInScreens));

        WidthInScreens = widthInScreens;
        HeightInScreens = heightInScreens;
    }

    /// <summary>Gets the room width in 256-pixel scroll-zone cells.</summary>
    public int WidthInScreens { get; }
    /// <summary>Gets the room height in 256-pixel scroll-zone cells.</summary>
    public int HeightInScreens { get; }
    /// <summary>Gets the number of logical room cells within the fixed storage allocation.</summary>
    public int LogicalCellCount => WidthInScreens * HeightInScreens;

    /// <summary>Installs an application-owned copy of the native 50-byte scroll allocation.</summary>
    public static RoomScrollGrid LoadCompiled(
        ISnesAddressSpace bus,
        ReadOnlySpan<byte> storage,
        int widthInScreens,
        int heightInScreens)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (storage.Length != StorageByteCount)
        {
            throw new ArgumentException(
                $"Compiled room scroll storage must contain exactly {StorageByteCount} bytes.",
                nameof(storage));
        }

        var grid = new RoomScrollGrid(bus, widthInScreens, heightInScreens);
        for (int index = 0; index < StorageByteCount; index++)
        {
            byte value = storage[index];
            grid._cells[index] = value;
            bus.WriteByte(WorkRamAddress + index, value);
        }

        return grid;
    }


    /// <summary>
    /// Builds the implicit scroll table used when a room state's scroll word is nonnegative.
    /// </summary>
    /// <remarks>
    /// This is the literal nested loop at $82:E88D: every row begins as green value two,
    /// while the final row receives <paramref name="lastRowState" />. The loop writes only
    /// the room's width x height cells; the rest of the 50-byte WRAM allocation keeps what
    /// the previous room left there, and the scroll routines read it at the room's edges.
    /// </remarks>
    public static RoomScrollGrid CreateImplicit(
        ISnesAddressSpace bus,
        int widthInScreens,
        int heightInScreens,
        RoomScrollState lastRowState)
    {
        ArgumentNullException.ThrowIfNull(bus);
        RoomScrollStates.Validate(lastRowState, nameof(lastRowState));
        ISnesMutableMemory memory = bus as ISnesMutableMemory ?? throw new InvalidOperationException(
            "Implicit scroll storage keeps the previous room's bytes past its cells; it requires WRAM.");
        var grid = new RoomScrollGrid(bus, widthInScreens, heightInScreens);
        for (int index = 0; index < StorageByteCount; index++)
        {
            if (index >= grid.LogicalCellCount)
            {
                grid._cells[index] = memory.ReadWorkRamByte(WorkRamAddress + index);
                continue;
            }
            byte value = (byte)(index / widthInScreens == heightInScreens - 1
                ? lastRowState
                : RoomScrollState.Green);
            grid._cells[index] = value;
            bus.WriteByte(WorkRamAddress + index, value);
        }
        return grid;
    }

    /// <summary>Reads one raw WRAM index used by the assembly's multiplication arithmetic.</summary>
    public byte ReadStorage(int index)
    {
        if ((uint)index >= StorageByteCount)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Scroll routine indexed outside the 50-byte WRAM buffer.");
        return _cells[index];
    }

    /// <summary>Typed view of one byte inside the owned 50-byte scroll allocation.</summary>
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
        if ((uint)index > 0xffff)
            throw new ArgumentOutOfRangeException(nameof(index));
        return index < StorageByteCount
            ? _cells[index]
            : (_bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Native scroll edge reads require WRAM."))
                .ReadWorkRamByte(WorkRamAddress + index);
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
        _cells[index] = (byte)state;
        _bus.WriteByte(WorkRamAddress + index, (byte)state);
    }

    /// <summary>Updates a logical cell as a scroll PLM would.</summary>
    public void SetLogicalState(int x, int y, RoomScrollState state)
    {
        if ((uint)x >= WidthInScreens || (uint)y >= HeightInScreens)
            throw new ArgumentOutOfRangeException(nameof(x));
        RoomScrollStates.Validate(state, nameof(state));
        int index = y * WidthInScreens + x;
        _cells[index] = (byte)state;
        _bus.WriteByte(WorkRamAddress + index, (byte)state);
    }
}
