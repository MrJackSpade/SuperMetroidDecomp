namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded table of little-endian WRAM words that a domain memory view exposes (#629).
/// Callers index whole elements; the view's catalog cell owns the address and the bound.
/// </summary>
[CpuBusBoundary("domain view: bounded WRAM word table bound to one catalog cell")]
public readonly struct WorkRamWordTable
{
    /// <summary>Bounded little-endian word access to the catalog cell backing this table.</summary>
    private readonly WramWords words;

    /// <summary>Binds the table to a WRAM word-table <paramref name="cell"/>.</summary>
    internal WorkRamWordTable(ISnesMemoryBus bus, CpuBusCell cell) => words = new WramWords(bus, cell);

    /// <summary>The number of words in the table.</summary>
    public int Count => words.Count;

    /// <summary>Word <paramref name="index"/>: read low byte first, written low byte first.</summary>
    public ushort this[int index]
    {
        get => words[index].Read();
        set => words[index].Write(value);
    }

    /// <summary>Copies every word out, in element order.</summary>
    public ushort[] ToArray()
    {
        var values = new ushort[Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = words[index].Read();
        return values;
    }
}
