using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The falling sparks species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FallingSparksBehavior
{
    /// <summary>Room RNG used to choose the spark's horizontal launch delta.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates sparks and applies their projectile definition and motion.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Allocates the periodic sprite-object trail.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;

    /// <summary>Binds room RNG, projectile allocation, and sprite-object trail ownership.</summary>
    internal FallingSparksBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects)
    {
        _room = room;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Allocates and initializes enemy projectile <c>$86:F498</c>.</summary>
    internal void SpawnFallingSpark(RoomEnemySlot source)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // The common spawn routine first copies definition $F498, including its blank
        // startup map, $F3F0 pre-instruction, $F353 list, radii, property flags, damage,
        // and interaction behavior. Initializer $F391 then owns only the source position,
        // aliased motion words, and RNG-selected horizontal delta below.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FallingSpark,
            source.Graphics);
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(unchecked((ushort)(source.Position.Y.Whole + 8)), source.Position.Y.Fraction));

        // `$86:F391` zeros both ordinary velocity words, then calls the shared RNG. The
        // horizontal table is one record short. The compiled definition retains the
        // final RNG outcome's instruction-byte overread, including its signed delta.
        projectile.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        Func<int> nextRandom = _room.Random.Generate;
        SignedSixteenSixteen launch = FallingSparkLaunches.FromRandom(nextRandom());
        projectile.State = new FallingSparkState(launch);
    }

    /// <summary>How many times faster a spark drifts sideways once it has hit the floor.</summary>
    private const int FallingSparkImpactDriftScale = 4;

    /// <summary>The fraction of a pixel a falling spark's vertical speed gains each falling update: a quarter pixel.</summary>
    private const int FallingSparkGravity = 16384;

    /// <summary>The whole-pixel vertical speed a falling spark's speed stops rising at.</summary>
    private const int FallingSparkTerminalWholeSpeed = 4;

    /// <summary>The vertical speed a spark bounces up at when it hits the floor: half a pixel upward.</summary>
    private static readonly SignedSixteenSixteen FallingSparkImpactBounce = SignedSixteenSixteen.FromParts(-1, 32768);

    /// <summary>Ports pre-instruction <c>$86:F3F0</c>.</summary>
    internal void RunFallingSparkPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        int frameCounterByte)
    {
        // A falling spark first moves through the generic collision helper. The helper's accepted movement is
        // *additional* to the spark's own 16.16 movement below; omitting it shifts the floor-contact frame.
        FallingSparkState spark = projectile.StateOf<FallingSparkState>();
        if (spark.VerticalSpeed.WholePixels >= 0)
        {
            // Move_EnemyProjectile_Vertically ($86:897B), slope reactions included, reads the spark's whole speed in
            // pixels as an 8.8 speed in 1/256ths of a pixel.
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, Axis.Vertical,
                    new SignedEightEight(spark.VerticalSpeed.WholeWord)))
            {
                BeginFallingSparkFloorImpact(projectile);
                return;
            }

            int acceleratedFraction = spark.VerticalSpeed.Fraction + FallingSparkGravity;
            int acceleratedWhole = spark.VerticalSpeed.WholePixels + WordCarry.OutOfWord(acceleratedFraction);

            // CMP #$0004 / BCS deliberately stops raising the whole part once it reaches four. The fraction still
            // received its quarter-pixel increment immediately before this test, reproducing the cartridge's slightly
            // asymmetric cap.
            spark.VerticalSpeed = SignedSixteenSixteen.FromParts(
                acceleratedWhole < FallingSparkTerminalWholeSpeed ? acceleratedWhole : spark.VerticalSpeed.WholePixels,
                unchecked((ushort)acceleratedFraction));
        }

        AddFallingSparkVerticalVelocity(projectile);
        AddFallingSparkHorizontalVelocity(projectile);

        if ((frameCounterByte & 3) == 0)
            SpawnFallingSparkTrail(projectile);
    }

    /// <summary>Adds the spark's vertical speed to its position with fractional carry.</summary>
    private static void AddFallingSparkVerticalVelocity(
        RoomEnemyProjectileSlot projectile) =>
        projectile.Position = projectile.Position.AdvanceY(projectile.StateOf<FallingSparkState>().VerticalSpeed);

    /// <summary>Adds the spark's horizontal drift with fractional carry.</summary>
    internal static void AddFallingSparkHorizontalVelocity(
        RoomEnemyProjectileSlot projectile) =>
        projectile.Position = projectile.Position.AdvanceX(projectile.StateOf<FallingSparkState>().HorizontalStep);

    /// <summary>
    /// Switches a falling spark to its floor-impact program and scales its stored
    /// horizontal delta by four with native carry propagation.
    /// </summary>
    internal static void BeginFallingSparkFloorImpact(RoomEnemyProjectileSlot projectile)
    {
        projectile.StartAnimation(new FallingSparkAnimation(FallingSparkProgram.HitFloor));

        // The impact quadruples the sideways drift, wrapping as the 32-bit doubling does.
        FallingSparkState spark = projectile.StateOf<FallingSparkState>();
        spark.HorizontalStep = SignedSixteenSixteen.FromRaw(unchecked(spark.HorizontalStep.RawValue * FallingSparkImpactDriftScale));

        // Half a pixel upward. Because the whole part is negative, later frames skip collision and gravity while the
        // impact spark arcs upward through its eleven-frame blinking animation.
        spark.VerticalSpeed = FallingSparkImpactBounce;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 2)));
    }

    /// <summary>Spawns a trail object at the spark's current whole-pixel position.</summary>
    private void SpawnFallingSparkTrail(RoomEnemyProjectileSlot projectile)
    {
        _ = _spriteObjects.SpawnRoomSpriteObject(
            projectile.Position.Whole,
            RoomSpriteObjectKind.FallingSparkTrail,
            projectile.Graphics);
    }

    /// <summary>Connects the falling-spark pre-instruction to the projectile handler table.</summary>
    internal void AddFallingSparksProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunFallingSparkPreInstruction(context.Projectile, context.Level, context.ShortCounter);
        }, EnemyProjectilePreInstruction.FallingSpark);
    }
}
