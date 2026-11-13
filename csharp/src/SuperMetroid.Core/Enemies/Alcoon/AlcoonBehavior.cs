using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new AlcoonEntity(this, slot, common);

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
    private AlcoonSegment? ApplyAlcoonCue(RoomEnemySlot slot, AlcoonEnemyState state, RoomLevelData? level, AlcoonCue cue)
    {
        switch (cue.Kind)
        {
            case AlcoonCueKind.FireUpward:
                SpawnAlcoonFireball(slot, AlcoonFireballLaunchDefinitions.Upward);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            case AlcoonCueKind.FireHorizontally:
                SpawnAlcoonFireball(slot, AlcoonFireballLaunchDefinitions.Level);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            case AlcoonCueKind.FireDownward:
                SpawnAlcoonFireball(slot, AlcoonFireballLaunchDefinitions.Downward);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            case AlcoonCueKind.StartWalking:
                return StartAlcoonWalking(state);
            case AlcoonCueKind.Step:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                return MoveAlcoonHorizontally(slot, state, level);
            case AlcoonCueKind.Move:
                return MoveAlcoonHorizontally(slot, state, level);
            default:
                throw new InvalidOperationException($"Undefined {nameof(AlcoonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Maximum horizontal distance for activation, after the landing-height check.</summary>
    private const int AlcoonEmergeXDistance = 80;
    /// <summary>Distance from spawn that causes an emerged Alcoon to jump back underground.</summary>
    private const int AlcoonHideXDistance = 112;
    /// <summary>Guards the initializer's simulated ascent and floor search against invalid room geometry.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Initializes the Alcoon, including its floor search.</summary>
    private void InitializeAlcoon(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Alcoon initialization requires the room's collision layer to simulate its landing.");
        }

        var state = new AlcoonEnemyState();
        slot.SetSpeciesState(state);

        state.StepCounter = 0;
        state.SpawnPosition = slot.Position.Whole;
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        state.Playback = new(AlcoonAnimations.StartingWith(AlcoonSegment.WalkingLeft), AnimationPlace.StartOfAnimation);
        state.Action = AlcoonAction.WaitingForSamus;
        SetupAlcoonJumpMovement(state);

        // The initializer really executes every half-pixel-accelerated ascent
        // step, then every collision-aware descent step. It ultimately restores only the
        // whole Y coordinate; the collision-produced subposition deliberately survives.
        int iterations = 0;
        do
        {
            AddAlcoonYVelocity(slot, state);
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }
        while (state.Velocity.Y.WholePixels < 0);

        while (true)
        {
            SignedSixteenSixteen displacement = state.Velocity.Y;
            if (_terrain.MoveEnemyVertically(level, slot, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }

        state.LandingPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithWholeY(state.SpawnPosition.Y);
    }

    /// <summary>Runs Alcoon's per-frame behaviour through all seven of its functions.</summary>
    private void RunAlcoonMain(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Action)
        {
            case AlcoonAction.WaitingForSamus:
                WaitForAlcoonActivation(slot, state, samus);
                return;
            case AlcoonAction.EmergingRising:
                RunAlcoonEmergingRise(slot, state);
                return;
            case AlcoonAction.EmergingFalling:
                RunAlcoonEmergingFall(slot, state, samus, level);
                return;
            case AlcoonAction.WalkingAndFiring:
                RunEmergedAlcoon(slot, state, samus, level);
                return;
            case AlcoonAction.WaitingForFireAnimation:
                return;
            case AlcoonAction.HidingRising:
                RunAlcoonHidingRise(slot, state);
                return;
            case AlcoonAction.HidingFalling:
                RunAlcoonHidingFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Alcoon function {state.Action?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Starts Alcoon's jump when Samus nears its predicted landing point.</summary>
    private void WaitForAlcoonActivation(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon proximity AI requires the active Samus actor.");

        if (CoordinateAxis.Distance(state.LandingPosition.Y, samus.Position.Y.Whole) >= 32)
            return;

        int signedXDistance = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole);
        if (Math.Abs(signedXDistance) >= AlcoonEmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (signedXDistance < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            StartAlcoonAnimation(slot, state, AlcoonSegment.AirborneLeftLookingUp);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            StartAlcoonAnimation(slot, state, AlcoonSegment.AirborneRightLookingUp);
        }
        state.Action = AlcoonAction.EmergingRising;
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.AlcoonEmergeSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Integrates the emerging ascent until vertical velocity ceases to be negative.</summary>
    private static void RunAlcoonEmergingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels < 0)
            return;

        state.Action = AlcoonAction.EmergingFalling;
        StartAlcoonAnimation(
            slot,
            state,
            state.Velocity.X.WholePixels < 0
                ? AlcoonSegment.AirborneLeftLookingForward
                : AlcoonSegment.AirborneRightLookingForward);
    }

    /// <summary>Applies collision-aware descent, then chooses the initial walking direction toward Samus.</summary>
    private void RunAlcoonEmergingFall(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon landing direction requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon falling AI requires room collision data.");

        SignedSixteenSixteen displacement = state.Velocity.Y;
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            AccelerateAlcoonY(state);
            return;
        }

        if (CoordinateAxis.Precedes(samus.Position.X.Whole, slot.Position.X.Whole))
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            StartAlcoonAnimation(slot, state, AlcoonSegment.WalkingLeft);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            StartAlcoonAnimation(slot, state, AlcoonSegment.WalkingRight);
        }
        state.Action = AlcoonAction.WalkingAndFiring;
        state.StepCounter = 1;
    }

    /// <summary>Maintains floor contact, starts a retreat at the spawn-distance limit, or begins a forward volley.</summary>
    private void RunEmergedAlcoon(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon firing AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon walking AI requires room collision data.");

        // This is a real two-pixel downward move, not merely a floor probe. On ordinary
        // floors it collides and keeps Alcoon aligned; if the floor vanishes it lets the
        // walking actor fall by the same amount before the remainder of main AI runs.
        _terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(2));

        if (CoordinateAxis.Distance(state.SpawnPosition.X, slot.Position.X.Whole) >= AlcoonHideXDistance)
        {
            state.Action = AlcoonAction.HidingRising;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.FromParts(-4, 0));
            StartAlcoonAnimation(
                slot,
                state,
                state.Velocity.X.WholePixels < 0
                    ? AlcoonSegment.AirborneLeftLookingForward
                    : AlcoonSegment.AirborneRightLookingForward);
            return;
        }

        if (state.StepCounter != 0)
            return;

        bool samusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
        bool movingLeft = state.Velocity.X.WholePixels < 0;
        if (samusIsLeft != movingLeft)
            return;

        StartAlcoonAnimation(
            slot,
            state,
            movingLeft
                ? AlcoonSegment.FireLeft
                : AlcoonSegment.FireRight);
        state.Action = AlcoonAction.WaitingForFireAnimation;
    }

    /// <summary>Starts the retreat arc upward until gravity reverses its Y velocity.</summary>
    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels >= 0)
            state.Action = AlcoonAction.HidingFalling;
    }

    /// <summary>Returns the retreating actor to its whole-pixel spawn position while retaining subpositions.</summary>
    private static void RunAlcoonHidingFall(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, state.SpawnPosition.Y) < 0)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // The reset snaps only whole coordinates. Both subpositions and
        // the last animation map remain live while the actor waits underground.
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.Action = AlcoonAction.WaitingForSamus;
    }

    /// <summary>Resets the jump to a twelve-pixel upward velocity and half-pixel acceleration.</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.Velocity = new FixedVector<RoomPixels>(
            SignedSixteenSixteen.Zero, SignedSixteenSixteen.FromParts(-12, 0));
        state.YAcceleration = SignedSixteenSixteen.FromParts(0, 32768);
    }

    /// <summary>Adds the per-slot 16.16 acceleration to Alcoon's vertical velocity.</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + state.YAcceleration);
    }

    /// <summary>Integrates vertical velocity without collision during airborne ascent or retreat.</summary>
    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.Velocity.Y);
    }

    /// <summary>Rejects an initializer that cannot find a landing floor within the simulation bound.</summary>
    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemySlot slot)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon slot {slot.SlotIndex} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    /// <summary>Switches Alcoon's animation so its first step runs on the next update.</summary>
    private static void StartAlcoonAnimation(RoomEnemySlot slot, AlcoonEnemyState state, AlcoonSegment segment)
    {
        state.Playback = new(AlcoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the Alcoon's start-walking cue and returns the walk the animation continues at.</summary>
    private AlcoonSegment StartAlcoonWalking(AlcoonEnemyState state)
    {
        state.Action = AlcoonAction.WalkingAndFiring;
        int count = AlcoonWalkDefinitions.StepCount.Of(_room.Random.Read());
        state.StepCounter = count == 0 ? 2 : count;
        return state.Velocity.X.WholePixels < 0
            ? AlcoonSegment.WalkingLeft
            : AlcoonSegment.WalkingRight;
    }

    /// <summary>
    /// Implements both movement cues: the animation runs on unless a wall turns the Alcoon
    /// round, when it continues at the other walk's first pose.
    /// </summary>
    private AlcoonSegment? MoveAlcoonHorizontally(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Alcoon movement instruction requires room collision data.");

        SignedSixteenSixteen displacement = state.Velocity.X;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            return null;
        }

        int oldVelocity = state.Velocity.X.WholePixels;
        state.Velocity = state.Velocity.WithWholeX(-oldVelocity);
        return oldVelocity >= 0
            ? AlcoonSegment.WalkingLeftLoop
            : AlcoonSegment.WalkingRightLoop;
    }

    /// <summary>Connects Alcoon fireballs to their movement frame behaviour.</summary>
    internal static void AddAlcoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunAlcoonFireballFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.AlcoonFireballFlight);
    }

    /// <summary>Runs Alcoon's initialization AI.</summary>
    internal void RunAlcoonInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeAlcoon(slot, surroundings.Level);
    }

    /// <summary>Runs Alcoon's main AI.</summary>
    internal void RunAlcoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunAlcoonMain(slot, slot.RequireSpeciesState<AlcoonEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Alcoon's typed animation.</summary>
    internal void AdvanceAlcoonTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(slot, AlcoonAnimations.Segments,
            static (AlcoonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyAlcoonCue(slot, state, context.Level, cue) is { } walk
                ? new AnimationContinuation<AlcoonSegment>(AlcoonAnimations.StartingWith(walk), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Initial horizontal fireball speed, four pixels per update.</summary>
    private static readonly SignedEightEight AlcoonFireballGraphicsHorizontalSpeed = SignedEightEight.FromParts(4, 0);
    /// <summary>Horizontal speed at which fireball deceleration stops.</summary>
    private static readonly SignedEightEight AlcoonFireballTerminalHorizontalSpeed = SignedEightEight.FromParts(2, 0);
    /// <summary>Horizontal speed reduction applied per fireball update, a quarter pixel per update.</summary>
    private static readonly SignedEightEight AlcoonFireballHorizontalDeceleration = SignedEightEight.FromParts(0, 64);

    /// <summary>Spits an Alcoon fireball with the vertical speed of <paramref name="launch"/>.</summary>
    internal void SpawnAlcoonFireball(RoomEnemySlot alcoon, AlcoonFireballLaunch launch)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        AlcoonEnemyState state = alcoon.RequireSpeciesState<AlcoonEnemyState>();
        bool movingLeft = state.Velocity.X.WholePixels < 0;

        // Spawning copies the fireball's complete authored
        // definition before its initializer touches position, subposition, and velocity.
        // Keeping that division exact makes radii, damage, persistence, shot blocking,
        // animation, and frame behaviour owned by the definition instead of duplicating them here.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.AlcoonFireball,
            SpriteGraphics.SpawnedBy(alcoon));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(alcoon.Position.X.Whole + (movingLeft ? -16 : 16)), CoordinateAxis.Wrap(alcoon.Position.Y.Whole - 12)));
        projectile.Velocity = projectile.Velocity.WithX(movingLeft
            ? -AlcoonFireballGraphicsHorizontalSpeed
            : AlcoonFireballGraphicsHorizontalSpeed);
        projectile.Velocity = projectile.Velocity.WithY(launch.YVelocity);
    }

    /// <summary>Runs the Alcoon fireball's per-frame behaviour.</summary>
    internal static void RunAlcoonFireballFrameBehavior(
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

        // The speed slows toward the terminal speed and then holds it; neither step leaves the 8.8 range, as the speed
        // starts at the launch speed and only moves toward the terminal speed.
        SignedEightEight velocity = projectile.Velocity.X;
        if (velocity.IsNegative)
        {
            velocity += AlcoonFireballHorizontalDeceleration;
            if (velocity >= -AlcoonFireballTerminalHorizontalSpeed)
                velocity = -AlcoonFireballTerminalHorizontalSpeed;
        }
        else
        {
            velocity -= AlcoonFireballHorizontalDeceleration;
            if (velocity < AlcoonFireballTerminalHorizontalSpeed)
                velocity = AlcoonFireballTerminalHorizontalSpeed;
        }
        projectile.Velocity = projectile.Velocity.WithX(velocity);
    }
}
