namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Terrain blocks the game writes by name.</summary>
public static partial class RoomTerrainBlocks
{
    /// <summary>
    /// A plain solid block with no picture, as the room never shows it (<see cref="RoomBlockAppearance.BeyondRoom"/>, #2100):
    /// what a probe outside a room's width or height reads, and what fills a room's level space past its data
    /// (<see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/>).
    /// </summary>
    public static readonly RoomTerrainBlock EdgeSolid = new(RoomBlockAppearance.BeyondRoom, RoomCollisionType.SolidBlock);
}
