using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// A block Samus's body, her shots, missiles or bombs meet in a terrain (#2553): the block the cell stands for and how it
/// collides, and, when the block reaches a room's objects, the reaction it carries. A plain terrain's blocks carry none.
/// </summary>
/// <param name="Standing">The block the met cell stands for and how it collides.</param>
/// <param name="Reacting">The block as a reaction reaches it, which only a room's terrain yields; none on a plain terrain.</param>
internal readonly record struct MetBlock(StandingBlock Standing, ReactingBlock? Reacting)
{
    /// <summary>The block the met cell stands for (<see cref="StandingBlock.Block"/>).</summary>
    internal RoomCollisionBlock Block => Standing.Block;

    /// <summary>How the block collides (<see cref="StandingBlock.CollisionType"/>).</summary>
    internal StandingCollisionType CollisionType => Standing.CollisionType;
}
