namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Immutable physical level copied by the SR388 discovery setup. This 32-by-12
/// source is distinct from the visible BG page; the final four 32-block rows of
/// the declared room retain their native zero initialization.
/// </summary>
internal static class IntroBabyDiscoveryCollisionDefinitions
{
    /// <summary>$8C:C083, first byte copied by the discovery room setup.</summary>
    internal const int SourceAddress = 0x8cc083;
    /// <summary>Thirty-two physical foreground words per source row.</summary>
    internal const int Columns = 32;
    /// <summary>Twelve ROM-authored rows; the room itself is sixteen rows tall.</summary>
    internal const int SourceRows = 12;
    /// <summary>Ten zero rows precede the two authored nonzero rows.</summary>
    internal const int ZeroRows = 10;
    /// <summary>The eleventh source row contains word $1000 in every column.</summary>
    internal const ushort PenultimateRowWord = 0x1000;
    /// <summary>The final source row contains word $8000 in every column.</summary>
    internal const ushort FinalRowWord = 0x8000;
    /// <summary>The native setup copies exactly $300 bytes.</summary>
    internal const int SourceByteCount = Columns * SourceRows * sizeof(ushort);

    private static readonly byte[] sourceBytes = BuildSourceBytes();

    internal static ReadOnlySpan<byte> SourceBytes => sourceBytes;

    /// <summary>Return a fresh copy for the room's mutable foreground allocation.</summary>
    internal static byte[] CopySourceBytes() => (byte[])sourceBytes.Clone();

    private static byte[] BuildSourceBytes()
    {
        var bytes = new byte[SourceByteCount];
        for (int row = ZeroRows; row < SourceRows; row++)
        {
            ushort word = row == SourceRows - 1 ? FinalRowWord : PenultimateRowWord;
            for (int column = 0; column < Columns; column++)
            {
                int offset = (row * Columns + column) * sizeof(ushort);
                bytes[offset] = (byte)word;
                bytes[offset + 1] = (byte)(word >> 8);
            }
        }
        return bytes;
    }
}
