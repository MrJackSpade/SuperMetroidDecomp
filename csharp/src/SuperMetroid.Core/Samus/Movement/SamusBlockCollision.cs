using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's room-block movement scans.
/// </summary>
/// <remarks>
/// Every kind of block reacts in its own way: Samus passes air, spike, shootable and unrevealed air, sets off bombable
/// air and reacts to special air; slopes scale or stop her; solid, shootable, grapple and spike blocks stop her, a spike
/// hurting her; a door block leads through its door; and special and bomb blocks start their room objects, which
/// decide whether she passes. An extension block stands for the block it extends.
/// </remarks>
public static class SamusBlockCollision
{
    /// <summary>
    /// Runs the block-only observation of the wall-jump block collision check.
    /// The check reports the available distance but does
    /// not commit Samus's whole-pixel probe position. Collision still writes her fractions of a pixel.
    /// </summary>
    internal static BlockMoveResult ProbeWallHorizontal(
        RoomLevelData level,
        SamusKinematicsState state,
        SamusState owner,
        SamusPoseId pose,
        SignedSixteenSixteen signedDistance,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(state);
        SamusKinematicsState probe = state.CreateCollisionProbe();

        // The probe moves as her horizontal step does, with the same square-slope and unsupported-block reactions. A
        // slope's alignment after the scan moves only the probe, but what the probe touches still reacts: a scroll
        // trigger it reaches, through an extension block too, wakes its owner during the wall-jump check.
        BlockMoveResult result = MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, probe, owner, pose, new HorizontalBlockMove { Displacement = signedDistance, BlockReactionDirection = SamusCollisionDirection.NonDirectionalProbe });
        // Submerging sand writes these real motion values even through wall observation.
        state.VerticalVelocity = state.VerticalVelocity.WithSpeed(probe.VerticalVelocity.Speed);
        state.YAcceleration = probe.YAcceleration;

        // A block hit (and square-slope clipping) writes Samus's real X fraction of a pixel even through the wall-jump probe. The
        // probe's accepted displacement is whole pixels on a block hit, so its final fraction keeps that write, and her
        // whole X is unchanged; without it, movement away from the wall would gather fractional speed during the
        // wall-check pose.
        if (result.Collided && result.EnemyCollision is null)
            state.Position = state.Position.WithFractionX(probe.Position.X.FractionOfPixel);

        // The step preceding the block scan has a separate
        // real side effect: its touching path clears Samus's vertical fraction of a pixel. Copy that single
        // write back without committing the probe's X movement or slope alignment.
        if (result.EnemyCollision is { WasTouching: true })
            state.Position = state.Position.WithFractionY(PixelFraction.None);

