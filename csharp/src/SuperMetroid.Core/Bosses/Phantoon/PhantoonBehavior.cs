using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The Phantoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PhantoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonEye, EnemyDefinitionId.PhantoonMouth, EnemyDefinitionId.PhantoonTentacles];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PhantoonEntity(this, slot, common);

    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during Phantoon's death sequence.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the body, eye, mouth, and tentacle slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies body palettes and BG2 tilemap changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves Phantoon and flame contact bounds.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds room and enemy services for the four-part boss actor.</summary>
    internal PhantoonBehavior(PhantoonBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>Native per-update increase in introductory wave amplitude.</summary>
    private const ushort PhantoonIntroAmplitudeDelta = 64;
    /// <summary>Wave amplitude at which the intro advances to the next phase.</summary>
    private const ushort PhantoonIntroMaximumAmplitude = 3072;
    /// <summary>Per-update phase increment for the wavy BG2 distortion.</summary>
    private const ushort PhantoonWavyPhaseDelta = 8;

    /// <summary>Ports <c>InitAI_PhantoonBody</c> at $A7:CDF3.</summary>
    private void InitializePhantoonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Phantoon body requires native slot zero, not slot {body.SlotIndex}.");
        }

        Phantoon = new PhantoonEnemyState(body)
        {
            BackgroundTilemapPrepared = true,
        };

        // Native clears all $1000 bytes of the enemy BG2 working image to tile $0338.
        // This renderer keeps the visible mirror in VRAM, so one exact word transfer gives
        // ProcessExtendedTilemap the same clean surface used by the subsequent body maps.
        VramRegion bg2 = new EnemyVram(_presentation.Vram).EnemyBg2Tilemap;
        ushort[] clearedTilemap = new ushort[bg2.WordCount];
        Array.Fill(clearedTilemap, PhantoonBg2FrameDefinitions.BlankTile);
        bg2.Write(clearedTilemap);

        // Target BG palette seven is black at fight start. In the software PPU CGRAM is the
        // renderer-visible incarnation of native current/target palette storage; clearing
        // these exact sixteen words makes the later health-palette interpolation observable.
        _presentation.BossPalettes.PhantoonBody.Fill(Bgr555.Black);

        Part(body).FunctionTimer = PhantoonTimerDefinitions.InitialFlameDelayFrames;
        Part(body).StartingFlameIndex = 0;
        Part(body).StartingFlamesActive = false;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        InitializePhantoonPart(body, PhantoonPartProgram.InvulnerableBody);
    }

    /// <summary>Ports the shared part initializer at $A7:CE55.</summary>
    private void InitializePhantoonPart(RoomEnemySlot part)
    {
        PhantoonEnemyState state = RequirePhantoonState(part);
        PhantoonPartProgram program = part.Parameter2 switch
        {
            1 => PhantoonPartProgram.EyeClosed,
            2 => PhantoonPartProgram.Tentacles,
            3 => PhantoonPartProgram.MouthInitial,
            _ => throw new InvalidDataException(
                $"Phantoon part slot {part.SlotIndex} has invalid part index ${part.Parameter2:X4}."),
        };
        InitializePhantoonPart(part, program);

        switch (part.Parameter2)
        {
            case 1:
                state.Eye = part;
                break;
            case 2:
                state.Tentacles = part;
                break;
            case 3:
                state.Mouth = part;
                break;
        }
    }

    /// <summary>Copies body presentation state into a physical part and starts its chosen program.</summary>
    private void InitializePhantoonPart(RoomEnemySlot part, PhantoonPartProgram program)
    {
        PhantoonEnemyState state = Phantoon ??
            throw new InvalidOperationException("Phantoon body must initialize before its parts.");
        part.Frame = EnemyFrameId.Empty;
        part.InstructionTimer = NativeCountdown.Start(1);
        part.Timer = NativeCountdown.Start(0);
        part.PaletteIndex = state.Body.PaletteIndex;
        part.VramTilesIndex = state.Body.VramTilesIndex;
        state.PartProgram(part) = AnimationProgramCursor<PhantoonPartProgram>.Start(program);
        PhantoonPartState words = state.Parts[part.SlotIndex];
        part.SetSpeciesState(words, words.ClearWithRecord);
        state.Parts[part.SlotIndex].Function = PhantoonAiFunction.SpawnStartingFlames;

        // Every call writes these shared aliases, including the body fallthrough. Slot three
        // owns the casual-flame pattern index/control words used much later in the fight.
        if (_slotTable._slots.Length > 3)
        {
            _slotTable._slots[3].Parameter1 = 0;
            state.Parts[3].CasualFlameCount = PhantoonCasualFlameDefinitions.NoActiveSchedule;
        }
    }

    /// <summary>Ports <c>MainAI_Phantoon</c> at $A7:CEA6.</summary>
    private void RunPhantoonMain(
        RoomEnemySlot body,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(body);
        state.LastMaterializationSound = null;
        PhantoonAiFunction dispatched = state.Parts[body.SlotIndex].Function;

        switch (dispatched)
        {
            case PhantoonAiFunction.SpawnStartingFlames:
                RunPhantoonStartingFlameSpawner(body, state);
                break;
            case PhantoonAiFunction.WaitBeforeActivatingStartingFlames:
                RunPhantoonStartingFlamePause(state.Parts[body.SlotIndex]);
                break;
            case PhantoonAiFunction.WaitForStartingFlamesToDisappear:
                RunPhantoonStartingFlameOrbitWait(body, state);
                break;
            case PhantoonAiFunction.WavyFadeIn:
                RunPhantoonWavyFadeIn(body, state);
                break;
            case PhantoonAiFunction.PickFirstRoundPattern:
                RunPhantoonPickFirstRoundPattern(body, state);
                break;
            case PhantoonAiFunction.MoveInFigureEightThenOpenEye:
                RunPhantoonFirstRoundFigureEight(body, state);
                break;
            case PhantoonAiFunction.EyeTracksSamus:
                RunPhantoonEyeTracking(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.BecomeSolidAndSwoop:
                BeginPhantoonVulnerableSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.Swooping:
                RunPhantoonSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.FadeOutWhileSwooping:
                RunPhantoonFadeOutWhileSwooping(
                    body,
                    state,
                    RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.WaitAfterFadeOut:
                RunPhantoonHiddenWait(state.Parts[body.SlotIndex]);
                break;
            case PhantoonAiFunction.PickNextAppearance:
                PlacePhantoonForNextFigureEight(body, state);
                break;
            case PhantoonAiFunction.FadeInBeforeFigureEight:
                RunPhantoonFadeInBeforeFigureEight(body, state);
                break;
            case PhantoonAiFunction.BecomeSolidAfterFlameRain:
                BeginPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonAiFunction.FadeInDuringFlameRain:
                RunPhantoonFlameRainFadeIn(body, state);
                break;
            case PhantoonAiFunction.TrackSamusDuringFlameRain:
                RunPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonAiFunction.FadeOutDuringFlameRain:
                RunPhantoonFlameRainFadeOut(body, state);
                break;
            case PhantoonAiFunction.SpawnFlameRain:
                RunPhantoonHiddenFlameRain(body, state);
                break;
            case PhantoonAiFunction.FadeOutBeforeFirstFlameRain:
                RunPhantoonInitialFlameRain(body, state);
                break;
            case PhantoonAiFunction.FadeOutBeforeRage:
                RunPhantoonFadeOutBeforeRage(body, state);
                break;
            case PhantoonAiFunction.MoveToTopCenterForRage:
                RunPhantoonRageHiddenWait(body, state);
                break;
            case PhantoonAiFunction.FadeInForRage:
                RunPhantoonRageFadeIn(body, state);
                break;
            case PhantoonAiFunction.Enraged:
                RunPhantoonRage(body, state);
                break;
            case PhantoonAiFunction.FadeOutAfterRage:
                RunPhantoonRageFadeOut(body, state);
                break;
            case PhantoonAiFunction.FinishFatalSwoop:
                RunPhantoonFatalSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.DyingFadeInOut:
                RunPhantoonDyingFadeCycles(body, state);
                break;
            case PhantoonAiFunction.DyingExplosions:
                RunPhantoonDyingExplosions(body, state);
                break;
            case PhantoonAiFunction.BeginFinalWavyDeath:
                BeginPhantoonWavyMosaicDeath(body, state);
                break;
            case PhantoonAiFunction.DyingFadeOut:
                RunPhantoonWavyMosaicDeath(body, state);
                break;
            case PhantoonAiFunction.AlmostDead:
                ClearPhantoonDeathGraphics(body, state);
                break;
            case PhantoonAiFunction.Dead:
                ActivateWreckedShipAfterPhantoon(body, state);
                break;
            case PhantoonAiFunction.NoOperation:
                break;
            default:
                throw new InvalidDataException(
                    $"Phantoon function {state.Parts[body.SlotIndex].Function} is not translated.");
        }

        // The source contains a documented X-register bug that makes only the slot-zero
        // invocation run this synchronization block. Our main dispatcher likewise calls it
        // only for the body, then copies the updated point into the three drawing records.
        // $A7:CEB2 tests the X the function returned with: $DAD7 leaves it at the VRAM
        // write-stack index, so that frame skips the part sync and BG2 scroll entirely.
        if (dispatched == PhantoonAiFunction.AlmostDead)
            return;
        state.Mouth.Position = state.Mouth.Position.WithWholeX(body.Position.X.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Mouth.Position = state.Mouth.Position.WithWholeY(body.Position.Y.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
        if (state.Eye.Parameter1 == 0)
        {
            state.Bg2Scroll = new(
                unchecked((ushort)(camera.X - body.Position.X.Whole + 40)),
                unchecked((ushort)(camera.Y - body.Position.Y.Whole + 40)));
        }
    }

    /// <summary>Emits the intro flames on the authored schedule before Phantoon materializes.</summary>
    internal void RunPhantoonStartingFlameSpawner(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.StartingFlameRequests++;
        if (SpawnPhantoonStartingFlame(body, unchecked((byte)state.Parts[body.SlotIndex].StartingFlameIndex)))
            state.StartingFlamesSpawned++;
        _audio.QueueEnemySound(PhantoonSoundDefinitions.StartingFlame, PhantoonSoundDefinitions.StartingFlameQueueCapacity);
        state.Parts[body.SlotIndex].FunctionTimer = 30;
        state.Parts[body.SlotIndex].StartingFlameIndex = unchecked((ushort)(state.Parts[body.SlotIndex].StartingFlameIndex + 1));
        if (unchecked((short)(state.Parts[body.SlotIndex].StartingFlameIndex - 8)) < 0)
            return;

        state.Parts[body.SlotIndex].StartingFlameIndex = 0;
        state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WaitBeforeActivatingStartingFlames;
        state.Parts[body.SlotIndex].FunctionTimer = 30;
        state.BossDoorPlmRequest = PlmHeaderId.DrawPhantoonDoorDuringBossFight;
    }

    /// <summary>Holds the body between intro flame spawns.</summary>
    private static void RunPhantoonStartingFlamePause(PhantoonPartState body)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;
        body.FunctionTimer = 240;
        body.StartingFlamesActive = true;
        body.Function = PhantoonAiFunction.WaitForStartingFlamesToDisappear;
    }

    /// <summary>Waits for the initial flames to finish orbiting before the next phase.</summary>
    private static void RunPhantoonStartingFlameOrbitWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.Parts[body.SlotIndex].StartingFlamesActive = false;
        state.SemiTransparent = true;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WavyFadeIn;
        state.Mouth.Parameter1 = PhantoonBlendingRomData.WavyFadeInControl;
        state.Parts[body.SlotIndex].FunctionTimer = 120;
        state.Parts[state.Mouth.SlotIndex].WaveAmplitude = PhantoonIntroMaximumAmplitude;
        state.Parts[state.Mouth.SlotIndex].WaveAmplitudeShrinking = false;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        state.MusicRequest = MusicCommand.SelectTrack(5);
        state.Parts[state.Mouth.SlotIndex].WavyPhaseDelta = PhantoonWavyPhaseDelta;
        state.Tentacles.Parameter1 = PhantoonWaveRomData.IntroMode;
        state.Wave.Begin(PhantoonWaveRomData.IntroMode);
    }

    /// <summary>Raises intro wave amplitude and reveals the body palette.</summary>
    private void RunPhantoonWavyFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (AdvancePhantoonWaveAmplitude(
                state.Parts[state.Mouth.SlotIndex],
                PhantoonIntroAmplitudeDelta,
                PhantoonIntroMaximumAmplitude))
        {
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.PickFirstRoundPattern;
            state.Mouth.Parameter1 = 1;
            state.Parts[body.SlotIndex].FunctionTimer = 30;
            return;
        }

        if (TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            state.Parts[state.Mouth.SlotIndex].WaveAmplitudeShrinking = true;
    }

    /// <summary>Chooses the first-round movement and flame pattern from encounter state.</summary>
    internal void RunPhantoonPickFirstRoundPattern(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        RoomEnemySlot eye = state.Eye;
        eye.Parameter1 = 0;
        state.Parts[eye.SlotIndex].ClosedTimer = PhantoonTimerDefinitions.EyeClosed[(_frameContext._enemyFrameNmiFrameCounter >> 1) & 3];
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

        NativeWordCounterStep figureEight = NativeWordCounter.Decrement(checked((ushort)state.Parts[state.Eye.SlotIndex].ClosedTimer));
        state.Parts[state.Eye.SlotIndex].ClosedTimer = figureEight.Value;
        // Native DEC/BEQ/BPL admits both zero and negative expiration. Waiting
        // for underflow adds a movement/flame frame before the eye-opening handoff.
        if (!figureEight.IsZeroOrNegative)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.NoOperation;
        InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeOpen);
        body.Parameter2 = 0;
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
                body.Position -= new Offset16<RoomPixels>(unchecked((short)(dx)), unchecked((short)(dy)));
                bodyWords.PathCursor = bodyWords.PathCursor == 0 ? (ushort)(PhantoonPathDefinitions.Length - 1) : unchecked((ushort)(bodyWords.PathCursor - 1));
            }
            else
            {
                body.Position += new Offset16<RoomPixels>(unchecked((short)(dx)), unchecked((short)(dy)));
                bodyWords.PathCursor = bodyWords.PathCursor >= PhantoonPathDefinitions.Length - 1 ? (ushort)0 : unchecked((ushort)(bodyWords.PathCursor + 1));
            }
        }
    }

    /// <summary>Changes horizontal velocity over the forward half of a figure-eight.</summary>
    internal static void AdjustPhantoonForwardFigureEightSpeed(PhantoonPartState body)
    {
        if (body.SpeedPhase == 0)
        {
            AddPhantoonSpeed(
                body,
                low: PhantoonMotionDefinitions.SlowFraction,
                high: PhantoonMotionDefinitions.SlowWhole);
            if (unchecked((short)(body.SpeedWhole - PhantoonMotionDefinitions.ForwardSlowCap)) >= 0)
            {
                body.SpeedWhole = unchecked((ushort)(PhantoonMotionDefinitions.ForwardSlowCap - 1));
                body.SpeedFraction = 0;
                body.SpeedPhase = 1;
            }
            return;
        }

        if ((body.SpeedPhase & 1) != 0)
        {
            AddPhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole);
            if (unchecked((short)(body.SpeedWhole - PhantoonMotionDefinitions.ForwardFastCap)) >= 0)
            {
                body.SpeedWhole = PhantoonMotionDefinitions.ForwardFastCap;
                body.SpeedFraction = 0;
                body.SpeedPhase = unchecked((ushort)(body.SpeedPhase + 1));
            }
            return;
        }

        SubtractPhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole);
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
            SubtractPhantoonSpeed(
                body,
                PhantoonMotionDefinitions.SlowFraction,
                PhantoonMotionDefinitions.SlowWhole);
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
            SubtractPhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole);
            ushort cap = PhantoonMotionDefinitions.ReverseFastCap;
            if (body.SpeedWhole == cap || unchecked((short)(body.SpeedWhole - cap)) < 0)
            {
                body.SpeedWhole = unchecked((ushort)(cap + 1));
                body.SpeedFraction = 0;
                body.SpeedPhase = unchecked((ushort)(body.SpeedPhase + 1));
            }
            return;
        }

        AddPhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole);
        ushort maximum = PhantoonMotionDefinitions.ReverseMaximum;
        if (unchecked((short)(body.SpeedWhole - maximum)) >= 0)
        {
            body.SpeedWhole = maximum;
            body.SpeedFraction = 0;
            body.SpeedPhase = 0;
        }
    }

    /// <summary>Adds a native two-word speed value to the body motion accumulator.</summary>
    private static void AddPhantoonSpeed(PhantoonPartState body, ushort low, ushort high)
    {
        uint value = SnesSixteenSixteen.Compose(checked((ushort)body.SpeedWhole), checked((ushort)body.SpeedFraction));
        value = unchecked(value + (SnesSixteenSixteen.Compose(high, low)));
        body.SpeedFraction = unchecked((ushort)value);
        body.SpeedWhole = SnesFixedPosition.WholeOf(unchecked((int)value));
    }

    /// <summary>Subtracts a native two-word speed value from the body motion accumulator.</summary>
    private static void SubtractPhantoonSpeed(PhantoonPartState body, ushort low, ushort high)
    {
        uint value = SnesSixteenSixteen.Compose(checked((ushort)body.SpeedWhole), checked((ushort)body.SpeedFraction));
        value = unchecked(value - (SnesSixteenSixteen.Compose(high, low)));
        body.SpeedFraction = unchecked((ushort)value);
        body.SpeedWhole = SnesFixedPosition.WholeOf(unchecked((int)value));
    }

    /// <summary>Advances the mouth's casual flame spawn timer during ordinary movement.</summary>
    internal void StepPhantoonCasualFlameSchedule(PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth;
        NativeWordCounterStep holdStep = NativeWordCounter.Decrement(checked((ushort)state.Parts[mouth.SlotIndex].CasualFlameHold));
        state.Parts[mouth.SlotIndex].CasualFlameHold = holdStep.Value;
        if (!holdStep.IsZeroOrNegative)
            return;

        if (unchecked((short)state.Parts[mouth.SlotIndex].CasualFlameCount) >= 0)
        {
            NativeWordCounterStep countStep = NativeWordCounter.Decrement(checked((ushort)state.Parts[mouth.SlotIndex].CasualFlameCount));
            state.Parts[mouth.SlotIndex].CasualFlameCount = countStep.Value;
            // Native zero and negative counts both terminate the pattern. The
            // inter-pattern delay is word one; word zero is never a timer.
            bool exhausted = countStep.IsZeroOrNegative;
            if (exhausted)
                state.Parts[mouth.SlotIndex].CasualFlameCount = PhantoonCasualFlameDefinitions.NoActiveSchedule;
            int timerIndex = exhausted ? 1 : state.Parts[mouth.SlotIndex].CasualFlameCount + 1;
            state.Parts[mouth.SlotIndex].CasualFlameHold = PhantoonCasualFlameDefinitions.Pattern(state.Parts[mouth.SlotIndex].CasualFlamePattern)[timerIndex];
            InstallPhantoonProgram(state, mouth, PhantoonPartProgram.MouthSpawnFlame);
            return;
        }

        state.Parts[mouth.SlotIndex].CasualFlamePattern = unchecked((ushort)(_room.Random.Generate() & 3));
        PhantoonCasualFlameDefinitions.Schedule pattern = PhantoonCasualFlameDefinitions.Pattern(state.Parts[mouth.SlotIndex].CasualFlamePattern);
        state.Parts[mouth.SlotIndex].CasualFlameCount = pattern[0];
        state.Parts[mouth.SlotIndex].CasualFlameHold = pattern[state.Parts[mouth.SlotIndex].CasualFlameCount + 1];
    }

    /// <summary>Blends the body palette toward its visible colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        ushort denominator)
    {
        if ((_frameContext._enemyFrameNmiFrameCounter & 1) != 0 || state.Parts[state.Eye.SlotIndex].FadeComplete)
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

        int healthBand = Math.Min(7, Math.Max(0, (body.Health - 1) / 312));
        for (int color = 0; color < 16; color++)
        {
            Bgr555 current = _presentation.BossPalettes.PhantoonBody[color];
            Bgr555 target = ReadPhantoonHealthColor(healthBand, color);
            _presentation.BossPalettes.PhantoonBody.Write(
                color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        state.Parts[eye.SlotIndex].FadeNumerator = unchecked((ushort)(numerator + 1));
    }

    /// <summary>Interpolates one RGB5 color during Phantoon's fade using native channel rounding.</summary>
    internal static Bgr555 CalculatePhantoonTransitionColor(
        ushort numerator,
        ushort denominator,
        Bgr555 current,
        Bgr555 target)
    {
        return current.Zip(target, (_, from, to) =>
            CalculatePhantoonTransitionComponent(numerator, denominator, from, to));
    }

    /// <summary>Interpolates one RGB5 channel during a palette transition.</summary>
    private static int CalculatePhantoonTransitionComponent(
        ushort numerator,
        ushort denominator,
        int current,
        int target)
    {
        if (numerator == 0)
            return current;
        if (numerator - 1 == denominator)
            return target;
        // $A7:DCF1 divides the absolute component delta in 8.8 fixed point,
        // then applies its sign before selecting the high byte. Integer division
        // of the signed delta both adds an extra fade step and rounds darkening
        // in the wrong direction. The native divisor is an eight-bit register.
        byte remainingSteps = unchecked((byte)(denominator - numerator + 1));
        int quotient = remainingSteps == 0 ? ushort.MaxValue : SnesEightEight.FromWhole(Math.Abs(target - current)) / remainingSteps;
        int signedStep = target < current ? -quotient : quotient;
        return SnesLittleEndian.HighByte((ushort)(SnesEightEight.FromWhole(current) + signedStep));
    }

    /// <summary>Changes BG2 wave amplitude toward its current phase target.</summary>
    private static bool AdvancePhantoonWaveAmplitude(
        PhantoonPartState mouth,
        ushort delta,
        ushort maximum)
    {
        if (mouth.WaveAmplitudeShrinking)
        {
            ushort previous = checked((ushort)mouth.WaveAmplitude);
            mouth.WaveAmplitude = unchecked((ushort)(mouth.WaveAmplitude - delta));
            if (previous < delta)
            {
                mouth.WaveAmplitude = 0;
                return true;
            }
            return false;
        }

        mouth.WaveAmplitude = unchecked((ushort)(mouth.WaveAmplitude + delta));
        if (mouth.WaveAmplitude >= maximum)
            mouth.WaveAmplitude = maximum;
        return false;
    }

    /// <summary>Counts the body function timer down and reports its expiry.</summary>
    private static bool TickPhantoonFunctionTimer(PhantoonPartState body)
    {
        NativeWordCounterStep step = NativeWordCounter.Decrement(checked((ushort)body.FunctionTimer));
        body.FunctionTimer = step.Value;
        return step.IsZeroOrNegative;
    }

    /// <summary>Requires the body's encounter state; each part property requires its attached slot when read.</summary>
    internal PhantoonEnemyState RequireCompletePhantoonState(RoomEnemySlot body) => RequirePhantoonState(body);

    /// <summary>Requires a live Samus state for Phantoon movement and eye targeting.</summary>
    private static SamusState RequirePhantoonSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Phantoon's eye and swoop programs require the active Samus actor.");

    // The spiral fireballs are the first ordinary combat attack. Their exact bank-$86
    // translation belongs to the next combat slice; retaining the explicit call boundary
    // prevents the eye-opening instruction from being mistaken for completed behavior.
    /// <summary>Starts the spiral-flame pattern around the body.</summary>
    private void SpawnPhantoonSpiralFlames(RoomEnemySlot body)
    {
        for (int direction = 7; direction >= 0; direction--)
            SpawnPhantoonDestroyableFlame(body, PhantoonFlameSpawnType.Spiral, (byte)direction);
    }

    /// <summary>Binds bomb hit responses for vulnerable Phantoon phases.</summary>
    internal void RegisterPhantoonBombReactions(EnemySharedReactions table)
    {
        // Common bomb damage. Rinka, Skree, and Powamp deliberately call the no-death-animation form of common shot
        // AI; their private callbacks own slot cleanup or a staged death. Bank-$A8 callbacks reuse the same
        // translated post-common routines as beams. Normal bombs cannot freeze, but their damage, staged-death,
        // child synchronization, recoil, release, and attack side effects are otherwise the same callback tails.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.PhantoonShot)
        {
            RunsGenericDeath = false,
            // Phantoon's `$A7:DD9B` hitbox callback enters the same no-death common-damage routine for bombs as it
            // does for beams and missiles, then consumes the exact damage delta in its encounter reaction state
            // machine. A lethal hit begins the authored death sequence, while sub-threshold damage feeds the
            // eye-close/rage accumulator instead of deleting a slot.
            AfterCommon = c =>
            {
                PhantoonEnemyState phantoon = Phantoon ??
                    throw new InvalidOperationException("Phantoon shot callback has no encounter state.");
                int appliedDamage = c.HealthBefore - c.Enemy.Health;
                phantoon.AcceptedProjectileHits++;
                ResolvePhantoonShotReaction(c.Enemy, phantoon, c.Bomb.Type, appliedDamage);
            },
        });
    }

    /// <summary>Registers Phantoon flame initializers, pre-instructions, and instructions.</summary>
    internal void RegisterPhantoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunPhantoonStartingFlameWaiting(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonStartingFlames);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonStartingFlameOrbit(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonStartingFlames_Activated);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonCasualFlameFalling(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonCasualFlameImpactPause(context.Projectile, _projectiles.CurrentFrame16);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_HitGround);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonCasualFlameBouncing(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Bouncing);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonEnragedFlame(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Enraged);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonRainFlame(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Rain);

        table.RegisterPreInstruction(context =>
        {
            RunPhantoonSpiralFlame(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Spiral);

        table.RegisterShotAnimation(RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            () => new PhantoonFlameAnimation(PhantoonFlameProgram.DestroyableShot));
        table.RegisterShotAnimation(RoomEnemyProjectileKind.PhantoonStartingFlame,
            () => new PhantoonFlameAnimation(PhantoonFlameProgram.DestroyableIdle));
        table.RegisterCue<PhantoonFlameCue>((projectile, samus, cue) =>
        {
            switch (cue)
            {
                case PhantoonFlameCue.SpawnDrop:
                    RequestPhantoonFlameDrop(projectile);
                    return;
                case PhantoonFlameCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameCue)} {cue}.");
            }
        });
    }

    /// <summary>Ports PhantoonBody's initialization AI.</summary>
    internal void RunPhantoonBodyInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializePhantoonBody(slot);
    }

    /// <summary>Ports PhantoonEye's initialization AI.</summary>
    internal void RunPhantoonEyeInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializePhantoonPart(slot);
    }

    /// <summary>Accumulated damage required to alter Phantoon's vulnerable-eye phase.</summary>
    private const ushort PhantoonDamageThreshold = 300;

    /// <summary>
    /// Runs bank-$A0's five-slot beam/missile walk for Phantoon, then dispatches the
    /// selected bank-$A7 hitbox callback. One projectile resolves per pass, matching the
    /// native early exit after the first overlapping enemy/component pair.
    /// </summary>
    public int ResolvePhantoonProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        if (Phantoon is not { } state)
            return 0;

        RoomEnemySlot body = state.Body;
        if (body.EnemyDefinitionPointer != EnemyDefinitionId.PhantoonBody ||
            CommonEnemyEmptyExtendedFrameDefinitions.DrawsNothing(EnemyFrames.Spritemap(body.Frame)) ||
            !body.InvincibilityTimer.IsZero ||
            body.Properties.HasAny(
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return 0;
        }

        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            if (!projectile.HasEnemyCollisionPayload)
                continue;

            ushort projectileType = projectile.Type;
            ushort projectileDamage = checked((ushort)projectile.Damage);
            SamusProjectileFamily family = projectile.PackedType.Family;
            // A0:9BE6..9BF6 excludes bombs and every family at/above beam
            // explosions. Lingering missile explosions must not reach shot AI.
            if (family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or
                >= SamusProjectileFamily.BeamExplosion)
                continue;

            if (!body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
            {
                throw new InvalidDataException(
                    "Phantoon lost the extended-spritemap property required by his hitboxes.");
            }

            if (!_collision.TryFindExtendedHitboxCallback(
                    body,
                    projectile.Position.Whole,
                    projectile.Radius,
                    selectShotCallback: true,
                    out ushort hitboxShotAiPointer))
            {
                continue;
            }
            EnemyInteractionCallback? hitboxShotAi = EnemyCollision.HitboxCallback(body, hitboxShotAiPointer);

            // `$A0:9B7F` applies this before calling the selected component function. A
            // no-op shell hit therefore still marks the beam and shakes for a Super Missile.
            if (family == SamusProjectileFamily.SuperMissile)
            {
                _earthquake.EarthquakeTimer = NativeCountdown.Start(SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration);
                _earthquake.EarthquakeType = SamusProjectileRomData.NonBeam.SuperMissileEnemyHitEarthquakeType;
            }

            if (hitboxShotAi == EnemyInteractionCallback.NoOp ||
                Part(body).Function >= PhantoonAiFunction.DyingFadeInOut)
            {
                projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(
                    projectile.SlotIndex,
                    body.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                        !new SamusProjectileTypeWord(projectile.Type).HasAnyBeam(SamusBeamFlags.Plasma));
                return 1;
            }

            if (hitboxShotAi != EnemyInteractionCallback.PhantoonShot)
            {
                throw new InvalidDataException(
                    $"Phantoon hitbox shot AI $A7:{hitboxShotAiPointer:X4} is not translated.");
            }

            // The native extended-hitbox walker marks the projectile before $A7:DD9B.
            // That callback's damage helper does not convert it into an explosion:
            // the projectile pre-instruction owns the following-frame deletion, while
            // penetrating Plasma retains its type, direction, and instruction stream.
            projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(projectile.SlotIndex,
                body.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                !new SamusProjectileTypeWord(projectileType).HasAnyBeam(SamusBeamFlags.Plasma));

            int healthBefore = body.Health;
            // Phantoon calls common normal-shot damage, including its charged-beam
            // override. SBA particles retain that bit; reading only the uncharged
            // combination row made Wave Shield disappear without damaging the boss.
            EnemyCollision.NormalShotVulnerability vulnerability =
                EnemyCollision.ReadNormalShotVulnerability(body, new SamusProjectileTypeWord(projectileType));
            if (vulnerability.FreezeImmediately)
            {
                body.FrozenTimer = NativeCountdown.Start(400);
                body.AiStates |= EnemyAiStates.Frozen;
                body.InvincibilityTimer = NativeCountdown.Start(10);
            }
            else
            {
                int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
                if (damage != 0)
                {
                    body.FlashTimer = NativeCountdown.Start(body.HurtFlashFrames + 8);
                    body.AiStates |= EnemyAiStates.Hurt;
                    // Plasma alone installs this clock; ordinary missiles retain
                    // the timer behavior of the shared native damage helper.
                    if (new SamusProjectileTypeWord(projectileType).HasAnyBeam(SamusBeamFlags.Plasma))
                        body.InvincibilityTimer = NativeCountdown.Start(EnemyShotTiming.PlasmaInvincibilityFrames);
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                }
            }

            int appliedDamage = healthBefore - body.Health;
            state.AcceptedProjectileHits++;
            ResolvePhantoonShotReaction(
                body,
                state,
                projectileType,
                appliedDamage);
            return 1;
        }

        return 0;
    }

    /// <summary>Ports the boss-owned tail of $A7:DD9B after common vulnerability damage.</summary>
    internal void ResolvePhantoonShotReaction(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        ushort projectileType,
        int damage)
    {
        if (body.Health == 0)
        {
            state.LastCombatSoundEffect = PhantoonSoundDefinitions.ShotCry;
            state.Tentacles.Parameter2 = 1;
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            BeginPhantoonDeathSequence(body, state);
            return;
        }

        if ((body.AiStates & EnemyAiStates.Hurt) == 0)
            return;

        state.LastCombatSoundEffect = PhantoonSoundDefinitions.ShotCry;
        SamusProjectileFamily family = new SamusProjectileTypeWord(projectileType).Family;
        PhantoonAiFunction function = state.Parts[body.SlotIndex].Function;
        bool vulnerableWindow = function is
            PhantoonAiFunction.EyeTracksSamus or
            PhantoonAiFunction.TrackSamusDuringFlameRain;
        bool swooping = function == PhantoonAiFunction.Swooping;

        if (vulnerableWindow || swooping)
        {
            // A single Super Missile (600 retail damage) skips ordinary round accumulation
            // and enters the eight-wave rage sequence immediately.
            if (damage >= PhantoonDamageThreshold &&
                family == SamusProjectileFamily.SuperMissile)
            {
                ClosePhantoonAfterDamage(body, state, enraged: true);
                return;
            }

            state.Parts[state.Tentacles.SlotIndex].RoundDamage = unchecked((ushort)(
                state.Parts[state.Tentacles.SlotIndex].RoundDamage + damage));
            if (state.Parts[state.Tentacles.SlotIndex].RoundDamage >= PhantoonDamageThreshold)
            {
                if (swooping)
                    state.Parts[body.SlotIndex].FunctionTimer = 1;
                else
                    ClosePhantoonAfterDamage(body, state, enraged: false);
                state.Tentacles.Parameter2 = 2;
                return;
            }

            if (vulnerableWindow)
            {
                ushort random = unchecked((ushort)(_room.Random.Generate() & 7));
                state.Parts[state.Eye.SlotIndex].ShotMarker = PhantoonPatternDefinitions.ShotEyeMarkers[random];
                state.Mouth.Parameter2 = random;
                state.Tentacles.Parameter2 = 1;
                if (!state.Parts[state.Tentacles.SlotIndex].SwoopRequested)
                {
                    state.Parts[state.Tentacles.SlotIndex].SwoopRequested = true;
                    if (state.Parts[body.SlotIndex].FunctionTimer >= 16)
                        state.Parts[body.SlotIndex].FunctionTimer = 16;
                }
                return;
            }
        }

        // Native writes this otherwise-unused marker even when a valid damaging shot lands
        // outside the three reaction states. Retain it because the four records are exposed
        // to the debugger and later reverse-engineering may identify another consumer.
        state.Tentacles.Parameter2 = 2;
    }

    /// <summary>Closes the eye and ends the vulnerable interval after a damaging hit.</summary>
    private static void ClosePhantoonAfterDamage(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        bool enraged)
    {
        state.Parts[body.SlotIndex].Function = enraged
            ? PhantoonAiFunction.FadeOutBeforeRage
            : PhantoonAiFunction.FadeOutWhileSwooping;
        state.Parts[body.SlotIndex].FunctionTimer = 0;
        RoomEnemySlot tentacles = state.Tentacles;
        RoomEnemySlot eye = state.Eye;
        state.Parts[tentacles.SlotIndex].SwoopRequested = false;
        state.Parts[tentacles.SlotIndex].RoundDamage = 0;
        state.SemiTransparent = true;
        InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        InstallPhantoonProgram(state, eye, PhantoonPartProgram.EyeClose);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[eye.SlotIndex].FadeComplete = false;
        tentacles.Parameter2 = 2;
    }

    /// <summary>Switches from combat to the fatal swoop and death phases.</summary>
    private static void BeginPhantoonDeathSequence(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = state.Parts[body.SlotIndex].Function is
            PhantoonAiFunction.Swooping or
            PhantoonAiFunction.FadeOutWhileSwooping
                ? PhantoonAiFunction.FinishFatalSwoop
                : PhantoonAiFunction.DyingFadeInOut;
        state.Parts[state.Eye.SlotIndex].FigureEightReversed = false;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        state.Wave.InitializeDeathScroll(state.Bg2Scroll.X);
        state.SemiTransparent = true;
        state.Mouth.Parameter2 = 1;
    }

    /// <summary>Ports Phantoon's palette-only hurt AI at $A7:DD3F.</summary>
    internal void ApplyPhantoonHurt(RoomEnemySlot body, PhantoonEnemyState state)
    {
        RoomEnemySlot tentacles = state.Tentacles;
        bool restoreHealthPalette = body.FlashTimer.RawValue == 8 ||
            (body.FrameCounter & 2) == 0 && (SnesLittleEndian.HighByteInPlace(tentacles.Parameter2)) != 0;
        if (restoreHealthPalette)
        {
            CopyPhantoonHealthPalette(body);
            tentacles.Parameter2 = SnesLittleEndian.LowByte(tentacles.Parameter2);
            return;
        }

        if ((body.FrameCounter & 2) == 0 || (SnesLittleEndian.HighByteInPlace(tentacles.Parameter2)) != 0)
            return;

        _presentation.BossPalettes.PhantoonBody.Fill(Bgr555.White);
        tentacles.Parameter2 = SnesLittleEndian.Word(
            SnesLittleEndian.LowByte(tentacles.Parameter2), (byte)(SnesLittleEndian.HighByte(tentacles.Parameter2) | 1));
    }

    /// <summary>Copies the selected health colors into the visible body palette.</summary>
    internal void CopyPhantoonHealthPalette(RoomEnemySlot body)
    {
        int healthBand = Math.Min(7, Math.Max(0, (body.Health - 1) / 312));
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
            _presentation.BossPalettes.PhantoonBody.Write(color, ReadPhantoonHealthColor(healthBand, color));
    }

    /// <summary>Resolves the selected visual color; health-band selection stays in AI.</summary>
    private Bgr555 ReadPhantoonHealthColor(int healthBand, int color) =>
        (_room.Artwork?.PhantoonColors ?? throw new InvalidDataException(
            "Phantoon health palette requires installed artwork.")).ResolveHealth(healthBand, color);

    /// <summary>Ports the eye-open vulnerable window at $A7:D60D.</summary>
    internal static void RunPhantoonEyeTracking(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        if (TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
        {
            // Variable B in the tentacle record is the damage accumulated during this
            // opening. Native clears it before deciding whether a shot requested a swoop.
            state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
            if (!state.Parts[state.Tentacles.SlotIndex].SwoopRequested)
            {
                state.Parts[body.SlotIndex].Function = PhantoonAiFunction.NoOperation;
                InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
                InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeCloseAndPickNewPattern);
                body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);

                // This flag makes PickNewPhantoonPattern choose the initial flame-rain
                // branch after the eye-close list invokes its $D076 callback.
                body.Parameter2 = 1;
                return;
            }

            state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
            state.Parts[body.SlotIndex].FunctionTimer = 60;
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BecomeSolidAndSwoop;
        }

        PointPhantoonEyeAtSamus(body, state, samus);
    }

    /// <summary>Ports the one-frame opaque-swoop setup at $A7:D65C.</summary>
    private static void BeginPhantoonVulnerableSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        state.SemiTransparent = false;
        BeginPhantoonSwoop(body, state);
        InstallPhantoonProgram(state, body, PhantoonPartProgram.FullHitboxBody);
    }

    /// <summary>Ports the 360-frame opaque swoop at $A7:D678.</summary>
    private static void RunPhantoonSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        MovePhantoonInSwoop(body, state, samus, fatal: false);
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutWhileSwooping;
        state.SemiTransparent = true;
        InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        RoomEnemySlot eye = state.Eye;
        InstallPhantoonProgram(state, eye, PhantoonPartProgram.EyeClose);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[eye.SlotIndex].FadeComplete = false;
        state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
    }

    /// <summary>Fades the body while continuing its swoop across the room.</summary>
    private void RunPhantoonFadeOutWhileSwooping(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        MovePhantoonInSwoop(body, state, samus, fatal: false);
        AdvancePhantoonFadeOut(state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WaitAfterFadeOut;
        state.Parts[body.SlotIndex].FunctionTimer = 120;
    }

    /// <summary>Counts the hidden interval before reappearing.</summary>
    private static void RunPhantoonHiddenWait(PhantoonPartState body)
    {
        if (TickPhantoonFunctionTimer(body))
            body.Function = PhantoonAiFunction.PickNextAppearance;
    }

    /// <summary>Ports the hidden left/right placement and round-two picker at $A7:D6E2.</summary>
    private void PlacePhantoonForNextFigureEight(
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
        body.Parameter2 = 0;

        // The native call deliberately installs a pattern function which is immediately
        // overwritten with the fade-in function. Its timer, direction, and movement-side
        // effects remain live, so call the shared callback implementation in full.
        PickPhantoonSecondRoundPattern(state);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeInBeforeFigureEight;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Reveals Phantoon before the next figure-eight pass.</summary>
    private void RunPhantoonFadeInBeforeFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (state.Parts[state.Eye.SlotIndex].FadeComplete)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Ports the visible setup for every flame-rain window at $A7:D73F.</summary>
    private static void BeginPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        state.SemiTransparent = false;
        InstallPhantoonProgram(state, body, PhantoonPartProgram.FullHitboxBody);
        InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeballCentered);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeInDuringFlameRain;
    }

    /// <summary>Reveals the body while preparing the flame-rain attack.</summary>
    private void RunPhantoonFlameRainFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 1);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.TrackSamusDuringFlameRain;
        state.Parts[body.SlotIndex].FunctionTimer = 90;
    }

    /// <summary>Counts the open-eye interval during flame rain.</summary>
    private static void RunPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
        if (state.Parts[state.Tentacles.SlotIndex].SwoopRequested)
        {
            state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
            body.Parameter2 = 1;
            BeginPhantoonSwoop(body, state);
            return;
        }

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutDuringFlameRain;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeClose);
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
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        ushort pattern = unchecked((ushort)(_room.Random.Generate() & 7));
        var placement = PhantoonPatternDefinitions.RainPlacement(pattern);
        state.Parts[body.SlotIndex].PathCursor = placement.Cursor;
        body.Position = body.Position.WithWhole(new(placement.X, placement.Y));
        state.Parts[state.Eye.SlotIndex].FigureEightReversed = false;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    /// <summary>Starts the first rain pattern after the intro round.</summary>
    private void RunPhantoonInitialFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeOut(state, denominator: 12);
        RoomEnemySlot eye = state.Eye;
        StepPhantoonFigureEight(body, state.Parts[body.SlotIndex], state.Parts[eye.SlotIndex]);
        StepPhantoonCasualFlameSchedule(state);
        NativeWordCounterStep closedStep = NativeWordCounter.Decrement(checked((ushort)state.Parts[eye.SlotIndex].ClosedTimer));
        state.Parts[eye.SlotIndex].ClosedTimer = closedStep.Value;
        // $A7:D836 DEC/BEQ/BPL: reaching zero expires the timer, as does underflow.
        if (!closedStep.IsZeroOrNegative)
            return;

        state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.Position.X.Whole < 128 ? (ushort)0 : (ushort)2);
    }

    /// <summary>Hides the body before entering the enraged attack cycle.</summary>
    private void RunPhantoonFadeOutBeforeRage(
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
    private static void RunPhantoonRageHiddenWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeInForRage;
        body.Position = body.Position.WithWhole(new(128, 32));
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Reveals the body at the start of the enraged swoop.</summary>
    private void RunPhantoonRageFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (!state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.Enraged;
        state.Parts[body.SlotIndex].FunctionTimer = 4;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Ports all eight alternating radial flame waves at $A7:D8AC.</summary>
    private void RunPhantoonRage(RoomEnemySlot body, PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
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

        InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeClose);
        state.Parts[state.Eye.SlotIndex].RageWaveCount = 0;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutAfterRage;
    }

    /// <summary>Hides the body after the enraged swoop.</summary>
    private void RunPhantoonRageFadeOut(
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
        bool fatal)
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

        ushort targetY = fatal ? (ushort)112 : unchecked((ushort)(samus.Position.Y.Whole - 48));
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

        InstallPhantoonProgram(
            state,
            state.Eye,
            PhantoonPatternDefinitions.EyeProgram(direction));
    }

    /// <summary>Blends the body palette toward hidden colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeOut(
        PhantoonEnemyState state,
        ushort denominator)
    {
        if ((_frameContext._enemyFrameNmiFrameCounter & 1) != 0 || state.Parts[state.Eye.SlotIndex].FadeComplete)
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
            Bgr555 current = _presentation.BossPalettes.PhantoonBody[color];
            Bgr555 target = (_room.Artwork?.PhantoonColors ?? throw new InvalidDataException(
                "Phantoon fade palette requires installed artwork.")).ResolveFadeOut(color);
            _presentation.BossPalettes.PhantoonBody.Write(
                color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
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

    /// <summary>Restarts a part's program with a one-tick timer, as a native list-pointer store does.</summary>
    private static void InstallPhantoonProgram(
        PhantoonEnemyState state,
        RoomEnemySlot slot,
        PhantoonPartProgram program)
    {
        slot.InstructionTimer = NativeCountdown.Start(1);
        state.PartProgram(slot) = AnimationProgramCursor<PhantoonPartProgram>.Start(program);
    }
    /// <summary>Per-update wave increment during the wavy mosaic death animation.</summary>
    private const ushort PhantoonDeathWaveDelta = 256;
    /// <summary>Wave ceiling reached before the death animation advances.</summary>
    private const ushort PhantoonDeathWaveMaximum = 61440;

    /// <summary>Tracks Samus during the fatal swoop and advances once the body reaches the center band.</summary>
    private static void RunPhantoonFatalSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state, samus);
        MovePhantoonInSwoop(body, state, samus, fatal: true);
        if (body.Position.X.Whole is >= 96 and < 160)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingFadeInOut;
    }

    /// <summary>Alternates five fade-outs and five fade-ins at $A7:D948.</summary>
    private void RunPhantoonDyingFadeCycles(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot eye = state.Eye;
        if ((state.Parts[eye.SlotIndex].DyingFadeCycles & 1) != 0)
            AdvancePhantoonFadeIn(body, state, denominator: 12);
        else
            AdvancePhantoonFadeOut(state, denominator: 12);

        if (!state.Parts[eye.SlotIndex].FadeComplete)
            return;
        state.Parts[eye.SlotIndex].FadeComplete = false;
        state.Parts[eye.SlotIndex].DyingFadeCycles = unchecked((ushort)(state.Parts[eye.SlotIndex].DyingFadeCycles + 1));
        if (unchecked((short)(state.Parts[eye.SlotIndex].DyingFadeCycles - 10)) < 0)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingExplosions;
        state.Parts[body.SlotIndex].FunctionTimer = 15;
        state.Parts[state.Tentacles.SlotIndex].ExplosionIndex = 0;
        state.Parts[body.SlotIndex].ExplosionPass = 0;
    }

    /// <summary>Runs the literal 13-entry-then-5..12-twice explosion schedule.</summary>
    internal void RunPhantoonDyingExplosions(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        RoomEnemySlot tentacles = state.Tentacles;
        var (xOffset, yOffset, explosionType, delay) = PhantoonDeathExplosionDefinitions.Read(state.Parts[tentacles.SlotIndex].ExplosionIndex);
        state.Parts[body.SlotIndex].FunctionTimer = delay;
        ushort x = unchecked((ushort)(body.Position.X.Whole + xOffset));
        ushort y = unchecked((ushort)(body.Position.Y.Whole + yOffset));

        int before = _projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);
        state.DeathExplosionRequests++;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), explosionType);
        if (_projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive) > before)
            state.DeathExplosionsSpawned++;
        state.LastCombatSoundEffect = explosionType == MiscDustAnimation.BigExplosion
            ? PhantoonSoundDefinitions.BigDeathExplosionSound
            : PhantoonSoundDefinitions.DeathExplosionSound;

        state.Parts[tentacles.SlotIndex].ExplosionIndex = unchecked((ushort)(state.Parts[tentacles.SlotIndex].ExplosionIndex + 1));
        if (state.Parts[tentacles.SlotIndex].ExplosionIndex < PhantoonDeathExplosionDefinitions.Count)
            return;

        state.Parts[tentacles.SlotIndex].ExplosionIndex = PhantoonDeathExplosionDefinitions.RepeatStart;
        state.Parts[body.SlotIndex].ExplosionPass = unchecked((ushort)(state.Parts[body.SlotIndex].ExplosionPass + 1));
        if (state.Parts[body.SlotIndex].ExplosionPass >= PhantoonDeathExplosionDefinitions.PassCount)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BeginFinalWavyDeath;
    }

    /// <summary>Starts the distorted BG2 and mosaic stages after the fatal swoop.</summary>
    internal static void BeginPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth;
        state.Parts[mouth.SlotIndex].WaveAmplitude = 0;
        state.Parts[mouth.SlotIndex].WaveAmplitudeShrinking = false;
        state.Parts[mouth.SlotIndex].WavyPhaseDelta = PhantoonWavyPhaseDelta;
        state.Tentacles.Parameter1 = PhantoonWaveRomData.DeathMode;
        state.Wave.Begin(PhantoonWaveRomData.DeathMode);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingFadeOut;
        state.Parts[state.Eye.SlotIndex].MosaicRegister = PhantoonDeathDefinitions.InitialMosaicRegister;
        state.MosaicBlockSize = MosaicBlockSizeOf(PhantoonDeathDefinitions.InitialMosaicRegister);

        // The body remains the BG2 producer. Eye/tentacle/mouth become invisible and ignore
        // Samus while preserving every unrelated raw property bit exactly as $DA6C does.
        EnemyPropertyWord hiddenPartProperties = body.Properties.Replace(
            EnemyProperties.ProcessInstructions,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        state.Eye.Properties = hiddenPartProperties;
        state.Tentacles.Properties = hiddenPartProperties;
        mouth.Properties = hiddenPartProperties;
        state.LastCombatSoundEffect = PhantoonSoundDefinitions.DyingCry;
    }

    /// <summary>Advances death-wave and mosaic counters before room activation.</summary>
    private void RunPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        AdvancePhantoonWaveAmplitude(
            state.Parts[state.Mouth.SlotIndex],
            PhantoonDeathWaveDelta,
            PhantoonDeathWaveMaximum);

        RoomEnemySlot eye = state.Eye;
        if (state.Parts[eye.SlotIndex].MosaicRegister != PhantoonDeathDefinitions.MosaicFinished)
        {
            // The mosaic grows one size step every sixteenth frame.
            if (_frameContext._enemyFrameNmiFrameCounter % 16 != 0)
                return;

            byte mosaic = unchecked((byte)state.Parts[eye.SlotIndex].MosaicRegister);
            if (mosaic == PhantoonDeathDefinitions.FinalMosaicRegister)
            {
                state.Parts[eye.SlotIndex].MosaicRegister = PhantoonDeathDefinitions.MosaicFinished;
                state.Parts[eye.SlotIndex].FadeComplete = false;
                return;
            }

            mosaic = unchecked((byte)(mosaic + PhantoonDeathDefinitions.MosaicSizeStep));
            state.Parts[eye.SlotIndex].MosaicRegister = unchecked((ushort)((SnesLittleEndian.HighByteInPlace(state.Parts[eye.SlotIndex].MosaicRegister)) | mosaic));
            state.MosaicBlockSize = MosaicBlockSizeOf(mosaic);
            return;
        }

        AdvancePhantoonFadeOut(state, denominator: 12);
        if (state.Parts[eye.SlotIndex].FadeComplete)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.AlmostDead;
    }

    /// <summary>
    /// The BG2 mosaic block size in screen pixels a death-mosaic step on the eye's counter shows: the counter's
    /// high nibble is the size minus one, and every step also enables BG2, so the size alone carries the display.
    /// </summary>
    internal static int MosaicBlockSizeOf(byte mosaic) => (mosaic >> PhantoonDeathDefinitions.MosaicSizeShift) + 1;

    /// <summary>Clears the visible enemy BG2 page and starts the 60-frame power delay.</summary>
    private void ClearPhantoonDeathGraphics(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.MosaicBlockSize = 1;
        state.Eye.Parameter1 = 0;
        state.SemiTransparent = false;
        state.Mouth.Parameter1 = PhantoonBlendingRomData.DeleteControlWord;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.Dead;
        state.Parts[body.SlotIndex].FunctionTimer = 60;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        body.Position = body.Position.WithWhole(new(384, 128));

        ushort[] clearedTilemap = new ushort[512];
        Array.Fill(clearedTilemap, PhantoonBg2FrameDefinitions.BlankTile);
        new EnemyVram(_presentation.Vram).EnemyBg2Tilemap.Write(clearedTilemap);
    }

    /// <summary>Enables the cleared Wrecked Ship room and its post-boss state.</summary>
    private void ActivateWreckedShipAfterPhantoon(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (state.Parts[body.SlotIndex].FunctionTimer != 0)
        {
            state.Parts[body.SlotIndex].FunctionTimer = NativeWordCounter.Decrement(checked((ushort)state.Parts[body.SlotIndex].FunctionTimer)).Value;
            return;
        }
        if ((_frameContext._enemyFrameNmiFrameCounter & 3) != 0)
            return;

        state.Parts[state.Eye.SlotIndex].FadeDenominator = 12;
        if (!AdvanceWreckedShipPowerPalette(state.Parts[state.Eye.SlotIndex]))
            return;

        state.MainScreenBg2Enabled = true;

        // $A0:B95D scatters sixteen room-graphics pickups over the visible boss arena.
        // The coordinates are absolute room coordinates, not offsets from Phantoon's body.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.PhantoonBody,
            count: 16,
            xBase: 64,
            xMask: 127,
            yBase: 96,
            yMask: SnesLittleEndian.Word(0, 63));
        state.ItemDropRequested = true;
        EnemyPropertyWord deletedProperties = body.Properties.With(EnemyProperties.Deleted);
        body.Properties = deletedProperties;
        state.Eye.Properties = deletedProperties;
        state.Tentacles.Properties = deletedProperties;
        state.Mouth.Properties = deletedProperties;
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        state.BossDoorPlmRequest = PlmHeaderId.RestorePhantoonDoorAfterBossFight;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        state.WreckedShipPowerPaletteComplete = true;
    }

    /// <summary>Ports $A7:DC5A over CGRAM colors 0..111.</summary>
    internal bool AdvanceWreckedShipPowerPalette(PhantoonPartState eye)
    {
        ushort denominator = checked((ushort)eye.FadeDenominator);
        ushort numerator = checked((ushort)eye.FadeNumerator);
        if (unchecked((ushort)(denominator + 1)) < numerator)
        {
            eye.FadeNumerator = 0;
            return true;
        }

        CgramPalette room = _presentation.BossPalettes.WreckedShipPowerOn;
        for (int color = 0; color < room.Count; color++)
        {
            Bgr555 current = room[color];
            Bgr555 target = (_room.Artwork?.PhantoonColors ?? throw new InvalidDataException(
                "Wrecked Ship power-on palette requires installed artwork.")).ResolvePowerOn(color);
            room.Write(
                color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.FadeNumerator = unchecked((ushort)(numerator + 1));
        return false;
    }
    /// <summary>Advances Phantoon's typed animation.</summary>
    internal void AdvancePhantoonTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        AdvancePhantoonPartAnimation(slot);
    }

    /// <summary>
    /// Runs a Phantoon part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvancePhantoonPartAnimation(RoomEnemySlot part)
    {
        if (!EnemyProgramAnimation.Expire(part))
            return;
        PhantoonEnemyState state = RequirePhantoonState(part);
        AnimationProgramCursor<PhantoonPartProgram> cursor = state.PartProgram(part);
        // Writing the cursor back after the cues lets the program's own sleep overwrite a cue's install on this
        // part, as the native Sleep does. Phantoon's part lists load no loop counter.
        EnemyProgramAnimation.Run(part, PhantoonAnimations.Programs, ref cursor, PhantoonPartFrameSpritemaps.ShownFrames.Of,
            (cue, continuation) =>
            {
                ApplyPhantoonPartCue(part, cue);
                return null;
            });
        state.PartProgram(part) = cursor;
    }

    /// <summary>Carries out one cue of a Phantoon part program.</summary>
    internal void ApplyPhantoonPartCue(
        RoomEnemySlot part,
        PhantoonPartCue cue)
    {
        PhantoonEnemyState state = RequireCompletePhantoonStateForPart(part);
        switch (cue)
        {
            case PhantoonPartCue.PlayMaterializationSound:
                state.LastMaterializationSound =
                    PhantoonSoundDefinitions.MaterializationSound(
                        state.MaterializationSoundIndex);
                state.MaterializationSoundIndex++;
                if (state.MaterializationSoundIndex >= PhantoonSoundDefinitions.MaterializationSoundCount)
                    state.MaterializationSoundIndex = 0;
                return;

            case PhantoonPartCue.BeginEyeTracking:
                BeginPhantoonEyeTracking(state);
                // The advance resumes its own cursor after the cue, so the program's hold overwrites the
                // eye program this installs, as the native Sleep does; the body's program survives.
                return;

            case PhantoonPartCue.PickNewPattern:
                PickPhantoonSecondRoundPattern(state);
                return;

            case PhantoonPartCue.SpawnCasualFlame:
                SpawnPhantoonDestroyableFlame(state.Body, PhantoonFlameSpawnType.Casual, 0);
                _audio.QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlameQueueCapacity);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(PhantoonPartCue)} {cue}.");
        }
    }

    /// <summary>Starts the eye's Samus-tracking list for the current attack phase.</summary>
    internal void BeginPhantoonEyeTracking(PhantoonEnemyState state)
    {
        state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
        RoomEnemySlot body = state.Body;
        InstallPhantoonProgram(state, body, PhantoonPartProgram.EyeHitboxBody);
        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].FunctionTimer = PhantoonTimerDefinitions.VulnerableWindow[_room.Random.Generate() & 7];
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.EyeTracksSamus;
        InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeballCentered);
    }

    /// <summary>Selects a later-round flame and swoop pattern from encounter state.</summary>
    internal void PickPhantoonSecondRoundPattern(
        PhantoonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        RoomEnemySlot eye = state.Eye;
        state.Parts[body.SlotIndex].FunctionTimer = 60;
        state.Parts[eye.SlotIndex].ClosedTimer = PhantoonTimerDefinitions.EyeClosed[_room.Random.Generate() & 7];
        if ((_frameContext._enemyFrameNmiFrameCounter & 1) != 0)
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

        state.Parts[body.SlotIndex].Function = body.Parameter2 != 0
            ? PhantoonAiFunction.FadeOutBeforeFirstFlameRain
            : PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        if (body.Parameter2 != 0)
            state.Parts[eye.SlotIndex].FadeComplete = false;
    }

    /// <summary>Requires the initialized four-slot encounter state for a part callback.</summary>
    private PhantoonEnemyState RequireCompletePhantoonStateForPart(RoomEnemySlot stateSlot) =>
        RequireCompletePhantoonState(stateSlot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody
            ? stateSlot
            : _slotTable._slots[0]);
    /// <summary>Initial flame callback that holds position before orbit activation.</summary>
    private const ushort PhantoonStartingFlameWaitingPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PhantoonStartingFlames;
    /// <summary>Activated flame callback that advances its orbit around the body.</summary>
    private const ushort PhantoonStartingFlameOrbitPreInstruction =
        (ushort)EnemyProjectilePreInstruction.PhantoonStartingFlames_Activated;

    /// <summary>Allocates one intro flame with its chosen orbit direction.</summary>
    internal bool SpawnPhantoonStartingFlame(RoomEnemySlot body, byte directionIndex)
    {
        RoomEnemyProjectileSlot? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.StartingFlame));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        flame.Variable0 = unchecked((byte)(directionIndex * 32));
        PositionPhantoonFlameAroundBody(flame, body, checked((ushort)flame.Variable0), radius: 48);
        flame.PreInstruction = PhantoonStartingFlameWaitingPreInstruction;
        return true;
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
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling;

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
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Enraged;
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
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Rain;
                break;

            case PhantoonFlameSpawnType.Spiral:
                if (index > 7)
                    throw new InvalidDataException($"Phantoon spiral direction {index} exceeds seven.");
                flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(128));
                flame.Variable0 = PhantoonFlameSpawnDefinitions.SpiralAngle(index);
                flame.Position = flame.Position.WithWhole(new(body.Position.X.Whole, unchecked((ushort)(body.Position.Y.Whole + 16))));
                flame.PreInstruction =
                    (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Spiral;
                break;

            default:
                throw new InvalidOperationException($"Undefined Phantoon flame spawn type {type}.");
        }
        return true;
    }

    /// <summary>Holds an intro flame until its delayed orbit begins.</summary>
    internal void RunPhantoonStartingFlameWaiting(RoomEnemyProjectileSlot flame)
    {
        PhantoonEnemyState? state = Phantoon;
        if (state is null || !state.Parts[state.Body.SlotIndex].StartingFlamesActive)
            return;
        flame.PreInstruction = PhantoonStartingFlameOrbitPreInstruction;
        flame.Velocity = new(new SnesSignedEightEight(180), new SnesSignedEightEight(48));
    }

    /// <summary>Advances an intro flame around Phantoon's body.</summary>
    internal void RunPhantoonStartingFlameOrbit(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        if (flame.Velocity.X.RawValue != 0)
        {
            flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.X.RawValue - 1))));
        }
        // $86:9B4C reads the 16-bit NMI_FrameCounter.
        else if ((_projectiles.CurrentFrame16 & 1) != 0)
        {
            flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue - 1))));
            if (flame.Velocity.Y.RawValue == 0)
            {
                flame.Position = flame.Position.WithWhole(new(state.Body.Position.X.Whole, unchecked((ushort)(state.Body.Position.Y.Whole + 16))));
                flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.Delete));
                flame.InstructionTimer = NativeCountdown.Start(1);
                return;
            }
        }

        flame.Variable0 = unchecked((byte)(flame.Variable0 + 1));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            checked((ushort)flame.Variable0),
            unchecked((byte)flame.Velocity.Y.RawValue));
    }

    /// <summary>Moves a casual flame downward before floor contact.</summary>
    internal static void RunPhantoonCasualFlameFalling(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, horizontal: false))
            return;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_HitGround;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualHitGround));
        flame.InstructionTimer = NativeCountdown.Start(1);
        flame.Variable0 = 8;
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole + 8)));
    }

    /// <summary>Holds the flame briefly at the floor after impact.</summary>
    internal static void RunPhantoonCasualFlameImpactPause(
        RoomEnemyProjectileSlot flame,
        int nmiFrameCounter)
    {
        NativeWordCounterStep pauseStep = NativeWordCounter.Decrement(checked((ushort)flame.Variable0));
        flame.Variable0 = pauseStep.Value;
        if (!pauseStep.IsZeroOrNegative)
            return;

        flame.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Bouncing;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualBouncing));
        flame.InstructionTimer = NativeCountdown.Start(1);
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole - 8)));
        flame.Velocity = flame.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-768));
        flame.Variable0 = 0;
        // $86:99EB reads the 16-bit NMI_FrameCounter.
        flame.Velocity = flame.Velocity.WithX(SnesSignedEightEight.FromSignedRaw((nmiFrameCounter & 1) == 0 ? (short)128 : (short)-128));
    }

    /// <summary>Advances the flame's post-impact bounce motion.</summary>
    internal static void RunPhantoonCasualFlameBouncing(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, horizontal: false))
        {
            flame.Variable0 = unchecked((ushort)(flame.Variable0 + 1));
            if (flame.Variable0 < 3)
            {
                flame.Velocity = flame.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(flame.Variable0 == 1 ? (short)-512 : (short)-256));
                return;
            }
            RestPhantoonCasualFlame(flame);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, horizontal: true))
            RestPhantoonCasualFlame(flame);
    }

    /// <summary>Installs the resting phase after a flame's bounce ends.</summary>
    private static void RestPhantoonCasualFlame(RoomEnemyProjectileSlot flame)
    {
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualResting));
        flame.InstructionTimer = NativeCountdown.Start(1);
        flame.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_869A44;
    }

    /// <summary>Advances a rage-phase flame through its authored movement.</summary>
    internal void RunPhantoonEnragedFlame(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + PhantoonFlameMotionRomData.RageRadiusStep))));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + (byte)flame.Velocity.X.RawValue));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            checked((ushort)flame.Variable0),
            unchecked((byte)flame.Velocity.Y.RawValue));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    /// <summary>Updates a delayed rain flame as it enters and falls through the room.</summary>
    internal void RunPhantoonRainFlame(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        if (flame.Velocity.X.RawValue != 0)
        {
            flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.X.RawValue - 1))));
            if (flame.Velocity.X.RawValue != 0 && unchecked((short)flame.Velocity.X.RawValue) >= 0)
                return;
            // Expiry falls through to movement on this call, including signed
            // underflow. Keep each actor's native queue call rather than a last-SFX latch.
            _audio.QueueEnemySound(PhantoonFlameMotionRomData.RainFallSound, PhantoonFlameMotionRomData.RainSoundQueueCapacity);
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, horizontal: false))
            return;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.RainImpact));
        flame.InstructionTimer = NativeCountdown.Start(1);
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole + 8)));
        flame.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_869A44;
        _audio.QueueEnemySound(PhantoonFlameMotionRomData.RainFallSound, PhantoonFlameMotionRomData.RainSoundQueueCapacity);
    }

    /// <summary>Moves a spiral flame around Phantoon's body.</summary>
    internal void RunPhantoonSpiralFlame(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + PhantoonFlameMotionRomData.SpiralRadiusStep))));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + PhantoonFlameMotionRomData.SpiralAngleStep));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            checked((ushort)flame.Variable0),
            unchecked((byte)flame.Velocity.Y.RawValue));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    /// <summary>Converts an orbit phase into a world-space flame position around the body.</summary>
    private static void PositionPhantoonFlameAroundBody(
        RoomEnemyProjectileSlot flame,
        RoomEnemySlot body,
        ushort angle,
        ushort radius)
    {
        // Unlike the common byte-table enemy sine routine, this callback uses a
        // word sample. Its high byte preserves the exact full radius at cardinal angles.
        int xOffset = ReadPhantoonFlameComponent(angle, radius);
        int yOffset = ReadPhantoonFlameComponent(unchecked((byte)(angle - 64)), radius);
        flame.Position = flame.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + xOffset)), unchecked((ushort)(body.Position.Y.Whole + 16 + yOffset))));
    }

    /// <summary>Ports $86:9BA2/$9BF3: unsigned half-wave multiplication followed by whole-result negation.</summary>
    internal static int ReadPhantoonFlameComponent(ushort angle, ushort radius)
    {
        int byteAngle = unchecked((byte)angle);
        int sample = EnemyTrigonometryTables.SignedSine((byte)(byteAngle & 127));
        int magnitude = (SnesEightEight.Multiply(unchecked((byte)sample), SnesLittleEndian.LowByte(radius))) + SnesEightEight.WholeOf(sample) * (SnesLittleEndian.LowByte(radius));
        return byteAngle < 128 ? magnitude : -magnitude;
    }

    /// <summary>Switches a flame to its delete program after leaving the room bounds.</summary>
    private static void DeletePhantoonFlameOutsideRoom(RoomEnemyProjectileSlot flame)
    {
        if (unchecked((short)flame.Position.X.Whole) < 0 || flame.Position.X.Whole >= 256 ||
            unchecked((short)flame.Position.Y.Whole) < 0 || flame.Position.Y.Whole >= 256)
        {
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.Delete));
            flame.InstructionTimer = NativeCountdown.Start(1);
        }
    }

    /// <summary>
    /// Ports projectile instruction <c>$86:980E</c>. The cartridge forwards the flame's
    /// exact position and enemy header <c>$A0:E4FF</c> to <c>Spawn_Enemy_Drops</c>; the
    /// translated pickup owner is still an outer subsystem, so publish the same semantic
    /// request with the header-derived item table rather than silently discarding the drop.
    /// </summary>
    internal void RequestPhantoonFlameDrop(RoomEnemyProjectileSlot flame)
    {
        if (flame.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame)
        {
            throw new InvalidOperationException(
                $"Projectile {flame.Kind} reached Phantoon drop opcode $980E.");
        }

        RoomEnemyDefinition eye = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.PhantoonEye);
        _phantoonFlameDropRequests.Add(new PhantoonFlameDropRequest());
        _deathsAndDrops.SpawnEnemyDropFromChanceTable(
            flame.Position.Whole,
            eye.ItemDropChancesPointer);
    }
    /// <summary>Flame-destruction drop requests published during the current room encounter.</summary>
    internal readonly List<PhantoonFlameDropRequest> _phantoonFlameDropRequests = new();

    /// <summary>Active four-slot encounter state when retail Phantoon occupies slot zero.</summary>
    public PhantoonEnemyState? Phantoon { get; internal set; }

    /// <summary>Discards prior encounter state and pending flame-drop requests on room load.</summary>
    internal void ResetPhantoonRoomState()
    {
        Phantoon = null;
        _phantoonFlameDropRequests.Clear();
    }

    /// <summary>Requires the native slot-zero body before handling another Phantoon part.</summary>
    private PhantoonEnemyState RequirePhantoonState(RoomEnemySlot slot)
    {
        if (Phantoon is null ||
            _slotTable._slots[0].EnemyDefinitionPointer != EnemyDefinitionId.PhantoonBody)
        {
            throw new InvalidOperationException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} requires Phantoon's retail body in slot zero.");
        }
        return Phantoon;
    }

    /// <summary>Recognizes the four native enemy definitions that share Phantoon encounter state.</summary>
    internal static bool IsPhantoonPartDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PhantoonBody or EnemyDefinitionId.PhantoonEye or
        EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth;

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Phantoon is not null)
        {
            Phantoon.LastMaterializationSound = null;
            Phantoon.LastCombatSoundEffect = null;
            Phantoon.MusicRequest = null;
        }
        ResetPhantoonRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Phantoon is not null)
        {
            Phantoon.LastMaterializationSound = null;
            Phantoon.LastCombatSoundEffect = null;
            Phantoon.MusicRequest = null;
        }
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (Phantoon?.LastMaterializationSound is { } phantoonMaterialization)
            audio.QueueEnemySound(phantoonMaterialization, maximumQueued: 6);
        if (Phantoon?.LastCombatSoundEffect is { } phantoonCombat)
            audio.QueueEnemySound(phantoonCombat, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(Phantoon?.MusicRequest, MusicCommandDelay.EightFrames);
    }

    /// <summary>Ports PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPhantoonMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>Ports PhantoonEye's main AI.</summary>
    internal static void RunPhantoonEyeMainAi(RoomEnemySlot _, IEnemyAiFrame _1)
    {
        // Phantoon's eye is positioned by the body and animated by its own list;
        // the header main is the literal common RTL at the start of bank $A7.
    }

    /// <summary>Ports PhantoonTentacles's main AI.</summary>
    internal static void RunPhantoonTentaclesMainAi(RoomEnemySlot _, IEnemyAiFrame _1)
    {
        // $A7:E011 is a literal RTL. These drawing parts animate entirely through
        // their independent bank-$A7 instruction lists after the shared body main.
    }

    /// <summary>Binds Phantoon body contact to the shared enemy collision path.</summary>
    internal void RegisterPhantoonBodyTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.PhantoonTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Phantoon requires his authored extended touch hitboxes.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.NoOp)
                    return;
                if (context.HitboxTouchAi != EnemyInteractionCallback.PhantoonTouch)
                {
                    throw new InvalidDataException(
                        $"Phantoon hitbox touch AI $A7:{context.HitboxTouchAiPointer:X4} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
            });
    }

    // $A7:DD3F owns every Phantoon hurt frame. It alternates palette seven
    // between white and the new health band; body movement/main AI resumes
    // only after common flash timing clears handler bit two.
    /// <summary>Binds the native hurt callback used after a vulnerable-eye hit.</summary>
    internal void RegisterPhantoonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody,
            (slot, _) => ApplyPhantoonHurt(slot, RequireCompletePhantoonState(slot)));

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindPhantoonHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(PhantoonBehavior.IsPhantoonPartDefinition(enemy.EnemyDefinitionPointer)))
            return null;

        // Phantoon's extended frame roots include hitbox-list pointers,
        // but its editable BG2 tilemap writes must never become collision
        // data. Preserve the native component and rectangle order from
        // the compiled bank-$A7 definitions, including point no-ops.
        foreach (PhantoonCollisionComponent component in
                 PhantoonCollisionDefinitions.ComponentsAt(EnemyFrames.Spritemap(enemy.Frame)))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (PhantoonCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? PhantoonHitboxCallbackCodec.ToCallbackWord(hitbox.Shot)
                    : PhantoonHitboxCallbackCodec.ToCallbackWord(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The record words of the Phantoon part in <paramref name="slot"/>.</summary>
    private PhantoonPartState Part(RoomEnemySlot slot) => RequirePhantoonState(slot).Parts[slot.SlotIndex];
}
