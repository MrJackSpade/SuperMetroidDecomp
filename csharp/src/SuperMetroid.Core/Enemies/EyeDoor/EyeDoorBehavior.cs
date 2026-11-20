using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye doors' projectiles: the shots the eye fires, the sweat it drips and the smoke it leaves.</summary>
[RoomLifetime]
internal sealed class EyeDoorBehavior
{
    /// <summary>Provides the loaded room and random seed for smoke placement.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns the eye door's projectiles, sweat and smoke.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds eye-door effects to a room and its enemy projectile pool.</summary>
    internal EyeDoorBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Spawns the projectile the eye-door room object's animation chooses,
    /// with the room's graphics. The projectile keeps the room object's block and room
    /// because its setup reads both.
    /// </summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request)
    {
        RoomEnemyProjectileKind kind = request.Definition switch
        {
            EyeDoorProjectileDefinition.Sweat =>
                RoomEnemyProjectileKind.EyeDoorSweat,
            EyeDoorProjectileDefinition.ScatteredSmoke or EyeDoorProjectileDefinition.CenteredSmoke =>
                RoomEnemyProjectileKind.EyeDoorSmoke,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(EyeDoorProjectileDefinition)} {request.Definition}."),
        };

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        Position16<RoomBlocks> block = request.RoomObjectBlock;
        switch (request.Definition)
        {
            case EyeDoorProjectileDefinition.Sweat:
                EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Sweat, graphics: default);
                projectile.StartAnimation(new EyeDoorSweatAnimation(EyeDoorSweatSegment.Falling));
                InitializeEyeDoorSweat(projectile, request.Facing, block);
                return;

