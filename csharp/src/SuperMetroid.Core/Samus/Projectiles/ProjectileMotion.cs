using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Per-update projectile movement: the beam, Wave Beam, Hyper Beam, missile and Super Missile flights, Super Missile links, and the movement-window deletion check.</summary>
[RoomLifetime]
internal sealed class ProjectileMotion
{
    /// <summary>Clears shots that expire or leave the movement window.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>The shot pool, whose free places the invisible Super Missile links take.</summary>
    private readonly SamusShotPool _pool;
    /// <summary>Converts beam and missile block impacts into their family-specific outcomes.</summary>
    private readonly ProjectileTerrainCollision _terrainCollision;
    /// <summary>Emits beam and missile trails before their parent shots move.</summary>
    private readonly ProjectileTrails _trails;

    /// <summary>Connects projectile motion to animation lifetime, terrain reactions, the shot pool, and trails.</summary>
    internal ProjectileMotion(ProjectileAnimation animation, SamusShotPool pool, ProjectileTerrainCollision terrainCollision, ProjectileTrails trails)
    {
        _animation = animation;
        _pool = pool;
        _terrainCollision = terrainCollision;
        _trails = trails;
    }
    /// <summary>Accelerates a non-Wave beam, emits its old-position trail, and resolves directional block collision.</summary>
    internal void FlyBeam(
        RoomLevelData level,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        ISamusRoomObjects roomObjects)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }

        // The trail beam's update allocates a persistent trail before applying acceleration or movement.
        // Thus the detached 8x8 sprite records the beam's old world position and visibly falls
        // behind it. Failure to preserve this order makes the trail sit inside the projectile.
        shot.TrailTimer = shot.TrailTimer.Decrement().Remaining;
        if (shot.TrailTimer.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.TrailInterval);
            _trails.SpawnTrail(shot);
        }

        Accelerate(shot, SamusProjectileAccelerations.For(shot.Direction).Beam);

        SamusProjectileDirection direction = shot.Direction;
        bool collided = direction switch
        {
            SamusProjectileDirection.UpFacingRight or
            SamusProjectileDirection.DownFacingRight or
            SamusProjectileDirection.DownFacingLeft or
            SamusProjectileDirection.UpFacingLeft => ProjectileTerrainCollision.MoveVertically(level, shot, roomObjects),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left =>
                ProjectileTerrainCollision.MoveHorizontally(level, shot, roomObjects),
            SamusProjectileDirection.UpRight or
            SamusProjectileDirection.DownRight or
            SamusProjectileDirection.DownLeft or
            SamusProjectileDirection.UpLeft =>
                ProjectileTerrainCollision.MoveHorizontally(level, shot, roomObjects) ||
                ProjectileTerrainCollision.MoveVertically(level, shot, roomObjects),
            _ => throw new InvalidDataException(
                $"Beam shot has undefined direction {direction}."),
        };

        if (collided)
            _terrainCollision.KillBeamOnBlockCollision(shot);

        // The beam update checks the movement window after the direction step even
        // when collision changed the beam into an explosion. KillBeam moves its
        // origin to the contact edge, which can put an otherwise retained shot
        // outside that window on this very frame.
        DeleteIfOutsideMovementWindow(shot, cameraPosition);
    }

    /// <summary>Counts the Wave Beam's trail, every <paramref name="trailInterval"/> updates after its first, before its block-transparent movement.</summary>
    internal void FlyWaveBeam(
        RoomLevelData level,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        ISamusRoomObjects roomObjects,
        int trailInterval)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }

        // The Wave shots differ only in the interval reloaded after a trail timer expires. Firing always starts it at
        // four; an uncharged Wave or Ice and Wave shot reloads three only after its first trail. Spawning precedes
        // acceleration and movement, so the trail samples the old world position just like a beam without Wave.
        shot.TrailTimer = shot.TrailTimer.Decrement().Remaining;
        if (shot.TrailTimer.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(trailInterval);
            _trails.SpawnTrail(shot);
        }

        RunWaveBeamShared(level, shot, cameraPosition, roomObjects);
    }

    /// <summary>Uses Wave's block-transparent movement for Hyper Beam without spawning projectile trails.</summary>
    internal void FlyHyperBeam(
        RoomLevelData level,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        ISamusRoomObjects roomObjects)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }

        // Hyper's update falls directly into the shared Wave movement. Hyper deliberately has
        // no projectile-trail timer or trail spawn; its long beam picture
        // and the separately counting muzzle flare provide the complete presentation.
        RunWaveBeamShared(level, shot, cameraPosition, roomObjects);
    }

    /// <summary>Advances Wave or Hyper positions and notifies shootable blocks while ignoring whether terrain stops the shot.</summary>
    private void RunWaveBeamShared(
        RoomLevelData level,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        ISamusRoomObjects roomObjects)
    {
        SamusProjectileDirection direction = shot.Direction;
        Accelerate(shot, SamusProjectileAccelerations.For(direction).Beam);

        // Wave's block collisions advance the same 1/65536-pixel positions and scan every block touched by
        // the projectile radii, but deliberately never report a collision. That is
        // the defining Wave Beam behavior: block reactions may run, yet terrain never kills
        // or clips the shot. The span scans below publish ordinary shootable-block
        // room objects while discarding their collision result.
        if (MovesHorizontally(direction))
            _terrainCollision.MoveWaveShotHorizontally(level, shot, roomObjects);
        if (MovesVertically(direction))
        {
            shot.Position = shot.Position.AdvanceY(shot.Velocity.Y.ToSixteenSixteenDelta());
            ProjectileTerrainCollision.ScanVerticalWaveShotReactions(level, shot, roomObjects);
        }

        DeleteIfOutsideMovementWindow(shot, cameraPosition);
    }

    /// <summary>Handles ordinary missile ignition, acceleration, trail, point collision, and post-impact deletion.</summary>
    internal void FlyMissile(
        RoomLevelData level,
        SamusState samus,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }

        shot.TrailTimer = shot.TrailTimer.Decrement().Remaining;
        if (shot.TrailTimer.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.TrailInterval);
            _trails.SpawnTrail(shot);
        }

        SamusProjectileDirection direction = shot.Direction;
        SamusDirectionAccelerations accelerations = SamusProjectileAccelerations.For(direction);

        // The missile's flight first applies the same small directional
        // acceleration as beams. On the ignition frame the missile's ignition below replaces velocity,
        // so this addition is intentionally overwritten; subsequent frames retain it.
        Accelerate(shot, accelerations.Beam);

        if (shot.MissileIgnition != SamusMissileIgnition.Ignited)
        {
            // A missile ignites on its first input phase: its velocity is rebuilt from its
            // ignition speed plus Samus's inherited motion.
            IgniteMissile(samus, shot);
        }
        else
        {
            Accelerate(shot, accelerations.Missile);
        }

        bool collided = direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft =>
                MoveMissileVertically(level, shot, roomObjects),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left =>
                MoveMissileHorizontally(level, shot, roomObjects),
            SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight or
                SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft =>
                MoveMissileHorizontally(level, shot, roomObjects) ||
                MoveMissileVertically(level, shot, roomObjects),
            _ => throw new InvalidDataException(
                $"Missile shot has undefined direction {direction}."),
        };
        if (collided)
            _terrainCollision.KillMissile(shot, sharedProjectiles);

        // The missile direction step may create an explosion just
        // beyond the retention boundary. The shot's flight still runs the offscreen
        // check on that same frame, after impact sound/cooldown side effects.
        DeleteIfOutsideMovementWindow(shot, cameraPosition);
    }

    /// <summary>Ignites a Super Missile with its invisible link and updates both point reactions after each movement axis.</summary>
    internal void FlySuperMissile(
        RoomLevelData level,
        SamusState samus,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            ClearAllSuperMissileLinks();
            return;
        }

        // The Super Missile trail first waits the four updates its firing sets, then two after every
        // expiry. Super Missiles therefore emit twice as frequently as ordinary missiles;
        // both families still select the same trail art.
        shot.TrailTimer = shot.TrailTimer.Decrement().Remaining;
        if (shot.TrailTimer.IsZero)
        {
            shot.TrailTimer = WrappingCountdown.Start(ProjectileTimingDefinitions.SuperMissileTrailInterval);
            _trails.SpawnTrail(shot);
        }

        SamusProjectileDirection direction = shot.Direction;
        if (shot.MissileIgnition != SamusMissileIgnition.Ignited)
        {
            // A Super Missile ignites as a missile does on its first input phase, and then
            // immediately allocates its invisible collision link.
            IgniteMissile(samus, shot);
            SpawnSuperMissileLink(samus, shot);
        }
        else
        {
            Accelerate(shot, SamusProjectileAccelerations.For(direction).SuperMissile);
        }

        bool collided = false;
        if (MovesHorizontally(direction))
        {
            collided = MoveMissileHorizontally(level, shot, roomObjects);
            if (collided)
                _terrainCollision.KillMissile(shot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(
                level, shot, axis: Axis.Horizontal, sharedProjectiles, roomObjects);
        }
        if (!collided && MovesVertically(direction))
        {
            collided = MoveMissileVertically(level, shot, roomObjects);
            if (collided)
                _terrainCollision.KillMissile(shot, sharedProjectiles);
            UpdateSuperMissileLinkAxis(
                level, shot, axis: Axis.Vertical, sharedProjectiles, roomObjects);
        }

        if (DeleteIfOutsideMovementWindow(shot, cameraPosition))
        {
            ClearAllSuperMissileLinks();
        }
    }

    /// <summary>Whether a shot in <paramref name="direction"/> moves horizontally: every direction but straight up or down.</summary>
    private static bool MovesHorizontally(SamusProjectileDirection direction) => direction is
        SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight or
        SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft;

    /// <summary>Whether a shot in <paramref name="direction"/> moves vertically: every direction but straight left or right.</summary>
    private static bool MovesVertically(SamusProjectileDirection direction) => direction is
        SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight or
        SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft or
        SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft;

    /// <summary>
    /// Adds one update's acceleration to the shot's velocity, the direction's entry for the shot's kind. The velocity
    /// stays far inside its range, since a shot accelerating that long has left the movement window.
    /// </summary>
    private static void Accelerate(SamusProjectile shot, EightEightVector<RoomPixels> acceleration) =>
        shot.Velocity += acceleration;

    /// <summary>
    /// Applies the shared beam/missile deletion window after movement. The game's
    /// check allows a 64-pixel apron above and left of the viewport and extends through
    /// coordinate 319 on the other axes; centralizing those bounds keeps projectile
    /// families from drifting apart.
    /// </summary>
    /// <returns>True when the projectile was outside the movement window and cleared.</returns>
    private bool DeleteIfOutsideMovementWindow(
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition)
    {
        const int MinimumVisibleCoordinate = -64;
        const int MaximumVisibleCoordinateExclusive = 320;

        int screenX = CoordinateAxis.Difference(shot.Position.X.Whole, cameraPosition.X);
        int screenY = CoordinateAxis.Difference(shot.Position.Y.Whole, cameraPosition.Y);
        bool outside =
            screenX < MinimumVisibleCoordinate ||
            screenX >= MaximumVisibleCoordinateExclusive ||
            screenY < MinimumVisibleCoordinate ||
            screenY >= MaximumVisibleCoordinateExclusive;
        if (!outside)
            return false;

        _animation.ClearProjectile(shot);
        return true;
    }

    /// <summary>
    /// Ignites a missile: its velocity is rebuilt from its ignition speed, one pixel per update, or more after a
    /// reflection's head start, combined with Samus's inherited motion.
    /// </summary>
    private static void IgniteMissile(SamusState samus, SamusProjectile shot)
    {
        SignedEightEight ignitionSpeed = shot.MissileIgnition == SamusMissileIgnition.PendingAfterReflection
            ? SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed
            : SamusProjectileMotionDefinitions.MissileIgnitionSpeed;
        shot.MissileIgnition = SamusMissileIgnition.Ignited;
        BeamFiring.InitializeDirectionalVelocity(samus.ProjectileInheritance, shot, ignitionSpeed);
    }

    /// <summary>
    /// Creates the invisible collision link at ignition in the lowest free place and records its place in the owner; with
    /// no free place the owner keeps the link it named (<see cref="UnlinkedSuperMissileLink"/>).
    /// </summary>
    internal void SpawnSuperMissileLink(
        SamusState samus,
        SamusProjectile owner)
    {
        if (_pool.FindFreePlace() is not { } place)
            return;

        SamusProjectile link = _pool.OccupyAtRest(place);
        link.Type = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
        link.PicturedBeams = owner.PicturedBeams;
        link.Direction = owner.Direction;
        link.IsSpent = owner.IsSpent;
        link.Position = link.Position.WithWhole(owner.Position.Whole);

        // The Super Missile link deliberately calls the ordinary muzzle setup again after copying
        // the owner's coordinates. At ignition both positions are equivalent; retaining the
        // call matters for moving/transition poses whose origin tables can change.
        BeamFiring.InitializePosition(samus, link);
        const SamusProjectileKind kind = SamusProjectileKind.SuperMissileLink;
        link.Damage = SamusProjectileDamage.Read(kind);
        link.Playback = SamusProjectilePlayback.Start(SamusProjectileKinds.SoleAnimation(kind));
        link.FrameHold = WrappingCountdown.Start(1);
        link.Behavior = SamusShotBehaviors.SuperMissileLink;
        link.SuperMissileLinkPlace = UnlinkedSuperMissileLink.Place;

        owner.SuperMissileLinkPlace = link.Place;
        _pool.ShotCount.CountShot();
    }

    /// <summary>Keeps an ordinary link stationary or clears the whole Super Missile family on its deletion signal.</summary>
    internal void HoldSuperMissileLink(SamusProjectile link)
    {
        // The link update leaves an ordinary link completely stationary. A spent link clears every
        // Super Missile shot, including its owner.
        if (!link.IsSpent)
            return;
        _animation.ClearProjectile(link);
        ClearAllSuperMissileLinks();
    }

    /// <summary>Places a fast link behind the owner for its extra point reaction, retaining impact checks on explosion frames.</summary>
    private void UpdateSuperMissileLinkAxis(
        RoomLevelData level,
        SamusProjectile owner,
        Axis axis,
        SamusBombProjectileSystem sharedProjectiles,
        ISamusRoomObjects roomObjects)
    {
        if (owner.MissileIgnition != SamusMissileIgnition.Ignited || owner.SuperMissileLinkPlace is not { } linkPlace)
            return;
        SamusProjectile link = _pool.Shots[linkPlace];
        if (!link.IsActive)
            return;

        SignedEightEight velocity = axis == Axis.Vertical ? owner.Velocity.Y : owner.Velocity.X;
        int wholeMagnitude = velocity.MagnitudeWholePixels;
        bool ownerExploded = owner.Type.IsFamily(SamusProjectileFamily.MissileExplosion);
        if (wholeMagnitude < SuperMissileDefinitions.FastLinkSpeed)
        {
            // A slow link clears at once if the owner has already exploded,
            // otherwise it only follows the owner's center.
            if (ownerExploded)
            {
                _animation.ClearProjectile(link);
                return;
            }
            if (axis == Axis.Vertical)
                link.Position = link.Position.WithWholeY(owner.Position.Y.Whole);
            else
                link.Position = link.Position.WithWholeX(owner.Position.X.Whole);
            return;
        }

        // A fast link samples its shortfall behind the owner's speed beyond the owner's previous
        // position to close the point-collision gap, and does so even when the owner has
        // just exploded; only then is it cleared. A missile that skips a door cap and
        // strikes the door behind it still opens the cap through its link.
        int offset = wholeMagnitude - SuperMissileDefinitions.FastLinkShortfall;
        int ownerPosition = axis == Axis.Vertical ? owner.Position.Y.Whole : owner.Position.X.Whole;
        int linkPosition = CoordinateAxis.Wrap(ownerPosition + (velocity.IsNegative ? offset : -offset));
        if (axis == Axis.Vertical)
            link.Position = link.Position.WithWholeY(linkPosition);
        else
            link.Position = link.Position.WithWholeX(linkPosition);
        if (MissilePointReaction(
                level,
                link,
                roomObjects: roomObjects))
            _terrainCollision.KillMissile(link, sharedProjectiles);
        if (ownerExploded)
            _animation.ClearProjectile(link);
    }

    /// <summary>Ends every Super Missile shot after owner or link termination.</summary>
    private void ClearAllSuperMissileLinks()
    {
        foreach (SamusProjectile candidate in _pool.InUpdateOrder)
        {
            if (candidate.Type.IsFamily(SamusProjectileFamily.SuperMissile))
                _animation.ClearProjectile(candidate);
        }
    }

    /// <summary>Advances missile X and tests its center unless it has crossed the room's screen-width boundary.</summary>
    private static bool MoveMissileHorizontally(
        RoomLevelData level,
        SamusProjectile shot,
        ISamusRoomObjects roomObjects)
    {
        shot.Position = shot.Position.AdvanceX(shot.Velocity.X.ToSixteenSixteenDelta());

        // Missile block collision tests the projectile center, not its radius-spanning leading edge. Room
        // width is counted in 256-pixel screens; coordinates in or past the first screen beyond
        // the room skip reaction and are left for the later 64-pixel viewport deletion.
        int roomWidthInScreens = RoomGridConversions.ScreensCovering(level.Size.X);
        if (RoomGridConversions.PixelToScreen(shot.Position.X.Whole) >= roomWidthInScreens)
            return false;
        return MissilePointReaction(
            level, shot, roomObjects: roomObjects);
    }

    /// <summary>Advances missile Y and tests its center unless it has crossed the room's screen-height boundary.</summary>
    private static bool MoveMissileVertically(
        RoomLevelData level,
        SamusProjectile shot,
        ISamusRoomObjects roomObjects)
    {
        shot.Position = shot.Position.AdvanceY(shot.Velocity.Y.ToSixteenSixteenDelta());

        int roomHeightInScreens = RoomGridConversions.ScreensCovering(level.Size.Y);
        if (RoomGridConversions.PixelToScreen(shot.Position.Y.Whole) >= roomHeightInScreens)
            return false;
        return MissilePointReaction(
            level, shot, roomObjects: roomObjects);
    }

    /// <summary>Resolves the missile center's extension chain, room-object reaction, and whether the block stops the missile, by block type.</summary>
    /// <returns>True when the block stops the missile.</returns>
    private static bool MissilePointReaction(
        RoomLevelData level,
        SamusProjectile shot,
        ISamusRoomObjects roomObjects)
    {
        int blockX = RoomGridConversions.PixelToBlock(shot.Position.X.Whole);
        int blockY = RoomGridConversions.PixelToBlock(shot.Position.Y.Whole);
        if ((uint)blockX >= level.Size.X ||
            (uint)blockY >= level.Size.Y)
        {
            return false;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(new(CoordinateAxis.Wrap(blockX), CoordinateAxis.Wrap(blockY)));

        // Extension blocks end the step with a repeat result, causing the same
        // point reaction to run again on the signed horizontal/vertical parent. A zero-behaviour
        // extension is transparent. Reuse the level resolver so missiles
        // and the broad Samus body scans cannot disagree about chained extensions.
        if (!SamusBlockCollision.TryResolveExtension(level, ref block))
            return false;
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
        {
            _ = roomObjects.CollectibleObjects.TryNotifyCollectibleProjectileHit(block.At);
            return true;
        }
        if (block.CollisionType is RoomCollisionType.ShootableAir or RoomCollisionType.ShootableBlock)
        {
            // The shootable reactions run the spawn before the block's collision kind decides
            // the result: shootable air lets the missile pass; a shootable block stops it.
            ProjectileTerrainCollision.TrySpawnShootableReaction(level, shot, block, roomObjects);
            return block.CollisionType == RoomCollisionType.ShootableBlock;
        }

        if (block.CollisionType is RoomCollisionType.BombableAir or RoomCollisionType.BombableBlock)
        {
            // Bombable reactions always index the bomb-block room-object table for nonnegative behaviour.
            // A missile or Super Missile still spawns the room object, but the bomb-block setup
            // immediately clears the new room object because its family is neither a bomb
            // nor a power bomb; no terrain write, timer, sound, or persistent room object survives that update.
            // Whether the block stops the missile is independent of setup: bombable air passes through,
            // while bombable solid destroys the missile.
            return block.CollisionType == RoomCollisionType.BombableBlock;
        }
        return block.CollisionType switch
        {
            // The point reaction treats these categories as transparent air.
            RoomCollisionType.Air or
            RoomCollisionType.SpikeAir or
            RoomCollisionType.SpecialAir or
            RoomCollisionType.UnrevealedAir => false,

            // These categories stop the missile immediately and therefore kill it.
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpikeBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.GrappleBlock => true,

            // Missile slope collision divides slope behaviour values into the five square definitions and
            // the remaining 27 pixel-height definitions. Both paths use the missile center.
            RoomCollisionType.Slope =>
                MissileSlopePointReaction(block, shot),

            // The cases above cover every collision kind left after following the extensions.
            // Keep an explicit guard so a kind reaching here fails rather than silently
            // inventing a collision result.
            RoomCollisionType.ShootableAir or
                RoomCollisionType.HorizontalExtension or
                RoomCollisionType.BombableAir or
                RoomCollisionType.ShootableBlock or
                RoomCollisionType.VerticalExtension or
                RoomCollisionType.BombableBlock => throw new InvalidDataException(
                $"Invalid missile collision type {block.CollisionType} at block {block.Position}."),
            _ => throw new InvalidDataException(
                $"Invalid missile collision type {block.CollisionType} at block {block.Position}."),
        };
    }

    /// <summary>Tests the missile center against mirrored height tables or the selected square-slope quadrant.</summary>
    internal static bool MissileSlopePointReaction(
        RoomCollisionBlock block,
        SamusProjectile shot)
    {
        SlopeShape slopeShape = block.Behavior.Slope.Shape;
        if (!slopeShape.IsSquare)
        {
            // Non-square slope collision mirrors the projectile's within-block coordinate before indexing
            // the game table: the behaviour's flips mirror X and Y. A table height at or
            // above the mirrored Y point (`height - y <= 0`, signed) is
            // solid. Values can reach 20 for overhanging shapes, so do not reduce them modulo 16.
            int xInBlock = RoomGridConversions.OffsetWithinBlock(shot.Position.X.Whole, mirrored: block.Behavior.Slope.FlipsHorizontally);
            int yInBlock = RoomGridConversions.OffsetWithinBlock(shot.Position.Y.Whole, mirrored: block.Behavior.Slope.FlipsVertically);

            int height = slopeShape.HeightAt(xInBlock);
            return height <= yInBlock;
        }

        // Square slope collision treats each square slope as four 8x8 quadrants: the missile centre's halves of the
        // block, mirrored by the block's reflections, select the quadrant whichever axis the missile moves along.
        return SquareSlopes.IsSolid(block.Behavior, SquareSlopeQuadrant.Showing(block.Behavior,
            SquareSlopeQuadrant.ColumnFor(RoomGridConversions.InSecondHalfOfBlock(shot.Position.X.Whole)), SquareSlopeQuadrant.RowFor(RoomGridConversions.InSecondHalfOfBlock(shot.Position.Y.Whole))));
    }
}
