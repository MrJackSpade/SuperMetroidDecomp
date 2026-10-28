using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded window of a <see cref="VramStagingBuffer"/>: byte access and little-endian words at byte offsets, so
/// callers address bytes within one staging meaning (a tile sheet, a rot table) and never reach outside it.
/// </summary>
public readonly struct StagingByteView
{
    /// <summary>The whole staging buffer.</summary>
    private readonly byte[] buffer;
    /// <summary>The first buffer byte of this view.</summary>
    private readonly int start;

    /// <summary>Binds a view of <paramref name="length"/> bytes at <paramref name="start"/>.</summary>
    internal StagingByteView(byte[] buffer, int start, int length)
    {
        this.buffer = Ensure.NotNull(buffer);
        if (start < 0 || length < 0 || start + length > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(length), length, $"View {start}+{length} exceeds the {buffer.Length}-byte buffer.");
        this.start = start;
        Length = length;
    }

    /// <summary>The number of bytes in the view.</summary>
    public int Length { get; }

    /// <summary>Byte <paramref name="index"/> of the view.</summary>
    public byte this[int index]
    {
        get => buffer[Offset(index)];
        set => buffer[Offset(index)] = value;
    }

    /// <summary>Writes <paramref name="value"/> to every byte.</summary>
    public void Fill(byte value) => Span.Fill(value);

    /// <summary>Reads the little-endian word whose low byte is at <paramref name="byteOffset"/>.</summary>
    public ushort ReadWord(int byteOffset) => LittleEndianBytes.Word(this[byteOffset], this[byteOffset + 1]);

    /// <summary>Writes the little-endian word whose low byte is at <paramref name="byteOffset"/>, low byte first.</summary>
    public void WriteWord(int byteOffset, ushort value)
    {
        this[byteOffset] = LittleEndianBytes.LowByte(value);
        this[byteOffset + 1] = LittleEndianBytes.HighByte(value);
    }

    /// <summary>The view's bytes.</summary>
    internal Span<byte> Span => buffer.AsSpan(start, Length);

    /// <summary>Checks a byte index and locates it in the buffer.</summary>
    private int Offset(int index) => (uint)index < (uint)Length
        ? start + index
        : throw new ArgumentOutOfRangeException(nameof(index), index, $"Staging view holds {Length} bytes.");
}
