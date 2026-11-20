using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
[RoomLifetime]
internal sealed class PhantoonPatterns
{
    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Phantoon's fades, health colors, wave amplitude and animations, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _phantoonPresentation;
    /// <summary>Phantoon's combat, whose flame behaviours the destroyable flames are bound to.</summary>
    private readonly PhantoonCombat _combat;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonPatterns(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyFrameContext frameContext, PhantoonPresentation phantoonPresentation,
        PhantoonCombat combat)
    {
        _combat = combat;
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _frameContext = frameContext;
        _phantoonPresentation = phantoonPresentation;
    }

    /// <summary>Chooses the first-round movement and flame pattern from encounter state.</summary>
    internal void RunPhantoonPickFirstRoundPattern(
        PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        state.Wave.Length = null;
        state.Round.EyeClosedWait.LastFor(PhantoonTimerDefinitions.EyeClosed[PhantoonCadenceDefinitions.FirstEyeClosedChoice.Of(_frameContext.FrameCounter)]);
        state.Phase = PhantoonPhase.MoveInFigureEightThenOpenEye;
        state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
        if (PhantoonRandomDefinitions.FigureEightRunsReversed.IsTakenBy(_room.Random.Generate()))
        {
            state.FigureEight.Speed = PhantoonPathSpeed.Zero;
            state.FigureEight.Direction = PhantoonPathDirection.Backwards;
            state.FigureEight.PathPlace = PhantoonPath.At(PhantoonPathDefinitions.LastPlace);
        }
        else
        {
            state.FigureEight.Speed = PhantoonMotionDefinitions.FigureEightStartSpeed;
            state.FigureEight.Direction = PhantoonPathDirection.Forwards;
            state.FigureEight.PathPlace = PhantoonPath.At(0);
        }
    }

