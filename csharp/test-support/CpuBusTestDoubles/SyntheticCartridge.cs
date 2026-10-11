using SuperMetroid.Core.Hardware;

/// <summary>
/// Exact bus test double (#629): a constructed cartridge that serves only declared bytes. Seeded bytes
/// return their values, declared zero-filled ranges return zero, and every other read fails with the
/// configured exception, so an importer or audit proves exactly which cartridge bytes it consumes.
/// Every served read is logged in order; writes are rejected.
/// </summary>
[CpuBusBoundary("exact bus test: constructed cartridge serving only declared bytes")]
internal sealed class SyntheticCartridge : ISnesAddressSpace, IImportCartridgeSource
{
    private readonly Dictionary<int, byte> bytes = [];
    private readonly List<Func<int, bool>> zeroFilled = [];
    private readonly Func<int, Exception> undeclaredRead;
    private readonly Func<int, Exception> write;

    /// <summary>Creates an empty cartridge.</summary>
    /// <param name="undeclaredRead">The failure for a read of an undeclared CPU address.</param>
    /// <param name="write">The failure for any write.</param>
    internal SyntheticCartridge(Func<int, Exception> undeclaredRead, Func<int, Exception> write)
    {
        this.undeclaredRead = undeclaredRead ?? throw new ArgumentNullException(nameof(undeclaredRead));
        this.write = write ?? throw new ArgumentNullException(nameof(write));
    }

    /// <summary>Every served read's CPU address, in order.</summary>
    internal List<int> Reads { get; } = [];

    /// <summary>Seeds one byte.</summary>
    internal void SetByte(int address, byte value) => bytes[address] = value;

    /// <summary>Seeds a little-endian word at <paramref name="address"/> and the following byte.</summary>
    internal void SetWord(int address, int value)
    {
        SetByte(address, unchecked((byte)value));
        SetByte(address + 1, unchecked((byte)(value >> 8)));
    }

    /// <summary>Declares the <paramref name="count"/> bytes from <paramref name="start"/> readable as zero.</summary>
    internal void DeclareZero(int start, int count) => DeclareZero(address => address >= start && address < start + count);

    /// <summary>Declares every address <paramref name="match"/> selects readable as zero.</summary>
    internal void DeclareZero(Func<int, bool> match) => zeroFilled.Add(match ?? throw new ArgumentNullException(nameof(match)));

    /// <inheritdoc />
    public byte ReadCartridgeByte(int cpuAddress)
    {
        if (bytes.TryGetValue(cpuAddress, out byte value))
        {
            Reads.Add(cpuAddress);
            return value;
        }
        foreach (Func<int, bool> match in zeroFilled)
        {
            if (!match(cpuAddress))
                continue;
            Reads.Add(cpuAddress);
            return 0;
        }
        throw undeclaredRead(cpuAddress);
    }

    /// <inheritdoc />
    public void WriteByte(int address, byte value) => throw write(address);
}
