namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The word a scanline window table holds for one screen line (#1422): the line's left screen column in its low eight
/// bits and its right column in its high eight bits. The eye-beam, rainbow-beam and suit-pickup rasterizations build,
/// compare and patch these words column by column, and the renderer reads each line's span back out of them.
/// </summary>
[EncodingOwner("scanline window span word")]
public static class ScanlineSpanWord
{
    /// <summary>The bits of the left column, below the right column.</summary>
    private const int LeftColumnBits = 8;

    /// <summary>The span word of a line from column <paramref name="left"/> through column <paramref name="right"/>.</summary>
    public static ushort Of(byte left, byte right) => (ushort)(left | right << LeftColumnBits);

    /// <summary>The left screen column of a span word.</summary>
    public static byte Left(ushort word) => unchecked((byte)word);

    /// <summary>The right screen column of a span word.</summary>
    public static byte Right(ushort word) => unchecked((byte)(word >> LeftColumnBits));

    /// <summary>The right screen column of a span word held as an int, 0 through 65535; other values are rejected.</summary>
    public static byte Right(int word) => Right(checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, word)));
}
