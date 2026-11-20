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

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind, EnemyProjectileBehavior.Still, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        Position16<RoomBlocks> block = request.RoomObjectBlock;
        switch (request.Definition)
        {
            case EyeDoorProjectileDefinition.Sweat:
                projectile.StartAnimation(new EyeDoorSweatAnimation(EyeDoorSweatSegment.Falling, SweatDrop));
                InitializeEyeDoorSweat(projectile, request.Facing, block);
                return;

            case EyeDoorProjectileDefinition.ScatteredSmoke:
                InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.ScatteredSmokeAnimation,
                    EyeDoorEffectDefinitions.ScatteredSmokePlacement, block);
                return;

            case EyeDoorProjectileDefinition.CenteredSmoke:
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
        var shot = new ShotBehavior(new EyeDoorShotState(request.Door, system));
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.EyeDoorProjectile, shot, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Fired, shot));
        InitializeEyeDoorProjectile(projectile, request);
    }

    /// <summary>A sweat drop (#1760): it falls from its spawn and splashes on the floor.</summary>
    internal EnemyProjectileBehavior Sweat => SweatDrop;

    /// <summary>The sweat drop's behaviour, which owns its animations' cues.</summary>
    private SweatBehavior SweatDrop => field ??= new SweatBehavior();

    /// <summary>An eye door's shot, holding its heading and the door it came from: it aims at Samus and flies until it hits.</summary>
    /// <param name="shot">What the shot keeps.</param>
    internal sealed class ShotBehavior(EyeDoorShotState shot)
        : EnemyProjectileMotionBehavior<SingleMotion>(initial: null), IEnemyProjectileCues<EyeDoorProjectileCue>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Shot, this);

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            FlyEyeDoorProjectile(context.Projectile, shot, context.Level, this);

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EyeDoorProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue.Kind)
            {
                case EyeDoorProjectileCueKind.AimAtSamus:
                    AimEyeDoorProjectileAtSamus(projectile, shot, context.Samus.RoomView);
                    break;
                case EyeDoorProjectileCueKind.StartFlight:
                    projectile.StartMotion(this, SingleMotion.Moving);
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
    }

    /// <summary>An eye door's sweat drop, which falls from the door and splashes on the floor.</summary>
    private sealed class SweatBehavior()
        : EnemyProjectileMotionBehavior<SingleMotion>(SingleMotion.Moving), IEnemyProjectileCues<EyeDoorSweatCue>
    {
        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            DripEyeDoorSweat(context.Projectile, context.Level, this);

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EyeDoorSweatCue cue)
        {
            switch (cue)
            {
                case EyeDoorSweatCue.StopMoving:
                    context.Projectile.HoldStill();
                    break;
                case EyeDoorSweatCue.Delete:
                    context.Projectile.Delete();
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(EyeDoorSweatCue)} {cue}.");
            }
        }
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
        RoomLevelData level,
        IEnemyProjectileCues<EyeDoorProjectileCue> cues)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Horizontal, EyeDoorEffectCollision.Rule) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, EyeDoorEffectCollision.Rule))
        {
            StartEyeDoorProjectileImpact(projectile, cues);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X + ReadEyeDoorAcceleration(shot.Heading + TrigonometryAngle.QuarterTurn));
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y +
            ReadEyeDoorAcceleration(shot.Heading + TrigonometryAngle.HalfTurn));

        if (shot.Doors.HasOpenedDoor(shot.Door))
            StartEyeDoorProjectileImpact(projectile, cues);
    }

    /// <summary>Applies sweat gravity and starts the impact animation on a downward vertical collision.</summary>
    internal static void DripEyeDoorSweat(
        EnemyProjectile projectile,
        RoomLevelData level,
        IEnemyProjectileCues<EyeDoorSweatCue> sweat)
    {
        EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Horizontal, EyeDoorEffectCollision.Rule);
        bool collidedVertically = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, EyeDoorEffectCollision.Rule);
        if (!projectile.Velocity.Y.IsNegative && collidedVertically)
        {
            projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole - EyeDoorEffectDefinitions.SweatSplashRise);
            // The floor splash stops the drop's movement, as the animation switch does.
            projectile.HoldStill();
            projectile.StartAnimation(new EyeDoorSweatAnimation(EyeDoorSweatSegment.HitFloor, sweat));
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
    private static void StartEyeDoorProjectileImpact(EnemyProjectile projectile, IEnemyProjectileCues<EyeDoorProjectileCue> cues)
    {
        projectile.HoldStill();
        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Impact, cues));
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
