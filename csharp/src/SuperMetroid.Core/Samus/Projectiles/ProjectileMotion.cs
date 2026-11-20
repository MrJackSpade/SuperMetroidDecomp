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

        // A beam leaves its trail where it is before it accelerates and moves, so the trail falls behind it.
        if (shot.CountsDownToTrail(ProjectileTimingDefinitions.TrailInterval))
            _trails.SpawnTrail(shot);

        Accelerate(shot, SamusProjectileAccelerations.For(shot.Direction).Beam);

        // A diagonal beam moves vertically only when its horizontal move found no collision.
        SamusProjectileDirectionAxes axes = SamusProjectileDirectionDefinitions.Axes(shot.Direction);
        bool collided = (axes.MovesHorizontally && ProjectileTerrainCollision.MoveHorizontally(level, shot, roomObjects)) ||
            (axes.MovesVertically && ProjectileTerrainCollision.MoveVertically(level, shot, roomObjects));

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

        // The Wave shots differ only in the interval reloaded after a trail timer expires. Firing always starts it at the
        // trail interval; an uncharged Wave or Ice and Wave shot reloads its shorter one only after its first trail. Spawning precedes
        // acceleration and movement, so the trail samples the old world position just like a beam without Wave.
        if (shot.CountsDownToTrail(trailInterval))
            _trails.SpawnTrail(shot);

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

        // Wave's block collisions advance the same positions and scan every block touched by
        // the projectile radii, but deliberately never report a collision. That is
        // the defining Wave Beam behavior: block reactions may run, yet terrain never kills
        // or clips the shot. The span scans below publish ordinary shootable-block
        // room objects while discarding their collision result.
        SamusProjectileDirectionAxes axes = SamusProjectileDirectionDefinitions.Axes(direction);
        if (axes.MovesHorizontally)
            _terrainCollision.MoveWaveShotHorizontally(level, shot, roomObjects);
        if (axes.MovesVertically)
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
        SamusBombProjectileSystem samusBombs,
        ISamusRoomObjects roomObjects)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            return;
        }

        if (shot.CountsDownToTrail(ProjectileTimingDefinitions.TrailInterval))
            _trails.SpawnTrail(shot);

        SamusProjectileDirection direction = shot.Direction;
        MissileShotState missile = shot.KindStateAs<MissileShotState>();
        if (!missile.Ignition.HasIgnited)
        {
            // A missile ignites on its first input phase: its velocity is rebuilt from its
            // ignition speed plus Samus's inherited motion.
            IgniteMissile(samus, shot, missile);
        }
        else
        {
            // An ignited missile takes a beam's small directional acceleration and then its own.
            SamusDirectionAccelerations accelerations = SamusProjectileAccelerations.For(direction);
            Accelerate(shot, accelerations.Beam);
            Accelerate(shot, accelerations.Missile);
        }

        // A diagonal missile moves vertically only when its horizontal move found no collision.
        SamusProjectileDirectionAxes axes = SamusProjectileDirectionDefinitions.Axes(direction);
        bool collided = (axes.MovesHorizontally && MoveMissileHorizontally(level, shot, roomObjects)) ||
            (axes.MovesVertically && MoveMissileVertically(level, shot, roomObjects));
        if (collided)
            _terrainCollision.KillMissile(shot, samusBombs);

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
        SamusBombProjectileSystem samusBombs,
        ISamusRoomObjects roomObjects)
    {
        if (shot.IsSpent)
        {
            _animation.ClearProjectile(shot);
            ClearAllSuperMissileLinks();
            return;
        }

        // The Super Missile trail first waits the updates its firing sets, then its own interval
        // (ProjectileTimingDefinitions.SuperMissileTrailInterval) after every expiry. Super Missiles therefore emit twice as frequently as ordinary missiles;
        // both families still select the same trail art.
        if (shot.CountsDownToTrail(ProjectileTimingDefinitions.SuperMissileTrailInterval))
            _trails.SpawnTrail(shot);

        SamusProjectileDirection direction = shot.Direction;
        SuperMissileShotState superMissile = shot.KindStateAs<SuperMissileShotState>();
        if (!superMissile.Ignition.HasIgnited)
        {
            // A Super Missile ignites as a missile does on its first input phase, and then
            // immediately allocates its invisible collision link.
            IgniteMissile(samus, shot, superMissile);
            SpawnSuperMissileLink(samus, shot, superMissile);
        }
        else
        {
            Accelerate(shot, SamusProjectileAccelerations.For(direction).SuperMissile);
        }

        bool collided = false;
        SamusProjectileDirectionAxes axes = SamusProjectileDirectionDefinitions.Axes(direction);
        if (axes.MovesHorizontally)
        {
            collided = MoveMissileHorizontally(level, shot, roomObjects);
            if (collided)
                _terrainCollision.KillMissile(shot, samusBombs);
            UpdateSuperMissileLinkAxis(
                level, shot, superMissile, axis: Axis.Horizontal, samusBombs, roomObjects);
        }
        if (!collided && axes.MovesVertically)
        {
            collided = MoveMissileVertically(level, shot, roomObjects);
            if (collided)
                _terrainCollision.KillMissile(shot, samusBombs);
            UpdateSuperMissileLinkAxis(
                level, shot, superMissile, axis: Axis.Vertical, samusBombs, roomObjects);
        }

        if (DeleteIfOutsideMovementWindow(shot, cameraPosition))
        {
            ClearAllSuperMissileLinks();
        }
    }

    /// <summary>
    /// Adds one update's acceleration to the shot's velocity, the direction's entry for the shot's kind. The velocity
    /// stays far inside its range, since a shot accelerating that long has left the movement window.
    /// </summary>
    private static void Accelerate(SamusProjectile shot, PixelVelocity<RoomPixels> acceleration) =>
        shot.Velocity += acceleration;

    /// <summary>
    /// Applies the shared beam/missile deletion window after movement. The game's
    /// check allows an apron above and left of the viewport (<see cref="SamusProjectileMotionDefinitions.MovementWindowApron"/>) and extends through
    /// <see cref="SamusProjectileMotionDefinitions.MovementWindowFarEdge"/> on the other sides; centralizing those bounds keeps projectile
    /// families from drifting apart.
    /// </summary>
    /// <returns>True when the projectile was outside the movement window and cleared.</returns>
    private bool DeleteIfOutsideMovementWindow(
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition)
    {
        int screenX = CameraPastRoomEdge.FromCamera(shot.Position.X.Whole, cameraPosition.X);
        int screenY = CameraPastRoomEdge.FromCamera(shot.Position.Y.Whole, cameraPosition.Y);
        bool outside =
            screenX < -SamusProjectileMotionDefinitions.MovementWindowApron ||
            screenX >= SamusProjectileMotionDefinitions.MovementWindowFarEdge ||
            screenY < -SamusProjectileMotionDefinitions.MovementWindowApron ||
            screenY >= SamusProjectileMotionDefinitions.MovementWindowFarEdge;
        if (!outside)
            return false;

        _animation.ClearProjectile(shot);
        return true;
    }

    /// <summary>
    /// Ignites a missile: its velocity is rebuilt from its ignition speed, one pixel per update, or more after a
    /// reflection's head start, combined with Samus's inherited motion.
    /// </summary>
    private static void IgniteMissile(SamusState samus, SamusProjectile shot, MissileShotState missile)
    {
        SignedEightEight ignitionSpeed = missile.Ignition is SamusMissileIgnition.PendingAfterReflection
            ? SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed
            : SamusProjectileMotionDefinitions.MissileIgnitionSpeed;
        missile.Ignite(ignitionSpeed);
        BeamFiring.InitializeDirectionalVelocity(samus.ProjectileInheritance, shot, ignitionSpeed);
    }

    /// <summary>
    /// Creates the invisible collision link at ignition in the lowest free place and records its place in the owner; with
    /// no free place the owner keeps the link it named (<see cref="UnlinkedSuperMissileLink"/>).
    /// </summary>
    private void SpawnSuperMissileLink(
        SamusState samus,
        SamusProjectile owner,
        SuperMissileShotState ownerState)
    {
        if (_pool.FindFreePlace() is not { } place)
            return;

        SamusProjectile link = _pool.OccupyAtRest(place);
        link.Type = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
        link.PicturedBeams = owner.PicturedBeams;
        link.Direction = owner.Direction;
        if (owner.IsSpent)
            link.Spend();
        link.Position = link.Position.WithWhole(owner.Position.Whole);

        // The link then takes the muzzle's position for Samus's pose as it is now, over the owner's coordinates; the two
        // differ when her pose's muzzle has moved since the owner was placed.
        BeamFiring.InitializePosition(samus, link);
        const SamusProjectileKind kind = SamusProjectileKind.SuperMissileLink;
        link.Damage = SamusProjectileDamage.Read(kind);
        link.Animation = SamusProjectilePlayback.Start(SamusProjectileKinds.SoleAnimation(kind));
        link.Install(SamusShotBehaviors.SuperMissileLink,
            new SuperMissileShotState(new SamusMissileIgnition.Pending(), UnlinkedSuperMissileLink.Place));

        ownerState.LinkChainPosition = link.Place;
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
        SuperMissileShotState ownerState,
        Axis axis,
        SamusBombProjectileSystem samusBombs,
        ISamusRoomObjects roomObjects)
    {
        if (!ownerState.Ignition.HasIgnited || ownerState.LinkChainPosition is not { } linkPlace)
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
        // strikes the door behind it still opens the cap through its link. A missile flown past the room's top or left
        // edge stands at a place OffRoomPlaces keeps, and its link trails it across that edge.
        int offset = wholeMagnitude - SuperMissileDefinitions.FastLinkShortfall;
        int ownerPosition = axis == Axis.Vertical ? owner.Position.Y.Whole : owner.Position.X.Whole;
        int linkPosition = OffRoomPlaces.Place(ownerPosition - velocity.Along(offset));
        if (axis == Axis.Vertical)
            link.Position = link.Position.WithWholeY(linkPosition);
        else
            link.Position = link.Position.WithWholeX(linkPosition);
        if (MissilePointReaction(
                level,
                link,
                roomObjects: roomObjects))
            _terrainCollision.KillMissile(link, samusBombs);
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
        // width is counted in screens; coordinates in or past the first screen beyond
        // the room skip reaction and are left for the later viewport deletion.
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
        if (blockX >= level.Size.X || blockY >= level.Size.Y)
            return false;

        RoomCollisionBlock block = level.GetCollisionBlock(new(blockX, blockY));

        // An extension block reacts as the block it leads to, as it does for Samus's body; one that leads nowhere is air.
        if (!level.TryReachBlockStoodFor(ref block))
            return false;
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

            // A slope stops a missile by where its centre lies: in a square slope's solid quarter, or below another
            // slope's surface.
            RoomCollisionType.Slope =>
                MissileSlopePointReaction(block, shot),

            RoomCollisionType.ShootableAir or
            RoomCollisionType.ShootableBlock => MissileShootableReaction(level, shot, block, roomObjects),

            // A bombable block answers only bombs and Power Bombs, so a missile leaves it as it was: bombable air lets the
            // missile pass and a bombable block stops it.
            RoomCollisionType.BombableAir => false,
            RoomCollisionType.BombableBlock => true,

            // Following the extensions leaves no extension block, so one reaching here is invalid room data.
            RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension or _ =>
                throw new InvalidDataException($"Invalid missile collision type {block.CollisionType} at block {block.Position}."),
        };
    }

    /// <summary>
    /// Whether a shootable block or shootable air stops the missile. A collectible's trigger tells its collectible it was
    /// hit and stops the missile; any other shootable reaction runs its spawn before the block's kind decides: shootable air
    /// lets the missile pass and a shootable block stops it.
    /// </summary>
    private static bool MissileShootableReaction(RoomLevelData level, SamusProjectile shot, RoomCollisionBlock block,
        ISamusRoomObjects roomObjects)
    {
        if (block.CollisionType == RoomCollisionType.ShootableBlock &&
            block.Behavior.Shootable == ShootableReaction.CollectibleTrigger)
        {
            roomObjects.CollectibleObjects.NotifyCollectibleProjectileHit(block.At);
            return true;
        }
        ProjectileTerrainCollision.TrySpawnShootableReaction(level, shot, block, roomObjects);
        return block.CollisionType == RoomCollisionType.ShootableBlock;
    }

    /// <summary>Tests the missile's centre against a slope's mirrored surface, or the square slope's quarter it lies in.</summary>
    private static bool MissileSlopePointReaction(
        RoomCollisionBlock block,
        SamusProjectile shot)
    {
        SlopeShape slopeShape = block.Behavior.Slope.Shape;
        if (!slopeShape.IsSquare)
        {
            // A slope mirrors the projectile's place within the block by the block's horizontal and vertical flips;
            // the shot is in the solid where the slope's surface at its column lies at or above it. A surface height
            // can reach 20 for an overhanging shape, so it is compared whole, never folded into a block's sixteen pixels.
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
