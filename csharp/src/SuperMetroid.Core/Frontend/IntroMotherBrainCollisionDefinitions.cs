namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed physical foreground copied from $8C:BEC3 for the Mother Brain intro flashback.
/// This is not the visible BG page. Each character represents one 16-bit level word:
/// '.' = $0000, '#' = $8000, and 'x' = $1000. The two remaining 16-block rows in the
/// room are zero-initialized by the existing room constructor.
/// </summary>
internal static class IntroMotherBrainCollisionDefinitions
{
    /// <summary>Sixteen level blocks per physical source row.</summary>
    internal const int Columns = 16;
    /// <summary>Fourteen ROM-authored rows; the native copy is 448 bytes.</summary>
    internal const int SourceRows = 14;

    private static readonly string[] sourceRows =
    [
        "................",
        "################",
        "######...#......",
        "######..###.....",
        "#####...........",
        "######..........",
        "####............",
        "####............",
        "####.....x..xxxx",
        "######...#..####",
        "#######..#......",
        "#######..#......",
        "#######..##.....",
        "################",
    ];

    private static readonly byte[] sourceBytes = BuildSourceBytes();

    internal static ReadOnlySpan<byte> SourceBytes => sourceBytes;

    /// <summary>Each flashback receives its own mutable debugger-visible native copy.</summary>
    internal static byte[] CopySourceBytes() => (byte[])sourceBytes.Clone();

    private static byte[] BuildSourceBytes()
    {
        if (sourceRows.Length != SourceRows)
            throw new InvalidDataException("Mother Brain flashback collision row count changed.");
        var bytes = new byte[Columns * SourceRows * sizeof(ushort)];
        for (int row = 0; row < SourceRows; row++)
        {
            if (sourceRows[row].Length != Columns)
                throw new InvalidDataException($"Mother Brain flashback collision row {row} is not 16 blocks wide.");
            for (int column = 0; column < Columns; column++)
            {
                ushort word = sourceRows[row][column] switch
                {
                    '.' => 0x0000,
                    '#' => 0x8000,
                    'x' => 0x1000,
                    char unknown => throw new InvalidDataException(
                        $"Mother Brain flashback collision symbol {unknown} is invalid."),
                };
                int offset = (row * Columns + column) * sizeof(ushort);
                bytes[offset] = (byte)word;
                bytes[offset + 1] = (byte)(word >> 8);
            }
        }
        return bytes;
    }
}
