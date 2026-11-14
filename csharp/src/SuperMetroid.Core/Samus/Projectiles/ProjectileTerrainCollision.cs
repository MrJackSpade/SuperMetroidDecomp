using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile movement against room terrain: the initial beam collision, horizontal and vertical block scans, shot-block reactions (including wave beams through door bands) and killing a beam or missile on contact.</summary>
[RoomLifetime]
internal sealed class ProjectileTerrainCollision
{
    /// <summary>Clears projectile slots when the kill step rejects an already converted family.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Publishes the shared quake requested by a Super Missile impact.</summary>
    private readonly ProjectileEarthquakes _earthquakes;
    /// <summary>Queues distinct beam and missile impact sounds at the collision seam.</summary>
    private readonly ProjectileImpactAudio _impactAudio;
    /// <summary>The terrain a Wave shot's horizontal scan finds where it lands halfway through a block.</summary>
    private readonly StraddledTerrain<RoomTerrainBlock> _straddledTerrain;

    /// <summary>
    /// Binds the projectile animation, quake, and impact-audio owners used when terrain consumes a shot, and the
    /// installation's straddled terrain the horizontal Wave scan reads.
    /// </summary>
    internal ProjectileTerrainCollision(ProjectileAnimation animation, ProjectileEarthquakes earthquakes, ProjectileImpactAudio impactAudio,
        StraddledTerrain<RoomTerrainBlock> straddledTerrain)
    {
        _animation = animation;
        _earthquakes = earthquakes;
        _impactAudio = impactAudio;
        _straddledTerrain = Ensure.NotNull(straddledTerrain);
    }

    /// <summary>Advances a shot by its signed 1/256-pixel X velocity and scans its leading edge for block reactions and collision.</summary>
    internal static bool MoveHorizontally(
        RoomLevelData level,
        SamusProjectile slot,
        ISamusRoomObjects? roomObjects)
    {
        slot.Position = slot.Position.AdvanceX(slot.Velocity.X.ToSixteenSixteenDelta());
        return ScanHorizontalShotReactions(level, slot, roomObjects);
    }

    /// <summary>
    /// Advances a Wave or Hyper shot by its signed 1/256-pixel X velocity and scans its leading edge for block reactions, which
    /// never report a collision.
    /// </summary>
    internal void MoveWaveShotHorizontally(
        RoomLevelData level,
        SamusProjectile slot,
        ISamusRoomObjects? roomObjects)
    {
        slot.Position = slot.Position.AdvanceX(slot.Velocity.X.ToSixteenSixteenDelta());
        ScanHorizontalWaveShotReactions(level, slot, roomObjects, _straddledTerrain);
    }

    /// <summary>
    /// Runs the producer-owned collision call made before a new beam receives its speed.
    /// </summary>
    internal bool RunInitialBeamCollision(
        RoomLevelData level,
        SamusProjectile slot,
        ISamusRoomObjects? roomObjects,
        bool waveBeam)
    {
        // The producer explicitly sets both 1/256-pixel speeds to zero. Firing left is
        // the sole exception: its firing setup writes -1 before the horizontal scan so the
        // amount-sign branch samples the left radius. Diagonal-left directions retain zero,
        // including the game's counterintuitive right-edge initial horizontal probe.
        slot.Velocity = new(slot.Direction == SamusProjectileDirection.Left ? LeftScanVelocity : default, default);

        SamusProjectileDirection direction = slot.Direction;
        bool collided = false;
        if (direction is
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.Right or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.Left or
            SamusProjectileDirection.UpLeft)
        {
            bool horizontalReaction = false;
            if (waveBeam)
                MoveWaveShotHorizontally(level, slot, roomObjects);
            else
                horizontalReaction = MoveHorizontally(level, slot, roomObjects);

            // The Wave-beam scan deliberately reports no collision after publishing a block
            // reaction. Preserve that contract here instead of allowing our shared scanner's
            // Boolean return to suppress the diagonal vertical probe. Ordinary beams retain
            // the reaction result because their producer aborts immediately on a collision.
            collided = !waveBeam && horizontalReaction;
        }

        // Diagonals call vertical collision only when the no-Wave horizontal call reported
        // no collision. Wave's horizontal scan never reports one even when it
        // publishes a block reaction, so it necessarily reaches this second axis as well.
        bool diagonal = direction is
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.UpLeft;
        if (!collided && (diagonal || direction is
            SamusProjectileDirection.UpFacingRight or
            SamusProjectileDirection.DownFacingRight or
            SamusProjectileDirection.DownFacingLeft or
            SamusProjectileDirection.UpFacingLeft))
        {
            bool verticalReaction = MoveVertically(level, slot, roomObjects, waveBeam);
            collided = !waveBeam && verticalReaction;
        }

        if (!waveBeam && collided)
        {
            KillBeamOnBlockCollision(slot);
            return true;
        }

        // Wave collision deliberately discards whether terrain stopped it, after retaining every block and
        // room-object side effect. Its caller will overwrite the temporary zero/-1 speeds immediately.
        return false;
    }

