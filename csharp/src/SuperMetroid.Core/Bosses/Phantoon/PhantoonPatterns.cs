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
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _phantoonPresentation;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonPatterns(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyFrameContext frameContext, PhantoonPresentation phantoonPresentation)
    {
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
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        state.EyeState.WaveLength = null;
        state.EyeState.ClosedTimer = PhantoonTimerDefinitions.EyeClosed[PhantoonCadenceDefinitions.FirstEyeClosedChoice.Of(_frameContext.FrameCounter)];
        state.BodyState.Function = PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        state.BodyState.FigureEightStage = PhantoonFigureEightStage.EasingIn;
        if (PhantoonRandomDefinitions.FigureEightReversed.IsSetIn(_room.Random.Generate()))
        {
            state.BodyState.FigureEightSpeed = SignedSixteenSixteen.Zero;
            state.EyeState.FigureEightReversed = true;
            state.BodyState.PathCursor = 533;
        }
        else
        {
            state.BodyState.FigureEightSpeed = SignedSixteenSixteen.FromParts(1, 0);
            state.EyeState.FigureEightReversed = false;
            state.BodyState.PathCursor = 0;
        }
    }

    /// <summary>Moves the body through its first-round figure-eight and schedules its next phase.</summary>
    internal void RunPhantoonFirstRoundFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        StepPhantoonFigureEight(body, state.BodyState, state.EyeState);
        StepPhantoonCasualFlameSchedule(state);

        // Both reaching zero and running past it expire the timer. A timer started at zero
        // expires on its first step, one movement/flame frame before the eye-opening handoff.
        if (--state.EyeState.ClosedTimer > 0)
            return;

        state.BodyState.Function = PhantoonAiFunction.NoOperation;
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeOpen);
        state.BodyState.FlameRainRequested = false;
        SpawnPhantoonSpiralFlames(body);
    }

    /// <summary>Advances the body and eye on their figure-eight path.</summary>
    internal static void StepPhantoonFigureEight(RoomEnemySlot body, PhantoonBodyState bodyPart, PhantoonEyeState eyePart)
    {
        if (eyePart.FigureEightReversed)
            AdjustPhantoonReverseFigureEightSpeed(bodyPart);
        else
            AdjustPhantoonForwardFigureEightSpeed(bodyPart);

        int steps = eyePart.FigureEightReversed
            ? -bodyPart.FigureEightSpeed.WholePixels
            : bodyPart.FigureEightSpeed.WholePixels;
        for (int step = 0; step < steps; step++)
        {
            Offset16<RoomPixels> pathStep = PhantoonPath.Step(bodyPart.PathCursor);
            if (eyePart.FigureEightReversed)
            {
                body.Position -= pathStep;
                bodyPart.PathCursor = bodyPart.PathCursor == 0 ? PhantoonPathDefinitions.Length - 1 : bodyPart.PathCursor - 1;
            }
            else
            {
                body.Position += pathStep;
                bodyPart.PathCursor = bodyPart.PathCursor >= PhantoonPathDefinitions.Length - 1 ? 0 : bodyPart.PathCursor + 1;
            }
        }
    }

    /// <summary>Changes horizontal velocity over the forward half of a figure-eight.</summary>
    internal static void AdjustPhantoonForwardFigureEightSpeed(PhantoonBodyState body)
    {
        if (body.FigureEightStage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(
                body,
                low: PhantoonMotionDefinitions.SlowFraction,
                high: PhantoonMotionDefinitions.SlowWhole, direction: PhantoonSpeedChange.Accelerate);
            if (body.FigureEightSpeed.WholePixels >= PhantoonMotionDefinitions.ForwardSlowCap)
            {
                body.FigureEightSpeed = SignedSixteenSixteen.FromParts(PhantoonMotionDefinitions.ForwardSlowCap - 1, 0);
                body.FigureEightStage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (body.FigureEightStage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Accelerate);
            if (body.FigureEightSpeed.WholePixels >= PhantoonMotionDefinitions.ForwardFastCap)
            {
                body.FigureEightSpeed = SignedSixteenSixteen.FromParts(PhantoonMotionDefinitions.ForwardFastCap, 0);
                body.FigureEightStage = PhantoonFigureEightStage.Settling;
            }
            return;
        }

        ChangePhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Decelerate);
        int minimum = PhantoonMotionDefinitions.ForwardMinimum;
        if (body.FigureEightSpeed.WholePixels <= minimum)
        {
            body.FigureEightSpeed = SignedSixteenSixteen.FromParts(minimum + 1, 0);
            body.FigureEightStage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Changes horizontal velocity over the returning half of a figure-eight.</summary>
    internal static void AdjustPhantoonReverseFigureEightSpeed(PhantoonBodyState body)
    {
        if (body.FigureEightStage == PhantoonFigureEightStage.EasingIn)
        {
            ChangePhantoonSpeed(
                body,
                PhantoonMotionDefinitions.SlowFraction,
                PhantoonMotionDefinitions.SlowWhole, direction: PhantoonSpeedChange.Decelerate);
            int cap = PhantoonMotionDefinitions.ReverseSlowCap;
            if (body.FigureEightSpeed.WholePixels <= cap)
            {
                body.FigureEightSpeed = SignedSixteenSixteen.FromParts(cap + 2, 0);
                body.FigureEightStage = PhantoonFigureEightStage.Surging;
            }
            return;
        }

        if (body.FigureEightStage == PhantoonFigureEightStage.Surging)
        {
            ChangePhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Decelerate);
            int cap = PhantoonMotionDefinitions.ReverseFastCap;
            if (body.FigureEightSpeed.WholePixels <= cap)
            {
                body.FigureEightSpeed = SignedSixteenSixteen.FromParts(cap + 1, 0);
                body.FigureEightStage = PhantoonFigureEightStage.Settling;
            }
            return;
        }

        ChangePhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Accelerate);
        int maximum = PhantoonMotionDefinitions.ReverseMaximum;
        if (body.FigureEightSpeed.WholePixels >= maximum)
        {
            body.FigureEightSpeed = SignedSixteenSixteen.FromParts(maximum, 0);
            body.FigureEightStage = PhantoonFigureEightStage.EasingIn;
        }
    }

    /// <summary>Adds a 16.16 speed change to the body's figure-eight speed, or subtracts it when slowing.</summary>
    private static void ChangePhantoonSpeed(PhantoonBodyState body, int low, int high, PhantoonSpeedChange direction)
    {
        SignedSixteenSixteen change = SignedSixteenSixteen.FromParts(high, low);
        body.FigureEightSpeed = direction == PhantoonSpeedChange.Decelerate
            ? body.FigureEightSpeed - change
            : body.FigureEightSpeed + change;
    }

    /// <summary>Advances the mouth's casual flame spawn timer during ordinary movement.</summary>
    internal void StepPhantoonCasualFlameSchedule(PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth;
        if (--state.MouthState.CasualFlameHold > 0)
            return;

        if (state.MouthState.CasualFlameCount is { } count)
        {
            int remaining = count - 1;
            // A burst with no flame left after this one ends and starts its cooldown; one at zero spits a last flame too.
            bool exhausted = remaining <= 0;
            PhantoonCasualFlameSchedule schedule = PhantoonCasualFlames.Pattern(state.MouthState.CasualFlamePattern);
            state.MouthState.CasualFlameCount = exhausted ? null : remaining;
            state.MouthState.CasualFlameHold = exhausted ? schedule.Cooldown : schedule.IntervalBefore(remaining);
            PhantoonPresentation.InstallPhantoonProgram(state, mouth, PhantoonPartProgram.MouthSpawnFlame);
            return;
        }

        state.MouthState.CasualFlamePattern = _room.Random.Next(PhantoonRandomDefinitions.CasualFlamePattern);
        PhantoonCasualFlameSchedule pattern = PhantoonCasualFlames.Pattern(state.MouthState.CasualFlamePattern);
        state.MouthState.CasualFlameCount = pattern.FlameCount;
        state.MouthState.CasualFlameHold = pattern.IntervalBefore(pattern.FlameCount);
    }

    /// <summary>Starts the spiral-flame pattern around the body.</summary>
    /// <remarks>The spiral fireballs are the first ordinary combat attack: eight destroyable flames, spawned in their
    /// authored order round the body.</remarks>
    private void SpawnPhantoonSpiralFlames(RoomEnemySlot body)
    {
        foreach (TrigonometryAngle start in PhantoonFlameSpawns.SpiralStarts)
            SpawnPhantoonSpiralFlame(body, start);
    }

    /// <summary>Runs the eye-open vulnerable window.</summary>
    internal static void RunPhantoonEyeTracking(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        if (PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
        {
            // The tentacles' round damage is the damage accumulated during this
            // opening. Clears it before deciding whether a shot requested a swoop.
            state.TentaclesState.RoundDamage = 0;
            if (!state.TentaclesState.SwoopRequested)
            {
                state.BodyState.Function = PhantoonAiFunction.NoOperation;
                PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
                PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeCloseAndPickNewPattern);
                body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

                // This flag makes the next pattern choice take the initial flame-rain
                // branch after the eye-close list invokes its flame-rain callback.
                state.BodyState.FlameRainRequested = true;
                return;
            }

            state.TentaclesState.SwoopRequested = false;
            state.BodyState.FunctionTimer = 60;
            state.BodyState.Function = PhantoonAiFunction.BecomeSolidAndSwoop;
        }

        PointPhantoonEyeAtSamus(body, state, samus);
    }

    /// <summary>Runs the one-frame opaque-swoop setup.</summary>
    internal static void BeginPhantoonVulnerableSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        state.Translucent = false;
        BeginPhantoonSwoop(state);
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.FullHitboxBody);
    }

    /// <summary>Runs the 360-frame opaque swoop.</summary>
    internal static void RunPhantoonSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.AboveSamus);
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        state.BodyState.Function = PhantoonAiFunction.FadeOutWhileSwooping;
        state.Translucent = true;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        RoomEnemySlot eye = state.Eye;
        PhantoonPresentation.InstallPhantoonProgram(state, eye, PhantoonPartProgram.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.EyeState.FadeComplete = false;
        state.TentaclesState.RoundDamage = 0;
    }

    /// <summary>Fades the body while continuing its swoop across the room.</summary>
    internal void RunPhantoonFadeOutWhileSwooping(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.AboveSamus);
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.EyeState.FadeComplete)
            return;

        state.BodyState.Function = PhantoonAiFunction.WaitAfterFadeOut;
        state.BodyState.FunctionTimer = 120;
    }

    /// <summary>Counts the hidden interval before reappearing.</summary>
    internal static void RunPhantoonHiddenWait(PhantoonBodyState body)
    {
        if (PhantoonPresentation.TickPhantoonFunctionTimer(body))
            body.Function = PhantoonAiFunction.PickNextAppearance;
    }

    /// <summary>Runs the hidden left/right placement and round-two picker.</summary>
    internal void PlacePhantoonForNextFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (PhantoonRandomDefinitions.FigureEightStartsRight.IsSetIn(_room.Random.Generate()))
        {
            state.BodyState.PathCursor = 136;
            body.Position = body.Position.WithWholeX(208);
        }
        else
        {
            state.BodyState.PathCursor = 399;
            body.Position = body.Position.WithWholeX(48);
        }
        body.Position = body.Position.WithWholeY(96);
        state.EyeState.FigureEightReversed = false;
        state.BodyState.FigureEightSpeed = SignedSixteenSixteen.FromParts(1, 0);
        state.BodyState.FlameRainRequested = false;

        // The call deliberately installs a pattern function which is immediately
        // overwritten with the fade-in function. Its timer, direction, and movement-side
        // effects remain live, so call the shared callback implementation in full.
        PickPhantoonSecondRoundPattern(state);
        state.BodyState.Function = PhantoonAiFunction.FadeInBeforeFigureEight;
        state.EyeState.FadeComplete = false;
    }

    /// <summary>Reveals Phantoon before the next figure-eight pass.</summary>
    internal void RunPhantoonFadeInBeforeFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (state.EyeState.FadeComplete)
            state.BodyState.Function = PhantoonAiFunction.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Runs the visible setup for every flame-rain window.</summary>
    internal static void BeginPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.EyeState.FadeComplete = false;
        state.Translucent = false;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.FullHitboxBody);
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeballCentered);
        state.BodyState.Function = PhantoonAiFunction.FadeInDuringFlameRain;
    }

    /// <summary>Reveals the body while preparing the flame-rain attack.</summary>
    internal void RunPhantoonFlameRainFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 1);
        if (!state.EyeState.FadeComplete)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.BodyState.Function = PhantoonAiFunction.TrackSamusDuringFlameRain;
        state.BodyState.FunctionTimer = 90;
    }

    /// <summary>Counts the open-eye interval during flame rain.</summary>
    internal static void RunPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        state.TentaclesState.RoundDamage = 0;
        if (state.TentaclesState.SwoopRequested)
        {
            state.TentaclesState.SwoopRequested = false;
            state.BodyState.FlameRainRequested = true;
            BeginPhantoonSwoop(state);
            return;
        }

        state.BodyState.Function = PhantoonAiFunction.FadeOutDuringFlameRain;
        state.EyeState.FadeComplete = false;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.Translucent = true;
    }

    /// <summary>Hides the body after the vulnerable rain interval.</summary>
    internal void RunPhantoonFlameRainFadeOut(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.EyeState.FadeComplete)
            return;

        state.BodyState.Function = PhantoonAiFunction.SpawnFlameRain;
        state.BodyState.FunctionTimer = PhantoonTimerDefinitions.RainHiding[_room.Random.Next(PhantoonRandomDefinitions.RainHidingChoice)];
    }

    /// <summary>Emits the scheduled rain pattern while Phantoon is hidden.</summary>
    internal void RunPhantoonHiddenFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        int pattern = _room.Random.Next(PhantoonRandomDefinitions.FlameRainPattern);
        var placement = PhantoonFlameRain.RainPlacement(pattern);
        state.BodyState.PathCursor = placement.Cursor;
        body.Position = body.Position.WithWhole(placement.Position);
        state.EyeState.FigureEightReversed = false;
        state.BodyState.Function = PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    /// <summary>Starts the first rain pattern after the intro round.</summary>
    internal void RunPhantoonInitialFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        StepPhantoonFigureEight(body, state.BodyState, state.EyeState);
        StepPhantoonCasualFlameSchedule(state);
        // Reaching zero expires the timer, as does running past it.
        if (--state.EyeState.ClosedTimer > 0)
            return;

        state.TentaclesState.SwoopRequested = false;
        state.BodyState.Function = PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.Position.X.Whole < 128 ? 0 : 2);
    }

    /// <summary>Hides the body before entering the enraged attack cycle.</summary>
    internal void RunPhantoonFadeOutBeforeRage(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.EyeState.FadeComplete)
            return;
        state.BodyState.Function = PhantoonAiFunction.MoveToTopCenterForRage;
        state.BodyState.FunctionTimer = 120;
    }

    /// <summary>Holds the hidden rage phase until its timer expires.</summary>
    internal static void RunPhantoonRageHiddenWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;
        state.BodyState.Function = PhantoonAiFunction.FadeInForRage;
        body.Position = body.Position.WithWhole(new(128, 32));
        state.EyeState.FadeComplete = false;
    }

    /// <summary>Reveals the body at the start of the enraged swoop.</summary>
    internal void RunPhantoonRageFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (!state.EyeState.FadeComplete)
            return;
        state.BodyState.Function = PhantoonAiFunction.Enraged;
        state.BodyState.FunctionTimer = 4;
        state.EyeState.FadeComplete = false;
    }

    /// <summary>Runs all eight alternating radial flame waves.</summary>
    internal void RunPhantoonRage(RoomEnemySlot body, PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        // Alternating waves contain seven and eight flames, not seven each.
        IReadOnlyList<PhantoonRageFlameStart> wave = PhantoonCadenceDefinitions.OddRageWave.IsSetIn(state.EyeState.RageWaveCount)
            ? PhantoonFlameSpawns.OddRageWave
            : PhantoonFlameSpawns.EvenRageWave;
        foreach (PhantoonRageFlameStart start in wave)
            SpawnPhantoonRageFlame(body, start);

        _audio.QueueEnemySound(PhantoonRageDefinitions.WaveSound, PhantoonRageDefinitions.WaveSoundPriority);
        state.EyeState.RageWaveCount = WrappingCounter.IncrementCount(state.EyeState.RageWaveCount);
        if (state.EyeState.RageWaveCount < PhantoonRageDefinitions.WaveCount)
        {
            state.BodyState.FunctionTimer = PhantoonRageDefinitions.WaveInterval;
            return;
        }

        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeClose);
        state.EyeState.RageWaveCount = 0;
        state.BodyState.Function = PhantoonAiFunction.FadeOutAfterRage;
    }

    /// <summary>Hides the body after the enraged swoop.</summary>
    internal void RunPhantoonRageFadeOut(
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.EyeState.FadeComplete)
            return;
        state.BodyState.Function = PhantoonAiFunction.WaitAfterFadeOut;
        state.BodyState.FunctionTimer = 120;
    }

    /// <summary>Seeds swoop speed and direction from Samus's current position.</summary>
    private static void BeginPhantoonSwoop(
        PhantoonEnemyState state)
    {
        state.TentaclesState.SwoopVelocity = new(SignedEightEight.FromPixels(4), SignedEightEight.FromPixels(4));
        state.TentaclesState.SwoopTargetX = 0;
        state.TentaclesState.SwoopReturning = false;
        state.BodyState.Function = PhantoonAiFunction.Swooping;
        state.BodyState.FunctionTimer = 360;
    }

    /// <summary>
    /// Moves Phantoon in a swoop using the NTSC/Japan-USA regional values present in the target
    /// release. The asymmetric negative velocity thresholds are intentional; replacing
    /// them with tidy +/- clamps changes the retail swoop path.
    /// </summary>
    internal static void MovePhantoonInSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus,
        PhantoonSwoopTarget target)
    {
        PhantoonTentaclesState tentaclePart = state.TentaclesState;
        // The target sweeps two pixels an update out to its turn and back to zero, where it starts out again.
        if (tentaclePart.SwoopReturning)
        {
            tentaclePart.SwoopTargetX -= 2;
            if (tentaclePart.SwoopTargetX == 0)
                tentaclePart.SwoopReturning = false;
        }
        else
        {
            tentaclePart.SwoopTargetX += 2;
            if (tentaclePart.SwoopTargetX >= PhantoonMotionDefinitions.SwoopTargetTurnX)
                tentaclePart.SwoopReturning = true;
        }
        int targetX = tentaclePart.SwoopTargetX;

        // Each pull stops past its threshold, so the swoop's speeds stay within about eight pixels an update either way.
        SignedEightEight swoopX = tentaclePart.SwoopVelocity.X;
        if (CoordinateAxis.Difference(targetX, body.Position.X.Whole) < 0)
        {
            if (swoopX >= PhantoonMotionDefinitions.SwoopNegativeXThreshold)
                swoopX -= PhantoonMotionDefinitions.SwoopHorizontalPull;
        }
        else if (swoopX < PhantoonMotionDefinitions.SwoopPositiveXLimit)
        {
            swoopX += PhantoonMotionDefinitions.SwoopHorizontalPull;
        }
        tentaclePart.SwoopVelocity = tentaclePart.SwoopVelocity.WithX(swoopX);
        body.Position = body.Position.AdvanceX(tentaclePart.SwoopVelocity.X.ToSixteenSixteenDelta());
        if (CoordinateAxis.Difference(body.Position.X.Whole, PhantoonMotionDefinitions.SwoopMinimumX) < 0)
            body.Position = body.Position.WithWholeX(CoordinateAxis.Wrap(PhantoonMotionDefinitions.SwoopMinimumX));
        else if (CoordinateAxis.Difference(body.Position.X.Whole, PhantoonMotionDefinitions.SwoopMaximumX) >= 0)
            body.Position = body.Position.WithWholeX(PhantoonMotionDefinitions.SwoopMaximumX);

        int targetY = target == PhantoonSwoopTarget.DeathHeight ? 112 : CoordinateAxis.Wrap(samus.Position.Y.Whole - 48);
        SignedEightEight swoopY = tentaclePart.SwoopVelocity.Y;
        if (CoordinateAxis.Difference(targetY, body.Position.Y.Whole) < 0)
        {
            if (swoopY >= PhantoonMotionDefinitions.SwoopNegativeYThreshold)
                swoopY -= PhantoonMotionDefinitions.SwoopVerticalPull;
        }
        else if (swoopY < PhantoonMotionDefinitions.SwoopPositiveYLimit)
        {
            swoopY += PhantoonMotionDefinitions.SwoopVerticalPull;
        }
        tentaclePart.SwoopVelocity = tentaclePart.SwoopVelocity.WithY(swoopY);
        body.Position = body.Position.AdvanceY(tentaclePart.SwoopVelocity.Y.ToSixteenSixteenDelta());
        if (CoordinateAxis.Difference(body.Position.Y.Whole, 64) < 0)
            body.Position = body.Position.WithWholeY(64);
        else if (CoordinateAxis.Difference(body.Position.Y.Whole, 216) >= 0)
            body.Position = body.Position.WithWholeY(216);
    }

    /// <summary>Selects the eye pose and angle that track Samus from the current body position.</summary>
    internal static void PointPhantoonEyeAtSamus(
        RoomEnemySlot body,
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

        PhantoonPresentation.InstallPhantoonProgram(
            state,
            state.Eye,
            PhantoonEyeGaze.EyeProgram(direction));
    }

    /// <summary>Blends the body palette toward hidden colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeOut(
        PhantoonEnemyState state,
        int denominator)
    {
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.EyeState.FadeComplete)
            return;

        state.EyeState.FadeDenominator = denominator;
        int numerator = state.EyeState.FadeNumerator;
        if (denominator + 1 < numerator)
        {
            state.EyeState.FadeNumerator = 0;
            state.EyeState.FadeComplete = true;
            state.Colours.CompleteFade();
            return;
        }

        // The fade's step is the import's shade of it, toward his faded-out colours (#1512).
        state.Colours.Fade(PhantoonBodyLook.FadedOut, denominator, numerator);
        state.EyeState.FadeNumerator = numerator + 1;
    }

    /// <summary>Places eight delayed rain flames in rotating room columns, each falling eight updates after the last.</summary>
    private void SpawnPhantoonFlameRain(RoomEnemySlot body, int pattern)
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
        state.BodyState.FunctionTimer = 60;
        state.EyeState.ClosedTimer = PhantoonTimerDefinitions.EyeClosed[_room.Random.Next(PhantoonRandomDefinitions.EyeClosedChoice)];
        if (PhantoonCadenceDefinitions.FigureEightReverses.IsSetIn(_frameContext.FrameCounter))
        {
            if (!state.EyeState.FigureEightReversed)
                state.BodyState.PathCursor = state.BodyState.PathCursor == 0 ? 533 : state.BodyState.PathCursor - 1;
            state.BodyState.FigureEightSpeed = SignedSixteenSixteen.Zero;
            state.BodyState.FigureEightStage = PhantoonFigureEightStage.EasingIn;
            state.EyeState.FigureEightReversed = true;
        }
        else
        {
            if (state.EyeState.FigureEightReversed)
                state.BodyState.PathCursor = state.BodyState.PathCursor >= 533 ? 0 : state.BodyState.PathCursor + 1;
            state.BodyState.FigureEightSpeed = SignedSixteenSixteen.FromParts(1, 0);
            state.BodyState.FigureEightStage = PhantoonFigureEightStage.EasingIn;
            state.EyeState.FigureEightReversed = false;
        }

        state.BodyState.Function = state.BodyState.FlameRainRequested
            ? PhantoonAiFunction.FadeOutBeforeFirstFlameRain
            : PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        if (state.BodyState.FlameRainRequested)
            state.EyeState.FadeComplete = false;
    }

    /// <summary>Initializes a casual flame, which falls from the mouth harmless until it strikes the floor.</summary>
    /// <param name="body">Phantoon's body.</param>
    internal bool SpawnPhantoonCasualFlame(RoomEnemySlot body)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.Velocity = flame.Velocity.WithX(SignedEightEight.Zero);
        flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 32)));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualFalling0));
        flame.FrameBehavior =
            EnemyProjectileFrameBehavior.PhantoonCasualFlameFall;

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
    internal bool SpawnPhantoonRageFlame(RoomEnemySlot body, PhantoonRageFlameStart start)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonOrbitingFlameState(start.Angle)
        {
            TurnPerUpdate = PhantoonFlameSpawns.RageTurnPerUpdate(start),
        };
        flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 32)));
        flame.FrameBehavior =
            EnemyProjectileFrameBehavior.PhantoonRageFlameOrbit;
        return true;
    }

    /// <summary>Initializes a spiral flame, which circles the body clockwise from its start as its circle widens.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="start">The flame's starting angle round the body.</param>
    internal bool SpawnPhantoonSpiralFlame(RoomEnemySlot body, TrigonometryAngle start)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonOrbitingFlameState(start);
        flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, CoordinateAxis.Wrap(body.Position.Y.Whole + 16)));
        flame.FrameBehavior =
            EnemyProjectileFrameBehavior.PhantoonSpiralFlameOrbit;
        return true;
    }

    /// <summary>Initializes a rain flame over a room column, falling after a delay.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="column">The room column the flame falls in, 0 through 8.</param>
    /// <param name="fallDelay">The updates the flame waits before it falls.</param>
    internal bool SpawnPhantoonRainFlame(RoomEnemySlot body, int column, int fallDelay)
    {
        EnemyProjectile? flame = AllocateDestroyableFlame(body);
        if (flame is null)
            return false;
        flame.State = new PhantoonRainFlameState(fallDelay);
        flame.Position = flame.Position.WithWholeX(PhantoonFlameSpawns.RainX(column));
        flame.Position = flame.Position.WithWholeY(40);
        flame.FrameBehavior =
            EnemyProjectileFrameBehavior.PhantoonFlameRainFall;
        return true;
    }

    /// <summary>Allocates a destroyable flame and gives it the idle program and a still, whole-pixel start.</summary>
    /// <returns>The flame, or null when every enemy projectile slot is in use.</returns>
    private EnemyProjectile? AllocateDestroyableFlame(RoomEnemySlot body)
    {
        EnemyProjectile? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            SpriteGraphics.SpawnedBy(body));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.DestroyableIdle));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = flame.Velocity.WithY(SignedEightEight.Zero);
        return flame;
    }
}
