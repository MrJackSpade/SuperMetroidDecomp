using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The falling sparks: each drops under gravity, bounces on the floor and leaves a trail.</summary>
[RoomLifetime]
internal sealed class FallingSparksBehavior
{
    /// <summary>Room RNG used to choose the spark's horizontal launch delta.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates sparks and applies their projectile definition and motion.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Allocates the periodic effect-actor trail.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>How far a falling spark's floor probe creeps it down before it falls.</summary>
    private readonly FallingSparkFloorProbe _floorProbe;

    /// <summary>Binds room RNG, projectile allocation, effect-actor trail ownership and the floor probe's creep.</summary>
    internal FallingSparksBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEffectActors effectActors,
        FallingSparkFloorProbe floorProbe)
    {
        _room = room;
        _projectiles = projectiles;
        _effectActors = effectActors;
        _floorProbe = floorProbe;
    }

    /// <summary>Allocates and initializes a falling spark projectile.</summary>
    internal void SpawnFallingSpark(RoomEnemyPlace source)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // The spark starts as its definition has it: blank at first, with its animation, size, damage and the way it
        // touches Samus. It then appears below its source, still, and drifts sideways by a random launch.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FallingSpark,
            Spark,
            SpriteGraphics.SpawnedBy(source));
        projectile.StartAnimation(new FallingSparkAnimation(FallingSparkSegment.Falling));
        projectile.Position = source.Position.WithWholeY(source.Position.Y.Whole + FallingSparkMotionDefinitions.SpawnBelowSource);

        // The setup zeros both ordinary velocities, then takes a random number. The
        // authored launches are one short of the eight selections; the last selection drifts by
        // FallingSparkUnauthoredLaunch.
        projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
        Func<int> nextRandom = _room.Random.Generate;
        SignedSixteenSixteen launch = FallingSparkLaunchDefinitions.Launch.Choose(nextRandom())
            .DriftWith(() => _room.Session.Overreads.FallingSparkUnauthoredLaunch);
        Spark.States.Start(projectile, new FallingSparkState(launch));
    }


    /// <summary>Moves the falling spark one update.</summary>
    internal void FallFallingSpark(
        EnemyProjectile projectile,
        RoomLevelData level,
        int frameCycle)
    {
        // A falling spark first looks for the floor: the probe creeps it down (FallingSparkFloorProbe) through the
        // enemy projectiles' block collision, slope reactions included, and the creep stays on top of the spark's own
        // fall below.
        FallingSparkState spark = Spark.States.Of(projectile);
        if (!spark.VerticalSpeed.IsNegative)
        {
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical, _floorProbe.CreepFor(spark.VerticalSpeed)))
            {
                BeginFallingSparkFloorImpact(projectile);
                return;
            }

            // Gravity adds a quarter pixel to the speed's fraction; a fraction that passes a whole pixel raises the
            // whole speed by one.
            SignedSixteenSixteen accelerated = spark.VerticalSpeed + FallingSparkMotionDefinitions.Gravity;

            // The whole speed stops rising once it would reach four. The fraction still keeps its quarter-pixel
            // increment, giving the slightly asymmetric cap.
            spark.VerticalSpeed = accelerated.WholePixels < FallingSparkMotionDefinitions.TerminalWholeSpeed
                ? accelerated
                : accelerated.WithWholePixels(spark.VerticalSpeed.WholePixels);
        }

        AddFallingSparkVerticalVelocity(projectile);
        AddFallingSparkHorizontalVelocity(projectile);

        if (FallingSparkCadenceDefinitions.TrailEveryFourthUpdate.IsDue(frameCycle))
            SpawnFallingSparkTrail(projectile);
    }

    /// <summary>Adds the spark's vertical speed to its position, the fraction's overflow passing into the whole.</summary>
    private void AddFallingSparkVerticalVelocity(
        EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceY(Spark.States.Of(projectile).VerticalSpeed);

    /// <summary>Adds the spark's horizontal drift, the fraction's overflow passing into the whole.</summary>
    internal void AddFallingSparkHorizontalVelocity(
        EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceX(Spark.States.Of(projectile).HorizontalStep);

    /// <summary>
    /// Switches a falling spark to its floor-impact animation and scales its stored
    /// horizontal delta, the fraction's overflow passing into the whole.
    /// </summary>
    internal void BeginFallingSparkFloorImpact(EnemyProjectile projectile)
    {
        projectile.StartAnimation(new FallingSparkAnimation(FallingSparkSegment.HitFloor));

        // The impact quadruples the sideways drift, wrapping round past the largest drift: the unauthored launch the last
        // random selection reads can be any drift, so its quadruple can pass the largest the drift holds.
        FallingSparkState spark = Spark.States.Of(projectile);
        spark.HorizontalStep *= FallingSparkMotionDefinitions.ImpactDriftScale;

        // While the bounce carries the spark upward, later updates skip collision and gravity as the
        // impact spark arcs through its eleven-frame blinking animation.
        spark.VerticalSpeed = FallingSparkMotionDefinitions.ImpactBounce;
        projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole - FallingSparkMotionDefinitions.ImpactLift);
    }

    /// <summary>Spawns a trail object at the spark's current whole-pixel position.</summary>
    private void SpawnFallingSparkTrail(EnemyProjectile projectile)
    {
        _ = _effectActors.SpawnRoomEffectActor(
            projectile.Position.Whole,
            RoomEffectActorKind.FallingSparkTrail,
            projectile.Graphics);
    }

    /// <summary>A falling spark (#1760): it drops under gravity, bouncing on the floor and leaving a trail.</summary>
    internal StatefulSingleMotionProjectileBehavior<FallingSparkState> Spark => field ??= new(
        context => FallFallingSpark(context.Projectile, context.Level, context.ShortCounter));
}