        return result;
    }

    /// <summary>
    /// Runs horizontal block collision, the position addition of a move
    /// without solid-enemy collision, and the subsequent non-square slope alignment.
    /// </summary>
    /// <param name="terrain">The room terrain the move collides with and reacts to.</param>
    /// <param name="state">Live or probe kinematics whose position, to a fraction of a pixel, and collision-related motion values are updated.</param>
    /// <param name="owner">The Samus whose collision reactions (spikes, boost blocks, bomb blocks) act.</param>
    /// <param name="pose">
    /// The pose Samus moves in, which the elevator's pseudo-doors, the stations and the Chozo statue's hand admit by.
    /// </param>
    /// <param name="move">One horizontal block move: the displacement and which collision side effects it publishes (#633).</param>
    internal static BlockMoveResult MoveHorizontal(SamusTerrain terrain, SamusKinematicsState state, SamusState owner, SamusPoseId pose, HorizontalBlockMove move)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(state);

        if (move.CollisionMovementDirection is { } storedDirection &&
            storedDirection is not (SamusCollisionDirection.Left or SamusCollisionDirection.Right))
            throw new ArgumentOutOfRangeException(nameof(move.CollisionMovementDirection));

        SignedSixteenSixteen acceptedDisplacement = move.Displacement;
        bool collided = false;
        SolidEnemyCollisionResult? enemyCollision = null;
        RoomCollisionBlock? brokenBombBlock = null;

        if ((!acceptedDisplacement.IsZero || move.CollisionMovementDirection.HasValue) && state.InteractiveEnemies.Count != 0)
        {
            // Enemy collision measures the move's distance without its direction, which the collision direction carries.
            // Both the whole pixels and the fraction are kept: enemy clipping clears the fraction only when it actually
            // collides.
            UnsignedSixteenSixteen magnitude = move.Displacement.Magnitude();
            // The sign of the requested distance selects the direction: zero is not negative and moves right.
            // The probe itself reads the side the displacement was calculated toward, so a zero move
            // still detects tangency (and clears fractional Y) in the facing direction.
            bool entersLeft = move.Displacement.IsNegative;
            SamusCollisionDirection direction = move.CollisionMovementDirection ??
                (entersLeft ? SamusCollisionDirection.Left : SamusCollisionDirection.Right);
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                magnitude);
            state.RecordSolidEnemyCollision(direction, probe.Enemy);

            if (probe.Collided)
            {
                // On enemy collision the movement skips block detection and moves Samus by the clipped whole distance
                // alone. The accepted gap is never negative: the probe ignores an enemy Samus is already embedded in.
                SignedSixteenSixteen clippedMagnitude = SignedSixteenSixteen.FromWholePixels(probe.Distance);
                acceptedDisplacement = move.Displacement.Along(clippedMagnitude);
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
                ? TerrainScanEdges.Edge(state.Radius.X + targetCenter - 1)
                : TerrainScanEdges.Edge(targetCenter - state.Radius.X);

            int topBlockY = RoomGridConversions.PixelToBlock(TerrainScanEdges.Edge(state.Position.Y.Whole - state.Radius.Y));
            int verticalSpan = GetVerticalBlockSpan(state);
            int blockX = RoomGridConversions.PixelToBlock(leadingBoundary);

            for (int rowOffset = 0; rowOffset <= verticalSpan; rowOffset++)
            {
                RoomCollisionBlock block = terrain.Level.GetCollisionBlockOrEdgeSolid(
                    new(TerrainScanEdges.Block(blockX), TerrainScanEdges.Block(topBlockY + rowOffset)));
                if (!terrain.Level.TryReachBlockStoodFor(ref block))
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
                            }
                            break;
                        }

                        acceptedDisplacement = SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(
                            block.Behavior,
                            acceptedDisplacement,
                            state.VerticalVelocity.Speed);
                        break;

                    case RoomCollisionType.SolidBlock:
                    case RoomCollisionType.ShootableBlock:
                    case RoomCollisionType.GrappleBlock:
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        break;

                    case RoomCollisionType.DoorBlock:
                        // A door block leads somewhere. Samus enters a door it leads through, which reports no
                        // collision, so this scan allows her into the doorway. The elevator's door is not a door
                        // and clips her as an ordinary solid block does.
                        if (MeetDoorBlock(terrain, block, pose, DoorBlockMeeting.Contacts))
                        {
                            acceptedDisplacement = ClipHorizontalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                        }
                        break;

                    case RoomCollisionType.SpikeAir:
                    case RoomCollisionType.ShootableAir:
                    case RoomCollisionType.UnrevealedAir:
                        // Spike, shootable and unrevealed air let Samus pass with no reaction.
                        break;
                    case RoomCollisionType.BombableAir:
                        ActivateCollisionBombableAir(terrain, block, owner, move.CanBreakBombBlocks);
                        break;

                    case RoomCollisionType.SpecialAir:
                        // Sand's submerging reaction clears vertical speed/gravity even
                        // when reached through the horizontal step.
                        collided = SamusInsideBlockReactions.ReactCollision(new(state, state.CollisionBodyAttack(owner)), terrain, block, Axis.Horizontal,
                            ref acceptedDisplacement, out _, move.BlockReactionDirection) == SpecialAirCollisionOutcome.Blocks;
                        if (block.Behavior.SpecialAir == SpecialAirReaction.ScrollTrigger &&
                            !terrain.RoomObjects.Scrolls.TryNotifyScrollTouch(block.At))
                        {
                            throw new InvalidOperationException(
                                $"Scroll trigger block {block.Position} has no active scroll room-object owner; " +
                                $"live=[{string.Join(',', terrain.RoomObjects.Scrolls.TriggerBlocks)}].");
                        }
                        break;

                    case RoomCollisionType.SpikeBlock:
                        SamusTerrainHazardCollision.ApplySolidSpikeCollision(
                            terrain.Level,
                            owner,
                            block);
                        acceptedDisplacement = ClipHorizontalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        break;

                    case RoomCollisionType.SpecialBlock when
                        block.Behavior.Special == SpecialBlockReaction.CollectibleTrigger:
                        // Visible item frames are special blocks that collect an item. Block collision spawns the
                        // shared item detector, whose setup triggers the item at this
                        // origin and reports no collision; Samus therefore passes through
                        // rather than clipping against the item's visual block.
                        if (!terrain.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(block.At))
                        {
                            throw new InvalidOperationException(
                                $"Collectible block {block.Position} has no active item room-object owner.");
                        }
                        break;

                    case RoomCollisionType.SpecialBlock:
                        // Both special-block reactions spawn the room object and let Samus pass unless its setup stops her.
                        // A full pool returns without running setup, so even a block whose
                        // setup would normally reject contact falls through as air.
                        if (terrain.RoomObjects.EveryPlaceHeld)
                            break;
                        // A Speed Booster block breaks at once under a full-stage boost or a directional
                        // shinespark, so this same horizontal scan continues through the block it leaves.
                        if (terrain.RoomObjects.BlockReactions.TrySpawnSamusSpeedBoosterBlock(
                                terrain.Level,
                                block.At,
                                block.Behavior,
                                owner.RoomView))
                        {
                            break;
                        }
                        // Station setups reject the both-ways direction before finding their
                        // parent. An observational scan still clips against the access
                        // block, but must not activate the station or demand an owner.
                        if (move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
                            block.Behavior.IsStationTrigger &&
                            !terrain.RoomObjects.Stations.TryNotifyStationCollision(
                                 terrain.Level,
                                 block.At,
                                 block.Behavior,
                                 pose.ForRoomCollision(),
                                 Axis.Horizontal,
                                 movingPositive: acceptedDisplacement.IsPositive))
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
                        break;

                    case RoomCollisionType.BombableBlock:
                        // A bomb block spawns its room object for the block's reaction.
                        // Its setup reports the block solid unless Samus
                        // is speed boosting, screw attacking, or in a shinespark pose. On an
                        // accepted break it installs the air-type bomb-parent visual and
                        // reports no collision, so this scan continues through the new air.
                        if (terrain.RoomObjects.EveryPlaceHeld)
                            break;
                        if (block.Behavior.Bombable == BombableReaction.Inert || !CanBreakCollisionBombBlock(owner, move.CanBreakBombBlocks))
                        {
                            acceptedDisplacement = ClipHorizontalToSolid(
                                state,
                                acceptedDisplacement,
                                leadingBoundary);
                            collided = true;
                            break;
                        }
                        if (!BombableReactions.IsBreak(block.Behavior.Bombable))
                        {
                            throw new InvalidDataException(
                                $"Collision bomb block {block.Position} has invalid behaviour ({block.Behavior}).");
                        }
                        // Every contact that breaks a bomb block starts its block reaction, which runs the breakup
                        // and grows the block back (#1582).
                        bool spawned = terrain.RoomObjects.BlockReactions
                            .TrySpawnCollisionBombBlock(terrain.Level, block.At, block.Behavior);
                        if (spawned)
                            brokenBombBlock ??= block;
                        break;

                    case RoomCollisionType.HorizontalExtension:
                    case RoomCollisionType.VerticalExtension:
                    default:
                        throw new InvalidDataException(
                            $"Block {block.Position} type {block.CollisionType} escaped the " +
                            "complete horizontal collision check.");
                }

                if (collided)
                    break;
            }
        }

        state.Position = state.Position.AdvanceX(acceptedDisplacement);

        // Only the live movement publishes inheritance. Copies used
        // for prospective poses and wall probes must not replace the preceding move.
        if (ReferenceEquals(owner.Kinematics, state) &&
            move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
            move.AlignToSlopeAfterMovement)
            owner.ProjectileInheritance.Record(
                move.Displacement.IsNegative ? HorizontalDirection.Left : HorizontalDirection.Right,
                acceptedDisplacement);

        // Align Y to whole pixels after both collision and non-collision paths.
        // Direct uses (notably the prospective-running-pose probe)
        // omit that movement step and must retain the unaligned whole-pixel Y. The game never turns the alignment off
        // for a move that asks for it (#1682): only a movement type no pose has would have cleared its enable.
        SlopeAlignmentResult alignment = SamusSlopePhysics.AlignYPosition(
            terrain.Level,
            state.Position.Whole,
            state.Radius.Y,
            horizontalSlopeCollisionEnabled: move.AlignToSlopeAfterMovement);
        state.Position = state.Position.WithWholeY(alignment.Position.Y);
        // Slope alignment only sets this latch; it does not clear a square-floor contact
        // published by the previous vertical pass. The vertical move consumes it before
        // the next terrain Y move of some distance clears it.
        if (alignment.Adjusted)
            state.RestOnSlope();

        return new BlockMoveResult(
            acceptedDisplacement,
            collided,
            enemyCollision);
    }

    /// <summary>
    /// Moves Samus vertically against the blocks, scanning them left to right or right to left, then adds the final
    /// step to her position, to a fraction of a pixel, when nothing solid stops her.
    /// </summary>
    /// <param name="terrain">The room terrain the move collides with and reacts to.</param>
    /// <param name="state">Live or probe kinematics whose position, to a fraction of a pixel, and collision-related motion values are updated.</param>
    /// <param name="owner">The Samus whose collision reactions act.</param>
    /// <param name="pose">The pose Samus moves in, which pose-gated block reactions admit by.</param>
    /// <param name="move">One vertical block move: the displacement and which collision side effects it publishes.</param>
    internal static BlockMoveResult MoveVertical(SamusTerrain terrain, SamusKinematicsState state, SamusState owner, SamusPoseId pose, VerticalBlockMove move)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(state);

        SignedSixteenSixteen acceptedDisplacement = move.Displacement;
        bool collided = false;
        bool sandContact = false;
        SolidEnemyCollisionResult? enemyCollision = null;

        // Most of Samus's moves probe the solid enemies before reacting to terrain. A few moves skip
        // solid-enemy collision instead, which the move states; the move's list of enemies stays as it is, since
        // other gameplay reads it.
        // A zero-speed bounce apex still enters the downward solid-enemy probe.
        // Exact platform tangency can therefore finish the bounce this frame;
        // only the later terrain check skips a zero displacement.
        if (move.IncludeSolidEnemies && state.InteractiveEnemies.Count != 0)
        {
            // As for a horizontal move, enemy collision measures the distance without its direction.
            UnsignedSixteenSixteen magnitude = move.Displacement.Magnitude();
            SamusCollisionDirection direction = move.Displacement.IsNegative
                ? SamusCollisionDirection.Up
                : SamusCollisionDirection.Down;
            SolidEnemyCollisionResult probe = SamusSolidEnemyCollision.Probe(
                state,
                state.InteractiveEnemies,
                direction,
                magnitude);
            state.RecordSolidEnemyCollision(direction, probe.Enemy);

            if (probe.Collided)
            {
                // Solid-enemy success bypasses the block check and moves only to the
                // current enemy boundary. This is also how the game's vertical-collision flags
                // distinguish an enemy stop from an unobstructed terrain move. The accepted gap is never negative: the
                // probe ignores an enemy Samus is already embedded in.
                SignedSixteenSixteen clippedMagnitude = SignedSixteenSixteen.FromWholePixels(probe.Distance);
                acceptedDisplacement = acceptedDisplacement.Along(clippedMagnitude);
                collided = true;
                enemyCollision = probe;
            }
        }

        if (!collided && !acceptedDisplacement.IsZero)
        {
            // Only a terrain move of some distance clears this latch. A solid-enemy
            // stop skips the terrain, and a zero movement does not clear it either.
            state.LeaveSlope();
            VerticalTerrainScan scan = ScanVerticalTerrain(terrain, state, owner, pose, new VerticalTerrainProbe { Displacement = acceptedDisplacement, ScanLeftToRight = move.ScanLeftToRight, CanBreakBombBlocks = move.CanBreakBombBlocks, DoorBlocks = move.DoorBlocks, BlockReactionDirection = move.BlockReactionDirection });
            acceptedDisplacement = scan.AcceptedDisplacement;
            collided = scan.Collided;
            sandContact = scan.SandContact;
        }

        state.Position = state.Position.AdvanceY(acceptedDisplacement);
        if (ReferenceEquals(owner.Kinematics, state) &&
            move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe)
            owner.ProjectileInheritance.Record(
                move.Displacement.IsNegative ? VerticalDirection.Up : VerticalDirection.Down,
                acceptedDisplacement);
        // Sand contact counts as a collision whether Samus moved up or down. A changed pose's probe
        // (ProbeChangedPoseVertical) instead consumes only the block reaction's collision result.
        if (sandContact)
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
    /// (right to left): every block under the leading boundary, each reacting by its collision type.
    /// A zero displacement scans downward from the current bottom boundary.
    /// </summary>
    private static VerticalTerrainScan ScanVerticalTerrain(SamusTerrain terrain, SamusKinematicsState state, SamusState owner, SamusPoseId pose, VerticalTerrainProbe move)
    {
        SignedSixteenSixteen acceptedDisplacement = move.Displacement;
        bool collided = false;
        bool sandContact = false;
        int targetCenter = state.Position.Y.MovedBy(acceptedDisplacement).Whole;
        int leadingBoundary = !acceptedDisplacement.IsNegative
            ? TerrainScanEdges.Edge(state.Radius.Y + targetCenter - 1)
            : TerrainScanEdges.Edge(targetCenter - state.Radius.Y);
        int blockY = RoomGridConversions.PixelToBlock(leadingBoundary);
        int horizontalSpan = GetHorizontalBlockSpan(state);
        int firstBlockX = move.ScanLeftToRight
            ? RoomGridConversions.PixelToBlock(TerrainScanEdges.Edge(state.Position.X.Whole - state.Radius.X))
            : RoomGridConversions.PixelToBlock(TerrainScanEdges.Edge(state.Position.X.Whole + state.Radius.X - 1));

        for (int offset = 0; offset <= horizontalSpan; offset++)
        {
            int blockX = move.ScanLeftToRight ? firstBlockX + offset : firstBlockX - offset;
            RoomCollisionBlock block = terrain.Level.GetCollisionBlockOrEdgeSolid(new(TerrainScanEdges.Block(blockX), TerrainScanEdges.Block(blockY)));
            if (!terrain.Level.TryReachBlockStoodFor(ref block))
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
                            // The blocks-left count starts at the span and counts down while the
                            // scan runs left-to-right; the mirrored scan starts it at zero and counts
                            // up while scanning right-to-left. Either way the count names the same
                            // physical block, so the left and right edge tests beside a square-slope
                            // wall do not alternate with the scan order.
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

                    // A non-square slope's floor or ceiling test follows the collision movement direction, not the distance's sign. The
                    // changed-pose probe's both-ways direction therefore tests both ways as a floor.
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
                    // A door block reacts to a vertical move as it does to a horizontal one.
                    if (MeetDoorBlock(terrain, block, pose, move.DoorBlocks))
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
                        collided = SamusInsideBlockReactions.ReactCollision(new(state, state.CollisionBodyAttack(owner)), terrain, block, Axis.Vertical,
                            ref acceptedDisplacement, out bool touchedSand, move.BlockReactionDirection) == SpecialAirCollisionOutcome.Blocks;
                        sandContact |= touchedSand;
                    }
                    if (block.CollisionType == RoomCollisionType.SpecialAir &&
                        block.Behavior.SpecialAir == SpecialAirReaction.ScrollTrigger &&
                        !terrain.RoomObjects.Scrolls.TryNotifyScrollTouch(block.At))
                    {
                        throw new InvalidOperationException(
                            $"Scroll trigger block {block.Position} has no active scroll room-object owner; " +
                            $"live=[{string.Join(',', terrain.RoomObjects.Scrolls.TriggerBlocks)}].");
                    }
                    // Scroll wake-up itself lets Samus pass; sand has a separate
                    // collision and contact contract handled above.
                    break;
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnrevealedAir:
                    // These air blocks let Samus pass without room-object setup.
                    break;
                case RoomCollisionType.BombableAir:
                    ActivateCollisionBombableAir(terrain, block, owner, move.CanBreakBombBlocks);
                    break;

                case RoomCollisionType.SpikeBlock:
                    SamusTerrainHazardCollision.ApplySolidSpikeCollision(
                        terrain.Level,
                        owner,
                        block);
                    acceptedDisplacement = ClipVerticalToSolid(
                        state,
                        acceptedDisplacement,
                        leadingBoundary);
                    collided = true;
                    break;

                case RoomCollisionType.SpecialBlock when
                    block.Behavior.Special == SpecialBlockReaction.CollectibleTrigger:
                    if (!terrain.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(block.At))
                    {
                        throw new InvalidOperationException(
                            $"Collectible block {block.Position} has no active item room-object owner.");
                    }
                    break;

                case RoomCollisionType.SpecialBlock:
                    // When the room has no place left for the block's room object, no setup runs, so the
                    // unchanged special block is treated as air for this collision probe.
                    if (terrain.RoomObjects.EveryPlaceHeld)
                        break;
                    // Hand reactions change state but return unconditional collision:
                    // even an admitted morph contact is clipped by this same scan.
                    terrain.RoomObjects.ChozoStatueHand.NotifyChozoStatueHandCollision(terrain.Level,
                        block.At, owner,
                        pose, movingDown: acceptedDisplacement.IsPositive &&
                                (move.BlockReactionDirection ?? SamusCollisionDirection.Down) == SamusCollisionDirection.Down);
                    // The vertical special-solid check shares the speed-block setup
                    // with horizontal collision. Accepted boost contact becomes air
                    // before clipping; rejected/non-speed special blocks remain solid.
                    if (terrain.RoomObjects.BlockReactions.TrySpawnSamusSpeedBoosterBlock(
                            terrain.Level,
                            block.At,
                            block.Behavior,
                            owner.RoomView))
                    {
                        break;
                    }
                    if (block.Behavior.TryGetContactCrumble(out ContactCrumbleBlock crumble) &&
                        acceptedDisplacement.IsPositive &&
                        // The contact crumble checks the collision direction, not the sign of
                        // the tested clearance. Pose probes use the both-ways direction and delete the room object.
                        (move.BlockReactionDirection ?? SamusCollisionDirection.Down) == SamusCollisionDirection.Down)
                    {
                        // The contact crumble always reports the block solid. Setup removes
                        // the special collision kind immediately (leaving the block
                        // solid), and this downward scan remains clipped.
                        if (!terrain.RoomObjects.BlockReactions.TrySpawnSamusContactCrumbleBlock(
                                terrain.Level,
                                block.At,
                                crumble))
                            break;
                    }
                    // Pose expansion carries the both-ways direction, not a downward
                    // landing. Keep solidity without waking the save-station owner.
                    if (move.BlockReactionDirection != SamusCollisionDirection.NonDirectionalProbe &&
                        block.Behavior.IsStationTrigger &&
                        !terrain.RoomObjects.Stations.TryNotifyStationCollision(
                             terrain.Level,
                             block.At,
                             block.Behavior,
                             pose.ForRoomCollision(),
                             Axis.Vertical,
                             movingPositive: acceptedDisplacement.IsPositive))
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
                    // The vertical reaction shares the exact room-object
                    // setup and collision contract documented in the horizontal branch above.
                    if (terrain.RoomObjects.EveryPlaceHeld)
                    {
                        // The full room-object pool prevents rejection setup from stopping
                        // Samus. Block collision therefore lets her pass and
                        // treats an untouched bomb block as air.
                        break;
                    }
                    if (block.Behavior.Bombable == BombableReaction.Inert || !CanBreakCollisionBombBlock(owner, move.CanBreakBombBlocks))
                    {
                        acceptedDisplacement = ClipVerticalToSolid(
                            state,
                            acceptedDisplacement,
                            leadingBoundary);
                        collided = true;
                        break;
                    }
                    if (!BombableReactions.IsBreak(block.Behavior.Bombable))
                    {
                        throw new InvalidDataException(
                            $"Collision bomb block {block.Position} has invalid behaviour ({block.Behavior}).");
                    }
                    terrain.RoomObjects.BlockReactions
                        .TrySpawnCollisionBombBlock(terrain.Level, block.At, block.Behavior);
                    break;

                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                default:
                    throw new InvalidDataException(
                        $"Block {block.Position} type {block.CollisionType} escaped the " +
                        "complete vertical collision check.");
            }

            if (collided)
                break;
        }
        return new VerticalTerrainScan(acceptedDisplacement, collided, sandContact);
    }

    /// <summary>
    /// The single-block collision check after a change of pose:
    /// the terrain scan alone, in the downward direction, then the probe's move by the accepted distance.
    /// Unlike an ordinary vertical move it tests the blocks even for no distance, so a zero probe still tests the
    /// blocks under the current bottom boundary. It checks no solid enemies.
    /// </summary>
    /// <param name="level">The room's collision blocks.</param>
    /// <param name="state">A collision probe copy; its position receives the accepted distance.</param>
    /// <param name="owner">The Samus whose collision reactions act.</param>
    /// <param name="pose">
    /// The pose being installed, which the elevator ride's standing-pose gate tests: the game writes it to Samus's
    /// pose before the larger-radius check.
    /// </param>
    /// <param name="displacement">The distance to test, negative upward; zero tests the current bottom boundary.</param>
    /// <param name="scanLeftToRight">Scan order chosen by the frame counter's parity.</param>
    /// <param name="roomObjects">The room's room-object owner for special and bombable blocks.</param>
    internal static BlockMoveResult ProbeChangedPoseVertical(
        RoomLevelData level,
        SamusKinematicsState state,
        SamusState owner,
        SamusPoseId pose,
        SignedSixteenSixteen displacement,
        bool scanLeftToRight,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(state);
        VerticalTerrainScan scan = ScanVerticalTerrain(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, owner, pose, new VerticalTerrainProbe { Displacement = displacement, ScanLeftToRight = scanLeftToRight, CanBreakBombBlocks = false, DoorBlocks = DoorBlockMeeting.Contacts, BlockReactionDirection = SamusCollisionDirection.NonDirectionalProbe });
        state.Position = state.Position.AdvanceY(scan.AcceptedDisplacement);
        return new BlockMoveResult(scan.AcceptedDisplacement, scan.Collided);
    }

    /// <summary>
    /// Samus's collision meeting a door block (#2545): the contact she makes with it, which the room's door contacts act
    /// on unless an elevator carries her past it; returns whether the block stops her as a solid block does.
    /// </summary>
    /// <param name="terrain">The room's level, where the block leads, and its door contacts.</param>
    /// <param name="block">The door block she met.</param>
    /// <param name="pose">Her pose, which decides whether the elevator's door arms the elevator.</param>
    /// <param name="meeting">Whether the move contacts door blocks or is carried past them.</param>
    private static bool MeetDoorBlock(SamusTerrain terrain, RoomCollisionBlock block, SamusPoseId pose, DoorBlockMeeting meeting)
    {
        DoorContact contact = DoorContact.Of(terrain.Level.DoorExitAt(block), pose.ForRoomCollision());
        switch (meeting)
        {
            case DoorBlockMeeting.Contacts:
                terrain.RoomObjects.DoorContacts.Meet(contact);
                break;
            case DoorBlockMeeting.CarriedPastByElevator:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DoorBlockMeeting)} {meeting}.");
        }
        return contact.Blocks;
    }

    /// <summary>
    /// Runs the shared bomb-block setup for non-solid cells without consuming its collision result.
    /// </summary>
    private static void ActivateCollisionBombableAir(
        SamusTerrain terrain,
        RoomCollisionBlock block,
        SamusState owner,
        bool explicitAdmission)
    {
        // The game's horizontal and vertical air reactions invoke the same setup as solid
        // bomb blocks, but ignores its collision result. A refused break, or one with no room left for its
        // effect, must never turn this non-solid cell into a movement obstruction.
        if (block.Behavior.Bombable == BombableReaction.Inert || !CanBreakCollisionBombBlock(owner, explicitAdmission))
            return;
        terrain.RoomObjects.BlockReactions.TrySpawnCollisionBombBlock(terrain.Level, block.At, block.Behavior);
    }

    /// <summary>
    /// The bomb-block setup reads the live boost stage and pose for every scan that reaches it,
    /// independently of the movement owner; this includes the observational wall-jump
    /// probe. A move that admits a break explicitly breaks the block whatever Samus's pose.
    /// </summary>
    private static bool CanBreakCollisionBombBlock(SamusState owner, bool explicitAdmission) =>
        explicitAdmission || SamusPoseMetadata.BreaksCollisionBombBlocks(owner);

    /// <summary>
    /// Resolves a non-square slope against Samus's leading vertical boundary and retains
    /// the clipped distance when the slope intercepts the requested travel.
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
        if (RoomGridConversions.PixelToBlock(state.Position.X.Whole) != blockX)
            return new SlopeClip(displacement, false);

        if (movingDown)
        {
            if (block.Behavior.Slope.FlipsVertically)
                return new SlopeClip(displacement, false);

            int height = SamusSlopePhysics.ReadAlignmentHeight(block.Behavior, state.Position.X.Whole);
            int bottomDepth = RoomGridConversions.OffsetWithinBlock(state.Radius.Y + targetCenter - 1);
            int correction = height - bottomDepth - 1;
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
            int correction = height - topDepth - 1;
            if (correction > 0)
                return new SlopeClip(displacement, false);

            return new SlopeClip(ClippedToSlope(displacement, correction), true);
        }

        // The clipped travel is the displacement's whole pixels moved back by the slope's correction, never below
        // zero, the fraction dropped. The correction is zero or negative, so the slope never lengthens the travel.
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
        SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeQuadrant.ColumnFor(RoomGridConversions.InSecondHalfOfBlock(leadingBoundary)), SquareSlopeRowHalf.Top);
        bool selectedSolid = SquareSlopes.IsSolid(shape, quadrant);
        bool collide;

        if (remainingRows == 0)
        {
            // On the bottom scanned block, the body's bottom half can accept the selected
            // quadrant directly. Otherwise the test goes on to the opposite
            // vertical quadrant test below.
            if (!RoomGridConversions.InSecondHalfOfBlock(state.Radius.Y + state.Position.Y.Whole - 1) && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopes.IsSolid(shape, quadrant.OtherRow);
        }
        else
        {
            // Every row after the first tests the selected quadrant. On the first row, Samus's
            // top lying in a block's lower half is the exception; a selected quadrant that is not tested or not solid
            // leaves the opposite vertical quadrant to decide.
            bool selectedTested = remainingRows != totalRows ||
                !RoomGridConversions.InSecondHalfOfBlock(state.Position.Y.Whole - state.Radius.Y);
            collide = (selectedTested && selectedSolid) || SquareSlopes.IsSolid(shape, quadrant.OtherRow);
        }

        if (!collide)
        {
            clippedDisplacement = displacement;
            return false;
        }

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
        SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeColumnHalf.Left, SquareSlopeQuadrant.RowFor(RoomGridConversions.InSecondHalfOfBlock(leadingBoundary)));
        bool selectedSolid = SquareSlopes.IsSolid(shape, quadrant);
        bool collide;

        // A blocks-left count of zero always denotes the physical rightmost block, independent of
        // which of the two alternating scans reached it. A count equal to the span denotes the physical leftmost
        // block. Interior blocks necessarily test both eight-pixel halves.
        if (blocksLeftToCheck == 0)
        {
            if (!RoomGridConversions.InSecondHalfOfBlock(state.Radius.X + state.Position.X.Whole - 1) && !selectedSolid)
            {
                clippedDisplacement = displacement;
                return false;
            }

            collide = selectedSolid || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
        }
        else
        {
            // The physical leftmost block skips the selected quadrant when Samus's left edge lies in its right half; a
            // selected quadrant that is not tested or not solid leaves the opposite horizontal quadrant to decide.
            bool selectedTested = blocksLeftToCheck != totalColumns ||
                !RoomGridConversions.InSecondHalfOfBlock(state.Position.X.Whole - state.Radius.X);
            collide = (selectedTested && selectedSolid) || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
        }

        if (!collide)
        {
            clippedDisplacement = displacement;
            return false;
        }

        clippedDisplacement = ClipVerticalToSquareSlope(
            state,
            displacement,
            leadingBoundary);
        // A downward square-slope hit publishes support. Without this
        // latch, fast running probes farther than the eight-pixel floor thickness
        // on the next frame and can embed Samus below its surface.
        if (!displacement.IsNegative) state.RestOnSlope();
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
            int whole = TerrainScanEdges.Approach(state.Radius.X + RoomGridConversions.BlockEnd(leadingBoundary) + 1, state.Position.X.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(PixelFraction.None);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
        else
        {
            int whole = TerrainScanEdges.Approach(RoomGridConversions.BlockStart(leadingBoundary) - state.Radius.X, state.Position.X.Whole);
            if (whole < 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(PixelFraction.Last);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
    }

    /// <summary>Clips horizontal travel at the occupied half of a square slope block.</summary>
    private static SignedSixteenSixteen ClipHorizontalToSquareSlope(
        SamusKinematicsState state,
        SignedSixteenSixteen displacement,
        int leadingBoundary)
    {
        if (displacement.IsNegative)
        {
            int whole = TerrainScanEdges.Approach(
                state.Radius.X + RoomGridConversions.HalfBlockEnd(leadingBoundary) + 1, state.Position.X.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionX(PixelFraction.None);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }

        int rightWhole = TerrainScanEdges.Approach(
            RoomGridConversions.HalfBlockStart(leadingBoundary) - state.Radius.X, state.Position.X.Whole);
        if (rightWhole < 0)
            rightWhole = 0;
        state.Position = state.Position.WithFractionX(PixelFraction.Last);
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
            int whole = TerrainScanEdges.Approach(state.Radius.Y + RoomGridConversions.BlockEnd(leadingBoundary) + 1, state.Position.Y.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(PixelFraction.None);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }
        else
        {
            int whole = TerrainScanEdges.Approach(RoomGridConversions.BlockStart(leadingBoundary) - state.Radius.Y, state.Position.Y.Whole);
            if (whole < 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(PixelFraction.Last);
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
            int whole = TerrainScanEdges.Approach(
                state.Radius.Y + RoomGridConversions.HalfBlockEnd(leadingBoundary) + 1, state.Position.Y.Whole);
            if (whole >= 0)
                whole = 0;
            state.Position = state.Position.WithFractionY(PixelFraction.None);
            return SignedSixteenSixteen.FromWholePixels(whole);
        }

        int downWhole = TerrainScanEdges.Approach(
            RoomGridConversions.HalfBlockStart(leadingBoundary) - state.Radius.Y, state.Position.Y.Whole);
        if (downWhole < 0)
            downWhole = 0;
        state.Position = state.Position.WithFractionY(PixelFraction.Last);

        // Only the downward square-slope clip writes this flag. The no-speed vertical move consumes it to use
        // a one-pixel grounding probe instead of total-horizontal-speed-plus-one.
        state.RestOnSlope();
        return SignedSixteenSixteen.FromWholePixels(downWhole);
    }

    /// <summary>Counts block rows touched by Samus's current vertical collision extent.</summary>
    private static int GetVerticalBlockSpan(SamusKinematicsState state)
    {
        int alignedTop = RoomGridConversions.BlockStart(TerrainScanEdges.Edge(state.Position.Y.Whole - state.Radius.Y));
        return RoomGridConversions.WholeBlocksIn(TerrainScanEdges.Edge(state.Radius.Y + state.Position.Y.Whole - 1 - alignedTop));
    }

    /// <summary>Counts block columns touched by Samus's current horizontal collision extent.</summary>
    private static int GetHorizontalBlockSpan(SamusKinematicsState state)
    {
        int alignedLeft = RoomGridConversions.BlockStart(TerrainScanEdges.Edge(state.Position.X.Whole - state.Radius.X));
        return RoomGridConversions.WholeBlocksIn(TerrainScanEdges.Edge(state.Radius.X + state.Position.X.Whole - 1 - alignedLeft));
    }

    /// <summary>
    /// The collision check after a grapple, using the vertical step.
    /// Call before replacing the connected pose and its collision radius.
    /// </summary>
    public static void EjectAfterGrapple(RoomLevelData level, SamusKinematicsState state)
    {
        // The overlap probes above and below Samus only measure distances; the check moves her at most by the
        // overlap below. These probes do not trigger room objects, doors, damage, or enemies.
        int below = GrappleVerticalOverlap(level, state, direction: VerticalDirection.Down);
        int above = GrappleVerticalOverlap(level, state, direction: VerticalDirection.Up);
        if (below == 0 || above != 0) return;
        state.Position -= new Offset16<RoomPixels>(0, below);
        // A body whose radius reaches a block's height may overlap two rows. The check repeats once, not until clear.
        if (state.Radius.Y >= RoomGridConversions.BlockPixels)
            state.Position = state.Position.WithWholeY(state.Position.Y.Whole - GrappleVerticalOverlap(level, state, direction: VerticalDirection.Down));
    }

    /// <summary>
    /// Measures block overlap in the selected vertical direction after grapple releases
    /// Samus, so the release can eject her before the new pose radius is installed.
    /// </summary>
    private static int GrappleVerticalOverlap(RoomLevelData level, SamusKinematicsState state, VerticalDirection direction)
    {
        int boundary = TerrainScanEdges.Edge(state.Position.Y.Whole + (direction == VerticalDirection.Down ? state.Radius.Y - 1 : -state.Radius.Y));
        int left = TerrainScanEdges.Edge(state.Position.X.Whole - state.Radius.X);
        int right = TerrainScanEdges.Edge(state.Position.X.Whole + state.Radius.X - 1);
        int firstColumn = RoomGridConversions.PixelToBlock(left);
        int lastColumn = RoomGridConversions.PixelToBlock(right);
        int maximum = 0;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockOrEdgeSolid(new(TerrainScanEdges.Block(column), RoomGridConversions.PixelToBlock(boundary)));
            // The overlap counts the pixels from the solid edge through Samus's boundary pixel, which it includes; a
            // column she does not overlap counts none.
            int overlap;
            // After a grapple even vertical extensions count as solid, without following their
            // behaviour, unlike ordinary movement.
            if (block.CollisionType.IsSolidKind())
                overlap = RoomGridConversions.OffsetWithinBlock(boundary) + 1;
            else if (block.CollisionType != RoomCollisionType.Slope)
                continue;
            else if (block.Behavior.Slope.IsNonSquare)
            {
                if (column != RoomGridConversions.PixelToBlock(state.Position.X.Whole)) continue;
                if (block.Behavior.Slope.FlipsVertically == (direction == VerticalDirection.Down))
                    overlap = RoomGridConversions.OffsetWithinBlock(boundary) + 1;
                else
                {
                    int difference = SamusSlopePhysics.ReadAlignmentHeight(block.Behavior, state.Position.X.Whole) - RoomGridConversions.OffsetWithinBlock(boundary) - 1;
                    overlap = direction == VerticalDirection.Down ? (difference <= 0 ? -difference : 0) : (difference >= 0 ? difference + 1 : 0);
                }
            }
            else
            {
                SquareSlopeShape shape = SquareSlopes.ShapeOf(block.Behavior);
                SquareSlopeQuadrant quadrant = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeColumnHalf.Left, SquareSlopeQuadrant.RowFor(RoomGridConversions.InSecondHalfOfBlock(boundary)));
                bool selected = SquareSlopes.IsSolid(shape, quadrant);
                bool collides;
                if (column == lastColumn)
                    collides = !RoomGridConversions.InSecondHalfOfBlock(right) ? selected : selected || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
                else if (column != firstColumn || !RoomGridConversions.InSecondHalfOfBlock(left))
                    collides = selected || SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
                else
                    collides = SquareSlopes.IsSolid(shape, quadrant.OtherColumn);
                overlap = collides
                    ? (direction == VerticalDirection.Down ? RoomGridConversions.OffsetWithinHalfBlock(boundary) : RoomGridConversions.OffsetFromHalfBlockEnd(boundary)) + 1
                    : 0;
            }
            maximum = Math.Max(maximum, overlap);
        }
        return maximum;
    }
}
