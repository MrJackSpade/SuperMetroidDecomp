using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Enemy movement against room terrain (bank $A0 movers): horizontal and vertical steps with square and
/// non-square slopes, solid-ahead probes, and the spike/enemy-breakable block reaction that spawns its PLM
/// through the active room PLM owner supplied for each enemy frame.
/// </summary>
internal sealed class EnemyTerrain
{
    /// <summary>Ports <c>MoveEnemyRightBy_14_12_IgnoreSlopes</c> at $A0:C6AB.</summary>
    internal bool MoveEnemyHorizontallyIgnoringNonSquareSlopes(
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
    internal bool MoveEnemyHorizontallyTreatingSlopesAsWalls(
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

        uint oldPosition = slot.Position.X.RawValue;
        uint newPosition = unchecked(oldPosition + (uint)displacement);
        ushort targetCenter = SnesFixedPosition.WholeOf(unchecked((int)newPosition));
        bool movingLeft = displacement < 0;
        ushort targetEdge = movingLeft
            ? unchecked((ushort)(targetCenter - slot.Radius.X))
            : unchecked((ushort)(targetCenter + slot.Radius.X - 1));

        ushort topPixel = unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y));
        ushort bottomPixel = unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int spanMinusOne = unchecked((ushort)(bottomPixel - (topPixel & 0xfff0))) >> 4;
        // $A0:C6E2-$C716: the 8-bit multiplier forms (top row & $FF) * width, the target
        // column is added unbounded, and each scanned row adds the width again.
        int firstIndex = unchecked((byte)(topPixel >> 4)) * level.Size.X + (targetEdge >> 4);
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyHorizontalProbeIsSolid(
                    level,
                    slot,
                    EnemyMoverBlockIndex(firstIndex + scanIndex * level.Size.X),
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
            slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(newPosition));
            return false;
        }

        // $A0:C755/C76D compare against the pre-move position before accepting the aligned
        // center. That prevents a large corrupt displacement from snapping through a wall.
        if (movingLeft)
        {
            ushort aligned = unchecked((ushort)((targetEdge | 0x000f) + slot.Radius.X + 1));
            if (aligned <= slot.Position.X.Whole)
                slot.Position = slot.Position.WithWholeX(aligned);
            slot.Position = slot.Position.WithFractionX(0);
        }
        else
        {
            ushort aligned = unchecked((ushort)((targetEdge & 0xfff0) - slot.Radius.X));
            if (aligned >= slot.Position.X.Whole)
                slot.Position = slot.Position.WithWholeX(aligned);
            slot.Position = slot.Position.WithFractionX(0xffff);
        }
        return true;
    }

    /// <summary>Ports <c>MoveEnemyDownBy_14_12</c> at $A0:C786.</summary>
    internal bool MoveEnemyVertically(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement)
    {
        if (displacement == 0)
            return false;

        uint oldPosition = slot.Position.Y.RawValue;
        uint newPosition = unchecked(oldPosition + (uint)displacement);
        ushort targetCenter = SnesFixedPosition.WholeOf(unchecked((int)newPosition));
        bool movingUp = displacement < 0;
        ushort targetEdge = movingUp
            ? unchecked((ushort)(targetCenter - slot.Radius.Y))
            : unchecked((ushort)(targetCenter + slot.Radius.Y - 1));

        ushort leftPixel = unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X));
        ushort rightPixel = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1));
        int spanMinusOne = unchecked((ushort)(rightPixel - (leftPixel & 0xfff0))) >> 4;
        // $A0:C7D0-$C7F0: (target row & $FF) * width plus the left column, then each
        // scanned column steps to the next native block index.
        int firstIndex = unchecked((byte)(targetEdge >> 4)) * level.Size.X + (leftPixel >> 4);
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyVerticalProbeIsSolid(level, slot, EnemyMoverBlockIndex(firstIndex + scanIndex), new EnemyVerticalProbe { TargetCenter = targetCenter, TargetEdge = targetEdge, MovingUp = movingUp, Remaining = remaining, SpanMinusOne = spanMinusOne }))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(newPosition));
            return false;
        }

        // A non-square slope reactor may already have placed the center more precisely.
        // The native outer helper performs this conditional coarse alignment afterward.
        if (movingUp)
        {
            ushort aligned = unchecked((ushort)((targetEdge | 0x000f) + slot.Radius.Y + 1));
            if (aligned <= slot.Position.Y.Whole)
                slot.Position = slot.Position.WithWholeY(aligned);
            slot.Position = slot.Position.WithFractionY(0);
        }
        else
        {
            ushort aligned = unchecked((ushort)((targetEdge & 0xfff0) - slot.Radius.Y));
            if (aligned >= slot.Position.Y.Whole)
                slot.Position = slot.Position.WithWholeY(aligned);
            slot.Position = slot.Position.WithFractionY(0xffff);
        }
        return true;
    }

    /// <summary>
    /// Ports the geometry-only portion of $A0:BF8A used by Skree/Metaree. Unlike the common
    /// collision dispatcher, this routine deliberately tests only level-word bit 15.
    /// </summary>
    internal static bool EnemyHasSolidHighBitAhead(
        RoomLevelData level,
        RoomEnemySlot slot,
        int unsignedDistance,
        bool movingDown)
    {
        uint position = slot.Position.Y.RawValue;
        uint target = movingDown
            ? unchecked(position + (uint)unsignedDistance)
            : unchecked(position - (uint)unsignedDistance);
        ushort targetCenter = SnesFixedPosition.WholeOf(unchecked((int)target));
        ushort targetEdge = movingDown
            ? unchecked((ushort)(targetCenter + slot.Radius.Y - 1))
            : unchecked((ushort)(targetCenter - slot.Radius.Y));
        int row = targetEdge >> 4;
        int firstColumn = unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X)) >> 4;
        int lastColumn = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1)) >> 4;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            if ((uint)column >= (uint)level.Size.X ||
                (uint)row >= (uint)level.Size.Y ||
                new RoomLevelWord(level.GetCollisionBlock(new(unchecked((ushort)column), unchecked((ushort)row))).LevelWord).HasSolidProbeBit)
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
    internal static bool EnemyHasSolidHighBitHorizontallyAhead(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement)
    {
        uint position = slot.Position.X.RawValue;
        ushort targetCenter = SnesFixedPosition.WholeOf(unchecked((int)(position + (uint)displacement)));
        bool movingLeft = displacement < 0;
        ushort targetEdge = movingLeft
            ? unchecked((ushort)(targetCenter - slot.Radius.X))
            : unchecked((ushort)(targetCenter + slot.Radius.X - 1));

        ushort topPixel = unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y));
        ushort bottomPixel = unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int firstRow = topPixel >> 4;
        int lastRow = bottomPixel >> 4;
        int column = targetEdge >> 4;
        for (int row = firstRow; row <= lastRow; row++)
        {
            if ((uint)column >= (uint)level.Size.X ||
                (uint)row >= (uint)level.Size.Y ||
                new RoomLevelWord(level.GetCollisionBlock(new(unchecked((ushort)column), unchecked((ushort)row))).LevelWord).HasSolidProbeBit)
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
    internal static bool EnemyHasSolidHighBitVerticallyAhead(RoomLevelData level, RoomEnemySlot slot, int displacement)
    {
        uint position = slot.Position.Y.RawValue;
        ushort center = SnesFixedPosition.WholeOf(unchecked((int)(position + (uint)displacement)));
        ushort edge = displacement < 0
            ? unchecked((ushort)(center - slot.Radius.Y))
            : unchecked((ushort)(center + slot.Radius.Y - 1));
        int blockY = edge >> 4;
        int firstBlockX = unchecked((ushort)(slot.Position.X.Whole - slot.Radius.X)) >> 4;
        int lastBlockX = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - 1)) >> 4;
        if ((uint)blockY >= (uint)level.Size.Y) return true;
        for (int blockX = firstBlockX; blockX <= lastBlockX; blockX++)
        {
            if ((uint)blockX >= (uint)level.Size.X ||
                new RoomLevelWord(level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY))).LevelWord).HasSolidProbeBit)
                return true;
        }
        return false;
    }

    /// <summary>Classifies the next horizontal block, including square-slope quadrants and spike-block PLM reactions.</summary>
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
                $"Enemy horizontal collision type ${(int)block.CollisionType:X1} is invalid."),
        };
    }

    /// <summary>Classifies the next vertical block and may align the enemy against a non-square slope.</summary>
    private bool EnemyVerticalProbeIsSolid(RoomLevelData level, RoomEnemySlot slot, int nativeIndex, EnemyVerticalProbe probe)
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
                    probe.TargetCenter,
                    probe.MovingUp),
            RoomCollisionType.Slope => SquareVerticalSlopeIsSolid(
                slot,
                probe.TargetEdge,
                block.Bts,
                probe.Remaining,
                probe.SpanMinusOne),
            RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => false,
            RoomCollisionType.SpikeBlock => ReactToEnemySpikeBlock(level, blockIndex, block),
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock => true,
            _ => throw new InvalidDataException(
                $"Enemy vertical collision type ${(int)block.CollisionType:X1} is invalid."),
        };
    }

    /// <summary>
    /// The movers index <c>LevelData,X</c> with a 16-bit byte offset, so the block index
    /// wraps at $8000 blocks.
    /// </summary>
    private static int EnemyMoverBlockIndex(int index) => unchecked((ushort)(index << 1)) >> 1;

    /// <summary>
    /// Reads a block for the bank-$A0 movers, which never bound the index to the authored
    /// room: past the room plane they read the rest of the native level allocation (the
    /// decompressed BTS/BG2 stream, then the $8000 prefill). Extension links are followed
    /// as the native reaction dispatcher repeats them.
    /// </summary>
    private static RoomCollisionBlock ResolveEnemyMoverBlock(RoomLevelData level, int nativeIndex)
    {
        int blockIndex = nativeIndex;
        int maximumLinks = level.Size.X * level.Size.Y;
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
                    block.Bts.ExtensionOffset * level.Size.X,
                RoomCollisionType.Air or RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
                    RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or
                    RoomCollisionType.HorizontalExtension or RoomCollisionType.UnusedAir or
                    RoomCollisionType.BombableAir or RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock or
                    RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
                    RoomCollisionType.ShootableBlock or RoomCollisionType.VerticalExtension or
                    RoomCollisionType.GrappleBlock or RoomCollisionType.BombableBlock => 0,
                _ => throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}."),
            };
            if (offset == 0)
                return block;
            blockIndex = EnemyMoverBlockIndex(blockIndex + offset);
        }

        throw new InvalidDataException(
            $"Enemy collision BTS chain from native block {nativeIndex} is cyclic.");
    }

    /// <summary>Reads the native level allocation past the authored room, rejecting types that require unbounded BTS.</summary>
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
    internal static int ResolveEnemyCollisionBlockIndex(
        RoomLevelData level,
        Position16<RoomBlocks> start)
    {
        if (!level.Size.Contains(start))
            return -1;

        int blockIndex = level.Size.RowMajorIndex(start);
        int maximumLinks = level.Size.X * level.Size.Y;
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
                    block.Bts.ExtensionOffset * level.Size.X,
                RoomCollisionType.Air or RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
                    RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or
                    RoomCollisionType.HorizontalExtension or RoomCollisionType.UnusedAir or
                    RoomCollisionType.BombableAir or RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock or
                    RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
                    RoomCollisionType.ShootableBlock or RoomCollisionType.VerticalExtension or
                    RoomCollisionType.GrappleBlock or RoomCollisionType.BombableBlock => 0,
                _ => throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}."),
            };
            if (offset == 0)
                return blockIndex;
            blockIndex += offset;
        }

        throw new InvalidDataException(
            $"Enemy collision BTS chain from block ({start.X},{start.Y}) is cyclic.");
    }

    /// <summary>Tests the target half-tile quadrant and the enemy's vertical span against a square slope.</summary>
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
            if (((slot.Radius.Y + slot.Position.Y.Whole - 1) & 8) == 0)
                return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0;
        }
        else if (remaining == spanMinusOne &&
                 ((slot.Position.Y.Whole - slot.Radius.Y) & 8) != 0)
        {
            return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 2) & 0x80) != 0;
        }

        if ((SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0)
            return true;
        return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 2) & 0x80) != 0;
    }

    /// <summary>Tests the target half-tile quadrant and the enemy's horizontal span against a square slope.</summary>
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
            if (((slot.Radius.X + slot.Position.X.Whole - 1) & 8) == 0)
                return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0;
        }
        else if (remaining == spanMinusOne &&
                 ((slot.Position.X.Whole - slot.Radius.X) & 8) != 0)
        {
            return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 1) & 0x80) != 0;
        }

        if ((SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex) & 0x80) != 0)
            return true;
        return (SquareSlopeDefinitions.ReadEnemyQuadrant(tableIndex ^ 1) & 0x80) != 0;
    }

    /// <summary>Checks slope height under the enemy center and snaps its Y position to the floor or underside.</summary>
    private static bool NonSquareVerticalSlopeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        RoomCollisionBlock block,
        ushort targetCenter,
        bool movingUp)
    {
        if (block.Index % level.Size.X != slot.Position.X.Whole >> 4)
            return false;

        RoomBlockBehavior bts = block.Bts;
        int xWithinBlock = (bts.SlopeFlipsHorizontally
            ? slot.Position.X.Whole ^ 0x000f
            : slot.Position.X.Whole) & 0x000f;
        int height = ReadNonSquareSlopeHeight(bts, xWithinBlock);
        if (movingUp)
        {
            if (!bts.SlopeFlipsVertically)
                return false;
            int edgeWithinBlock = ((targetCenter - slot.Radius.Y) & 0x000f) ^ 0x000f;
            int adjustment = height - edgeWithinBlock - 1;
            if (adjustment > 0)
                return false;
            slot.Position = slot.Position.WithY(new(unchecked((ushort)(targetCenter - adjustment)), 0));
            return true;
        }

        if (bts.SlopeFlipsVertically)
            return false;
        int bottomWithinBlock = (slot.Radius.Y + targetCenter - 1) & 0x000f;
        int downwardAdjustment = height - bottomWithinBlock - 1;
        if (height - bottomWithinBlock != 1 && downwardAdjustment >= 0)
            return false;
        slot.Position = slot.Position.WithY(new(unchecked((ushort)(targetCenter + downwardAdjustment)), 0xffff));
        return true;
    }

    /// <summary>Ports <c>AlignEnemyYPositionWIthNonSquareSlope</c> at $A0:C8AD.</summary>
    internal static void AlignEnemyYWithNonSquareSlope(RoomLevelData level, RoomEnemySlot slot) =>
        AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);

    /// <summary>
    /// Runs the same two probes as <see cref="AlignEnemyYWithNonSquareSlope"/> while retaining
    /// the carry result returned by native $A0:C8AD. Yard is the first translated caller that
    /// consumes that carry to suppress corner-transition animation after a slope adjustment.
    /// </summary>
    internal static bool AlignEnemyYWithNonSquareSlopeAndReportAdjustment(
        RoomLevelData level,
        RoomEnemySlot slot)
    {
        bool adjustedFloor = AlignAgainstSlopeAtPixel(
            level,
            slot,
            new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1))),
            underside: false);
        bool adjustedCeiling = AlignAgainstSlopeAtPixel(
            level,
            slot,
            new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y))),
            underside: true);
        return adjustedFloor || adjustedCeiling;
    }

    /// <summary>Adjusts the enemy center when the sampled floor or ceiling pixel penetrates a matching non-square slope.</summary>
    internal static bool AlignAgainstSlopeAtPixel(
        RoomLevelData level,
        RoomEnemySlot slot,
        Position16<RoomPixels> pixel,
        bool underside)
    {
        int blockX = pixel.X >> 4;
        int blockY = pixel.Y >> 4;
        if ((uint)blockX >= (uint)level.Size.X ||
            (uint)blockY >= (uint)level.Size.Y)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
        if (block.CollisionType != RoomCollisionType.Slope ||
            !block.Bts.IsNonSquareSlope)
            return false;
        if (underside != block.Bts.SlopeFlipsVertically)
            return false;

        int xWithinBlock = (block.Bts.SlopeFlipsHorizontally ? pixel.X ^ 0x000f : pixel.X) & 0x000f;
        int height = ReadNonSquareSlopeHeight(block.Bts, xWithinBlock);
        int edgeWithinBlock = underside ? (pixel.Y & 0x000f) ^ 0x000f : pixel.Y & 0x000f;
        int adjustment = height - edgeWithinBlock - 1;
        if (adjustment >= 0)
            return false;
        slot.Position = slot.Position.WithWholeY(underside
            ? unchecked((ushort)(slot.Position.Y.Whole - adjustment))
            : unchecked((ushort)(slot.Position.Y.Whole + adjustment)));
        return true;
    }

    /// <summary>Looks up the authored height of a slope shape at an already oriented block-local X.</summary>
    internal static int ReadNonSquareSlopeHeight(RoomBlockBehavior bts, int xWithinBlock) =>
        SlopeHeightDefinitions.Read(bts.SlopeShape, xWithinBlock);
    /// <summary>
    /// The active frame's room PLM owner for enemy-breakable blocks. This synchronous call
    /// context is not persisted; every enemy frame supplies it again after debugger restoration.
    /// </summary>
    [NonSerialized] private RoomPlmSystem? _collisionPlms;

    /// <summary>Spawns the breakable-block PLM for an enemy spike reaction and clears collision even if its pool is full.</summary>
    internal bool ReactToEnemySpikeBlock(RoomLevelData level, int blockIndex, RoomCollisionBlock block)
    {
        if (!EnemyBreakableTerrainDefinitions.IsEnemyBreakable(block.Behavior)) return true;
        var plms = _collisionPlms ?? throw new InvalidOperationException("Enemy-breakable terrain requires the active room PLM owner.");
        plms.BreakableTerrain.TrySpawnEnemyBreakableBlock(level, blockIndex);
        return false; // $A0:C2D6 clears carry even if the PLM pool was full.
    }

    /// <summary>Supplies the active room PLM owner to <see cref="EnemyTerrain"/> for one enemy frame.</summary>
    internal readonly struct EnemyTerrainScope : IDisposable
    {
        /// <summary>Terrain owner whose active PLM context is temporarily replaced.</summary>
        private readonly EnemyTerrain owner;
        /// <summary>PLM context restored when the enemy frame leaves this scope.</summary>
        private readonly RoomPlmSystem? previous;
        /// <summary>Installs the frame's room PLM owner while retaining the previous context.</summary>
        public EnemyTerrainScope(EnemyTerrain owner, RoomPlmSystem? current)
        {
            this.owner = owner;
            previous = owner._collisionPlms;
            owner._collisionPlms = current;
        }
        /// <summary>Restores the terrain owner's prior PLM context.</summary>
        public void Dispose() => owner._collisionPlms = previous;
    }
}
