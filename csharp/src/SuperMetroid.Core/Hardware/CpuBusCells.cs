namespace SuperMetroid.Core.Hardware;

/// <summary>The mutable WRAM/SRAM bus a typed domain memory view reads and writes (#629).</summary>
[CpuBusAccess]
public interface ISnesMemoryBus : ISnesAddressSpace, ISnesMutableMemory;

/// <summary>One WRAM byte at a fixed CPU address. Constructed only by domain memory views.</summary>
[CpuBusAccess]
public readonly struct WramByte
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Stores address.</summary>
    private readonly int address;

    /// <summary>Binds a one-byte WRAM <paramref name="cell"/>.</summary>
    public WramByte(ISnesMemoryBus bus, CpuBusCell cell)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(cell);
        cell.Require(CpuBusRegion.WorkRam, 1);
        address = cell.Address;
    }

    /// <summary>Reads the byte.</summary>
    public byte Read() => bus.ReadWorkRamByte(address);

    /// <summary>Writes the byte.</summary>
    public void Write(byte value) => bus.WriteByte(address, value);
}

/// <summary>
/// One little-endian WRAM word at a fixed CPU address; the high byte wraps within the bank as the
/// 65C816 does for a fixed-bank access. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
public readonly struct WramWord
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Stores address.</summary>
    private readonly int address;

    /// <summary>Binds a two-byte WRAM <paramref name="cell"/>.</summary>
    public WramWord(ISnesMemoryBus bus, CpuBusCell cell)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(cell);
        cell.Require(CpuBusRegion.WorkRam, 2);
        address = cell.Address;
    }

    /// <summary>Initializes a new wram word instance.</summary>
    private WramWord(ISnesMemoryBus bus, int address)
    {
        this.bus = bus;
        this.address = address;
    }

    /// <summary>The word at an element address already bounds-checked by its table.</summary>
    internal static WramWord AtElement(ISnesMemoryBus bus, int address) => new(bus, address);

    /// <summary>Reads the word: low byte first, then the bank-wrapped high byte.</summary>
    public ushort Read() =>
        bus.ReadWorkRamWordFixedBank(address);

    /// <summary>Writes the word: low byte first, then the bank-wrapped high byte.</summary>
    public void Write(ushort value)
    {
        bus.WriteByte(address, (byte)value);
        bus.WriteByte(SnesAddressMath.AddWithinBank(address, 1), SnesLittleEndian.HighByte(value));
    }
}

/// <summary>
/// A bounded WRAM byte range at a fixed CPU address. Element offsets wrap within the bank as a
/// fixed-bank indexed access does. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
public readonly struct WramBytes
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Stores address.</summary>
    private readonly int address;

    /// <summary>Binds a WRAM range <paramref name="cell"/>.</summary>
    public WramBytes(ISnesMemoryBus bus, CpuBusCell cell)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(cell);
        cell.Require(CpuBusRegion.WorkRam, cell.ByteCount);
        address = cell.Address;
        Length = cell.ByteCount;
    }

    /// <summary>The number of bytes in the range.</summary>
    public int Length { get; }

    /// <summary>Reads byte <paramref name="index"/>.</summary>
    public byte Read(int index) => bus.ReadWorkRamByte(Address(index));

    /// <summary>Writes byte <paramref name="index"/>.</summary>
    public void Write(int index, byte value) => bus.WriteByte(Address(index), value);

    /// <summary>Writes <paramref name="bytes"/> over the whole range, in address order.</summary>
    public void Write(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, Length);
        for (int index = 0; index < bytes.Length; index++)
            Write(index, bytes[index]);
    }

    /// <summary>Address.</summary>
    private int Address(int index)
    {
        if ((uint)index >= (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"WRAM range holds {Length} bytes.");
        return SnesAddressMath.AddWithinBank(address, index);
    }
}

/// <summary>
/// A bounded table of little-endian WRAM words at a fixed CPU address, stepping two bytes per
/// element within the bank. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
public readonly struct WramWords
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Stores address.</summary>
    private readonly int address;

    /// <summary>Binds a WRAM word-table <paramref name="cell"/> (an even byte count).</summary>
    public WramWords(ISnesMemoryBus bus, CpuBusCell cell)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(cell);
        if (cell.ByteCount % 2 != 0)
            throw new ArgumentException($"{cell.Name} holds {cell.ByteCount} bytes, not whole words.", nameof(cell));
        cell.Require(CpuBusRegion.WorkRam, cell.ByteCount);
        address = cell.Address;
        Count = cell.ByteCount / 2;
    }

    /// <summary>The number of words in the table.</summary>
    public int Count { get; }

    /// <summary>Element <paramref name="index"/> as a word cell.</summary>
    public WramWord this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"WRAM table holds {Count} words.");
            return WramWord.AtElement(bus, SnesAddressMath.AddWithinBank(address, index * 2));
        }
    }
}

/// <summary>
/// A bounded SRAM byte range at a fixed CPU address in a bank-$70-$7D/$F0-$FF window.
/// Constructed only by domain memory views and save codecs.
/// </summary>
[CpuBusAccess]
public readonly struct SramBytes
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Stores address.</summary>
    private readonly int address;

    /// <summary>Binds an SRAM range <paramref name="cell"/>.</summary>
    public SramBytes(ISnesMemoryBus bus, CpuBusCell cell)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(cell);
        cell.Require(CpuBusRegion.SaveRam, cell.ByteCount);
        address = cell.Address;
        Length = cell.ByteCount;
    }

    /// <summary>The number of bytes in the range.</summary>
    public int Length { get; }

    /// <summary>Reads byte <paramref name="index"/>.</summary>
    public byte Read(int index) => bus.ReadSaveRamByte(Address(index));

    /// <summary>Writes byte <paramref name="index"/>.</summary>
    public void Write(int index, byte value) => bus.WriteByte(Address(index), value);

    /// <summary>Reads the little-endian word at byte <paramref name="index"/>.</summary>
    public ushort ReadWord(int index) => (ushort)(SnesLittleEndian.Word(Read(index), Read(index + 1)));

    /// <summary>Writes the little-endian word at byte <paramref name="index"/>, low byte first.</summary>
    public void WriteWord(int index, ushort value)
    {
        Write(index, (byte)value);
        Write(index + 1, SnesLittleEndian.HighByte(value));
    }

    /// <summary>Address.</summary>
    private int Address(int index)
    {
        if ((uint)index >= (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"SRAM range holds {Length} bytes.");
        return address + index;
    }
}
