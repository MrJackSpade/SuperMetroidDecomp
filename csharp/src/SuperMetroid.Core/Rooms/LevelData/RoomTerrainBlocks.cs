namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Terrain blocks the game writes by name.</summary>
public static class RoomTerrainBlocks
{
    /// <summary>
    /// A plain solid block showing block zero, unmirrored: what a probe outside a room's width or height reads, and what
    /// fills a room's level space past its data (<see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/>).
    /// </summary>
    public static readonly RoomTerrainBlock EdgeSolid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
}