            case EyeDoorProjectileDefinition.ScatteredSmoke:
                EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, EnemyProjectileBehavior.Still, graphics: default);
                InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.ScatteredSmokeAnimation,
                    EyeDoorEffectDefinitions.ScatteredSmokePlacement, block);
                return;

            case EyeDoorProjectileDefinition.CenteredSmoke:
                EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, EnemyProjectileBehavior.Still, graphics: default);
                InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.CenteredSmokeAnimation,
                    EyeDoorEffectDefinitions.CenteredSmokePlacement, block);
                return;

            default:
                throw new InvalidOperationException(
                    $"Eye-door projectile definition {request.Definition} escaped its setup step.");
        }
    }

    /// <summary>
    /// Spawns the eye's attack with room graphics, placed on the side the eye faces and keeping the eye's door, which
    /// <paramref name="system"/> records opened.
    /// </summary>
    public void SpawnEyeDoorAttack(EyeDoorAttackRequest request, SharedGameState system)
    {
        Ensure.NotNull(system);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.EyeDoorProjectile,
            new ShotBehavior(new EyeDoorShotState(request.Door, system)), graphics: default);
        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Fired));
        InitializeEyeDoorProjectile(projectile, request);
    }

    /// <summary>A sweat drop (#1760): it falls from its spawn and splashes on the floor.</summary>
    internal EnemyProjectileBehavior Sweat => field ??= new SweatBehavior();

    /// <summary>An eye door's shot, holding its heading and the door it came from: it aims at Samus and flies until it hits.</summary>
    /// <param name="shot">What the shot keeps.</param>
    private sealed class ShotBehavior(EyeDoorShotState shot)
        : EnemyProjectileMotionBehavior<EyeDoorProjectileMotion>(initial: null, shot: () => new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Shot)),
            IEnemyProjectileCues<EyeDoorProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, EyeDoorProjectileMotion motion)
        {
            if (motion != EyeDoorProjectileMotion.ShotFlying)
                throw new InvalidOperationException($"An eye door's shot does not move by {nameof(EyeDoorProjectileMotion)} {motion}.");
            FlyEyeDoorProjectile(context.Projectile, shot, context.Level);
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EyeDoorProjectileCue cue)
        {
            if (cue.Kind == EyeDoorProjectileCueKind.AimAtSamus)
                AimEyeDoorProjectileAtSamus(context.Projectile, shot, context.Samus.RoomView);
            else
                ApplyEyeDoorProjectileCue(context.Projectile, cue);
        }
    }

    /// <summary>An eye door's sweat drop, which falls from the door.</summary>
    private sealed class SweatBehavior()
        : EnemyProjectileMotionBehavior<EyeDoorProjectileMotion>(EyeDoorProjectileMotion.SweatFalling), IEnemyProjectileCues<EyeDoorProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, EyeDoorProjectileMotion motion)
        {
            if (motion != EyeDoorProjectileMotion.SweatFalling)
                throw new InvalidOperationException($"An eye door's sweat does not move by {nameof(EyeDoorProjectileMotion)} {motion}.");
            DripEyeDoorSweat(context.Projectile, context.Level);
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EyeDoorProjectileCue cue) => ApplyEyeDoorProjectileCue(context.Projectile, cue);
    }

    /// <summary>
    /// Places the eye's shot on the side the eye faces and retains its door and the game state that records that door's
    /// opening.
    /// </summary>
    private static void InitializeEyeDoorProjectile(
        EnemyProjectile projectile,
        EyeDoorAttackRequest request)
    {
        Offset16<RoomPixels> origin = request.Facing switch
        {
            EyeDoorOrientation.Left => EyeDoorEffectDefinitions.LeftShotOrigin,
            EyeDoorOrientation.Right => EyeDoorEffectDefinitions.RightShotOrigin,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {request.Facing}."),
        };
        projectile.Position = projectile.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + origin);
    }

    /// <summary>Places sweat below the eye block and launches it toward the side the eye faces.</summary>
    private static void InitializeEyeDoorSweat(
        EnemyProjectile projectile,
        EyeDoorOrientation facing,
        Position16<RoomBlocks> block)
    {
        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + EyeDoorEffectDefinitions.SweatOrigin);
        projectile.Velocity = facing switch
        {
            EyeDoorOrientation.Left => EyeDoorEffectDefinitions.LeftSweatLaunch,
            EyeDoorOrientation.Right => EyeDoorEffectDefinitions.RightSweatLaunch,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {facing}."),
        };
    }

    /// <summary>
    /// Starts smoke showing <paramref name="animation"/> scattered across <paramref name="placement"/>: one random number
    /// places the smoke on both axes, then the smoke draws one more random number of its own.
    /// </summary>
    internal void InitializeEyeDoorSmoke(
        EnemyProjectile projectile,
        DustExplosionAnimation animation,
        DustExplosionPlacement placement,
        Position16<RoomBlocks> block)
    {
        int random = _room.Random.Read();
        // Stores only the animation, keeping the definition's one-update timer.
        projectile.InstallAnimation(new DustExplosionProjectileAnimation(animation));

        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + EyeDoorEffectDefinitions.SmokeCentre + new Offset16<RoomPixels>(
            placement.Base.X + placement.XScatter.Of(random),
            placement.Base.Y + placement.YScatter.Of(random)));

        // The smoke draws the next random number once both axes have read the current one.
        _room.Random.Generate();
    }

    /// <summary>Moves the eye shot, accelerates along its angle, and switches to impact on collision or door opening.</summary>
    internal static void FlyEyeDoorProjectile(
        EnemyProjectile projectile,
        EyeDoorShotState shot,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Horizontal, EyeDoorEffectCollision.Rule) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, EyeDoorEffectCollision.Rule))
        {
            StartEyeDoorProjectileImpact(projectile);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X + ReadEyeDoorAcceleration(shot.Heading + TrigonometryAngle.QuarterTurn));
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y +
            ReadEyeDoorAcceleration(shot.Heading + TrigonometryAngle.HalfTurn));

        if (shot.Doors.HasOpenedDoor(shot.Door))
            StartEyeDoorProjectileImpact(projectile);
    }

    /// <summary>Applies sweat gravity and starts the impact animation on a downward vertical collision.</summary>
    internal static void DripEyeDoorSweat(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Horizontal, EyeDoorEffectCollision.Rule);
        bool collidedVertically = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, EyeDoorEffectCollision.Rule);
        if (!projectile.Velocity.Y.IsNegative && collidedVertically)
        {
            projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole - EyeDoorEffectDefinitions.SweatSplashRise);
            // The floor splash stops the drop's movement, as the animation switch does.
            projectile.HoldStill();
            projectile.StartAnimation(new EyeDoorSweatAnimation(EyeDoorSweatSegment.HitFloor));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + EyeDoorEffectDefinitions.SweatGravity);
    }

    /// <summary>
    /// The eye shot's per-update acceleration along <paramref name="angle"/>'s sine: a sixteenth of the one-pixel sine
    /// velocity, rounded toward decreasing coordinates, so a small negative sine still accelerates the shot back.
    /// </summary>
    internal static SignedEightEight ReadEyeDoorAcceleration(TrigonometryAngle angle) =>
        EnemyMath.SineVelocity(angle, 1).DividedRoundingDown(16);

    /// <summary>Sends an eye-door shot into its typed impact animation, stopping its movement as the animation switch does.</summary>
    private static void StartEyeDoorProjectileImpact(EnemyProjectile projectile)
    {
        projectile.HoldStill();
        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Impact));
    }

    /// <summary>Carries out one cue of the eye-door shot's animations.</summary>
    private static void ApplyEyeDoorProjectileCue(EnemyProjectile projectile, EyeDoorProjectileCue cue)
    {
        switch (cue.Kind)
        {
            // Only a shot aims (ShotBehavior); its sweat's and its shot's other cues act alike.
            case EyeDoorProjectileCueKind.AimAtSamus:
                throw new InvalidDataException($"An eye door's sweat reached the shot's aim cue, which only a shot runs.");
            case EyeDoorProjectileCueKind.StartFlight:
                projectile.StartMotion(EyeDoorProjectileMotion.ShotFlying);
                break;
            case EyeDoorProjectileCueKind.StopMoving:
                projectile.HoldStill();
                break;
            case EyeDoorProjectileCueKind.Delete:
                projectile.Delete();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>
    /// Aims the projectile at Samus: keeps the angle to
    /// Samus and launches the shot along it at unit speed.
    /// </summary>
    private static void AimEyeDoorProjectileAtSamus(EnemyProjectile projectile, EyeDoorShotState shot, IRoomSamus samus)
    {
        shot.Heading = EnemyMath.CalculateAimAngle(
            new(samus.Position.X.Whole - projectile.Position.X.Whole, samus.Position.Y.Whole - projectile.Position.Y.Whole));
        projectile.Velocity = projectile.Velocity.WithX(EnemyMath.SineVelocity(shot.Heading + TrigonometryAngle.QuarterTurn, 1));
        projectile.Velocity = projectile.Velocity.WithY(EnemyMath.SineVelocity(shot.Heading + TrigonometryAngle.HalfTurn, 1));
    }
}
