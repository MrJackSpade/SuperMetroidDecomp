namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded SRAM byte range at a fixed CPU address in a bank-$70-$7D/$F0-$FF window.
/// Constructed only by domain memory views and save codecs.
/// </summary>
[CpuBusAccess]
public readonly struct SramBytes
{
    /// <summary>Mutable CPU bus supplying this bounded save-RAM range.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>CPU address of the range's first byte in a save-RAM window.</summary>
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

    /// <summary>Checks a byte index and resolves its linear save-RAM window address.</summary>
    private int Address(int index)
    {
        if ((uint)index >= (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"SRAM range holds {Length} bytes.");
        return address + index;
    }
}
