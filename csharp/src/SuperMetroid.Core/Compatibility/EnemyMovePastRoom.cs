namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An enemy's terrain move whose scan runs past the room's last row meets the terrain beyond the room (#1851): the
/// level data that followed the room's grid, then solid blocks (<see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/>),
/// rather than stopping at the room's edge. A block there whose reaction would read its behaviour (a slope, an extension
/// link or a spike block) is refused, since the behaviour beyond the room is whatever earlier rooms left.
/// </summary>
/// <remarks>
/// Consulted by every enemy's horizontal and vertical terrain move during the enemy pass, for each block its scan or an
/// extension link reaches past the room's grid. Evidence: the original movers (<c>$A0:C6AD</c>, <c>$A0:C788</c>) index
/// the room's level space without a bound on its height and react to whatever block they find there; the room's level
/// space past the grid holds what <see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/> describes, and the behaviour plane
/// past it is not cleared between rooms.
/// </remarks>
[CompatibilityService("an enemy's terrain move whose scan runs past the room's last row meets the terrain beyond the room instead of stopping at its edge")]
public static class EnemyMovePastRoom
{
    /// <summary>The block an enemy's terrain move meets at <paramref name="block"/>, past the room's grid.</summary>
    /// <typeparam name="TBlock">The room's terrain block with its behaviour.</typeparam>
    /// <param name="block">The block the move reached, in the room's columns, past the room's last row.</param>
    /// <param name="beyondRoom">Reads the room's terrain beyond its grid.</param>
    /// <param name="reactionReadsBehaviour">Whether the move's reaction to a block reads the block's behaviour.</param>
    /// <exception cref="NotSupportedException">The block's reaction reads its behaviour, which beyond the room is unbounded.</exception>
    public static TBlock Meets<TBlock>(Position16<RoomBlocks> block, Func<Position16<RoomBlocks>, TBlock> beyondRoom,
        Func<TBlock, bool> reactionReadsBehaviour)
    {
        Ensure.NotNull(beyondRoom);
        Ensure.NotNull(reactionReadsBehaviour);
        TBlock met = beyondRoom(block);
        if (reactionReadsBehaviour(met))
        {
            throw new NotSupportedException(
                $"An enemy's terrain move reached {met} at block {block}, past the room's grid, whose reaction reads " +
                "its behaviour; the behaviour there is unbounded.");
        }
        return met;
    }
}
