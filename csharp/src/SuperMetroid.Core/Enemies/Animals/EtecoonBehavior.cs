using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Animation;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new EtecoonEntity(this, place, common);

    /// <summary>Probes the floor, walls, and ceiling throughout the teaching route.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Pauses animation timing while a room earthquake is active.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>Samus's passage through a door, which holds a dormant Etecoon's wake and countdown.</summary>
    private readonly DoorPassage _doorPassage;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the terrain, quake and door passage consulted by Etecoon AI.</summary>
    internal EtecoonBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, DoorPassage doorPassage, EnemyAudioQueue audio)
    {
        _audio = audio;
        _doorPassage = doorPassage;
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
    /// avoids replacing authored fixed-point values with retuned ones.</remarks>
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

    /// <summary>The room column from which on Samus hears the Etecoons' hops and wall jumps.</summary>
    private const int EtecoonAudibleFromSamusX = 256;

    /// <summary>Whether Samus stands far enough into the room to hear the Etecoons' hops and wall jumps.</summary>
    private static bool SamusHearsEtecoon(SamusState samus) =>
        !CoordinateAxis.Precedes(samus.Position.X.Whole, EtecoonAudibleFromSamusX);

    /// <summary>Initializes the Etecoon.</summary>
    internal static void InitializeEtecoon(RoomEnemyPlace place)
    {
        var state = new EtecoonEnemyState(EtecoonAction.WaitingForSamus);
        state.InstallSpawnSettings(place.SpawnSettings<EtecoonSpawnSettings>());
        place.SetSpeciesState(state);

        // The initializer sets the animating property, which the enemy animation pass reads before it plays the
        // authored animation, so the Etecoon animates whether or not its population entry set it.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        state.Playback = EnemyAnimationPlayback.Start(place, EtecoonAnimations.StartingWith(EtecoonSegment.Initial));
        state.AwaitingSamus = true;
    }

    /// <summary>Runs an Etecoon's per-frame behaviour and its actions.</summary>
    private void RunEtecoonMain(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // An earthquake suspends the Etecoon for 128 updates; its route is kept.
        if (state.QuakePauseFrames != 0)
        {
            state.QuakePauseFrames--;
            return;
        }

        switch (state.Action)
        {
            case EtecoonAction.WaitingForSamus:
                RunEtecoonWaiting(place, state, samus);
                break;
            case EtecoonAction.PreJumpCountdown:
                RunEtecoonPreJumpCountdown(place, state, samus);
                break;
            case EtecoonAction.InitialJump:
                RunEtecoonInitialJump(place, state, samus, level);
                break;
            case EtecoonAction.FaceSamusPause:
                RunEtecoonFaceSamusPause(place, state);
                break;
            case EtecoonAction.RunLeftToWall:
                RunEtecoonLeftToWall(place, state, level);
                break;
            case EtecoonAction.RunRightToWall:
                RunEtecoonRightToWall(place, state, level);
                break;
            case EtecoonAction.Airborne:
                RunEtecoonAirborne(place, state, samus, level);
                break;
            case EtecoonAction.WallPause:
                RunEtecoonWallPause(place, state);
                break;
            case EtecoonAction.RouteAfterLanding:
                RunEtecoonRouteAfterLanding(place, state);
                break;
            case EtecoonAction.MoveLeftToTeachingStart:
                RunEtecoonMoveLeftToTeachingStart(place, state, level);
                break;
            case EtecoonAction.MoveRightToTeachingStart:
                RunEtecoonMoveRightToTeachingStart(place, state, level);
                break;
            case EtecoonAction.RunRightToLongJump:
                RunEtecoonRightToLongJump(place, state, level);
                break;
            case EtecoonAction.LongJump:
                RunEtecoonLongJump(place, state, samus, level);
                break;
            case EtecoonAction.RunRightAfterLongJump:
                RunEtecoonRightAfterLongJump(place, state, level);
                break;
            case EtecoonAction.LongJumpLanding:
                RunEtecoonLongJumpLanding(place, state, samus, level);
                break;
            case EtecoonAction.TeachingJump:
                RunEtecoonTeachingJump(place, state, samus, level);
                break;
            case EtecoonAction.IdleBetweenTeachingJumps:
                RunEtecoonIdleBetweenTeachingJumps(place, state, samus);
                break;
            case EtecoonAction.WaitForSamusBeforeLongRun:
                RunEtecoonWaitForSamusBeforeLongRun(place, state, samus);
                break;
            case EtecoonAction.RunRightToReturnJump:
                RunEtecoonRightToReturnJump(place, state, level);
                break;
            case EtecoonAction.ReturnJump:
                RunEtecoonReturnJump(place, state, samus, level);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the dormant/wake loop.</summary>
    private void RunEtecoonWaiting(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        // Destination sprite drawing runs AI while door-transition audio is disabled.
        // Preserve the initial wake edge and the countdown until Samus's passage ends.
        if (_doorPassage.IsUnderWay)
            return;
        if (!state.AwaitingSamus)
        {
            if (DecrementEtecoonTimerAndTestExpired(state))
            {
                StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
                state.Action = EtecoonAction.PreJumpCountdown;
                state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
            }
            return;
        }

        if (!EtecoonIsWithinY(place, samus, EtecoonActivationYDistance))
            return;

        if (state.Route == EtecoonRoute.RunLeftToTeachingStart)
            _audio.QueueEnemySound(EtecoonWakeSound, priority: SoundRequestPriority.Essential);
        state.Playback = new(EtecoonAnimations.Flexing, AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        state.AwaitingSamus = false;
        state.ActionTimer = EtecoonTimingDefinitions.FlexDuration;
    }

    /// <summary>Expires the wake pause, installs the first jump speed, and gates its sound by Samus's X.</summary>
    private void RunEtecoonPreJumpCountdown(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        StartEtecoonAnimation(place, state, EtecoonSegment.ContinueHoppingFacingLeft);
        state.Action = EtecoonAction.InitialJump;
        if (SamusHearsEtecoon(samus))
            _audio.QueueEnemySound(EtecoonJumpSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Finishes the initial hop and either faces nearby Samus or schedules another hop.</summary>
    private void RunEtecoonInitialJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (!MoveEtecoonVertically(place, state, samus, level))
            return;

        if (state.Velocity.Y.WholePixels >= 0)
        {
            if (EtecoonIsWithinY(place, samus, EtecoonNearbyDistance) &&
                EtecoonIsWithinX(place, samus, EtecoonNearbyDistance))
            {
                if (!SamusIsToEtecoonsLeft(place, samus))
                {
                    StartEtecoonAnimation(place, state, EtecoonSegment.LookRightAtSamusAndRunLeft);
                    state.Head(side: HorizontalDirection.Left);
                }
                else
                {
                    StartEtecoonAnimation(place, state, EtecoonSegment.LookLeftAtSamusAndRunRight);
                    state.Head(side: HorizontalDirection.Right);
                }
                state.ActionTimer = EtecoonTimingDefinitions.FaceSamusPause;
                state.Action = EtecoonAction.FaceSamusPause;
            }
            else
            {
                state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
                state.Action = EtecoonAction.PreJumpCountdown;
                StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
            }
            return;
        }

        SetEtecoonVerticalVelocity(state, SignedSixteenSixteen.Zero);
        place.AnimationClock.HoldFor(EtecoonTimingDefinitions.CrestHold);
        state.Playback = EtecoonAnimations.LeftHopFromCrest;
    }

    /// <summary>Looks at Samus before running to the left or right wall, whichever way it was heading.</summary>
    private static void RunEtecoonFaceSamusPause(
        RoomEnemyPlace place,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (state.HeadingRight)
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.BeginRunningRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
            state.Action = EtecoonAction.RunRightToWall;
        }
        else
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.BeginRunningLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
            state.Action = EtecoonAction.RunLeftToWall;
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
    }

    /// <summary>Runs left until terrain collision, then reverses into the rightward teaching path.</summary>
    private void RunEtecoonLeftToWall(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(place, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
        state.Action = EtecoonAction.RunRightToWall;
        StartEtecoonAnimation(place, state, EtecoonSegment.RunningRight);
        state.Head(side: HorizontalDirection.Right);
    }

    /// <summary>Runs right until the 32-pixel solid lookahead arms the airborne wall-jump state.</summary>
    private void RunEtecoonRightToWall(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (EnemyTerrain.EnemyMeetsAttachmentBlockHorizontallyAhead(
                level,
                place,
                EtecoonRightWallProbe))
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.JumpingRight);
            state.Action = EtecoonAction.Airborne;
            return;
        }

        MoveEtecoonHorizontally(place, state, level);
    }

    /// <summary>Handles wall bounce or floor landing during the alternating teaching hops.</summary>
    private void RunEtecoonAirborne(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(place, state);
        if (MoveEtecoonHorizontally(place, state, level))
        {
            if (state.HeadingRight)
            {
                StartEtecoonAnimation(place, state, EtecoonSegment.WallJumpLeftEligible);
                state.Head(side: HorizontalDirection.Left);
            }
            else
            {
                StartEtecoonAnimation(place, state, EtecoonSegment.WallJumpRightEligible);
                state.Head(side: HorizontalDirection.Right);
            }
            state.Action = EtecoonAction.WallPause;
            state.ActionTimer = EtecoonTimingDefinitions.WallPause;
            if (SamusHearsEtecoon(samus))
                _audio.QueueEnemySound(EtecoonWallSound, priority: SoundRequestPriority.Normal);
            return;
        }

        if (!MoveEtecoonVertically(place, state, samus, level))
            return;

        StartEtecoonAnimation(
            place,
            state,
            state.HeadingRight
                ? EtecoonSegment.HoppingFacingLeft
                : EtecoonSegment.HoppingFacingRight);
        state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
        state.Action = EtecoonAction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
    }

    /// <summary>Clings to a wall, then launches in the stored opposite direction.</summary>
    private void RunEtecoonWallPause(RoomEnemyPlace place, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(place, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (state.HeadingRight)
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.WallJumpRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
        }
        else
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.WallJumpLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
        }
        state.Action = EtecoonAction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
    }

    /// <summary>Takes the authored post-landing route.</summary>
    private static void RunEtecoonRouteAfterLanding(
        RoomEnemyPlace place,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        switch (state.Route)
        {
            case EtecoonRoute.RunLeftToTeachingStart:
                state.Action = EtecoonAction.MoveLeftToTeachingStart;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningLeft), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
                break;
            case EtecoonRoute.RunRightToTeachingStart:
                state.Action = EtecoonAction.MoveRightToTeachingStart;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
                break;
            case EtecoonRoute.TeachAtOnce:
                state.Action = EtecoonAction.TeachingJump;
                // The landing hop faced away from the heading; the teaching hop continues it.
                state.Playback = new(EtecoonAnimations.StartingWith(state.HeadingRight
                    ? EtecoonSegment.ContinueHoppingFacingLeft
                    : EtecoonSegment.ContinueHoppingFacingRight), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EtecoonRoute)} {state.Route}.");
        }
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs to the left teaching marker before starting an idle hopping interval.</summary>
    private void RunEtecoonMoveLeftToTeachingStart(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (CoordinateAxis.Difference(place.Position.X.Whole, EtecoonFirstTeachingX) >= 0)
            return;

        state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
        state.Action = EtecoonAction.IdleBetweenTeachingJumps;
        StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
    }

    /// <summary>Runs to the right teaching marker before starting an idle hopping interval.</summary>
    private void RunEtecoonMoveRightToTeachingStart(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (CoordinateAxis.Difference(place.Position.X.Whole, EtecoonRunStartX) < 0)
            return;

        state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
        state.Action = EtecoonAction.IdleBetweenTeachingJumps;
        StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
    }

    /// <summary>Runs to X=600 and launches the stronger rightward demonstration jump.</summary>
    private void RunEtecoonRightToLongJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (CoordinateAxis.Difference(place.Position.X.Whole, EtecoonRunStartX) < 0)
            return;

        state.Action = EtecoonAction.LongJump;
        SetEtecoonVerticalVelocity(state, EtecoonLongJumpYVelocity);
        StartEtecoonAnimation(place, state, EtecoonSegment.JumpingRight);
    }

    /// <summary>Advances the long jump on both axes and returns to running art at X=680.</summary>
    private void RunEtecoonLongJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        MoveEtecoonVertically(place, state, samus, level);
        if (CoordinateAxis.Difference(place.Position.X.Whole, EtecoonLongJumpMidpointX) < 0)
            return;

        StartEtecoonAnimation(place, state, EtecoonSegment.RunningRight);
        state.Action = EtecoonAction.RunRightAfterLongJump;
    }

    /// <summary>Continues the long route to X=840 before installing its landing hop.</summary>
    private void RunEtecoonRightAfterLongJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (CoordinateAxis.Difference(place.Position.X.Whole, EtecoonLongJumpEndX) < 0)
            return;

        StartEtecoonAnimation(place, state, EtecoonSegment.JumpingRight);
        state.Action = EtecoonAction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, EtecoonLongJumpLandingYVelocity);
    }

    /// <summary>Waits for floor contact after the long route, then resumes the teaching-hop pause.</summary>
    private void RunEtecoonLongJumpLanding(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (!MoveEtecoonVertically(place, state, samus, level))
            return;

        state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
        StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
        state.Action = EtecoonAction.IdleBetweenTeachingJumps;
    }

    /// <summary>Handles ceiling and floor contact for a demonstration hop, selecting another hop or Samus wait.</summary>
    private void RunEtecoonTeachingJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(place, state);
        if (!MoveEtecoonVertically(place, state, samus, level))
            return;

        if (state.Velocity.Y.WholePixels >= 0)
        {
            state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
            StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
            state.Action = state.Route == EtecoonRoute.TeachAtOnce &&
                CoordinateAxis.Difference(place.Position.X.Whole, EtecoonLongRunMaximumX) < 0
                    ? EtecoonAction.WaitForSamusBeforeLongRun
                    : EtecoonAction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, SignedSixteenSixteen.Zero);
        place.AnimationClock.HoldFor(EtecoonTimingDefinitions.CrestHold);
        state.Playback = EtecoonAnimations.LeftHopFromCrest;
    }

    /// <summary>Counts teaching hops and takes the return run after four on nonlooping routes.</summary>
    private void RunEtecoonIdleBetweenTeachingJumps(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(place, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        // Four teaching hops are counted. Then clears only the count, keeping the
        // heading used by the airborne states. The count is cleared on reaching four, so it never leaves 0 through 4.
        state.TeachingHops++;
        bool fewerThanFourTeachingJumps = state.TeachingHops < EtecoonEnemyState.TeachingHopsPerLesson;
        if (!fewerThanFourTeachingJumps)
        {
            // The hop count is cleared once complete before testing for the teach-at-once route,
            // which therefore loops in groups of four rather than counting on forever.
            state.TeachingHops = 0;
        }
        if (fewerThanFourTeachingJumps || state.Route == EtecoonRoute.TeachAtOnce)
        {
            state.Action = EtecoonAction.TeachingJump;
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.ContinueHoppingFacingLeft), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Action = EtecoonAction.RunRightToReturnJump;
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity);
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        place.AnimationClock.StepOnNextUpdate();
        if (SamusHearsEtecoon(samus))
            _audio.QueueEnemySound(EtecoonJumpSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Starts the long run when Samus approaches; otherwise schedules another teaching hop.</summary>
    private void RunEtecoonWaitForSamusBeforeLongRun(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(place, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (EtecoonIsWithinY(place, samus, EtecoonNearbyDistance) &&
            EtecoonIsWithinX(place, samus, EtecoonLongRunTriggerDistance))
        {
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
            state.Action = EtecoonAction.RunRightToLongJump;
        }
        else
        {
            SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.ContinueHoppingFacingLeft), AnimationPlace.StartOfAnimation);
            state.Action = EtecoonAction.TeachingJump;
            if (SamusHearsEtecoon(samus))
                _audio.QueueEnemySound(EtecoonJumpSound, priority: SoundRequestPriority.Normal);
        }
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs to the return marker and installs the rightward jump back toward the wall route.</summary>
    private void RunEtecoonRightToReturnJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (CoordinateAxis.Difference(place.Position.X.Whole, EtecoonRunStartX) < 0)
            return;

        state.Action = EtecoonAction.ReturnJump;
        StartEtecoonAnimation(place, state, EtecoonSegment.JumpingRight);
    }

    /// <summary>Completes the return jump and restores leftward wall-running speed on landing.</summary>
    private void RunEtecoonReturnJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (!MoveEtecoonVertically(place, state, samus, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity);
        state.Action = EtecoonAction.RunLeftToWall;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity);
        StartEtecoonAnimation(place, state, EtecoonSegment.RunningLeft);
    }

    /// <summary>Makes the earthquake's parameter and timer writes that pause an Etecoon.</summary>
    private void PauseEtecoonForEarthquake(RoomEnemyPlace place, EtecoonEnemyState state)
    {
        if (!_earthquake.IsShaking)
            return;

        state.QuakePauseFrames = EtecoonQuakeDefinitions.QuakePauseFrames;
        place.AnimationClock.Extend(EtecoonQuakeDefinitions.QuakePauseFrames);
    }

    /// <summary>Moves an Etecoon horizontally by its signed 1/65536-pixel speed.</summary>
    private bool MoveEtecoonHorizontally(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level) =>
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            place,
            state.Velocity.X);

    /// <summary>Moves an Etecoon vertically, including its falling-speed cap.</summary>
    private bool MoveEtecoonVertically(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.Velocity.Y;
        // Below the cap the Etecoon falls faster by Samus's own downward acceleration.
        if (state.Velocity.Y.WholePixels < EtecoonTimingDefinitions.FallingSpeedCap)
            state.Velocity = state.Velocity.WithY(state.Velocity.Y + samus.Kinematics.YAcceleration.AsDisplacement());
        return _terrain.MoveEnemyVertically(level, place, displacement);
    }

    /// <summary>Installs a Y speed for the next movement phase.</summary>
    private static void SetEtecoonVerticalVelocity(EtecoonEnemyState state, SignedSixteenSixteen speed) =>
        state.Velocity = state.Velocity.WithY(speed);

    /// <summary>Installs an X speed for the next movement phase.</summary>
    private static void SetEtecoonHorizontalVelocity(EtecoonEnemyState state, SignedSixteenSixteen speed) =>
        state.Velocity = state.Velocity.WithX(speed);

    /// <summary>Counts one update off the current behaviour's wait and reports whether the wait is over: a wait started at N ends on its Nth update.</summary>
    private static bool DecrementEtecoonTimerAndTestExpired(EtecoonEnemyState state)
    {
        state.ActionTimer--;
        return state.ActionTimer == 0;
    }

    /// <summary>Checks strict wrapped horizontal distance from an Etecoon to Samus.</summary>
    private static bool EtecoonIsWithinX(
        RoomEnemyPlace place,
        SamusState samus,
        int distance) =>
        CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole) < distance;

    /// <summary>Checks strict wrapped vertical distance from an Etecoon to Samus.</summary>
    private static bool EtecoonIsWithinY(
        RoomEnemyPlace place,
        SamusState samus,
        int distance) =>
        CoordinateAxis.Distance(place.Position.Y.Whole, samus.Position.Y.Whole) < distance;

    /// <summary>
    /// Whether the eight-way direction from the Etecoon to Samus points left: straight left, up-left or down-left.
    /// Samus is straight left or right while she is within <see cref="EtecoonDirectionBand"/> vertically, and
    /// otherwise straight above or below, which is not left, while she is within it horizontally.
    /// </summary>
    private static bool SamusIsToEtecoonsLeft(
        RoomEnemyPlace place,
        SamusState samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole);
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole);
        bool straightAboveOrBelow = Math.Abs(deltaY) >= EtecoonDirectionBand && Math.Abs(deltaX) < EtecoonDirectionBand;
        return deltaX < 0 && !straightAboveOrBelow;
    }

    /// <summary>Restarts an authored Etecoon animation at timer one.</summary>
    private static void StartEtecoonAnimation(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        EtecoonSegment segment)
    {
        state.Playback = new(EtecoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Advances Etecoon's typed animation.</summary>
    internal static void AdvanceEtecoonTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, EtecoonAnimations.Segments,
            static (EtecoonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Etecoon animation raised {cue}."));

    /// <summary>Runs Etecoon's main AI.</summary>
    internal void RunEtecoonMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunEtecoonMain(place, place.RequireSpeciesState<EtecoonEnemyState>(), frame.Samus, frame.Level);
    }
}
