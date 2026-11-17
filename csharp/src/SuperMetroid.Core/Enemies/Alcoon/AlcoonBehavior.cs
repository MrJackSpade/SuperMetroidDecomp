using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The Alcoon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class AlcoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Alcoon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new AlcoonEntity(this, place, common);

    /// <summary>Provides RNG for the randomized post-volley walk count.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Alcoon's jump landing and its walking collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the three directed fireball variants.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Alcoon's per-room AI to collision, RNG, and projectile services.</summary>
    internal AlcoonBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Carries out one cue of the Alcoon's animations: spitting a fireball, starting to walk, or stepping. Starting to walk
    /// always continues at the walk for the Alcoon's facing, and a step that meets a wall turns it round to the other walk.
    /// </summary>
    private AnimationContinuation<AlcoonSegment>? ApplyAlcoonCue(RoomEnemyPlace place, AlcoonEnemyState state, RoomLevelData level, AlcoonCue cue)
    {
        if (cue is AlcoonCue.Fire fire)
        {
            SpawnAlcoonFireball(place, fire.Launch);
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
            return null;
        }
        AlcoonCueKind kind = ((AlcoonCue.Signal)cue).Kind;
        switch (kind)
        {
            case AlcoonCueKind.StartWalking:
                return StartAlcoonWalking(state);
            case AlcoonCueKind.Step:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                return MoveAlcoonHorizontally(place, state, level);
            case AlcoonCueKind.Move:
                return MoveAlcoonHorizontally(place, state, level);
            default:
                throw new InvalidOperationException($"Undefined {nameof(AlcoonCueKind)} {kind}.");
        }
    }

    /// <summary>Guards the initializer's simulated ascent and floor search against invalid room geometry.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Initializes the Alcoon, including its floor search.</summary>
    private void InitializeAlcoon(RoomEnemyPlace place, RoomLevelData level)
    {
        var state = new AlcoonEnemyState(AlcoonAction.WaitingForSamus);
        place.SetSpeciesState(state);

        state.StepCounter = 0;
        state.SpawnPosition = place.Position.Whole;
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        state.Playback = new(AlcoonAnimations.StartingWith(AlcoonSegment.WalkingLeft), AnimationPlace.StartOfAnimation);
        SetupAlcoonJumpMovement(state);

        // The initializer really executes every half-pixel-accelerated ascent
        // step, then every collision-aware descent step. It ultimately restores only the
        // whole Y coordinate; the collision-produced subposition deliberately survives.
        int iterations = 0;
        do
        {
            AddAlcoonYVelocity(place, state);
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, place);
        }
        while (state.VerticalVelocity.WholePixels < 0);

        while (true)
        {
            SignedSixteenSixteen displacement = state.VerticalVelocity;
            if (_terrain.MoveEnemyVertically(level, place, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, place);
        }

        state.LandingPosition = place.Position.Whole;
        place.Position = place.Position.WithWholeY(state.SpawnPosition.Y);
    }

    /// <summary>Runs Alcoon's per-frame behaviour through all seven of its actions.</summary>
    private void RunAlcoonMain(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case AlcoonAction.WaitingForSamus:
                WaitForAlcoonActivation(place, state, samus);
                return;
            case AlcoonAction.EmergingRising:
                RunAlcoonEmergingRise(place, state);
                return;
            case AlcoonAction.EmergingFalling:
                RunAlcoonEmergingFall(place, state, samus, level);
                return;
            case AlcoonAction.WalkingAndFiring:
                RunEmergedAlcoon(place, state, samus, level);
                return;
            case AlcoonAction.WaitingForFireAnimation:
                return;
            case AlcoonAction.HidingRising:
                RunAlcoonHidingRise(place, state);
                return;
            case AlcoonAction.HidingFalling:
                RunAlcoonHidingFall(place, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Alcoon has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Starts Alcoon's jump when Samus nears its predicted landing point.</summary>
    private void WaitForAlcoonActivation(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus)
    {
        if (CoordinateAxis.Distance(state.LandingPosition.Y, samus.Position.Y.Whole) >= AlcoonMotionDefinitions.EmergeYDistance)
            return;

        int signedXDistance = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole);
        if (Math.Abs(signedXDistance) >= AlcoonMotionDefinitions.EmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (signedXDistance < 0)
        {
            state.WalkingDirection = HorizontalDirection.Left;
            StartAlcoonAnimation(place, state, AlcoonSegment.AirborneLeftLookingUp);
        }
        else
        {
            state.WalkingDirection = HorizontalDirection.Right;
            StartAlcoonAnimation(place, state, AlcoonSegment.AirborneRightLookingUp);
        }
        state.Action = AlcoonAction.EmergingRising;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonEmergeSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Integrates the emerging ascent until vertical velocity ceases to be negative.</summary>
    private static void RunAlcoonEmergingRise(RoomEnemyPlace place, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(place, state);
        AccelerateAlcoonY(state);
        if (state.VerticalVelocity.WholePixels < 0)
            return;

        state.Action = AlcoonAction.EmergingFalling;
        StartAlcoonAnimation(
            place,
            state,
            FacesLeft(state)
                ? AlcoonSegment.AirborneLeftLookingForward
                : AlcoonSegment.AirborneRightLookingForward);
    }

    /// <summary>Applies collision-aware descent, then chooses the initial walking direction toward Samus.</summary>
    private void RunAlcoonEmergingFall(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.VerticalVelocity;
        if (!_terrain.MoveEnemyVertically(level, place, displacement))
        {
            AccelerateAlcoonY(state);
            return;
        }

        if (CoordinateAxis.Precedes(samus.Position.X.Whole, place.Position.X.Whole))
        {
            state.WalkingDirection = HorizontalDirection.Left;
            StartAlcoonAnimation(place, state, AlcoonSegment.WalkingLeft);
        }
        else
        {
            state.WalkingDirection = HorizontalDirection.Right;
            StartAlcoonAnimation(place, state, AlcoonSegment.WalkingRight);
        }
        state.Action = AlcoonAction.WalkingAndFiring;
        state.StepCounter = 1;
    }

    /// <summary>Maintains floor contact, starts a retreat at the spawn-distance limit, or begins a forward volley.</summary>
    private void RunEmergedAlcoon(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // This is a real two-pixel downward move, not merely a floor probe. On ordinary
        // floors it collides and keeps Alcoon aligned; if the floor vanishes it lets the
        // walking actor fall by the same amount before the remainder of main AI runs.
        _terrain.MoveEnemyVertically(level, place, AlcoonMotionDefinitions.FloorPress);

        if (CoordinateAxis.Distance(state.SpawnPosition.X, place.Position.X.Whole) >= AlcoonMotionDefinitions.HideXDistance)
        {
            state.Action = AlcoonAction.HidingRising;
            state.VerticalVelocity = AlcoonMotionDefinitions.HideJumpVelocity;
            StartAlcoonAnimation(
                place,
                state,
                FacesLeft(state)
                    ? AlcoonSegment.AirborneLeftLookingForward
                    : AlcoonSegment.AirborneRightLookingForward);
            return;
        }

        if (state.StepCounter != 0)
            return;

        bool samusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) < 0;
        bool movingLeft = FacesLeft(state);
        if (samusIsLeft != movingLeft)
            return;

        StartAlcoonAnimation(
            place,
            state,
            movingLeft
                ? AlcoonSegment.FireLeft
                : AlcoonSegment.FireRight);
        state.Action = AlcoonAction.WaitingForFireAnimation;
    }

    /// <summary>Starts the retreat arc upward until gravity reverses its Y velocity.</summary>
    private static void RunAlcoonHidingRise(RoomEnemyPlace place, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(place, state);
        AccelerateAlcoonY(state);
        if (state.VerticalVelocity.WholePixels >= 0)
            state.Action = AlcoonAction.HidingFalling;
    }

    /// <summary>Returns the retreating actor to its whole-pixel spawn position while retaining subpositions.</summary>
    private static void RunAlcoonHidingFall(RoomEnemyPlace place, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(place, state);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, state.SpawnPosition.Y) < 0)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // The reset snaps only whole coordinates. Both subpositions and
        // the last animation frame remain live while the actor waits underground.
        place.Position = place.Position.WithWhole(state.SpawnPosition);
        state.Action = AlcoonAction.WaitingForSamus;
    }

    /// <summary>Resets the jump to a twelve-pixel upward velocity and half-pixel acceleration.</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.WalkingDirection = null;
        state.VerticalVelocity = AlcoonMotionDefinitions.EmergeJumpVelocity;
        state.YAcceleration = AlcoonMotionDefinitions.Gravity;
    }

    /// <summary>Adds the Alcoon's acceleration to its vertical velocity.</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        state.VerticalVelocity += state.YAcceleration;
    }

    /// <summary>Integrates vertical velocity without collision during airborne ascent or retreat.</summary>
    private static void AddAlcoonYVelocity(RoomEnemyPlace place, AlcoonEnemyState state)
    {
        place.Position = place.Position.AdvanceY(state.VerticalVelocity);
    }

    /// <summary>Rejects an initializer that cannot find a landing floor within the simulation bound.</summary>
    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemyPlace place)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon place {place.Ordinal} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    /// <summary>Switches Alcoon's animation so its first step runs on the next update.</summary>
    private static void StartAlcoonAnimation(RoomEnemyPlace place, AlcoonEnemyState state, AlcoonSegment segment)
    {
        state.Playback = new(AlcoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the Alcoon's start-walking cue and returns the walk the animation continues at.</summary>
    private AnimationContinuation<AlcoonSegment> StartAlcoonWalking(AlcoonEnemyState state)
    {
        state.Action = AlcoonAction.WalkingAndFiring;
        int count = AlcoonWalkDefinitions.StepCount.Of(_room.Random.Read());
        state.StepCounter = count == 0 ? AlcoonWalkDefinitions.StepsForZeroSample : count;
        return new(AlcoonAnimations.StartingWith(FacesLeft(state) ? AlcoonSegment.WalkingLeft : AlcoonSegment.WalkingRight),
            AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Implements both movement cues: the animation runs on unless a wall turns the Alcoon
    /// round, when it continues at the other walk's first pose.
    /// </summary>
    private AnimationContinuation<AlcoonSegment>? MoveAlcoonHorizontally(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.WalkingDirection switch
        {
            HorizontalDirection.Left => -AlcoonWalkDefinitions.StepDistance,
            HorizontalDirection.Right => AlcoonWalkDefinitions.StepDistance,
            null => SignedSixteenSixteen.Zero,
            _ => throw new InvalidOperationException($"Undefined {nameof(HorizontalDirection)} {state.WalkingDirection}."),
        };
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
        {
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
            return null;
        }

        // A wall turns it round; one that has not picked a side yet keeps walking nowhere and continues at the left walk.
        bool turnsLeft = !FacesLeft(state);
        state.WalkingDirection = state.WalkingDirection switch
        {
            HorizontalDirection.Left => HorizontalDirection.Right,
            HorizontalDirection.Right => HorizontalDirection.Left,
            _ => state.WalkingDirection,
        };
        return turnsLeft
            ? AlcoonAnimations.WalkingLeftFromFirstStride
            : AlcoonAnimations.WalkingRightFromFirstStride;
    }

    /// <summary>Whether the Alcoon faces left: it does only once it has picked the left side.</summary>
    private static bool FacesLeft(AlcoonEnemyState state) => state.WalkingDirection == HorizontalDirection.Left;

    /// <summary>An Alcoon fireball (#1760): it flies with vertical, then horizontal, room collision and horizontal drag.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyAlcoonFireball(context.Projectile, context.Level));

    /// <summary>Runs Alcoon's initialization AI.</summary>
    internal void RunAlcoonInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeAlcoon(place, surroundings.PopulationLevel);
    }

    /// <summary>Runs Alcoon's main AI.</summary>
    internal void RunAlcoonMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunAlcoonMain(place, place.RequireSpeciesState<AlcoonEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Alcoon's typed animation.</summary>
    internal void AdvanceAlcoonTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, AlcoonAnimations.Segments,
            static (AlcoonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyAlcoonCue(place, state, context.Level, cue));

    /// <summary>Spits an Alcoon fireball with the vertical speed of <paramref name="launch"/>.</summary>
    internal void SpawnAlcoonFireball(RoomEnemyPlace alcoon, AlcoonFireballLaunch launch)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        AlcoonEnemyState state = alcoon.RequireSpeciesState<AlcoonEnemyState>();
        bool movingLeft = FacesLeft(state);

        // Spawning copies the fireball's complete authored
        // definition before its initializer touches position, subposition, and velocity.
        // Keeping that division exact makes radii, damage, persistence, shot blocking,
        // animation, and frame behaviour owned by the definition instead of duplicating them here.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.AlcoonFireball,
            Fireball,
            SpriteGraphics.SpawnedBy(alcoon));
        projectile.StartAnimation(new AlcoonFireballAnimation(AlcoonFireballSegment.Initial));
        Offset16<RoomPixels> muzzle = AlcoonFireballLaunchDefinitions.MuzzleFacingRight;
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(alcoon.Position.Whole + (movingLeft ? muzzle.MirroredX() : muzzle));
        projectile.Velocity = projectile.Velocity.WithX(movingLeft
            ? -AlcoonFireballLaunchDefinitions.LaunchSpeed
            : AlcoonFireballLaunchDefinitions.LaunchSpeed);
        projectile.Velocity = projectile.Velocity.WithY(launch.YVelocity);
    }

    /// <summary>Flies the Alcoon fireball one update.</summary>
    internal static void FlyAlcoonFireball(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        // Unlike Ridley's fireball, this definition checks vertical collision first. If
        // either axis collides, the projectile clears its ID immediately and
        // never applies horizontal deceleration on that frame.
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Delete();
            return;
        }

        // The speed slows toward the terminal speed and then holds it; neither step leaves the 1/256-pixel range, as the speed
        // starts at the launch speed and only moves toward the terminal speed.
        SignedEightEight velocity = projectile.Velocity.X;
        if (velocity.IsNegative)
        {
            velocity += AlcoonFireballLaunchDefinitions.Deceleration;
            if (velocity >= -AlcoonFireballLaunchDefinitions.TerminalSpeed)
                velocity = -AlcoonFireballLaunchDefinitions.TerminalSpeed;
        }
        else
        {
            velocity -= AlcoonFireballLaunchDefinitions.Deceleration;
            if (velocity < AlcoonFireballLaunchDefinitions.TerminalSpeed)
                velocity = AlcoonFireballLaunchDefinitions.TerminalSpeed;
        }
        projectile.Velocity = projectile.Velocity.WithX(velocity);
    }
}
