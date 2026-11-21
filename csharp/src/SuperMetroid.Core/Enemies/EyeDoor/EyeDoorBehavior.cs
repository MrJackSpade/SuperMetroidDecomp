using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye doors' projectiles: the shots the eye fires, the sweat it drips and the smoke it leaves.</summary>
[RoomLifetime]
internal sealed partial class EyeDoorBehavior
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

    /// <summary>What <see cref="SpawnEyeDoorProjectile"/> does for each <see cref="EyeDoorProjectileDefinition"/> member, each member's own arm.</summary>
    private IdentityTable<EyeDoorProjectileDefinition, Action<EyeDoorProjectileRequest, RoomEnemyProjectileKind, EnemyProjectile, Position16<RoomBlocks>>> SpawnEyeDoorProjectileForEyeDoorProjectileDefinition => field ??= new(new Dictionary<EyeDoorProjectileDefinition, Action<EyeDoorProjectileRequest, RoomEnemyProjectileKind, EnemyProjectile, Position16<RoomBlocks>>>
    {
        [EyeDoorProjectileDefinition.Sweat] = (request, kind, projectile, block) =>
            InitializeEyeDoorSweat(projectile, request.Facing, block),
        [EyeDoorProjectileDefinition.ScatteredSmoke] = (request, kind, projectile, block) =>
            InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.ScatteredSmokePlacement, block),
        [EyeDoorProjectileDefinition.CenteredSmoke] = (request, kind, projectile, block) =>
            InitializeEyeDoorSmoke(projectile, EyeDoorEffectDefinitions.CenteredSmokePlacement, block),
    });

    /// <summary>The animation each <see cref="EyeDoorProjectileDefinition"/>'s projectile spawns playing (#2553).</summary>
    private IdentityTable<EyeDoorProjectileDefinition, Func<EnemyProjectileAnimation>> EyeDoorProjectileAnimations => field ??= new(new Dictionary<EyeDoorProjectileDefinition, Func<EnemyProjectileAnimation>>
    {
        [EyeDoorProjectileDefinition.Sweat] = () => new EyeDoorSweatAnimation(Sweat, EyeDoorSweatSegment.Falling),
        // A smoke keeps the definition's one-update timer.
        [EyeDoorProjectileDefinition.ScatteredSmoke] = static () => new DustExplosionProjectileAnimation(EyeDoorEffectDefinitions.ScatteredSmokeAnimation),
        [EyeDoorProjectileDefinition.CenteredSmoke] = static () => new DustExplosionProjectileAnimation(EyeDoorEffectDefinitions.CenteredSmokeAnimation),
    });

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

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind,
            request.Definition == EyeDoorProjectileDefinition.Sweat ? Sweat : EnemyProjectileBehavior.Still, graphics: SpriteGraphics.SharedStandard,
            EyeDoorProjectileAnimations[request.Definition]());
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        Position16<RoomBlocks> block = request.RoomObjectBlock;
        SpawnEyeDoorProjectileForEyeDoorProjectileDefinition[request.Definition](request, kind, projectile, block);
    }

    /// <summary>
    /// Spawns the eye's attack with room graphics, placed on the side the eye faces and keeping the eye's door, which
    /// <paramref name="system"/> records opened.
    /// </summary>
    public void SpawnEyeDoorAttack(EyeDoorAttackRequest request, SharedGameState system)
    {
        Ensure.NotNull(system);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.EyeDoorProjectile, Shot, graphics: SpriteGraphics.SharedStandard, new EyeDoorProjectileAnimation(Shot, EyeDoorProjectileSegment.Fired));
        if (projectile is null)
            return; // Spawning fails when the room has no place left for an enemy projectile.

        InitializeEyeDoorProjectile(Shot, projectile, request, system);
    }

    /// <summary>
    /// The eye's shot (#1760): it flies until it strikes the room or its door opens; a shot one runs its typed shot
    /// animation, the animation its definition names.
    /// </summary>
    internal EyeDoorProjectileBehavior Shot => field ??= new EyeDoorProjectileBehavior(
        initial: null, shot: EyeDoorProjectileSegment.Shot);

    /// <summary>A sweat drop (#1760): it falls from its spawn and splashes on the floor.</summary>
    internal EyeDoorProjectileBehavior Sweat => field ??= new EyeDoorProjectileBehavior(EyeDoorProjectileMotion.SweatFalling);

    /// <summary>
    /// An eye door's shot or sweat: the shot's flight, which its animation starts, the sweat's fall, which its spawn starts,
    /// and their animations' cues.
    /// </summary>
    internal sealed partial class EyeDoorProjectileBehavior
        : EnemyProjectileMotionBehavior<EyeDoorProjectileMotion>, IEnemyProjectileCues<EyeDoorProjectileCue>
    {
        /// <summary>Where the animation a destroying shot starts begins, or null for the shared delete animation.</summary>
        private readonly EyeDoorProjectileSegment? shot;

        /// <summary>Creates an <see cref="EyeDoorProjectileBehavior"/>.</summary>
        /// <param name="initial">The motion the projectile starts with: the sweat's fall, or none for the shot.</param>
        /// <param name="shot">Where the animation a destroying shot starts begins, or null for the shared delete animation.</param>
        public EyeDoorProjectileBehavior(EyeDoorProjectileMotion? initial, EyeDoorProjectileSegment? shot = null)
            : base(initial)
        {
            this.shot = shot;
        }

        /// <summary>Each shot's door, the game state that records its opening and its heading, given as its eye fires it.</summary>
        internal EnemyProjectileStates<EyeDoorShotState> Shots { get; } = new();

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() =>
            shot is { } segment ? new EyeDoorProjectileAnimation(this, segment) : base.ShotAnimation();

        /// <summary>What <c>Move</c> does for each <see cref="EyeDoorProjectileMotion"/> member, each member's own arm.</summary>
        private IdentityTable<EyeDoorProjectileMotion, Action<EnemyProjectileMotionContext>> MoveForEyeDoorProjectileMotion => field ??= new(new Dictionary<EyeDoorProjectileMotion, Action<EnemyProjectileMotionContext>>
        {
            [EyeDoorProjectileMotion.ShotFlying] = context => FlyEyeDoorProjectile(this, context.Projectile, context.Level),
            [EyeDoorProjectileMotion.SweatFalling] = context => DripEyeDoorSweat(this, context.Projectile, context.Level),
        });

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, EyeDoorProjectileMotion motion)
        {
            MoveForEyeDoorProjectileMotion[motion](context);
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EyeDoorProjectileCue cue) =>
            ApplyEyeDoorProjectileCue(this, context.Projectile, context.Samus.RoomView, cue);
    }

    /// <summary>
    /// Places the eye's shot on the side the eye faces and retains its door and the game state that records that door's
    /// opening.
    /// </summary>
    private static void InitializeEyeDoorProjectile(
        EyeDoorProjectileBehavior owner,
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
        owner.Shots.Start(projectile, new EyeDoorShotState(request.Door, doors));
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
    /// Places smoke at <paramref name="block"/>, scattered across <paramref name="placement"/>: one random number places
    /// the smoke on both axes, then the smoke draws one more random number of its own.
    /// </summary>
    internal void InitializeEyeDoorSmoke(
        EnemyProjectile projectile,
        DustExplosionPlacement placement,
        Position16<RoomBlocks> block)
    {
        int random = _room.Random.Read();

        projectile.Position = projectile.Position.WithWhole(block.ToRoomPixels() + EyeDoorEffectDefinitions.SmokeCentre + new Offset16<RoomPixels>(
            placement.Base.X + placement.XScatter.Of(random),
            placement.Base.Y + placement.YScatter.Of(random)));

        // The smoke draws the next random number once both axes have read the current one.
        _room.Random.Generate();
    }

    /// <summary>Moves the eye shot, accelerates along its angle, and switches to impact on collision or door opening.</summary>
    internal static void FlyEyeDoorProjectile(
        EyeDoorProjectileBehavior owner,
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Horizontal, EyeDoorEffectCollision.Rule) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, EyeDoorEffectCollision.Rule))
        {
            StartEyeDoorProjectileImpact(owner, projectile);
            return;
        }

        EyeDoorShotState shot = owner.Shots.Of(projectile);
        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X + ReadEyeDoorAcceleration(shot.Heading + TrigonometryAngle.QuarterTurn));
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y +
            ReadEyeDoorAcceleration(shot.Heading + TrigonometryAngle.HalfTurn));

        if (shot.Doors.HasOpenedDoor(shot.Door))
            StartEyeDoorProjectileImpact(owner, projectile);
    }

    /// <summary>Applies sweat gravity and starts the impact animation on a downward vertical collision.</summary>
    internal static void DripEyeDoorSweat(
        EyeDoorProjectileBehavior owner,
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
            projectile.StartAnimation(new EyeDoorSweatAnimation(owner, EyeDoorSweatSegment.HitFloor));
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
    private static void StartEyeDoorProjectileImpact(EyeDoorProjectileBehavior owner, EnemyProjectile projectile)
    {
        projectile.HoldStill();
        projectile.StartAnimation(new EyeDoorProjectileAnimation(owner, EyeDoorProjectileSegment.Impact));
    }

    /// <summary>What <see cref="ApplyEyeDoorProjectileCue"/> does for each <see cref="EyeDoorProjectileCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<EyeDoorProjectileCueKind, Action<EyeDoorProjectileBehavior, EnemyProjectile, IRoomSamus>> ApplyEyeDoorProjectileCueForEyeDoorProjectileCueKind => field ??= new(new Dictionary<EyeDoorProjectileCueKind, Action<EyeDoorProjectileBehavior, EnemyProjectile, IRoomSamus>>
    {
        [EyeDoorProjectileCueKind.AimAtSamus] = static (owner, projectile, samus) => AimEyeDoorProjectileAtSamus(owner, projectile, samus),
        [EyeDoorProjectileCueKind.StartFlight] = static (owner, projectile, samus) => projectile.StartMotion(owner, EyeDoorProjectileMotion.ShotFlying),
        [EyeDoorProjectileCueKind.StopMoving] = static (owner, projectile, samus) => projectile.HoldStill(),
        [EyeDoorProjectileCueKind.Delete] = static (owner, projectile, samus) => projectile.Delete(),
    });

    /// <summary>Carries out one cue of the eye-door shot's animations.</summary>
    private static void ApplyEyeDoorProjectileCue(EyeDoorProjectileBehavior owner, EnemyProjectile projectile, IRoomSamus samus, EyeDoorProjectileCue cue)
    {
        ApplyEyeDoorProjectileCueForEyeDoorProjectileCueKind[cue.Kind](owner, projectile, samus);
    }

    /// <summary>
    /// Aims the projectile at Samus: keeps the angle to
    /// Samus and launches the shot along it at unit speed.
    /// </summary>
    private static void AimEyeDoorProjectileAtSamus(EyeDoorProjectileBehavior owner, EnemyProjectile projectile, IRoomSamus samus)
    {
        EyeDoorShotState shot = owner.Shots.Of(projectile);
        shot.Heading = EnemyMath.CalculateAimAngle(
            new(samus.Position.X.Whole - projectile.Position.X.Whole, samus.Position.Y.Whole - projectile.Position.Y.Whole));
        projectile.Velocity = projectile.Velocity.WithX(EnemyMath.SineVelocity(shot.Heading + TrigonometryAngle.QuarterTurn, 1));
        projectile.Velocity = projectile.Velocity.WithY(EnemyMath.SineVelocity(shot.Heading + TrigonometryAngle.HalfTurn, 1));
    }
}
