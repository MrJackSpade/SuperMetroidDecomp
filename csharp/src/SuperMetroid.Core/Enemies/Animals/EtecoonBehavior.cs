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
internal sealed class EtecoonBehavior : IEnemySpecies, IEnemySpawnFactory<EtecoonSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Etecoon];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EtecoonSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new EtecoonEntity(this, occupant, place, common, settings));

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

    /// <summary>Whether Samus stands far enough into the room to hear the Etecoons' hops and wall jumps.</summary>
    private static bool SamusHearsEtecoon(SamusState samus) =>
        samus.Position.X.Whole >= EtecoonRouteDefinitions.AudibleFromSamusX;

    /// <summary>Initializes the Etecoon.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static EtecoonEnemyState InitializeEtecoon(RoomEnemyOccupant occupant, EtecoonSpawnSettings etecoonSpawnSettings, RoomEnemyPlace place)
    {
        // The placement names the route, which the Etecoon keeps for when it takes it (#2531).
        var state = new EtecoonEnemyState(EtecoonAction.Dormant, etecoonSpawnSettings.Route);

        // The setup makes the Etecoon animate, whether or not its placement does.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.PointAtOrigin;
        state.Playback = EnemyAnimationPlayback.Start(place, EtecoonAnimations.StartingWith(EtecoonSegment.StartingCrouch));
        return state;
    }

    /// <summary>Runs an Etecoon for one update and its actions.</summary>
    private void RunEtecoonMain(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // An earthquake suspends the Etecoon for its quake pause; its route is kept.
        if (state.QuakePauseFrames.IsRunning)
        {
            state.QuakePauseFrames.Count();
            return;
        }

        switch (state.Action)
        {
            case EtecoonAction.Dormant:
                RunEtecoonDormant(place, state, samus);
                break;
            case EtecoonAction.Flexing:
                RunEtecoonFlexing(place, state);
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

    /// <summary>Wakes the dormant Etecoon once Samus comes level with it, starting its flex.</summary>
    private void RunEtecoonDormant(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        // The destination room's Etecoons already update while Samus passes through the door. They do not wake until her
        // passage ends.
        if (_doorPassage.IsUnderWay)
            return;
        if (!EtecoonIsWithinY(place, samus, EtecoonRouteDefinitions.ActivationYDistance))
            return;

        if (state.Route == EtecoonRoute.RunLeftToTeachingStart)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EtecoonWakeSound, priority: SoundRequestPriority.Essential);
        state.Playback = new(EtecoonAnimations.Flexing, AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
        state.Action = EtecoonAction.Flexing;
        state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.FlexDuration);
    }

    /// <summary>Counts the flex down, then crouches for the Etecoon's first hop.</summary>
    private void RunEtecoonFlexing(RoomEnemyPlace place, EtecoonEnemyState state)
    {
        // A door passage holds the countdown, as it holds the wake.
        if (_doorPassage.IsUnderWay)
            return;
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
        state.Action = EtecoonAction.PreJumpCountdown;
        state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
    }

    /// <summary>Waits out the hop crouch, then starts the hop: its first jump speed, and its sound gated by Samus's X.</summary>
    private void RunEtecoonPreJumpCountdown(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
        StartEtecoonAnimation(place, state, EtecoonSegment.ContinueHoppingFacingLeft);
        state.Action = EtecoonAction.InitialJump;
        if (SamusHearsEtecoon(samus))
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EtecoonJumpSound, priority: SoundRequestPriority.Normal);
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

        if (!state.Velocity.Y.IsNegative)
        {
            if (EtecoonIsWithinY(place, samus, EtecoonRouteDefinitions.NearbyDistance) &&
                EtecoonIsWithinX(place, samus, EtecoonRouteDefinitions.NearbyDistance))
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
                state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.FaceSamusPause);
                state.Action = EtecoonAction.FaceSamusPause;
            }
            else
            {
                state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
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

        if (state.Heading == HorizontalDirection.Right)
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.BeginRunningRight);
            SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.RightRunSpeed);
            state.Action = EtecoonAction.RunRightToWall;
        }
        else
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.BeginRunningLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.LeftRunSpeed);
            state.Action = EtecoonAction.RunLeftToWall;
        }
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
    }

    /// <summary>Runs left until terrain collision, then reverses into the rightward teaching path.</summary>
    private void RunEtecoonLeftToWall(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(place, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.RightRunSpeed);
        state.Action = EtecoonAction.RunRightToWall;
        StartEtecoonAnimation(place, state, EtecoonSegment.RunningRight);
        state.Head(side: HorizontalDirection.Right);
    }

    /// <summary>Runs right until the solid lookahead arms the airborne wall-jump state.</summary>
    private void RunEtecoonRightToWall(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (EnemyTerrain.EnemyMeetsAttachmentBlockHorizontallyAhead(
                level,
                place,
                EtecoonMotionDefinitions.RightWallLookahead))
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
            if (state.Heading == HorizontalDirection.Right)
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
            state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.WallPause);
            if (SamusHearsEtecoon(samus))
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.EtecoonWallSound, priority: SoundRequestPriority.Normal);
            return;
        }

        if (!MoveEtecoonVertically(place, state, samus, level))
            return;

        StartEtecoonAnimation(
            place,
            state,
            state.Heading == HorizontalDirection.Right
                ? EtecoonSegment.HoppingFacingLeft
                : EtecoonSegment.HoppingFacingRight);
        state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
        state.Action = EtecoonAction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
    }

    /// <summary>Clings to a wall, then launches in the stored opposite direction.</summary>
    private void RunEtecoonWallPause(RoomEnemyPlace place, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(place, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (state.Heading == HorizontalDirection.Right)
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.WallJumpRight);
            SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.RightRunSpeed);
        }
        else
        {
            StartEtecoonAnimation(place, state, EtecoonSegment.WallJumpLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.LeftRunSpeed);
        }
        state.Action = EtecoonAction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
    }

    /// <summary>Takes the authored post-landing route.</summary>
    private static void RunEtecoonRouteAfterLanding(
        RoomEnemyPlace place,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
        EtecoonRoute route = state.Route;
        switch (route)
        {
            case EtecoonRoute.RunLeftToTeachingStart:
                state.Action = EtecoonAction.MoveLeftToTeachingStart;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningLeft), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.LeftRunSpeed);
                break;
            case EtecoonRoute.RunRightToTeachingStart:
                state.Action = EtecoonAction.MoveRightToTeachingStart;
                state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.RightRunSpeed);
                break;
            case EtecoonRoute.TeachAtOnce:
                state.Action = EtecoonAction.TeachingJump;
                // The landing hop faced away from the heading; the teaching hop continues it.
                state.Playback = new(EtecoonAnimations.StartingWith(state.Heading == HorizontalDirection.Right
                    ? EtecoonSegment.ContinueHoppingFacingLeft
                    : EtecoonSegment.ContinueHoppingFacingRight), AnimationPlace.StartOfAnimation);
                SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.RightRunSpeed);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EtecoonRoute)} {route}.");
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
        if (place.Position.X.Whole >= EtecoonRouteDefinitions.FirstTeachingX)
            return;

        state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
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
        if (place.Position.X.Whole < EtecoonRouteDefinitions.RunStartX)
            return;

        state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
        state.Action = EtecoonAction.IdleBetweenTeachingJumps;
        StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
    }

    /// <summary>Runs to <see cref="EtecoonRouteDefinitions.RunStartX"/> and launches the stronger rightward demonstration jump.</summary>
    private void RunEtecoonRightToLongJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (place.Position.X.Whole < EtecoonRouteDefinitions.RunStartX)
            return;

        state.Action = EtecoonAction.LongJump;
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.LongJumpSpeed);
        StartEtecoonAnimation(place, state, EtecoonSegment.JumpingRight);
    }

    /// <summary>Advances the long jump on both axes and returns to running art at <see cref="EtecoonRouteDefinitions.LongJumpMidpointX"/>.</summary>
    private void RunEtecoonLongJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        MoveEtecoonVertically(place, state, samus, level);
        if (place.Position.X.Whole < EtecoonRouteDefinitions.LongJumpMidpointX)
            return;

        StartEtecoonAnimation(place, state, EtecoonSegment.RunningRight);
        state.Action = EtecoonAction.RunRightAfterLongJump;
    }

    /// <summary>Continues the long route to <see cref="EtecoonRouteDefinitions.LongJumpEndX"/> before installing its landing hop.</summary>
    private void RunEtecoonRightAfterLongJump(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(place, state, level);
        if (place.Position.X.Whole < EtecoonRouteDefinitions.LongJumpEndX)
            return;

        StartEtecoonAnimation(place, state, EtecoonSegment.JumpingRight);
        state.Action = EtecoonAction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.LongJumpLandingSpeed);
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

        state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
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

        if (!state.Velocity.Y.IsNegative)
        {
            state.ActionTimer = UpdateCountdown.For(EtecoonTimingDefinitions.HopCrouch);
            StartEtecoonAnimation(place, state, EtecoonSegment.HoppingFacingLeft);
            state.Action = state.Route == EtecoonRoute.TeachAtOnce &&
                place.Position.X.Whole < EtecoonRouteDefinitions.LongRunMaximumX
                    ? EtecoonAction.WaitForSamusBeforeLongRun
                    : EtecoonAction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, SignedSixteenSixteen.Zero);
        place.AnimationClock.HoldFor(EtecoonTimingDefinitions.CrestHold);
        state.Playback = EtecoonAnimations.LeftHopFromCrest;
    }

    /// <summary>Counts teaching hops and takes the return run after a lesson's hops on nonlooping routes.</summary>
    private void RunEtecoonIdleBetweenTeachingJumps(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(place, state);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        // Each hop counts towards the lesson. When the lesson's hops are done the count empties, and only the count:
        // the heading the airborne states use stays.
        state.TeachingHops++;
        bool lessonContinues = state.TeachingHops < EtecoonRouteDefinitions.TeachingHopsPerLesson;
        if (!lessonContinues)
        {
            // The count empties before the teach-at-once route is tested, so that route hops lesson after lesson.
            state.TeachingHops = 0;
        }
        if (lessonContinues || state.Route == EtecoonRoute.TeachAtOnce)
        {
            state.Action = EtecoonAction.TeachingJump;
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.ContinueHoppingFacingLeft), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Action = EtecoonAction.RunRightToReturnJump;
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
            SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.RightRunSpeed);
        }
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
        place.AnimationClock.StepOnNextUpdate();
        if (SamusHearsEtecoon(samus))
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EtecoonJumpSound, priority: SoundRequestPriority.Normal);
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

        if (EtecoonIsWithinY(place, samus, EtecoonRouteDefinitions.NearbyDistance) &&
            EtecoonIsWithinX(place, samus, EtecoonRouteDefinitions.LongRunTriggerDistance))
        {
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.RunningRight), AnimationPlace.StartOfAnimation);
            state.Action = EtecoonAction.RunRightToLongJump;
        }
        else
        {
            SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
            state.Playback = new(EtecoonAnimations.StartingWith(EtecoonSegment.ContinueHoppingFacingLeft), AnimationPlace.StartOfAnimation);
            state.Action = EtecoonAction.TeachingJump;
            if (SamusHearsEtecoon(samus))
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.EtecoonJumpSound, priority: SoundRequestPriority.Normal);
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
        if (place.Position.X.Whole < EtecoonRouteDefinitions.RunStartX)
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

        SetEtecoonHorizontalVelocity(state, EtecoonMotionDefinitions.LeftRunSpeed);
        state.Action = EtecoonAction.RunLeftToWall;
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.HopSpeed);
        StartEtecoonAnimation(place, state, EtecoonSegment.RunningLeft);
    }

    /// <summary>Pauses an Etecoon and its animation while the earthquake shakes the room.</summary>
    private void PauseEtecoonForEarthquake(RoomEnemyPlace place, EtecoonEnemyState state)
    {
        if (!_earthquake.IsShaking)
            return;

        state.QuakePauseFrames = UpdateCountdown.Lasting(EtecoonQuakeDefinitions.QuakePauseFrames);
        place.AnimationClock.Extend(EtecoonQuakeDefinitions.QuakePauseFrames);
    }

    /// <summary>Moves an Etecoon horizontally by its speed.</summary>
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
        if (state.Velocity.Y < EtecoonTimingDefinitions.FallingSpeedCap)
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
        return state.ActionTimer.Count();
    }

    /// <summary>Whether Samus lies less than <paramref name="distance"/> pixels from the Etecoon horizontally.</summary>
    private static bool EtecoonIsWithinX(
        RoomEnemyPlace place,
        SamusState samus,
        int distance) =>
        Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) < distance;

    /// <summary>Whether Samus lies less than <paramref name="distance"/> pixels from the Etecoon vertically.</summary>
    private static bool EtecoonIsWithinY(
        RoomEnemyPlace place,
        SamusState samus,
        int distance) =>
        Math.Abs(place.Position.Y.Whole - samus.Position.Y.Whole) < distance;

    /// <summary>
    /// Whether the eight-way direction from the Etecoon to Samus points left: straight left, up-left or down-left.
    /// Samus is straight left or right while she is within <see cref="EtecoonRouteDefinitions.DirectionBand"/> vertically, and
    /// otherwise straight above or below, which is not left, while she is within it horizontally.
    /// </summary>
    private static bool SamusIsToEtecoonsLeft(
        RoomEnemyPlace place,
        SamusState samus)
    {
        int deltaX = samus.Position.X.Whole - place.Position.X.Whole;
        int deltaY = samus.Position.Y.Whole - place.Position.Y.Whole;
        bool straightAboveOrBelow = Math.Abs(deltaY) >= EtecoonRouteDefinitions.DirectionBand && Math.Abs(deltaX) < EtecoonRouteDefinitions.DirectionBand;
        return deltaX < 0 && !straightAboveOrBelow;
    }

    /// <summary>Starts the Etecoon's animation at <paramref name="segment"/>, showing its first frame on the next update.</summary>
    private static void StartEtecoonAnimation(
        RoomEnemyPlace place,
        EtecoonEnemyState state,
        EtecoonSegment segment)
    {
        state.Playback = new(EtecoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Advances Etecoon's typed animation.</summary>
    internal static void AdvanceEtecoonTypedAnimation(RoomEnemyPlace place, EtecoonEnemyState etecoonEnemyState) =>
        EnemyAnimationPlayback.Advance(place, etecoonEnemyState, EtecoonAnimations.Segments,
            static (EtecoonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Etecoon animation raised {cue}."));

    /// <summary>Runs Etecoon's main AI.</summary>
    internal void RunEtecoonMainAi(RoomEnemyPlace place, EtecoonEnemyState etecoonEnemyState, IEnemyAiFrame frame)
    {
        RunEtecoonMain(place, etecoonEnemyState, frame.Samus, frame.Level);
    }
}
