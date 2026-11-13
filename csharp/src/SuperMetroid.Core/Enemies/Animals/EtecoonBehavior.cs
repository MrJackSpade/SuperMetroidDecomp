using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The Etecoon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EtecoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Etecoon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new EtecoonEntity(this, slot, common);

    /// <summary>Probes the floor, walls, and ceiling throughout the teaching route.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Pauses animation timing while a room earthquake is active.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>Suppresses the pre-jump transition while an elevator door transition owns the room.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the terrain, quake, and elevator owners consulted by Etecoon AI.</summary>
    internal EtecoonBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, ElevatorBehavior elevatorBehavior, EnemyAudioQueue audio)
    {
        _audio = audio;
        _elevatorBehavior = elevatorBehavior;
        _terrain = terrain;
        _earthquake = earthquake;
    }

    /// <summary>Vertical distance within which Samus can wake a dormant Etecoon.</summary>
    private const int EtecoonActivationYDistance = 128;
    /// <summary>Strict proximity radius used before the initial Samus-facing pause.</summary>
    private const int EtecoonNearbyDistance = 64;
    /// <summary>Axis band used to choose cardinal rather than diagonal facing.</summary>
    private const int EtecoonDirectionBand = 32;
    /// <summary>Horizontal range that lets Samus trigger the long demonstration run.</summary>
    private const int EtecoonLongRunTriggerDistance = 48;
    /// <summary>Fixed-point lookahead distance for the right wall probe.</summary>
    private static readonly SignedSixteenSixteen EtecoonRightWallProbe = SignedSixteenSixteen.FromWholePixels(32);
    /// <summary>Room X where the left teaching route stops and begins hopping.</summary>
    private const int EtecoonFirstTeachingX = 537;
    /// <summary>Room X where the right teaching route begins a jump or return.</summary>
    private const int EtecoonRunStartX = 600;
    /// <summary>Room X where the long jump switches back to running art.</summary>
    private const int EtecoonLongJumpMidpointX = 680;
    /// <summary>Room X where the long route installs its landing jump.</summary>
    private const int EtecoonLongJumpEndX = 840;
    /// <summary>Last X that permits the alternate wait before another teaching jump.</summary>
    private const int EtecoonLongRunMaximumX = 832;

    /// <summary>Y speed for an ordinary Etecoon hop.</summary>
    /// <remarks>These are the eight authored speeds. Keeping them named beside their consumers
    /// avoids replacing authored fixed-point values with host-side tuning.</remarks>
    private static readonly SignedSixteenSixteen EtecoonJumpYVelocity = SignedSixteenSixteen.FromParts(-3, 0);
    /// <summary>Y speed for the stronger long jump.</summary>
    private static readonly SignedSixteenSixteen EtecoonLongJumpYVelocity = SignedSixteenSixteen.FromParts(-4, 0);
    /// <summary>Y speed of the hop that lands the long route.</summary>
    private static readonly SignedSixteenSixteen EtecoonLongJumpLandingYVelocity = SignedSixteenSixteen.FromParts(-1, 0);
    /// <summary>X speed of rightward teaching runs.</summary>
    private static readonly SignedSixteenSixteen EtecoonRightVelocity = SignedSixteenSixteen.FromParts(2, 0);
    /// <summary>X speed of leftward teaching runs.</summary>
    private static readonly SignedSixteenSixteen EtecoonLeftVelocity = SignedSixteenSixteen.FromParts(-2, 0);

    /// <summary>Enemy-library sound queued when an Etecoon wakes near Samus.</summary>
    private static readonly SoundEffectId EtecoonWakeSound =
        EnemySoundEffectDefinitions.EtecoonWakeSound;
    /// <summary>Enemy-library sound queued at an audible teaching hop.</summary>
    private static readonly SoundEffectId EtecoonJumpSound =
        EnemySoundEffectDefinitions.EtecoonJumpSound;
    /// <summary>Enemy-library sound queued when an airborne Etecoon meets a wall.</summary>
    private static readonly SoundEffectId EtecoonWallSound =
        EnemySoundEffectDefinitions.EtecoonWallSound;

    /// <summary>Initializes the Etecoon.</summary>
    internal static void InitializeEtecoon(RoomEnemySlot slot)
    {
        var state = new EtecoonEnemyState();
        state.InstallSpawnSettings(slot.SpawnSettings<EtecoonSpawnSettings>());
        slot.SetSpeciesState(state);

        // The initializer sets the animation-script property. The host scheduler also uses the
        // same bit to admit the authored animation script, so retain the authored
        // bit instead of inferring animation activity from the population record.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        state.Playback = EnemyAnimationPlayback.Start(slot, EtecoonAnimations.StartingWith(EtecoonSegment.Initial));
        state.Function = EtecoonAiFunction.WaitingForSamus;
        state.AwaitingSamus = true;
    }

    /// <summary>Runs an Etecoon's per-frame behaviour and its function dispatcher.</summary>
    private void RunEtecoonMain(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // An earthquake suspends the Etecoon for 128 updates; its route is kept.
        if (state.QuakePauseFrames != 0)
        {
            state.QuakePauseFrames--;
            return;
        }

        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Etecoon AI requires the active Samus actor.");
        RoomLevelData activeLevel = level ?? throw new InvalidOperationException(
            "Etecoon AI requires active room collision data.");

        switch (state.Function)
        {
            case EtecoonAiFunction.WaitingForSamus:
                RunEtecoonWaiting(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.PreJumpCountdown:
                RunEtecoonPreJumpCountdown(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.InitialJump:
                RunEtecoonInitialJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.FaceSamusPause:
                RunEtecoonFaceSamusPause(slot, state);
                break;
            case EtecoonAiFunction.RunLeftToWall:
                RunEtecoonLeftToWall(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.RunRightToWall:
                RunEtecoonRightToWall(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.Airborne:
                RunEtecoonAirborne(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.WallPause:
                RunEtecoonWallPause(slot, state);
                break;
            case EtecoonAiFunction.RouteAfterLanding:
                RunEtecoonRouteAfterLanding(slot, state);
                break;
            case EtecoonAiFunction.MoveLeftToTeachingStart:
                RunEtecoonMoveLeftToTeachingStart(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.MoveRightToTeachingStart:
                RunEtecoonMoveRightToTeachingStart(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.RunRightToLongJump:
                RunEtecoonRightToLongJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.LongJump:
                RunEtecoonLongJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.RunRightAfterLongJump:
                RunEtecoonRightAfterLongJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.LongJumpLanding:
                RunEtecoonLongJumpLanding(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.TeachingJump:
                RunEtecoonTeachingJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.IdleBetweenTeachingJumps:
                RunEtecoonIdleBetweenTeachingJumps(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.WaitForSamusBeforeLongRun:
                RunEtecoonWaitForSamusBeforeLongRun(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.RunRightToReturnJump:
                RunEtecoonRightToReturnJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.ReturnJump:
                RunEtecoonReturnJump(slot, state, activeSamus, activeLevel);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Runs the dormant/wake loop.</summary>
    private void RunEtecoonWaiting(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        // Destination sprite drawing runs AI while door-transition audio is disabled.
        // Preserve the initial wake edge and the countdown until the pause clears.
        if (_elevatorBehavior.ElevatorDoorTransitionActive)
            return;
        if (!state.AwaitingSamus)
        {
            if (DecrementEtecoonTimerAndTestExpired(state))
            {
                StartEtecoonAnimation(slot, state, EtecoonSegment.HoppingFacingLeft);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                state.FunctionTimer = WrappingCountdown.Start(11);
            }
            return;
        }

        if (!EtecoonIsWithinY(slot, samus, EtecoonActivationYDistance))
            return;

        if (state.Route == EtecoonRoute.RunLeftToTeachingStart)
            _audio.QueueEnemySound(EtecoonWakeSound, priority: SoundRequestPriority.Essential);
        StartEtecoonAnimation(slot, state, EtecoonSegment.Flexing);
        state.AwaitingSamus = false;
        state.FunctionTimer = WrappingCountdown.Start(256);
    }

    /// <summary>Expires the wake pause, installs the first jump speed, and gates its sound by Samus's X.</summary>
    private void RunEtecoonPreJumpCountdown(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        state.Playback = new(EtecoonAnimations.StartingWith(EtecoonAnimations.AfterSleep(Playing(state.Playback))), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
        state.Function = EtecoonAiFunction.InitialJump;
        if (CoordinateAxis.Difference(samus.Position.X.Whole, 256) >= 0)
            _audio.QueueEnemySound(EtecoonJumpSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Finishes the initial hop and either faces nearby Samus or schedules another hop.</summary>
    private void RunEtecoonInitialJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        if (state.Velocity.Y.WholePixels >= 0)
        {
            if (EtecoonIsWithinY(slot, samus, EtecoonNearbyDistance) &&
                EtecoonIsWithinX(slot, samus, EtecoonNearbyDistance))
            {
                if (!SamusIsToEtecoonsLeft(slot, samus))
                {
                    StartEtecoonAnimation(slot, state, EtecoonSegment.LookRightAtSamusAndRunLeft);
                    state.Head(side: HorizontalDirection.Left);
                }
                else
                {
                    StartEtecoonAnimation(slot, state, EtecoonSegment.LookLeftAtSamusAndRunRight);
                    state.Head(side: HorizontalDirection.Right);
                }
                state.FunctionTimer = WrappingCountdown.Start(32);
                state.Function = EtecoonAiFunction.FaceSamusPause;
            }
            else
            {
                state.FunctionTimer = WrappingCountdown.Start(11);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                StartEtecoonAnimation(slot, state, EtecoonSegment.HoppingFacingLeft);
            }
            return;
        }

        SetEtecoonVerticalVelocity(state, SignedSixteenSixteen.Zero);
        slot.AnimationClock.HoldFor(3);
        state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.HitCeiling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Waits 32 calls before choosing a left or right wall run from the stored facing bit.</summary>
    private static void RunEtecoonFaceSamusPause(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        state.Playback = new(EtecoonAnimations.StartingWith(EtecoonAnimations.AfterSleep(Playing(state.Playback))), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
        if (state.HeadingRight)
        {
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
            state.Function = EtecoonAiFunction.RunRightToWall;
        }
        else
        {
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
            state.Function = EtecoonAiFunction.RunLeftToWall;
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
    }

    /// <summary>Runs left until terrain collision, then reverses into the rightward teaching path.</summary>
    private void RunEtecoonLeftToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(slot, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
        state.Function = EtecoonAiFunction.RunRightToWall;
        StartEtecoonAnimation(slot, state, EtecoonSegment.RunningRight);
        state.Head(side: HorizontalDirection.Right);
    }

    /// <summary>Runs right until the 32-pixel solid lookahead arms the airborne wall-jump state.</summary>
    private void RunEtecoonRightToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (EnemyTerrain.EnemyMeetsAttachmentBlockHorizontallyAhead(
                level,
                slot,
                EtecoonRightWallProbe))
        {
            StartEtecoonAnimation(slot, state, EtecoonSegment.JumpingRight);
            state.Function = EtecoonAiFunction.Airborne;
            return;
        }

        MoveEtecoonHorizontally(slot, state, level);
    }

    /// <summary>Handles wall bounce or floor landing during the alternating teaching hops.</summary>
    private void RunEtecoonAirborne(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(slot, state);
        if (MoveEtecoonHorizontally(slot, state, level))
        {
            if (state.HeadingRight)
            {
                StartEtecoonAnimation(slot, state, EtecoonSegment.WallJumpLeftEligible);
                state.Head(side: HorizontalDirection.Left);
            }
            else
            {
                StartEtecoonAnimation(slot, state, EtecoonSegment.WallJumpRightEligible);
                state.Head(side: HorizontalDirection.Right);
            }
            state.Function = EtecoonAiFunction.WallPause;
            state.FunctionTimer = WrappingCountdown.Start(8);
            if (CoordinateAxis.Difference(samus.Position.X.Whole, 256) >= 0)
                _audio.QueueEnemySound(EtecoonWallSound, priority: SoundRequestPriority.Normal);
            return;
        }

        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        StartEtecoonAnimation(
            slot,
            state,
            state.HeadingRight
                ? EtecoonSegment.HoppingFacingLeft
                : EtecoonSegment.HoppingFacingRight);
        state.FunctionTimer = WrappingCountdown.Start(11);
        state.Function = EtecoonAiFunction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
    }

    /// <summary>Waits eight calls at a wall, then launches in the stored opposite direction.</summary>
    private void RunEtecoonWallPause(RoomEnemySlot slot, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(slot, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (state.HeadingRight)
        {
            StartEtecoonAnimation(slot, state, EtecoonSegment.WallJumpRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
        }
        else
        {
            StartEtecoonAnimation(slot, state, EtecoonSegment.WallJumpLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
        }
        state.Function = EtecoonAiFunction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
    }

    /// <summary>Takes the authored post-landing route.</summary>
    private static void RunEtecoonRouteAfterLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        switch (state.Route)
        {
            case EtecoonRoute.RunLeftToTeachingStart:
                state.Function = EtecoonAiFunction.MoveLeftToTeachingStart;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningLeft), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
                break;
            case EtecoonRoute.RunRightToTeachingStart:
                state.Function = EtecoonAiFunction.MoveRightToTeachingStart;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
                break;
            case EtecoonRoute.TeachAtOnce:
                state.Function = EtecoonAiFunction.TeachingJump;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonAnimations.AfterSleep(Playing(state.Playback))), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EtecoonRoute)} {state.Route}.");
        }
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs to the left teaching marker before starting an idle hopping interval.</summary>
    private void RunEtecoonMoveLeftToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonFirstTeachingX) >= 0)
            return;

        state.FunctionTimer = WrappingCountdown.Start(11);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        StartEtecoonAnimation(slot, state, EtecoonSegment.HoppingFacingLeft);
    }

    /// <summary>Runs to the right teaching marker before starting an idle hopping interval.</summary>
    private void RunEtecoonMoveRightToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonRunStartX) < 0)
            return;

        state.FunctionTimer = WrappingCountdown.Start(11);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        StartEtecoonAnimation(slot, state, EtecoonSegment.HoppingFacingLeft);
    }

    /// <summary>Runs to X=600 and launches the stronger rightward demonstration jump.</summary>
    private void RunEtecoonRightToLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonRunStartX) < 0)
            return;

        state.Function = EtecoonAiFunction.LongJump;
        SetEtecoonVerticalVelocity(state, EtecoonLongJumpYVelocity);
        StartEtecoonAnimation(slot, state, EtecoonSegment.JumpingRight);
    }

    /// <summary>Advances the long jump on both axes and returns to running art at X=680.</summary>
    private void RunEtecoonLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        MoveEtecoonVertically(slot, state, samus, level);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonLongJumpMidpointX) < 0)
            return;

        StartEtecoonAnimation(slot, state, EtecoonSegment.RunningRight);
        state.Function = EtecoonAiFunction.RunRightAfterLongJump;
    }

    /// <summary>Continues the long route to X=840 before installing its landing hop.</summary>
    private void RunEtecoonRightAfterLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonLongJumpEndX) < 0)
            return;

        StartEtecoonAnimation(slot, state, EtecoonSegment.JumpingRight);
        state.Function = EtecoonAiFunction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, EtecoonLongJumpLandingYVelocity);
    }

    /// <summary>Waits for floor contact after the long route, then resumes the teaching-hop pause.</summary>
    private void RunEtecoonLongJumpLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        state.FunctionTimer = WrappingCountdown.Start(11);
        StartEtecoonAnimation(slot, state, EtecoonSegment.HoppingFacingLeft);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
    }

    /// <summary>Handles ceiling and floor contact for a demonstration hop, selecting another hop or Samus wait.</summary>
    private void RunEtecoonTeachingJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(slot, state);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        if (state.Velocity.Y.WholePixels >= 0)
        {
            state.FunctionTimer = WrappingCountdown.Start(11);
            StartEtecoonAnimation(slot, state, EtecoonSegment.HoppingFacingLeft);
            state.Function = state.Route == EtecoonRoute.TeachAtOnce &&
                CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonLongRunMaximumX) < 0
                    ? EtecoonAiFunction.WaitForSamusBeforeLongRun
                    : EtecoonAiFunction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, SignedSixteenSixteen.Zero);
        slot.AnimationClock.HoldFor(3);
        state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.HitCeiling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Counts teaching hops and takes the return run after four on nonlooping routes.</summary>
    private void RunEtecoonIdleBetweenTeachingJumps(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(slot, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        // Four teaching hops are counted. Then clears only the count, keeping the
        // heading used by the airborne states. The count is cleared on reaching four, so it never leaves 0 through 4.
        state.TeachingHops++;
        bool fewerThanFourTeachingJumps = state.TeachingHops < EtecoonEnemyState.TeachingHopsPerLesson;
        if (!fewerThanFourTeachingJumps)
        {
            // The route counter is cleared once complete before testing route two.
            // Route two therefore loops in groups of four rather than overflowing forever.
            state.TeachingHops = 0;
        }
        if (fewerThanFourTeachingJumps || state.Route == EtecoonRoute.TeachAtOnce)
        {
            state.Function = EtecoonAiFunction.TeachingJump;
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonAnimations.AfterSleep(Playing(state.Playback))), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Function = EtecoonAiFunction.RunRightToReturnJump;
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        slot.AnimationClock.StepOnNextUpdate();
        if (CoordinateAxis.Difference(samus.Position.X.Whole, 256) >= 0)
            _audio.QueueEnemySound(EtecoonJumpSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Starts the long run when Samus approaches; otherwise schedules another teaching hop.</summary>
    private void RunEtecoonWaitForSamusBeforeLongRun(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(slot, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (EtecoonIsWithinY(slot, samus, EtecoonNearbyDistance) &&
            EtecoonIsWithinX(slot, samus, EtecoonLongRunTriggerDistance))
        {
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
            state.Function = EtecoonAiFunction.RunRightToLongJump;
        }
        else
        {
            SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonAnimations.AfterSleep(Playing(state.Playback))), AnimationPlace.StartOfAnimation);
            state.Function = EtecoonAiFunction.TeachingJump;
            if (CoordinateAxis.Difference(samus.Position.X.Whole, 256) >= 0)
                _audio.QueueEnemySound(EtecoonJumpSound, priority: SoundRequestPriority.Normal);
        }
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs to the return marker and installs the rightward jump back toward the wall route.</summary>
    private void RunEtecoonRightToReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, EtecoonRunStartX) < 0)
            return;

        state.Function = EtecoonAiFunction.ReturnJump;
        StartEtecoonAnimation(slot, state, EtecoonSegment.JumpingRight);
    }

    /// <summary>Completes the return jump and restores leftward wall-running speed on landing.</summary>
    private void RunEtecoonReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
        state.Function = EtecoonAiFunction.RunLeftToWall;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        StartEtecoonAnimation(slot, state, EtecoonSegment.RunningLeft);
    }

    /// <summary>Makes the earthquake's parameter and timer writes that pause an Etecoon.</summary>
    private void PauseEtecoonForEarthquake(RoomEnemySlot slot, EtecoonEnemyState state)
    {
        if (!_earthquake.IsShaking)
            return;

        state.QuakePauseFrames = EtecoonQuakeDefinitions.QuakePauseFrames;
        slot.AnimationClock.Extend(128);
    }

    /// <summary>Moves an Etecoon horizontally by its signed 16.16 speed.</summary>
    private bool MoveEtecoonHorizontally(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level) =>
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            state.Velocity.X);

    /// <summary>Moves an Etecoon vertically, including its five-pixel gravity cap.</summary>
    private bool MoveEtecoonVertically(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.Velocity.Y;
        if (state.Velocity.Y.WholePixels < 5)
        {
            AddEtecoonFixed(
                state,
                samus.Kinematics.YAcceleration.Whole,
                samus.Kinematics.YAcceleration.Fraction);
        }
        return _terrain.MoveEnemyVertically(level, slot, displacement);
    }

    /// <summary>Adds a signed 16.16 whole/fraction increment to the Etecoon's vertical speed.</summary>
    private static void AddEtecoonFixed(
        EtecoonEnemyState state,
        int wholeIncrement,
        int fractionIncrement)
    {
        SignedSixteenSixteen increment =
            SignedSixteenSixteen.FromParts(wholeIncrement, fractionIncrement);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + increment);
    }

    /// <summary>Installs a Y speed for the next movement phase.</summary>
    private static void SetEtecoonVerticalVelocity(EtecoonEnemyState state, SignedSixteenSixteen speed) =>
        state.Velocity = state.Velocity.WithY(speed);

    /// <summary>Installs an X speed for the next movement phase.</summary>
    private static void SetEtecoonHorizontalVelocity(EtecoonEnemyState state, SignedSixteenSixteen speed) =>
        state.Velocity = state.Velocity.WithX(speed);

    /// <summary>Uses post-decrement semantics, treating a one-to-zero or signed underflow as expiry.</summary>
    private static bool DecrementEtecoonTimerAndTestExpired(EtecoonEnemyState state)
    {
        CountdownStep step = state.FunctionTimer.Decrement();
        state.FunctionTimer = step.Remaining;
        return step.IsZeroOrNegative;
    }

    /// <summary>Checks strict wrapped horizontal distance from an Etecoon to Samus.</summary>
    private static bool EtecoonIsWithinX(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) < distance;

    /// <summary>Checks strict wrapped vertical distance from an Etecoon to Samus.</summary>
    private static bool EtecoonIsWithinY(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        CoordinateAxis.Distance(slot.Position.Y.Whole, samus.Position.Y.Whole) < distance;

    /// <summary>
    /// Whether the eight-way direction from the Etecoon to Samus points left: straight left, up-left or down-left.
    /// Samus is straight left or right while she is within <see cref="EtecoonDirectionBand"/> vertically, and
    /// otherwise straight above or below, which is not left, while she is within it horizontally.
    /// </summary>
    private static bool SamusIsToEtecoonsLeft(
        RoomEnemySlot slot,
        SamusState samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole);
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole);
        bool straightAboveOrBelow = Math.Abs(deltaY) >= EtecoonDirectionBand && Math.Abs(deltaX) < EtecoonDirectionBand;
        return deltaX < 0 && !straightAboveOrBelow;
    }

    /// <summary>Restarts an authored Etecoon animation at timer one.</summary>
    private static void StartEtecoonAnimation(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        EtecoonSegment segment)
    {
        state.Playback = new(EtecoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>The segment <paramref name="playback"/> is playing: the one its current phase plays, as a sleep rests in it.</summary>
    private static EtecoonSegment Playing(AnimationPlayback<EtecoonSegment> playback) =>
        playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>Advances Etecoon's typed animation.</summary>
    internal static void AdvanceEtecoonTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, EtecoonAnimations.Segments,
            static (EtecoonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Etecoon animation raised {cue}."));

    /// <summary>Runs Etecoon's main AI.</summary>
    internal void RunEtecoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunEtecoonMain(slot, slot.RequireSpeciesState<EtecoonEnemyState>(), frame.Samus, frame.Level);
    }
}
