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
    public const int StorageByteCount = 0x32;
    public const int WorkRamAddress = 0x7ecd20;
    public const int LandingSiteRomAddress = 0x8f9283;

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

    public int WidthInScreens { get; }
    public int HeightInScreens { get; }
    public int LogicalCellCount => WidthInScreens * HeightInScreens;

    /// <summary>
    /// All 50 bytes, including bytes beyond the room dimensions. The original explicit
    /// loader at <c>$82:E878-$82:E889</c> copies 25 words unconditionally, so edge reads can
    /// observe data following a shorter ROM table rather than an invented zero padding.
    /// </summary>
    public ReadOnlySpan<byte> Storage => _cells;

    /// <summary>Loads an explicit bank-$8F scroll table and mirrors it into authentic WRAM.</summary>
    public static RoomScrollGrid LoadExplicit(
        ISnesAddressSpace bus,
        int sourceAddress,
        int widthInScreens,
        int heightInScreens)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)sourceAddress > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress));

        var grid = new RoomScrollGrid(bus, widthInScreens, heightInScreens);
        int sourceBank = sourceAddress & 0xff0000;
        int sourceOffset = sourceAddress & 0xffff;
        for (int index = 0; index < StorageByteCount; index++)
        {
            byte value = bus.ReadByte(sourceBank | ((sourceOffset + index) & 0xffff));
            grid._cells[index] = value;
            bus.WriteByte(WorkRamAddress + index, value);
        }

        return grid;
    }

    /// <summary>Loads Landing Site's 9x5 table beginning at ROM <c>$8F:9283</c>.</summary>
    public static RoomScrollGrid LoadLandingSite(ISnesAddressSpace bus) =>
        LoadExplicit(bus, LandingSiteRomAddress, widthInScreens: 9, heightInScreens: 5);

    /// <summary>
    /// Builds the implicit scroll table used when a room state's scroll word is nonnegative.
    /// </summary>
    /// <remarks>
    /// This is the literal nested loop at $82:E84A: every row begins as blue/green value
    /// two, while the final row receives the low byte of <paramref name="lastRowValue" />.
    /// The remaining bytes in the fixed 50-byte WRAM allocation are cleared because this
    /// path constructs the buffer rather than copying adjacent ROM bytes into all 50 slots.
    /// </remarks>
    public static RoomScrollGrid CreateImplicit(
        ISnesAddressSpace bus,
        int widthInScreens,
        int heightInScreens,
        byte lastRowValue)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var grid = new RoomScrollGrid(bus, widthInScreens, heightInScreens);
        for (int index = 0; index < StorageByteCount; index++)
        {
            byte value = index < grid.LogicalCellCount
                ? (index / widthInScreens == heightInScreens - 1 ? lastRowValue : (byte)2)
                : (byte)0;
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
            : _bus.ReadByte(WorkRamAddress + index);
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
    public void SetStorage(int index, byte value)
    {
        if ((uint)index >= StorageByteCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        _cells[index] = value;
        _bus.WriteByte(WorkRamAddress + index, value);
    }

    /// <summary>Updates a logical cell as a scroll PLM would.</summary>
    public void SetLogicalCell(int x, int y, byte value)
    {
        if ((uint)x >= WidthInScreens || (uint)y >= HeightInScreens)
            throw new ArgumentOutOfRangeException(nameof(x));
        if (value > 2)
            throw new ArgumentOutOfRangeException(nameof(value), "Known room scroll values are red=0, blue=1, or green=2.");
        int index = y * WidthInScreens + x;
        _cells[index] = value;
        _bus.WriteByte(WorkRamAddress + index, value);
    }

    /// <summary>Semantic setter for known red, blue, and green scroll states.</summary>
    public void SetLogicalState(int x, int y, RoomScrollState state) =>
        SetLogicalCell(x, y, (byte)state);
}
