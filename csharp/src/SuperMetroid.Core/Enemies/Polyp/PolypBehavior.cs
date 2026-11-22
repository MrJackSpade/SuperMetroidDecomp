using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The Polyp species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class PolypBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Polyp];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new PolypEntity(this, occupant, place, common));

    /// <summary>Provides the shared room RNG and enemy state used by all Polyp actors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns the rock projectiles a Polyp launches.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the Polyp species to one room's enemies and projectile pool.</summary>
    internal PolypBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
        Rock = new(FlyPolypRock);
    }

    /// <summary>Initializes the Polyp.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal PolypEnemyState InitializePolyp(RoomEnemyPlace place)
    {
        place.AnimationClock.Restart();
        var state = new PolypEnemyState(PolypAction.WaitingForSamus)
        {
            Playback = new(PolypAnimations.StartingWith(PolypSegment.Stationary), AnimationPlace.StartOfAnimation),
        };

        _room.Random.Reseed(SpeciesRandomReseed.PolypPlacement);
        return state;
    }

    /// <summary>What <see cref="RunPolypMain"/> does for each <see cref="PolypAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<PolypAction, Action<RoomEnemyOccupant, RoomEnemyPlace, PolypEnemyState, SamusState>> RunPolypMainForPolypAction => field ??= new(new Dictionary<PolypAction, Action<RoomEnemyOccupant, RoomEnemyPlace, PolypEnemyState, SamusState>>
    {
        [PolypAction.WaitingForSamus] = (_, place, state, samus) =>
        {
            if (IsWithinPolypAxis(samus.Position.X.Whole, place.Position.X.Whole) &&
                IsWithinPolypAxis(samus.Position.Y.Whole, place.Position.Y.Whole))
            {
                // The game only sets the shoot action here. The rock is emitted
                // on the next enemy frame, a one-frame tell preserved deliberately.
                state.Action = PolypAction.ShootingRock;
            }
        },
        [PolypAction.ShootingRock] = (occupant, place, state, samus) => ShootPolypRock(occupant, place, state),
        [PolypAction.Cooldown] = (_, place, state, samus) =>
        {
            if (state.CooldownTimer.StepEnds())
                state.Action = PolypAction.WaitingForSamus;
        },
    });

    /// <summary>Runs the Polyp's main AI and its three actions.</summary>
    private void RunPolypMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PolypEnemyState state,
        SamusState samus)
    {
        RunPolypMainForPolypAction[state.Action](occupant, place, state, samus);
    }

    /// <summary>Launches a rock with three independent random choices: its horizontal speed, its rise and the cooldown after it.</summary>
    private void ShootPolypRock(RoomEnemyOccupant occupant, RoomEnemyPlace place, PolypEnemyState state)
    {
        SignedEightEight xVelocity = PolypLaunchDefinitions.XVelocity.Choose(_room.Random.Generate());

        QuadraticSpeedCurvePlace launchPlace = PolypLaunches.LaunchPlace(_room.Random.Generate());
        SpawnPolypRock(occupant, place, launchPlace, xVelocity);

        state.Action = PolypAction.Cooldown;
        state.CooldownTimer = UpdateCountdown.For(PolypLaunchDefinitions.Cooldown.Choose(_room.Random.Generate()));
    }

    /// <summary>
    /// Whether Samus lies within the Polyp's launch reach on one axis: nearer than
    /// <see cref="PolypLaunchDefinitions.LaunchReach"/> pixels either way, a distance of exactly that reach lying outside.
    /// </summary>
    private static bool IsWithinPolypAxis(int samusCoordinate, int enemyCoordinate)
    {
        return Math.Abs(samusCoordinate - enemyCoordinate) < PolypLaunchDefinitions.LaunchReach;
    }

    /// <summary>A Polyp rock (#1760): it rises and falls quadratically until it leaves the camera.</summary>
    internal StatefulSingleMotionProjectileBehavior<PolypRockState> Rock { get; }

    /// <summary>Moves a Polyp rock (<see cref="Rock"/>).</summary>
    private void FlyPolypRock(EnemyProjectileMotionContext context) => FlyPolypRock(context.Projectile, context.Camera);

    /// <summary>Advances Polyp's typed animation.</summary>
    internal static void AdvancePolypTypedAnimation(RoomEnemyPlace place, PolypEnemyState polypEnemyState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, polypEnemyState, PolypAnimations.Segments,
            static (PolypEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs Polyp's main AI.</summary>
    internal void RunPolypMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, PolypEnemyState polypEnemyState, IEnemyAiFrame frame)
    {
        RunPolypMain(occupant, place, polypEnemyState, frame.Samus);
    }

    /// <summary>
    /// Spawns a lava rock from the Polyp and initializes it.
    /// The rock draws from the Polyp's spawned-object sheet in its colours and starts at the Polyp's position, fractions included.
    /// </summary>
    internal void SpawnPolypRock(
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        QuadraticSpeedCurvePlace launchPlace,
        SignedEightEight xVelocity)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.PolypRock, Rock, SpriteGraphics.SpawnedBy(occupant, source.Shading), new PolypRockAnimation(PolypRockSegment.LavaRock));
        if (projectile is null)
            return;

        Rock.States.Start(projectile, new PolypRockState(launchPlace));
        projectile.Velocity = new(xVelocity, SignedEightEight.Zero);
        projectile.Position = source.Position;
    }

    /// <summary>What <see cref="FlyPolypRock"/> does for each <see cref="PolypRockMovement"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<PolypRockMovement, Action<EnemyProjectile>> FlyPolypRockForPolypRockMovement => field ??= new(new Dictionary<PolypRockMovement, Action<EnemyProjectile>>
    {
        [PolypRockMovement.Rising] = projectile => StepRisingPolypRock(projectile),
        [PolypRockMovement.Falling] = projectile => StepFallingPolypRock(projectile),
    });

    /// <summary>Flies the rock for one update, rising or falling.</summary>
    internal void FlyPolypRock(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        PolypRockMovement phase = Rock.States.Of(projectile).Phase;
        FlyPolypRockForPolypRockMovement[phase](projectile);

        EnemyView.DeleteOutside(projectile, camera);
    }

    /// <summary>Integrates two upward quadratic samples unless the apex consumes this frame.</summary>
    private void StepRisingPolypRock(EnemyProjectile projectile)
    {
        PolypRockState rock = Rock.States.Of(projectile);
        rock.SpeedCurvePlace -= PolypLaunchDefinitions.RockFallStep;
        if (rock.SpeedCurvePlace.IsNegative)
        {
            // Crossing below zero consumes an entire apex frame: it enters its falling
            // state and returns before either vertical or horizontal movement.
            rock.SpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            rock.Phase = PolypRockMovement.Falling;
            return;
        }

        // The rock integrates two adjacent quadratic samples per frame, first
        // the mark after its speed's and then its speed's own. This is why the place moves by two marks
        // while the visual arc still advances at gameplay-frame rate.
        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace + QuadraticSpeedCurvePlace.OneMark, VerticalDirection.Up);
        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace, VerticalDirection.Up);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>Clamps downward speed and integrates two positive quadratic samples per frame.</summary>
    private void StepFallingPolypRock(EnemyProjectile projectile)
    {
        PolypRockState rock = Rock.States.Of(projectile);
        rock.SpeedCurvePlace += PolypLaunchDefinitions.RockFallStep;
        if (rock.SpeedCurvePlace >= PolypLaunchDefinitions.RockFastestFallPlace)
            rock.SpeedCurvePlace = PolypLaunchDefinitions.RockFastestFallPlace;

        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace - QuadraticSpeedCurvePlace.OneMark, VerticalDirection.Down);
        AddPolypRockQuadraticStep(projectile, rock.SpeedCurvePlace, VerticalDirection.Down);
        MovePolypRockHorizontally(projectile);
    }

    /// <summary>
    /// Adds one curve mark's speed toward <paramref name="direction"/>, the fraction first and then the whole pixels.
    /// </summary>
    internal static void AddPolypRockQuadraticStep(
        EnemyProjectile projectile,
        QuadraticSpeedCurvePlace place,
        VerticalDirection direction)
    {
        // A fraction past a whole pixel carries into the whole pixels.
        SignedSixteenSixteen speed = place.Speed.Toward(direction);
        projectile.Position = projectile.Position.WithY(projectile.Position.Y.MovedBy(speed));
    }

    /// <summary>Moves the rock sideways by its horizontal speed.</summary>
    private static void MovePolypRockHorizontally(EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
    }
}
