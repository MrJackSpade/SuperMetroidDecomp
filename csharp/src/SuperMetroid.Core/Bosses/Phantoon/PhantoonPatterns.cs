using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
[RoomLifetime]
internal sealed class PhantoonPatterns
{
    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Applies body palettes and BG2 tilemap changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _phantoonPresentation;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonPatterns(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyPresentation presentation, EnemyFrameContext frameContext, PhantoonPresentation phantoonPresentation, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _presentation = presentation;
        _frameContext = frameContext;
        _phantoonPresentation = phantoonPresentation;
    }

    /// <summary>Chooses the first-round movement and flame pattern from encounter state.</summary>
    internal void RunPhantoonPickFirstRoundPattern(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        RoomEnemySlot eye = state.Eye;
        state.Parts[eye.SlotIndex].WaveMode = 0;
        state.Parts[eye.SlotIndex].ClosedTimer = PhantoonTimerDefinitions.EyeClosed[(_frameContext.FrameCounter >> 1) & 3];
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        state.Parts[body.SlotIndex].SpeedFraction = 0;
        state.Parts[body.SlotIndex].SpeedPhase = 0;
        if ((_room.Random.Generate() & 1) != 0)
        {
            state.Parts[body.SlotIndex].SpeedWhole = 0;
            state.Parts[eye.SlotIndex].FigureEightReversed = true;
            state.Parts[body.SlotIndex].PathCursor = 533;
        }
        else
        {
            state.Parts[body.SlotIndex].SpeedWhole = 1;
            state.Parts[eye.SlotIndex].FigureEightReversed = false;
            state.Parts[body.SlotIndex].PathCursor = 0;
        }
    }

    /// <summary>Moves the body through its first-round figure-eight and schedules its next phase.</summary>
    internal void RunPhantoonFirstRoundFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        StepPhantoonFigureEight(body, state.Parts[body.SlotIndex], state.Parts[state.Eye.SlotIndex]);
        StepPhantoonCasualFlameSchedule(state);

