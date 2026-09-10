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
public static partial class SamusBlockCollision
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
    /// not commit Samus's whole-pixel probe position. Collision still writes subpixels.
    /// </summary>
    public static BlockMoveResult ProbeWallHorizontal(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusKinematicsState state,
        int signedDistance,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        SamusKinematicsState probe = state.SamusOwner is null
            ? new SamusKinematicsState()
            : new SamusKinematicsState(state.SamusOwner);
        probe.CollisionPose = state.CollisionPose;
        probe.XPosition = state.XPosition;
        probe.XSubposition = state.XSubposition;
        probe.YPosition = state.YPosition;
        probe.YSubposition = state.YSubposition;
        probe.XRadius = state.XRadius;
        probe.YRadius = state.YRadius;
        probe.YSpeed = state.YSpeed;
        probe.YSubspeed = state.YSubspeed;
        probe.YDirection = state.YDirection;
        probe.YAcceleration = state.YAcceleration;
        probe.YSubacceleration = state.YSubacceleration;
        probe.HorizontalSlopeCollisionEnable = state.HorizontalSlopeCollisionEnable;
        probe.PositionAdjustedBySlope = state.PositionAdjustedBySlope;
        // The enemy entries are immutable value snapshots. Sharing their ordered list is
        // safe, and lets the observational wall probe see exactly the same native actors.
        probe.InteractiveEnemies = state.InteractiveEnemies;

        // Reusing the translated horizontal dispatcher also preserves square-slope and
        // unsupported-block behavior. Any post-scan slope alignment touches only `probe`,
        // but bank-$94 collision side effects are not observational: touching a scroll
        // trigger still wakes its bank-$84 owner. Pass that shared owner through rather
        // than treating an extension-resolved trigger as an orphan during wall-jump checks.
        BlockMoveResult result = MoveHorizontal(
            bus,
            level,
            probe,
            signedDistance,
            plms: plms);

        // $94:8F49 (and square-slope clipping) writes Samus's real X subposition
        // even through the observational $94:967F entry point. The probe's accepted
        // displacement is whole pixels on a block hit, so its final fractional word
        // retains that write. Keep the live integer X unchanged. Dropping this write
        // let away movement accumulate fractional speed during the wall-check pose.
        if (result.Collided && result.EnemyCollision is null)
            state.XSubposition = probe.XSubposition;

        // The bank-$A0 routine preceding the block scan has a separate
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
        RoomPlmSystem? plms = null,
        bool publishDoorSideEffects = true,
        bool alignToSlopeAfterMovement = true)
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
            state.RecordSolidEnemyCollision(direction, probe.EnemyIndex);

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
                    case RoomCollisionType.Air:
                        break;

                    case RoomCollisionType.Slope:
                        if (!block.Bts.IsNonSquareSlope)
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

                    case RoomCollisionType.SolidBlock:
                    case RoomCollisionType.ShootableBlock:
                    case RoomCollisionType.GrappleBlock:
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.DoorBlock:
                        // `$94:938B` resolves BTS through the current room's bank-$8F door
                        // list. A normal bank-$8F destination publishes `door_def_ptr`, sets
                        // game state $09, and returns carry clear, so this scan must allow
                        // Samus into the doorway. Elevator pseudo-destinations have bit 15
                        // clear and fall through to the ordinary solid clipping routine.
                        CartridgeDoorHeader horizontalDoor = level.ResolveDoorCollision(
                            bus,
                            block.Behavior,
                            state.CollisionPose,
                            publishDoorSideEffects);
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

                    case RoomCollisionType.SpikeAir:
                    case RoomCollisionType.ShootableAir:
                    case RoomCollisionType.UnusedAir:
                        // Horizontal table entries 2/4/6 are literal clear-carry stubs.
                        break;
                    case RoomCollisionType.BombableAir:
                        ActivateCollisionBombableAir(level, block, state, canBreakBombBlocks, plms);
                        break;

                    case RoomCollisionType.SpecialAir:
                        // Sand's submerging callback clears vertical speed/gravity even
                        // when reached through the horizontal dispatcher.
                        SamusInsideBlockReactions.ReactCollision(bus, state, block, false,
                            ref acceptedDisplacement, out _);
                        if (block.Bts == RoomBlockBehaviorValues.ScrollTrigger &&
                            (plms is null || !plms.TryNotifyScrollTouch(block.Index)))
                        {
                            throw new InvalidOperationException(
                                $"Scroll trigger block {block.Index} has no active $B703 PLM owner; " +
                                $"live=[{string.Join(',', plms?.ScrollPlms.Select(scroll => scroll.BlockIndex) ?? [])}].");
                        }
                        break;

                    case RoomCollisionType.SpikeBlock:
                        if (state.SamusOwner is { } horizontalSamus)
                        {
                            SamusTerrainHazardCollision.ApplySolidSpikeCollision(
                                bus,
                                horizontalSamus,
                                block);
                        }
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.SpecialBlock when
                        block.Bts == RoomBlockBehaviorValues.CollectibleTrigger:
                        // Visible item frames are type-$B/BTS-$45. Bank $94 spawns the
                        // shared $EED3 detector, whose setup triggers the item at this
                        // origin and returns carry clear; Samus therefore passes through
                        // rather than clipping against the item's visual block.
                        if (plms is null || !plms.TryNotifyCollectibleTouch(block.Index))
                        {
                            throw new InvalidOperationException(
                                $"Collectible block {block.Index} has no active item PLM owner.");
                        }
                        break;

                    case RoomCollisionType.SpecialBlock:
                        // `$94:90CB` dispatches the speed-block entries through setup
                        // `$84:CDEA`. A stage-four boost or directional shinespark clears
                        // collision synchronously, so this same horizontal scan continues
                        // through the newly non-solid cell.
                        if (state.SamusOwner is { } speedBoostingSamus &&
                            plms is not null &&
                            plms.TrySpawnSamusSpeedBoosterBlock(
                                level,
                                block.Index,
                                block.Bts,
                                speedBoostingSamus))
                        {
                            break;
                        }
                        if (block.Bts.TryGetStationAccess(out _) &&
                            (plms is null ||
                             !plms.TryNotifyStationCollision(
                                 block.Index,
                                 block.Bts,
                                 state.CollisionPose,
                                 horizontal: true,
                                 movingPositive: acceptedDisplacement > 0,
                                 roomWidthInBlocks: level.WidthInBlocks)))
                        {
                            throw new InvalidOperationException(
                                $"Station access block {block.Index} BTS ${block.Behavior:X2} " +
                                "has no active map/resource/save-station PLM owner.");
                        }
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.BombableBlock:
                        // `$94:932D` indexes the collision-bomb-block PLM table with BTS
                        // 0..7. Its setup at `$84:CE83` returns carry (solid) unless Samus
                        // is speed boosting, screw attacking, or in pose `$C9-$CE`. On an
                        // accepted break it installs the air-type bomb-parent visual and
                        // returns carry clear, so this scan continues through the new air.
                        if (block.Bts.UsesAreaReactionTable || !CanBreakCollisionBombBlock(state, canBreakBombBlocks))
                        {
                            acceptedDisplacement = ClipHorizontalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                            break;
                        }
                        if (!block.Bts.IsNormalReactionIndex(8))
                        {
                            throw new InvalidDataException(
                                $"Collision bomb block {block.Index} has invalid BTS ${block.Behavior:X2}.");
                        }
                        // Production movement supplies the active room PLM owner, matching
                        // `$94:933F`'s immediate long call into Spawn_PLM. Keeping the null
                        // fallback preserves this low-level collision routine as a usable
                        // setup-only unit seam: it performs CE83's synchronous clear, while
                        // explicitly omitting the later independent animation lifecycle.
                        bool spawned = plms is null
                            ? ClearCollisionTypeWithoutLifecycle(level, block.Index)
                            : plms.TrySpawnCollisionBombBlock(level, block.Index, block.Bts);
                        if (spawned)
                            brokenBombBlock ??= block;
                        break;

                    default:
                        throw new InvalidDataException(
                            $"Block {block.Index} type ${block.CollisionType:X1} escaped the " +
                            "complete horizontal collision dispatcher.");
                }

                if (collided)
                    break;
            }
        }

        state.SetXFixed(unchecked(state.XFixed + (uint)acceptedDisplacement));

        // $90:9350/$90:93B1 call $94:87F4 after both collision and non-collision paths.
        // Direct bank-$94 callers (notably the prospective-running-pose probe)
        // omit that wrapper step and must retain the unaligned whole-pixel Y.
        SlopeAlignmentResult alignment = SamusSlopePhysics.AlignYPosition(
            bus,
            level,
            state.XPosition,
            state.YPosition,
            state.YRadius,
            horizontalSlopeCollisionEnabled: alignToSlopeAfterMovement &&
                (state.HorizontalSlopeCollisionEnable & 2) != 0);
        state.YPosition = alignment.YPosition;
        // $94:87F4 only sets this latch; it does not clear a square-floor contact
        // published by the previous vertical pass. $90:923F consumes it before
        // $94:9763 clears it on entry to the next nonzero terrain Y move.
        state.PositionAdjustedBySlope |= alignment.Adjusted;

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
        RoomPlmSystem? plms = null,
        bool publishDoorSideEffects = true,
        bool publishQuicksandGrounding = true)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(state);

        int acceptedDisplacement = displacement;
        bool collided = false;
        bool sandContact = false;
        SolidEnemyCollisionResult? enemyCollision = null;
        RoomCollisionBlock? collisionBlock = null;
        RoomCollisionBlock? brokenBombBlock = null;

        // Most bank-$90 callers enter through MoveSamus_Up/Down and therefore probe the
        // native solid-enemy list before dispatching bank-$94 terrain. A few callers use
        // the explicitly named `$94:9763` *NoSolidEnemyCollision* entry instead. Keep that
        // distinction as an argument at the shared collision seam; clearing the caller's
        // enemy snapshot would mutate unrelated gameplay state and would make the omission
        // impossible to verify.
        // A zero-speed bounce apex still enters the downward solid-enemy probe.
        // Exact platform tangency can therefore finish the bounce this frame;
        // only the later terrain dispatcher skips a zero displacement.
        if (includeSolidEnemies && state.InteractiveEnemies.Count != 0)
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
            state.RecordSolidEnemyCollision(direction, probe.EnemyIndex);

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
            // Only the nonzero terrain entry $94:9763 clears this latch. A solid-enemy
            // stop bypasses that entry, and a zero movement does not clear it either.
            state.PositionAdjustedBySlope = false;
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
                    case RoomCollisionType.Air:
                        break;

                    case RoomCollisionType.Slope:
                        if (!block.Bts.IsNonSquareSlope)
                        {
                            bool squareCollision = ReactVerticalSquareSlope(
                                state,
                                block,
                                acceptedDisplacement,
                                leadingBoundary,
                                // $94:959E initializes $1A to the span and decrements it
                                // while scanning left-to-right. $94:95F5 initializes it to
                                // zero and increments while scanning right-to-left. Passing
                                // the traversal offset directly made the physical left/right
                                // edge tests alternate every NMI beside a square-slope wall.
                                blocksLeftToCheck: scanLeftToRight
                                    ? horizontalSpan - offset
                                    : offset,
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

                    case RoomCollisionType.SolidBlock:
                    case RoomCollisionType.ShootableBlock:
                    case RoomCollisionType.GrappleBlock:
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.DoorBlock:
                        // `$94:93CE` is the vertical twin of the handler above. Preserve
                        // its carry result here; the room-level owner publishes the same
                        // native door pointer for the frontend dispatcher to consume.
                        CartridgeDoorHeader verticalDoor = level.ResolveDoorCollision(
                            bus,
                            block.Behavior,
                            state.CollisionPose,
                            publishDoorSideEffects);
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

                    case RoomCollisionType.SpikeAir:
                    case RoomCollisionType.SpecialAir:
                        if (block.CollisionType == RoomCollisionType.SpecialAir)
                        {
                            collided = SamusInsideBlockReactions.ReactCollision(bus, state, block, true,
                                ref acceptedDisplacement, out bool touchedSand);
                            sandContact |= touchedSand;
                            if (collided) collisionBlock = block;
                        }
                        if (block.CollisionType == RoomCollisionType.SpecialAir &&
                            block.Bts == RoomBlockBehaviorValues.ScrollTrigger &&
                            (plms is null || !plms.TryNotifyScrollTouch(block.Index)))
                        {
                            throw new InvalidOperationException(
                                $"Scroll trigger block {block.Index} has no active $B703 PLM owner; " +
                                $"live=[{string.Join(',', plms?.ScrollPlms.Select(scroll => scroll.BlockIndex) ?? [])}].");
                        }
                        // Scroll wake-up itself remains carry-clear; sand has a separate
                        // carry/contact contract handled above.
                        break;
                    case RoomCollisionType.ShootableAir:
                    case RoomCollisionType.UnusedAir:
                        // These air entries return clear carry without PLM setup.
                        break;
                    case RoomCollisionType.BombableAir:
                        ActivateCollisionBombableAir(level, block, state, canBreakBombBlocks, plms);
                        break;

                    case RoomCollisionType.SpikeBlock:
                        if (state.SamusOwner is { } verticalSamus)
                        {
                            SamusTerrainHazardCollision.ApplySolidSpikeCollision(
                                bus,
                                verticalSamus,
                                block);
                        }
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.SpecialBlock when
                        block.Bts == RoomBlockBehaviorValues.CollectibleTrigger:
                        if (plms is null || !plms.TryNotifyCollectibleTouch(block.Index))
                        {
                            throw new InvalidOperationException(
                                $"Collectible block {block.Index} has no active item PLM owner.");
                        }
                        break;

                    case RoomCollisionType.SpecialBlock:
                        // Hand reactions change state but return unconditional collision:
                        // even an admitted morph contact is clipped by this same scan.
                        if (state.SamusOwner is { } chozoSamus)
                            plms?.NotifyChozoStatueHandCollision(bus, level, block, chozoSamus,
                                state.CollisionPose, movingDown: acceptedDisplacement > 0);
                        // The vertical special-solid dispatcher shares setup `$84:CDEA`
                        // with horizontal collision. Accepted boost contact becomes air
                        // before clipping; rejected/non-speed special blocks remain solid.
                        if (state.SamusOwner is { } speedBoostingSamus &&
                            plms is not null &&
                            plms.TrySpawnSamusSpeedBoosterBlock(
                                level,
                                block.Index,
                                block.Bts,
                                speedBoostingSamus))
                        {
                            break;
                        }
                        if (!block.Bts.UsesAreaReactionTable &&
                            block.Bts.IsNormalReactionIndex(8) &&
                            acceptedDisplacement > 0)
                        {
                            if (plms is null)
                            {
                                throw new InvalidOperationException(
                                    $"Contact crumble block {block.Index} BTS " +
                                    $"${block.Behavior:X2} requires an active room PLM owner.");
                            }

                            // `$94:9102` always returns carry set for CE37. Setup removes
                            // the special collision nibble immediately (leaving the block
                            // type-eight solid), and this downward scan remains clipped.
                            plms.TrySpawnSamusContactCrumbleBlock(
                                level,
                                block.Index,
                                block.Bts);
                        }
                        if (block.Bts.TryGetStationAccess(out _) &&
                            (plms is null ||
                             !plms.TryNotifyStationCollision(
                                 block.Index,
                                 block.Bts,
                                 state.CollisionPose,
                                 horizontal: false,
                                 movingPositive: acceptedDisplacement > 0,
                                 roomWidthInBlocks: level.WidthInBlocks)))
                        {
                            throw new InvalidOperationException(
                                $"Station access block {block.Index} BTS ${block.Behavior:X2} " +
                                "has no active map/resource/save-station PLM owner.");
                        }
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.BombableBlock:
                        // Vertical dispatch is `$94:934C` and shares the exact bank-$84
                        // setup/carry contract documented in the horizontal branch above.
                        if (block.Bts.UsesAreaReactionTable || !CanBreakCollisionBombBlock(state, canBreakBombBlocks))
                        {
                            acceptedDisplacement = ClipVerticalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                            break;
                        }
                        if (!block.Bts.IsNormalReactionIndex(8))
                        {
                            throw new InvalidDataException(
                                $"Collision bomb block {block.Index} has invalid BTS ${block.Behavior:X2}.");
                        }
                        bool spawned = plms is null
                            ? ClearCollisionTypeWithoutLifecycle(level, block.Index)
                            : plms.TrySpawnCollisionBombBlock(level, block.Index, block.Bts);
                        if (spawned)
                            brokenBombBlock ??= block;
                        break;

                    default:
                        throw new InvalidDataException(
                            $"Block {block.Index} type ${block.CollisionType:X1} escaped the " +
                            "complete vertical collision dispatcher.");
                }

                if (collided)
                    break;
            }
        }

        state.SetYFixed(unchecked(state.YFixed + (uint)acceptedDisplacement));
        if (publishQuicksandGrounding && displacement > 0 && sandContact)
            collided = true;
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
        level.SetForegroundEntry(blockIndex, RoomPlmVisualBlockIndexes.CollisionBombParent);
        return true;
    }

    /// <summary>
    /// Runs the shared bomb-block setup for non-solid cells without consuming carry.
    /// </summary>
    private static void ActivateCollisionBombableAir(RoomLevelData level, RoomCollisionBlock block,
        SamusKinematicsState state, bool explicitAdmission, RoomPlmSystem? plms)
    {
        // Native horizontal/vertical air dispatch invokes the same setup as solid
        // bomb blocks, but ignores its carry. Rejection or exhausted actor slots
        // must never turn this non-solid cell into a movement obstruction.
        if (block.Bts.UsesAreaReactionTable || !CanBreakCollisionBombBlock(state, explicitAdmission))
            return;
        if (plms is not null)
            plms.TrySpawnCollisionBombBlock(level, block.Index, block.Bts);
        else
            ClearCollisionTypeWithoutLifecycle(level, block.Index);
    }

    /// <summary>
    /// Setup $84:CE83 checks boost stage independently of pose or movement handler.
    /// Existing explicit admissions cover Screw Attack/Shinespark and ownerless probes;
    /// reading the live owner here also admits grounded/falling/transition Speedball.
    /// </summary>
    private static bool CanBreakCollisionBombBlock(SamusKinematicsState state, bool explicitAdmission) =>
        explicitAdmission || state.SamusOwner?.HorizontalSpeed.IsActivelySpeedBoosting == true;

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
            if (block.Bts.SlopeFlipsVertically)
                return (displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(bus, block.Bts, state.XPosition);
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
            if (!block.Bts.SlopeFlipsVertically)
                return (displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(bus, block.Bts, state.XPosition);
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
        int shape = block.Bts.SlopeShape;
        int orientation = block.Bts.SlopeOrientation;

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
        int blocksLeftToCheck,
        int totalColumns,
        out int clippedDisplacement)
    {
        int shape = block.Bts.SlopeShape;
        int orientation = block.Bts.SlopeOrientation;

        // Vertical collision uses leading Y bit three as the high quadrant-coordinate bit,
        // hence >>2 yields zero or two. The horizontal counter is the native number of
        // physical blocks left: LTR counts span..0 while RTL counts 0..span.
        int quadrant = 4 * shape + (orientation ^ ((leadingBoundary & 8) >> 2));
        bool selectedSolid = SquareSlopeQuadrantSolidity[quadrant] != 0;
        bool collide;

        // $1A=0 always denotes the physical rightmost block, independent of which of the
        // two alternating scan routines reached it. $1A=$1C denotes the physical leftmost
        // block. Interior blocks necessarily test both eight-pixel halves.
        if (blocksLeftToCheck == 0)
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
            if (blocksLeftToCheck != totalColumns ||
                ((state.XPosition - state.XRadius) & 8) == 0)
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
        // $94:8E43 publishes support on a downward square-slope hit. Without this
        // latch, fast running probes farther than the eight-pixel floor thickness
        // on the next frame and can embed Samus below its surface.
        if (displacement >= 0) state.PositionAdjustedBySlope = true;
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
        => level.GetCollisionBlockOrPrefilledSolid(blockX, blockY);

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
                RoomCollisionType.HorizontalExtension when
                    block.Bts != RoomBlockBehaviorValues.None => block.Bts.ExtensionOffset,
                RoomCollisionType.VerticalExtension when
                    block.Bts != RoomBlockBehaviorValues.None =>
                    block.Bts.ExtensionOffset * level.WidthInBlocks,
                RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => 0,
                _ => int.MinValue,
            };

            if (delta == int.MinValue)
                return true;
            if (delta == 0)
                return false;

            int targetIndex = block.Index + delta;
            if ((uint)targetIndex >= (uint)blockCount)
            {
                throw new InvalidDataException(
                    $"Collision extension block {block.Index} type ${block.CollisionType:X1}/" +
                    $"BTS ${block.Behavior:X2} resolves outside room level data.");
            }
            block = level.GetCollisionBlockByIndex(targetIndex);
        }

        throw new InvalidDataException(
            $"Collision extension chain beginning at block {block.Index} contains a cycle.");
    }

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
    SolidEnemyCollisionResult? EnemyCollision = null)
{
    /// <summary>
    /// True when bank $90 selected downward movement and neither terrain nor a solid enemy
    /// stopped it. This is the condition that publishes solid-vertical result two
    /// (falling); an unobstructed upward carry instead publishes result zero (no change).
    /// </summary>
    public bool IsUnobstructedDownwardMovement =>
        !Collided && AcceptedDisplacement >= 0;
}
