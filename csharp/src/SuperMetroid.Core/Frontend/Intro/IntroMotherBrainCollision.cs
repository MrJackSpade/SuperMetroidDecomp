using System.Buffers.Binary;
using static SuperMetroid.Core.Frontend.Intro.IntroMotherBrainCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro Mother Brain room's collision (<see cref="IntroMotherBrainCollisionDefinitions"/>): the authored left
/// contour with the ceiling, floor, central formation and platforms.
/// </summary>
internal static class IntroMotherBrainCollision
{
    /// <summary>Combines the authored left collision contour with geometric ceiling, floor, center, and right-platform blocks.</summary>
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
                int word = Block(row, column);
                int offset = (row * Columns + column) * sizeof(ushort);
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)word));
            }
        return bytes;
    }
}
