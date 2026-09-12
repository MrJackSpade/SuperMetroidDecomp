using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared bank-$A0 enemy movement primitives. Enemy AI supplies a signed 16.16 displacement;
/// these helpers retain the cartridge's scan order, BTS extension dispatch, square-slope
/// half selection, non-square vertical geometry, and collision-edge alignment.
/// </summary>
public sealed partial class RoomEnemySystem
{
    // $A0:C435. Bit seven of an entry is the carry result returned by the native square-
    // slope reactor. The low two bits are unused by this host translation but are retained
    // literally so a debugger can compare the lookup with the ROM table.
    private static readonly byte[] SquareSlopeCollisionTable =
    [
        0x00, 0x01, 0x82, 0x83,
        0x00, 0x81, 0x02, 0x83,
        0x00, 0x01, 0x02, 0x83,
        0x00, 0x81, 0x82, 0x83,
        0x80, 0x81, 0x82, 0x83,
    ];

    /// <summary>Ports <c>MoveEnemyRightBy_14_12_IgnoreSlopes</c> at $A0:C6AB.</summary>
    private static bool MoveEnemyHorizontallyIgnoringNonSquareSlopes(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement) =>
        MoveEnemyHorizontally(
            level,
            slot,
            displacement,
            treatNonSquareSlopesAsWalls: false);

    /// <summary>
    /// Ports <c>MoveEnemyRightBy_14_12_TreatSlopesAsWalls</c> at $A0:C69D. Square slopes
    /// retain their native half-tile geometry; only non-square slopes consume direct-page
    /// flag $4000 and become solid walls.
    /// </summary>
    private static bool MoveEnemyHorizontallyTreatingSlopesAsWalls(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement) =>
        MoveEnemyHorizontally(
            level,
            slot,
            displacement,
            treatNonSquareSlopesAsWalls: true);

