using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Frontend.Intro.IntroMotherBrainCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro Mother Brain room's collision (<see cref="IntroMotherBrainCollisionDefinitions"/>): the authored left
/// contour with the ceiling, floor, central formation and platforms.
/// </summary>
internal static class IntroMotherBrainCollision
{
    /// <summary>Combines the authored left collision contour with geometric ceiling, floor, center, and right-platform blocks.</summary>
    private static RoomTerrainBlock Block(int row, int column)
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
        return Empty;
    }

    /// <summary>
    /// Builds the flashback room's terrain, row-major and freshly for each flashback: the authored source rows, then
    /// empty rows to the room's full height.
    /// </summary>
    internal static RoomTerrainBlock[] CreateForeground()
    {
        var foreground = new RoomTerrainBlock[Columns * RoomRows];
        foreground.AsSpan().Fill(Empty);
        for (int row = 0; row < SourceRows; row++)
            for (int column = 0; column < Columns; column++)
                foreground[row * Columns + column] = Block(row, column);
        return foreground;
    }
}
