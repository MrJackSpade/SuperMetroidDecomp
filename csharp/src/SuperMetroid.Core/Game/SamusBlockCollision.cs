using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Incremental port of Samus's room-block movement scans in bank $94.
/// </summary>
/// <remarks>
/// Supported dispatcher categories are deliberately explicit: type 0 air, type 1
/// non-square slopes, and ordinary solid-family types $8/$C/$E. Other types have PLM,
/// damage, door, extension, or square-slope behavior and throw instead of being flattened
/// to “air” or “solid.”
/// </remarks>
public static class SamusBlockCollision
{
    // kTab948E54 at $94:8E54 describes the solid/empty 8x8 quadrants of square slope
    // shapes zero through four. Each shape owns four bytes; BTS bits 6-7 rotate/mirror the
    // selected quadrant before the leading-boundary half chooses its neighbor.
    private static ReadOnlySpan<byte> SquareSlopeQuadrantSolidity =>
    [
        0x00, 0x00, 0x80, 0x80,
        0x00, 0x80, 0x00, 0x80,
        0x00, 0x00, 0x00, 0x80,
        0x00, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80,
    ];

    /// <summary>
    /// Ports <c>BlockColl_Handle_Horiz</c> at <c>$94:9543</c>, the position addition in
    /// <c>Samus_MoveRight_NoSolidColl</c>, and the subsequent non-square slope alignment.
    /// </summary>
    public static BlockMoveResult MoveHorizontal(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusKinematicsState state,
        int displacement)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(state);

        int acceptedDisplacement = displacement;
        bool collided = false;
        RoomCollisionBlock? collisionBlock = null;

        if (acceptedDisplacement != 0)
        {
            // The target center and leading boundary are calculated once, before a slope
            // handler is allowed to scale ci_r18_r20. This slightly surprising ordering is
            // exactly how $94:9543 initializes its collision-info scratch words.
            ushort targetCenter = unchecked((ushort)(
                unchecked(state.XFixed + (uint)acceptedDisplacement) >> 16));
            ushort leadingBoundary = acceptedDisplacement >= 0
                ? unchecked((ushort)(state.XRadius + targetCenter - 1))
                : unchecked((ushort)(targetCenter - state.XRadius));

            int topBlockY = unchecked((ushort)(state.YPosition - state.YRadius)) >> 4;
            int verticalSpan = GetVerticalBlockSpan(state);
            int blockX = leadingBoundary >> 4;

            for (int rowOffset = 0; rowOffset <= verticalSpan; rowOffset++)
            {
                RoomCollisionBlock block = GetRequiredBlock(level, blockX, topBlockY + rowOffset);
                switch (block.CollisionType)
                {
                    case 0:
                        break;

                    case 1:
                        if ((block.Behavior & 0x1f) < 5)
                        {
                            bool squareCollision = ReactHorizontalSquareSlope(
                                state,
                                block,
                                acceptedDisplacement,
                                leadingBoundary,
                                remainingRows: verticalSpan - rowOffset,
                                totalRows: verticalSpan,
                                out int clippedDisplacement);
                            if (squareCollision)
                            {
                                acceptedDisplacement = clippedDisplacement;
                                collided = true;
                                collisionBlock = block;
                            }
                            break;
                        }

                        acceptedDisplacement = SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(
                            bus,
                            block.Behavior,
                            acceptedDisplacement,
                            state.VerticalSpeedFixed);
                        break;

                    case 8:
                    case 12:
                    case 14:
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    default:
                        throw Unsupported(block, "horizontal dispatcher");
                }

                if (collided)
                    break;
            }
        }

        state.SetXFixed(unchecked(state.XFixed + (uint)acceptedDisplacement));

