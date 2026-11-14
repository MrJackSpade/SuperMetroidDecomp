using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy movement against room terrain: horizontal and vertical steps with square and
/// non-square slopes, solid-ahead probes, and the spike/enemy-breakable block reaction that spawns its room object
/// through the active room-object owner supplied for each enemy frame.
/// </summary>
[RoomLifetime]
internal sealed class EnemyTerrain
{
    /// <summary>Moves the enemy horizontally, ignoring non-square slopes.</summary>
    internal bool MoveEnemyHorizontallyIgnoringNonSquareSlopes(
        RoomLevelData level,
        RoomEnemySlot slot,
        SignedSixteenSixteen displacement) =>
        MoveEnemyHorizontally(
            level,
            slot,
            displacement,
            slopes: NonSquareSlopes.Ignored);

    /// <summary>
    /// Moves the enemy horizontally, treating non-square slopes as walls. Square slopes
    /// retain their half-block geometry; only non-square slopes become solid walls.
    /// </summary>
    internal bool MoveEnemyHorizontallyTreatingSlopesAsWalls(
        RoomLevelData level,
        RoomEnemySlot slot,
        SignedSixteenSixteen displacement) =>
        MoveEnemyHorizontally(
            level,
            slot,
            displacement,
            slopes: NonSquareSlopes.Walls);

