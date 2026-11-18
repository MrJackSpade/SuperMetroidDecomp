using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The terrain writes room-object families share: drawing a block and making a block a door block.</summary>
internal static class RoomObjectLevelWrites
{
    /// <summary>Writes <paramref name="terrain"/> at a block, which shows the terrain's own picture.</summary>
    internal static void DrawTerrain(RoomLevelData level, Position16<RoomBlocks> block, RoomTerrainBlock terrain) =>
        level.SetObjectTerrain(block, terrain);

    /// <summary>
    /// Makes a block of the grid a door block leading through <paramref name="door"/>, as Mother Brain's escape door is set
    /// up (#2090): only its collision kind and behaviour change, so it keeps its picture and what it shows (#2496). A door
    /// block reads no behaviour, so it holds none.
    /// </summary>
    internal static void WriteDoorBlock(RoomLevelData level, Position16<RoomBlocks> block, DoorDefinition door)
    {
        level.SetObjectCollisionAndBehavior(block, RoomCollisionType.DoorBlock, RoomBlockBehavior.None);
        level.SetDoorExit(block, DoorBlockExit.Physical(door));
    }
}
