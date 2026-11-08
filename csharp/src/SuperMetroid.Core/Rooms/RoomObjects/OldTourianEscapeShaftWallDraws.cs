using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.OldTourianEscapeShaftWallObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The open-wall draw of the old Tourian escape shaft.</summary>
internal static class OldTourianEscapeShaftWallDraws
{
    /// <summary>
    /// A vertical run of three air blocks at the PLM, then another one block to
    /// its right.
    /// </summary>
    internal static bool TryGetDraw(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        if (request != OpenWallDraw)
        {
            draw = default;
            return false;
        }
        RoomTerrainBlock[] column = [RoomTerrainBlocks.BlankAir, RoomTerrainBlocks.BlankAir, RoomTerrainBlocks.BlankAir];
        draw = new(request, new RoomObjectShotBlockDrawDefinitions.Run[]
        {
            new(RoomObjectRunDirection.Vertical, column, new(0, 0)),
            new(RoomObjectRunDirection.Vertical, column, new(1, 0)),
        });
        return true;
    }
}
