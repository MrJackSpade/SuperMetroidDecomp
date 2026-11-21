using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// How the room's terrain stops an ordinary enemy projectile's axis move: a block beyond the room's grid is a wall, the
/// extension links are followed to the block they lead to, square slopes stop it only in their filled quadrants and align
/// it to the half block, non-square slopes stop only a vertical move in the projectile's own column against their height
/// profile, and the solid, door, spike, special, shootable and bombable blocks are walls. Air of every kind and grapple
/// blocks let it pass.
/// </summary>
internal sealed class EnemyProjectileTerrainCollision : EnemyProjectileBlockCollision
{
    /// <summary>The collision every enemy projectile without one of its own moves with.</summary>
    internal static EnemyProjectileTerrainCollision Rule { get; } = new();

    /// <summary>The terrain collision has no state; <see cref="Rule"/> is its one instance.</summary>
    private EnemyProjectileTerrainCollision()
    {
    }

    /// <inheritdoc/>
    protected override EnemyProjectileBlockReaction BlockReaction(
        RoomLevelData level,
        EnemyProjectileScannedMove move,
        Position16<RoomBlocks> block,
        bool firstOfScan)
    {
        if (!level.Size.Contains(block))
            return EnemyProjectileBlockReaction.StopsAtFace;

        // A projectile meets the block an extension link leads to. Air is open, while a door remains a wall to an enemy
        // projectile.
        if (EnemyTerrain.ResolveEnemyCollisionBlock(level, block) is not { } collisionBlock)
            return EnemyProjectileBlockReaction.StopsAtFace;

        EnemyProjectile projectile = move.Projectile;
        Axis axis = move.Scan.Edge;
        int targetEdge = move.LeadingEdge;
        RoomCollisionType type = collisionBlock.CollisionType;
        if (type == RoomCollisionType.Slope && !collisionBlock.Behavior.Slope.IsNonSquare)
        {
            // The horizontal scan tests only occupied eight-pixel quadrants touched by the
            // leading edge. Empty halves of square slopes are not solid walls.
            int perpendicularPosition = (axis == Axis.Horizontal) ? projectile.Position.Y.Whole : projectile.Position.X.Whole;
            int perpendicularRadius = (axis == Axis.Horizontal) ? projectile.Radius.Y : projectile.Radius.X;
            Position16<RoomPixels> blockOrigin = block.ToRoomPixels();
            int blockStart = (axis == Axis.Horizontal) ? blockOrigin.Y : blockOrigin.X;
            int firstPixel = Math.Max(blockStart, perpendicularPosition - perpendicularRadius);
            int lastPixel = Math.Min(RoomGridConversions.BlockEnd(blockStart), perpendicularPosition + perpendicularRadius - 1);
            for (int half = RoomGridConversions.HalfBlockOf(firstPixel); half <= RoomGridConversions.HalfBlockOf(lastPixel); half++)
            {
                SquareSlopeQuadrant quadrant = axis == Axis.Horizontal
                    ? SquareSlopeQuadrant.Showing(collisionBlock.Behavior, SquareSlopeQuadrant.ColumnFor(targetEdge), SquareSlopeQuadrant.RowOfHalf(half))
                    : SquareSlopeQuadrant.Showing(collisionBlock.Behavior, SquareSlopeQuadrant.ColumnOfHalf(half), SquareSlopeQuadrant.RowFor(targetEdge));
                if (!SquareSlopes.IsSolid(collisionBlock.Behavior, quadrant))
                    continue;

                // The square-slope reaction places the projectile at an eight-pixel
                // boundary before the collision's full-block clamp.
                int radius = (axis == Axis.Horizontal) ? projectile.Radius.X : projectile.Radius.Y;
                return new EnemyProjectileBlockReaction.StopsAligned(move.MovingNegative
                    ? TerrainScanEdges.Edge(RoomGridConversions.HalfBlockEnd(targetEdge) + radius + 1)
                    : TerrainScanEdges.Edge(RoomGridConversions.HalfBlockStart(targetEdge) - radius));
            }
            return EnemyProjectileBlockReaction.Passes;
        }

        if (type == RoomCollisionType.Slope && collisionBlock.Behavior.Slope.IsNonSquare)
        {
            // A horizontal move passes a non-square slope; the vertical move meets its height profile.
            if (axis == Axis.Horizontal)
                return EnemyProjectileBlockReaction.Passes;

            // Vertical projectile collision uses the same 16-sample slope
            // profiles as Samus's. Only the column containing the projectile center owns
            // non-square geometry; neighboring columns touched by its radius do not turn
            // the slope into a full-height wall.
            if (block.X != RoomGridConversions.PixelToBlock(projectile.Position.X.Whole))
                return EnemyProjectileBlockReaction.Passes;

            bool movingUp = move.MovingNegative;
            if (movingUp != collisionBlock.Behavior.Slope.FlipsVertically)
                return EnemyProjectileBlockReaction.Passes;

            int height = EnemyTerrain.ReadNonSquareSlopeHeight(
                collisionBlock.Behavior,
                RoomGridConversions.OffsetWithinBlock(projectile.Position.X.Whole, mirrored: collisionBlock.Behavior.Slope.FlipsHorizontally));
            int edgeWithinBlock = RoomGridConversions.OffsetWithinBlock(targetEdge, mirrored: movingUp);
            int adjustment = height - edgeWithinBlock - 1;
            if (movingUp)
            {
                if (adjustment > 0)
                    return EnemyProjectileBlockReaction.Passes;
            }
            else if (height - edgeWithinBlock != 1 && adjustment >= 0)
            {
                return EnemyProjectileBlockReaction.Passes;
            }

            int blockTop = block.ToRoomPixels().Y;
            return new EnemyProjectileBlockReaction.StopsAligned(movingUp
                ? TerrainScanEdges.Edge(RoomGridConversions.BlockEnd(blockTop) + 1 - height + projectile.Radius.Y)
                : TerrainScanEdges.Edge(blockTop + height - projectile.Radius.Y));
        }

        return IsWall(type) ? EnemyProjectileBlockReaction.StopsAtFace : EnemyProjectileBlockReaction.Passes;
    }

    /// <summary>
    /// The block reactions an enemy projectile has whatever
    /// the block's shape. Slopes and extensions are resolved first.
    /// Spike blocks stop projectiles; grapple blocks do not.
    /// </summary>
    private static bool IsWall(RoomCollisionType type) => type is
        RoomCollisionType.Slope or
        RoomCollisionType.HorizontalExtension or
        RoomCollisionType.SolidBlock or
        RoomCollisionType.DoorBlock or
        RoomCollisionType.SpikeBlock or
        RoomCollisionType.SpecialBlock or
        RoomCollisionType.ShootableBlock or
        RoomCollisionType.VerticalExtension or
        RoomCollisionType.BombableBlock;
}
