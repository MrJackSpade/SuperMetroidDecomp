using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One little-endian WRAM word at a fixed CPU address; the high byte wraps within the bank as the
/// 65C816 does for a fixed-bank access. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
internal readonly struct WramWord
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
