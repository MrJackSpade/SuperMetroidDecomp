using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The falling sparks species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FallingSparksBehavior
{
    /// <summary>Room RNG used to choose the spark's horizontal launch delta.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates sparks and applies their projectile definition and motion.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Allocates the periodic sprite-object trail.</summary>
    private readonly RoomSpriteObjects _spriteObjects;

    /// <summary>Binds room RNG, projectile allocation, and sprite-object trail ownership.</summary>
    internal FallingSparksBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomSpriteObjects spriteObjects)
    {
        _room = room;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Allocates and initializes a falling spark projectile.</summary>
    internal void SpawnFallingSpark(RoomEnemySlot source)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // The common spawn first copies the spark's definition, including its blank
        // startup map, frame behaviour, animation, radii, property flags, damage,
        // and interaction behavior. The spark's initializer then owns only the source position,
        // shared motion values, and RNG-selected horizontal delta below.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FallingSpark,
            Spark,
            SpriteGraphics.SpawnedBy(source));
        projectile.StartAnimation(new FallingSparkAnimation(FallingSparkSegment.Falling));
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(CoordinateAxis.Wrap(source.Position.Y.Whole + 8), source.Position.Y.Fraction));

        // The initializer zeros both ordinary velocities, then takes a random number. The
        // authored launches are one short of the eight selections; the last selection drifts by
        // FallingSparkUnauthoredLaunch.
        projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.Zero);
        Func<int> nextRandom = _room.Random.Generate;
        SignedSixteenSixteen launch = FallingSparkLaunches.FromRandom(nextRandom(), () => _room.Overreads.FallingSparkUnauthoredLaunch);
        projectile.State = new FallingSparkState(launch);
    }

    /// <summary>How many times faster a spark drifts sideways once it has hit the floor.</summary>
    private const int FallingSparkImpactDriftScale = 4;

    /// <summary>The fraction of a pixel a falling spark's vertical speed gains each falling update: a quarter pixel.</summary>
    private static readonly SignedSixteenSixteen FallingSparkGravity = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The whole-pixel vertical speed a falling spark's speed stops rising at.</summary>
    private const int FallingSparkTerminalWholeSpeed = 4;

    /// <summary>The vertical speed a spark bounces up at when it hits the floor: half a pixel upward.</summary>
    private static readonly SignedSixteenSixteen FallingSparkImpactBounce = -SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>Runs the falling spark's per-frame behaviour.</summary>
    internal void RunFallingSparkFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level,
        int frameCycle)
    {
        // A falling spark first moves through the generic collision helper. The helper's accepted movement is
        // *additional* to the spark's own 1/65536-pixel movement below; omitting it shifts the floor-contact frame.
        FallingSparkState spark = projectile.StateOf<FallingSparkState>();
        if (spark.VerticalSpeed.WholePixels >= 0)
        {
            // The enemy-projectile vertical mover, slope reactions included, reads the spark's whole speed in
            // pixels, zero or more on this branch, as that many 256ths of a pixel an update.
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical,
                    SignedEightEight.FromTwoHundredFiftySixthsOfPixel(spark.VerticalSpeed.WholePixels)))
            {
                BeginFallingSparkFloorImpact(projectile);
                return;
            }

            // Gravity adds a quarter pixel to the speed's fraction; a fraction that passes a whole pixel raises the
            // whole speed by one.
            SignedSixteenSixteen accelerated = spark.VerticalSpeed + FallingSparkGravity;

            // The whole speed stops rising once it would reach four. The fraction still keeps its quarter-pixel
            // increment, giving the slightly asymmetric cap.
            spark.VerticalSpeed = accelerated.WholePixels < FallingSparkTerminalWholeSpeed
                ? accelerated
                : accelerated.WithWholePixels(spark.VerticalSpeed.WholePixels);
        }

        AddFallingSparkVerticalVelocity(projectile);
        AddFallingSparkHorizontalVelocity(projectile);

        if (FallingSparkCadenceDefinitions.TrailEveryFourthUpdate.IsDue(frameCycle))
            SpawnFallingSparkTrail(projectile);
    }

    /// <summary>Adds the spark's vertical speed to its position, the fraction's overflow passing into the whole.</summary>
    private static void AddFallingSparkVerticalVelocity(
        EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceY(projectile.StateOf<FallingSparkState>().VerticalSpeed);

    /// <summary>Adds the spark's horizontal drift, the fraction's overflow passing into the whole.</summary>
    internal static void AddFallingSparkHorizontalVelocity(
        EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.AdvanceX(projectile.StateOf<FallingSparkState>().HorizontalStep);

    /// <summary>
    /// Switches a falling spark to its floor-impact animation and scales its stored
    /// horizontal delta by four, the fraction's overflow passing into the whole.
    /// </summary>
    internal static void BeginFallingSparkFloorImpact(EnemyProjectile projectile)
    {
        projectile.StartAnimation(new FallingSparkAnimation(FallingSparkSegment.HitFloor));

        // The impact quadruples the sideways drift, wrapping round past the largest drift: the unauthored launch the last
        // random selection reads can be any drift, so its quadruple can pass the largest the drift holds.
        FallingSparkState spark = projectile.StateOf<FallingSparkState>();
        spark.HorizontalStep *= FallingSparkImpactDriftScale;

        // Half a pixel upward. Because the whole part is negative, later frames skip collision and gravity while the
        // impact spark arcs upward through its eleven-frame blinking animation.
        spark.VerticalSpeed = FallingSparkImpactBounce;
        projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole - 2);
    }

    /// <summary>Spawns a trail object at the spark's current whole-pixel position.</summary>
    private void SpawnFallingSparkTrail(EnemyProjectile projectile)
    {
        _ = _spriteObjects.SpawnRoomSpriteObject(
            projectile.Position.Whole,
            RoomSpriteObjectKind.FallingSparkTrail,
            projectile.Graphics);
    }

    /// <summary>A falling spark (#1760): it drops under gravity, bouncing on the floor and leaving a trail.</summary>
    internal SingleMotionProjectileBehavior Spark => field ??= new(
        EnemyProjectileFrameBehavior.FallingSparkBounce,
        context => RunFallingSparkFrameBehavior(context.Projectile, context.Level, context.ShortCounter));
}
