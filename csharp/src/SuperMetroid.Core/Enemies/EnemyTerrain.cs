using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy movement against room terrain (bank $A0 movers): horizontal and vertical steps with square and
/// non-square slopes, solid-ahead probes, and the spike/enemy-breakable block reaction that spawns its PLM
/// through the active room PLM owner supplied for each enemy frame.
/// </summary>
[RoomLifetime]
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
            slopes: NonSquareSlopes.Ignored);

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
            slopes: NonSquareSlopes.Walls);

    /// <summary>
    /// Shared body of the three native horizontal enemy movers. The remaining $8000
    /// process-slopes mode will reuse this seam when its first retail caller is translated.
    /// </summary>
    private bool MoveEnemyHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement,
        NonSquareSlopes slopes)
    {
        if (displacement == 0)
            return false;

        uint oldPosition = slot.Position.X.RawValue;
        uint newPosition = unchecked(oldPosition + (uint)displacement);
        int targetCenter = FixedCoordinate.WholeOf(unchecked((int)newPosition));
        bool movingLeft = displacement < 0;
        int targetEdge = movingLeft
            ? CoordinateAxis.Wrap(targetCenter - slot.Radius.X)
            : CoordinateAxis.Wrap(targetCenter + slot.Radius.X - 1);

        int topPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y);
        int bottomPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int spanMinusOne = CoordinateAxis.Wrap(bottomPixel - RoomGridConversions.BlockStart(topPixel)) >> 4;
        // $A0:C6E2-$C716: the 8-bit multiplier forms (top row & $FF) * width, the target
        // column is added unbounded, and each scanned row adds the width again.
        int firstIndex = WordWrap.LowByte(topPixel >> 4) * level.Size.X + (targetEdge >> 4);
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
                    slopes == NonSquareSlopes.Walls))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.Position = slot.Position.WithX(FixedCoordinate.FromRaw(newPosition));
            return false;
        }

        // $A0:C755/C76D compare against the pre-move position before accepting the aligned
        // center. That prevents a large corrupt displacement from snapping through a wall.
        if (movingLeft)
        {
            int aligned = CoordinateAxis.Wrap(RoomGridConversions.BlockEnd(targetEdge) + slot.Radius.X + 1);
            if (aligned <= slot.Position.X.Whole)
                slot.Position = slot.Position.WithWholeX(aligned);
            slot.Position = slot.Position.WithFractionX(0);
        }
        else
        {
            int aligned = CoordinateAxis.Wrap(RoomGridConversions.BlockStart(targetEdge) - slot.Radius.X);
            if (aligned >= slot.Position.X.Whole)
                slot.Position = slot.Position.WithWholeX(aligned);
            slot.Position = slot.Position.WithFractionX(RoomPixelMasks.LastFraction);
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
        int targetCenter = FixedCoordinate.WholeOf(unchecked((int)newPosition));
        bool movingUp = displacement < 0;
        int targetEdge = movingUp
            ? CoordinateAxis.Wrap(targetCenter - slot.Radius.Y)
            : CoordinateAxis.Wrap(targetCenter + slot.Radius.Y - 1);

        int leftPixel = CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X);
        int rightPixel = CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1);
        int spanMinusOne = CoordinateAxis.Wrap(rightPixel - RoomGridConversions.BlockStart(leftPixel)) >> 4;
        // $A0:C7D0-$C7F0: (target row & $FF) * width plus the left column, then each
        // scanned column steps to the next native block index.
        int firstIndex = WordWrap.LowByte(targetEdge >> 4) * level.Size.X + (leftPixel >> 4);
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyVerticalProbeIsSolid(level, slot, EnemyMoverBlockIndex(firstIndex + scanIndex), new EnemyVerticalProbe { TargetCenter = new(slot.Position.X.Whole, targetCenter), TargetEdge = new(slot.Position.X.Whole, targetEdge), MovingUp = movingUp, Remaining = remaining, SpanMinusOne = spanMinusOne }))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.Position = slot.Position.WithY(FixedCoordinate.FromRaw(newPosition));
            return false;
        }

        // A non-square slope reactor may already have placed the center more precisely.
        // The native outer helper performs this conditional coarse alignment afterward.
        if (movingUp)
        {
            int aligned = CoordinateAxis.Wrap(RoomGridConversions.BlockEnd(targetEdge) + slot.Radius.Y + 1);
            if (aligned <= slot.Position.Y.Whole)
                slot.Position = slot.Position.WithWholeY(aligned);
            slot.Position = slot.Position.WithFractionY(0);
        }
        else
        {
            int aligned = CoordinateAxis.Wrap(RoomGridConversions.BlockStart(targetEdge) - slot.Radius.Y);
            if (aligned >= slot.Position.Y.Whole)
                slot.Position = slot.Position.WithWholeY(aligned);
            slot.Position = slot.Position.WithFractionY(RoomPixelMasks.LastFraction);
        }
        return true;
    }

    /// <summary>
    /// Ports the downward, geometry-only portion of $A0:BF8A used by Skree/Metaree. Unlike the common
    /// collision dispatcher, this routine deliberately tests only whether each block stops attachment.
    /// </summary>
    internal static bool EnemyMeetsAttachmentBlockBelow(
        RoomLevelData level,
        RoomEnemySlot slot,
        int unsignedDistance)
    {
        uint target = unchecked(slot.Position.Y.RawValue + (uint)unsignedDistance);
        int targetCenter = FixedCoordinate.WholeOf(unchecked((int)target));
        int targetEdge = CoordinateAxis.Wrap(targetCenter + slot.Radius.Y - 1);
        int row = targetEdge >> 4;
        int firstColumn = CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X) >> 4;
        int lastColumn = CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1) >> 4;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            if ((uint)column >= level.Size.X ||
                (uint)row >= level.Size.Y ||
                level.GetCollisionBlock(new(CoordinateAxis.Wrap(column), CoordinateAxis.Wrap(row))).Terrain.StopsAttachment)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Ports <c>EnemyFunc_BBBF</c> at $A0:BBBF. Unlike the ordinary horizontal collision
    /// mover, this look-ahead probe deliberately considers only whether each block stops attachment
    /// and does not resolve extension links or slope geometry.
    /// </summary>
    internal static bool EnemyMeetsAttachmentBlockHorizontallyAhead(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement)
    {
        uint position = slot.Position.X.RawValue;
        int targetCenter = FixedCoordinate.WholeOf(unchecked((int)(position + (uint)displacement)));
        bool movingLeft = displacement < 0;
        int targetEdge = movingLeft
            ? CoordinateAxis.Wrap(targetCenter - slot.Radius.X)
            : CoordinateAxis.Wrap(targetCenter + slot.Radius.X - 1);

        int topPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y);
        int bottomPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int firstRow = topPixel >> 4;
        int lastRow = bottomPixel >> 4;
        int column = targetEdge >> 4;
        for (int row = firstRow; row <= lastRow; row++)
        {
            if ((uint)column >= level.Size.X ||
                (uint)row >= level.Size.Y ||
                level.GetCollisionBlock(new(CoordinateAxis.Wrap(column), CoordinateAxis.Wrap(row))).Terrain.StopsAttachment)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Ports $A0:BC76's read-only vertical attachment probe. It does not move the
    /// actor or apply slope or extension behavior; Yard hiding and Stoke support checks
    /// use it to test attachment without consuming their look-ahead distance.
    /// </summary>
    internal static bool EnemyMeetsAttachmentBlockVerticallyAhead(RoomLevelData level, RoomEnemySlot slot, int displacement)
    {
        uint position = slot.Position.Y.RawValue;
        int center = FixedCoordinate.WholeOf(unchecked((int)(position + (uint)displacement)));
        int edge = displacement < 0
            ? CoordinateAxis.Wrap(center - slot.Radius.Y)
            : CoordinateAxis.Wrap(center + slot.Radius.Y - 1);
        int blockY = edge >> 4;
        int firstBlockX = CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X) >> 4;
        int lastBlockX = CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1) >> 4;
        if ((uint)blockY >= level.Size.Y) return true;
        for (int blockX = firstBlockX; blockX <= lastBlockX; blockX++)
        {
            if ((uint)blockX >= level.Size.X ||
                level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY))).Terrain.StopsAttachment)
                return true;
        }
        return false;
    }

    /// <summary>Classifies the next horizontal block, including square-slope quadrants and spike-block PLM reactions.</summary>
    private bool EnemyHorizontalProbeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        int startBlock,
        int targetEdge,
        int remaining,
        int spanMinusOne,
        bool treatNonSquareSlopesAsWalls)
    {
        RoomCollisionBlock block = ResolveEnemyMoverBlock(level, startBlock);
        int blockIndex = block.Index;
        return block.CollisionType switch
        {
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnusedAir or
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.Slope when block.Behavior.IsNonSquareSlope =>
                treatNonSquareSlopesAsWalls,
            RoomCollisionType.Slope => SquareHorizontalSlopeIsSolid(
                slot,
                targetEdge,
                block.Behavior,
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
                $"Enemy horizontal collision type {block.CollisionType} is invalid."),
        };
    }

    /// <summary>Classifies the next vertical block and may align the enemy against a non-square slope.</summary>
    private bool EnemyVerticalProbeIsSolid(RoomLevelData level, RoomEnemySlot slot, int startBlock, EnemyVerticalProbe probe)
    {
        RoomCollisionBlock block = ResolveEnemyMoverBlock(level, startBlock);
        int blockIndex = block.Index;
        return block.CollisionType switch
        {
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnusedAir or
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.Slope when block.Behavior.IsNonSquareSlope =>
                NonSquareVerticalSlopeIsSolid(
                    level,
                    slot,
                    block,
                    probe.TargetCenter.Y,
                    probe.MovingUp),
            RoomCollisionType.Slope => SquareVerticalSlopeIsSolid(
                slot,
                probe.TargetEdge.Y,
                block.Behavior,
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
                $"Enemy vertical collision type {block.CollisionType} is invalid."),
        };
    }

    /// <summary>
    /// The movers index <c>LevelData,X</c> with a 16-bit byte offset, so the block index
    /// wraps at $8000 blocks.
    /// </summary>
    private static int EnemyMoverBlockIndex(int index) => WordWrap.Unsigned(index << 1) >> 1;

    /// <summary>
    /// Reads a block for the bank-$A0 movers, which never bound the index to the authored
    /// room: past the room plane they read the blocks beyond the room (see
    /// <see cref="RoomLevelData.GetBlockBeyondRoom"/>). Extension links are followed
    /// as the native reaction dispatcher repeats them.
    /// </summary>
    private static RoomCollisionBlock ResolveEnemyMoverBlock(RoomLevelData level, int startBlock)
    {
        int blockIndex = startBlock;
        int maximumLinks = level.Size.X * level.Size.Y;
        for (int link = 0; link <= maximumLinks; link++)
        {
            if (!level.IsLogicalBlockIndex(blockIndex))
                return ReadEnemyMoverAllocationTail(level, blockIndex);
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            int offset = block.CollisionType switch
            {
                RoomCollisionType.HorizontalExtension when
                    block.Behavior != RoomBlockBehaviorValues.None => block.Behavior.ExtensionOffset,
                RoomCollisionType.VerticalExtension when
                    block.Behavior != RoomBlockBehaviorValues.None =>
                    block.Behavior.ExtensionOffset * level.Size.X,
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
            $"Enemy collision extension chain from block {startBlock} is cyclic.");
    }

    /// <summary>Reads a block beyond the authored room, rejecting kinds whose behaviour there is unbounded.</summary>
    private static RoomCollisionBlock ReadEnemyMoverAllocationTail(RoomLevelData level, int blockIndex)
    {
        RoomCollisionBlock block = level.GetBlockBeyondRoom(blockIndex);
        // Slopes, extensions and spike blocks read their behaviour, and a block's behaviour
        // beyond the authored room is unbounded state left by earlier rooms.
        if (block.CollisionType is RoomCollisionType.Slope or
            RoomCollisionType.HorizontalExtension or
            RoomCollisionType.VerticalExtension or
            RoomCollisionType.SpikeBlock)
        {
            throw new NotSupportedException(
                $"Enemy collision reached behaviour-dependent type {block.CollisionType} at " +
                $"block {blockIndex}, past the room plane; block behaviour there is unbounded.");
        }
        return block;
    }

    /// <summary>
    /// Resolves signed extension links exactly as the native dispatcher repeats them.
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
                    block.Behavior != RoomBlockBehaviorValues.None => block.Behavior.ExtensionOffset,
                RoomCollisionType.VerticalExtension when
                    block.Behavior != RoomBlockBehaviorValues.None =>
                    block.Behavior.ExtensionOffset * level.Size.X,
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
            $"Enemy collision extension chain from block ({start.X},{start.Y}) is cyclic.");
    }

    /// <summary>Tests the target half-tile quadrant and the enemy's vertical span against a square slope.</summary>
    private static bool SquareHorizontalSlopeIsSolid(
        RoomEnemySlot slot,
        int targetEdge,
        RoomBlockBehavior behavior,
        int remaining,
        int spanMinusOne)
    {
        int tableIndex = 4 * behavior.SlopeShape +
            (behavior.SlopeOrientation ^ ((targetEdge & 8) >> 3));
        if (remaining == 0)
        {
            if (((slot.Radius.Y + slot.Position.Y.Whole - 1) & 8) == 0)
                return SquareSlopes.EnemyQuadrantIsSolid(tableIndex);
        }
        else if (remaining == spanMinusOne &&
                 ((slot.Position.Y.Whole - slot.Radius.Y) & 8) != 0)
        {
            return SquareSlopes.EnemyQuadrantIsSolid(tableIndex ^ 2);
        }

        if (SquareSlopes.EnemyQuadrantIsSolid(tableIndex))
            return true;
        return SquareSlopes.EnemyQuadrantIsSolid(tableIndex ^ 2);
    }

    /// <summary>Tests the target half-tile quadrant and the enemy's horizontal span against a square slope.</summary>
    private static bool SquareVerticalSlopeIsSolid(
        RoomEnemySlot slot,
        int targetEdge,
        RoomBlockBehavior behavior,
        int remaining,
        int spanMinusOne)
    {
        int tableIndex = 4 * behavior.SlopeShape +
            (behavior.SlopeOrientation ^ ((targetEdge & 8) >> 2));
        if (remaining == 0)
        {
            if (((slot.Radius.X + slot.Position.X.Whole - 1) & 8) == 0)
                return SquareSlopes.EnemyQuadrantIsSolid(tableIndex);
        }
        else if (remaining == spanMinusOne &&
                 ((slot.Position.X.Whole - slot.Radius.X) & 8) != 0)
        {
            return SquareSlopes.EnemyQuadrantIsSolid(tableIndex ^ 1);
        }

        if (SquareSlopes.EnemyQuadrantIsSolid(tableIndex))
            return true;
        return SquareSlopes.EnemyQuadrantIsSolid(tableIndex ^ 1);
    }

    /// <summary>Checks slope height under the enemy center and snaps its Y position to the floor or underside.</summary>
    private static bool NonSquareVerticalSlopeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        RoomCollisionBlock block,
        int targetCenter,
        bool movingUp)
    {
        if (block.Index % level.Size.X != slot.Position.X.Whole >> 4)
            return false;

        RoomBlockBehavior behavior = block.Behavior;
        int xWithinBlock = behavior.SlopeFlipsHorizontally
            ? RoomGridConversions.OffsetFromBlockEnd(slot.Position.X.Whole)
            : RoomGridConversions.OffsetWithinBlock(slot.Position.X.Whole);
        int height = ReadNonSquareSlopeHeight(behavior, xWithinBlock);
        if (movingUp)
        {
            if (!behavior.SlopeFlipsVertically)
                return false;
            int edgeWithinBlock = RoomGridConversions.OffsetFromBlockEnd(targetCenter - slot.Radius.Y);
            int adjustment = height - edgeWithinBlock - 1;
            if (adjustment > 0)
                return false;
            slot.Position = slot.Position.WithY(new(CoordinateAxis.Wrap(targetCenter - adjustment), 0));
            return true;
        }

        if (behavior.SlopeFlipsVertically)
            return false;
        int bottomWithinBlock = RoomGridConversions.OffsetWithinBlock(slot.Radius.Y + targetCenter - 1);
        int downwardAdjustment = height - bottomWithinBlock - 1;
        if (height - bottomWithinBlock != 1 && downwardAdjustment >= 0)
            return false;
        slot.Position = slot.Position.WithY(new(CoordinateAxis.Wrap(targetCenter + downwardAdjustment), RoomPixelMasks.LastFraction));
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
            new(slot.Position.X.Whole, CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1)),
            surface: SlopeSurface.Top);
        bool adjustedCeiling = AlignAgainstSlopeAtPixel(
            level,
            slot,
            new(slot.Position.X.Whole, CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y)),
            surface: SlopeSurface.Underside);
        return adjustedFloor || adjustedCeiling;
    }

    /// <summary>Adjusts the enemy center when the sampled floor or ceiling pixel penetrates a matching non-square slope.</summary>
    internal static bool AlignAgainstSlopeAtPixel(
        RoomLevelData level,
        RoomEnemySlot slot,
        Position16<RoomPixels> pixel,
        SlopeSurface surface)
    {
        int blockX = pixel.X >> 4;
        int blockY = pixel.Y >> 4;
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
        if (block.CollisionType != RoomCollisionType.Slope ||
            !block.Behavior.IsNonSquareSlope)
            return false;
        if ((surface == SlopeSurface.Underside) != block.Behavior.SlopeFlipsVertically)
            return false;

        int xWithinBlock = block.Behavior.SlopeFlipsHorizontally
            ? RoomGridConversions.OffsetFromBlockEnd(pixel.X)
            : RoomGridConversions.OffsetWithinBlock(pixel.X);
        int height = ReadNonSquareSlopeHeight(block.Behavior, xWithinBlock);
        int edgeWithinBlock = surface == SlopeSurface.Underside
            ? RoomGridConversions.OffsetFromBlockEnd(pixel.Y)
            : RoomGridConversions.OffsetWithinBlock(pixel.Y);
        int adjustment = height - edgeWithinBlock - 1;
        if (adjustment >= 0)
            return false;
        slot.Position = slot.Position.WithWholeY(surface == SlopeSurface.Underside
            ? CoordinateAxis.Wrap(slot.Position.Y.Whole - adjustment)
            : CoordinateAxis.Wrap(slot.Position.Y.Whole + adjustment));
        return true;
    }

    /// <summary>Looks up the authored height of a slope shape at an already oriented block-local X.</summary>
    internal static int ReadNonSquareSlopeHeight(RoomBlockBehavior behavior, int xWithinBlock) =>
        SlopeHeights.Read(behavior.SlopeShape, xWithinBlock);
    /// <summary>
    /// The active frame's room PLM owner for enemy-breakable blocks. This synchronous call
    /// context is not persisted; every enemy frame supplies it again after debugger restoration.
    /// </summary>
    [NonSerialized] private EnemyBreakableTerrainPlms? _collisionPlms;

    /// <summary>Spawns the breakable-block PLM for an enemy spike reaction and clears collision even if its pool is full.</summary>
    internal bool ReactToEnemySpikeBlock(RoomLevelData level, int blockIndex, RoomCollisionBlock block)
    {
        if (!EnemyBreakableTerrain.IsEnemyBreakable(block.Behavior)) return true;
        var plms = _collisionPlms ?? throw new InvalidOperationException("Enemy-breakable terrain requires the active room PLM owner.");
        plms.TrySpawnEnemyBreakableBlock(level, blockIndex);
        return false; // $A0:C2D6 clears carry even if the PLM pool was full.
    }

    /// <summary>Supplies the active room PLM owner to <see cref="EnemyTerrain"/> for one enemy frame.</summary>
    internal readonly struct EnemyTerrainScope : IDisposable
    {
        /// <summary>Terrain owner whose active PLM context is temporarily replaced.</summary>
        private readonly EnemyTerrain owner;
        /// <summary>PLM context restored when the enemy frame leaves this scope.</summary>
        private readonly EnemyBreakableTerrainPlms? previous;
        /// <summary>Installs the frame's room PLM owner while retaining the previous context.</summary>
        public EnemyTerrainScope(EnemyTerrain owner, EnemyBreakableTerrainPlms? current)
        {
            this.owner = owner;
            previous = owner._collisionPlms;
            owner._collisionPlms = current;
        }
        /// <summary>Restores the terrain owner's prior PLM context.</summary>
        public void Dispose() => owner._collisionPlms = previous;
    }
}
