namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded WRAM byte buffer that a domain memory view exposes (#629). It is bound to one
/// catalog cell, so callers address bytes by their offset within the buffer's own domain
/// layout (a tile sheet, a bitset, a staging page) and can never reach outside it.
/// </summary>
[CpuBusBoundary("domain view: bounded WRAM byte buffer bound to one catalog cell")]
public readonly struct WorkRamByteTable
{
    /// <summary>Bounded byte access to the catalog cell backing this table.</summary>
    private readonly WramBytes bytes;

    /// <summary>Binds the buffer to a WRAM range <paramref name="cell"/>.</summary>
    internal WorkRamByteTable(ISnesMemoryBus bus, CpuBusCell cell) => bytes = new WramBytes(bus, cell);

    /// <summary>The number of bytes in the buffer.</summary>
    public int Length => bytes.Length;

    /// <summary>Byte <paramref name="index"/> of the buffer.</summary>
    public byte this[int index]
    {
        get => bytes.Read(index);
        set => bytes.Write(index, value);
    }

    /// <summary>Reads the whole buffer into <paramref name="destination"/>, in address order.</summary>
    public void CopyTo(Span<byte> destination)
    {
        Ensure.LengthEqual<byte>(destination, Length);
        for (int index = 0; index < destination.Length; index++)
            destination[index] = bytes.Read(index);
    }

    /// <summary>Writes <paramref name="source"/> over the whole buffer, in address order.</summary>
    public void CopyFrom(ReadOnlySpan<byte> source) => bytes.Write(source);

    /// <summary>Writes <paramref name="value"/> to every byte, in address order.</summary>
    public void Fill(byte value)
    {
        for (int index = 0; index < Length; index++)
            bytes.Write(index, value);
    }

    /// <summary>Reads the little-endian word whose low byte is at <paramref name="byteOffset"/>, low byte first.</summary>
    public ushort ReadWord(int byteOffset) =>
        (ushort)(SnesLittleEndian.Word(bytes.Read(byteOffset), bytes.Read(byteOffset + 1)));

    /// <summary>Writes the little-endian word whose low byte is at <paramref name="byteOffset"/>, low byte first.</summary>
    public void WriteWord(int byteOffset, ushort value)
    {
        bytes.Write(byteOffset, (byte)value);
        bytes.Write(byteOffset + 1, SnesLittleEndian.HighByte(value));
    }
}
