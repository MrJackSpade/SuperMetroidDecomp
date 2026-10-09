namespace SuperMetroid.Core.Frontend;

/// <summary>Physical foreground at8C:BEC3. Regular regions are constructed geometrically;
/// the left silhouette is authored collision-mask data reviewed under1165.</summary>
internal static class IntroMotherBrainCollisionDefinitions
{
    /// <summary>Sixteen level blocks per physical source row.</summary>
    internal const int Columns = 16;
    /// <summary>Fourteen imported rows; the native copy is448 bytes.</summary>
    internal const int SourceRows = 14;
    /// <summary>Block type8,$8000, solid foreground with zero tile/flip bits.</summary>
    private const ushort Solid = 0x8000;
    /// <summary>Block type1,$1000, top collision cells with native zero BTS.</summary>
    private const ushort PlatformTop = 0x1000;
    /// <summary>First and last nonzero full-width source rows.</summary>
    private const int CeilingRow = 1, FloorRow = 13;
    /// <summary>Upper central formation spans rows2/3,centered on column9.</summary>
    private const int UpperTipRow = 2, CentralColumn = 9;
    /// <summary>Lower central and right platforms start at row8;right platform starts column12.</summary>
    private const int PlatformRow = 8, RightPlatformColumn = 12;

    /// <summary>Authored collision-mask widths on rows2..12: tank interior,upper notch and pipe base.
    /// These select blocking geometry independently of visible pixels; a fitted curve would
    /// restate or alter the chosen level mask. See introMotherBrainCollisionContourDisposition.</summary>
    private static ReadOnlySpan<byte> LeftContourWidths => [6, 6, 5, 6, 4, 4, 4, 6, 7, 7, 7];

    private static ushort Block(int row, int column)
    {
        if (row is CeilingRow or FloorRow) return Solid;
        if (row > CeilingRow && row < FloorRow && column < LeftContourWidths[row - UpperTipRow])
            return Solid;
        int upperRadius = row - UpperTipRow;
        if (upperRadius is 0 or 1 && Math.Abs(column - CentralColumn) <= upperRadius)
            return Solid;
        bool centralPlatform = column == CentralColumn && row >= PlatformRow && row < FloorRow;
        bool rightPlatform = column >= RightPlatformColumn && row >= PlatformRow && row <= PlatformRow + 1;
        if (centralPlatform || rightPlatform) return row == PlatformRow ? PlatformTop : Solid;
        if (row == FloorRow - 1 && column == CentralColumn + 1) return Solid;
        return 0;
    }

    /// <summary>Construct the exact debugger-visible native bytes independently for each flashback.</summary>
    internal static byte[] CopySourceBytes()
    {
        var bytes = new byte[Columns * SourceRows * sizeof(ushort)];
        for (int row = 0; row < SourceRows; row++)
            for (int column = 0; column < Columns; column++)
            {
                ushort word = Block(row, column);
                int offset = (row * Columns + column) * sizeof(ushort);
                bytes[offset] = (byte)word;
                bytes[offset + 1] = (byte)(word >> 8);
            }
        return bytes;
    }
}
