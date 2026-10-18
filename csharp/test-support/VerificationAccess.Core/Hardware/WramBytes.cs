namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded WRAM byte range at a fixed CPU address. Element offsets wrap within the bank as a
/// fixed-bank indexed access does. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
public readonly struct WramBytes
{
    /// <summary>Mutable CPU bus supplying this bounded WRAM range.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>CPU address of the range's first byte.</summary>
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

    /// <summary>Checks a byte index and adds it with fixed-bank CPU address wrapping.</summary>
    private int Address(int index)
    {
        if ((uint)index >= (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"WRAM range holds {Length} bytes.");
        return SnesAddressMath.AddWithinBank(address, index);
    }
}
