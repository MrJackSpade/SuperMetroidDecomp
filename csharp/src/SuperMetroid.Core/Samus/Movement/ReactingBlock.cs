using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// A met block of a room's terrain (#2553), whose reaction reaches the room's objects: the room's terrain and the block
/// the met cell stands for. Only <see cref="RoomTerrain"/> yields one.
/// </summary>
/// <param name="Room">The room's terrain, whose room objects the block's reaction reaches.</param>
/// <param name="Standing">The block the met cell stands for and how it collides.</param>
internal readonly partial record struct ReactingBlock(RoomTerrain Room, StandingBlock Standing)
{
    /// <summary>The block the met cell stands for (<see cref="StandingBlock.Block"/>).</summary>
    internal RoomCollisionBlock Block => Standing.Block;

    /// <summary>How the block collides (<see cref="StandingBlock.CollisionType"/>).</summary>
    internal StandingCollisionType CollisionType => Standing.CollisionType;
}
