using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Incremental port of Samus's room-block movement scans.
/// </summary>
/// <remarks>
/// Supported dispatcher categories are deliberately explicit: type 0 air, type 1
/// non-square slopes, and ordinary solid-family types $8/$C/$E. Other types have room-object,
/// damage, door, extension, or square-slope behavior and throw instead of being flattened
/// to “air” or “solid.”
/// </remarks>
public static class SamusBlockCollision
{
    /// <summary>
    /// Runs the block-only observation of the wall-jump block collision check.
    /// The check publishes available distance in a scratch value but does
    /// not commit Samus's whole-pixel probe position. Collision still writes subpixels.
    /// </summary>
    internal static BlockMoveResult ProbeWallHorizontal(

        RoomLevelData level,
        SamusKinematicsState state,
        SamusState? owner,
        SignedSixteenSixteen signedDistance,
        ISamusRoomObjects? roomObjects = null)
    {
        Ensure.NotNull(state);
        SamusKinematicsState probe = state.CreateCollisionProbe();

        // Reusing the translated horizontal dispatcher also preserves square-slope and
        // unsupported-block behavior. Any post-scan slope alignment touches only `probe`,
        // but collision side effects are not observational: touching a scroll
        // trigger still wakes its owner. Pass that shared owner through rather
        // than treating an extension-resolved trigger as an orphan during wall-jump checks.
        BlockMoveResult result = MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, probe, owner, new HorizontalBlockMove { Displacement = signedDistance, BlockReactionDirection = SamusCollisionDirection.NonDirectionalProbe });
        // Submerging sand writes these real motion words even through wall observation.
        state.YSpeed = probe.YSpeed;
        state.YAcceleration = probe.YAcceleration;

        // (and square-slope clipping) writes Samus's real X subposition
        // even through the observational wall-jump probe entry point. The probe's accepted
        // displacement is whole pixels on a block hit, so its final fractional word
        // retains that write. Keep the live integer X unchanged. Dropping this write
        // let away movement accumulate fractional speed during the wall-check pose.
        if (result.Collided && result.EnemyCollision is null)
            state.Position = state.Position.WithFractionX(probe.Position.X.Fraction);

        // The routine preceding the block scan has a separate
        // real side effect: its touching path clears Samus's Y subposition. Copy that single
        // write back without committing the probe's X movement or slope alignment.
        if (result.EnemyCollision is { WasTouching: true })
            state.Position = state.Position.WithFractionY(0);

