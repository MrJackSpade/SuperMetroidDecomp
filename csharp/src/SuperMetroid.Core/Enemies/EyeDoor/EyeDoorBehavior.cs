using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye door species owner: its per-room state, behaviour and the interactions it registers.</summary>
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
    /// Replays spawning a projectile with room graphics and the definition-specific initializer selected
    /// by the eye-door room object's animation. The room object's block and room argument are retained
    /// because the game initializers read both directly from their spawning room object.
    /// </summary>
    public void SpawnEyeDoorProjectile(
        EyeDoorProjectileRequest request,
        SharedGameState system)
    {
        Ensure.NotNull(system);

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
                    $"Eye-door projectile definition {request.Definition} escaped its initializer step.");
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

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.EyeDoorProjectile, Shot, graphics: default);
        projectile.StartAnimation(new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Initial));
        InitializeEyeDoorProjectile(projectile, request, system);
    }

    /// <summary>
    /// The eye's shot (#1760): it flies until it strikes the room or its door opens; a shot one runs its typed shot
    /// animation, the animation its definition names.
    /// </summary>
    internal EnemyProjectileBehavior Shot => field ??= new EyeDoorProjectileBehavior(
        initial: null, shot: () => new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Shot));

    /// <summary>A sweat drop (#1760): it falls from its spawn and splashes on the floor.</summary>
    internal EnemyProjectileBehavior Sweat => field ??= new EyeDoorProjectileBehavior(EyeDoorProjectileMotion.SweatFalling);

    /// <summary>
    /// An eye door's shot or sweat: the shot's flight, which its animation starts, the sweat's fall, which its spawn starts,
    /// and their animations' cues.
    /// </summary>
    /// <param name="initial">The motion the projectile starts with: the sweat's fall, or none for the shot.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    private sealed class EyeDoorProjectileBehavior(EyeDoorProjectileMotion? initial, Func<EnemyProjectileAnimation>? shot = null)
        : EnemyProjectileMotionBehavior<EyeDoorProjectileMotion>(initial, shot), IEnemyProjectileCues<EyeDoorProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, EyeDoorProjectileMotion motion)
        {
            switch (motion)
            {
                case EyeDoorProjectileMotion.ShotFlying:
                    FlyEyeDoorProjectile(context.Projectile, context.Level);
                    break;
                case EyeDoorProjectileMotion.SweatFalling:
                    DripEyeDoorSweat(context.Projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(EyeDoorProjectileMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EyeDoorProjectileCue cue) =>
            ApplyEyeDoorProjectileCue(context.Projectile, context.Samus.RoomView, cue);
    }

    /// <summary>
    /// Places the eye's shot on the side the eye faces and retains its door and the game state that records that door's
    /// opening.
    /// </summary>
    private static void InitializeEyeDoorProjectile(
        EnemyProjectile projectile,
        EyeDoorAttackRequest request,
        SharedGameState doors)
    {
        Offset16<RoomPixels> origin = request.Facing switch
        {
            EyeDoorOrientation.Left => EyeDoorEffectDefinitions.LeftShotOrigin,
            EyeDoorOrientation.Right => EyeDoorEffectDefinitions.RightShotOrigin,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {request.Facing}."),
        };
        projectile.State = new EyeDoorShotState(request.Door, doors);
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
    /// Starts smoke showing <paramref name="animation"/> scattered across <paramref name="placement"/>: uses one shared
    /// random sample for both smoke offsets, then advances the seed.
    /// </summary>
    internal void InitializeEyeDoorSmoke(
        EnemyProjectile projectile,
        MiscDustAnimation animation,
        MiscDustPlacementDefinition placement,
        Position16<RoomBlocks> block)
    {
        int random = _room.Random.Read();
        // Stores only the animation, keeping the definition's one-update timer.
        projectile.InstallAnimation(new MiscDustProjectileAnimation(animation));

        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + EyeDoorEffectDefinitions.SmokeCentre + new Offset16<RoomPixels>(
            placement.Base.X + placement.XScatter.Of(random),
            placement.Base.Y + placement.YScatter.Of(random)));

        // The spawn advances the global seed after sampling it for both coordinates.
        _room.Random.Generate();
    }

    /// <summary>Moves the eye shot, accelerates along its angle, and switches to impact on collision or door opening.</summary>
    internal static void FlyEyeDoorProjectile(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Horizontal, EyeDoorEffectCollision.Rule) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, EyeDoorEffectCollision.Rule))
        {
            StartEyeDoorProjectileImpact(projectile);
            return;
        }

        EyeDoorShotState shot = projectile.StateOf<EyeDoorShotState>();
        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X + ReadEyeDoorAcceleration(shot.Heading));
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y +
            ReadEyeDoorAcceleration(shot.Heading - TrigonometryAngle.QuarterTurn));

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
    private static void ApplyEyeDoorProjectileCue(EnemyProjectile projectile, IRoomSamus samus, EyeDoorProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case EyeDoorProjectileCueKind.AimAtSamus:
                AimEyeDoorProjectileAtSamus(projectile, samus);
                break;
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
    private static void AimEyeDoorProjectileAtSamus(EnemyProjectile projectile, IRoomSamus samus)
    {
        EyeDoorShotState shot = projectile.StateOf<EyeDoorShotState>();
        shot.Heading = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, projectile.Position.Y.Whole))).ToMirroredTrigonometryAngle();
        projectile.Velocity = projectile.Velocity.WithX(EnemyMath.SineVelocity(shot.Heading, 1));
        projectile.Velocity = projectile.Velocity.WithY(EnemyMath.SineVelocity(shot.Heading - TrigonometryAngle.QuarterTurn, 1));
    }
}
