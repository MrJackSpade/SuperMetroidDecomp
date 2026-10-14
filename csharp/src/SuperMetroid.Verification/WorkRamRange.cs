/// <summary>One contiguous span of native work RAM, as a byte offset into the 128 KiB <c>$7E:0000-$7F:FFFF</c> image.</summary>
/// <param name="Start">Offset of the first byte.</param>
/// <param name="Length">Number of bytes; positive.</param>
internal readonly record struct WorkRamRange(int Start, int Length)
{
    /// <summary>The offset just past the range.</summary>
    public int End => Start + Length;

    /// <summary>The range holding <paramref name="count"/> consecutive little-endian words from <paramref name="start"/>.</summary>
    /// <param name="start">Offset of the first word.</param>
    /// <param name="count">Number of words.</param>
    public static WorkRamRange Words(int start, int count = 1) => new(start, count * sizeof(ushort));

    /// <summary>The range from <paramref name="start"/> up to, but excluding, <paramref name="end"/>.</summary>
    /// <param name="start">Offset of the first byte.</param>
    /// <param name="end">Offset just past the last byte.</param>
    public static WorkRamRange Between(int start, int end) => new(start, end - start);
}
