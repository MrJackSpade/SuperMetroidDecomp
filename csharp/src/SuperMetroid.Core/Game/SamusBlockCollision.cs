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
    /// Ports the block-only observation made by <c>WallJumpBlockCollisionDetection</c> at
    /// <c>$94:967F</c>. The native routine publishes available distance in `$12` but does
    /// not commit Samus's probe position; a copied kinematics object preserves that rule.
    /// </summary>
    public static BlockMoveResult ProbeWallHorizontal(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusKinematicsState state,
        int signedDistance)
    {
        ArgumentNullException.ThrowIfNull(state);
        SamusKinematicsState probe = new()
        {
            XPosition = state.XPosition,
            XSubposition = state.XSubposition,
            YPosition = state.YPosition,
            YSubposition = state.YSubposition,
            XRadius = state.XRadius,
            YRadius = state.YRadius,
            YSpeed = state.YSpeed,
            YSubspeed = state.YSubspeed,
            YDirection = state.YDirection,
            YAcceleration = state.YAcceleration,
            YSubacceleration = state.YSubacceleration,
            HorizontalSlopeCollisionEnable = state.HorizontalSlopeCollisionEnable,
            PositionAdjustedBySlope = state.PositionAdjustedBySlope,
            // The enemy entries are immutable value snapshots. Sharing their ordered list is
            // safe, and lets the observational wall probe see exactly the same native actors.
            InteractiveEnemies = state.InteractiveEnemies,
        };

        // Reusing the translated horizontal dispatcher also preserves square-slope and
        // unsupported-block behavior. Any post-scan slope alignment touches only `probe`.
        BlockMoveResult result = MoveHorizontal(bus, level, probe, signedDistance);

        // The bank-$94 portion is observational, but the bank-$A0 routine it follows has one
        // real side effect: its touching path executes STZ SamusYSubPosition. Copy that single
        // native write back without committing the probe's X movement or slope alignment.
        if (result.EnemyCollision is { WasTouching: true })
            state.YSubposition = 0;

        return result;
    }

    /// <summary>
    /// Ports <c>BlockColl_Handle_Horiz</c> at <c>$94:9543</c>, the position addition in
    /// <c>Samus_MoveRight_NoSolidColl</c>, and the subsequent non-square slope alignment.
    /// </summary>
    public static BlockMoveResult MoveHorizontal(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusKinematicsState state,
        int displacement,
        bool canBreakBombBlocks = false,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(state);

        int acceptedDisplacement = displacement;
        bool collided = false;
        SolidEnemyCollisionResult? enemyCollision = null;
        RoomCollisionBlock? collisionBlock = null;
        RoomCollisionBlock? brokenBombBlock = null;

        if (acceptedDisplacement != 0 && state.InteractiveEnemies.Count != 0)
        {
            // `$90:9350/$93B1` presents bank $A0 with an unsigned magnitude even though the
            // managed block mover receives signed 16.16 displacement. Preserve both halves:
            // enemy clipping clears the fractional half only when it actually collides.
            uint magnitude = acceptedDisplacement < 0
                ? unchecked((uint)(-acceptedDisplacement))
                : unchecked((uint)acceptedDisplacement);
            SamusCollisionDirection direction = acceptedDisplacement < 0
                ? SamusCollisionDirection.Left
                : SamusCollisionDirection.Right;
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                unchecked((ushort)(magnitude >> 16)),
                unchecked((ushort)magnitude));

            if (probe.Collided)
            {
                // On enemy collision the bank-$90 wrapper skips bank-$94 block detection and
                // invokes the corresponding no-collision position adder with `$12.0000`.
                int clippedMagnitude = probe.Distance << 16;
                acceptedDisplacement = acceptedDisplacement < 0
                    ? -clippedMagnitude
                    : clippedMagnitude;
                collided = true;
                enemyCollision = probe;
            }
        }

        if (!collided && acceptedDisplacement != 0)
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
                if (!TryResolveExtension(level, ref block))
                    continue;
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

                    case 9:
                        // `$94:938B` resolves BTS through the current room's bank-$8F door
                        // list. A normal bank-$8F destination publishes `door_def_ptr`, sets
                        // game state $09, and returns carry clear, so this scan must allow
                        // Samus into the doorway. Elevator pseudo-destinations have bit 15
                        // clear and fall through to the ordinary solid clipping routine.
                        CartridgeDoorHeader horizontalDoor =
                            level.ResolveDoorCollision(bus, block.Behavior);
                        if ((horizontalDoor.DestinationRoomPointer & 0x8000) == 0)
                        {
                            acceptedDisplacement = ClipHorizontalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                        }
                        break;

                    case 15:
                        // `$94:932D` indexes the collision-bomb-block PLM table with BTS
                        // 0..7. Its setup at `$84:CE83` returns carry (solid) unless Samus
                        // is speed boosting, screw attacking, or in pose `$C9-$CE`. On an
                        // accepted break it clears only level_data's high nibble and returns
                        // carry clear, so this very scan continues through the new air.
                        if ((block.Behavior & 0x80) != 0 || !canBreakBombBlocks)
                        {
                            acceptedDisplacement = ClipHorizontalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                            break;
                        }
                        if (block.Behavior > 7)
                            throw Unsupported(block, "horizontal bomb-block table");
                        // Production movement supplies the active room PLM owner, matching
                        // `$94:933F`'s immediate long call into Spawn_PLM. Keeping the null
                        // fallback preserves this low-level collision routine as a usable
                        // setup-only unit seam: it performs CE83's synchronous clear, while
                        // explicitly omitting the later independent animation lifecycle.
                        bool spawned = plms is null
                            ? ClearCollisionTypeWithoutLifecycle(level, block.Index)
                            : plms.TrySpawnCollisionBombBlock(level, block.Index, block.Behavior);
                        if (spawned)
                            brokenBombBlock ??= block;
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
            alignment.CeilingBlock,
            brokenBombBlock,
            enemyCollision);
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
        bool scanLeftToRight,
        bool canBreakBombBlocks = false,
        bool includeSolidEnemies = true,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(state);

        int acceptedDisplacement = displacement;
        bool collided = false;
        SolidEnemyCollisionResult? enemyCollision = null;
        RoomCollisionBlock? collisionBlock = null;
        RoomCollisionBlock? brokenBombBlock = null;
        state.PositionAdjustedBySlope = false;

        // Most bank-$90 callers enter through MoveSamus_Up/Down and therefore probe the
        // native solid-enemy list before dispatching bank-$94 terrain. A few callers use
        // the explicitly named `$94:9763` *NoSolidEnemyCollision* entry instead. Keep that
        // distinction as an argument at the shared collision seam; clearing the caller's
        // enemy snapshot would mutate unrelated gameplay state and would make the omission
        // impossible to verify.
        if (includeSolidEnemies && acceptedDisplacement != 0 && state.InteractiveEnemies.Count != 0)
        {
            // `$90:93EC/$9440` uses the same unsigned `$12.$14` magnitude contract as X.
            uint magnitude = acceptedDisplacement < 0
                ? unchecked((uint)(-acceptedDisplacement))
                : unchecked((uint)acceptedDisplacement);
            SamusCollisionDirection direction = acceptedDisplacement < 0
                ? SamusCollisionDirection.Up
                : SamusCollisionDirection.Down;
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                unchecked((ushort)(magnitude >> 16)),
                unchecked((ushort)magnitude));

            if (probe.Collided)
            {
                // Solid-enemy success bypasses the block dispatcher and moves only to the
                // current enemy boundary. This is also how native vertical-collision flags
                // distinguish an enemy stop from an unobstructed terrain move.
                int clippedMagnitude = probe.Distance << 16;
                acceptedDisplacement = acceptedDisplacement < 0
                    ? -clippedMagnitude
                    : clippedMagnitude;
                collided = true;
                enemyCollision = probe;
            }
        }

        if (!collided && acceptedDisplacement != 0)
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
                if (!TryResolveExtension(level, ref block))
                    continue;
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

                    case 9:
                        // `$94:93CE` is the vertical twin of the handler above. Preserve
                        // its carry result here; the room-level owner publishes the same
                        // native door pointer for the frontend dispatcher to consume.
                        CartridgeDoorHeader verticalDoor =
                            level.ResolveDoorCollision(bus, block.Behavior);
                        if ((verticalDoor.DestinationRoomPointer & 0x8000) == 0)
                        {
                            acceptedDisplacement = ClipVerticalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                        }
                        break;

                    case 15:
                        // Vertical dispatch is `$94:934C` and shares the exact bank-$84
                        // setup/carry contract documented in the horizontal branch above.
                        if ((block.Behavior & 0x80) != 0 || !canBreakBombBlocks)
                        {
                            acceptedDisplacement = ClipVerticalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                            break;
                        }
                        if (block.Behavior > 7)
                            throw Unsupported(block, "vertical bomb-block table");
                        bool spawned = plms is null
                            ? ClearCollisionTypeWithoutLifecycle(level, block.Index)
                            : plms.TrySpawnCollisionBombBlock(level, block.Index, block.Behavior);
                        if (spawned)
                            brokenBombBlock ??= block;
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
            CeilingSlopeBlock: null,
            BrokenBombBlock: brokenBombBlock,
            EnemyCollision: enemyCollision);
    }

    /// <summary>
    /// Executes collision-bomb setup without installing its independent PLM handler.
    /// </summary>
    /// <remarks>
    /// This narrowly retained fallback is for direct collision tests and embedders that do
    /// not own a room runtime. The playable runtime always passes <see cref="RoomPlmSystem"/>
    /// and therefore never takes this incomplete branch.
    /// </remarks>
    private static bool ClearCollisionTypeWithoutLifecycle(RoomLevelData level, int blockIndex)
    {
        level.ClearCollisionType(blockIndex);
        return true;
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

    /// <summary>
    /// Follows collision-extension blocks exactly like the `$94:9515/$9535` redispatch
    /// loop. Type `$5` adds signed BTS horizontally; type `$D` adds signed BTS rows.
    /// A zero BTS returns carry clear immediately and therefore behaves as air.
    /// </summary>
    internal static bool TryResolveExtension(
        RoomLevelData level,
        ref RoomCollisionBlock block)
    {
        int blockCount = checked(level.WidthInBlocks * level.HeightInBlocks);
        for (int hops = 0; hops < blockCount; hops++)
        {
            int delta = block.CollisionType switch
            {
                5 when block.Behavior != 0 => unchecked((sbyte)block.Behavior),
                13 when block.Behavior != 0 =>
                    unchecked((sbyte)block.Behavior) * level.WidthInBlocks,
                5 or 13 => 0,
                _ => int.MinValue,
            };

            if (delta == int.MinValue)
                return true;
            if (delta == 0)
                return false;

            int targetIndex = block.Index + delta;
            if ((uint)targetIndex >= (uint)blockCount)
            {
                throw new NotSupportedException(
                    $"Collision extension block {block.Index} type ${block.CollisionType:X1}/" +
                    $"BTS ${block.Behavior:X2} resolves outside room level data.");
            }
            block = level.GetCollisionBlockByIndex(targetIndex);
        }

        throw new NotSupportedException(
            $"Collision extension chain beginning at block {block.Index} contains a cycle.");
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
    RoomCollisionBlock? CeilingSlopeBlock,
    RoomCollisionBlock? BrokenBombBlock = null,
    SolidEnemyCollisionResult? EnemyCollision = null);
