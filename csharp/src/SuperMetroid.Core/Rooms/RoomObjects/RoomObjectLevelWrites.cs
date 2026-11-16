using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The terrain writes room-object families share: drawing a block and replacing its collision kind and behaviour.</summary>
internal static class RoomObjectLevelWrites
{
    /// <summary>Writes <paramref name="terrain"/> at a block, which shows the terrain's own picture.</summary>
    internal static void DrawTerrain(RoomLevelData level, Position16<RoomBlocks> block, RoomTerrainBlock terrain) =>
        level.SetObjectTerrain(block, terrain);

    /// <summary>
    /// Makes a block of the grid a door block leading through <paramref name="door"/>, keeping its appearance, as Mother
    /// Brain's escape door is set up (#2090). A door block reads no behaviour, so it holds none.
    /// </summary>
    internal static void WriteDoorBlock(RoomLevelData level, Position16<RoomBlocks> block, DoorDefinition door)
    {
        WriteObjectCollisionAndBehavior(level, block, RoomCollisionType.DoorBlock, RoomBlockBehavior.None);
        level.SetDoorExit(block, DoorBlockExit.Physical(door));
    }

    /// <summary>
    /// Replaces a block's collision kind and behaviour, keeping its appearance, as a room object converts the block it sits on.
    /// </summary>
    internal static void WriteObjectCollisionAndBehavior(RoomLevelData level, Position16<RoomBlocks> block, RoomCollisionType collision, RoomBlockBehavior behavior)
    {
        RoomTerrainBlock original = level.GetObjectCollisionBlock(block).Terrain;
        level.SetObjectTerrain(block, original.WithCollision(collision));
        level.SetObjectBehavior(block, behavior);
    }
}
