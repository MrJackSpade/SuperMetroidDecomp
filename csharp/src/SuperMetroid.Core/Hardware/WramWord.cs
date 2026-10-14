namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One little-endian WRAM word at a fixed CPU address; the high byte wraps within the bank as the
/// 65C816 does for a fixed-bank access. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
public readonly struct WramWord
{
    /// <summary>Mutable CPU bus supplying the word's bank-wrapped byte accesses.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Fixed CPU address of the word's low byte.</summary>
    private readonly int address;

    /// <summary>Binds a two-byte WRAM <paramref name="cell"/>.</summary>
    public WramWord(ISnesMemoryBus bus, CpuBusCell cell)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(cell);
        cell.Require(CpuBusRegion.WorkRam, 2);
        address = cell.Address;
    }

    /// <summary>Creates an element view after the containing word table has checked its bounds.</summary>
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
