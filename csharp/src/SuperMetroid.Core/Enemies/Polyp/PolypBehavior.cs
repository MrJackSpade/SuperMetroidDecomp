using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>The Polyp species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PolypBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Polyp];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new PolypEntity(this, place, common));

    /// <summary>Provides the shared room RNG and enemy state used by all Polyp actors.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns the rock projectiles a Polyp launches.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the Polyp species to one room's enemies and projectile pool.</summary>
    internal PolypBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Initializes the Polyp.</summary>
    internal void InitializePolyp(RoomEnemyPlace place)
    {
        place.AnimationClock.Restart();
        var state = new PolypEnemyState(PolypAction.WaitingForSamus)
        {
            Playback = new(PolypAnimations.StartingWith(PolypSegment.Stationary), AnimationPlace.StartOfAnimation),
        };
        place.SetSpeciesState(state);

        _room.Random.Reseed(SpeciesRandomReseed.PolypPlacement);
    }

    /// <summary>Runs the Polyp's main AI and its three actions.</summary>
    private void RunPolypMain(
        RoomEnemyPlace place,
        PolypEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case PolypAction.WaitingForSamus:
                if (IsWithinPolypAxis(samus.Position.X.Whole, place.Position.X.Whole) &&
                    IsWithinPolypAxis(samus.Position.Y.Whole, place.Position.Y.Whole))
                {
                    // The game only sets the shoot action here. The rock is emitted
                    // on the next enemy frame, a one-frame tell preserved deliberately.
                    state.Action = PolypAction.ShootingRock;
                }
                return;

            case PolypAction.ShootingRock:
                ShootPolypRock(place, state);
                return;

            case PolypAction.Cooldown:
                if (state.CooldownTimer.StepEnds())
                    state.Action = PolypAction.WaitingForSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Polyp has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Launches a rock with three independent random choices: its horizontal speed, its rise and the cooldown after it.</summary>
    private void ShootPolypRock(RoomEnemyPlace place, PolypEnemyState state)
    {
        SignedEightEight xVelocity = PolypLaunchDefinitions.XVelocity.Choose(_room.Random.Generate());

        QuadraticSpeedCurvePlace launchPlace = PolypLaunches.LaunchPlace(_room.Random.Generate());
        SpawnPolypRock(place, launchPlace, xVelocity);

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
    internal StatefulSingleMotionProjectileBehavior<PolypRockState> Rock => field ??= new(
        context => FlyPolypRock(context.Projectile, context.Camera));

    /// <summary>Advances Polyp's typed animation.</summary>
    internal static void AdvancePolypTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, PolypAnimations.Segments,
            static (PolypEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Polyp's cue-free animation raised {cue}."));

    /// <summary>Runs Polyp's main AI.</summary>
    internal void RunPolypMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunPolypMain(place, place.RequireSpeciesState<PolypEnemyState>(), frame.Samus);
    }

    /// <summary>
    /// Spawns a lava rock from the Polyp and initializes it.
    /// The rock draws from the Polyp's spawned-object sheet in its colours and starts at the Polyp's position, fractions included.
    /// </summary>
    internal void SpawnPolypRock(
        RoomEnemyPlace source,
        QuadraticSpeedCurvePlace launchPlace,
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
        projectile.InstallAnimation(new PolypRockAnimation(PolypRockSegment.LavaRock));
        Rock.States.Start(projectile, new PolypRockState(launchPlace));
        projectile.Velocity = new(xVelocity, SignedEightEight.Zero);
        projectile.Position = source.Position;
    }

    /// <summary>Flies the rock for one update, rising or falling.</summary>
    internal void FlyPolypRock(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        PolypRockMovement phase = Rock.States.Of(projectile).Phase;
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
