using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Movement;

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
    /// <summary>
    /// Ports the block-only observation made by <c>WallJumpBlockCollisionDetection</c> at
    /// <c>$94:967F</c>. The native routine publishes available distance in `$12` but does
    /// not commit Samus's whole-pixel probe position. Collision still writes subpixels.
    /// </summary>
    internal static BlockMoveResult ProbeWallHorizontal(

        RoomLevelData level,
        SamusKinematicsState state,
        SamusState? owner,
        int signedDistance,
        ISamusRoomPlms? plms = null)
    {
        Ensure.NotNull(state);
        SamusKinematicsState probe = state.CreateCollisionProbe();

        // Reusing the translated horizontal dispatcher also preserves square-slope and
        // unsupported-block behavior. Any post-scan slope alignment touches only `probe`,
        // but bank-$94 collision side effects are not observational: touching a scroll
        // trigger still wakes its bank-$84 owner. Pass that shared owner through rather
        // than treating an extension-resolved trigger as an orphan during wall-jump checks.
        BlockMoveResult result = MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, probe, owner, new HorizontalBlockMove { Displacement = signedDistance, BlockReactionDirection = SamusCollisionDirection.NonDirectionalProbe });
        // Submerging sand writes these real motion words even through wall observation.
        state.YSpeed = probe.YSpeed;
        state.YAcceleration = probe.YAcceleration;

        // $94:8F49 (and square-slope clipping) writes Samus's real X subposition
        // even through the observational $94:967F entry point. The probe's accepted
        // displacement is whole pixels on a block hit, so its final fractional word
        // retains that write. Keep the live integer X unchanged. Dropping this write
        // let away movement accumulate fractional speed during the wall-check pose.
        if (result.Collided && result.EnemyCollision is null)
            state.Position = state.Position.WithFractionX(probe.Position.X.Fraction);

        // The bank-$A0 routine preceding the block scan has a separate
        // real side effect: its touching path executes STZ SamusYSubPosition. Copy that single
        // native write back without committing the probe's X movement or slope alignment.
        if (result.EnemyCollision is { WasTouching: true })
            state.Position = state.Position.WithFractionY(0);

        return result;
    }

    /// <summary>
    /// Ports <c>BlockColl_Handle_Horiz</c> at <c>$94:9543</c>, the position addition in
    /// <c>Samus_MoveRight_NoSolidColl</c>, and the subsequent non-square slope alignment.
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

        int acceptedDisplacement = move.Displacement;
        bool collided = false;
        SolidEnemyCollisionResult? enemyCollision = null;
        RoomCollisionBlock? collisionBlock = null;
        RoomCollisionBlock? brokenBombBlock = null;

        if ((acceptedDisplacement != 0 || move.CollisionMovementDirection.HasValue) && state.InteractiveEnemies.Count != 0)
        {
            // `$90:9350/$93B1` presents bank $A0 with an unsigned magnitude even though the
            // managed block mover receives signed 16.16 displacement. Preserve both halves:
            // enemy clipping clears the fractional half only when it actually collides.
            uint magnitude = acceptedDisplacement < 0
                ? unchecked((uint)(-acceptedDisplacement))
                : unchecked((uint)acceptedDisplacement);
            // `LDA $12 : BMI` selects the entry: zero is not negative and enters right.
            // The probe itself reads the calculator's stored direction, so a zero move
            // still detects tangency (and clears fractional Y) in the facing direction.
            bool entersLeft = acceptedDisplacement < 0;
            SamusCollisionDirection direction = move.CollisionMovementDirection ??
                (entersLeft ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                SnesFixedPosition.WholeOf(unchecked((int)magnitude)),
                unchecked((ushort)magnitude));
            state.RecordSolidEnemyCollision(direction, probe.EnemyIndex);

            if (probe.Collided)
            {
                // On enemy collision the bank-$90 wrapper skips bank-$94 block detection and
                // invokes the corresponding no-collision position adder with `$12.0000`.
                int clippedMagnitude = SnesSixteenSixteen.FromWhole(probe.Distance);
                acceptedDisplacement = entersLeft
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
                SnesSixteenSixteen.WholeOf(state.Position.X.RawValue + (uint)acceptedDisplacement)));
            ushort leadingBoundary = acceptedDisplacement >= 0
                ? unchecked((ushort)(state.Radius.X + targetCenter - 1))
                : unchecked((ushort)(targetCenter - state.Radius.X));

            int topBlockY = unchecked((ushort)(state.Position.Y.Whole - state.Radius.Y)) >> 4;
            int verticalSpan = GetVerticalBlockSpan(state);
            int blockX = leadingBoundary >> 4;

            for (int rowOffset = 0; rowOffset <= verticalSpan; rowOffset++)
            {
                RoomCollisionBlock block = terrain.Level.GetCollisionBlockOrPrefilledSolid(
                    new(unchecked((ushort)blockX), unchecked((ushort)(topBlockY + rowOffset))));
                if (!TryResolveExtension(terrain.Level, ref block))
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
                            block.Bts.Value,
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
                        DoorListEntry horizontalDoor = terrain.Level.ResolveDoorCollision(
                            block.Bts.Value,
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
                        // Horizontal table entries 2/4/6 are literal clear-carry stubs.
                        break;
                    case RoomCollisionType.BombableAir:
                        ActivateCollisionBombableAir(terrain.Level, block, owner, move.CanBreakBombBlocks, terrain.Plms);
                        break;

                    case RoomCollisionType.SpecialAir:
                        // Sand's submerging callback clears vertical speed/gravity even
                        // when reached through the horizontal dispatcher.
                        collided = SamusInsideBlockReactions.ReactCollision(new(state, state.CollisionContactDamageIndex(owner)), block, Axis.Horizontal,
                            ref acceptedDisplacement, out _, move.BlockReactionDirection, terrain.Plms);
                        if (collided) collisionBlock = block;
                        if (block.Bts == RoomBlockBehaviorValues.ScrollTrigger &&
                            (terrain.Plms is null || !terrain.Plms.Scrolls.TryNotifyScrollTouch(block.Index)))
                        {
                            throw new InvalidOperationException(
                                $"Scroll trigger block {block.Index} has no active $B703 PLM owner; " +
                                $"live=[{string.Join(',', terrain.Plms?.Scrolls.Snapshots.Select(scroll => scroll.BlockIndex) ?? [])}].");
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
                        block.Bts == RoomBlockBehaviorValues.CollectibleTrigger:
                        // Visible item frames are type-$B/BTS-$45. Bank $94 spawns the
                        // shared $EED3 detector, whose setup triggers the item at this
                        // origin and returns carry clear; Samus therefore passes through
                        // rather than clipping against the item's visual block.
                        if (terrain.Plms is null || !terrain.Plms.CollectiblePlms.TryNotifyCollectibleTouch(block.Index))
                        {
                            throw new InvalidOperationException(
                                $"Collectible block {block.Index} has no active item PLM owner.");
                        }
                        break;

                    case RoomCollisionType.SpecialBlock:
                        // Both special-solid dispatchers enter Spawn_PLM with clear carry.
                        // A full pool returns without running setup, so even a block whose
                        // setup would normally reject contact falls through as air.
                        if (terrain.Plms?.Quicksand.IsAllocationFull == true)
                            break;
                        // `$94:90CB` dispatches the speed-block entries through setup
                        // `$84:CDEA`. A stage-four boost or directional shinespark clears
                        // collision synchronously, so this same horizontal scan continues
                        // through the newly non-solid cell.
                        if (owner is { } speedBoostingSamus &&
                            terrain.Plms is not null &&
                            terrain.Plms.BlockReactions.TrySpawnSamusSpeedBoosterBlock(
                                terrain.Level,
                                block.Index,
                                block.Bts,
                                speedBoostingSamus.RoomView))
                        {
                            break;
                        }
                        // Native station setups reject direction $F before finding their
                        // parent. An observational scan still clips against the access
                        // tile, but must not activate the station or demand an owner.
                        if (move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
                            block.Bts.TryGetStationAccess(out _) &&
                            (terrain.Plms is null ||
                             !terrain.Plms.Stations.TryNotifyStationCollision(
                                 block.Index,
                                 block.Bts,
                                 state.CollisionPose.ForRoomCollision(),
                                 Axis.Horizontal,
                                 movingPositive: acceptedDisplacement > 0,
                                 roomWidthInBlocks: terrain.Level.Size.X)))
                        {
                            throw new InvalidOperationException(
                                $"Station access block {block.Index} BTS ${block.Bts.Value:X2} " +
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
                        if (terrain.Plms?.Quicksand.IsAllocationFull == true)
                            break;
                        if (block.Bts.UsesAreaReactionTable || !CanBreakCollisionBombBlock(owner, move.CanBreakBombBlocks))
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
                                $"Collision bomb block {block.Index} has invalid BTS ${block.Bts.Value:X2}.");
                        }
                        // Production movement supplies the active room PLM owner, matching
                        // `$94:933F`'s immediate long call into Spawn_PLM. Keeping the null
                        // fallback preserves this low-level collision routine as a usable
                        // setup-only unit seam: it performs CE83's synchronous clear, while
                        // explicitly omitting the later independent animation lifecycle.
                        bool spawned = terrain.Plms is null
                            ? ClearCollisionTypeWithoutLifecycle(terrain.Level, block.Index)
                            : terrain.Plms.BlockReactions.TrySpawnCollisionBombBlock(terrain.Level, block.Index, block.Bts);
                        if (spawned)
                            brokenBombBlock ??= block;
                        break;

                    case RoomCollisionType.HorizontalExtension:
                    case RoomCollisionType.VerticalExtension:
                    default:
                        throw new InvalidDataException(
                            $"Block {block.Index} type ${(int)block.CollisionType:X1} escaped the " +
                            "complete horizontal collision dispatcher.");
                }

                if (collided)
                    break;
            }
        }

        state.Position = state.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(acceptedDisplacement));

        // Only the live bank-$90 movement wrapper publishes inheritance. Copies used
        // for prospective poses and wall probes must not replace the preceding move.
        if (owner is { } mover && ReferenceEquals(mover.Kinematics, state) &&
            move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
            move.AlignToSlopeAfterMovement)
            mover.ProjectileInheritance.Record(
                move.Displacement < 0 ? SamusCollisionDirection.Left : SamusCollisionDirection.Right,
                acceptedDisplacement);

        // $90:9350/$90:93B1 call $94:87F4 after both collision and non-collision paths.
        // Direct bank-$94 callers (notably the prospective-running-pose probe)
        // omit that wrapper step and must retain the unaligned whole-pixel Y.
        SlopeAlignmentResult alignment = SamusSlopePhysics.AlignYPosition(
            terrain.Level,
            state.Position.Whole,
            state.Radius.Y,
            horizontalSlopeCollisionEnabled: move.AlignToSlopeAfterMovement &&
                (state.HorizontalSlopeCollisionEnable & 2) != 0);
        state.Position = state.Position.WithWholeY(alignment.Position.Y);
        // $94:87F4 only sets this latch; it does not clear a square-floor contact
        // published by the previous vertical pass. $90:923F consumes it before
        // $94:9763 clears it on entry to the next nonzero terrain Y move.
        state.PositionAdjustedBySlope |= alignment.Adjusted;

        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            enemyCollision);
    }

    /// <summary>
    /// Ports the left-to-right/right-to-left vertical scans at <c>$94:959E/$94:95F5</c>
    /// plus <c>Samus_MoveDown_NoSolidColl</c>'s final fixed-point position addition.
    /// </summary>
    public static BlockMoveResult MoveVertical(SamusTerrain terrain, SamusKinematicsState state, SamusState? owner, VerticalBlockMove move)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(state);

        int acceptedDisplacement = move.Displacement;
        bool collided = false;
        bool sandContact = false;
        SolidEnemyCollisionResult? enemyCollision = null;

        // Most bank-$90 callers enter through MoveSamus_Up/Down and therefore probe the
        // native solid-enemy list before dispatching bank-$94 terrain. A few callers use
        // the explicitly named `$94:9763` *NoSolidEnemyCollision* entry instead. Keep that
        // distinction as an argument at the shared collision seam; clearing the caller's
        // enemy snapshot would mutate unrelated gameplay state and would make the omission
        // impossible to verify.
        // A zero-speed bounce apex still enters the downward solid-enemy probe.
        // Exact platform tangency can therefore finish the bounce this frame;
        // only the later terrain dispatcher skips a zero displacement.
        if (move.IncludeSolidEnemies && state.InteractiveEnemies.Count != 0)
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
                SnesFixedPosition.WholeOf(unchecked((int)magnitude)),
                unchecked((ushort)magnitude));
            state.RecordSolidEnemyCollision(direction, probe.EnemyIndex);

            if (probe.Collided)
            {
                // Solid-enemy success bypasses the block dispatcher and moves only to the
                // current enemy boundary. This is also how native vertical-collision flags
                // distinguish an enemy stop from an unobstructed terrain move.
                int clippedMagnitude = SnesSixteenSixteen.FromWhole(probe.Distance);
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
            VerticalTerrainScan scan = ScanVerticalTerrain(terrain, state, owner, new VerticalTerrainProbe { Displacement = acceptedDisplacement, ScanLeftToRight = move.ScanLeftToRight, CanBreakBombBlocks = move.CanBreakBombBlocks, PublishDoorSideEffects = move.PublishDoorSideEffects, BlockReactionDirection = move.BlockReactionDirection });
            acceptedDisplacement = scan.AcceptedDisplacement;
            collided = scan.Collided;
            sandContact = scan.SandContact;
        }

        state.Position = state.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(acceptedDisplacement));
        if (owner is { } mover && ReferenceEquals(mover.Kinematics, state) &&
            move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe)
            mover.ProjectileInheritance.Record(
                move.Displacement < 0 ? SamusCollisionDirection.Up : SamusCollisionDirection.Down,
                acceptedDisplacement);
        // $94:9763 publishes sand contact after either signed movement direction.
        // Pose probes instead consume only the block handler's carry result.
        if (move.PublishQuicksandGrounding && sandContact)
            collided = true;
        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            EnemyCollision: enemyCollision);
    }

    /// <summary>Vertical scan result including sand contact, which can affect motion without blocking travel.</summary>
    [IndependentFlag("independent scan facts: a collision, and sand contact")]
    private readonly record struct VerticalTerrainScan(int AcceptedDisplacement, bool Collided, bool SandContact);

    /// <summary>
    /// The vertical block scans at <c>$94:959E</c> (left to right) and <c>$94:95F5</c>
    /// (right to left): every block under the leading boundary, dispatched by collision type.
    /// A zero displacement scans downward from the current bottom boundary.
    /// </summary>
    private static VerticalTerrainScan ScanVerticalTerrain(SamusTerrain terrain, SamusKinematicsState state, SamusState? owner, VerticalTerrainProbe move)
    {
        int acceptedDisplacement = move.Displacement;
        bool collided = false;
        bool sandContact = false;
        ushort targetCenter = unchecked((ushort)(
            SnesSixteenSixteen.WholeOf(state.Position.Y.RawValue + (uint)acceptedDisplacement)));
        ushort leadingBoundary = acceptedDisplacement >= 0
            ? unchecked((ushort)(state.Radius.Y + targetCenter - 1))
            : unchecked((ushort)(targetCenter - state.Radius.Y));
        int blockY = leadingBoundary >> 4;
        int horizontalSpan = GetHorizontalBlockSpan(state);
        int firstBlockX = move.ScanLeftToRight
            ? unchecked((ushort)(state.Position.X.Whole - state.Radius.X)) >> 4
            : unchecked((ushort)(state.Position.X.Whole + state.Radius.X - 1)) >> 4;

        for (int offset = 0; offset <= horizontalSpan; offset++)
        {
            int blockX = move.ScanLeftToRight ? firstBlockX + offset : firstBlockX - offset;
            RoomCollisionBlock block = terrain.Level.GetCollisionBlockOrPrefilledSolid(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
            if (!TryResolveExtension(terrain.Level, ref block))
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
                            blocksLeftToCheck: move.ScanLeftToRight
                                ? horizontalSpan - offset
                                : offset,
                            totalColumns: horizontalSpan,
                            out int clippedDisplacement);
                        if (squareCollision)
                        {
                            acceptedDisplacement = clippedDisplacement;
                            collided = true;
                        }
                        break;
                    }

                    // $94:86FE selects its floor or ceiling test from bit 0 of
                    // CollisionMovementDirection, not the distance's sign. The changed-
                    // pose probe's direction $F therefore tests both ways as a floor.
                    (acceptedDisplacement, collided) = ClipVerticalToNonSquareSlope(
                        state,
                        block,
                        blockX,
                        acceptedDisplacement,
                        targetCenter,
                        reactsDownward: move.BlockReactionDirection is { } reactionDirection
                            ? ((ushort)reactionDirection & 1) != 0
                            : acceptedDisplacement >= 0);
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
                    // `$94:93CE` is the vertical twin of the handler above. Preserve
                    // its carry result here; the room-level owner publishes the same
                    // native door pointer for the frontend dispatcher to consume.
                    DoorListEntry verticalDoor = terrain.Level.ResolveDoorCollision(
                        block.Bts.Value,
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
                        collided = SamusInsideBlockReactions.ReactCollision(new(state, state.CollisionContactDamageIndex(owner)), block, Axis.Vertical,
                            ref acceptedDisplacement, out bool touchedSand, move.BlockReactionDirection, terrain.Plms);
                        sandContact |= touchedSand;
                    }
                    if (block.CollisionType == RoomCollisionType.SpecialAir &&
                        block.Bts == RoomBlockBehaviorValues.ScrollTrigger &&
                        (terrain.Plms is null || !terrain.Plms.Scrolls.TryNotifyScrollTouch(block.Index)))
                    {
                        throw new InvalidOperationException(
                            $"Scroll trigger block {block.Index} has no active $B703 PLM owner; " +
                            $"live=[{string.Join(',', terrain.Plms?.Scrolls.Snapshots.Select(scroll => scroll.BlockIndex) ?? [])}].");
                    }
                    // Scroll wake-up itself remains carry-clear; sand has a separate
                    // carry/contact contract handled above.
                    break;
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnusedAir:
                    // These air entries return clear carry without PLM setup.
                    break;
                case RoomCollisionType.BombableAir:
                    ActivateCollisionBombableAir(terrain.Level, block, owner, move.CanBreakBombBlocks, terrain.Plms);
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
                    block.Bts == RoomBlockBehaviorValues.CollectibleTrigger:
                    if (terrain.Plms is null || !terrain.Plms.CollectiblePlms.TryNotifyCollectibleTouch(block.Index))
                    {
                        throw new InvalidOperationException(
                            $"Collectible block {block.Index} has no active item PLM owner.");
                    }
                    break;

                case RoomCollisionType.SpecialBlock:
                    // Spawn_PLM preserves bank $94's clear carry when all forty slots
                    // are occupied. No setup runs, so the unchanged special block is
                    // treated as air for this collision probe.
                    if (terrain.Plms?.Quicksand.IsAllocationFull == true)
                        break;
                    // Hand reactions change state but return unconditional collision:
                    // even an admitted morph contact is clipped by this same scan.
                    if (owner is { } chozoSamus)
                        terrain.Plms?.ChozoStatueHand.NotifyChozoStatueHandCollision(terrain.Level, block, chozoSamus,
                            state.CollisionPose, movingDown: acceptedDisplacement > 0 &&
                                (move.BlockReactionDirection ?? SamusCollisionDirection.Down) == SamusCollisionDirection.Down);
                    // The vertical special-solid dispatcher shares setup `$84:CDEA`
                    // with horizontal collision. Accepted boost contact becomes air
                    // before clipping; rejected/non-speed special blocks remain solid.
                    if (owner is { } speedBoostingSamus &&
                        terrain.Plms is not null &&
                        terrain.Plms.BlockReactions.TrySpawnSamusSpeedBoosterBlock(
                            terrain.Level,
                            block.Index,
                            block.Bts,
                            speedBoostingSamus.RoomView))
                    {
                        break;
                    }
                    if (!block.Bts.UsesAreaReactionTable &&
                        block.Bts.IsNormalReactionIndex(8) &&
                        acceptedDisplacement > 0 &&
                        // CE37 checks the native direction nibble, not the sign of
                        // the tested clearance. Pose probes use F and delete the PLM.
                        (move.BlockReactionDirection ?? SamusCollisionDirection.Down) == SamusCollisionDirection.Down)
                    {
                        if (terrain.Plms is null)
                        {
                            throw new InvalidOperationException(
                                $"Contact crumble block {block.Index} BTS " +
                                $"${block.Bts.Value:X2} requires an active room PLM owner.");
                        }

                        // `$94:9102` always returns carry set for CE37. Setup removes
                        // the special collision nibble immediately (leaving the block
                        // type-eight solid), and this downward scan remains clipped.
                        if (!terrain.Plms.BlockReactions.TrySpawnSamusContactCrumbleBlock(
                                terrain.Level,
                                block.Index,
                                block.Bts))
                            break;
                    }
                    // Pose expansion carries native direction $F, not a downward
                    // landing. Keep solidity without waking the save-station owner.
                    if (move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
                        block.Bts.TryGetStationAccess(out _) &&
                        (terrain.Plms is null ||
                         !terrain.Plms.Stations.TryNotifyStationCollision(
                             block.Index,
                             block.Bts,
                             state.CollisionPose.ForRoomCollision(),
                             Axis.Vertical,
                             movingPositive: acceptedDisplacement > 0,
                             roomWidthInBlocks: terrain.Level.Size.X)))
                    {
                        throw new InvalidOperationException(
                            $"Station access block {block.Index} BTS ${block.Bts.Value:X2} " +
                            "has no active map/resource/save-station PLM owner.");
                    }
                    acceptedDisplacement = ClipVerticalToSolid(
                        state,
                        acceptedDisplacement,
                        leadingBoundary);
                    collided = true;
                    break;

                case RoomCollisionType.BombableBlock:
                    // Vertical dispatch is `$94:934C` and shares the exact bank-$84
                    // setup/carry contract documented in the horizontal branch above.
                    if (terrain.Plms?.Quicksand.IsAllocationFull == true)
                    {
                        // The full native pool prevents rejection setup from setting
                        // carry. Bank $94 therefore keeps its pre-call clear carry and
                        // treats an untouched bomb block as air.
                        break;
                    }
                    if (block.Bts.UsesAreaReactionTable || !CanBreakCollisionBombBlock(owner, move.CanBreakBombBlocks))
                    {
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        break;
                    }
                    if (!block.Bts.IsNormalReactionIndex(8))
                    {
                        throw new InvalidDataException(
                            $"Collision bomb block {block.Index} has invalid BTS ${block.Bts.Value:X2}.");
                    }
                    if (terrain.Plms is null)
                        ClearCollisionTypeWithoutLifecycle(terrain.Level, block.Index);
                    else
                        terrain.Plms.BlockReactions.TrySpawnCollisionBombBlock(terrain.Level, block.Index, block.Bts);
                    break;

                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                default:
                    throw new InvalidDataException(
                        $"Block {block.Index} type ${(int)block.CollisionType:X1} escaped the " +
                        "complete vertical collision dispatcher.");
            }

            if (collided)
                break;
        }
        return new VerticalTerrainScan(acceptedDisplacement, collided, sandContact);
    }

    /// <summary>
    /// Ports <c>BlockCollisionDetectionDueToChangeOfPose_SingleBlock</c> at <c>$94:96E3</c>:
    /// the terrain scan alone, with native direction $F, then the probe's position addition.
    /// Unlike <c>$94:9763</c> it has no zero-distance exit, so a zero probe still tests the
    /// blocks under the current bottom boundary. It checks no solid enemies.
    /// </summary>
    /// <param name="level">The room's collision blocks.</param>
    /// <param name="state">A collision probe copy; its position receives the accepted distance.</param>
    /// <param name="owner">The Samus whose collision reactions act, or null for an ownerless probe.</param>
    /// <param name="displacement">Signed 16.16 distance to test; zero tests the current bottom boundary.</param>
    /// <param name="scanLeftToRight">Scan order chosen by the NMI frame counter's parity.</param>
    /// <param name="plms">The room's PLM owner for special and bombable blocks.</param>
    internal static BlockMoveResult ProbeChangedPoseVertical(

        RoomLevelData level,
        SamusKinematicsState state,
        SamusState? owner,
        int displacement,
        bool scanLeftToRight,
        ISamusRoomPlms? plms)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(state);
        VerticalTerrainScan scan = ScanVerticalTerrain(new SamusTerrain { Level = level, Plms = plms }, state, owner, new VerticalTerrainProbe { Displacement = displacement, ScanLeftToRight = scanLeftToRight, CanBreakBombBlocks = false, PublishDoorSideEffects = true, BlockReactionDirection = SamusCollisionDirection.NonDirectionalProbe });
        state.Position = state.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(scan.AcceptedDisplacement));
        return new BlockMoveResult(scan.AcceptedDisplacement, scan.Collided);
    }

    /// <summary>
    /// Executes collision-bomb setup without installing its independent PLM handler.
    /// </summary>
    /// <remarks>
    /// This narrowly retained fallback is for direct collision tests and embedders that do
    /// not own a room runtime. The playable runtime always passes <see cref="ISamusRoomPlms"/>
    /// and therefore never takes this incomplete branch.
    /// </remarks>
    private static bool ClearCollisionTypeWithoutLifecycle(RoomLevelData level, int blockIndex)
    {
        level.SetForegroundEntry(blockIndex, RoomPlmVisualBlockDefinitions.CollisionBombParent);
        return true;
    }

    /// <summary>
    /// Runs the shared bomb-block setup for non-solid cells without consuming carry.
    /// </summary>
    private static void ActivateCollisionBombableAir(RoomLevelData level, RoomCollisionBlock block,
        SamusState? owner, bool explicitAdmission, ISamusRoomPlms? plms)
    {
        // Native horizontal/vertical air dispatch invokes the same setup as solid
        // bomb blocks, but ignores its carry. Rejection or exhausted actor slots
        // must never turn this non-solid cell into a movement obstruction.
        if (block.Bts.UsesAreaReactionTable || !CanBreakCollisionBombBlock(owner, explicitAdmission))
            return;
        if (plms is not null)
            plms.BlockReactions.TrySpawnCollisionBombBlock(level, block.Index, block.Bts);
        else
            ClearCollisionTypeWithoutLifecycle(level, block.Index);
    }

    /// <summary>
    /// Setup $84:CE83 reads the live boost stage and pose for every scan that reaches it,
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
        int displacement,
        ushort targetCenter,
        bool reactsDownward)
    {
        bool movingDown = reactsDownward;

        // Non-square slopes react vertically only under Samus's center column. The broader
        // X-radius scan still visits neighboring blocks, but $94:86FE rejects them here.
        if ((state.Position.X.Whole >> 4) != blockX)
            return new SlopeClip(displacement, false);

        if (movingDown)
        {
            if (block.Bts.SlopeFlipsVertically)
                return new SlopeClip(displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(block.Bts, state.Position.X.Whole);
            int bottomNibble = unchecked((ushort)(state.Radius.Y + targetCenter - 1)) & 15;
            short correction = unchecked((short)(height - bottomNibble - 1));
            if (correction > 0)
                return new SlopeClip(displacement, false);

            int whole = unchecked((short)(SnesSixteenSixteen.WholeOf(displacement))) + correction;
            if (whole < 0)
                whole = 0;
            return new SlopeClip(SnesSixteenSixteen.FromWhole(whole), true);
        }
        else
        {
            if (!block.Bts.SlopeFlipsVertically)
                return new SlopeClip(displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(block.Bts, state.Position.X.Whole);
            int invertedTopNibble = ((targetCenter - state.Radius.Y) & 15) ^ 15;
            short correction = unchecked((short)(height - invertedTopNibble - 1));
            if (correction > 0)
                return new SlopeClip(displacement, false);

            int whole = unchecked((short)(SnesSixteenSixteen.WholeOf(displacement))) + correction;
            if (whole < 0)
                whole = 0;
            return new SlopeClip(SnesSixteenSixteen.FromWhole(whole), true);
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
        bool selectedSolid = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant) != 0;
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

            collide = selectedSolid || SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 2) != 0;
        }
        else
        {
            // The first scanned row is remainingRows==totalRows; intermediate rows always
            // test the selected quadrant. The top-body bit supplies the native exception on
            // that first row before the opposite vertical quadrant is consulted.
            if (remainingRows != totalRows || ((state.Position.Y.Whole - state.Radius.Y) & 8) == 0)
            {
                if (selectedSolid)
                {
                    goto Clip;
                }
            }

            collide = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 2) != 0;
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
        bool selectedSolid = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant) != 0;
        bool collide;

        // $1A=0 always denotes the physical rightmost block, independent of which of the
        // two alternating scan routines reached it. $1A=$1C denotes the physical leftmost
        // block. Interior blocks necessarily test both eight-pixel halves.
        if (blocksLeftToCheck == 0)
        {
            if (((state.Radius.X + state.Position.X.Whole - 1) & 8) == 0 && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
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

            collide = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
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

    /// <summary>Clips horizontal travel at a solid block and writes the native X fraction for the hit side.</summary>
    private static int ClipHorizontalToSolid(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(state.Radius.X + (leadingBoundary | 15) + 1 - state.Position.X.Whole));
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(0);
            return SnesSixteenSixteen.FromWhole(whole);
        }
        else
        {
            short whole = unchecked((short)((leadingBoundary & ~15) - state.Radius.X - state.Position.X.Whole));
            if (whole < 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(ushort.MaxValue);
            return SnesSixteenSixteen.FromWhole(whole);
        }
    }

    /// <summary>Clips horizontal travel at the occupied half of a square slope tile.</summary>
    private static int ClipHorizontalToSquareSlope(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(
                state.Radius.X + (leadingBoundary | 7) + 1 - state.Position.X.Whole));
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(0);
            return SnesSixteenSixteen.FromWhole(whole);
        }

        short rightWhole = unchecked((short)(
            (leadingBoundary & ~7) - state.Radius.X - state.Position.X.Whole));
        if (rightWhole < 0)
            rightWhole = 0;
        state.Position = state.Position.WithFractionX(ushort.MaxValue);
        return SnesSixteenSixteen.FromWhole(rightWhole);
    }

    /// <summary>Clips vertical travel at a solid block and writes the native Y fraction for the hit side.</summary>
    private static int ClipVerticalToSolid(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(state.Radius.Y + (leadingBoundary | 15) + 1 - state.Position.Y.Whole));
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(0);
            return SnesSixteenSixteen.FromWhole(whole);
        }
        else
        {
            short whole = unchecked((short)((leadingBoundary & ~15) - state.Radius.Y - state.Position.Y.Whole));
            if (whole < 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(ushort.MaxValue);
            return SnesSixteenSixteen.FromWhole(whole);
        }
    }

    /// <summary>Clips vertical travel against a square slope, marking support on downward contact.</summary>
    private static int ClipVerticalToSquareSlope(
        SamusKinematicsState state,
        int displacement,
        ushort leadingBoundary)
    {
        if (displacement < 0)
        {
            short whole = unchecked((short)(
                state.Radius.Y + (leadingBoundary | 7) + 1 - state.Position.Y.Whole));
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(0);
            return SnesSixteenSixteen.FromWhole(whole);
        }

        short downWhole = unchecked((short)(
            (leadingBoundary & ~7) - state.Radius.Y - state.Position.Y.Whole));
        if (downWhole < 0)
            downWhole = 0;
        state.Position = state.Position.WithFractionY(ushort.MaxValue);

        // Only the downward square-slope clip writes this flag. $90:923F consumes it to use
        // a one-pixel grounding probe instead of total-horizontal-speed-plus-one.
        state.PositionAdjustedBySlope = true;
        return SnesSixteenSixteen.FromWhole(downWhole);
    }

    /// <summary>Counts tile rows touched by Samus's current vertical collision extent.</summary>
    private static int GetVerticalBlockSpan(SamusKinematicsState state)
    {
        ushort alignedTop = unchecked((ushort)(state.Position.Y.Whole - state.Radius.Y));
        alignedTop &= ushort.MaxValue & ~15;
        return unchecked((ushort)(state.Radius.Y + state.Position.Y.Whole - 1 - alignedTop)) >> 4;
    }

    /// <summary>Counts tile columns touched by Samus's current horizontal collision extent.</summary>
    private static int GetHorizontalBlockSpan(SamusKinematicsState state)
    {
        ushort alignedLeft = unchecked((ushort)(state.Position.X.Whole - state.Radius.X));
        alignedLeft &= ushort.MaxValue & ~15;
        return unchecked((ushort)(state.Radius.X + state.Position.X.Whole - 1 - alignedLeft)) >> 4;
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
        int blockCount = checked(level.Size.X * level.Size.Y);
        for (int hops = 0; hops < blockCount; hops++)
        {
            int delta = block.CollisionType switch
            {
                RoomCollisionType.HorizontalExtension when
                    block.Bts != RoomBlockBehaviorValues.None => block.Bts.ExtensionOffset,
                RoomCollisionType.VerticalExtension when
                    block.Bts != RoomBlockBehaviorValues.None =>
                    block.Bts.ExtensionOffset * level.Size.X,
                RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => 0,
                RoomCollisionType.Air or RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
                    RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or RoomCollisionType.UnusedAir or
                    RoomCollisionType.BombableAir or RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock or
                    RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
                    RoomCollisionType.ShootableBlock or RoomCollisionType.GrappleBlock or
                    RoomCollisionType.BombableBlock => int.MinValue,
                _ => throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}."),
            };

            if (delta == int.MinValue)
                return true;
            if (delta == 0)
                return false;

            int targetIndex = block.Index + delta;
            if ((uint)targetIndex >= (uint)blockCount)
            {
                throw new InvalidDataException(
                    $"Collision extension block {block.Index} type ${(int)block.CollisionType:X1}/" +
                    $"BTS ${block.Bts.Value:X2} resolves outside room level data.");
            }
            block = level.GetCollisionBlockByIndex(targetIndex);
        }

        throw new InvalidDataException(
            $"Collision extension chain beginning at block {block.Index} contains a cycle.");
    }

    /// <summary>
    /// Native PostGrappleCollisionDetection ($90:EF22), using the vertical dispatcher
    /// at $94:84CD. Call before replacing the connected pose and its collision radius.
    /// </summary>
    public static void EjectAfterGrapple(RoomLevelData level, SamusKinematicsState state)
    {
        // Native horizontal scans only populate scratch distances: this routine never
        // applies them. These probes do not trigger PLMs, doors, damage, or enemies.
        int up = GrappleVerticalOverlap(level, state, direction: VerticalDirection.Down);
        int down = GrappleVerticalOverlap(level, state, direction: VerticalDirection.Up);
        if (up == 0 || down != 0) return;
        state.Position -= new Offset16<RoomPixels>(0, unchecked((short)up));
        // Tall bodies may overlap two rows. Retail repeats once, not until clear.
        if (state.Radius.Y >= 16)
            state.Position = state.Position.WithWholeY(unchecked((ushort)(state.Position.Y.Whole - GrappleVerticalOverlap(level, state, direction: VerticalDirection.Down))));
    }

    /// <summary>
    /// Measures block overlap in the selected vertical direction after grapple releases
    /// Samus, allowing the caller to eject her before the new pose radius is installed.
    /// </summary>
    private static int GrappleVerticalOverlap(RoomLevelData level, SamusKinematicsState state, VerticalDirection direction)
    {
        int boundary = unchecked((ushort)(state.Position.Y.Whole + (direction == VerticalDirection.Down ? state.Radius.Y - 1 : -state.Radius.Y)));
        int left = unchecked((ushort)(state.Position.X.Whole - state.Radius.X));
        int right = unchecked((ushort)(state.Position.X.Whole + state.Radius.X - 1));
        int firstColumn = left >> 4;
        int lastColumn = right >> 4;
        int maximum = 0;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockOrPrefilledSolid(new(unchecked((ushort)column), unchecked((ushort)(boundary >> 4))));
            int depth;
            // The post-grapple table treats even vertical extensions as solid, without
            // following BTS. Ordinary movement's dispatcher is not interchangeable.
            if (block.CollisionType >= RoomCollisionType.SolidBlock)
                depth = boundary & 15;
            else if (block.CollisionType != RoomCollisionType.Slope)
                continue;
            else if (block.Bts.IsNonSquareSlope)
            {
                if (column != state.Position.X.Whole >> 4) continue;
                if (block.Bts.SlopeFlipsVertically == (direction == VerticalDirection.Down))
                    depth = boundary & 15;
                else
                {
                    int difference = SamusSlopePhysics.ReadAlignmentHeight(block.Bts, state.Position.X.Whole) - (boundary & 15) - 1;
                    depth = direction == VerticalDirection.Down ? (difference <= 0 ? ~difference : -1) : (difference >= 0 ? difference : -1);
                }
            }
            else
            {
                int quadrant = 4 * block.Bts.SlopeShape + (block.Bts.SlopeOrientation ^ ((boundary & 8) >> 2));
                bool selected = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant) != 0;
                bool collides;
                if (column == lastColumn)
                    collides = (right & 8) == 0 ? selected : selected || SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
                else if (column != firstColumn || (left & 8) == 0)
                    collides = selected || SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
                else
                    collides = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
                depth = collides ? (direction == VerticalDirection.Down ? boundary & 7 : (boundary & 7) ^ 7) : -1;
            }
            maximum = Math.Max(maximum, depth + 1);
        }
        return maximum;
    }
}
