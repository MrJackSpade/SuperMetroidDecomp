using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.OldTourianEscapeShaftWallPlmDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The open-wall draw of the old Tourian escape shaft.</summary>
internal static class OldTourianEscapeShaftWallDraws
{
    /// <summary>
    /// $84:9283: a vertical run of three $00FF air words at the PLM, then another one block to
    /// its right.
    /// </summary>
    internal static bool TryGetDraw(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (request != OpenWallDraw)
        {
            draw = default;
            return false;
        }
        RoomTerrainBlock[] column = [RoomTerrainBlocks.BlankAir, RoomTerrainBlocks.BlankAir, RoomTerrainBlocks.BlankAir];
        draw = new(request, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(PlmRunDirection.Vertical, column, new(1, 0)),
            new(PlmRunDirection.Vertical, column, new(0, 0)),
        });
        return true;
    }
}
