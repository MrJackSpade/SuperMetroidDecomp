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
    /// <summary>Phantoon's fades, health colors, wave amplitude, animations and phase timer, shared by every phase of the fight.</summary>
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
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        state.Wave.Length = null;
        state.Round.EyeClosedTimer = PhantoonTimerDefinitions.EyeClosed[PhantoonCadenceDefinitions.FirstEyeClosedChoice.Of(_frameContext.FrameCounter)];
        state.Phase = PhantoonPhase.MoveInFigureEightThenOpenEye;
        state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
        if (PhantoonRandomDefinitions.FigureEightReversed.IsSetIn(_room.Random.Generate()))
        {
            state.FigureEight.Speed = SignedSixteenSixteen.Zero;
            state.FigureEight.Reversed = true;
            state.FigureEight.PathPlace = PhantoonPath.At(PhantoonPathDefinitions.LastPlace);
        }
        else
        {
            state.FigureEight.Speed = PhantoonMotionDefinitions.FigureEightStartSpeed;
            state.FigureEight.Reversed = false;
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

        if (!state.Round.StepEyeClosedTimerRunsOut())
            return;

        state.Phase = PhantoonPhase.WaitingOnEye;
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeOpen);
        state.FlameRainRequested = false;
        SpawnPhantoonSpiralFlames(state);
    }

    /// <summary>Moves his body along its figure-eight path by the figure-eight's speed.</summary>
    internal static void StepPhantoonFigureEight(RoomEnemyPlace body, PhantoonFigureEightState figureEight)
    {
        if (figureEight.Reversed)
            AdjustPhantoonReverseFigureEightSpeed(figureEight);
        else
            AdjustPhantoonForwardFigureEightSpeed(figureEight);

        int steps = figureEight.Reversed
            ? -figureEight.Speed.WholePixels
            : figureEight.Speed.WholePixels;
        for (int step = 0; step < steps; step++)
        {
            Offset16<RoomPixels> pathStep = figureEight.PathPlace.Entry;
            if (figureEight.Reversed)
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
    internal static void AdjustPhantoonForwardFigureEightSpeed(PhantoonFigureEightState figureEight)
    {
        if (figureEight.Stage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.SlowSpeedChange, PhantoonSpeedChange.Accelerate);
            if (figureEight.Speed.WholePixels >= PhantoonMotionDefinitions.ForwardSlowCap)
            {
                figureEight.Speed = PhantoonMotionDefinitions.ForwardSurgeStart;
                figureEight.Stage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (figureEight.Stage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange, PhantoonSpeedChange.Accelerate);
            if (figureEight.Speed.WholePixels >= PhantoonMotionDefinitions.ForwardFastCap)
            {
                figureEight.Speed = SignedSixteenSixteen.FromParts(PhantoonMotionDefinitions.ForwardFastCap, 0);
                figureEight.Stage = PhantoonFigureEightStage.Settling;
            }
            return;
        }

        ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange, PhantoonSpeedChange.Decelerate);
        if (figureEight.Speed.WholePixels <= PhantoonMotionDefinitions.ForwardMinimum)
        {
            figureEight.Speed = PhantoonMotionDefinitions.ForwardEaseStart;
            figureEight.Stage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Changes horizontal velocity over the returning half of a figure-eight.</summary>
    internal static void AdjustPhantoonReverseFigureEightSpeed(PhantoonFigureEightState figureEight)
    {
        if (figureEight.Stage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.SlowSpeedChange, PhantoonSpeedChange.Decelerate);
            if (figureEight.Speed.WholePixels <= PhantoonMotionDefinitions.ReverseSlowCap)
            {
                figureEight.Speed = PhantoonMotionDefinitions.ReverseSurgeStart;
                figureEight.Stage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (figureEight.Stage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange, PhantoonSpeedChange.Decelerate);
            if (figureEight.Speed.WholePixels <= PhantoonMotionDefinitions.ReverseFastCap)
            {
                figureEight.Speed = PhantoonMotionDefinitions.ReverseSettleStart;
                figureEight.Stage = PhantoonFigureEightStage.Settling;
            }
            return;
        }

        ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange, PhantoonSpeedChange.Accelerate);
        int maximum = PhantoonMotionDefinitions.ReverseMaximum;
        if (figureEight.Speed.WholePixels >= maximum)
        {
            figureEight.Speed = SignedSixteenSixteen.FromParts(maximum, 0);
            figureEight.Stage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Adds a speed change to the figure-eight's speed, or subtracts it when slowing.</summary>
    private static void ChangePhantoonSpeed(PhantoonFigureEightState figureEight, SignedSixteenSixteen change, PhantoonSpeedChange direction)
    {
        figureEight.Speed = direction == PhantoonSpeedChange.Decelerate
            ? figureEight.Speed - change
            : figureEight.Speed + change;
    }

    /// <summary>Advances the casual-flame burst while he runs his figure-eight.</summary>
    internal void StepPhantoonCasualFlameSchedule(PhantoonEnemyState state)
    {
        if (--state.CasualFlames.Hold > 0)
            return;

        if (state.CasualFlames.NextFlame is { } flame)
        {
            // The mouth spits the flame; the next waits its own wait, and once the last is spat the burst ends and its
            // cooldown starts.
            ListStep<PhantoonCasualFlame>? next = flame.Following;
            state.CasualFlames.NextFlame = next;
            state.CasualFlames.Hold = next is { } following ? following.Entry.WaitBeforeUpdates : state.CasualFlames.Schedule.Cooldown;
            state.MouthPart.StartAnimation(PhantoonPartSegment.MouthSpawnFlame);
            return;
        }

        PhantoonCasualFlameSchedule burst = Ensure.Element(PhantoonCasualFlameDefinitions.Choices, _room.Random.Next(PhantoonRandomDefinitions.CasualFlamePattern));
        ListStep<PhantoonCasualFlame> first = ListStep<PhantoonCasualFlame>.FirstOf(burst.Flames);
        state.CasualFlames.Schedule = burst;
        state.CasualFlames.NextFlame = first;
        state.CasualFlames.Hold = first.Entry.WaitBeforeUpdates;
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
        if (PhantoonPresentation.TickPhantoonPhaseTimer(state))
        {
            // The round's damage is the damage accumulated during this
            // opening. Clears it before deciding whether a shot requested a swoop.
            state.Round.Damage = 0;
            if (!state.Swoop.Requested)
            {
                state.Phase = PhantoonPhase.WaitingOnEye;
                state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
                state.EyePart.StartAnimation(PhantoonPartSegment.EyeCloseAndPickNewPattern);
                body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

                // This flag makes the next pattern choice take the initial flame-rain
                // branch after the eye-close animation runs its flame-rain cue.
                state.FlameRainRequested = true;
                return;
            }

            state.Swoop.Requested = false;
            state.PhaseTimer = PhantoonArenaDefinitions.BeforeSwoopUpdates;
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
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        state.Phase = PhantoonPhase.FadeOutWhileSwooping;
        state.Translucent = true;
        state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyFade.Complete = false;
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
        if (!state.BodyFade.Complete)
            return;

        state.Phase = PhantoonPhase.WaitAfterFadeOut;
        state.PhaseTimer = PhantoonArenaDefinitions.HiddenAfterFadeOutUpdates;
    }

    /// <summary>Counts the hidden interval before reappearing.</summary>
    internal static void RunPhantoonHiddenWait(PhantoonEnemyState state)
    {
        if (PhantoonPresentation.TickPhantoonPhaseTimer(state))
            state.Phase = PhantoonPhase.PickNextAppearance;
    }

    /// <summary>Runs the hidden left/right placement and round-two picker.</summary>
    internal void PlacePhantoonForNextFigureEight(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (PhantoonRandomDefinitions.FigureEightStartsRight.IsSetIn(_room.Random.Generate()))
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
        state.FigureEight.Reversed = false;
        state.FigureEight.Speed = PhantoonMotionDefinitions.FigureEightStartSpeed;
        state.FlameRainRequested = false;

        // The call deliberately starts a pattern phase which is immediately
        // overwritten with the fade-in phase. Its timer, direction, and movement-side
        // effects remain live, so run the shared pattern choice in full.
        PickPhantoonSecondRoundPattern(state);
        state.Phase = PhantoonPhase.FadeInBeforeFigureEight;
        state.BodyFade.Complete = false;
    }

    /// <summary>Reveals Phantoon before the next figure-eight pass.</summary>
    internal void RunPhantoonFadeInBeforeFigureEight(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (state.BodyFade.Complete)
            state.Phase = PhantoonPhase.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Runs the visible setup for every flame-rain window.</summary>
    internal static void BeginPhantoonFlameRainVulnerableWindow(PhantoonEnemyState state)
    {
        state.BodyFade.Complete = false;
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
        if (!state.BodyFade.Complete)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Phase = PhantoonPhase.TrackSamusDuringFlameRain;
        state.PhaseTimer = PhantoonArenaDefinitions.FlameRainTrackingUpdates;
    }

    /// <summary>Counts the open-eye interval during flame rain.</summary>
    internal static void RunPhantoonFlameRainVulnerableWindow(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        state.Round.Damage = 0;
        if (state.Swoop.Requested)
        {
            state.Swoop.Requested = false;
            state.FlameRainRequested = true;
            BeginPhantoonSwoop(state);
            return;
        }

        state.Phase = PhantoonPhase.FadeOutDuringFlameRain;
        state.BodyFade.Complete = false;
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
        if (!state.BodyFade.Complete)
            return;

        state.Phase = PhantoonPhase.SpawnFlameRain;
        state.PhaseTimer = PhantoonTimerDefinitions.RainHiding[_room.Random.Next(PhantoonRandomDefinitions.RainHidingChoice)];
    }

    /// <summary>Emits the scheduled rain pattern while Phantoon is hidden.</summary>
    internal void RunPhantoonHiddenFlameRain(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        PhantoonRainPattern pattern = Ensure.Element(PhantoonRainPatternDefinitions.Choices, _room.Random.Next(PhantoonRandomDefinitions.FlameRainPattern));
        state.FigureEight.PathPlace = PhantoonPath.At(pattern.PathPlace);
        body.Position = body.Position.WithWhole(pattern.BodyPosition);
        state.FigureEight.Reversed = false;
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
        if (!state.Round.StepEyeClosedTimerRunsOut())
            return;

        state.Swoop.Requested = false;
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
        if (!state.BodyFade.Complete)
            return;
        state.Phase = PhantoonPhase.MoveToTopCenterForRage;
        state.PhaseTimer = PhantoonArenaDefinitions.BeforeRageUpdates;
    }

    /// <summary>Holds the hidden rage phase until its timer expires.</summary>
    internal static void RunPhantoonRageHiddenWait(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;
        state.Phase = PhantoonPhase.FadeInForRage;
        body.Position = body.Position.WithWhole(PhantoonArenaDefinitions.RageStart);
        state.BodyFade.Complete = false;
    }

    /// <summary>Reveals the body at the start of the enraged swoop.</summary>
    internal void RunPhantoonRageFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (!state.BodyFade.Complete)
            return;
        state.Phase = PhantoonPhase.Enraged;
        state.PhaseTimer = PhantoonArenaDefinitions.FirstRageWaveUpdates;
        state.BodyFade.Complete = false;
    }

    /// <summary>Runs all eight alternating radial flame waves.</summary>
    internal void RunPhantoonRage(PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
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
            state.PhaseTimer = PhantoonRageDefinitions.WaveInterval;
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
        if (!state.BodyFade.Complete)
            return;
        state.Phase = PhantoonPhase.WaitAfterFadeOut;
        state.PhaseTimer = PhantoonArenaDefinitions.HiddenAfterFadeOutUpdates;
    }

    /// <summary>Seeds swoop speed and direction from Samus's current position.</summary>
    private static void BeginPhantoonSwoop(
        PhantoonEnemyState state)
    {
        state.Swoop.Velocity = new(PhantoonArenaDefinitions.SwoopStartSpeed, PhantoonArenaDefinitions.SwoopStartSpeed);
        state.Swoop.TargetX = 0;
        state.Swoop.TargetReturning = false;
        state.Phase = PhantoonPhase.Swooping;
        state.PhaseTimer = PhantoonArenaDefinitions.SwoopUpdates;
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
        SignedEightEight swoopX = swoop.Velocity.X;
        if (CoordinateAxis.Difference(targetX, body.Position.X.Whole) < 0)
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
        if (CoordinateAxis.Difference(body.Position.X.Whole, PhantoonMotionDefinitions.SwoopMinimumX) < 0)
            body.Position = body.Position.WithWholeX(CoordinateAxis.Wrap(PhantoonMotionDefinitions.SwoopMinimumX));
        else if (CoordinateAxis.Difference(body.Position.X.Whole, PhantoonMotionDefinitions.SwoopMaximumX) >= 0)
            body.Position = body.Position.WithWholeX(PhantoonMotionDefinitions.SwoopMaximumX);

        int targetY = target == PhantoonSwoopTarget.DeathHeight ? PhantoonArenaDefinitions.DeathSwoopY : CoordinateAxis.Wrap(samus.Position.Y.Whole - PhantoonArenaDefinitions.SwoopAboveSamus);
        SignedEightEight swoopY = swoop.Velocity.Y;
        if (CoordinateAxis.Difference(targetY, body.Position.Y.Whole) < 0)
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
        if (CoordinateAxis.Difference(body.Position.Y.Whole, PhantoonMotionDefinitions.SwoopMinimumY) < 0)
            body.Position = body.Position.WithWholeY(PhantoonMotionDefinitions.SwoopMinimumY);
        else if (CoordinateAxis.Difference(body.Position.Y.Whole, PhantoonMotionDefinitions.SwoopMaximumY) >= 0)
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
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.BodyFade.Complete)
            return;

        // The fade's step is a typed fade toward his faded-out colours (#1512, #2251).
        if (state.BodyFade.TakeStep(steps) is { } step)
            state.Colours.Fade(PhantoonBodyLook.FadedOut, step, steps);
        else
            state.Colours.CompleteFade();
    }

    /// <summary>Places the rain's delayed flames in rotating room columns, each falling a delay step after the last.</summary>
    private void SpawnPhantoonFlameRain(RoomEnemyPlace body, PhantoonRainPattern pattern)
    {
        ListStep<int> column = pattern.FirstColumn;
        int fallDelay = PhantoonArenaDefinitions.RainFallDelayStep;
        for (int flame = 0; flame < PhantoonArenaDefinitions.RainFlameCount; flame++)
        {
            SpawnPhantoonRainFlame(body, column.Entry, fallDelay);
            column = column.NextInCycle;
            fallDelay += PhantoonArenaDefinitions.RainFallDelayStep;
        }
    }

    /// <summary>Selects a later-round flame and swoop pattern from encounter state.</summary>
    internal void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state)
    {
        state.PhaseTimer = PhantoonArenaDefinitions.PatternPickUpdates;
        state.Round.EyeClosedTimer = PhantoonTimerDefinitions.EyeClosed[_room.Random.Next(PhantoonRandomDefinitions.EyeClosedChoice)];
        if (PhantoonCadenceDefinitions.FigureEightReverses.IsSetIn(_frameContext.FrameCounter))
        {
            if (!state.FigureEight.Reversed)
                state.FigureEight.PathPlace = state.FigureEight.PathPlace.PreviousInCycle;
            state.FigureEight.Speed = SignedSixteenSixteen.Zero;
            state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
            state.FigureEight.Reversed = true;
        }
        else
        {
            if (state.FigureEight.Reversed)
                state.FigureEight.PathPlace = state.FigureEight.PathPlace.NextInCycle;
            state.FigureEight.Speed = PhantoonMotionDefinitions.FigureEightStartSpeed;
            state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
            state.FigureEight.Reversed = false;
        }

        state.Phase = state.FlameRainRequested
            ? PhantoonPhase.FadeOutBeforeFirstFlameRain
            : PhantoonPhase.MoveInFigureEightThenOpenEye;
        if (state.FlameRainRequested)
            state.BodyFade.Complete = false;
    }

    /// <summary>Initializes a casual flame, which falls from the mouth harmless until it strikes the floor.</summary>
    /// <param name="body">Phantoon's body.</param>
    internal bool SpawnPhantoonCasualFlame(RoomEnemyPlace body)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.Velocity = flame.Velocity.WithX(SignedEightEight.Zero);
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.MouthOffset);
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualFallingKindling));
        flame.StartMotion(PhantoonFlameMotion.CasualFalling);

        // The falling flame's step changes the spawn definition's harmful, shootable properties to harmless ones.
        // The falling flame therefore cannot hurt Samus and cannot be shot until
        // it has struck the floor and changed into the bouncing form.
        flame.CanDamageSamus = false;
        flame.BlocksSamusProjectiles = false;
        return true;
    }

    /// <summary>Initializes an enraged flame, which circles the body from its start as its circle widens.</summary>
    /// <param name="phantoon">The encounter, whose body the flame circles.</param>
    /// <param name="start">Where on its circle the flame starts and which way it circles.</param>
    internal bool SpawnPhantoonRageFlame(PhantoonEnemyState phantoon, PhantoonRageFlameStart start)
    {
        RoomEnemyPlace body = phantoon.Body;
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonOrbitingFlameState(phantoon, start.Angle)
        {
            TurnPerUpdate = PhantoonFlameSpawns.RageTurnPerUpdate(start),
        };
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.MouthOffset);
        flame.StartMotion(PhantoonFlameMotion.RageOrbiting);
        return true;
    }

    /// <summary>Initializes a spiral flame, which circles the body clockwise from its start as its circle widens.</summary>
    /// <param name="phantoon">The encounter, whose body the flame circles.</param>
    /// <param name="start">The flame's starting angle round the body.</param>
    internal bool SpawnPhantoonSpiralFlame(PhantoonEnemyState phantoon, TrigonometryAngle start)
    {
        RoomEnemyPlace body = phantoon.Body;
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonOrbitingFlameState(phantoon, start);
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.FlameCircleCentre);
        flame.StartMotion(PhantoonFlameMotion.SpiralOrbiting);
        return true;
    }

    /// <summary>Initializes a rain flame over a room column, falling after a delay.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="columnX">The screen X of the rain column the flame falls in (<see cref="PhantoonFlameSpawns.RainXs"/>).</param>
    /// <param name="fallDelay">The updates the flame waits before it falls.</param>
    internal bool SpawnPhantoonRainFlame(RoomEnemyPlace body, int columnX, int fallDelay)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonRainFlameState(fallDelay);
        flame.Position = flame.Position.WithWholeX(columnX);
        flame.Position = flame.Position.WithWholeY(PhantoonArenaDefinitions.RainStartY);
        flame.StartMotion(PhantoonFlameMotion.RainFalling);
        return true;
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
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Burning));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = flame.Velocity.WithY(SignedEightEight.Zero);
        return flame;
    }
}