        // $90:9350/$90:93B1 call $94:87F4 after both collision and non-collision paths.
        SlopeAlignmentResult alignment = SamusSlopePhysics.AlignYPosition(
            bus,
            level,
            state.XPosition,
            state.YPosition,
            state.YRadius,
            horizontalSlopeCollisionEnabled: (state.HorizontalSlopeCollisionEnable & 2) != 0);
        state.YPosition = alignment.YPosition;
        state.PositionAdjustedBySlope = alignment.Adjusted;

        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            collisionBlock,
            alignment.Adjusted,
            alignment.FloorBlock,
            alignment.CeilingBlock);
    }

    /// <summary>
    /// Ports the left-to-right/right-to-left vertical scans at <c>$94:959E/$94:95F5</c>
    /// plus <c>Samus_MoveDown_NoSolidColl</c>'s final fixed-point position addition.
    /// </summary>
    public static BlockMoveResult MoveVertical(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusKinematicsState state,
        int displacement,
        bool scanLeftToRight)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(state);

        int acceptedDisplacement = displacement;
        bool collided = false;
        RoomCollisionBlock? collisionBlock = null;
        state.PositionAdjustedBySlope = false;

        if (acceptedDisplacement != 0)
        {
            ushort targetCenter = unchecked((ushort)(
                unchecked(state.YFixed + (uint)acceptedDisplacement) >> 16));
            ushort leadingBoundary = acceptedDisplacement >= 0
                ? unchecked((ushort)(state.YRadius + targetCenter - 1))
                : unchecked((ushort)(targetCenter - state.YRadius));
            int blockY = leadingBoundary >> 4;
            int horizontalSpan = GetHorizontalBlockSpan(state);
            int firstBlockX = scanLeftToRight
                ? unchecked((ushort)(state.XPosition - state.XRadius)) >> 4
                : unchecked((ushort)(state.XPosition + state.XRadius - 1)) >> 4;

            for (int offset = 0; offset <= horizontalSpan; offset++)
            {
                int blockX = scanLeftToRight ? firstBlockX + offset : firstBlockX - offset;
                RoomCollisionBlock block = GetRequiredBlock(level, blockX, blockY);
                switch (block.CollisionType)
                {
                    case 0:
                        break;

                    case 1:
                        if ((block.Behavior & 0x1f) < 5)
                        {
                            bool squareCollision = ReactVerticalSquareSlope(
                                state,
                                block,
                                acceptedDisplacement,
                                leadingBoundary,
                                scanOffset: offset,
                                totalColumns: horizontalSpan,
                                out int clippedDisplacement);
                            if (squareCollision)
                            {
                                acceptedDisplacement = clippedDisplacement;
                                collided = true;
                                collisionBlock = block;
                            }
                            break;
                        }

                        (acceptedDisplacement, collided) = ClipVerticalToNonSquareSlope(
                            bus,
                            state,
                            block,
                            blockX,
                            acceptedDisplacement,
                            targetCenter);
                        if (collided)
                            collisionBlock = block;
                        break;

                    case 8:
                    case 12:
                    case 14:
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    default:
                        throw Unsupported(block, "vertical dispatcher");
                }

                if (collided)
                    break;
            }
        }

        state.SetYFixed(unchecked(state.YFixed + (uint)acceptedDisplacement));
        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            collisionBlock,
            state.PositionAdjustedBySlope,
            FloorSlopeBlock: null,
            CeilingSlopeBlock: null);
    }

    private static (int Displacement, bool Collided) ClipVerticalToNonSquareSlope(
        ISnesAddressSpace bus,
        SamusKinematicsState state,
        RoomCollisionBlock block,
        int blockX,
        int displacement,
        ushort targetCenter)
    {
        bool movingDown = displacement >= 0;

        // Non-square slopes react vertically only under Samus's center column. The broader
        // X-radius scan still visits neighboring blocks, but $94:86FE rejects them here.
        if ((state.XPosition >> 4) != blockX)
            return (displacement, false);

        if (movingDown)
        {
            if ((block.Behavior & 0x80) != 0)
                return (displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(bus, block.Behavior, state.XPosition);
            int bottomNibble = unchecked((ushort)(state.YRadius + targetCenter - 1)) & 0x0f;
            short correction = unchecked((short)(height - bottomNibble - 1));
            if (correction > 0)
                return (displacement, false);

            int whole = unchecked((short)(displacement >> 16)) + correction;
            if (whole < 0)
                whole = 0;
            return (whole << 16, true);
        }
        else
        {
            if ((block.Behavior & 0x80) == 0)
                return (displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(bus, block.Behavior, state.XPosition);
            int invertedTopNibble = ((targetCenter - state.YRadius) & 0x0f) ^ 0x0f;
            short correction = unchecked((short)(height - invertedTopNibble - 1));
            if (correction > 0)
                return (displacement, false);

            int whole = unchecked((short)(displacement >> 16)) + correction;
            if (whole < 0)
                whole = 0;
            return (whole << 16, true);
        }
    }

    /// <summary>
    /// Ports <c>BlockColl_Horiz_Slope_Square</c> at <c>$94:8D2B</c>.
    /// </summary>
    private static bool ReactHorizontalSquareSlope(
        SamusKinematicsState state,
        RoomCollisionBlock block,
        int displacement,
        ushort leadingBoundary,
        int remainingRows,
        int totalRows,
        out int clippedDisplacement)
    {
        int shape = block.Behavior & 0x1f;
        int orientation = block.Behavior >> 6;

        // ci_r32 is the leading X boundary. Its bit three selects the left/right 8-pixel
        // half; BTS orientation occupies the same two-bit quadrant coordinate. XOR is
        // literal native indexing, not a geometric simplification.
        int quadrant = 4 * shape + (orientation ^ ((leadingBoundary & 8) >> 3));
        bool selectedSolid = SquareSlopeQuadrantSolidity[quadrant] != 0;
        bool collide;

        if (remainingRows == 0)
        {
            // On the bottom scanned block, the body's bottom half can accept the selected
            // quadrant directly. Otherwise the routine falls through to the opposite
            // vertical quadrant test below.
            if (((state.YRadius + state.YPosition - 1) & 8) == 0 && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopeQuadrantSolidity[quadrant ^ 2] != 0;
        }
        else
        {
            // The first scanned row is remainingRows==totalRows; intermediate rows always
            // test the selected quadrant. The top-body bit supplies the native exception on
            // that first row before the opposite vertical quadrant is consulted.
            if (remainingRows != totalRows || ((state.YPosition - state.YRadius) & 8) == 0)
            {
                if (selectedSolid)
                {
                    collide = true;
                    goto Clip;
                }
            }

            collide = SquareSlopeQuadrantSolidity[quadrant ^ 2] != 0;
        }

        if (!collide)
        {
            clippedDisplacement = displacement;
            return false;
        }

    Clip:
        clippedDisplacement = ClipHorizontalToSquareSlope(
            state,
            displacement,
            leadingBoundary);
        return true;
    }

    /// <summary>
    /// Ports <c>BlockColl_Vert_Slope_Square</c> at <c>$94:8DBD</c>.
    /// </summary>
    private static bool ReactVerticalSquareSlope(
        SamusKinematicsState state,
        RoomCollisionBlock block,
        int displacement,
        ushort leadingBoundary,
        int scanOffset,
        int totalColumns,
        out int clippedDisplacement)
    {
        int shape = block.Behavior & 0x1f;
        int orientation = block.Behavior >> 6;

        // Vertical collision uses leading Y bit three as the high quadrant-coordinate bit,
        // hence >>2 yields zero or two. The horizontal scan counter is traversal order, not
        // absolute room X; both left-to-right and right-to-left start it at zero.
        int quadrant = 4 * shape + (orientation ^ ((leadingBoundary & 8) >> 2));
        bool selectedSolid = SquareSlopeQuadrantSolidity[quadrant] != 0;
        bool collide;

        if (scanOffset == 0)
        {
            if (((state.XRadius + state.XPosition - 1) & 8) == 0 && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopeQuadrantSolidity[quadrant ^ 1] != 0;
        }
        else
        {
            if (scanOffset != totalColumns || ((state.XPosition - state.XRadius) & 8) == 0)
            {
                if (selectedSolid)
                {
                    collide = true;
                    goto Clip;
                }
            }

            collide = SquareSlopeQuadrantSolidity[quadrant ^ 1] != 0;
        }

        if (!collide)
        {
            clippedDisplacement = displacement;
            return false;
        }

    Clip:
        clippedDisplacement = ClipVerticalToSquareSlope(
            state,
            displacement,
            leadingBoundary);
        return true;
    }

    private static int ClipHorizontalToSolid(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(state.XRadius + (leadingBoundary | 0x000f) + 1 - state.XPosition));
            if (whole >= 0)
                whole = 0;
            state.XSubposition = 0;
            return whole << 16;
        }
        else
        {
            short whole = unchecked((short)((leadingBoundary & 0xfff0) - state.XRadius - state.XPosition));
            if (whole < 0)
                whole = 0;
            state.XSubposition = 0xffff;
            return whole << 16;
        }
    }

    private static int ClipHorizontalToSquareSlope(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(
                state.XRadius + (leadingBoundary | 0x0007) + 1 - state.XPosition));
            if (whole >= 0)
                whole = 0;
            state.XSubposition = 0;
            return whole << 16;
        }

        short rightWhole = unchecked((short)(
            (leadingBoundary & 0xfff8) - state.XRadius - state.XPosition));
        if (rightWhole < 0)
            rightWhole = 0;
        state.XSubposition = 0xffff;
        return rightWhole << 16;
    }

    private static int ClipVerticalToSolid(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(state.YRadius + (leadingBoundary | 0x000f) + 1 - state.YPosition));
            if (whole >= 0)
                whole = 0;
            state.YSubposition = 0;
            return whole << 16;
        }
        else
        {
            short whole = unchecked((short)((leadingBoundary & 0xfff0) - state.YRadius - state.YPosition));
            if (whole < 0)
                whole = 0;
            state.YSubposition = 0xffff;
            return whole << 16;
        }
    }

    private static int ClipVerticalToSquareSlope(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(
                state.YRadius + (leadingBoundary | 0x0007) + 1 - state.YPosition));
            if (whole >= 0)
                whole = 0;
            state.YSubposition = 0;
            return whole << 16;
        }

        short downWhole = unchecked((short)(
            (leadingBoundary & 0xfff8) - state.YRadius - state.YPosition));
        if (downWhole < 0)
            downWhole = 0;
        state.YSubposition = 0xffff;

        // Only the downward square-slope clip writes this flag. $90:923F consumes it to use
        // a one-pixel grounding probe instead of total-horizontal-speed-plus-one.
        state.PositionAdjustedBySlope = true;
        return downWhole << 16;
    }

    private static int GetVerticalBlockSpan(SamusKinematicsState state)
    {
        ushort alignedTop = unchecked((ushort)(state.YPosition - state.YRadius));
        alignedTop &= 0xfff0;
        return unchecked((ushort)(state.YRadius + state.YPosition - 1 - alignedTop)) >> 4;
    }

    private static int GetHorizontalBlockSpan(SamusKinematicsState state)
    {
        ushort alignedLeft = unchecked((ushort)(state.XPosition - state.XRadius));
        alignedLeft &= 0xfff0;
        return unchecked((ushort)(state.XRadius + state.XPosition - 1 - alignedLeft)) >> 4;
    }

    private static RoomCollisionBlock GetRequiredBlock(RoomLevelData level, int blockX, int blockY)
    {
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            throw new NotSupportedException(
                $"Samus collision reached out-of-room block ({blockX},{blockY}); native cleared-WRAM edge behavior is not translated.");
        }

        return level.GetCollisionBlock(blockX, blockY);
    }

    private static NotSupportedException Unsupported(RoomCollisionBlock block, string path) =>
        new($"Block {block.Index} type ${block.CollisionType:X1}/BTS ${block.Behavior:X2} requires untranslated {path} behavior.");
}

/// <summary>Observable result of one bank-$94 room-block movement scan.</summary>
public readonly record struct BlockMoveResult(
    int AcceptedDisplacement,
    bool Collided,
    RoomCollisionBlock? CollisionBlock,
    bool PositionAdjustedBySlope,
    RoomCollisionBlock? FloorSlopeBlock,
    RoomCollisionBlock? CeilingSlopeBlock);
