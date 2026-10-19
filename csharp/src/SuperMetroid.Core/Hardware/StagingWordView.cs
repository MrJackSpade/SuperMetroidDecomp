namespace SuperMetroid.Core.Hardware;

/// <summary>A bounded table of little-endian words in a <see cref="VramStagingBuffer"/>, indexed by whole element.</summary>
public readonly struct StagingWordView
{
    /// <summary>The whole staging buffer.</summary>
    private readonly byte[] buffer;
    /// <summary>The first buffer byte of this table.</summary>
    private readonly int start;

    /// <summary>Binds a table of <paramref name="count"/> words starting <paramref name="start"/> bytes in.</summary>
    internal StagingWordView(byte[] buffer, int start, int count)
    {
        this.buffer = Ensure.NotNull(buffer);
        if (start < 0 || count < 0 || start + count * sizeof(ushort) > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"Table {start}+{count} words exceeds the {buffer.Length}-byte buffer.");
        this.start = start;
        Count = count;
    }

    /// <summary>The number of words in the table.</summary>
    public int Count { get; }

    /// <summary>Word <paramref name="index"/>: low byte first.</summary>
    public ushort this[int index]
    {
        get
        {
            int offset = Offset(index);
            return SnesLittleEndian.Word(buffer[offset], buffer[offset + 1]);
        }
        set
        {
            int offset = Offset(index);
            buffer[offset] = SnesLittleEndian.LowByte(value);
            buffer[offset + 1] = SnesLittleEndian.HighByte(value);
        }
    }

    /// <summary>Copies every word out, in element order.</summary>
    public ushort[] ToArray()
    {
        var values = new ushort[Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = this[index];
        return values;
    }

    /// <summary>Checks an element index and locates its low byte.</summary>
    private int Offset(int index) => (uint)index < (uint)Count
        ? start + index * sizeof(ushort)
        : throw new ArgumentOutOfRangeException(nameof(index), index, $"Staging table holds {Count} words.");
}
