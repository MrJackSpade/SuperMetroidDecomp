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

    /// <summary>Whether Samus stands far enough into the room to hear the Etecoons' hops and wall jumps.</summary>
    private static bool SamusHearsEtecoon(SamusState samus) =>
        samus.Position.X.Whole >= EtecoonRouteDefinitions.AudibleFromSamusX;

    /// <summary>Initializes the Etecoon.</summary>
    internal static void InitializeEtecoon(RoomEnemyPlace place)
    {
        var state = new EtecoonEnemyState(EtecoonAction.WaitingForSamus);
        state.InstallSpawnSettings(place.SpawnSettings<EtecoonSpawnSettings>());
        place.SetSpeciesState(state);

        // The setup makes the Etecoon animate, whether or not its placement does.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        state.Playback = EnemyAnimationPlayback.Start(place, EtecoonAnimations.StartingWith(EtecoonSegment.StartingCrouch));
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
        // The destination room's Etecoons already update while Samus passes through the door. They neither wake nor
        // count down until her passage ends.
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

        if (!EtecoonIsWithinY(place, samus, EtecoonRouteDefinitions.ActivationYDistance))
            return;

        if (state.Route == EtecoonRoute.RunLeftToTeachingStart)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.EtecoonWakeSound, priority: SoundRequestPriority.Essential);
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

        if (state.Velocity.Y.WholePixels >= 0)
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

    /// <summary>Runs right until the 32-pixel solid lookahead arms the airborne wall-jump state.</summary>
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
            state.ActionTimer = EtecoonTimingDefinitions.WallPause;
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
        state.ActionTimer = EtecoonTimingDefinitions.HopCrouch;
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
        switch (state.Route)
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
        if (place.Position.X.Whole >= EtecoonRouteDefinitions.FirstTeachingX)
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
        if (place.Position.X.Whole < EtecoonRouteDefinitions.RunStartX)
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
        if (place.Position.X.Whole < EtecoonRouteDefinitions.RunStartX)
            return;

        state.Action = EtecoonAction.LongJump;
        SetEtecoonVerticalVelocity(state, EtecoonMotionDefinitions.LongJumpSpeed);
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
        if (place.Position.X.Whole < EtecoonRouteDefinitions.LongJumpMidpointX)
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
                place.Position.X.Whole < EtecoonRouteDefinitions.LongRunMaximumX
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
        bool fewerThanFourTeachingJumps = state.TeachingHops < EtecoonRouteDefinitions.TeachingHopsPerLesson;
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

    /// <summary>Whether Samus lies less than <paramref name="distance"/> pixels from the Etecoon horizontally, by <see cref="CoordinateAxis.Distance"/>.</summary>
    private static bool EtecoonIsWithinX(
        RoomEnemyPlace place,
        SamusState samus,
        int distance) =>
        CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole) < distance;

    /// <summary>Whether Samus lies less than <paramref name="distance"/> pixels from the Etecoon vertically, by <see cref="CoordinateAxis.Distance"/>.</summary>
    private static bool EtecoonIsWithinY(
        RoomEnemyPlace place,
        SamusState samus,
        int distance) =>
        CoordinateAxis.Distance(place.Position.Y.Whole, samus.Position.Y.Whole) < distance;

    /// <summary>
    /// Whether the eight-way direction from the Etecoon to Samus points left: straight left, up-left or down-left.
    /// Samus is straight left or right while she is within <see cref="EtecoonRouteDefinitions.DirectionBand"/> vertically, and
    /// otherwise straight above or below, which is not left, while she is within it horizontally.
    /// </summary>
    private static bool SamusIsToEtecoonsLeft(
        RoomEnemyPlace place,
        SamusState samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole);
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole);
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
