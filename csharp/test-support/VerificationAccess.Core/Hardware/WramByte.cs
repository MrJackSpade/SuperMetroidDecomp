using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>One WRAM byte at a fixed CPU address. Constructed only by domain memory views.</summary>
internal readonly struct WramByte
{
    /// <summary>Mutable CPU bus supplying this cell's WRAM reads and writes.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>Fixed CPU address validated as a one-byte WRAM cell.</summary>
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