        return result;
    }

    /// <summary>
    /// Runs horizontal block collision, the position addition of a move
    /// without solid-enemy collision, and the subsequent non-square slope alignment.
    /// </summary>
    /// <param name="terrain">SamusTerrain</param>
    /// <param name="state">Live or probe kinematics whose position, subposition, and collision-related motion words are updated.</param>
    /// <param name="owner">The Samus whose collision reactions (spikes, boost blocks, bomb blocks) act, or null for an ownerless probe.</param>
    /// <param name="move">One horizontal block move: the displacement and which collision side effects it publishes (#633).</param>
    public static BlockMoveResult MoveHorizontal(SamusTerrain terrain, SamusKinematicsState state, SamusState? owner, HorizontalBlockMove move)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(state);

        if (move.CollisionMovementDirection is { } storedDirection &&
            storedDirection is not (SamusCollisionDirection.Left or SamusCollisionDirection.Right))
            throw new ArgumentOutOfRangeException(nameof(move.CollisionMovementDirection));

        SignedSixteenSixteen acceptedDisplacement = move.Displacement;
        bool collided = false;
        SolidEnemyCollisionResult? enemyCollision = null;
        RoomCollisionBlock? collisionBlock = null;
        RoomCollisionBlock? brokenBombBlock = null;

        if ((!acceptedDisplacement.IsZero || move.CollisionMovementDirection.HasValue) && state.InteractiveEnemies.Count != 0)
        {
            // Presents enemy collision with an unsigned magnitude even though the
            // managed block mover receives signed 16.16 displacement. Preserve both halves:
            // enemy clipping clears the fractional half only when it actually collides.
            UnsignedSixteenSixteen magnitude = move.Displacement.Magnitude();
            // The sign of the requested distance selects the entry: zero is not negative and enters right.
            // The probe itself reads the calculator's stored direction, so a zero move
            // still detects tangency (and clears fractional Y) in the facing direction.
            bool entersLeft = move.Displacement.IsNegative;
            SamusCollisionDirection direction = move.CollisionMovementDirection ??
                (entersLeft ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                magnitude.Whole,
                magnitude.Fraction);
            state.RecordSolidEnemyCollision(direction, probe.EnemySlot);

            if (probe.Collided)
            {
                // On enemy collision the wrapper skips block detection and
                // invokes the corresponding no-collision position adder with the clipped whole distance. An accepted
                // gap is 0 through 32767 pixels: the probe skips any gap that reads as negative.
                SignedSixteenSixteen clippedMagnitude = SignedSixteenSixteen.FromWholePixels(probe.Distance);
                acceptedDisplacement = entersLeft
                    ? -clippedMagnitude
                    : clippedMagnitude;
                collided = true;
                enemyCollision = probe;
            }
        }

        if (!collided && !acceptedDisplacement.IsZero)
        {
            // The target center and leading boundary are calculated once, before a slope
            // is allowed to scale the move's distance. This slightly surprising ordering is
            // exactly how the horizontal block collision sets up its collision probe.
            int targetCenter = state.Position.X.MovedBy(acceptedDisplacement).Whole;
            int leadingBoundary = !acceptedDisplacement.IsNegative
                ? CoordinateAxis.Wrap(state.Radius.X + targetCenter - 1)
                : CoordinateAxis.Wrap(targetCenter - state.Radius.X);

            int topBlockY = CoordinateAxis.Wrap(state.Position.Y.Whole - state.Radius.Y) >> 4;
            int verticalSpan = GetVerticalBlockSpan(state);
            int blockX = leadingBoundary >> 4;

            for (int rowOffset = 0; rowOffset <= verticalSpan; rowOffset++)
            {
                RoomCollisionBlock block = terrain.Level.GetCollisionBlockOrEdgeSolid(
                    new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(topBlockY + rowOffset)));
                if (!TryResolveExtension(terrain.Level, ref block))
                    continue;
                switch (block.CollisionType)
                {
                    case RoomCollisionType.Air:
                        break;

                    case RoomCollisionType.Slope:
                        if (!block.Behavior.Slope.IsNonSquare)
                        {
                            bool squareCollision = ReactHorizontalSquareSlope(
                                state,
                                block,
                                acceptedDisplacement,
                                leadingBoundary,
                                remainingRows: verticalSpan - rowOffset,
                                totalRows: verticalSpan,
                                out SignedSixteenSixteen clippedDisplacement);
                            if (squareCollision)
                            {
                                acceptedDisplacement = clippedDisplacement;
                                collided = true;
                                collisionBlock = block;
                            }
                            break;
                        }

                        acceptedDisplacement = SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(
                            block.Behavior,
                            acceptedDisplacement,
                            state.YSpeed);
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
                        // Resolves behaviour through the current room's door
                        // list. A normal destination publishes the door, enters
                        // the door-transition game state, and reports no collision, so this scan must allow
                        // Samus into the doorway. Elevator pseudo-destinations are not doors
                        // and fall through to the ordinary solid clipping.
                        DoorListEntry horizontalDoor = terrain.Level.ResolveDoorCollision(
                            block.Behavior,
                            state.CollisionPose.ForRoomCollision(),
                            move.PublishDoorSideEffects);
                        if (horizontalDoor.IsElevatorPseudoDoor)
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
                        // Spike, shootable and unused air let Samus pass with no reaction.
                        break;
                    case RoomCollisionType.BombableAir:
                        ActivateCollisionBombableAir(terrain, block, owner, move.CanBreakBombBlocks);
                        break;

                    case RoomCollisionType.SpecialAir:
                        // Sand's submerging callback clears vertical speed/gravity even
                        // when reached through the horizontal dispatcher.
                        collided = SamusInsideBlockReactions.ReactCollision(new(state, state.CollisionContactDamageIndex(owner)), terrain, block, Axis.Horizontal,
                            ref acceptedDisplacement, out _, move.BlockReactionDirection) == SpecialAirCollisionOutcome.Blocks;
                        if (collided) collisionBlock = block;
                        if (block.Behavior.Reaction == BlockReactionDefinitions.ScrollTrigger &&
                            (terrain.RoomObjects is null || !terrain.RoomObjects.Scrolls.TryNotifyScrollTouch(block.At)))
                        {
                            throw new InvalidOperationException(
                                $"Scroll trigger block {block.Position} has no active scroll room-object owner; " +
                                $"live=[{string.Join(',', terrain.RoomObjects?.Scrolls.Snapshots.Select(scroll => scroll.Block) ?? [])}].");
                        }
                        break;

                    case RoomCollisionType.SpikeBlock:
                        if (owner is { } horizontalSamus)
                        {
                            SamusTerrainHazardCollision.ApplySolidSpikeCollision(
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
                        block.Behavior.Reaction == BlockReactionDefinitions.CollectibleTrigger:
                        // Visible item frames are special blocks of the item-frame behaviour. Block collision spawns the
                        // shared item detector, whose setup triggers the item at this
                        // origin and reports no collision; Samus therefore passes through
                        // rather than clipping against the item's visual block.
                        if (terrain.RoomObjects is null || !terrain.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(block.At))
                        {
                            throw new InvalidOperationException(
                                $"Collectible block {block.Position} has no active item room-object owner.");
                        }
                        break;

                    case RoomCollisionType.SpecialBlock:
                        // Both special-block reactions spawn the room object and let Samus pass unless its setup stops her.
                        // A full pool returns without running setup, so even a block whose
                        // setup would normally reject contact falls through as air.
                        if (terrain.RoomObjects?.Quicksand.IsAllocationFull == true)
                            break;
                        // Dispatches the speed-block entries through the speed-block
                        // setup. A stage-four boost or directional shinespark clears
                        // collision synchronously, so this same horizontal scan continues
                        // through the newly non-solid cell.
                        if (owner is { } speedBoostingSamus &&
                            terrain.RoomObjects is not null &&
                            terrain.RoomObjects.BlockReactions.TrySpawnSamusSpeedBoosterBlock(
                                terrain.Level,
                                block.At,
                                block.Behavior,
                                speedBoostingSamus.RoomView))
                        {
                            break;
                        }
                        // Station setups reject direction $F before finding their
                        // parent. An observational scan still clips against the access
                        // tile, but must not activate the station or demand an owner.
                        if (move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
                            block.Behavior.TryGetStationAccess(out _) &&
                            (terrain.RoomObjects is null ||
                             !terrain.RoomObjects.Stations.TryNotifyStationCollision(
                                 terrain.Level,
                                 block.At,
                                 block.Behavior,
                                 state.CollisionPose.ForRoomCollision(),
                                 Axis.Horizontal,
                                 movingPositive: acceptedDisplacement.IsPositive)))
                        {
                            throw new InvalidOperationException(
                                $"Station access block {block.Position} behaviour ({block.Behavior}) " +
                                "has no active map/resource/save-station room-object owner.");
                        }
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        collisionBlock = block;
                        break;

                    case RoomCollisionType.BombableBlock:
                        // Indexes the collision-bomb-block room-object table with behaviour
                        // 0..7. Its setup reports the block solid unless Samus
                        // is speed boosting, screw attacking, or in ShinesparkHorizontalRightPose through ShinesparkDiagonalLeftPose. On an
                        // accepted break it installs the air-type bomb-parent visual and
                        // reports no collision, so this scan continues through the new air.
                        if (terrain.RoomObjects?.Quicksand.IsAllocationFull == true)
                            break;
                        if (block.Behavior.Reaction.IsAreaSpecific || !CanBreakCollisionBombBlock(owner, move.CanBreakBombBlocks))
                        {
                            acceptedDisplacement = ClipHorizontalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            collisionBlock = block;
                            break;
                        }
                        if (!block.Behavior.Reaction.IsCommonEntryBelow(8))
                        {
                            throw new InvalidDataException(
                                $"Collision bomb block {block.Position} has invalid behaviour ({block.Behavior}).");
                        }
                        // Every contact that breaks a bomb block starts its block reaction, which runs the breakup
                        // and grows the block back (#1582).
                        bool spawned = RoomObjectsBreaking(terrain, block.At).BlockReactions
                            .TrySpawnCollisionBombBlock(terrain.Level, block.At, block.Behavior);
                        if (spawned)
                            brokenBombBlock ??= block;
                        break;

                    case RoomCollisionType.HorizontalExtension:
                    case RoomCollisionType.VerticalExtension:
                    default:
                        throw new InvalidDataException(
                            $"Block {block.Position} type {block.CollisionType} escaped the " +
                            "complete horizontal collision dispatcher.");
                }

                if (collided)
                    break;
            }
        }

        state.Position = state.Position.AdvanceX(acceptedDisplacement);

        // Only the live movement wrapper publishes inheritance. Copies used
        // for prospective poses and wall probes must not replace the preceding move.
        if (owner is { } mover && ReferenceEquals(mover.Kinematics, state) &&
            move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
            move.AlignToSlopeAfterMovement)
            mover.ProjectileInheritance.Record(
                move.Displacement.IsNegative ? HorizontalDirection.Left : HorizontalDirection.Right,
                acceptedDisplacement);

        // Align Y to whole pixels after both collision and non-collision paths.
        // Direct callers (notably the prospective-running-pose probe)
        // omit that wrapper step and must retain the unaligned whole-pixel Y.
        SlopeAlignmentResult alignment = SamusSlopePhysics.AlignYPosition(
            terrain.Level,
            state.Position.Whole,
            state.Radius.Y,
            horizontalSlopeCollisionEnabled: move.AlignToSlopeAfterMovement &&
                (state.HorizontalSlopeCollisionEnable & 2) != 0);
        state.Position = state.Position.WithWholeY(alignment.Position.Y);
        // Only sets this latch; it does not clear a square-floor contact
        // published by the previous vertical pass. The vertical move consumes it before
        // the next nonzero terrain Y move clears it on entry.
        state.PositionAdjustedBySlope |= alignment.Adjusted;

        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            enemyCollision);
    }

    /// <summary>
    /// Moves Samus vertically against the blocks, scanning them left to right or right to left, then adds the final
    /// fixed-point step to her position when nothing solid stops her.
    /// </summary>
    public static BlockMoveResult MoveVertical(SamusTerrain terrain, SamusKinematicsState state, SamusState? owner, VerticalBlockMove move)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(state);

        SignedSixteenSixteen acceptedDisplacement = move.Displacement;
        bool collided = false;
        bool sandContact = false;
        SolidEnemyCollisionResult? enemyCollision = null;

        // Most callers enter through the upward or downward move and therefore probe the
        // solid-enemy list before dispatching terrain. A few callers use
        // the explicitly named entry without solid-enemy collision instead. Keep that
        // distinction as an argument at the shared collision seam; clearing the caller's
        // enemy snapshot would mutate unrelated gameplay state and would make the omission
        // impossible to verify.
        // A zero-speed bounce apex still enters the downward solid-enemy probe.
        // Exact platform tangency can therefore finish the bounce this frame;
        // only the later terrain dispatcher skips a zero displacement.
        if (move.IncludeSolidEnemies && state.InteractiveEnemies.Count != 0)
        {
            // Uses the same unsigned whole-and-fraction distance magnitude contract as X.
            UnsignedSixteenSixteen magnitude = move.Displacement.Magnitude();
            SamusCollisionDirection direction = move.Displacement.IsNegative
                ? SamusCollisionDirection.Up
                : SamusCollisionDirection.Down;
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                magnitude.Whole,
                magnitude.Fraction);
            state.RecordSolidEnemyCollision(direction, probe.EnemySlot);

            if (probe.Collided)
            {
                // Solid-enemy success bypasses the block dispatcher and moves only to the
                // current enemy boundary. This is also how the game's vertical-collision flags
                // distinguish an enemy stop from an unobstructed terrain move. An accepted gap is 0 through 32767
                // pixels: the probe skips any gap that reads as negative.
                SignedSixteenSixteen clippedMagnitude = SignedSixteenSixteen.FromWholePixels(probe.Distance);
                acceptedDisplacement = acceptedDisplacement.IsNegative
                    ? -clippedMagnitude
                    : clippedMagnitude;
                collided = true;
                enemyCollision = probe;
            }
        }

        if (!collided && !acceptedDisplacement.IsZero)
        {
            // Only the nonzero terrain entry clears this latch. A solid-enemy
            // stop bypasses that entry, and a zero movement does not clear it either.
            state.PositionAdjustedBySlope = false;
            VerticalTerrainScan scan = ScanVerticalTerrain(terrain, state, owner, new VerticalTerrainProbe { Displacement = acceptedDisplacement, ScanLeftToRight = move.ScanLeftToRight, CanBreakBombBlocks = move.CanBreakBombBlocks, PublishDoorSideEffects = move.PublishDoorSideEffects, BlockReactionDirection = move.BlockReactionDirection });
            acceptedDisplacement = scan.AcceptedDisplacement;
            collided = scan.Collided;
            sandContact = scan.SandContact;
        }

        state.Position = state.Position.AdvanceY(acceptedDisplacement);
        if (owner is { } mover && ReferenceEquals(mover.Kinematics, state) &&
            move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe)
            mover.ProjectileInheritance.Record(
                move.Displacement.IsNegative ? VerticalDirection.Up : VerticalDirection.Down,
                acceptedDisplacement);
        // Publishes sand contact after either signed movement direction.
        // Pose probes instead consume only the block reaction's collision result.
        if (move.PublishQuicksandGrounding && sandContact)
            collided = true;
        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            EnemyCollision: enemyCollision);
    }

    /// <summary>Vertical scan result including sand contact, which can affect motion without blocking travel.</summary>
    [IndependentFlag("independent scan facts: a collision, and sand contact")]
    private readonly record struct VerticalTerrainScan(SignedSixteenSixteen AcceptedDisplacement, bool Collided, bool SandContact);

    /// <summary>
    /// The vertical block scans (left to right) and the mirrored scan
    /// (right to left): every block under the leading boundary, dispatched by collision type.
    /// A zero displacement scans downward from the current bottom boundary.
    /// </summary>
    private static VerticalTerrainScan ScanVerticalTerrain(SamusTerrain terrain, SamusKinematicsState state, SamusState? owner, VerticalTerrainProbe move)
    {
        SignedSixteenSixteen acceptedDisplacement = move.Displacement;
        bool collided = false;
        bool sandContact = false;
        int targetCenter = state.Position.Y.MovedBy(acceptedDisplacement).Whole;
        int leadingBoundary = !acceptedDisplacement.IsNegative
            ? CoordinateAxis.Wrap(state.Radius.Y + targetCenter - 1)
            : CoordinateAxis.Wrap(targetCenter - state.Radius.Y);
        int blockY = leadingBoundary >> 4;
        int horizontalSpan = GetHorizontalBlockSpan(state);
        int firstBlockX = move.ScanLeftToRight
            ? CoordinateAxis.Wrap(state.Position.X.Whole - state.Radius.X) >> 4
            : CoordinateAxis.Wrap(state.Position.X.Whole + state.Radius.X - 1) >> 4;

        for (int offset = 0; offset <= horizontalSpan; offset++)
        {
            int blockX = move.ScanLeftToRight ? firstBlockX + offset : firstBlockX - offset;
            RoomCollisionBlock block = terrain.Level.GetCollisionBlockOrEdgeSolid(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
            if (!TryResolveExtension(terrain.Level, ref block))
                continue;
            switch (block.CollisionType)
            {
                case RoomCollisionType.Air:
                    break;

                case RoomCollisionType.Slope:
                    if (!block.Behavior.Slope.IsNonSquare)
                    {
                        bool squareCollision = ReactVerticalSquareSlope(
                            state,
                            block,
                            acceptedDisplacement,
                            leadingBoundary,
                            // Initializes the blocks-left count to the span and decrements it
                            // while scanning left-to-right. The mirrored scan initializes it to
                            // zero and increments while scanning right-to-left. Passing
                            // the traversal offset directly made the physical left/right
                            // edge tests alternate every frame beside a square-slope wall.
                            blocksLeftToCheck: move.ScanLeftToRight
                                ? horizontalSpan - offset
                                : offset,
                            totalColumns: horizontalSpan,
                            out SignedSixteenSixteen clippedDisplacement);
                        if (squareCollision)
                        {
                            acceptedDisplacement = clippedDisplacement;
                            collided = true;
                        }
                        break;
                    }

                    // Selects its floor or ceiling test from bit 0 of
                    // CollisionMovementDirection, not the distance's sign. The changed-
                    // pose probe's direction $F therefore tests both ways as a floor.
                    (acceptedDisplacement, collided) = ClipVerticalToNonSquareSlope(
                        state,
                        block,
                        blockX,
                        acceptedDisplacement,
                        targetCenter,
                        // Right, down and the non-directional
                        // probe react as a floor, left and up as a ceiling.
                        reactsDownward: move.BlockReactionDirection is { } reactionDirection
                            ? reactionDirection is SamusCollisionDirection.Right or SamusCollisionDirection.Down or
                                SamusCollisionDirection.NonDirectionalProbe
                            : !acceptedDisplacement.IsNegative);
                    break;

                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.ShootableBlock:
                case RoomCollisionType.GrappleBlock:
                    acceptedDisplacement = ClipVerticalToSolid(
                        state,
                        acceptedDisplacement,
                        leadingBoundary);
                    collided = true;
                    break;

                case RoomCollisionType.DoorBlock:
                    // Is the vertical twin of the handler above. Preserve
                    // its collision result here; the room-level owner publishes the same
                    // door entry for the frontend dispatcher to consume.
                    DoorListEntry verticalDoor = terrain.Level.ResolveDoorCollision(
                        block.Behavior,
                        state.CollisionPose.ForRoomCollision(),
                        move.PublishDoorSideEffects);
                    if (verticalDoor.IsElevatorPseudoDoor)
                    {
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                    }
                    break;

                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                    if (block.CollisionType == RoomCollisionType.SpecialAir)
                    {
                        collided = SamusInsideBlockReactions.ReactCollision(new(state, state.CollisionContactDamageIndex(owner)), terrain, block, Axis.Vertical,
                            ref acceptedDisplacement, out bool touchedSand, move.BlockReactionDirection) == SpecialAirCollisionOutcome.Blocks;
                        sandContact |= touchedSand;
                    }
                    if (block.CollisionType == RoomCollisionType.SpecialAir &&
                        block.Behavior.Reaction == BlockReactionDefinitions.ScrollTrigger &&
                        (terrain.RoomObjects is null || !terrain.RoomObjects.Scrolls.TryNotifyScrollTouch(block.At)))
                    {
                        throw new InvalidOperationException(
                            $"Scroll trigger block {block.Position} has no active scroll room-object owner; " +
                            $"live=[{string.Join(',', terrain.RoomObjects?.Scrolls.Snapshots.Select(scroll => scroll.Block) ?? [])}].");
                    }
                    // Scroll wake-up itself lets Samus pass; sand has a separate
                    // collision and contact contract handled above.
                    break;
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnusedAir:
                    // These air blocks let Samus pass without room-object setup.
                    break;
                case RoomCollisionType.BombableAir:
                    ActivateCollisionBombableAir(terrain, block, owner, move.CanBreakBombBlocks);
                    break;

                case RoomCollisionType.SpikeBlock:
                    if (owner is { } verticalSamus)
                    {
                        SamusTerrainHazardCollision.ApplySolidSpikeCollision(
                            verticalSamus,
                            block);
                    }
                    acceptedDisplacement = ClipVerticalToSolid(
                        state,
                        acceptedDisplacement,
                        leadingBoundary);
                    collided = true;
                    break;

                case RoomCollisionType.SpecialBlock when
                    block.Behavior.Reaction == BlockReactionDefinitions.CollectibleTrigger:
                    if (terrain.RoomObjects is null || !terrain.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(block.At))
                    {
                        throw new InvalidOperationException(
                            $"Collectible block {block.Position} has no active item room-object owner.");
                    }
                    break;

                case RoomCollisionType.SpecialBlock:
                    // Spawning a room object lets Samus pass when all forty slots
                    // are occupied. No setup runs, so the unchanged special block is
                    // treated as air for this collision probe.
                    if (terrain.RoomObjects?.Quicksand.IsAllocationFull == true)
                        break;
                    // Hand reactions change state but return unconditional collision:
                    // even an admitted morph contact is clipped by this same scan.
                    if (owner is { } chozoSamus)
                        terrain.RoomObjects?.ChozoStatueHand.NotifyChozoStatueHandCollision(terrain.Level,
                            block.At, chozoSamus,
                            state.CollisionPose, movingDown: acceptedDisplacement.IsPositive &&
                                (move.BlockReactionDirection ?? SamusCollisionDirection.Down) == SamusCollisionDirection.Down);
                    // The vertical special-solid dispatcher shares the speed-block setup
                    // with horizontal collision. Accepted boost contact becomes air
                    // before clipping; rejected/non-speed special blocks remain solid.
                    if (owner is { } speedBoostingSamus &&
                        terrain.RoomObjects is not null &&
                        terrain.RoomObjects.BlockReactions.TrySpawnSamusSpeedBoosterBlock(
                            terrain.Level,
                            block.At,
                            block.Behavior,
                            speedBoostingSamus.RoomView))
                    {
                        break;
                    }
                    if (block.Behavior.TryGetContactCrumble(out ContactCrumbleBlock crumble) &&
                        acceptedDisplacement.IsPositive &&
                        // The contact crumble checks the direction nibble, not the sign of
                        // the tested clearance. Pose probes use F and delete the room object.
                        (move.BlockReactionDirection ?? SamusCollisionDirection.Down) == SamusCollisionDirection.Down)
                    {
                        if (terrain.RoomObjects is null)
                        {
                            throw new InvalidOperationException(
                                $"Contact crumble block {block.Position} behaviour " +
                                $"({block.Behavior}) requires an active room-object owner.");
                        }

                        // The contact crumble always reports the block solid. Setup removes
                        // the special collision nibble immediately (leaving the block
                        // type-eight solid), and this downward scan remains clipped.
                        if (!terrain.RoomObjects.BlockReactions.TrySpawnSamusContactCrumbleBlock(
                                terrain.Level,
                                block.At,
                                crumble))
                            break;
                    }
                    // Pose expansion carries direction $F, not a downward
                    // landing. Keep solidity without waking the save-station owner.
                    if (move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
                        block.Behavior.TryGetStationAccess(out _) &&
                        (terrain.RoomObjects is null ||
                         !terrain.RoomObjects.Stations.TryNotifyStationCollision(
                             terrain.Level,
                             block.At,
                             block.Behavior,
                             state.CollisionPose.ForRoomCollision(),
                             Axis.Vertical,
                             movingPositive: acceptedDisplacement.IsPositive)))
                    {
                        throw new InvalidOperationException(
                            $"Station access block {block.Position} behaviour ({block.Behavior}) " +
                            "has no active map/resource/save-station room-object owner.");
                    }
                    acceptedDisplacement = ClipVerticalToSolid(
                        state,
                        acceptedDisplacement,
                        leadingBoundary);
                    collided = true;
                    break;

                case RoomCollisionType.BombableBlock:
                    // Vertical dispatch shares the exact room-object
                    // setup and collision contract documented in the horizontal branch above.
                    if (terrain.RoomObjects?.Quicksand.IsAllocationFull == true)
                    {
                        // The full room-object pool prevents rejection setup from stopping
                        // Samus. Block collision therefore lets her pass and
                        // treats an untouched bomb block as air.
                        break;
                    }
                    if (block.Behavior.Reaction.IsAreaSpecific || !CanBreakCollisionBombBlock(owner, move.CanBreakBombBlocks))
                    {
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        break;
                    }
                    if (!block.Behavior.Reaction.IsCommonEntryBelow(8))
                    {
                        throw new InvalidDataException(
                            $"Collision bomb block {block.Position} has invalid behaviour ({block.Behavior}).");
                    }
                    RoomObjectsBreaking(terrain, block.At).BlockReactions
                        .TrySpawnCollisionBombBlock(terrain.Level, block.At, block.Behavior);
                    break;

                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                default:
                    throw new InvalidDataException(
                        $"Block {block.Position} type {block.CollisionType} escaped the " +
                        "complete vertical collision dispatcher.");
            }

            if (collided)
                break;
        }
        return new VerticalTerrainScan(acceptedDisplacement, collided, sandContact);
    }

    /// <summary>
    /// The single-block collision check after a change of pose:
    /// the terrain scan alone, in the downward direction, then the probe's position addition.
    /// Unlike the ordinary vertical entry it has no zero-distance exit, so a zero probe still tests the
    /// blocks under the current bottom boundary. It checks no solid enemies.
    /// </summary>
    /// <param name="level">The room's collision blocks.</param>
    /// <param name="state">A collision probe copy; its position receives the accepted distance.</param>
    /// <param name="owner">The Samus whose collision reactions act, or null for an ownerless probe.</param>
    /// <param name="displacement">Signed 16.16 distance to test; zero tests the current bottom boundary.</param>
    /// <param name="scanLeftToRight">Scan order chosen by the frame counter's parity.</param>
    /// <param name="roomObjects">The room's room-object owner for special and bombable blocks.</param>
    internal static BlockMoveResult ProbeChangedPoseVertical(

        RoomLevelData level,
        SamusKinematicsState state,
        SamusState? owner,
        SignedSixteenSixteen displacement,
        bool scanLeftToRight,
        ISamusRoomObjects? roomObjects)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(state);
        VerticalTerrainScan scan = ScanVerticalTerrain(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, owner, new VerticalTerrainProbe { Displacement = displacement, ScanLeftToRight = scanLeftToRight, CanBreakBombBlocks = false, PublishDoorSideEffects = true, BlockReactionDirection = SamusCollisionDirection.NonDirectionalProbe });
        state.Position = state.Position.AdvanceY(scan.AcceptedDisplacement);
        return new BlockMoveResult(scan.AcceptedDisplacement, scan.Collided);
    }

    /// <summary>The room objects that run the reaction of the bomb block at <paramref name="block"/> a scan breaks.</summary>
    /// <exception cref="InvalidOperationException">
    /// The scan runs without the room's objects, as the scans of rooms with no bomb blocks do, so it cannot start one.
    /// </exception>
    private static ISamusRoomObjects RoomObjectsBreaking(SamusTerrain terrain, Position16<RoomBlocks> block) =>
        terrain.RoomObjects ?? throw new InvalidOperationException(
            $"A scan without the room's objects broke bomb block {block}, whose reaction only the room's objects can run.");

    /// <summary>
    /// Runs the shared bomb-block setup for non-solid cells without consuming its collision result.
    /// </summary>
    private static void ActivateCollisionBombableAir(SamusTerrain terrain, RoomCollisionBlock block,
        SamusState? owner, bool explicitAdmission)
    {
        // The game's horizontal/vertical air dispatch invokes the same setup as solid
        // bomb blocks, but ignores its collision result. Rejection or exhausted actor slots
        // must never turn this non-solid cell into a movement obstruction.
        if (block.Behavior.Reaction.IsAreaSpecific || !CanBreakCollisionBombBlock(owner, explicitAdmission))
            return;
        RoomObjectsBreaking(terrain, block.At).BlockReactions.TrySpawnCollisionBombBlock(terrain.Level, block.At, block.Behavior);
    }

    /// <summary>
    /// The bomb-block setup reads the live boost stage and pose for every scan that reaches it,
    /// independently of the movement handler; this includes the observational wall-jump
    /// probe. Explicit admissions remain for ownerless probes.
    /// </summary>
    private static bool CanBreakCollisionBombBlock(SamusState? owner, bool explicitAdmission) =>
        explicitAdmission || (owner is not null && SamusPoseMetadata.BreaksCollisionBombBlocks(owner));

    /// <summary>
    /// Resolves a non-square slope against Samus's leading vertical boundary and retains
    /// the clipped 16.16 distance when the slope intercepts the requested travel.
    /// </summary>
    private static SlopeClip ClipVerticalToNonSquareSlope(
        SamusKinematicsState state,
        RoomCollisionBlock block,
        int blockX,
        SignedSixteenSixteen displacement,
        int targetCenter,
        bool reactsDownward)
    {
        bool movingDown = reactsDownward;

        // Non-square slopes react vertically only under Samus's center column. The broader
        // X-radius scan still visits neighboring blocks, but the slope reaction rejects them here.
        if ((state.Position.X.Whole >> 4) != blockX)
            return new SlopeClip(displacement, false);

        if (movingDown)
        {
            if (block.Behavior.Slope.FlipsVertically)
                return new SlopeClip(displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(block.Behavior, state.Position.X.Whole);
            int bottomDepth = RoomGridConversions.OffsetWithinBlock(state.Radius.Y + targetCenter - 1);
            int correction = CoordinateAxis.Difference(height - bottomDepth, 1);
            if (correction > 0)
                return new SlopeClip(displacement, false);

            return new SlopeClip(ClippedToSlope(displacement, correction), true);
        }
        else
        {
            if (!block.Behavior.Slope.FlipsVertically)
                return new SlopeClip(displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(block.Behavior, state.Position.X.Whole);
            int topDepth = RoomGridConversions.OffsetFromBlockEnd(targetCenter - state.Radius.Y);
            int correction = CoordinateAxis.Difference(height - topDepth, 1);
            if (correction > 0)
                return new SlopeClip(displacement, false);

            return new SlopeClip(ClippedToSlope(displacement, correction), true);
        }

        // The clipped travel is the displacement's whole pixels moved back by the slope's correction, never below
        // zero, the fraction dropped. The correction is zero or negative, so the result lies within 0 through 32767.
        static SignedSixteenSixteen ClippedToSlope(SignedSixteenSixteen displacement, int correction) =>
            SignedSixteenSixteen.FromWholePixels(Math.Max(0, displacement.WholePixels + correction));
    }

    /// <summary>
    /// Reacts to a square slope during horizontal block collision.
    /// </summary>
    private static bool ReactHorizontalSquareSlope(
        SamusKinematicsState state,
        RoomCollisionBlock block,
        SignedSixteenSixteen displacement,
        int leadingBoundary,
        int remainingRows,
        int totalRows,
        out SignedSixteenSixteen clippedDisplacement)
    {
        // The leading X boundary's eight-pixel half selects the column; the scan starts in the top row and then
        // tests the row below it.
        SquareSlopeShape shape = SquareSlopes.ShapeOf(block.Behavior);
        SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeQuadrant.ColumnFor((leadingBoundary & 8) != 0), SquareSlopeRowHalf.Top);
        bool selectedSolid = SquareSlopes.IsSolid(shape, quadrant);
        bool collide;

        if (remainingRows == 0)
        {
            // On the bottom scanned block, the body's bottom half can accept the selected
            // quadrant directly. Otherwise the routine falls through to the opposite
            // vertical quadrant test below.
            if (((state.Radius.Y + state.Position.Y.Whole - 1) & 8) == 0 && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopes.IsSolid(shape, quadrant.OtherRow);
        }
        else
        {
            // The first scanned row is remainingRows==totalRows; intermediate rows always
            // test the selected quadrant. The top-body bit supplies the exception on
            // that first row before the opposite vertical quadrant is consulted.
            if (remainingRows != totalRows || ((state.Position.Y.Whole - state.Radius.Y) & 8) == 0)
            {
                if (selectedSolid)
                {
                    goto Clip;
                }
            }

            collide = SquareSlopes.IsSolid(shape, quadrant.OtherRow);
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
    /// Reacts to a square slope during vertical block collision.
    /// </summary>
    private static bool ReactVerticalSquareSlope(
        SamusKinematicsState state,
        RoomCollisionBlock block,
        SignedSixteenSixteen displacement,
        int leadingBoundary,
        int blocksLeftToCheck,
        int totalColumns,
        out SignedSixteenSixteen clippedDisplacement)
    {
        // Vertical collision takes the row from the leading Y boundary's eight-pixel half. The horizontal counter is
        // the number of physical blocks left: left-to-right counts span..0 while right-to-left counts 0..span.
        SquareSlopeShape shape = SquareSlopes.ShapeOf(block.Behavior);
        SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeColumnHalf.Left, SquareSlopeQuadrant.RowFor((leadingBoundary & 8) != 0));
        bool selectedSolid = SquareSlopes.IsSolid(shape, quadrant);
        bool collide;

        // A blocks-left count of zero always denotes the physical rightmost block, independent of
        // which of the two alternating scan routines reached it. A count equal to the span denotes the physical leftmost
        // block. Interior blocks necessarily test both eight-pixel halves.
        if (blocksLeftToCheck == 0)
        {
            if (((state.Radius.X + state.Position.X.Whole - 1) & 8) == 0 && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
        }
        else
        {
            if (blocksLeftToCheck != totalColumns ||
                ((state.Position.X.Whole - state.Radius.X) & 8) == 0)
            {
                if (selectedSolid)
                {
                    goto Clip;
                }
            }

            collide = SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
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
        // Publishes support on a downward square-slope hit. Without this
        // latch, fast running probes farther than the eight-pixel floor thickness
        // on the next frame and can embed Samus below its surface.
        if (!displacement.IsNegative) state.PositionAdjustedBySlope = true;
        return true;
    }

    /// <summary>Clips horizontal travel at a solid block and writes the X fraction for the hit side.</summary>
    private static SignedSixteenSixteen ClipHorizontalToSolid(
        SamusKinematicsState state,
        SignedSixteenSixteen displacement,
        int leadingBoundary)
    {
        if (displacement.IsNegative)
        {
            int whole = CoordinateAxis.AsSigned(state.Radius.X + (leadingBoundary | 15) + 1 - state.Position.X.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(0);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
        else
        {
            int whole = CoordinateAxis.Difference((leadingBoundary & ~15) - state.Radius.X, state.Position.X.Whole);
            if (whole < 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(ushort.MaxValue);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
    }

    /// <summary>Clips horizontal travel at the occupied half of a square slope tile.</summary>
    private static SignedSixteenSixteen ClipHorizontalToSquareSlope(
        SamusKinematicsState state,
        SignedSixteenSixteen displacement,
        int leadingBoundary)
    {
        if (displacement.IsNegative)
        {
            int whole = CoordinateAxis.AsSigned(
                state.Radius.X + (leadingBoundary | 7) + 1 - state.Position.X.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(0);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }

        int rightWhole = CoordinateAxis.Difference(
            (leadingBoundary & ~7) - state.Radius.X, state.Position.X.Whole);
        if (rightWhole < 0)
            rightWhole = 0;
        state.Position = state.Position.WithFractionX(ushort.MaxValue);
        return SignedSixteenSixteen.FromWholePixels(rightWhole);
    }

    /// <summary>Clips vertical travel at a solid block and writes the Y fraction for the hit side.</summary>
    private static SignedSixteenSixteen ClipVerticalToSolid(
        SamusKinematicsState state,
        SignedSixteenSixteen displacement,
        int leadingBoundary)
    {
        if (displacement.IsNegative)
        {
            int whole = CoordinateAxis.AsSigned(state.Radius.Y + (leadingBoundary | 15) + 1 - state.Position.Y.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(0);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
        else
        {
            int whole = CoordinateAxis.Difference((leadingBoundary & ~15) - state.Radius.Y, state.Position.Y.Whole);
            if (whole < 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(ushort.MaxValue);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
    }

    /// <summary>Clips vertical travel against a square slope, marking support on downward contact.</summary>
    private static SignedSixteenSixteen ClipVerticalToSquareSlope(
        SamusKinematicsState state,
        SignedSixteenSixteen displacement,
        int leadingBoundary)
    {
        if (displacement.IsNegative)
        {
            int whole = CoordinateAxis.AsSigned(
                state.Radius.Y + (leadingBoundary | 7) + 1 - state.Position.Y.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(0);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }

        int downWhole = CoordinateAxis.Difference(
            (leadingBoundary & ~7) - state.Radius.Y, state.Position.Y.Whole);
        if (downWhole < 0)
            downWhole = 0;
        state.Position = state.Position.WithFractionY(ushort.MaxValue);

        // Only the downward square-slope clip writes this flag. The no-speed vertical move consumes it to use
        // a one-pixel grounding probe instead of total-horizontal-speed-plus-one.
        state.PositionAdjustedBySlope = true;
        return SignedSixteenSixteen.FromWholePixels(downWhole);
    }

    /// <summary>Counts tile rows touched by Samus's current vertical collision extent.</summary>
    private static int GetVerticalBlockSpan(SamusKinematicsState state)
    {
        int alignedTop = CoordinateAxis.Wrap(state.Position.Y.Whole - state.Radius.Y);
        alignedTop &= ushort.MaxValue & ~15;
        return CoordinateAxis.Wrap(state.Radius.Y + state.Position.Y.Whole - 1 - alignedTop) >> 4;
    }

    /// <summary>Counts tile columns touched by Samus's current horizontal collision extent.</summary>
    private static int GetHorizontalBlockSpan(SamusKinematicsState state)
    {
        int alignedLeft = CoordinateAxis.Wrap(state.Position.X.Whole - state.Radius.X);
        alignedLeft &= ushort.MaxValue & ~15;
        return CoordinateAxis.Wrap(state.Radius.X + state.Position.X.Whole - 1 - alignedLeft) >> 4;
    }

    /// <summary>
    /// Follows collision-extension blocks exactly as block collision does: a horizontal extension
    /// moves its signed behaviour in blocks along the row; a vertical extension moves it in rows.
    /// Each link leads on in the room's reading order (<see cref="RoomReadingOrder"/>), so a horizontal link past a
    /// row's end continues on the next row. A zero behaviour reports no collision immediately and therefore behaves as air.
    /// </summary>
    /// <exception cref="InvalidDataException">A link leads outside the room's grid, or the chain is cyclic.</exception>
    internal static bool TryResolveExtension(
        RoomLevelData level,
        ref RoomCollisionBlock block)
    {
        int blockCount = level.Size.Area;
        for (int hops = 0; hops < blockCount; hops++)
        {
            switch (block.CollisionType)
            {
                case RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension when block.Behavior.EndsExtension:
                    return false;
                case RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension:
                    break;
                case RoomCollisionType.Air or RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
                    RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or RoomCollisionType.UnusedAir or
                    RoomCollisionType.BombableAir or RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock or
                    RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
                    RoomCollisionType.ShootableBlock or RoomCollisionType.GrappleBlock or
                    RoomCollisionType.BombableBlock:
                    return true;
                default:
                    throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}.");
            }

            Position16<RoomBlocks> target = level.GridBlockStepped(block.At, block.ExtensionLink) ??
                throw new InvalidDataException(
                    $"Collision extension block {block.Position} type {block.CollisionType}/" +
                    $"behaviour ({block.Behavior}) resolves outside the room's grid.");
            block = level.GetCollisionBlock(target);
        }

        throw new InvalidDataException(
            $"Collision extension chain beginning at block {block.Position} contains a cycle.");
    }

    /// <summary>
    /// The collision check after a grapple, using the vertical dispatcher.
    /// Call before replacing the connected pose and its collision radius.
    /// </summary>
    public static void EjectAfterGrapple(RoomLevelData level, SamusKinematicsState state)
    {
        // The game's horizontal scans only populate scratch distances: this routine never
        // applies them. These probes do not trigger room objects, doors, damage, or enemies.
        int up = GrappleVerticalOverlap(level, state, direction: VerticalDirection.Down);
        int down = GrappleVerticalOverlap(level, state, direction: VerticalDirection.Up);
        if (up == 0 || down != 0) return;
        state.Position -= new Offset16<RoomPixels>(0, up);
        // Tall bodies may overlap two rows. Retail repeats once, not until clear.
        if (state.Radius.Y >= 16)
            state.Position = state.Position.WithWrappedWholeY(state.Position.Y.Whole - GrappleVerticalOverlap(level, state, direction: VerticalDirection.Down));
    }

    /// <summary>Pixels along a half block's side, the depth unit of a square slope's half.</summary>
    private const int HalfBlockPixels = 8;

    /// <summary>
    /// Measures block overlap in the selected vertical direction after grapple releases
    /// Samus, allowing the caller to eject her before the new pose radius is installed.
    /// </summary>
    private static int GrappleVerticalOverlap(RoomLevelData level, SamusKinematicsState state, VerticalDirection direction)
    {
        int boundary = CoordinateAxis.Wrap(state.Position.Y.Whole + (direction == VerticalDirection.Down ? state.Radius.Y - 1 : -state.Radius.Y));
        int left = CoordinateAxis.Wrap(state.Position.X.Whole - state.Radius.X);
        int right = CoordinateAxis.Wrap(state.Position.X.Whole + state.Radius.X - 1);
        int firstColumn = left >> 4;
        int lastColumn = right >> 4;
        int maximum = 0;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockOrEdgeSolid(new(CoordinateAxis.Wrap(column), CoordinateAxis.Wrap(boundary >> 4)));
            int depth;
            // The post-grapple table treats even vertical extensions as solid, without
            // following behaviour. Ordinary movement's dispatcher is not interchangeable.
            if (block.CollisionType.IsSolidKind())
                depth = boundary & 15;
            else if (block.CollisionType != RoomCollisionType.Slope)
                continue;
            else if (block.Behavior.Slope.IsNonSquare)
            {
                if (column != state.Position.X.Whole >> 4) continue;
                if (block.Behavior.Slope.FlipsVertically == (direction == VerticalDirection.Down))
                    depth = boundary & 15;
                else
                {
                    int difference = SamusSlopePhysics.ReadAlignmentHeight(block.Behavior, state.Position.X.Whole) - (boundary & 15) - 1;
                    depth = direction == VerticalDirection.Down ? (difference <= 0 ? -1 - difference : -1) : (difference >= 0 ? difference : -1);
                }
            }
            else
            {
                SquareSlopeShape shape = SquareSlopes.ShapeOf(block.Behavior);
                SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeColumnHalf.Left, SquareSlopeQuadrant.RowFor((boundary & 8) != 0));
                bool selected = SquareSlopes.IsSolid(shape, quadrant);
                bool collides;
                if (column == lastColumn)
                    collides = (right & 8) == 0 ? selected : selected || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
                else if (column != firstColumn || (left & 8) == 0)
                    collides = selected || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
                else
                    collides = SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
                int halfBlockOffset = boundary % HalfBlockPixels;
                depth = collides ? (direction == VerticalDirection.Down ? halfBlockOffset : HalfBlockPixels - 1 - halfBlockOffset) : -1;
            }
            maximum = Math.Max(maximum, depth + 1);
        }
        return maximum;
    }
}
