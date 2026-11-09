using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The terrain writes room-object families share: drawing a block and replacing its collision kind and behaviour.</summary>
internal static class RoomObjectLevelWrites
{
    /// <summary>Writes <paramref name="terrain"/> at a block, which shows the terrain's own picture.</summary>
    internal static void DrawTerrain(RoomLevelData level, int blockIndex, RoomTerrainBlock terrain) =>
        level.SetObjectTerrain(blockIndex, terrain);

    /// <summary>
    /// Replaces a block's collision kind and behaviour, keeping its appearance, as a room object converts the block it sits on.
    /// </summary>
    internal static void WriteObjectCollisionAndBehavior(RoomLevelData level, int blockIndex, RoomCollisionType collision, RoomBlockBehavior behavior)
    {
        RoomTerrainBlock original = level.GetObjectCollisionBlockByIndex(blockIndex).Terrain;
        level.SetObjectTerrain(blockIndex, original.WithCollision(collision));
        level.SetObjectBehavior(blockIndex, behavior);
    }
}
