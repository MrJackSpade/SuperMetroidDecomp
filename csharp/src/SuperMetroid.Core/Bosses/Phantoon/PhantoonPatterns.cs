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
            state.FigureEight.PathCursor = PhantoonPathDefinitions.LastPlace;
        }
        else
        {
            state.FigureEight.Speed = SignedSixteenSixteen.FromParts(1, 0);
            state.FigureEight.Reversed = false;
            state.FigureEight.PathCursor = 0;
        }
    }

    /// <summary>Moves the body through its first-round figure-eight and schedules its next phase.</summary>
    internal void RunPhantoonFirstRoundFigureEight(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        StepPhantoonFigureEight(body, state.FigureEight);
        StepPhantoonCasualFlameSchedule(state);

        // Both reaching zero and running past it expire the timer. A timer started at zero
        // expires on its first step, one movement/flame frame before the eye-opening handoff.
        if (--state.Round.EyeClosedTimer > 0)
            return;

        state.Phase = PhantoonPhase.WaitingOnEye;
        PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeOpen);
        state.FlameRainRequested = false;
        SpawnPhantoonSpiralFlames(body);
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
            Offset16<RoomPixels> pathStep = PhantoonPath.Step(figureEight.PathCursor);
            if (figureEight.Reversed)
            {
                body.Position -= pathStep;
                figureEight.PathCursor = figureEight.PathCursor == 0 ? PhantoonPathDefinitions.Length - 1 : figureEight.PathCursor - 1;
            }
            else
            {
                body.Position += pathStep;
                figureEight.PathCursor = figureEight.PathCursor >= PhantoonPathDefinitions.Length - 1 ? 0 : figureEight.PathCursor + 1;
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
                figureEight.Speed = SignedSixteenSixteen.FromParts(PhantoonMotionDefinitions.ForwardSlowCap - 1, 0);
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
        int minimum = PhantoonMotionDefinitions.ForwardMinimum;
        if (figureEight.Speed.WholePixels <= minimum)
        {
            figureEight.Speed = SignedSixteenSixteen.FromParts(minimum + 1, 0);
            figureEight.Stage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Changes horizontal velocity over the returning half of a figure-eight.</summary>
    internal static void AdjustPhantoonReverseFigureEightSpeed(PhantoonFigureEightState figureEight)
    {
        if (figureEight.Stage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.SlowSpeedChange, PhantoonSpeedChange.Decelerate);
            int cap = PhantoonMotionDefinitions.ReverseSlowCap;
            if (figureEight.Speed.WholePixels <= cap)
            {
                figureEight.Speed = SignedSixteenSixteen.FromParts(cap + 2, 0);
                figureEight.Stage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (figureEight.Stage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(figureEight, PhantoonMotionDefinitions.FastSpeedChange, PhantoonSpeedChange.Decelerate);
            int cap = PhantoonMotionDefinitions.ReverseFastCap;
            if (figureEight.Speed.WholePixels <= cap)
            {
                figureEight.Speed = SignedSixteenSixteen.FromParts(cap + 1, 0);
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

    /// <summary>Adds a 1/65536-pixel speed change to the figure-eight's speed, or subtracts it when slowing.</summary>
    private static void ChangePhantoonSpeed(PhantoonFigureEightState figureEight, SignedSixteenSixteen change, PhantoonSpeedChange direction)
    {
        figureEight.Speed = direction == PhantoonSpeedChange.Decelerate
            ? figureEight.Speed - change
            : figureEight.Speed + change;
    }

    /// <summary>Advances the casual-flame burst while he runs his figure-eight.</summary>
    internal void StepPhantoonCasualFlameSchedule(PhantoonEnemyState state)
    {
        RoomEnemyPlace mouth = state.Mouth;
        if (--state.CasualFlames.Hold > 0)
            return;

        if (state.CasualFlames.Remaining is { } count)
        {
            int remaining = count - 1;
            // A burst with no flame left after this one ends and starts its cooldown; one at zero spits a last flame too.
            bool exhausted = remaining <= 0;
            PhantoonCasualFlameSchedule schedule = PhantoonCasualFlames.Pattern(state.CasualFlames.Pattern);
            state.CasualFlames.Remaining = exhausted ? null : remaining;
            state.CasualFlames.Hold = exhausted ? schedule.Cooldown : schedule.IntervalBefore(remaining);
            PhantoonPresentation.StartPhantoonAnimation(state, mouth, PhantoonPartSegment.MouthSpawnFlame);
            return;
        }

        state.CasualFlames.Pattern = _room.Random.Next(PhantoonRandomDefinitions.CasualFlamePattern);
        PhantoonCasualFlameSchedule pattern = PhantoonCasualFlames.Pattern(state.CasualFlames.Pattern);
        state.CasualFlames.Remaining = pattern.FlameCount;
        state.CasualFlames.Hold = pattern.IntervalBefore(pattern.FlameCount);
    }

    /// <summary>Starts the spiral-flame pattern around the body.</summary>
    /// <remarks>The spiral fireballs are the first ordinary combat attack: eight destroyable flames, spawned in their
    /// authored order round the body.</remarks>
    private void SpawnPhantoonSpiralFlames(RoomEnemyPlace body)
    {
        foreach (TrigonometryAngle start in PhantoonFlameSpawns.SpiralStarts)
            SpawnPhantoonSpiralFlame(body, start);
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
                PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.InvulnerableBody);
                PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeCloseAndPickNewPattern);
                body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

                // This flag makes the next pattern choice take the initial flame-rain
                // branch after the eye-close animation runs its flame-rain cue.
                state.FlameRainRequested = true;
                return;
            }

            state.Swoop.Requested = false;
            state.PhaseTimer = 60;
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
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.FullHitboxBody);
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
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.InvulnerableBody);
        RoomEnemyPlace eye = state.Eye;
        PhantoonPresentation.StartPhantoonAnimation(state, eye, PhantoonPartSegment.EyeClose);
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
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.BodyFade.Complete)
            return;

        state.Phase = PhantoonPhase.WaitAfterFadeOut;
        state.PhaseTimer = 120;
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
            state.FigureEight.PathCursor = PhantoonPathDefinitions.RightAppearance;
            body.Position = body.Position.WithWholeX(208);
        }
        else
        {
            state.FigureEight.PathCursor = PhantoonPathDefinitions.LeftAppearance;
            body.Position = body.Position.WithWholeX(48);
        }
        body.Position = body.Position.WithWholeY(96);
        state.FigureEight.Reversed = false;
        state.FigureEight.Speed = SignedSixteenSixteen.FromParts(1, 0);
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
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (state.BodyFade.Complete)
            state.Phase = PhantoonPhase.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Runs the visible setup for every flame-rain window.</summary>
    internal static void BeginPhantoonFlameRainVulnerableWindow(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        state.BodyFade.Complete = false;
        state.Translucent = false;
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.FullHitboxBody);
        PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeballCentered);
        state.Phase = PhantoonPhase.FadeInDuringFlameRain;
    }

    /// <summary>Reveals the body while preparing the flame-rain attack.</summary>
    internal void RunPhantoonFlameRainFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 1);
        if (!state.BodyFade.Complete)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Phase = PhantoonPhase.TrackSamusDuringFlameRain;
        state.PhaseTimer = 90;
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
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.InvulnerableBody);
        PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.Translucent = true;
    }

    /// <summary>Hides the body after the vulnerable rain interval.</summary>
    internal void RunPhantoonFlameRainFadeOut(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
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

        int pattern = _room.Random.Next(PhantoonRandomDefinitions.FlameRainPattern);
        var placement = PhantoonFlameRain.RainPlacement(pattern);
        state.FigureEight.PathCursor = placement.Cursor;
        body.Position = body.Position.WithWhole(placement.Position);
        state.FigureEight.Reversed = false;
        state.Phase = PhantoonPhase.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    /// <summary>Starts the first rain pattern after the intro round.</summary>
    internal void RunPhantoonInitialFlameRain(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        StepPhantoonFigureEight(body, state.FigureEight);
        StepPhantoonCasualFlameSchedule(state);
        // Reaching zero expires the timer, as does running past it.
        if (--state.Round.EyeClosedTimer > 0)
            return;

        state.Swoop.Requested = false;
        state.Phase = PhantoonPhase.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.Position.X.Whole < 128 ? 0 : 2);
    }

    /// <summary>Hides the body before entering the enraged attack cycle.</summary>
    internal void RunPhantoonFadeOutBeforeRage(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.BodyFade.Complete)
            return;
        state.Phase = PhantoonPhase.MoveToTopCenterForRage;
        state.PhaseTimer = 120;
    }

    /// <summary>Holds the hidden rage phase until its timer expires.</summary>
    internal static void RunPhantoonRageHiddenWait(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;
        state.Phase = PhantoonPhase.FadeInForRage;
        body.Position = body.Position.WithWhole(new(128, 32));
        state.BodyFade.Complete = false;
    }

    /// <summary>Reveals the body at the start of the enraged swoop.</summary>
    internal void RunPhantoonRageFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (!state.BodyFade.Complete)
            return;
        state.Phase = PhantoonPhase.Enraged;
        state.PhaseTimer = 4;
        state.BodyFade.Complete = false;
    }

    /// <summary>Runs all eight alternating radial flame waves.</summary>
    internal void RunPhantoonRage(RoomEnemyPlace body, PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        // Alternating waves contain seven and eight flames, not seven each.
        IReadOnlyList<PhantoonRageFlameStart> wave = PhantoonCadenceDefinitions.OddRageWave.IsSetIn(state.RageWaveCount)
            ? PhantoonFlameSpawns.OddRageWave
            : PhantoonFlameSpawns.EvenRageWave;
        foreach (PhantoonRageFlameStart start in wave)
            SpawnPhantoonRageFlame(body, start);

        _audio.QueueEnemySound(PhantoonRageDefinitions.WaveSound, PhantoonRageDefinitions.WaveSoundPriority);
        state.RageWaveCount++;
        if (state.RageWaveCount < PhantoonRageDefinitions.WaveCount)
        {
            state.PhaseTimer = PhantoonRageDefinitions.WaveInterval;
            return;
        }

        PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeClose);
        state.RageWaveCount = 0;
        state.Phase = PhantoonPhase.FadeOutAfterRage;
    }

    /// <summary>Hides the body after the enraged swoop.</summary>
    internal void RunPhantoonRageFadeOut(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.BodyFade.Complete)
            return;
        state.Phase = PhantoonPhase.WaitAfterFadeOut;
        state.PhaseTimer = 120;
    }

    /// <summary>Seeds swoop speed and direction from Samus's current position.</summary>
    private static void BeginPhantoonSwoop(
        PhantoonEnemyState state)
    {
        state.Swoop.Velocity = new(SignedEightEight.FromPixels(4), SignedEightEight.FromPixels(4));
        state.Swoop.TargetX = 0;
        state.Swoop.TargetReturning = false;
        state.Phase = PhantoonPhase.Swooping;
        state.PhaseTimer = 360;
    }

    /// <summary>
    /// Moves Phantoon in a swoop using the Japanese and North American values present in the target
    /// release. The asymmetric negative velocity thresholds are intentional; replacing
    /// them with tidy +/- clamps changes the retail swoop path.
    /// </summary>
    internal static void MovePhantoonInSwoop(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus,
        PhantoonSwoopTarget target)
    {
        PhantoonSwoopState swoop = state.Swoop;
        // The target sweeps two pixels an update out to its turn and back to zero, where it starts out again.
        if (swoop.TargetReturning)
        {
            swoop.TargetX -= 2;
            if (swoop.TargetX == 0)
                swoop.TargetReturning = false;
        }
        else
        {
            swoop.TargetX += 2;
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

        int targetY = target == PhantoonSwoopTarget.DeathHeight ? 112 : CoordinateAxis.Wrap(samus.Position.Y.Whole - 48);
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
        if (CoordinateAxis.Difference(body.Position.Y.Whole, 64) < 0)
            body.Position = body.Position.WithWholeY(64);
        else if (CoordinateAxis.Difference(body.Position.Y.Whole, 216) >= 0)
            body.Position = body.Position.WithWholeY(216);
    }

    /// <summary>Selects the eye pose and angle that track Samus from the current body position.</summary>
    internal static void PointPhantoonEyeAtSamus(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        Offset16<RoomPixels> toSamus = samus.Position.Whole - body.Position.Whole;
        CompassDirection direction;
        if (Math.Abs(toSamus.Y) < 32)
            direction = toSamus.X < 0 ? CompassDirection.Left : CompassDirection.Right;
        else if (Math.Abs(toSamus.X) < 32)
            direction = toSamus.Y < 0 ? CompassDirection.Up : CompassDirection.Down;
        else if (toSamus.X < 0)
            direction = toSamus.Y < 0 ? CompassDirection.UpLeft : CompassDirection.DownLeft;
        else
            direction = toSamus.Y < 0 ? CompassDirection.UpRight : CompassDirection.DownRight;

        PhantoonPresentation.StartPhantoonAnimation(
            state,
            state.Eye,
            PhantoonEyeGaze.EyeAnimation(direction));
    }

    /// <summary>Blends the body colours toward hidden colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeOut(
        PhantoonEnemyState state,
        int denominator)
    {
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.BodyFade.Complete)
            return;

        state.BodyFade.Denominator = denominator;
        int numerator = state.BodyFade.Numerator;
        if (denominator + 1 < numerator)
        {
            state.BodyFade.Numerator = 0;
            state.BodyFade.Complete = true;
            state.Colours.CompleteFade();
            return;
        }

        // The fade's step is the import's shade of it, toward his faded-out colours (#1512).
        state.Colours.Fade(PhantoonBodyLook.FadedOut, denominator, numerator);
        state.BodyFade.Numerator = numerator + 1;
    }

    /// <summary>Places eight delayed rain flames in rotating room columns, each falling eight updates after the last.</summary>
    private void SpawnPhantoonFlameRain(RoomEnemyPlace body, int pattern)
    {
        int column = PhantoonFlameRain.FirstRainColumns[pattern];
        int fallDelay = 8;
        for (int flame = 0; flame < 8; flame++)
        {
            SpawnPhantoonRainFlame(body, column, fallDelay);
            column++;
            if (column >= 9)
                column = 0;
            fallDelay += 8;
        }
    }

    /// <summary>Selects a later-round flame and swoop pattern from encounter state.</summary>
    internal void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state)
    {
        state.PhaseTimer = 60;
        state.Round.EyeClosedTimer = PhantoonTimerDefinitions.EyeClosed[_room.Random.Next(PhantoonRandomDefinitions.EyeClosedChoice)];
        if (PhantoonCadenceDefinitions.FigureEightReverses.IsSetIn(_frameContext.FrameCounter))
        {
            if (!state.FigureEight.Reversed)
                state.FigureEight.PathCursor = state.FigureEight.PathCursor == 0 ? PhantoonPathDefinitions.LastPlace : state.FigureEight.PathCursor - 1;
            state.FigureEight.Speed = SignedSixteenSixteen.Zero;
            state.FigureEight.Stage = PhantoonFigureEightStage.EasingIn;
            state.FigureEight.Reversed = true;
        }
        else
        {
            if (state.FigureEight.Reversed)
                state.FigureEight.PathCursor = state.FigureEight.PathCursor >= PhantoonPathDefinitions.LastPlace ? 0 : state.FigureEight.PathCursor + 1;
            state.FigureEight.Speed = SignedSixteenSixteen.FromParts(1, 0);
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
        flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 32)));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualFalling));
        flame.StartMotion(PhantoonFlameMotion.CasualFalling);

        // The falling flame's step changes the spawn definition's harmful, shootable properties to harmless ones.
        // The falling flame therefore cannot hurt Samus and cannot be shot until
        // it has struck the floor and changed into the bouncing form.
        flame.CanDamageSamus = false;
        flame.BlocksSamusProjectiles = false;
        return true;
    }

    /// <summary>Initializes an enraged flame, which circles the body from its start as its circle widens.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="start">Where on its circle the flame starts and which way it circles.</param>
    internal bool SpawnPhantoonRageFlame(RoomEnemyPlace body, PhantoonRageFlameStart start)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonOrbitingFlameState(start.Angle)
        {
            TurnPerUpdate = PhantoonFlameSpawns.RageTurnPerUpdate(start),
        };
        flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 32)));
        flame.StartMotion(PhantoonFlameMotion.RageOrbiting);
        return true;
    }

    /// <summary>Initializes a spiral flame, which circles the body clockwise from its start as its circle widens.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="start">The flame's starting angle round the body.</param>
    internal bool SpawnPhantoonSpiralFlame(RoomEnemyPlace body, TrigonometryAngle start)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonOrbitingFlameState(start);
        flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 16)));
        flame.StartMotion(PhantoonFlameMotion.SpiralOrbiting);
        return true;
    }

    /// <summary>Initializes a rain flame over a room column, falling after a delay.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="column">The room column the flame falls in, 0 through 8.</param>
    /// <param name="fallDelay">The updates the flame waits before it falls.</param>
    internal bool SpawnPhantoonRainFlame(RoomEnemyPlace body, int column, int fallDelay)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonRainFlameState(fallDelay);
        flame.Position = flame.Position.WithWholeX(PhantoonFlameSpawns.RainX(column));
        flame.Position = flame.Position.WithWholeY(40);
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
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableIdle));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = flame.Velocity.WithY(SignedEightEight.Zero);
        return flame;
    }
}
