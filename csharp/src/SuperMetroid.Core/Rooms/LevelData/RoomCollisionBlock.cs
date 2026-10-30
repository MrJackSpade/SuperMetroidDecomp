namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>One block of a room's terrain grid as collision reads it: where it is, its terrain and its behaviour.</summary>
/// <param name="Index">Row-major room-block index; an edge-solid result for a probe outside the room uses minus one.</param>
/// <param name="Terrain">What the block shows and how it collides.</param>
/// <param name="Behavior">
/// The block's behaviour parameter. Its collision kind determines which of its named views applies; it is not a universal
/// terrain identifier.
/// </param>
public readonly record struct RoomCollisionBlock(int Index, RoomTerrainBlock Terrain, RoomBlockBehavior Behavior)
{
    /// <summary>How the block collides.</summary>
    public RoomCollisionType CollisionType => Terrain.Collision;
}