        CountdownStep figureEight = WrappingCounter.Decrement(checked((ushort)state.Parts[state.Eye.SlotIndex].ClosedTimer));
        state.Parts[state.Eye.SlotIndex].ClosedTimer = figureEight.Value;
        // Native DEC/BEQ/BPL admits both zero and negative expiration. Waiting
        // for underflow adds a movement/flame frame before the eye-opening handoff.
        if (!figureEight.IsZeroOrNegative)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.NoOperation;
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeOpen);
        state.Parts[body.SlotIndex].FlameRainRequested = false;
        SpawnPhantoonSpiralFlames(body);
    }

    /// <summary>Advances the body and eye on their native figure-eight path.</summary>
    internal static void StepPhantoonFigureEight(RoomEnemySlot body, PhantoonPartState bodyWords, PhantoonPartState eyeWords)
    {
        if (eyeWords.FigureEightReversed)
            AdjustPhantoonReverseFigureEightSpeed(bodyWords);
        else
            AdjustPhantoonForwardFigureEightSpeed(bodyWords);

        int steps = eyeWords.FigureEightReversed
            ? unchecked((ushort)-bodyWords.SpeedWhole)
            : bodyWords.SpeedWhole;
        for (int step = 0; step < steps; step++)
        {
            var (dx, dy) = PhantoonPathDefinitions.Step(bodyWords.PathCursor);
            if (eyeWords.FigureEightReversed)
            {
                body.Position -= new Offset16<RoomPixels>(unchecked(dx), unchecked(dy));
                bodyWords.PathCursor = bodyWords.PathCursor == 0 ? (ushort)(PhantoonPathDefinitions.Length - 1) : unchecked((ushort)(bodyWords.PathCursor - 1));
            }
            else
            {
                body.Position += new Offset16<RoomPixels>(unchecked(dx), unchecked(dy));
                bodyWords.PathCursor = bodyWords.PathCursor >= PhantoonPathDefinitions.Length - 1 ? (ushort)0 : unchecked((ushort)(bodyWords.PathCursor + 1));
            }
        }
    }

    /// <summary>Changes horizontal velocity over the forward half of a figure-eight.</summary>
    internal static void AdjustPhantoonForwardFigureEightSpeed(PhantoonPartState body)
    {
        if (body.SpeedPhase == 0)
        {
            ChangePhantoonSpeed(
                body,
                low: PhantoonMotionDefinitions.SlowFraction,
                high: PhantoonMotionDefinitions.SlowWhole, direction: PhantoonSpeedChange.Accelerate);
            if (unchecked((short)(body.SpeedWhole - PhantoonMotionDefinitions.ForwardSlowCap)) >= 0)
            {
                body.SpeedWhole = unchecked(PhantoonMotionDefinitions.ForwardSlowCap - 1);
                body.SpeedFraction = 0;
                body.SpeedPhase = 1;
            }
            return;
        }

        if ((body.SpeedPhase & 1) != 0)
        {
            ChangePhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Accelerate);
            if (unchecked((short)(body.SpeedWhole - PhantoonMotionDefinitions.ForwardFastCap)) >= 0)
            {
                body.SpeedWhole = PhantoonMotionDefinitions.ForwardFastCap;
                body.SpeedFraction = 0;
                body.SpeedPhase = unchecked((ushort)(body.SpeedPhase + 1));
            }
            return;
        }

        ChangePhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Decelerate);
        ushort minimum = PhantoonMotionDefinitions.ForwardMinimum;
        if (body.SpeedWhole == minimum || unchecked((short)(body.SpeedWhole - minimum)) < 0)
        {
            body.SpeedWhole = unchecked((ushort)(minimum + 1));
            body.SpeedFraction = 0;
            body.SpeedPhase = 0;
        }
    }

    /// <summary>Changes horizontal velocity over the returning half of a figure-eight.</summary>
    internal static void AdjustPhantoonReverseFigureEightSpeed(PhantoonPartState body)
    {
        if (body.SpeedPhase == 0)
        {
            ChangePhantoonSpeed(
                body,
                PhantoonMotionDefinitions.SlowFraction,
                PhantoonMotionDefinitions.SlowWhole, direction: PhantoonSpeedChange.Decelerate);
            ushort cap = PhantoonMotionDefinitions.ReverseSlowCap;
            if (body.SpeedWhole == cap || unchecked((short)(body.SpeedWhole - cap)) < 0)
            {
                body.SpeedWhole = unchecked((ushort)(cap + 2));
                body.SpeedFraction = 0;
                body.SpeedPhase = 1;
            }
            return;
        }

        if ((body.SpeedPhase & 1) != 0)
        {
            ChangePhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Decelerate);
            ushort cap = PhantoonMotionDefinitions.ReverseFastCap;
            if (body.SpeedWhole == cap || unchecked((short)(body.SpeedWhole - cap)) < 0)
            {
                body.SpeedWhole = unchecked((ushort)(cap + 1));
                body.SpeedFraction = 0;
                body.SpeedPhase = unchecked((ushort)(body.SpeedPhase + 1));
            }
            return;
        }

        ChangePhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole, direction: PhantoonSpeedChange.Accelerate);
        ushort maximum = PhantoonMotionDefinitions.ReverseMaximum;
        if (unchecked((short)(body.SpeedWhole - maximum)) >= 0)
        {
            body.SpeedWhole = maximum;
            body.SpeedFraction = 0;
            body.SpeedPhase = 0;
        }
    }

    /// <summary>Adds a native two-word speed value to the body motion accumulator, or subtracts it when slowing.</summary>
    private static void ChangePhantoonSpeed(PhantoonPartState body, ushort low, ushort high, PhantoonSpeedChange direction)
    {
        uint value = SnesSixteenSixteen.Compose(checked((ushort)body.SpeedWhole), checked((ushort)body.SpeedFraction));
        uint change = SnesSixteenSixteen.Compose(high, low);
        value = direction == PhantoonSpeedChange.Decelerate ? unchecked(value - change) : unchecked(value + change);
        body.SpeedFraction = unchecked((ushort)value);
        body.SpeedWhole = SnesFixedPosition.WholeOf(unchecked((int)value));
    }

    /// <summary>Advances the mouth's casual flame spawn timer during ordinary movement.</summary>
    internal void StepPhantoonCasualFlameSchedule(PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth;
        CountdownStep holdStep = WrappingCounter.Decrement(checked((ushort)state.Parts[mouth.SlotIndex].CasualFlameHold));
        state.Parts[mouth.SlotIndex].CasualFlameHold = holdStep.Value;
        if (!holdStep.IsZeroOrNegative)
            return;

        if (unchecked((short)state.Parts[mouth.SlotIndex].CasualFlameCount) >= 0)
        {
            CountdownStep countStep = WrappingCounter.Decrement(checked((ushort)state.Parts[mouth.SlotIndex].CasualFlameCount));
            state.Parts[mouth.SlotIndex].CasualFlameCount = countStep.Value;
            // Native zero and negative counts both terminate the pattern. The
            // inter-pattern delay is word one; word zero is never a timer.
            bool exhausted = countStep.IsZeroOrNegative;
            if (exhausted)
                state.Parts[mouth.SlotIndex].CasualFlameCount = PhantoonCasualFlameDefinitions.NoActiveSchedule;
            int timerIndex = exhausted ? 1 : state.Parts[mouth.SlotIndex].CasualFlameCount + 1;
            state.Parts[mouth.SlotIndex].CasualFlameHold = PhantoonCasualFlameDefinitions.Pattern(state.Parts[mouth.SlotIndex].CasualFlamePattern)[timerIndex];
            PhantoonPresentation.InstallPhantoonProgram(state, mouth, PhantoonPartProgram.MouthSpawnFlame);
            return;
        }

        state.Parts[mouth.SlotIndex].CasualFlamePattern = unchecked((ushort)(_room.Random.Generate() & 3));
        PhantoonCasualFlameDefinitions.Schedule pattern = PhantoonCasualFlameDefinitions.Pattern(state.Parts[mouth.SlotIndex].CasualFlamePattern);
        state.Parts[mouth.SlotIndex].CasualFlameCount = pattern[0];
        state.Parts[mouth.SlotIndex].CasualFlameHold = pattern[state.Parts[mouth.SlotIndex].CasualFlameCount + 1];
    }

    /// <summary>Starts the spiral-flame pattern around the body.</summary>
    /// <remarks>The spiral fireballs are the first ordinary combat attack. Their exact bank-$86 translation belongs to
    /// the next combat slice; retaining the explicit call boundary prevents the eye-opening instruction from being
    /// mistaken for completed behavior.</remarks>
    private void SpawnPhantoonSpiralFlames(RoomEnemySlot body)
    {
        for (int direction = 7; direction >= 0; direction--)
            SpawnPhantoonDestroyableFlame(body, PhantoonFlameSpawnType.Spiral, (byte)direction);
    }

    /// <summary>Ports the eye-open vulnerable window at $A7:D60D.</summary>
    internal static void RunPhantoonEyeTracking(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        if (PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
        {
            // Variable B in the tentacle record is the damage accumulated during this
            // opening. Native clears it before deciding whether a shot requested a swoop.
            state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
            if (!state.Parts[state.Tentacles.SlotIndex].SwoopRequested)
            {
                state.Parts[body.SlotIndex].Function = PhantoonAiFunction.NoOperation;
                PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
                PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeCloseAndPickNewPattern);
                body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);

                // This flag makes PickNewPhantoonPattern choose the initial flame-rain
                // branch after the eye-close list invokes its $D076 callback.
                state.Parts[body.SlotIndex].FlameRainRequested = true;
                return;
            }

            state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
            state.Parts[body.SlotIndex].FunctionTimer = 60;
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BecomeSolidAndSwoop;
        }

        PointPhantoonEyeAtSamus(body, state, samus);
    }

    /// <summary>Ports the one-frame opaque-swoop setup at $A7:D65C.</summary>
    internal static void BeginPhantoonVulnerableSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        state.SemiTransparent = false;
        BeginPhantoonSwoop(body, state);
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.FullHitboxBody);
    }

    /// <summary>Ports the 360-frame opaque swoop at $A7:D678.</summary>
    internal static void RunPhantoonSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.AboveSamus);
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutWhileSwooping;
        state.SemiTransparent = true;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        RoomEnemySlot eye = state.Eye;
        PhantoonPresentation.InstallPhantoonProgram(state, eye, PhantoonPartProgram.EyeClose);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[eye.SlotIndex].FadeComplete = false;
        state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
    }

    /// <summary>Fades the body while continuing its swoop across the room.</summary>
    internal void RunPhantoonFadeOutWhileSwooping(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.AboveSamus);
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WaitAfterFadeOut;
        state.Parts[body.SlotIndex].FunctionTimer = 120;
    }

    /// <summary>Counts the hidden interval before reappearing.</summary>
    internal static void RunPhantoonHiddenWait(PhantoonPartState body)
    {
        if (PhantoonPresentation.TickPhantoonFunctionTimer(body))
            body.Function = PhantoonAiFunction.PickNextAppearance;
    }

    /// <summary>Ports the hidden left/right placement and round-two picker at $A7:D6E2.</summary>
    internal void PlacePhantoonForNextFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if ((_room.Random.Generate() & 1) != 0)
        {
            state.Parts[body.SlotIndex].PathCursor = 136;
            body.Position = body.Position.WithWholeX(208);
        }
        else
        {
            state.Parts[body.SlotIndex].PathCursor = 399;
            body.Position = body.Position.WithWholeX(48);
        }
        body.Position = body.Position.WithWholeY(96);
        state.Parts[state.Eye.SlotIndex].FigureEightReversed = false;
        state.Parts[body.SlotIndex].SpeedWhole = 1;
        state.Parts[body.SlotIndex].SpeedFraction = 0;
        state.Parts[body.SlotIndex].FlameRainRequested = false;

        // The native call deliberately installs a pattern function which is immediately
        // overwritten with the fade-in function. Its timer, direction, and movement-side
        // effects remain live, so call the shared callback implementation in full.
        PickPhantoonSecondRoundPattern(state);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeInBeforeFigureEight;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Reveals Phantoon before the next figure-eight pass.</summary>
    internal void RunPhantoonFadeInBeforeFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (state.Parts[state.Eye.SlotIndex].FadeComplete)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Ports the visible setup for every flame-rain window at $A7:D73F.</summary>
    internal static void BeginPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        state.SemiTransparent = false;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.FullHitboxBody);
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeballCentered);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeInDuringFlameRain;
    }

    /// <summary>Reveals the body while preparing the flame-rain attack.</summary>
    internal void RunPhantoonFlameRainFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 1);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.TrackSamusDuringFlameRain;
        state.Parts[body.SlotIndex].FunctionTimer = 90;
    }

    /// <summary>Counts the open-eye interval during flame rain.</summary>
    internal static void RunPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
        if (state.Parts[state.Tentacles.SlotIndex].SwoopRequested)
        {
            state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
            state.Parts[body.SlotIndex].FlameRainRequested = true;
            BeginPhantoonSwoop(body, state);
            return;
        }

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutDuringFlameRain;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeClose);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.SemiTransparent = true;
    }

    /// <summary>Hides the body after the vulnerable rain interval.</summary>
    internal void RunPhantoonFlameRainFadeOut(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.SpawnFlameRain;
        state.Parts[body.SlotIndex].FunctionTimer = PhantoonTimerDefinitions.RainHiding[_room.Random.Generate() & 7];
    }

    /// <summary>Emits the scheduled rain pattern while Phantoon is hidden.</summary>
    internal void RunPhantoonHiddenFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        ushort pattern = unchecked((ushort)(_room.Random.Generate() & 7));
        var placement = PhantoonPatternDefinitions.RainPlacement(pattern);
        state.Parts[body.SlotIndex].PathCursor = placement.Cursor;
        body.Position = body.Position.WithWhole(placement.Position);
        state.Parts[state.Eye.SlotIndex].FigureEightReversed = false;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    /// <summary>Starts the first rain pattern after the intro round.</summary>
    internal void RunPhantoonInitialFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        RoomEnemySlot eye = state.Eye;
        StepPhantoonFigureEight(body, state.Parts[body.SlotIndex], state.Parts[eye.SlotIndex]);
        StepPhantoonCasualFlameSchedule(state);
        CountdownStep closedStep = WrappingCounter.Decrement(checked((ushort)state.Parts[eye.SlotIndex].ClosedTimer));
        state.Parts[eye.SlotIndex].ClosedTimer = closedStep.Value;
        // $A7:D836 DEC/BEQ/BPL: reaching zero expires the timer, as does underflow.
        if (!closedStep.IsZeroOrNegative)
            return;

        state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.Position.X.Whole < 128 ? (ushort)0 : (ushort)2);
    }

    /// <summary>Hides the body before entering the enraged attack cycle.</summary>
    internal void RunPhantoonFadeOutBeforeRage(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.MoveToTopCenterForRage;
        state.Parts[body.SlotIndex].FunctionTimer = 120;
    }

    /// <summary>Holds the hidden rage phase until its timer expires.</summary>
    internal static void RunPhantoonRageHiddenWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeInForRage;
        body.Position = body.Position.WithWhole(new(128, 32));
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Reveals the body at the start of the enraged swoop.</summary>
    internal void RunPhantoonRageFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.Enraged;
        state.Parts[body.SlotIndex].FunctionTimer = 4;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Ports all eight alternating radial flame waves at $A7:D8AC.</summary>
    internal void RunPhantoonRage(RoomEnemySlot body, PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        if ((state.Parts[state.Eye.SlotIndex].RageWaveCount & 1) == 0)
        {
            for (int direction = PhantoonRageRomData.EvenWaveFirstDirection; direction >= 0; direction--)
                SpawnPhantoonDestroyableFlame(body, PhantoonFlameSpawnType.Enraged, (byte)direction);
        }
        else
        {
            // Native DEY/CPY/BPL includes direction eight: alternating waves
            // intentionally contain seven and eight flames, not seven each.
            for (int direction = PhantoonRageRomData.OddWaveFirstDirection; direction >= PhantoonRageRomData.OddWaveLastDirection; direction--)
                SpawnPhantoonDestroyableFlame(body, PhantoonFlameSpawnType.Enraged, (byte)direction);
        }

        _audio.QueueEnemySound(PhantoonRageRomData.WaveSound, PhantoonRageRomData.WaveSoundQueueCapacity);
        state.Parts[state.Eye.SlotIndex].RageWaveCount = unchecked((ushort)(state.Parts[state.Eye.SlotIndex].RageWaveCount + 1));
        if (unchecked((short)(state.Parts[state.Eye.SlotIndex].RageWaveCount - PhantoonRageRomData.WaveCount)) < 0)
        {
            state.Parts[body.SlotIndex].FunctionTimer = PhantoonRageRomData.WaveInterval;
            return;
        }

        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeClose);
        state.Parts[state.Eye.SlotIndex].RageWaveCount = 0;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutAfterRage;
    }

    /// <summary>Hides the body after the enraged swoop.</summary>
    internal void RunPhantoonRageFadeOut(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WaitAfterFadeOut;
        state.Parts[body.SlotIndex].FunctionTimer = 120;
    }

    /// <summary>Seeds swoop speed and direction from Samus's current position.</summary>
    private static void BeginPhantoonSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot tentacles = state.Tentacles;
        state.Parts[tentacles.SlotIndex].SwoopVelocity = new(new SnesSignedEightEight(1024), new SnesSignedEightEight(1024));
        state.Parts[tentacles.SlotIndex].SwoopTarget = 0;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.Swooping;
        state.Parts[body.SlotIndex].FunctionTimer = 360;
    }

    /// <summary>
    /// Ports $A7:D2D1 using the NTSC/Japan-USA regional immediates present in the target
    /// cartridge. The asymmetric negative velocity thresholds are intentional; replacing
    /// them with tidy +/- clamps changes the retail swoop path.
    /// </summary>
    internal static void MovePhantoonInSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus,
        PhantoonSwoopTarget target)
    {
        RoomEnemySlot tentacles = state.Tentacles;
        ushort targetX;
        if (unchecked((short)state.Parts[tentacles.SlotIndex].SwoopTarget) < 0)
        {
            state.Parts[tentacles.SlotIndex].SwoopTarget = unchecked((ushort)(state.Parts[tentacles.SlotIndex].SwoopTarget - 2));
            targetX = unchecked((ushort)(state.Parts[tentacles.SlotIndex].SwoopTarget & PhantoonMotionDefinitions.SwoopTargetMask));
            if (targetX == 0)
                state.Parts[tentacles.SlotIndex].SwoopTarget = 0;
        }
        else
        {
            state.Parts[tentacles.SlotIndex].SwoopTarget = unchecked((ushort)(state.Parts[tentacles.SlotIndex].SwoopTarget + 2));
            if (unchecked((short)(state.Parts[tentacles.SlotIndex].SwoopTarget - 256)) >= 0)
                state.Parts[tentacles.SlotIndex].SwoopTarget |= PhantoonMotionDefinitions.SwoopReturningBit;
            targetX = unchecked((ushort)(state.Parts[tentacles.SlotIndex].SwoopTarget & PhantoonMotionDefinitions.SwoopTargetMask));
        }

        PhantoonPartState tentacleWords = state.Parts[tentacles.SlotIndex];
        int swoopX = tentacleWords.SwoopVelocity.X.RawValue;
        if (unchecked((short)(targetX - body.Position.X.Whole)) < 0)
        {
            if (unchecked((short)(swoopX - PhantoonMotionDefinitions.SwoopNegativeXThreshold)) >= 0)
                swoopX = unchecked((ushort)(swoopX - 32));
        }
        else if (unchecked((short)(swoopX - 2048)) < 0)
        {
            swoopX = unchecked((ushort)(swoopX + 32));
        }
        tentacleWords.SwoopVelocity = tentacleWords.SwoopVelocity.WithX(new SnesSignedEightEight(checked((ushort)swoopX)));
        body.Position = body.Position.AdvanceX(tentacleWords.SwoopVelocity.X.ToSixteenSixteenDelta());
        if (unchecked((short)(body.Position.X.Whole - PhantoonMotionDefinitions.SwoopMinimumX)) < 0)
            body.Position = body.Position.WithWholeX(PhantoonMotionDefinitions.SwoopMinimumX);
        else if (unchecked((short)(body.Position.X.Whole - PhantoonMotionDefinitions.SwoopMaximumX)) >= 0)
            body.Position = body.Position.WithWholeX(PhantoonMotionDefinitions.SwoopMaximumX);

        ushort targetY = target == PhantoonSwoopTarget.DeathHeight ? (ushort)112 : unchecked((ushort)(samus.Position.Y.Whole - 48));
        int swoopY = tentacleWords.SwoopVelocity.Y.RawValue;
        if (unchecked((short)(targetY - body.Position.Y.Whole)) < 0)
        {
            if (unchecked((short)(swoopY - PhantoonMotionDefinitions.SwoopNegativeYThreshold)) >= 0)
                swoopY = unchecked((ushort)(swoopY - 64));
        }
        else if (unchecked((short)(swoopY - 1536)) < 0)
        {
            swoopY = unchecked((ushort)(swoopY + 64));
        }
        tentacleWords.SwoopVelocity = tentacleWords.SwoopVelocity.WithY(new SnesSignedEightEight(checked((ushort)swoopY)));
        body.Position = body.Position.AdvanceY(tentacleWords.SwoopVelocity.Y.ToSixteenSixteenDelta());
        if (unchecked((short)(body.Position.Y.Whole - 64)) < 0)
            body.Position = body.Position.WithWholeY(64);
        else if (unchecked((short)(body.Position.Y.Whole - 216)) >= 0)
            body.Position = body.Position.WithWholeY(216);
    }

    /// <summary>Selects the eye pose and angle that track Samus from the current body position.</summary>
    internal static void PointPhantoonEyeAtSamus(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.Position.X.Whole - body.Position.X.Whole));
        short deltaY = unchecked((short)(samus.Position.Y.Whole - body.Position.Y.Whole));
        ushort direction;
        if (Math.Abs((int)deltaY) < 32)
            direction = deltaX < 0 ? (ushort)7 : (ushort)2;
        else if (Math.Abs((int)deltaX) < 32)
            direction = deltaY < 0 ? (ushort)0 : (ushort)4;
        else if (deltaX < 0)
            direction = deltaY < 0 ? (ushort)8 : (ushort)6;
        else
            direction = deltaY < 0 ? (ushort)1 : (ushort)3;

        PhantoonPresentation.InstallPhantoonProgram(
            state,
            state.Eye,
            PhantoonPatternDefinitions.EyeProgram(direction));
    }

    /// <summary>Blends the body palette toward hidden colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeOut(
        PhantoonEnemyState state,
        ushort denominator)
    {
        if ((_frameContext.FrameCounter & 1) != 0 || state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        RoomEnemySlot eye = state.Eye;
        state.Parts[eye.SlotIndex].FadeDenominator = denominator;
        ushort numerator = checked((ushort)state.Parts[eye.SlotIndex].FadeNumerator);
        if (unchecked((ushort)(denominator + 1)) < numerator)
        {
            state.Parts[eye.SlotIndex].FadeNumerator = 0;
            state.Parts[eye.SlotIndex].FadeComplete = true;
            return;
        }

        for (int color = 0; color < 16; color++)
        {
            Bgr555 current = _presentation.BossColours().PhantoonBody[color];
            Bgr555 target = (_bossArtwork.Artwork?.PhantoonColors ?? throw new InvalidDataException(
                "Phantoon fade palette requires installed artwork.")).ResolveFadeOut(color);
            _presentation.BossColours().PhantoonBody.Write(
                color,
                PhantoonPresentation.CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        state.Parts[eye.SlotIndex].FadeNumerator = unchecked((ushort)(numerator + 1));
    }

    /// <summary>Places eight delayed rain flames in rotating room columns.</summary>
    private void SpawnPhantoonFlameRain(RoomEnemySlot body, ushort pattern)
    {
        byte column = PhantoonPatternDefinitions.FirstRainColumns[pattern];
        ushort delay = 16;
        for (int flame = 0; flame < 8; flame++)
        {
            SpawnPhantoonDestroyableFlame(body, PhantoonFlameSpawnType.Rain, (byte)(delay | column));
            column++;
            if (column >= 9)
                column = 0;
            delay = unchecked((ushort)(delay + 16));
        }
    }

    /// <summary>Selects a later-round flame and swoop pattern from encounter state.</summary>
    internal void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        RoomEnemySlot eye = state.Eye;
        state.Parts[body.SlotIndex].FunctionTimer = 60;
        state.Parts[eye.SlotIndex].ClosedTimer = PhantoonTimerDefinitions.EyeClosed[_room.Random.Generate() & 7];
        if ((_frameContext.FrameCounter & 1) != 0)
        {
            if (!state.Parts[eye.SlotIndex].FigureEightReversed)
                state.Parts[body.SlotIndex].PathCursor = state.Parts[body.SlotIndex].PathCursor == 0 ? (ushort)533 : unchecked((ushort)(state.Parts[body.SlotIndex].PathCursor - 1));
            state.Parts[body.SlotIndex].SpeedWhole = 0;
            state.Parts[body.SlotIndex].SpeedFraction = 0;
            state.Parts[body.SlotIndex].SpeedPhase = 0;
            state.Parts[eye.SlotIndex].FigureEightReversed = true;
        }
        else
        {
            if (state.Parts[eye.SlotIndex].FigureEightReversed)
                state.Parts[body.SlotIndex].PathCursor = state.Parts[body.SlotIndex].PathCursor >= 533 ? (ushort)0 : unchecked((ushort)(state.Parts[body.SlotIndex].PathCursor + 1));
            state.Parts[body.SlotIndex].SpeedWhole = 1;
            state.Parts[body.SlotIndex].SpeedFraction = 0;
            state.Parts[body.SlotIndex].SpeedPhase = 0;
            state.Parts[eye.SlotIndex].FigureEightReversed = false;
        }

        state.Parts[body.SlotIndex].Function = state.Parts[body.SlotIndex].FlameRainRequested
            ? PhantoonAiFunction.FadeOutBeforeFirstFlameRain
            : PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        if (state.Parts[body.SlotIndex].FlameRainRequested)
            state.Parts[eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>
    /// Initializes destroyable flame $86:9C29. The high parameter byte chooses one of the
    /// four native producers: casual fall, rage split, vertical rain, or spiral.
    /// </summary>
    internal bool SpawnPhantoonDestroyableFlame(RoomEnemySlot body, PhantoonFlameSpawnType type, byte index)
    {
        RoomEnemyProjectileSlot? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.DestroyableIdle));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(0));
        switch (type)
        {
            case PhantoonFlameSpawnType.Casual:
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(0));
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 32))));
                flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualFalling0));
                flame.PreInstruction =
                    EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling;

                // $86:985F changes the spawn definition's $8028 properties to $2028.
                // The falling flame therefore cannot hurt Samus and cannot be shot until
                // it has struck the floor and changed into the bouncing form.
                flame.CanDamageSamus = false;
                flame.BlocksSamusProjectiles = false;
                break;

            case PhantoonFlameSpawnType.Enraged:
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(index >= 8
                    ? -PhantoonFlameMotionRomData.RageAngleStep : PhantoonFlameMotionRomData.RageAngleStep))));
                flame.Variable0 = PhantoonFlameSpawnDefinitions.RageAngle(index);
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 32))));
                flame.PreInstruction =
                    EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Enraged;
                break;

            case PhantoonFlameSpawnType.Rain:
                // The index byte is packed as dx: the low nibble is a column and the high
                // nibble is the delayed-fall timer divided by eight.
                byte column = (byte)(index % 16);
                if (column > 8)
                    throw new InvalidDataException($"Phantoon rain column {column} exceeds eight.");
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight((ushort)(index / 16 * 8)));
                flame.Position = flame.Position.WithWholeX(PhantoonFlameSpawnDefinitions.RainX(column));
                flame.Position = flame.Position.WithWholeY(40);
                flame.PreInstruction =
                    EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Rain;
                break;

            case PhantoonFlameSpawnType.Spiral:
                if (index > 7)
                    throw new InvalidDataException($"Phantoon spiral direction {index} exceeds seven.");
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(128));
                flame.Variable0 = PhantoonFlameSpawnDefinitions.SpiralAngle(index);
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 16))));
                flame.PreInstruction =
                    EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Spiral;
                break;

            default:
                throw new InvalidOperationException($"Undefined Phantoon flame spawn type {type}.");
        }
        return true;
    }
}
