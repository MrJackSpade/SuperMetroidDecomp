namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded table of little-endian WRAM words at a fixed CPU address, stepping two bytes per
/// element within the bank. Constructed only by domain memory views.
/// </summary>
[CpuBusAccess]
internal readonly struct WramWords
{
    /// <summary>Mutable CPU bus supplying the table's two-byte elements.</summary>
    private readonly ISnesMemoryBus bus;
    /// <summary>CPU address of the table's first word.</summary>
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
            return WramWordConstants.AtElement(bus, SnesAddressMath.AddWithinBank(address, index * 2));
        }
    }
}
