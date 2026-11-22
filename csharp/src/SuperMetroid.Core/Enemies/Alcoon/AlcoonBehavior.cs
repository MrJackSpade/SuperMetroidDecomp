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
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Alcoon], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.FromSurroundings(new AlcoonEntity(this, occupant, place, common));

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
    /// Carries out one cue of the Alcoon's animations: spitting a fireball, starting to walk, or taking a stride. Starting to
    /// walk always continues at the walk for the Alcoon's facing, and a stride that meets a wall turns it round to the other walk.
    /// </summary>
    private AnimationContinuation<AlcoonSegment>? ApplyAlcoonCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, AlcoonEnemyState state, RoomLevelData level, AlcoonCue cue) =>
        cue.Match(ApplyAlcoonCueCases, new ApplyAlcoonCueContext(occupant, place, state, level, this));

    /// <summary>What <see cref="ApplyAlcoonCue"/> does on each <see cref="AlcoonCue"/>, built once (#2553).</summary>
    private static AlcoonCueCases<ApplyAlcoonCueContext, AnimationContinuation<AlcoonSegment>?> ApplyAlcoonCueCases { get; } = new(
            Fire: static (fire, context) =>
            {
                context.Owner.SpawnAlcoonFireball(context.Occupant, context.Place, fire.Launch);
                context.Owner._audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            },
            Signal: static (signal, context) => context.Owner.AlcoonSignals[signal.Kind](context.Place, context.State, context.Level));

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Level">The level the cases read.</param>
    /// <param name="Owner">The <see cref="AlcoonBehavior"/> reacting.</param>
    private readonly record struct ApplyAlcoonCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Place, AlcoonEnemyState State, RoomLevelData Level, AlcoonBehavior Owner);

    /// <summary>What each of the Alcoon's signals does, and the animation it continues at, or null to run on.</summary>
    private IdentityTable<AlcoonCueKind, Func<RoomEnemyPlace, AlcoonEnemyState, RoomLevelData, AnimationContinuation<AlcoonSegment>?>> AlcoonSignals => field ??=
        new(new Dictionary<AlcoonCueKind, Func<RoomEnemyPlace, AlcoonEnemyState, RoomLevelData, AnimationContinuation<AlcoonSegment>?>>
        {
            [AlcoonCueKind.StartWalking] = (_, state, _) => StartAlcoonWalking(state),
            [AlcoonCueKind.CompleteWalkCycle] = (place, state, level) =>
            {
                state.WalkCyclesBeforeVolley.Step();
                return MoveAlcoonHorizontally(place, state, level);
            },
            [AlcoonCueKind.Move] = MoveAlcoonHorizontally,
        });

    /// <summary>Guards the setup's simulated ascent and floor search against invalid room geometry.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Initializes the Alcoon, including its floor search.</summary>
    private void InitializeAlcoon(RoomEnemyPlace place, RoomLevelData level)
    {
        var state = new AlcoonEnemyState(AlcoonAction.WaitingForSamus);
        place.SetSpeciesState(state);

        state.WalkCyclesBeforeVolley = UpdateCountdown.Ended();
        state.SpawnPosition = place.Position.Whole;
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        state.Playback = new(AlcoonAnimations.StartingWith(AlcoonSegment.WalkingLeft), AnimationPlace.StartOfAnimation);
        SetupAlcoonJumpMovement(state);

        // The setup really executes every half-pixel-accelerated ascent
        // step, then every collision-aware descent step. It ultimately restores only the
        // whole Y coordinate; where the collision left it within a pixel stays.
        int iterations = 0;
        do
        {
            AddAlcoonYVelocity(place, state);
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, place);
        }
        while (state.VerticalVelocity.IsNegative);

        while (true)
        {
            SignedSixteenSixteen displacement = state.VerticalVelocity;
            if (_terrain.MoveEnemyVertically(level, place, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, place);
        }

        state.LandingY = place.Position.Y.Whole;
        place.Position = place.Position.WithWholeY(state.SpawnPosition.Y);
    }

    /// <summary>What <see cref="RunAlcoonMain"/> does for each <see cref="AlcoonAction"/> member, each member's own arm.</summary>
    private IdentityTable<AlcoonAction, Action<RoomEnemyPlace, AlcoonEnemyState, SamusState, RoomLevelData>> RunAlcoonMainForAlcoonAction => field ??= new(new Dictionary<AlcoonAction, Action<RoomEnemyPlace, AlcoonEnemyState, SamusState, RoomLevelData>>
    {
        [AlcoonAction.WaitingForSamus] = (place, state, samus, level) => WaitForAlcoonActivation(place, state, samus),
        [AlcoonAction.EmergingRising] = (place, state, samus, level) => RunAlcoonEmergingRise(place, state),
        [AlcoonAction.EmergingFalling] = (place, state, samus, level) => RunAlcoonEmergingFall(place, state, samus, level),
        [AlcoonAction.WalkingAndFiring] = (place, state, samus, level) => RunEmergedAlcoon(place, state, samus, level),
        [AlcoonAction.WaitingForFireAnimation] = (place, state, samus, level) => { },
        [AlcoonAction.HidingRising] = (place, state, samus, level) => RunAlcoonHidingRise(place, state),
        [AlcoonAction.HidingFalling] = (place, state, samus, level) => RunAlcoonHidingFall(place, state),
    });

    /// <summary>Runs Alcoon for one update through all seven of its actions.</summary>
    private void RunAlcoonMain(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        RunAlcoonMainForAlcoonAction[state.Action](place, state, samus, level);
    }

    /// <summary>Starts Alcoon's jump when Samus nears its predicted landing point.</summary>
    private void WaitForAlcoonActivation(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(state.LandingY - samus.Position.Y.Whole) >= AlcoonMotionDefinitions.EmergeYDistance)
            return;

        int signedXDistance = samus.Position.X.Whole - place.Position.X.Whole;
        if (Math.Abs(signedXDistance) >= AlcoonMotionDefinitions.EmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (signedXDistance < 0)
        {
            state.WalkingDirection = AlcoonWalk.Left;
            StartAlcoonAnimation(place, state, AlcoonSegment.AirborneLeftLookingUp);
        }
        else
        {
            state.WalkingDirection = AlcoonWalk.Right;
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
        if (state.VerticalVelocity.IsNegative)
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

        if (samus.Position.X.Whole < place.Position.X.Whole)
        {
            state.WalkingDirection = AlcoonWalk.Left;
            StartAlcoonAnimation(place, state, AlcoonSegment.WalkingLeft);
        }
        else
        {
            state.WalkingDirection = AlcoonWalk.Right;
            StartAlcoonAnimation(place, state, AlcoonSegment.WalkingRight);
        }
        state.Action = AlcoonAction.WalkingAndFiring;
        state.WalkCyclesBeforeVolley = UpdateCountdown.Lasting(AlcoonWalkDefinitions.EmergedWalkCycles);
    }

    /// <summary>Maintains floor contact, starts a retreat at the spawn-distance limit, or begins a forward volley.</summary>
    private void RunEmergedAlcoon(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // This is a real downward move, not merely a floor probe. On ordinary
        // floors it collides and keeps Alcoon aligned; if the floor vanishes it lets the
        // walking actor fall by the same amount before the remainder of main AI runs.
        _terrain.MoveEnemyVertically(level, place, AlcoonMotionDefinitions.FloorPress);

        if (Math.Abs(state.SpawnPosition.X - place.Position.X.Whole) >= AlcoonMotionDefinitions.HideXDistance)
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

        if (state.WalkCyclesBeforeVolley.IsRunning)
            return;

        bool samusIsLeft = samus.Position.X.Whole < place.Position.X.Whole;
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
        if (!state.VerticalVelocity.IsNegative)
            state.Action = AlcoonAction.HidingFalling;
    }

    /// <summary>Returns the retreating actor to its whole-pixel spawn position, keeping where it lies within a pixel.</summary>
    private static void RunAlcoonHidingFall(RoomEnemyPlace place, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(place, state);
        if (place.Position.Y.Whole < state.SpawnPosition.Y)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // The reset snaps only whole coordinates. Where it lies within a pixel and the last animation frame stay as
        // they are while the actor waits underground.
        place.Position = place.Position.WithWhole(state.SpawnPosition);
        state.Action = AlcoonAction.WaitingForSamus;
    }

    /// <summary>Resets the jump to its upward velocity (<see cref="AlcoonMotionDefinitions.EmergeJumpVelocity"/>).</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.WalkingDirection = AlcoonWalk.Undecided;
        state.VerticalVelocity = AlcoonMotionDefinitions.EmergeJumpVelocity;
    }

    /// <summary>Adds the Alcoon's gravity (<see cref="AlcoonMotionDefinitions.Gravity"/>) to its vertical velocity (#2432).</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        state.VerticalVelocity += AlcoonMotionDefinitions.Gravity;
    }

    /// <summary>Integrates vertical velocity without collision during airborne ascent or retreat.</summary>
    private static void AddAlcoonYVelocity(RoomEnemyPlace place, AlcoonEnemyState state)
    {
        place.Position = place.Position.AdvanceY(state.VerticalVelocity);
    }

    /// <summary>Rejects a setup that cannot find a landing floor within the simulation bound.</summary>
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
        state.WalkCyclesBeforeVolley = UpdateCountdown.For(AlcoonWalkDefinitions.WalkCycles.Choose(_room.Random.Read()));
        return new(AlcoonAnimations.StartingWith(FacesLeft(state) ? AlcoonSegment.WalkingLeft : AlcoonSegment.WalkingRight),
            AnimationPlace.StartOfAnimation);
    }

    /// <summary>The stride an Alcoon takes on each walk: one that has not picked a side yet walks nowhere.</summary>
    private static IdentityTable<AlcoonWalk, SignedSixteenSixteen> StrideToward => field ??= new(new Dictionary<AlcoonWalk, SignedSixteenSixteen>
    {
        [AlcoonWalk.Left] = -AlcoonWalkDefinitions.StrideDistance,
        [AlcoonWalk.Right] = AlcoonWalkDefinitions.StrideDistance,
        [AlcoonWalk.Undecided] = SignedSixteenSixteen.Zero,
    });

    /// <summary>The walk a wall turns each walk round to: one that has not picked a side yet keeps walking nowhere.</summary>
    private static IdentityTable<AlcoonWalk, AlcoonWalk> TurnedRound => field ??= new(new Dictionary<AlcoonWalk, AlcoonWalk>
    {
        [AlcoonWalk.Left] = AlcoonWalk.Right,
        [AlcoonWalk.Right] = AlcoonWalk.Left,
        [AlcoonWalk.Undecided] = AlcoonWalk.Undecided,
    });

    /// <summary>
    /// Implements both movement cues: the animation runs on unless a wall turns the Alcoon
    /// round, when it continues at the other walk's first pose.
    /// </summary>
    private AnimationContinuation<AlcoonSegment>? MoveAlcoonHorizontally(
        RoomEnemyPlace place,
        AlcoonEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = StrideToward[state.WalkingDirection];
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
        {
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, place);
            return null;
        }

        // A wall turns it round; one that has not picked a side yet keeps walking nowhere and continues at the left walk.
        bool turnsLeft = !FacesLeft(state);
        state.WalkingDirection = TurnedRound[state.WalkingDirection];
        return turnsLeft
            ? AlcoonAnimations.WalkingLeftFromFirstStride
            : AlcoonAnimations.WalkingRightFromFirstStride;
    }

    /// <summary>Whether the Alcoon faces left: it does only once it has picked the left side.</summary>
    private static bool FacesLeft(AlcoonEnemyState state) => state.WalkingDirection == AlcoonWalk.Left;

    /// <summary>An Alcoon fireball (#1760): it flies with vertical, then horizontal, room collision and horizontal drag.</summary>
    private static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyAlcoonFireball(context.Projectile, context.Level));

    /// <summary>Runs Alcoon's initialization AI.</summary>
    internal void SetUpAlcoon(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        InitializeAlcoon(place, surroundings.Level);
    }

    /// <summary>Runs Alcoon's main AI.</summary>
    internal void RunAlcoonMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunAlcoonMain(place, place.RequireSpeciesState<AlcoonEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Alcoon's typed animation.</summary>
    internal void AdvanceAlcoonTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, AlcoonAnimations.Segments,
            static (AlcoonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyAlcoonCue(occupant, place, state, context.Level, cue));

    /// <summary>Spits an Alcoon fireball with the vertical speed of <paramref name="launch"/>.</summary>
    private void SpawnAlcoonFireball(RoomEnemyOccupant occupant, RoomEnemyPlace alcoon, AlcoonFireballLaunch launch)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.AlcoonFireball, Fireball, SpriteGraphics.SpawnedBy(occupant, alcoon.Shading), new AlcoonFireballAnimation(AlcoonFireballSegment.Spinning));
        if (projectile is null)
            return;

        AlcoonEnemyState state = alcoon.RequireSpeciesState<AlcoonEnemyState>();
        bool movingLeft = FacesLeft(state);

        // The fireball takes its radii, damage, persistence, shot blocking, animation and motion from its definition
        // before its setup sets its position, to a fraction of a pixel, and its velocity.
        Offset16<RoomPixels> muzzle = AlcoonFireballLaunchDefinitions.MuzzleFacingRight;
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(alcoon.Position.Whole + (movingLeft ? muzzle.MirroredX() : muzzle));
        projectile.Velocity = projectile.Velocity.WithX(
            AlcoonFireballLaunchDefinitions.LaunchSpeed.Toward(movingLeft ? HorizontalDirection.Left : HorizontalDirection.Right));
        projectile.Velocity = projectile.Velocity.WithY(launch.YVelocity);
    }

    /// <summary>Flies the Alcoon fireball one update.</summary>
    internal static void FlyAlcoonFireball(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        // Unlike Ridley's fireball, Alcoon's fireball moves vertically first. If either move meets a block, the
        // fireball is gone at once and does not slow horizontally on that update.
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Delete();
            return;
        }

        // The speed slows toward the terminal speed and then holds it, starting at the launch speed and only moving toward
        // the terminal speed.
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