    /// <summary>Advances a shot by its signed 1/256-pixel Y velocity and scans its leading edge; Wave retains reactions but is never stopped.</summary>
    internal static bool MoveVertically(
        RoomLevelData level,
        SamusProjectile slot,
        ISamusRoomObjects? roomObjects,
        bool waveBeam = false)
    {
        slot.Position = slot.Position.AdvanceY(slot.Velocity.Y.ToSixteenSixteenDelta());

        if (waveBeam)
        {
            ScanVerticalWaveShotReactions(level, slot, roomObjects);
            return false;
        }
        return ScanVerticalShotReactions(level, slot, roomObjects);
    }

    /// <summary>Checks every block along the projectile's vertical span and reports collision only when the scan's solid counter completes.</summary>
    internal static bool ScanHorizontalShotReactions(
        RoomLevelData level,
        SamusProjectile slot,
        ISamusRoomObjects? roomObjects)
    {
        int topBlock = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y));
        int bottomBlock = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int targetX = slot.Velocity.X.IsNegative
            ? CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X)
            : CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1);
        int blockX = RoomGridConversions.PixelToBlock(targetX);

        if ((uint)blockX >= level.Size.X ||
            (uint)topBlock >= level.Size.Y ||
            (uint)bottomBlock >= level.Size.Y)
        {
            return false;
        }

        // The span scan decrements its target counter only for reactions that stop the shot. A tall
        // projectile grazing air beside one solid block therefore keeps moving; every
        // touched block still gets its side effect before that aggregate result is tested.
        bool everyBlockSolid = true;
        int span = bottomBlock - topBlock;
        for (int blockY = topBlock; blockY <= bottomBlock; blockY++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
            var context = new ShotSpan(span, bottomBlock - blockY, targetX);
            if (!RunShotReaction(level, slot, block, roomObjects, context, out bool endSpan))
                everyBlockSolid = false;
            if (endSpan) return true;
        }
        return everyBlockSolid;
    }

    /// <summary>Checks every block along the projectile's horizontal span, preserving each block reaction before aggregating solidity.</summary>
    internal static bool ScanVerticalShotReactions(
        RoomLevelData level,
        SamusProjectile slot,
        ISamusRoomObjects? roomObjects)
    {
        int leftBlock = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X));
        int rightBlock = RoomGridConversions.PixelToBlock(CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1));
        int targetY = slot.Velocity.Y.IsNegative
            ? CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y)
            : CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int blockY = RoomGridConversions.PixelToBlock(targetY);

        if ((uint)blockY >= level.Size.Y ||
            (uint)leftBlock >= level.Size.X ||
            (uint)rightBlock >= level.Size.X)
        {
            return false;
        }

        bool everyBlockSolid = true;
        int span = rightBlock - leftBlock;
        for (int blockX = leftBlock; blockX <= rightBlock; blockX++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));
            var context = new ShotSpan(span, rightBlock - blockX, targetY);
            if (!RunShotReaction(level, slot, block, roomObjects, context, out bool endSpan, axis: Axis.Vertical))
                everyBlockSolid = false;
            if (endSpan) return true;
        }
        return everyBlockSolid;
    }

    /// <summary>
    /// A block span's scan state: the blocks spanned minus one, the blocks still to check
    /// minus one (the first, top or left, block has both equal) and the target boundary.
    /// </summary>
    private readonly record struct ShotSpan(int Span, int Remaining, int TargetBoundary);

    /// <summary>
    /// Square-slope collision for a beam, horizontal or vertical: the quarter cells of a
    /// square slope that the beam's edges reach in this block. A hit stops the beam like a
    /// solid block; unlike a non-square slope it does not end the span.
    /// </summary>
    private static bool SquareSlopeShotReaction(SamusProjectile slot, RoomCollisionBlock block,
        ShotSpan span, bool horizontalMovement)
    {
        if (slot.Type.IsFamily(SamusProjectileFamily.Missile) ||
            slot.Type.IsFamily(SamusProjectileFamily.SuperMissile))
        {
            // Missiles take the point test in RunMissileFrameBehavior; the missile
            // branch has no caller here.
            throw new InvalidOperationException("Missile reached the beam square-slope shot reaction.");
        }

        // The block's reflections mirror the quarter each edge reaches.
        SquareSlopeShape shape = SquareSlopes.ShapeOf(block.Behavior);
        bool Solid(SquareSlopeQuadrant quadrant) => SquareSlopes.IsSolid(shape, quadrant);

        if (horizontalMovement)
        {
            // The leading edge picks the column half; the scan then tests top/bottom rows.
            SquareSlopeQuadrant cell = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeQuadrant.ColumnFor(RoomGridConversions.InSecondHalfOfBlock(span.TargetBoundary)), SquareSlopeRowHalf.Top);
            bool topEdgeInBottomHalf = RoomGridConversions.InSecondHalfOfBlock(slot.Position.Y.Whole - slot.Radius.Y);
            bool bottomEdgeInBottomHalf = RoomGridConversions.InSecondHalfOfBlock(slot.Position.Y.Whole + slot.Radius.Y - 1);
            if (span.Span == 0)
                return (!topEdgeInBottomHalf && Solid(cell)) || (bottomEdgeInBottomHalf && Solid(cell.OtherRow));
            if (span.Remaining == 0)
                return bottomEdgeInBottomHalf ? Solid(cell) || Solid(cell.OtherRow) : Solid(cell);
            if (span.Remaining == span.Span && topEdgeInBottomHalf)
                return Solid(cell.OtherRow);
            return Solid(cell) || Solid(cell.OtherRow);
        }

        // The leading edge picks the row half; the scan then tests left/right columns.
        SquareSlopeQuadrant rowCell = SquareSlopeQuadrant.Showing(block.Behavior, SquareSlopeColumnHalf.Left, SquareSlopeQuadrant.RowFor(RoomGridConversions.InSecondHalfOfBlock(span.TargetBoundary)));
        bool leftEdgeInRightHalf = RoomGridConversions.InSecondHalfOfBlock(slot.Position.X.Whole - slot.Radius.X);
        bool rightEdgeInRightHalf = RoomGridConversions.InSecondHalfOfBlock(slot.Position.X.Whole + slot.Radius.X - 1);
        if (span.Span == 0)
            return (!leftEdgeInRightHalf && Solid(rowCell)) || (rightEdgeInRightHalf && Solid(rowCell.OtherColumn));
        if (span.Remaining == 0)
            return rightEdgeInRightHalf ? Solid(rowCell) || Solid(rowCell.OtherColumn) : Solid(rowCell);
        if (span.Remaining == span.Span && leftEdgeInRightHalf)
            return Solid(rowCell.OtherColumn);
        return Solid(rowCell) || Solid(rowCell.OtherColumn);
    }

    /// <summary>Resolves an extension and applies its slope, room-object, shootable, or ordinary solid reaction to one scanned block.</summary>
    private static bool RunShotReaction(
        RoomLevelData level,
        SamusProjectile slot,
        RoomCollisionBlock block,
        ISamusRoomObjects? roomObjects,
        ShotSpan span,
        out bool endSpan,
        Axis axis = Axis.Horizontal)
    {
        endSpan = false;
        // The extension-block resolution is shared by shot, bomb, grapple, collision, and inside
        // dispatch. Door caps deliberately put the shootable origin at one end and vertical
        // extension blocks across the other three cells. Resolve those signed behaviour
        // links before selecting a reaction; otherwise a centered horizontal beam merely
        // explodes against the middle extension and can never reach the blue-door behaviours.
        RoomCollisionBlock? resolvedBlock = ResolveShotReactionExtension(level, block);
        if (resolvedBlock is null)
            return false;
        block = resolvedBlock.Value;
        if (block.CollisionType == RoomCollisionType.Slope && !block.Behavior.Slope.IsNonSquare)
            return SquareSlopeShotReaction(slot, block, span, axis == Axis.Horizontal);
        if (block.CollisionType == RoomCollisionType.Slope && block.Behavior.Slope.IsNonSquare)
        {
            // Non-square slopes dispatch through the projectile's
            // perpendicular center block, not every row/column touched by its radius.
            int centerBlock = RoomGridConversions.PixelToBlock(axis == Axis.Horizontal ? slot.Position.Y.Whole : slot.Position.X.Whole);
            int slopeBlock = axis == Axis.Horizontal ? block.At.Y : block.At.X;
            if (centerBlock != slopeBlock) return false;

            int x = RoomGridConversions.OffsetWithinBlock(slot.Position.X.Whole, mirrored: block.Behavior.Slope.FlipsHorizontally);
            int y = RoomGridConversions.OffsetWithinBlock(slot.Position.Y.Whole, mirrored: block.Behavior.Slope.FlipsVertically);
            bool hit = block.Behavior.Slope.Shape.HeightAt(x) <= y;
            // A surface hit clears both scan counters. This overrides
            // earlier air rows and ends the span; Wave still discards whether the hit stops it.
            endSpan = hit;
            return hit;
        }

        // Chozo orbs and concealed item blocks are shootable blocks of the item-trigger behaviour. Their special
        // reaction does not use the ordinary behaviour 0..F shot-block table: its reaction
        // finds and triggers the already-loaded permanent-item room object at this origin.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
        {
            _ = roomObjects?.CollectibleObjects.TryNotifyCollectibleProjectileHit(block.At);
            return true;
        }

        // Both the solid and the shootable resident-object trigger collision routes allocate the generic shot trigger,
        // which publishes the live projectile type to the resident room object at this block.
        // A solid block's route reads its behaviour as the shot block's reaction.
        if (block.CollisionType == RoomCollisionType.SolidBlock &&
            block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
        {
            NotifyResidentProjectileHit(roomObjects, block, slot.Type);
            return true;
        }

        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            if (block.Behavior.Shootable == ShootableReaction.CollisionProbe)
            {
                // Gate setup ends the caller's span and forces its collision counter.
                // A full room-object pool skips setup entirely, so it must not end the scan.
                endSpan = roomObjects?.BlockReactions.TrySpawnProjectileShotBlock(
                    level, block.At, block.Behavior, slot.Type,
                    block.CollisionType == RoomCollisionType.ShootableBlock) == true;
                return block.CollisionType == RoomCollisionType.ShootableBlock;
            }
            TrySpawnShootableReaction(level, slot, block, roomObjects);
            return block.CollisionType == RoomCollisionType.ShootableBlock;
        }

        // Solid, special, grapple, door and spike blocks stop the shot with no reaction. A bomb block's reaction
        // answers only bombs and Power Bombs, so it stops a shot with no reaction too, and bombable air lets it pass.
        return block.CollisionType.IsSolidKind();
    }

    /// <summary>
    /// Follows signed horizontal or vertical behaviour links to the actual reaction block, each leading on in the room's
    /// reading order (<see cref="RoomReadingOrder"/>), rejecting cycles and out-of-room targets.
    /// </summary>
    private static RoomCollisionBlock? ResolveShotReactionExtension(
        RoomLevelData level,
        RoomCollisionBlock initialBlock)
    {
        RoomCollisionBlock block = initialBlock;
        for (int linkCount = 0; linkCount < 16; linkCount++)
        {
            switch (block.CollisionType)
            {
                case RoomCollisionType.HorizontalExtension:
                case RoomCollisionType.VerticalExtension:
                    if (block.Behavior.ExtensionOffset == 0)
                        return null;
                    break;

                case RoomCollisionType.Air:
                case RoomCollisionType.Slope:
                case RoomCollisionType.SpikeAir:
                case RoomCollisionType.SpecialAir:
                case RoomCollisionType.ShootableAir:
                case RoomCollisionType.UnrevealedAir:
                case RoomCollisionType.BombableAir:
                case RoomCollisionType.SolidBlock:
                case RoomCollisionType.DoorBlock:
                case RoomCollisionType.SpikeBlock:
                case RoomCollisionType.SpecialBlock:
                case RoomCollisionType.ShootableBlock:
                case RoomCollisionType.GrappleBlock:
                case RoomCollisionType.BombableBlock:
                    return block;
                default:
                    throw new InvalidOperationException($"Undefined RoomCollisionType {block.CollisionType}.");
            }

            Position16<RoomBlocks> target = level.GridBlockStepped(block.At, block.ExtensionLink) ??
                throw new InvalidDataException(
                    $"Projectile extension at block {block.Position} resolved outside the " +
                    $"{level.Size.Area}-block room grid.");
            block = level.GetCollisionBlock(target);
        }

        throw new InvalidDataException(
            $"Projectile extension chain from block {initialBlock.Position} did not terminate.");
    }

    /// <summary>Routes a shootable behaviour to its downward gate, resident room object, blue door, or ordinary shot-block owner.</summary>
    internal static void TrySpawnShootableReaction(
        RoomLevelData level,
        SamusProjectile slot,
        RoomCollisionBlock block,
        ISamusRoomObjects? roomObjects)
    {
        if (roomObjects is null)
            return;

        // The gate-trigger behaviours bypass the normal 16-entry shot-block table and
        // allocates one of eight temporary trigger room objects. Dispatch it before the table-range
        // validation below; those behaviours are intentionally outside that ordinary domain.
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.TryGetDownwardGateTrigger(out _))
        {
            roomObjects.Gates.TrySpawnDownwardGateTrigger(level, block.At, block.Behavior, slot.Type);
            return;
        }

        // The intact n00b tube, colored doors, eye doors, grey doors, and other resident
        // actors all share the shootable resident-object trigger. The resident room object determines the accepted family
        // after the generic shot trigger publishes the projectile type.
        if (block.Behavior.Shootable == ShootableReaction.ResidentObjectTrigger)
        {
            NotifyResidentProjectileHit(roomObjects, block, slot.Type);
            return;
        }

        // The shot-block table's entries 40..43 select the four blue-door entry room objects. These are not
        // ordinary breakable blocks: setup changes the cap origin to a solid block and the
        // door's animation opens all four blocks over eighteen frames. Keep this dispatch
        // beside the general table lookup so every beam/missile/bomb collision reaches the
        // same owner and power bombs retain the blue door setup's rejection behavior.
        if (block.Behavior.TryGetBlueDoorOrientation(out _))
        {
            roomObjects.DoorObjects.TrySpawnBlueDoorOpening(
                level,
                block.At,
                block.Behavior,
                slot.Type);
            return;
        }

        // The breaks, the Power Bomb and Super Missile blocks and the empty reaction are the ordinary
        // shot-block reactions, and the collision probe is the deliberate no-op room object. The
        // reaction that sets off only in a shootable block is also admitted so shootable-solid can
        // preserve its no-op slot; shootable-air's method exits without allocating. The RoomObject
        // owner performs the power-bomb/Super-Missile family checks synchronously, just as each
        // setup sees the current projectile type while the room object spawns.
        bool translatedBehavior = block.Behavior.Shootable is ShootableReaction.NothingWhenSolid or
                ShootableReaction.AnimalEscape or ShootableReaction.CollisionProbe ||
            ShotBlockReactions.IsLeading(block.Behavior.Shootable);
        if (translatedBehavior)
        {
            roomObjects.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                block.At,
                block.Behavior,
                slot.Type,
                solidBlock: block.CollisionType == RoomCollisionType.ShootableBlock);
        }
    }

    /// <summary>Publishes the projectile type to the room object occupying a resident-trigger block and fails if none owns it.</summary>
    private static void NotifyResidentProjectileHit(
        ISamusRoomObjects? roomObjects,
        RoomCollisionBlock block,
        SamusProjectileType projectileType)
    {
        if (roomObjects is not null &&
            roomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(block.At, projectileType))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Projectile hit resident-trigger block {block.Position} with no active room-object owner.");
    }

    /// <summary>
    /// The horizontal velocity a shot fired straight left starts its first block scan with: the smallest leftward
    /// velocity, 1/256 pixel per update, so the scan samples the left radius.
    /// </summary>
    private static readonly SignedEightEight LeftScanVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1);

    /// <summary>
    /// Kills a beam as block collision does: the
    /// explosion's sound queues (no cinematic test) before the swap.
    /// </summary>
    internal void KillBeamOnBlockCollision(SamusProjectile slot)
    {
        _impactAudio.RequestBeamImpactSound();
        KillBeam(slot);
    }

    /// <summary>Moves a beam explosion to its directional leading edge and installs the explosion animation.</summary>
    internal static void KillBeam(SamusProjectile slot)
    {
        // Killing the beam moves the explosion anchor to the leading edge before the explosion
        // replaces the animation. Diagonals adjust both axes in their respective signs.
        SamusProjectileDirection direction = slot.Direction;
        if (direction is SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight)
            slot.Position += new Offset16<RoomPixels>(slot.Radius.X, 0);
        else if (direction is SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft)
            slot.Position -= new Offset16<RoomPixels>(slot.Radius.X, 0);

        if (direction is SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or
            SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft)
            slot.Position -= new Offset16<RoomPixels>(0, slot.Radius.Y);
        else if (direction is SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingRight or
            SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.DownLeft)
            slot.Position += new Offset16<RoomPixels>(0, slot.Radius.Y);

        slot.Type = slot.Type.AsFamily(SamusProjectileFamily.BeamExplosion);
        slot.Playback = SamusProjectilePlayback.Start(SamusProjectileSegment.BeamExplosion);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Damage = 8;
        slot.FrameBehavior = SamusProjectileFrameBehavior.None;
    }

    /// <summary>Converts a missile impact to its family-specific explosion and requests Super Missile quake and impact audio.</summary>
    internal void KillMissile(
        SamusProjectile slot,
        SamusBombProjectileSystem sharedProjectiles)
    {
        // The kill step clears families beyond missiles immediately. A Super
        // Missile's supplemental probe can collide again after becoming an explosion;
        // restarting its animation here would extend both its lifetime and damage.
        if (slot.Type.Family.IsBombOrExplosion)
        {
            _animation.ClearProjectile(slot);
            return;
        }

        // Missile point collisions already supply the impact coordinate. Only the beam
        // branch of the step applies a leading-edge radius adjustment.

        // Impact audio is separate from launch audio and can also originate in enemy
        // collision before the projectile movement owner executes.
        _impactAudio.RequestMissileImpactSound();
        bool wasSuperMissile = slot.Type.IsFamily(SamusProjectileFamily.SuperMissile);
        slot.Type = slot.Type.AsFamily(SamusProjectileFamily.MissileExplosion);
        slot.Playback = SamusProjectilePlayback.Start(
            wasSuperMissile ? SamusProjectileSegment.SuperMissileExplosion : SamusProjectileSegment.MissileExplosion);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Damage = 8;
        slot.FrameBehavior = SamusProjectileFrameBehavior.None;

        if (wasSuperMissile)
        {
            // The impact writes the shared room quake immediately; later producers
            // can replace it before the frame's shake handler consumes the request.
            _earthquakes.RequestSuperMissileEarthquake(SuperMissileDefinitions.WallImpactEarthquake);
        }

        // Only cooldowns 21+ are shortened to 20. A normal missile begins at ten, so ordinary
        // wall impact does not extend or replace its remaining fire delay.
        if (sharedProjectiles.CooldownTimer.IsAtLeast(21))
            sharedProjectiles.SetSharedCooldown(20);
    }
    /// <summary>
    /// Scans the rows a Wave shot's leading edge spans for block reactions, one row per step from its top row; where the
    /// scan runs past the room's ceiling and lands halfway through a block (<see cref="WaveScanRead"/>) it reacts to the
    /// terrain <paramref name="straddledTerrain"/> finds straddling that block and the next.
    /// </summary>
    /// <param name="level">The room's terrain as it is now.</param>
    /// <param name="slot">The Wave or Hyper shot.</param>
    /// <param name="roomObjects">The room's objects, which a block reaction may spawn; none in a room without them.</param>
    /// <param name="straddledTerrain">The installation's straddled terrain, which the projectile system's terrain collision holds.</param>
    internal static void ScanHorizontalWaveShotReactions(
        RoomLevelData level, SamusProjectile slot, ISamusRoomObjects? roomObjects, StraddledTerrain<RoomTerrainBlock> straddledTerrain)
    {
        Ensure.NotNull(straddledTerrain);
        int top = CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y);
        int bottom = CoordinateAxis.Wrap(slot.Position.Y.Whole + slot.Radius.Y - 1);
        int span = RoomGridConversions.WholeBlocksIn(CoordinateAxis.Wrap(bottom - RoomGridConversions.BlockStart(top)));
        int targetX = CoordinateAxis.Wrap(slot.Velocity.X.IsNegative
            ? slot.Position.X.Whole - slot.Radius.X : slot.Position.X.Whole + slot.Radius.X - 1);
        // The game's check reads whether the whole coordinate is negative, not whether its pixel offset within the
        // screen is 128 or more. Testing the latter disables Wave reactions throughout
        // the lower half of every screen, including ordinary blue-door caps.
        if (span >= 16 || CoordinateAxis.AsSigned(slot.Position.Y.Whole) < 0 ||
            RoomGridConversions.PixelToScreen(slot.Position.Y.Whole) >= RoomGridConversions.ScreensCovering(level.Size.Y) ||
            RoomGridConversions.PixelToScreen(targetX) >= RoomGridConversions.ScreensCovering(level.Size.X))
            return;

        for (int step = 0; step <= span; step++)
        {
            WaveScanRead read = WaveScanRead.At(Position16<RoomPixels>.Of(targetX, top).ToRoomBlock(), level.Size.X, step);
            Position16<RoomBlocks> readBlock = read.BlockIn(level.Size.X);
            if (!level.Size.Contains(readBlock))
                continue;
            RoomCollisionBlock block = level.GetCollisionBlock(readBlock);
            if (read.StraddlesNext)
            {
                // The straddled block is the next one in the room's reading order, on the next row past a row's end.
                Position16<RoomBlocks> nextBlock = level.GridBlockStepped(readBlock, Offset16<RoomBlocks>.Of(1, 0)) ??
                    throw new NotSupportedException("A straddled Wave collision read crosses the room's grid.");
                RoomTerrainBlock next = level.GetCollisionBlock(nextBlock).Terrain;
                // Dispatch consumes the straddled block; the behaviour and room-object setup
                // still use the block the read starts in, as aligned reads do.
                block = block with { Terrain = straddledTerrain.Straddling(block.Terrain, next) };
            }
            RunShotReaction(level, slot, block, roomObjects, new ShotSpan(span, span - step, targetX), out bool endSpan);
            // Wave discards whether a block stops it, but still honors room-object scan termination.
            if (endSpan) return;
        }
    }

    /// <summary>
    /// Preserves the vertical scan's linear block order across the room's side edges.
    /// </summary>
    internal static void ScanVerticalWaveShotReactions(
        RoomLevelData level, SamusProjectile slot, ISamusRoomObjects? roomObjects)
    {
        int left = CoordinateAxis.Wrap(slot.Position.X.Whole - slot.Radius.X);
        int right = CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - 1);
        int span = RoomGridConversions.WholeBlocksIn(CoordinateAxis.Wrap(right - RoomGridConversions.BlockStart(left)));
        if (span >= 16) return;
        int targetY = CoordinateAxis.Wrap(slot.Velocity.Y.IsNegative
            ? slot.Position.Y.Whole - slot.Radius.Y
            : slot.Position.Y.Whole + slot.Radius.Y - 1);
        int centerScreenX = RoomGridConversions.PixelToScreen(slot.Position.X.Whole);
        if (centerScreenX >= RoomGridConversions.ScreensCovering(level.Size.X) ||
            RoomGridConversions.PixelToScreen(targetY) >= RoomGridConversions.ScreensCovering(level.Size.Y))
            return;

        // The row lookup takes the target row within the scan cycle. X is deliberately not clipped to the
        // room width: right-edge spans reach the following row; a span left
        // of the room's edge starts 4095 blocks forward while the center remains inside the room.
        // The block reaction step bounds the resulting allocation
        // offset, not the horizontal extent of this individual block span.
        Position16<RoomBlocks> firstBlock = Position16<RoomPixels>.Of(left, targetY).ToRoomBlock();
        for (int step = 0; step <= span; step++)
        {
            Position16<RoomBlocks> block = BlockScanCycle.BlockAlongRows(firstBlock, level.Size.X, step);
            if (level.Size.Contains(block))
            {
                RunShotReaction(level, slot, level.GetCollisionBlock(block), roomObjects,
                    new ShotSpan(span, span - step, targetY), out bool endSpan, axis: Axis.Vertical);
                if (endSpan) return;
            }
        }
    }
}