    /// <summary>
    /// Shared body of the three horizontal enemy movers. The remaining
    /// process-slopes mode will reuse this seam when its first retail caller is modelled.
    /// </summary>
    private bool MoveEnemyHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        SignedSixteenSixteen displacement,
        NonSquareSlopes slopes)
    {
        if (displacement.IsZero)
            return false;

        FixedCoordinate newPosition = slot.Position.X.MovedBy(displacement);
        int targetCenter = newPosition.Whole;
        bool movingLeft = displacement.IsNegative;
        int targetEdge = movingLeft
            ? CoordinateAxis.Wrap(targetCenter - slot.Radius.X)
            : CoordinateAxis.Wrap(targetCenter + slot.Radius.X - 1);

        int topPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y);
        int bottomPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int spanMinusOne = RoomGridConversions.WholeBlocksIn(CoordinateAxis.Wrap(bottomPixel - RoomGridConversions.BlockStart(topPixel)));
        // The scan walks down the target column from the top row, as the block scan cycle counts it.
        Position16<RoomBlocks> firstBlock = Position16<RoomPixels>.Of(targetEdge, topPixel).ToRoomBlock();
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyHorizontalProbeIsSolid(
                    level,
                    slot,
                    BlockScanCycle.BlockDownRows(firstBlock, level.Size.X, scanIndex),
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
            slot.Position = slot.Position.WithX(newPosition);
            return false;
        }

        // Both directions compare against the pre-move position before accepting the aligned
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
            slot.Position = slot.Position.WithFractionX(FixedCoordinate.LastFraction);
        }
        return true;
    }

    /// <summary>Moves the enemy vertically.</summary>
    internal bool MoveEnemyVertically(
        RoomLevelData level,
        RoomEnemySlot slot,
        SignedSixteenSixteen displacement)
    {
        if (displacement.IsZero)
            return false;

        FixedCoordinate newPosition = slot.Position.Y.MovedBy(displacement);
        int targetCenter = newPosition.Whole;
        bool movingUp = displacement.IsNegative;
        int targetEdge = movingUp
            ? CoordinateAxis.Wrap(targetCenter - slot.Radius.Y)
            : CoordinateAxis.Wrap(targetCenter + slot.Radius.Y - 1);

        int leftPixel = CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X);
        int rightPixel = CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1);
        int spanMinusOne = RoomGridConversions.WholeBlocksIn(CoordinateAxis.Wrap(rightPixel - RoomGridConversions.BlockStart(leftPixel)));
        // The scan walks along the target row from the left column, as the block scan cycle counts it.
        Position16<RoomBlocks> firstBlock = Position16<RoomPixels>.Of(leftPixel, targetEdge).ToRoomBlock();
        bool collided = false;
        for (int scanIndex = 0; scanIndex <= spanMinusOne; scanIndex++)
        {
            int remaining = spanMinusOne - scanIndex;
            if (EnemyVerticalProbeIsSolid(level, slot, BlockScanCycle.BlockAlongRows(firstBlock, level.Size.X, scanIndex), new EnemyVerticalProbe { TargetCenter = new(slot.Position.X.Whole, targetCenter), TargetEdge = new(slot.Position.X.Whole, targetEdge), MovingUp = movingUp, Remaining = remaining, SpanMinusOne = spanMinusOne }))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.Position = slot.Position.WithY(newPosition);
            return false;
        }

        // A non-square slope reactor may already have placed the center more precisely.
        // The outer helper performs this conditional coarse alignment afterward.
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
            slot.Position = slot.Position.WithFractionY(FixedCoordinate.LastFraction);
        }
        return true;
    }

    /// <summary>
    /// Implements the downward, geometry-only attachment probe used by Skree/Metaree. Unlike the common
    /// collision dispatcher, this probe deliberately tests only whether each block stops attachment.
    /// </summary>
    internal static bool EnemyMeetsAttachmentBlockBelow(
        RoomLevelData level,
        RoomEnemySlot slot,
        SignedSixteenSixteen distance)
    {
        int targetCenter = slot.Position.Y.MovedBy(distance).Whole;
        int targetEdge = CoordinateAxis.Wrap(targetCenter + slot.Radius.Y - 1);
        int row = RoomGridConversions.PixelToBlock(targetEdge);
        int firstColumn = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X));
        int lastColumn = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1));
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
    /// Tests the attachment block horizontally ahead. Unlike the ordinary horizontal collision
    /// mover, this look-ahead probe deliberately considers only whether each block stops attachment
    /// and does not resolve extension links or slope geometry.
    /// </summary>
    internal static bool EnemyMeetsAttachmentBlockHorizontallyAhead(
        RoomLevelData level,
        RoomEnemySlot slot,
        SignedSixteenSixteen displacement)
    {
        int targetCenter = slot.Position.X.MovedBy(displacement).Whole;
        bool movingLeft = displacement.IsNegative;
        int targetEdge = movingLeft
            ? CoordinateAxis.Wrap(targetCenter - slot.Radius.X)
            : CoordinateAxis.Wrap(targetCenter + slot.Radius.X - 1);

        int topPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y);
        int bottomPixel = CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int firstRow = RoomGridConversions.PixelToBlock(topPixel);
        int lastRow = RoomGridConversions.PixelToBlock(bottomPixel);
        int column = RoomGridConversions.PixelToBlock(targetEdge);
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
    /// Implements the read-only vertical attachment probe. It does not move the
    /// actor or apply slope or extension behavior; the Yard's hiding check uses it to test
    /// attachment without consuming its look-ahead distance.
    /// </summary>
    internal static bool EnemyMeetsAttachmentBlockVerticallyAhead(RoomLevelData level, RoomEnemySlot slot, SignedSixteenSixteen displacement)
    {
        int center = slot.Position.Y.MovedBy(displacement).Whole;
        int edge = displacement.IsNegative
            ? CoordinateAxis.Wrap(center - slot.Radius.Y)
            : CoordinateAxis.Wrap(center + slot.Radius.Y - 1);
        int blockY = RoomGridConversions.PixelToBlock(edge);
        int firstBlockX = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X));
        int lastBlockX = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1));
        if ((uint)blockY >= level.Size.Y) return true;
        for (int blockX = firstBlockX; blockX <= lastBlockX; blockX++)
        {
            if ((uint)blockX >= level.Size.X ||
                level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY))).Terrain.StopsAttachment)
                return true;
        }
        return false;
    }

    /// <summary>Classifies the next horizontal block, including square-slope quadrants and spike-block room-object reactions.</summary>
    private bool EnemyHorizontalProbeIsSolid(
        RoomLevelData level,
        RoomEnemySlot slot,
        Position16<RoomBlocks> startBlock,
        int targetEdge,
        int remaining,
        int spanMinusOne,
        bool treatNonSquareSlopesAsWalls)
    {
        RoomCollisionBlock block = ResolveEnemyMoverBlock(level, startBlock, out Position16<RoomBlocks> reached);
        return block.CollisionType switch
        {
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnrevealedAir or
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.Slope when block.Behavior.Slope.IsNonSquare =>
                treatNonSquareSlopesAsWalls,
            RoomCollisionType.Slope => SquareHorizontalSlopeIsSolid(
                slot,
                targetEdge,
                block.Behavior,
                remaining,
                spanMinusOne),
            RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension =>
                false, // A zero-offset extension resolves to air.
            RoomCollisionType.SpikeBlock => ReactToEnemySpikeBlock(level, reached, block),
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
    private bool EnemyVerticalProbeIsSolid(RoomLevelData level, RoomEnemySlot slot, Position16<RoomBlocks> startBlock, EnemyVerticalProbe probe)
    {
        RoomCollisionBlock block = ResolveEnemyMoverBlock(level, startBlock, out Position16<RoomBlocks> reached);
        return block.CollisionType switch
        {
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.ShootableAir or
            RoomCollisionType.UnrevealedAir or
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.Slope when block.Behavior.Slope.IsNonSquare =>
                NonSquareVerticalSlopeIsSolid(
                    slot,
                    reached,
                    block.Behavior,
                    probe.TargetCenter.Y,
                    probe.MovingUp),
            RoomCollisionType.Slope => SquareVerticalSlopeIsSolid(
                slot,
                probe.TargetEdge.Y,
                block.Behavior,
                probe.Remaining,
                probe.SpanMinusOne),
            RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => false,
            RoomCollisionType.SpikeBlock => ReactToEnemySpikeBlock(level, reached, block),
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
    /// Reads a block for the movers, which never bound the block their scan reaches to the authored
    /// room: past the room plane they read the blocks beyond the room (see
    /// <see cref="RoomLevelData.GetBlockBeyondRoom"/>). Extension links are followed
    /// as the reaction dispatcher repeats them, each counted by <see cref="BlockScanCycle.LinkedBlock"/>.
    /// </summary>
    /// <param name="level">The room's terrain.</param>
    /// <param name="startBlock">The block the scan reached, in the room's columns (<see cref="BlockScanCycle"/>).</param>
    /// <param name="reached">The block the extension links lead to, where a reaction to it takes place.</param>
    /// <returns>The terrain and behaviour of <paramref name="reached"/>.</returns>
    internal static RoomCollisionBlock ResolveEnemyMoverBlock(
        RoomLevelData level,
        Position16<RoomBlocks> startBlock,
        out Position16<RoomBlocks> reached)
    {
        reached = startBlock;
        for (int link = 0; link <= level.Size.Area; link++)
        {
            if (!level.Size.Contains(reached))
                return ReadEnemyMoverAllocationTail(level, reached);
            RoomCollisionBlock block = level.GetCollisionBlock(reached);
            if (ExtensionLink(block) is not { } extension)
                return block;
            reached = BlockScanCycle.LinkedBlock(reached, extension, level.Size.X);
        }

        throw new InvalidDataException(
            $"Enemy collision extension chain from block {startBlock} is cyclic.");
    }

    /// <summary>
    /// The columns a horizontal extension or the rows a vertical one leads an enemy's lookup on by, as the reaction
    /// dispatcher repeats them; none for an extension that ends its chain or a block of any other kind.
    /// </summary>
    private static Offset16<RoomBlocks>? ExtensionLink(RoomCollisionBlock block) => block.CollisionType switch
    {
        RoomCollisionType.HorizontalExtension when block.Behavior.EndsExtension is false =>
            Offset16<RoomBlocks>.Of(block.Behavior.ExtensionOffset, 0),
        RoomCollisionType.VerticalExtension when block.Behavior.EndsExtension is false =>
            Offset16<RoomBlocks>.Of(0, block.Behavior.ExtensionOffset),
        RoomCollisionType.Air or RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or
            RoomCollisionType.HorizontalExtension or RoomCollisionType.UnrevealedAir or
            RoomCollisionType.BombableAir or RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or RoomCollisionType.VerticalExtension or
            RoomCollisionType.GrappleBlock or RoomCollisionType.BombableBlock => null,
        _ => throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}."),
    };

    /// <summary>Reads a block beyond the authored room, rejecting kinds whose behaviour there is unbounded.</summary>
    private static RoomCollisionBlock ReadEnemyMoverAllocationTail(RoomLevelData level, Position16<RoomBlocks> beyond)
    {
        RoomCollisionBlock block = level.GetBlockBeyondRoom(beyond);
        // Slopes, extensions and spike blocks read their behaviour, and a block's behaviour
        // beyond the authored room is unbounded state left by earlier rooms.
        if (block.CollisionType is RoomCollisionType.Slope or
            RoomCollisionType.HorizontalExtension or
            RoomCollisionType.VerticalExtension or
            RoomCollisionType.SpikeBlock)
        {
            throw new NotSupportedException(
                $"Enemy collision reached behaviour-dependent type {block.CollisionType} at " +
                $"block {beyond}, past the room plane; block behaviour there is unbounded.");
        }
        return block;
    }

    /// <summary>
    /// Resolves signed extension links exactly as the dispatcher repeats them: each link leads on in the room's reading
    /// order (<see cref="RoomReadingOrder"/>), so a horizontal link past a row's end continues on the next row.
    /// A guard converts corrupt cyclic room data into a useful exception instead of a runaway
    /// recursion failure; valid chains always terminate well before the room allocation size.
    /// </summary>
    /// <returns>The block the chain ends on, or null when the start or a link lies outside the room's grid.</returns>
    internal static RoomCollisionBlock? ResolveEnemyCollisionBlock(
        RoomLevelData level,
        Position16<RoomBlocks> start)
    {
        Position16<RoomBlocks>? reached = start;
        for (int link = 0; link <= level.Size.Area; link++)
        {
            if (reached is not { } block || !level.Size.Contains(block))
                return null;
            RoomCollisionBlock collision = level.GetCollisionBlock(block);
            if (ExtensionLink(collision) is not { } extension)
                return collision;
            reached = RoomReadingOrder.Step(block, extension, level.Size.X);
        }

        throw new InvalidDataException(
            $"Enemy collision extension chain from block ({start.X},{start.Y}) is cyclic.");
    }

    /// <summary>Tests the target half-block quadrant and the enemy's vertical span against a square slope.</summary>
    private static bool SquareHorizontalSlopeIsSolid(
        RoomEnemySlot slot,
        int targetEdge,
        RoomBlockBehavior behavior,
        int remaining,
        int spanMinusOne)
    {
        SquareSlopeShape shape = SquareSlopes.ShapeOf(behavior);
        SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(behavior, SquareSlopeQuadrant.ColumnFor((targetEdge & 8) != 0), SquareSlopeRowHalf.Top);
        if (remaining == 0)
        {
            if (((slot.Radius.Y + slot.Position.Y.Whole - 1) & 8) == 0)
                return SquareSlopes.IsSolid(shape, quadrant);
        }
        else if (remaining == spanMinusOne &&
                 ((slot.Position.Y.Whole - slot.Radius.Y) & 8) != 0)
        {
            return SquareSlopes.IsSolid(shape, quadrant.OtherRow);
        }

        if (SquareSlopes.IsSolid(shape, quadrant))
            return true;
        return SquareSlopes.IsSolid(shape, quadrant.OtherRow);
    }

    /// <summary>Tests the target half-block quadrant and the enemy's horizontal span against a square slope.</summary>
    private static bool SquareVerticalSlopeIsSolid(
        RoomEnemySlot slot,
        int targetEdge,
        RoomBlockBehavior behavior,
        int remaining,
        int spanMinusOne)
    {
        SquareSlopeShape shape = SquareSlopes.ShapeOf(behavior);
        SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(behavior, SquareSlopeColumnHalf.Left, SquareSlopeQuadrant.RowFor((targetEdge & 8) != 0));
        if (remaining == 0)
        {
            if (((slot.Radius.X + slot.Position.X.Whole - 1) & 8) == 0)
                return SquareSlopes.IsSolid(shape, quadrant);
        }
        else if (remaining == spanMinusOne &&
                 ((slot.Position.X.Whole - slot.Radius.X) & 8) != 0)
        {
            return SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
        }

        if (SquareSlopes.IsSolid(shape, quadrant))
            return true;
        return SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
    }

    /// <summary>Checks slope height under the enemy center and snaps its Y position to the floor or underside.</summary>
    /// <param name="slot">The enemy moving.</param>
    /// <param name="block">The slope block the enemy's vertical move reached, in the room's columns.</param>
    /// <param name="behavior">The slope block's behaviour.</param>
    /// <param name="targetCenter">The enemy's vertical center after the move.</param>
    /// <param name="movingUp">Whether the enemy moves up.</param>
    private static bool NonSquareVerticalSlopeIsSolid(
        RoomEnemySlot slot,
        Position16<RoomBlocks> block,
        RoomBlockBehavior behavior,
        int targetCenter,
        bool movingUp)
    {
        // Only the slope block in the enemy center's column shapes the move; the others the scan reaches are passed.
        if (block.X != RoomGridConversions.PixelToBlock(slot.Position.X.Whole))
            return false;

        int xWithinBlock = behavior.Slope.FlipsHorizontally
            ? RoomGridConversions.OffsetFromBlockEnd(slot.Position.X.Whole)
            : RoomGridConversions.OffsetWithinBlock(slot.Position.X.Whole);
        int height = ReadNonSquareSlopeHeight(behavior, xWithinBlock);
        if (movingUp)
        {
            if (!behavior.Slope.FlipsVertically)
                return false;
            int edgeWithinBlock = RoomGridConversions.OffsetFromBlockEnd(targetCenter - slot.Radius.Y);
            int adjustment = height - edgeWithinBlock - 1;
            if (adjustment > 0)
                return false;
            slot.Position = slot.Position.WithY(new(CoordinateAxis.Wrap(targetCenter - adjustment), 0));
            return true;
        }

        if (behavior.Slope.FlipsVertically)
            return false;
        int bottomWithinBlock = RoomGridConversions.OffsetWithinBlock(slot.Radius.Y + targetCenter - 1);
        int downwardAdjustment = height - bottomWithinBlock - 1;
        if (height - bottomWithinBlock != 1 && downwardAdjustment >= 0)
            return false;
        slot.Position = slot.Position.WithY(new(CoordinateAxis.Wrap(targetCenter + downwardAdjustment), FixedCoordinate.LastFraction));
        return true;
    }

    /// <summary>Aligns the enemy's Y position with a non-square slope.</summary>
    internal static void AlignEnemyYWithNonSquareSlope(RoomLevelData level, RoomEnemySlot slot) =>
        AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);

    /// <summary>
    /// Runs the same two probes as <see cref="AlignEnemyYWithNonSquareSlope"/> while reporting
    /// whether the slope alignment moved the enemy. Yard is the first caller that
    /// uses that report to suppress corner-transition animation after a slope adjustment.
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
        int blockX = RoomGridConversions.PixelToBlock(pixel.X);
        int blockY = RoomGridConversions.PixelToBlock(pixel.Y);
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
        if (block.CollisionType != RoomCollisionType.Slope ||
            !block.Behavior.Slope.IsNonSquare)
            return false;
        if ((surface == SlopeSurface.Underside) != block.Behavior.Slope.FlipsVertically)
            return false;

        int xWithinBlock = block.Behavior.Slope.FlipsHorizontally
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
        SlopeHeights.Read(behavior.Slope.Shape, xWithinBlock);
    /// <summary>
    /// The active frame's room-object owner for enemy-breakable blocks. This synchronous call
    /// context belongs to one frame; every enemy frame supplies it again, including after a saved game state is restored.
    /// </summary>
    private EnemyBreakableTerrainObjects? _collisionObjects;

    /// <summary>Spawns the breakable-block room object for an enemy spike reaction and clears collision even if its pool is full.</summary>
    /// <param name="level">The room's terrain.</param>
    /// <param name="position">The spike block the enemy's lookup reached.</param>
    /// <param name="block">The spike block's terrain and behaviour.</param>
    internal bool ReactToEnemySpikeBlock(RoomLevelData level, Position16<RoomBlocks> position, RoomCollisionBlock block)
    {
        if (!EnemyBreakableTerrain.IsEnemyBreakable(block.Behavior)) return true;
        var roomObjects = _collisionObjects ?? throw new InvalidOperationException("Enemy-breakable terrain requires the active room-object owner.");
        roomObjects.TrySpawnEnemyBreakableBlock(level, position);
        return false; // The block reaction reports no collision even if the room-object pool was full.
    }

    /// <summary>Supplies the active room-object owner to <see cref="EnemyTerrain"/> for one enemy frame.</summary>
    internal readonly struct EnemyTerrainScope : IDisposable
    {
        /// <summary>Terrain owner whose active room-object context is temporarily replaced.</summary>
        private readonly EnemyTerrain owner;
        /// <summary>Room-object context restored when the enemy frame leaves this scope.</summary>
        private readonly EnemyBreakableTerrainObjects? previous;
        /// <summary>Installs the frame's room-object owner while retaining the previous context.</summary>
        public EnemyTerrainScope(EnemyTerrain owner, EnemyBreakableTerrainObjects? current)
        {
            this.owner = owner;
            previous = owner._collisionObjects;
            owner._collisionObjects = current;
        }
        /// <summary>Restores the terrain owner's prior room-object context.</summary>
        public void Dispose() => owner._collisionObjects = previous;
    }
}