    /// <summary>Moves the body through its first-round figure-eight and schedules its next phase.</summary>
    internal void RunPhantoonFirstRoundFigureEight(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        StepPhantoonFigureEight(body, state.FigureEight);
        StepPhantoonCasualFlameSchedule(state);

        if (!state.Round.EyeClosedWait.StepRunsOut())
            return;

        state.Phase = PhantoonPhase.WaitingOnEye;
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeOpen);
        state.NextPattern = PhantoonPatternChoice.FigureEight;
        SpawnPhantoonSpiralFlames(state);
    }

    /// <summary>Moves his body along its figure-eight path by the figure-eight's speed.</summary>
    private static void StepPhantoonFigureEight(RoomEnemyPlace body, PhantoonFigureEightState figureEight)
    {
        bool backwards = figureEight.Direction == PhantoonPathDirection.Backwards;
        if (backwards)
            AdjustPhantoonReverseFigureEightSpeed(figureEight);
        else
            AdjustPhantoonForwardFigureEightSpeed(figureEight);

        int steps = figureEight.Speed.WholePlacesAlong(figureEight.Direction);
        for (int step = 0; step < steps; step++)
        {
            Offset16<RoomPixels> pathStep = figureEight.PathPlace.Entry;
            if (backwards)
            {
                body.Position -= pathStep;
                figureEight.PathPlace = figureEight.PathPlace.PreviousInCycle;
            }
            else
            {
                body.Position += pathStep;
                figureEight.PathPlace = figureEight.PathPlace.NextInCycle;
            }
        }
    }

    /// <summary>Changes horizontal velocity over the forward half of a figure-eight.</summary>
    private static void AdjustPhantoonForwardFigureEightSpeed(PhantoonFigureEightState figureEight)
    {
        if (figureEight.Stage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.SlowSpeedChange);
            if (figureEight.Speed.WholePlaces >= PhantoonMotionDefinitions.ForwardSlowCap)
            {
                figureEight.Speed = PhantoonMotionDefinitions.ForwardSurgeStart;
                figureEight.Stage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (figureEight.Stage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange);
            if (figureEight.Speed.WholePlaces >= PhantoonMotionDefinitions.ForwardFastCap)
            {
                figureEight.Speed = PhantoonPathSpeed.FromPlaces(PhantoonMotionDefinitions.ForwardFastCap);
                figureEight.Stage = PhantoonFigureEightStage.Settling;
            }
            return;
        }

        ChangePhantoonSpeed(figureEight, -PhantoonMotionDefinitions.FastSpeedChange);
        if (figureEight.Speed.WholePlaces <= PhantoonMotionDefinitions.ForwardMinimum)
        {
            figureEight.Speed = PhantoonMotionDefinitions.ForwardEaseStart;
            figureEight.Stage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Changes horizontal velocity over the returning half of a figure-eight.</summary>
    private static void AdjustPhantoonReverseFigureEightSpeed(PhantoonFigureEightState figureEight)
    {
        if (figureEight.Stage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(figureEight, -PhantoonMotionDefinitions.SlowSpeedChange);
            if (figureEight.Speed.WholePlaces <= PhantoonMotionDefinitions.ReverseSlowCap)
            {
                figureEight.Speed = PhantoonMotionDefinitions.ReverseSurgeStart;
                figureEight.Stage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (figureEight.Stage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(figureEight, -PhantoonMotionDefinitions.FastSpeedChange);
            if (figureEight.Speed.WholePlaces <= PhantoonMotionDefinitions.ReverseFastCap)
            {
                figureEight.Speed = PhantoonMotionDefinitions.ReverseSettleStart;
                figureEight.Stage = PhantoonFigureEightStage.Settling;
            }
            return;
        }

        ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange);
        int maximum = PhantoonMotionDefinitions.ReverseMaximum;
        if (figureEight.Speed.WholePlaces >= maximum)
        {
            figureEight.Speed = PhantoonPathSpeed.FromPlaces(maximum);
            figureEight.Stage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Changes the figure-eight's speed by <paramref name="change"/>, a change below none where it slows.</summary>
    private static void ChangePhantoonSpeed(PhantoonFigureEightState figureEight, PhantoonPathSpeed change) =>
        figureEight.Speed += change;

    /// <summary>Advances the casual-flame burst while he runs his figure-eight.</summary>
    private void StepPhantoonCasualFlameSchedule(PhantoonEnemyState state)
    {
        if (!state.CasualFlames.Hold.StepRunsOut())
            return;

        if (state.CasualFlames.NextFlame is { } flame)
        {
            // The mouth spits the flame; the next waits its own wait, and once the last is spat the burst ends and its
            // cooldown starts.
            ListStep<PhantoonCasualFlame>? next = flame.Following;
            state.CasualFlames.NextFlame = next;
            state.CasualFlames.Hold.LastFor(next is { } following ? following.Entry.WaitBeforeUpdates : state.CasualFlames.Schedule.Cooldown);
            state.MouthPart.StartAnimation(PhantoonPartSegment.MouthSpawnFlame);
            return;
        }

        PhantoonCasualFlameSchedule burst = PhantoonCasualFlameDefinitions.Burst.Choose(_room.Random.Generate());
        ListStep<PhantoonCasualFlame> first = ListStep<PhantoonCasualFlame>.FirstOf(burst.Flames);
        state.CasualFlames.Schedule = burst;
        state.CasualFlames.NextFlame = first;
        state.CasualFlames.Hold.LastFor(first.Entry.WaitBeforeUpdates);
    }

    /// <summary>Starts the spiral-flame pattern around the body.</summary>
    /// <remarks>The spiral fireballs are the first ordinary combat attack: eight destroyable flames, spawned in their
    /// authored order round the body.</remarks>
    private void SpawnPhantoonSpiralFlames(PhantoonEnemyState state)
    {
        foreach (TrigonometryAngle start in PhantoonFlameSpawns.SpiralStarts)
            SpawnPhantoonSpiralFlame(state, start);
    }

    /// <summary>Runs the eye-open vulnerable window.</summary>
    internal static void RunPhantoonEyeTracking(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        if (state.PhaseWait.StepRunsOut())
        {
            // The round's damage is the damage accumulated during this
            // opening. Clears it before deciding whether a shot requested a swoop.
            state.Round.Damage = 0;
            if (state.WindowEnd != PhantoonWindowEnd.Swoop)
            {
                state.Phase = PhantoonPhase.WaitingOnEye;
                state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
                state.EyePart.StartAnimation(PhantoonPartSegment.EyeCloseAndPickNewPattern);
                body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

                // This flag makes the next pattern choice take the initial flame-rain
                // branch after the eye-close animation runs its flame-rain cue.
                state.NextPattern = PhantoonPatternChoice.FlameRain;
                return;
            }

            state.WindowEnd = PhantoonWindowEnd.EyeCloses;
            state.PhaseWait.LastFor(PhantoonArenaDefinitions.BeforeSwoopUpdates);
            state.Phase = PhantoonPhase.BecomeSolidAndSwoop;
        }

        PointPhantoonEyeAtSamus(body, state, samus);
    }

    /// <summary>Runs the one-frame opaque-swoop setup.</summary>
    internal static void BeginPhantoonVulnerableSwoop(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        state.Translucent = false;
        BeginPhantoonSwoop(state);
        state.BodyPart.StartAnimation(PhantoonPartSegment.FullHitboxBody);
    }

    /// <summary>Runs the 360-frame opaque swoop.</summary>
    internal static void RunPhantoonSwoop(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.AboveSamus);
        if (!state.PhaseWait.StepRunsOut())
            return;

        state.Phase = PhantoonPhase.FadeOutWhileSwooping;
        state.Translucent = true;
        state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyFade.Resume();
        state.Round.Damage = 0;
    }

    /// <summary>Fades the body while continuing its swoop across the room.</summary>
    internal void RunPhantoonFadeOutWhileSwooping(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.AboveSamus);
        AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (!state.BodyFade.IsFinished)
            return;

        state.Phase = PhantoonPhase.WaitAfterFadeOut;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.HiddenAfterFadeOutUpdates);
    }

    /// <summary>Counts the hidden interval before reappearing.</summary>
    internal static void RunPhantoonHiddenWait(PhantoonEnemyState state)
    {
        if (state.PhaseWait.StepRunsOut())
            state.Phase = PhantoonPhase.PickNextAppearance;
    }

    /// <summary>Runs the hidden left/right placement and round-two picker.</summary>
    internal void PlacePhantoonForNextFigureEight(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (PhantoonRandomDefinitions.FigureEightStartsRight.IsTakenBy(_room.Random.Generate()))
        {
            state.FigureEight.PathPlace = PhantoonPath.At(PhantoonPathDefinitions.RightAppearance);
            body.Position = body.Position.WithWholeX(PhantoonPathDefinitions.RightAppearancePosition.X);
        }
        else
        {
            state.FigureEight.PathPlace = PhantoonPath.At(PhantoonPathDefinitions.LeftAppearance);
            body.Position = body.Position.WithWholeX(PhantoonPathDefinitions.LeftAppearancePosition.X);
        }
        body.Position = body.Position.WithWholeY(PhantoonPathDefinitions.AppearanceY);
        state.FigureEight.Direction = PhantoonPathDirection.Forwards;
        state.FigureEight.Speed = PhantoonMotionDefinitions.FigureEightStartSpeed;
        state.NextPattern = PhantoonPatternChoice.FigureEight;

        // The round's timers and figure-eight direction are the shared pattern choice's; the phase is the fade-in's.
        ApplyPhantoonSecondRoundSettings(state);
        state.Phase = PhantoonPhase.FadeInBeforeFigureEight;
        state.BodyFade.Resume();
    }

    /// <summary>Reveals Phantoon before the next figure-eight pass.</summary>
    internal void RunPhantoonFadeInBeforeFigureEight(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (state.BodyFade.IsFinished)
            state.Phase = PhantoonPhase.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Runs the visible setup for every flame-rain window.</summary>
    internal static void BeginPhantoonFlameRainVulnerableWindow(PhantoonEnemyState state)
    {
        state.BodyFade.Resume();
        state.Translucent = false;
        state.BodyPart.StartAnimation(PhantoonPartSegment.FullHitboxBody);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeballCentered);
        state.Phase = PhantoonPhase.FadeInDuringFlameRain;
    }

    /// <summary>Reveals the body while preparing the flame-rain attack.</summary>
    internal void RunPhantoonFlameRainFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.FlameRainFadeInSteps);
        if (!state.BodyFade.IsFinished)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Phase = PhantoonPhase.TrackSamusDuringFlameRain;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.FlameRainTrackingUpdates);
    }

    /// <summary>Counts the open-eye interval during flame rain.</summary>
    internal static void RunPhantoonFlameRainVulnerableWindow(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        state.Round.Damage = 0;
        if (state.WindowEnd == PhantoonWindowEnd.Swoop)
        {
            state.WindowEnd = PhantoonWindowEnd.EyeCloses;
            state.NextPattern = PhantoonPatternChoice.FlameRain;
            BeginPhantoonSwoop(state);
            return;
        }

        state.Phase = PhantoonPhase.FadeOutDuringFlameRain;
        state.BodyFade.Resume();
        state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.Translucent = true;
    }

    /// <summary>Hides the body after the vulnerable rain interval.</summary>
    internal void RunPhantoonFlameRainFadeOut(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (!state.BodyFade.IsFinished)
            return;

        state.Phase = PhantoonPhase.SpawnFlameRain;
        state.PhaseWait.LastFor(PhantoonRandomDefinitions.RainHiding.Choose(_room.Random.Generate()));
    }

    /// <summary>Emits the scheduled rain pattern while Phantoon is hidden.</summary>
    internal void RunPhantoonHiddenFlameRain(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        PhantoonRainPattern pattern = PhantoonRainPatternDefinitions.LaterRain.Choose(_room.Random.Generate());
        state.FigureEight.PathPlace = PhantoonPath.At(pattern.PathPlace);
        body.Position = body.Position.WithWhole(pattern.BodyPosition);
        state.FigureEight.Direction = PhantoonPathDirection.Forwards;
        state.Phase = PhantoonPhase.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    /// <summary>Starts the first rain pattern after the intro round.</summary>
    internal void RunPhantoonInitialFlameRain(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        StepPhantoonFigureEight(body, state.FigureEight);
        StepPhantoonCasualFlameSchedule(state);
        if (!state.Round.EyeClosedWait.StepRunsOut())
            return;

        state.WindowEnd = PhantoonWindowEnd.EyeCloses;
        state.Phase = PhantoonPhase.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.Position.X.Whole < PhantoonArenaDefinitions.MiddleX
            ? PhantoonRainPatternDefinitions.FirstRainFromLeftHalf
            : PhantoonRainPatternDefinitions.FirstRainFromRightHalf);
    }

    /// <summary>Hides the body before entering the enraged attack cycle.</summary>
    internal void RunPhantoonFadeOutBeforeRage(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (!state.BodyFade.IsFinished)
            return;
        state.Phase = PhantoonPhase.MoveToTopCenterForRage;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.BeforeRageUpdates);
    }

    /// <summary>Holds the hidden rage phase until its wait's last update.</summary>
    internal static void RunPhantoonRageHiddenWait(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;
        state.Phase = PhantoonPhase.FadeInForRage;
        body.Position = body.Position.WithWhole(PhantoonArenaDefinitions.RageStart);
        state.BodyFade.Resume();
    }

    /// <summary>Reveals the body at the start of the enraged swoop.</summary>
    internal void RunPhantoonRageFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (!state.BodyFade.IsFinished)
            return;
        state.Phase = PhantoonPhase.Enraged;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.FirstRageWaveUpdates);
        state.BodyFade.Resume();
    }

    /// <summary>Runs all eight alternating radial flame waves.</summary>
    internal void RunPhantoonRage(PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        // Alternating waves contain seven and eight flames, not seven each.
        IReadOnlyList<PhantoonRageFlameStart> wave = PhantoonCadenceDefinitions.OddRageWave.IsSetIn(state.RageWaveCount)
            ? PhantoonFlameSpawns.OddRageWave
            : PhantoonFlameSpawns.EvenRageWave;
        foreach (PhantoonRageFlameStart start in wave)
            SpawnPhantoonRageFlame(state, start);

        _audio.QueueEnemySound(PhantoonRageDefinitions.WaveSound, PhantoonRageDefinitions.WaveSoundPriority);
        state.RageWaveCount++;
        if (state.RageWaveCount < PhantoonRageDefinitions.WaveCount)
        {
            state.PhaseWait.LastFor(PhantoonRageDefinitions.WaveInterval);
            return;
        }

        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
        state.RageWaveCount = 0;
        state.Phase = PhantoonPhase.FadeOutAfterRage;
    }

    /// <summary>Hides the body after the enraged swoop.</summary>
    internal void RunPhantoonRageFadeOut(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (!state.BodyFade.IsFinished)
            return;
        state.Phase = PhantoonPhase.WaitAfterFadeOut;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.HiddenAfterFadeOutUpdates);
    }

    /// <summary>Seeds swoop speed and direction from Samus's current position.</summary>
    private static void BeginPhantoonSwoop(
        PhantoonEnemyState state)
    {
        state.Swoop.Velocity = new(PhantoonArenaDefinitions.SwoopStartSpeed, PhantoonArenaDefinitions.SwoopStartSpeed);
        state.Swoop.TargetX = 0;
        state.Swoop.TargetReturning = false;
        state.Phase = PhantoonPhase.Swooping;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.SwoopUpdates);
    }

    /// <summary>
    /// Moves Phantoon in a swoop. Its speed limits are not symmetric: the negative limits differ from the positive ones,
    /// and the swoop's path follows from both.
    /// </summary>
    internal static void MovePhantoonInSwoop(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus,
        PhantoonSwoopTarget target)
    {
        PhantoonSwoopState swoop = state.Swoop;
        // The target sweeps out to its turn and back to zero, where it starts out again.
        if (swoop.TargetReturning)
        {
            swoop.TargetX -= PhantoonArenaDefinitions.SwoopTargetStep;
            if (swoop.TargetX == 0)
                swoop.TargetReturning = false;
        }
        else
        {
            swoop.TargetX += PhantoonArenaDefinitions.SwoopTargetStep;
            if (swoop.TargetX >= PhantoonMotionDefinitions.SwoopTargetTurnX)
                swoop.TargetReturning = true;
        }
        int targetX = swoop.TargetX;

        // Each pull stops past its threshold, so the swoop's speeds stay within about eight pixels an update either way.
        // The swoop holds him as far as 64 pixels left of the room, so his place and its bounds are compared across the
        // room's left edge, and a target above Samus can lie above the room's top.
        SignedEightEight swoopX = swoop.Velocity.X;
        if (DistanceAcrossRoomEdge.LiesBefore(targetX, body.Position.X.Whole))
        {
            if (swoopX >= PhantoonMotionDefinitions.SwoopNegativeXThreshold)
                swoopX -= PhantoonMotionDefinitions.SwoopHorizontalPull;
        }
        else if (swoopX < PhantoonMotionDefinitions.SwoopPositiveXLimit)
        {
            swoopX += PhantoonMotionDefinitions.SwoopHorizontalPull;
        }
        swoop.Velocity = swoop.Velocity.WithX(swoopX);
        body.Position = body.Position.AdvanceX(swoop.Velocity.X.ToSixteenSixteenDelta());
        if (DistanceAcrossRoomEdge.LiesBefore(body.Position.X.Whole, PhantoonMotionDefinitions.SwoopMinimumX))
            body.Position = OffRoomPlaces.WithWholeX(body.Position, PhantoonMotionDefinitions.SwoopMinimumX);
        else if (!DistanceAcrossRoomEdge.LiesBefore(body.Position.X.Whole, PhantoonMotionDefinitions.SwoopMaximumX))
            body.Position = body.Position.WithWholeX(PhantoonMotionDefinitions.SwoopMaximumX);

        int targetY = target == PhantoonSwoopTarget.DeathHeight ? PhantoonArenaDefinitions.DeathSwoopY : OffRoomPlaces.Place(samus.Position.Y.Whole - PhantoonArenaDefinitions.SwoopAboveSamus);
        SignedEightEight swoopY = swoop.Velocity.Y;
        if (DistanceAcrossRoomEdge.LiesBefore(targetY, body.Position.Y.Whole))
        {
            if (swoopY >= PhantoonMotionDefinitions.SwoopNegativeYThreshold)
                swoopY -= PhantoonMotionDefinitions.SwoopVerticalPull;
        }
        else if (swoopY < PhantoonMotionDefinitions.SwoopPositiveYLimit)
        {
            swoopY += PhantoonMotionDefinitions.SwoopVerticalPull;
        }
        swoop.Velocity = swoop.Velocity.WithY(swoopY);
        body.Position = body.Position.AdvanceY(swoop.Velocity.Y.ToSixteenSixteenDelta());
        if (DistanceAcrossRoomEdge.LiesBefore(body.Position.Y.Whole, PhantoonMotionDefinitions.SwoopMinimumY))
            body.Position = body.Position.WithWholeY(PhantoonMotionDefinitions.SwoopMinimumY);
        else if (!DistanceAcrossRoomEdge.LiesBefore(body.Position.Y.Whole, PhantoonMotionDefinitions.SwoopMaximumY))
            body.Position = body.Position.WithWholeY(PhantoonMotionDefinitions.SwoopMaximumY);
    }

    /// <summary>Selects the eye pose and angle that track Samus from the current body position.</summary>
    internal static void PointPhantoonEyeAtSamus(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        Offset16<RoomPixels> toSamus = samus.Position.Whole - body.Position.Whole;
        CompassDirection direction;
        if (Math.Abs(toSamus.Y) < PhantoonArenaDefinitions.EyeGazeStraightReach)
            direction = toSamus.X < 0 ? CompassDirection.Left : CompassDirection.Right;
        else if (Math.Abs(toSamus.X) < PhantoonArenaDefinitions.EyeGazeStraightReach)
            direction = toSamus.Y < 0 ? CompassDirection.Up : CompassDirection.Down;
        else if (toSamus.X < 0)
            direction = toSamus.Y < 0 ? CompassDirection.UpLeft : CompassDirection.DownLeft;
        else
            direction = toSamus.Y < 0 ? CompassDirection.UpRight : CompassDirection.DownRight;

        state.EyePart.StartAnimation(PhantoonEyeGaze.EyeAnimation(direction));
    }

    /// <summary>Fades his body toward his faded-out colours over <paramref name="steps"/> steps, one every second update.</summary>
    internal void AdvancePhantoonFadeOut(
        PhantoonEnemyState state,
        int steps)
    {
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.BodyFade.IsFinished)
            return;

        // The fade's step is a typed fade toward his faded-out colours (#1512, #2251).
        if (state.BodyFade.TakeStep(steps) is { } step)
            state.Colours.Fade(PhantoonBodyLook.FadedOut, step, steps);
        else
            state.Colours.CompleteFade();
    }

    /// <summary>Places the rain's hanging flames in rotating room columns, each falling a hang step after the last.</summary>
    private void SpawnPhantoonFlameRain(RoomEnemyPlace body, PhantoonRainPattern pattern)
    {
        ListStep<int> column = pattern.FirstColumn;
        int hangUpdates = PhantoonArenaDefinitions.RainFallDelayStep;
        for (int flame = 0; flame < PhantoonArenaDefinitions.RainFlameCount; flame++)
        {
            SpawnPhantoonRainFlame(body, column.Entry, hangUpdates);
            column = column.NextInCycle;
            hangUpdates += PhantoonArenaDefinitions.RainFallDelayStep;
        }
    }

    /// <summary>Selects a later-round flame and swoop pattern from encounter state.</summary>
    internal void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state)
    {
        ApplyPhantoonSecondRoundSettings(state);
        state.Phase = state.NextPattern == PhantoonPatternChoice.FlameRain
            ? PhantoonPhase.FadeOutBeforeFirstFlameRain
            : PhantoonPhase.MoveInFigureEightThenOpenEye;
        if (state.NextPattern == PhantoonPatternChoice.FlameRain)
            state.BodyFade.Resume();
    }

    /// <summary>
    /// Sets the round's pattern timer and eye-closed wait, drawing the wait from the room's random numbers, and turns the
    /// figure-eight forwards or backwards by the frame counter.
    /// </summary>
    private void ApplyPhantoonSecondRoundSettings(PhantoonEnemyState state)
    {
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.PatternPickUpdates);
        state.Round.EyeClosedWait.LastFor(PhantoonRandomDefinitions.EyeClosed.Choose(_room.Random.Generate()));
        if (PhantoonCadenceDefinitions.FigureEightReverses.IsSetIn(_frameContext.FrameCounter))
        {
            if (state.FigureEight.Direction == PhantoonPathDirection.Forwards)
                state.FigureEight.PathPlace = state.FigureEight.PathPlace.PreviousInCycle;
            state.FigureEight.Speed = PhantoonPathSpeed.Zero;
            state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
            state.FigureEight.Direction = PhantoonPathDirection.Backwards;
        }
        else
        {
            if (state.FigureEight.Direction == PhantoonPathDirection.Backwards)
                state.FigureEight.PathPlace = state.FigureEight.PathPlace.NextInCycle;
            state.FigureEight.Speed = PhantoonMotionDefinitions.FigureEightStartSpeed;
            state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
            state.FigureEight.Direction = PhantoonPathDirection.Forwards;
        }
    }

    /// <summary>Initializes a casual flame, which falls from the mouth harmless until it strikes the floor.</summary>
    /// <param name="body">Phantoon's body.</param>
    internal void SpawnPhantoonCasualFlame(RoomEnemyPlace body)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return;
        flame.Velocity = flame.Velocity.WithX(SignedEightEight.Zero);
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.MouthOffset);
        flame.StartAnimation(new PhantoonFlameAnimation(_combat.DestroyableFlame, PhantoonFlameSegment.CasualFallingKindling));
        flame.StartMotion(_combat.DestroyableFlame.CasualFalling);

        // The falling flame's step changes the spawn definition's harmful, shootable properties to harmless ones.
        // The falling flame therefore cannot hurt Samus and cannot be shot until
        // it has struck the floor and changed into the bouncing form.
        flame.CanDamageSamus = false;
        flame.BlocksSamusProjectiles = false;
    }

    /// <summary>Initializes an enraged flame, which circles the body from its start as its circle widens.</summary>
    /// <param name="phantoon">The encounter, whose body the flame circles.</param>
    /// <param name="start">Where on its circle the flame starts and which way it circles.</param>
    private void SpawnPhantoonRageFlame(PhantoonEnemyState phantoon, PhantoonRageFlameStart start)
    {
        RoomEnemyPlace body = phantoon.Body;
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return;
        _combat.DestroyableFlame.Orbits.Start(flame, new PhantoonOrbitingFlameState(phantoon, start.Angle)
        {
            TurnPerUpdate = PhantoonFlameSpawns.RageTurnPerUpdate(start),
        });
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.MouthOffset);
        flame.StartMotion(_combat.DestroyableFlame.RageOrbiting);
    }

    /// <summary>Initializes a spiral flame, which circles the body clockwise from its start as its circle widens.</summary>
    /// <param name="phantoon">The encounter, whose body the flame circles.</param>
    /// <param name="start">The flame's starting angle round the body.</param>
    private void SpawnPhantoonSpiralFlame(PhantoonEnemyState phantoon, TrigonometryAngle start)
    {
        RoomEnemyPlace body = phantoon.Body;
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return;
        _combat.DestroyableFlame.Orbits.Start(flame, new PhantoonOrbitingFlameState(phantoon, start));
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.FlameCircleCentre);
        flame.StartMotion(_combat.DestroyableFlame.SpiralOrbiting);
    }

    /// <summary>Initializes a rain flame over a room column, falling after it hangs.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="columnX">The screen X of the rain column the flame falls in (<see cref="PhantoonFlameSpawnDefinitions.RainXs"/>).</param>
    /// <param name="hangUpdates">The updates from the flame's first to its fall, the flame hanging on every earlier one.</param>
    private void SpawnPhantoonRainFlame(RoomEnemyPlace body, int columnX, int hangUpdates)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return;
        _combat.DestroyableFlame.Rains.Start(flame, new PhantoonRainFlameState(hangUpdates));
        flame.Position = flame.Position.WithWholeX(columnX);
        flame.Position = flame.Position.WithWholeY(PhantoonArenaDefinitions.RainStartY);
        flame.StartMotion(_combat.DestroyableFlame.RainFalling);
    }

    /// <summary>Allocates a destroyable flame and gives it the idle animation and a still, whole-pixel start.</summary>
    /// <returns>The flame, or null when every enemy projectile place is in use.</returns>
    private EnemyProjectile? AllocateDestroyableFlame(RoomEnemyPlace body)
    {
        EnemyProjectile? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            _combat.DestroyableFlame,
            SpriteGraphics.SpawnedBy(body));
        flame.StartAnimation(new PhantoonFlameAnimation(_combat.DestroyableFlame, PhantoonFlameSegment.Burning));
        flame.Position = flame.Position.WithoutFractions();
        flame.Velocity = flame.Velocity.WithY(SignedEightEight.Zero);
        return flame;
    }
}