    /// <summary>
    /// Shared body of the three native horizontal enemy movers. The remaining $8000
    /// process-slopes mode will reuse this seam when its first retail caller is translated.
    /// </summary>
    private static bool MoveEnemyHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement,
        bool treatNonSquareSlopesAsWalls)
    {
        if (displacement == 0)
            return false;

        uint oldPosition = ((uint)slot.XPosition << 16) | slot.XSubposition;
        uint newPosition = unchecked(oldPosition + (uint)displacement);
        ushort targetCenter = unchecked((ushort)(newPosition >> 16));
        bool movingLeft = displacement < 0;
        ushort targetEdge = movingLeft
            ? unchecked((ushort)(targetCenter - slot.XRadius))
            : unchecked((ushort)(targetCenter + slot.XRadius - 1));

        ushort topPixel = unchecked((ushort)(slot.YPosition - slot.YRadius));
        ushort bottomPixel = unchecked((ushort)(slot.YPosition + slot.YRadius - 1));
        int spanMinusOne = unchecked((ushort)(bottomPixel - (topPixel & 0xfff0))) >> 4;
        int column = targetEdge >> 4;
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int row = (topPixel >> 4) + scanIndex;
            int remaining = spanMinusOne - scanIndex;
            if (EnemyHorizontalProbeIsSolid(
                    level,
                    slot,
                    column,
                    row,
                    targetEdge,
                    remaining,
                    spanMinusOne,
                    treatNonSquareSlopesAsWalls))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.XPosition = targetCenter;
            slot.XSubposition = unchecked((ushort)newPosition);
            return false;
        }

        // $A0:C755/C76D compare against the pre-move position before accepting the aligned
        // center. That prevents a large corrupt displacement from snapping through a wall.
        if (movingLeft)
        {
            ushort aligned = unchecked((ushort)((targetEdge | 0x000f) + slot.XRadius + 1));
            if (aligned <= slot.XPosition)
                slot.XPosition = aligned;
            slot.XSubposition = 0;
        }
        else
        {
            ushort aligned = unchecked((ushort)((targetEdge & 0xfff0) - slot.XRadius));
            if (aligned >= slot.XPosition)
                slot.XPosition = aligned;
            slot.XSubposition = 0xffff;
        }
        return true;
    }

    /// <summary>Ports <c>MoveEnemyDownBy_14_12</c> at $A0:C786.</summary>
    private bool MoveEnemyVertically(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement)
    {
        if (displacement == 0)
            return false;

        uint oldPosition = ((uint)slot.YPosition << 16) | slot.YSubposition;
        uint newPosition = unchecked(oldPosition + (uint)displacement);
        ushort targetCenter = unchecked((ushort)(newPosition >> 16));
        bool movingUp = displacement < 0;
        ushort targetEdge = movingUp
            ? unchecked((ushort)(targetCenter - slot.YRadius))
            : unchecked((ushort)(targetCenter + slot.YRadius - 1));

        ushort leftPixel = unchecked((ushort)(slot.XPosition - slot.XRadius));
        ushort rightPixel = unchecked((ushort)(slot.XPosition + slot.XRadius - 1));
        int spanMinusOne = unchecked((ushort)(rightPixel - (leftPixel & 0xfff0))) >> 4;
        int row = targetEdge >> 4;
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int column = (leftPixel >> 4) + scanIndex;
            int remaining = spanMinusOne - scanIndex;
            if (EnemyVerticalProbeIsSolid(
                    level,
                    slot,
                    column,
                    row,
                    targetCenter,
                    targetEdge,
                    movingUp,
                    remaining,
                    spanMinusOne))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.YPosition = targetCenter;
            slot.YSubposition = unchecked((ushort)newPosition);
            return false;
        }

        // A non-square slope reactor may already have placed the center more precisely.
        // The native outer helper performs this conditional coarse alignment afterward.
        if (movingUp)
        {
            ushort aligned = unchecked((ushort)((targetEdge | 0x000f) + slot.YRadius + 1));
            if (aligned <= slot.YPosition)
                slot.YPosition = aligned;
            slot.YSubposition = 0;
        }
        else
        {
            ushort aligned = unchecked((ushort)((targetEdge & 0xfff0) - slot.YRadius));
            if (aligned >= slot.YPosition)
                slot.YPosition = aligned;
            slot.YSubposition = 0xffff;
        }
        return true;
    }

    /// <summary>
    /// Ports the geometry-only portion of $A0:BF8A used by Skree/Metaree. Unlike the common
    /// collision dispatcher, this routine deliberately tests only level-word bit 15.
    /// </summary>
    private static bool EnemyHasSolidHighBitAhead(
        RoomLevelData level,
        RoomEnemySlot slot,
        int unsignedDistance,
        bool movingDown)
    {
        uint position = ((uint)slot.YPosition << 16) | slot.YSubposition;
        uint target = movingDown
            ? unchecked(position + (uint)unsignedDistance)
            : unchecked(position - (uint)unsignedDistance);
        ushort targetCenter = unchecked((ushort)(target >> 16));
        ushort targetEdge = movingDown
            ? unchecked((ushort)(targetCenter + slot.YRadius - 1))
            : unchecked((ushort)(targetCenter - slot.YRadius));
        int row = targetEdge >> 4;
        int firstColumn = unchecked((ushort)(slot.XPosition - slot.XRadius)) >> 4;
        int lastColumn = unchecked((ushort)(slot.XPosition + slot.XRadius - 1)) >> 4;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            if ((uint)column >= (uint)level.WidthInBlocks ||
                (uint)row >= (uint)level.HeightInBlocks ||
                new RoomLevelWord(level.GetCollisionBlock(column, row).LevelWord).HasSolidProbeBit)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Ports <c>EnemyFunc_BBBF</c> at $A0:BBBF. Unlike the ordinary horizontal collision
    /// mover, this look-ahead probe deliberately considers only raw level-word bit fifteen
    /// and does not resolve BTS extensions or slope geometry.
    /// </summary>
    private static bool EnemyHasSolidHighBitHorizontallyAhead(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement)
    {
        uint position = ((uint)slot.XPosition << 16) | slot.XSubposition;
        ushort targetCenter = unchecked((ushort)((position + (uint)displacement) >> 16));
        bool movingLeft = displacement < 0;
        ushort targetEdge = movingLeft
            ? unchecked((ushort)(targetCenter - slot.XRadius))
            : unchecked((ushort)(targetCenter + slot.XRadius - 1));

        ushort topPixel = unchecked((ushort)(slot.YPosition - slot.YRadius));
        ushort bottomPixel = unchecked((ushort)(slot.YPosition + slot.YRadius - 1));
        int firstRow = topPixel >> 4;
        int lastRow = bottomPixel >> 4;
        int column = targetEdge >> 4;
        for (int row = firstRow; row <= lastRow; row++)
        {
            if ((uint)column >= (uint)level.WidthInBlocks ||
                (uint)row >= (uint)level.HeightInBlocks ||
                (level.GetCollisionBlock(column, row).LevelWord & 0x8000) != 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Ports $A0:BC76's read-only vertical solid-bit probe. It does not move the
    /// actor or apply slope/BTS behavior; Yard hiding and Stoke support checks
    /// use it to test attachment without consuming their look-ahead distance.
    /// </summary>
    private static bool EnemyHasSolidHighBitVerticallyAhead(RoomLevelData level, RoomEnemySlot slot, int displacement)
    {
        uint position = ((uint)slot.YPosition << 16) | slot.YSubposition;
        ushort center = unchecked((ushort)((position + (uint)displacement) >> 16));
        ushort edge = displacement < 0
            ? unchecked((ushort)(center - slot.YRadius))
            : unchecked((ushort)(center + slot.YRadius - 1));
        int blockY = edge >> 4;
        int firstBlockX = unchecked((ushort)(slot.XPosition - slot.XRadius)) >> 4;
        int lastBlockX = unchecked((ushort)(slot.XPosition + slot.XRadius - 1)) >> 4;
        if ((uint)blockY >= (uint)level.HeightInBlocks) return true;
        for (int blockX = firstBlockX; blockX <= lastBlockX; blockX++)
        {
            if ((uint)blockX >= (uint)level.WidthInBlocks ||
                new RoomLevelWord(level.GetCollisionBlock(blockX, blockY).LevelWord).HasSolidProbeBit)
                return true;
        }
        return false;
    }

    private static bool EnemyHorizontalProbeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        int blockX,
        int blockY,
        ushort targetEdge,
        int remaining,
        int spanMinusOne,
        bool treatNonSquareSlopesAsWalls)
    {
        int blockIndex = ResolveEnemyCollisionBlockIndex(level, blockX, blockY);
        if (blockIndex < 0)
            return true;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        return block.CollisionType switch
        {
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnusedAir or
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.Slope when block.Bts.IsNonSquareSlope =>
                treatNonSquareSlopesAsWalls,
            RoomCollisionType.Slope => SquareHorizontalSlopeIsSolid(
                slot,
                targetEdge,
                block.Bts,
                remaining,
                spanMinusOne),
            RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension =>
                false, // A zero-offset extension resolves to air.
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock => true,
            _ => throw new InvalidDataException(
                $"Enemy horizontal collision type ${block.CollisionType:X1} is invalid."),
        };
    }

    private bool EnemyVerticalProbeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        int blockX,
        int blockY,
        ushort targetCenter,
        ushort targetEdge,
        bool movingUp,
        int remaining,
        int spanMinusOne)
    {
        int blockIndex = ResolveEnemyCollisionBlockIndex(level, blockX, blockY);
        if (blockIndex < 0)
            return true;

        RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
        return block.CollisionType switch
        {
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnusedAir or
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.Slope when block.Bts.IsNonSquareSlope =>
                NonSquareVerticalSlopeIsSolid(
                    level,
                    slot,
                    block,
                    targetCenter,
                    movingUp),
            RoomCollisionType.Slope => SquareVerticalSlopeIsSolid(
                slot,
                targetEdge,
                block.Bts,
                remaining,
                spanMinusOne),
            RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => false,
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock => true,
            _ => throw new InvalidDataException(
                $"Enemy vertical collision type ${block.CollisionType:X1} is invalid."),
        };
    }

    /// <summary>
    /// Resolves signed type-$5/$D BTS links exactly as the native dispatcher repeats them.
    /// A guard converts corrupt cyclic room data into a useful exception instead of a host
    /// stack overflow; valid chains always terminate well before the room allocation size.
    /// </summary>
    private static int ResolveEnemyCollisionBlockIndex(
        RoomLevelData level,
        int blockX,
        int blockY)
    {
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return -1;
        }

        int blockIndex = blockY * level.WidthInBlocks + blockX;
        int maximumLinks = level.WidthInBlocks * level.HeightInBlocks;
        for (int link = 0; link <= maximumLinks; link++)
        {
            if ((uint)blockIndex >= (uint)maximumLinks)
                return -1;
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            int offset = block.CollisionType switch
            {
                RoomCollisionType.HorizontalExtension when
                    block.Bts != RoomBlockBehaviorValues.None => block.Bts.ExtensionOffset,
                RoomCollisionType.VerticalExtension when
                    block.Bts != RoomBlockBehaviorValues.None =>
                    block.Bts.ExtensionOffset * level.WidthInBlocks,
                _ => 0,
            };
            if (offset == 0)
                return blockIndex;
            blockIndex += offset;
        }

        throw new InvalidDataException(
            $"Enemy collision BTS chain from block ({blockX},{blockY}) is cyclic.");
    }

    private static bool SquareHorizontalSlopeIsSolid(
        RoomEnemySlot slot,
        ushort targetEdge,
        RoomBlockBehavior bts,
        int remaining,
        int spanMinusOne)
    {
        int tableIndex = 4 * bts.SlopeShape +
            (bts.SlopeOrientation ^ ((targetEdge & 8) >> 3));
        if (remaining == 0)
        {
            if (((slot.YRadius + slot.YPosition - 1) & 8) == 0)
                return (SquareSlopeCollisionTable[tableIndex] & 0x80) != 0;
        }
        else if (remaining == spanMinusOne &&
                 ((slot.YPosition - slot.YRadius) & 8) != 0)
        {
            return (SquareSlopeCollisionTable[tableIndex ^ 2] & 0x80) != 0;
        }

        if ((SquareSlopeCollisionTable[tableIndex] & 0x80) != 0)
            return true;
        return (SquareSlopeCollisionTable[tableIndex ^ 2] & 0x80) != 0;
    }

    private static bool SquareVerticalSlopeIsSolid(
        RoomEnemySlot slot,
        ushort targetEdge,
        RoomBlockBehavior bts,
        int remaining,
        int spanMinusOne)
    {
        int tableIndex = 4 * bts.SlopeShape +
            (bts.SlopeOrientation ^ ((targetEdge & 8) >> 2));
        if (remaining == 0)
        {
            if (((slot.XRadius + slot.XPosition - 1) & 8) == 0)
                return (SquareSlopeCollisionTable[tableIndex] & 0x80) != 0;
        }
        else if (remaining == spanMinusOne &&
                 ((slot.XPosition - slot.XRadius) & 8) != 0)
        {
            return (SquareSlopeCollisionTable[tableIndex ^ 1] & 0x80) != 0;
        }

        if ((SquareSlopeCollisionTable[tableIndex] & 0x80) != 0)
            return true;
        return (SquareSlopeCollisionTable[tableIndex ^ 1] & 0x80) != 0;
    }

    private bool NonSquareVerticalSlopeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        RoomCollisionBlock block,
        ushort targetCenter,
        bool movingUp)
    {
        if (block.Index % level.WidthInBlocks != slot.XPosition >> 4)
            return false;

        RoomBlockBehavior bts = block.Bts;
        int xWithinBlock = (bts.SlopeFlipsHorizontally
            ? slot.XPosition ^ 0x000f
            : slot.XPosition) & 0x000f;
        int height = ReadNonSquareSlopeHeight(bts, xWithinBlock);
        if (movingUp)
        {
            if (!bts.SlopeFlipsVertically)
                return false;
            int edgeWithinBlock = ((targetCenter - slot.YRadius) & 0x000f) ^ 0x000f;
            int adjustment = height - edgeWithinBlock - 1;
            if (adjustment > 0)
                return false;
            slot.YPosition = unchecked((ushort)(targetCenter - adjustment));
            slot.YSubposition = 0;
            return true;
        }

        if (bts.SlopeFlipsVertically)
            return false;
        int bottomWithinBlock = (slot.YRadius + targetCenter - 1) & 0x000f;
        int downwardAdjustment = height - bottomWithinBlock - 1;
        if (height - bottomWithinBlock != 1 && downwardAdjustment >= 0)
            return false;
        slot.YPosition = unchecked((ushort)(targetCenter + downwardAdjustment));
        slot.YSubposition = 0xffff;
        return true;
    }

    /// <summary>Ports <c>AlignEnemyYPositionWIthNonSquareSlope</c> at $A0:C8AD.</summary>
    private void AlignEnemyYWithNonSquareSlope(RoomLevelData level, RoomEnemySlot slot) =>
        AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);

    /// <summary>
    /// Runs the same two probes as <see cref="AlignEnemyYWithNonSquareSlope"/> while retaining
    /// the carry result returned by native $A0:C8AD. Yard is the first translated caller that
    /// consumes that carry to suppress corner-transition animation after a slope adjustment.
    /// </summary>
    private bool AlignEnemyYWithNonSquareSlopeAndReportAdjustment(
        RoomLevelData level,
        RoomEnemySlot slot)
    {
        bool adjustedFloor = AlignAgainstSlopeAtPixel(
            level,
            slot,
            slot.XPosition,
            unchecked((ushort)(slot.YPosition + slot.YRadius - 1)),
            underside: false);
        bool adjustedCeiling = AlignAgainstSlopeAtPixel(
            level,
            slot,
            slot.XPosition,
            unchecked((ushort)(slot.YPosition - slot.YRadius)),
            underside: true);
        return adjustedFloor || adjustedCeiling;
    }

    private bool AlignAgainstSlopeAtPixel(
        RoomLevelData level,
        RoomEnemySlot slot,
        ushort x,
        ushort y,
        bool underside)
    {
        int blockX = x >> 4;
        int blockY = y >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
        if (block.CollisionType != RoomCollisionType.Slope ||
            !block.Bts.IsNonSquareSlope)
            return false;
        if (underside != block.Bts.SlopeFlipsVertically)
            return false;

        int xWithinBlock = (block.Bts.SlopeFlipsHorizontally ? x ^ 0x000f : x) & 0x000f;
        int height = ReadNonSquareSlopeHeight(block.Bts, xWithinBlock);
        int edgeWithinBlock = underside ? (y & 0x000f) ^ 0x000f : y & 0x000f;
        int adjustment = height - edgeWithinBlock - 1;
        if (adjustment >= 0)
            return false;
        slot.YPosition = underside
            ? unchecked((ushort)(slot.YPosition - adjustment))
            : unchecked((ushort)(slot.YPosition + adjustment));
        return true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "Keep this table-only migration from changing the reflection-visible instance signatures of its many transitive diagnostic entry points.")]
    private int ReadNonSquareSlopeHeight(RoomBlockBehavior bts, int xWithinBlock) =>
        SlopeHeightDefinitions.Read(bts.SlopeShape, xWithinBlock);
}
