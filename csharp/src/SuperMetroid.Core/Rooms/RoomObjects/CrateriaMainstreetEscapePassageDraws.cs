using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.CrateriaMainstreetEscapePassageObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The open-passage draw of Crateria's main street escape.</summary>
internal static class CrateriaMainstreetEscapePassageDraws
{
    /// <summary>Recognizes the escape passage draw pointer and supplies its two adjacent air blocks.</summary>
    internal static bool TryGetDraw(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
    {
        if (request != ClearPairDraw)
        {
            draw = default;
            return false;
        }
        draw = new(request, new RoomObjectShotBlockDrawDefinitions.Run[]
        {
            new(RoomObjectRunDirection.Horizontal, new[] { RoomTerrainBlocks.BlankAir, RoomTerrainBlocks.BlankAir }, new(0, 0)),
        });
        return true;
    }
}
