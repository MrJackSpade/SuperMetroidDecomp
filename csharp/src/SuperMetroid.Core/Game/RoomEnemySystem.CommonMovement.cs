using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared bank-$A0 enemy movement primitives. Enemy AI supplies a signed 16.16 displacement;
/// these helpers retain the cartridge's scan order, BTS extension dispatch, square-slope
/// half selection, non-square vertical geometry, and collision-edge alignment.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports <c>MoveEnemyRightBy_14_12_IgnoreSlopes</c> at $A0:C6AB.</summary>
    private bool MoveEnemyHorizontallyIgnoringNonSquareSlopes(
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
    private bool MoveEnemyHorizontallyTreatingSlopesAsWalls(
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
    private bool MoveEnemyHorizontally(
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
        // $A0:C6E2-$C716: the 8-bit multiplier forms (top row & $FF) * width, the target
        // column is added unbounded, and each scanned row adds the width again.
        int firstIndex = ((topPixel >> 4) & 0xff) * level.WidthInBlocks + (targetEdge >> 4);
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyHorizontalProbeIsSolid(
                    level,
                    slot,
                    EnemyMoverBlockIndex(firstIndex + scanIndex * level.WidthInBlocks),
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
        // $A0:C7D0-$C7F0: (target row & $FF) * width plus the left column, then each
        // scanned column steps to the next native block index.
        int firstIndex = ((targetEdge >> 4) & 0xff) * level.WidthInBlocks + (leftPixel >> 4);
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyVerticalProbeIsSolid(
                    level,
                    slot,
                    EnemyMoverBlockIndex(firstIndex + scanIndex),
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

    /// <summary>Classifies one horizontal sweep block, dispatching slope and spike behavior after resolving extensions.</summary>
    /// <param name="level">Room level and its native allocation backing collision data.</param>
    /// <param name="slot">Enemy whose vertical span and spike response are evaluated.</param>
    /// <param name="nativeIndex">Wrapped block index produced by the bank-$A0 horizontal scan.</param>
    /// <param name="targetEdge">Pixel coordinate of the moving enemy edge being tested.</param>
    /// <param name="remaining">Number of scan rows remaining after this probe.</param>
    /// <param name="spanMinusOne">Last scan-row index, used to preserve endpoint-specific slope tests.</param>
    /// <param name="treatNonSquareSlopesAsWalls">Whether non-square slopes collide as walls for this movement mode.</param>
    /// <returns><see langword="true"/> when this probe blocks movement or reacts as a solid spike block.</returns>
    private bool EnemyHorizontalProbeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        int nativeIndex,
        ushort targetEdge,
        int remaining,
        int spanMinusOne,
        bool treatNonSquareSlopesAsWalls)
    {
        RoomCollisionBlock block = ResolveEnemyMoverBlock(level, nativeIndex);
        int blockIndex = block.Index;
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
            RoomCollisionType.SpikeBlock => ReactToEnemySpikeBlock(level, blockIndex, block),
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock => true,
            _ => throw new InvalidDataException(
                $"Enemy horizontal collision type ${block.CollisionType:X1} is invalid."),
        };
    }

    /// <summary>Classifies one vertical sweep block, including square or non-square slope response.</summary>
    /// <param name="level">Room level and its native allocation backing collision data.</param>
    /// <param name="slot">Enemy whose horizontal span and vertical position may be tested or adjusted.</param>
    /// <param name="nativeIndex">Wrapped block index produced by the bank-$A0 vertical scan.</param>
    /// <param name="targetCenter">Enemy center after applying the candidate vertical displacement.</param>
    /// <param name="targetEdge">Leading edge pixel coordinate used for the collision probe.</param>
    /// <param name="movingUp">Whether the candidate displacement moves toward the ceiling.</param>
    /// <param name="remaining">Number of scan columns remaining after this probe.</param>
    /// <param name="spanMinusOne">Last scan-column index, used to preserve endpoint-specific slope tests.</param>
    /// <returns><see langword="true"/> when the probe blocks movement or triggers a solid spike response.</returns>
    private bool EnemyVerticalProbeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        int nativeIndex,
        ushort targetCenter,
        ushort targetEdge,
        bool movingUp,
        int remaining,
        int spanMinusOne)
    {
        RoomCollisionBlock block = ResolveEnemyMoverBlock(level, nativeIndex);
        int blockIndex = block.Index;
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
            RoomCollisionType.SpikeBlock => ReactToEnemySpikeBlock(level, blockIndex, block),
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock => true,
            _ => throw new InvalidDataException(
                $"Enemy vertical collision type ${block.CollisionType:X1} is invalid."),
        };
    }

    /// <summary>
    /// The movers index <c>LevelData,X</c> with a 16-bit byte offset, so the block index
    /// wraps at $8000 blocks.
    /// </summary>
    private static int EnemyMoverBlockIndex(int index) => ((index << 1) & 0xffff) >> 1;

    /// <summary>
    /// Reads a block for the bank-$A0 movers, which never bound the index to the authored
    /// room: past the room plane they read the rest of the native level allocation (the
    /// decompressed BTS/BG2 stream, then the $8000 prefill). Extension links are followed
    /// as the native reaction dispatcher repeats them.
    /// </summary>
    private static RoomCollisionBlock ResolveEnemyMoverBlock(RoomLevelData level, int nativeIndex)
    {
        int blockIndex = nativeIndex;
        int maximumLinks = level.WidthInBlocks * level.HeightInBlocks;
        for (int link = 0; link <= maximumLinks; link++)
        {
            if (!level.IsLogicalBlockIndex(blockIndex))
                return ReadEnemyMoverAllocationTail(level, blockIndex);
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
                return block;
            blockIndex = EnemyMoverBlockIndex(blockIndex + offset);
        }

        throw new InvalidDataException(
            $"Enemy collision BTS chain from native block {nativeIndex} is cyclic.");
    }

    /// <summary>Reads a mover collision block beyond the logical room plane from the native level allocation tail.</summary>
    /// <param name="level">Room level whose backing allocation supplies the out-of-plane level word.</param>
    /// <param name="blockIndex">Wrapped block index outside the authored room plane.</param>
    /// <returns>A collision block using the tail level word and no BTS behavior.</returns>
    /// <exception cref="NotSupportedException">The tail word denotes a slope, extension, or spike block requiring unavailable BTS data.</exception>
    private static RoomCollisionBlock ReadEnemyMoverAllocationTail(RoomLevelData level, int blockIndex)
    {
        var block = new RoomCollisionBlock(
            Index: -1,
            LevelWord: level.ReadAllocationTailLevelWord(blockIndex),
            Behavior: 0);
        // Slopes, extensions and spike blocks read BTS, and native BTS past the authored
        // room ($7F:6402 + index) is unbounded memory left by earlier rooms.
        if (block.CollisionType is RoomCollisionType.Slope or
            RoomCollisionType.HorizontalExtension or
            RoomCollisionType.VerticalExtension or
            RoomCollisionType.SpikeBlock)
        {
            throw new NotSupportedException(
                $"Enemy collision reached BTS-dependent type ${(int)block.CollisionType:X1} at native " +
                $"block {blockIndex}, past the room plane; native BTS there is unbounded memory.");
        }
        return block;
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

    /// <summary>Tests square-slope quadrants intersected by a horizontal mover edge using the native half-tile rules.</summary>
    /// <param name="slot">Enemy footprint whose top and bottom span the slope block.</param>
    /// <param name="targetEdge">Horizontal leading-edge pixel coordinate used to select the slope half.</param>
    /// <param name="bts">Slope shape and orientation encoded by the block's BTS value.</param>
    /// <param name="remaining">Rows remaining in the horizontal footprint scan.</param>
    /// <param name="spanMinusOne">Last row index in that scan, distinguishing its far endpoint.</param>
    /// <returns><see langword="true"/> when either relevant slope quadrant has a solid probe bit.</returns>
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
                return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0;
        }
        else if (remaining == spanMinusOne &&
                 ((slot.YPosition - slot.YRadius) & 8) != 0)
        {
            return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 2) & 0x80) != 0;
        }

        if ((SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0)
            return true;
        return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 2) & 0x80) != 0;
    }

    /// <summary>Tests square-slope quadrants intersected by a vertical mover edge using the native half-tile rules.</summary>
    /// <param name="slot">Enemy footprint whose left and right span the slope block.</param>
    /// <param name="targetEdge">Vertical leading-edge pixel coordinate used to select the slope half.</param>
    /// <param name="bts">Slope shape and orientation encoded by the block's BTS value.</param>
    /// <param name="remaining">Columns remaining in the vertical footprint scan.</param>
    /// <param name="spanMinusOne">Last column index in that scan, distinguishing its far endpoint.</param>
    /// <returns><see langword="true"/> when either relevant slope quadrant has a solid probe bit.</returns>
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
                return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0;
        }
        else if (remaining == spanMinusOne &&
                 ((slot.XPosition - slot.XRadius) & 8) != 0)
        {
            return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 1) & 0x80) != 0;
        }

        if ((SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0)
            return true;
        return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 1) & 0x80) != 0;
    }

    /// <summary>Applies pixel-height collision and position correction for a non-square slope during vertical movement.</summary>
    /// <param name="level">Room dimensions used to ensure the slope block aligns with the enemy's column.</param>
    /// <param name="slot">Enemy whose vertical position may be adjusted to the slope surface.</param>
    /// <param name="block">Resolved non-square slope block and its orientation data.</param>
    /// <param name="targetCenter">Candidate enemy center after vertical displacement.</param>
    /// <param name="movingUp">Whether movement approaches the slope from below.</param>
    /// <returns><see langword="true"/> when the slope stops movement and adjusts the enemy position.</returns>
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

    /// <summary>Adjusts the enemy center when a selected floor or ceiling edge penetrates a non-square slope.</summary>
    /// <param name="level">Room level containing the candidate slope block.</param>
    /// <param name="slot">Enemy whose vertical center is adjusted when alignment is required.</param>
    /// <param name="x">Horizontal pixel coordinate of the edge sample.</param>
    /// <param name="y">Vertical pixel coordinate of the edge sample.</param>
    /// <param name="underside">Whether to test the slope underside rather than its floor surface.</param>
    /// <returns><see langword="true"/> when the sample penetrated the slope and changed the enemy center.</returns>
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

    /// <summary>Looks up a non-square slope's pixel height at a horizontal position within its tile.</summary>
    /// <param name="bts">Slope shape and flip orientation.</param>
    /// <param name="xWithinBlock">Pixel column within the 16-pixel block, after horizontal flip handling.</param>
    /// <returns>Surface height in pixels for the selected slope column.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "Keep this table-only migration from changing the reflection-visible instance signatures of its many transitive diagnostic entry points.")]
    private int ReadNonSquareSlopeHeight(RoomBlockBehavior bts, int xWithinBlock) =>
        SlopeHeightDefinitions.Read(bts.SlopeShape, xWithinBlock);
}
