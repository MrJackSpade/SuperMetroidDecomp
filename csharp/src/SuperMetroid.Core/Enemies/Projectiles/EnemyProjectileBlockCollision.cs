using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// How an enemy projectile's axis move (<see cref="EnemyProjectileEngine"/>) reacts to the blocks its block scan tests
/// (<see cref="EnemyProjectileBlockScan"/>): which block stops the move, and whether a stopping block aligns the
/// projectile somewhere other than against that block's face. Most enemy projectiles collide with the room's terrain
/// (<see cref="EnemyProjectileTerrainCollision"/>); a kind with a collision of its own passes it to the same move.
/// </summary>
internal abstract class EnemyProjectileBlockCollision
{
    /// <summary>
    /// Whether a block of <paramref name="move"/>'s scan stops the move: the scan's blocks are tested in order, and the first
    /// that stops the move ends the scan.
    /// </summary>
    /// <param name="level">The room's terrain.</param>
    /// <param name="move">The move whose scan is tested.</param>
    /// <param name="alignedPosition">
    /// Where the stopping block places the projectile on the move's axis, or null when the move stops against the block's
    /// face.
    /// </param>
    internal bool Stops(RoomLevelData level, EnemyProjectileScannedMove move, out int? alignedPosition)
    {
        for (int tested = 0; tested <= move.Scan.ExtraBlocks; tested++)
        {
            if (BlockStops(level, move, move.Scan.Block(tested, level.Size.X), firstOfScan: tested == 0, out alignedPosition))
                return true;
        }

        alignedPosition = null;
        return false;
    }

    /// <summary>Whether one block of the scan stops the move.</summary>
    /// <param name="level">The room's terrain.</param>
    /// <param name="move">The move whose scan is tested.</param>
    /// <param name="block">The block tested, in the room's columns; a row past the room's height is a block beyond its grid.</param>
    /// <param name="firstOfScan">Whether it is the scan's first block.</param>
    /// <param name="alignedPosition">Where a stopping block places the projectile on the move's axis, or null for its face.</param>
    protected abstract bool BlockStops(
        RoomLevelData level,
        EnemyProjectileScannedMove move,
        Position16<RoomBlocks> block,
        bool firstOfScan,
        out int? alignedPosition);
}
