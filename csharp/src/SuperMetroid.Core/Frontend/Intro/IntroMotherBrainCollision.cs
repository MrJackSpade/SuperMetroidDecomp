using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Frontend.Intro.IntroMotherBrainCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro Mother Brain room's collision (<see cref="IntroMotherBrainCollisionDefinitions"/>): the ceiling and
/// floor, the left wall's contour, the hanging formation, the pillar and the ledge.
/// </summary>
internal static class IntroMotherBrainCollision
{
    /// <summary>How the block at <paramref name="column"/> of <paramref name="row"/> collides.</summary>
    private static RoomCollisionType Block(int row, int column)
    {
        if (row is CeilingRow or FloorRow) return Solid;
        if (row > CeilingRow && row < FloorRow && column < LeftContourWidths[row - FormationTipRow])
            return Solid;
        int formationHalfWidth = row - FormationTipRow;
        if (formationHalfWidth is 0 or 1 && Math.Abs(column - CentralColumn) <= formationHalfWidth)
            return Solid;
        bool pillar = column == CentralColumn && row >= PlatformRow && row < FloorRow;
        bool ledge = column >= RightPlatformColumn && row >= PlatformRow && row <= PlatformRow + 1;
        if (pillar || ledge) return row == PlatformRow ? PlatformTop : Solid;
        // A solid block stands on the floor against the pillar's right side.
        if (row == FloorRow - 1 && column == CentralColumn + 1) return Solid;
        return Empty;
    }

    /// <summary>How each block of the flashback room collides, row-major and freshly for each flashback.</summary>
    internal static RoomCollisionType[] CreateCollision()
    {
        var foreground = new RoomCollisionType[Columns * RoomRows];
        for (int row = 0; row < RoomRows; row++)
            for (int column = 0; column < Columns; column++)
                foreground[row * Columns + column] = Block(row, column);
        return foreground;
    }
}
