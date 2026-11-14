using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The Polyp species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PolypBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.LavaRocks];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PolypEntity(this, slot, common);

    /// <summary>Provides the shared room RNG and enemy state used by all Polyp actors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates rock projectile slots when a Polyp launches.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the Polyp species to one room's enemies and projectile pool.</summary>
    internal PolypBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Samus must be within 64 pixels on each axis to trigger a launch.</summary>
    private const int PolypProximity = 64;
    /// <summary>Each Polyp initializer resets the shared enemy random seed to 17.</summary>
    private const int PolypRandomSeed = 17;

    /// <summary>Initializes the Polyp.</summary>
    internal void InitializePolyp(RoomEnemySlot slot)
    {
        Action<int> setRandomNumber = _room.Random.Seed;

        slot.AnimationClock.Restart();
        var state = new PolypEnemyState
        {
            Playback = new(PolypAnimations.StartingWith(PolypSegment.Stationary), AnimationPlace.StartOfAnimation),
            Action = PolypAction.WaitingForSamus,
            CooldownTimer = 0,
        };
        slot.SetSpeciesState(state);

        // Every Polyp initializer writes 17 to the shared seed. In Volcano the fourth
        // actor therefore leaves this exact value for the first gameplay frame; treating it
        // as actor-local randomness would produce a different launch sequence.
        setRandomNumber(PolypRandomSeed);
    }

    /// <summary>Runs the Polyp's main AI and its three actions.</summary>
    private void RunPolypMain(
        RoomEnemySlot slot,
        PolypEnemyState state,
        SamusState? samus)
    {
        switch (state.Action)
        {
            case PolypAction.WaitingForSamus:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Polyp proximity AI requires the active Samus actor.");
                }

                if (IsWithinPolypAxis(samus.Position.X.Whole, slot.Position.X.Whole) &&
                    IsWithinPolypAxis(samus.Position.Y.Whole, slot.Position.Y.Whole))
                {
                    // The game only sets the shoot action here. The rock is emitted
                    // on the next enemy frame, a one-frame tell preserved deliberately.
                    state.Action = PolypAction.ShootingRock;
                }
                return;

            case PolypAction.ShootingRock:
                ShootPolypRock(slot, state);
                return;

            case PolypAction.Cooldown:
                if (--state.CooldownTimer < 0)
                    state.Action = PolypAction.WaitingForSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Polyp has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Runs the three independent random-table selections.</summary>
    private void ShootPolypRock(RoomEnemySlot slot, PolypEnemyState state)
    {
        SignedEightEight xVelocity = PolypLaunches.XVelocity(_room.Random.Generate());

        int initialYSpeed = PolypLaunches.InitialYMark(_room.Random.Generate());
        SpawnPolypRock(slot, initialYSpeed, xVelocity);

        state.Action = PolypAction.Cooldown;
        state.CooldownTimer = PolypLaunches.Cooldown(_room.Random.Generate());
    }

    /// <summary>
    /// Recreates the signed Y-minus-X difference followed by the signed threshold comparison used by
    /// both proximity helpers. Equality at 64 pixels is outside; the modular -32768
    /// difference also remains outside without passing through <c>Math.Abs</c> overflow.
    /// </summary>
    private static bool IsWithinPolypAxis(int samusCoordinate, int enemyCoordinate)
    {
        return CoordinateAxis.Distance(samusCoordinate, enemyCoordinate) < PolypProximity;
    }

    /// <summary>A Polyp rock (#1760): it rises and falls quadratically until it leaves the camera.</summary>
    internal static readonly EnemyProjectileBehavior Rock = new SingleMotionProjectileBehavior(
        EnemyProjectileFrameBehavior.PolypRockFlight,
        context => RunPolypRockFrameBehavior(context.Projectile, context.Camera));

    /// <summary>Advances Polyp's typed animation.</summary>
    internal static void AdvancePolypTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, PolypAnimations.Segments,
            static (PolypEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Polyp's cue-free animation raised {cue}."));

    /// <summary>Runs Polyp's main AI.</summary>
    internal void RunPolypMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPolypMain(slot, slot.RequireSpeciesState<PolypEnemyState>(), frame.Samus);
    }

    /// <summary>Projectile frame behaviour installed on each launched rock.</summary>
    private const EnemyProjectileFrameBehavior PolypRockFrameBehavior =
        EnemyProjectileFrameBehavior.PolypRockFlight;
    /// <summary>Each rock frame moves its place along the quadratic speed curve by two marks.</summary>
    private static readonly QuadraticSpeedCurvePlace PolypRockGravityStep = QuadraticSpeedCurvePlace.FromMarks(2);
    /// <summary>Falling rocks stop accelerating at mark 64.</summary>
    private static readonly QuadraticSpeedCurvePlace PolypRockTerminalCurvePlace = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>
    /// Spawns a lava rock from the Polyp and initializes it.
    /// The rock draws from the Polyp's spawned-object sheet in its colours, and all four position halves are copied at allocation.
    /// </summary>
    internal void SpawnPolypRock(
        RoomEnemySlot source,
        int initialYSpeed,
        SignedEightEight xVelocity)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PolypRock,
            Rock,
            SpriteGraphics.SpawnedBy(source));
        projectile.InstallAnimation(new PolypRockAnimation(PolypRockSegment.Initial));
        projectile.FrameBehavior = PolypRockFrameBehavior;
        projectile.State = new PolypRockState(QuadraticSpeedCurvePlace.FromMarks(initialYSpeed));
        projectile.Velocity = new(xVelocity, SignedEightEight.Zero);
        projectile.Position = source.Position;
    }

    /// <summary>Runs the rock's per-frame behaviour and its rising/falling step.</summary>
    internal static void RunPolypRockFrameBehavior(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        PolypRockMovement phase = projectile.StateOf<PolypRockState>().Phase;
        switch (phase)
        {
            case PolypRockMovement.Rising:
                StepRisingPolypRock(projectile);
                break;
            case PolypRockMovement.Falling:
                StepFallingPolypRock(projectile);
                break;
            default:
                throw new InvalidDataException(
                    $"Undefined {nameof(PolypRockMovement)} {phase}.");
        }

        CacatacBehavior.DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    /// <summary>Integrates two upward quadratic samples unless the apex consumes this frame.</summary>
    private static void StepRisingPolypRock(EnemyProjectile projectile)
    {
        PolypRockState rock = projectile.StateOf<PolypRockState>();
        rock.SpeedCurvePlace -= PolypRockGravityStep;
        if (rock.SpeedCurvePlace.IsNegative)
        {
            // Crossing below zero consumes an entire apex frame: retail installs falling
            // state and returns before either vertical or horizontal movement.
            rock.SpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            rock.Phase = PolypRockMovement.Falling;
            return;
        }

        // The rock integrates two adjacent quadratic samples per frame, first
        // the mark after its speed's and then its speed's own. This is why the place moves by two marks
        // while the visual arc still advances at gameplay-frame rate.
        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace + QuadraticSpeedCurvePlace.OneMark, sign: EnemySpeedSign.Negative);
        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace, sign: EnemySpeedSign.Negative);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>Clamps downward speed and integrates two positive quadratic samples per frame.</summary>
    private static void StepFallingPolypRock(EnemyProjectile projectile)
    {
        PolypRockState rock = projectile.StateOf<PolypRockState>();
        rock.SpeedCurvePlace += PolypRockGravityStep;
        if (rock.SpeedCurvePlace >= PolypRockTerminalCurvePlace)
            rock.SpeedCurvePlace = PolypRockTerminalCurvePlace;

        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace - QuadraticSpeedCurvePlace.OneMark, sign: EnemySpeedSign.Positive);
        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace, sign: EnemySpeedSign.Positive);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>
    /// Adds one curve mark's speed in the direction <paramref name="sign"/> names, the fraction first and then the
    /// whole pixels.
    /// </summary>
    internal static void AddPolypRockQuadraticStep(
        EnemyProjectile projectile,
        QuadraticSpeedCurvePlace place,
        EnemySpeedSign sign)
    {
        // A fraction past a whole pixel carries into the whole pixels, and the whole pixels wrap around the room.
        SignedSixteenSixteen speed = place.Speed.Toward(sign);
        projectile.Position = projectile.Position.WithY(projectile.Position.Y.MovedBy(speed));
    }

    /// <summary>Moves the rock with the game's split signed-1/256-pixel horizontal addition.</summary>
    private static void MovePolypRockHorizontally(EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
    }
}
