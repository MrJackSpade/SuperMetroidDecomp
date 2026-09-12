using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Retail Phantoon load and fight-introduction program from <c>$A7:CDF3-$D5E6</c>. The
/// actor is four consecutive enemy records, not one convenient sprite: body owns BG2 and
/// the AI, while eye, tentacles, and mouth run independent extended-spritemap bytecode.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PhantoonBlankBg2Tile = 0x0338;
    private const ushort PhantoonBg2VramBase = 0x4800;
    private const int PhantoonBg2WordCount = 0x0800;
    private const ushort PhantoonInitialBodyInstruction =
        PhantoonInstructionLists.InvulnerableBody;
    private const ushort PhantoonInitialEyeInstruction = PhantoonInstructionLists.InitialEye;
    private const ushort PhantoonInitialTentacleInstruction =
        PhantoonInstructionLists.InitialTentacles;
    private const ushort PhantoonInitialMouthInstruction = PhantoonInstructionLists.InitialMouth;
    private const int PhantoonHealthPaletteTable = 0xa7cb41;
    private const int PhantoonEyeClosedTimerTable = 0xa7cd53;
    private const int PhantoonMovementTable = 0xa7e3d2;
    private const ushort PhantoonIntroAmplitudeDelta = 0x0040;
    private const ushort PhantoonIntroMaximumAmplitude = 0x0c00;
    private const ushort PhantoonWavyPhaseDelta = 0x0008;

    /// <summary>Ports <c>InitAI_PhantoonBody</c> at $A7:CDF3.</summary>
    private void InitializePhantoonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Phantoon body requires native slot zero, not slot {body.SlotIndex}.");
        }

        _phantoonState = new PhantoonEnemyState(body)
        {
            Bg2TilemapSize = 0x0360,
            BackgroundTilemapPrepared = true,
        };

        // Native clears all $1000 bytes of the enemy BG2 working image to tile $0338.
        // This renderer keeps the visible mirror in VRAM, so one exact word transfer gives
        // ProcessExtendedTilemap the same clean surface used by the subsequent body maps.
        ushort[] clearedTilemap = new ushort[PhantoonBg2WordCount];
        Array.Fill(clearedTilemap, PhantoonBlankBg2Tile);
        _vram!.ExecuteWordTransfer(clearedTilemap, PhantoonBg2VramBase, wordIncrement: 1);

        // Target BG palette seven is black at fight start. In the software PPU CGRAM is the
        // renderer-visible incarnation of native current/target palette storage; clearing
        // these exact sixteen words makes the later health-palette interpolation observable.
        for (int color = 112; color < 128; color++)
            _cgram!.SetColor(color, 0);

        body.VariableE = 0x0060; // Japan/USA regional constant at $A7:CE2B.
        body.VariableA = 0;
        body.VariableB = 0;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        InitializePhantoonPart(body, PhantoonInitialBodyInstruction);
    }

    /// <summary>Ports the shared part initializer at $A7:CE55.</summary>
    private void InitializePhantoonPart(RoomEnemySlot part)
    {
        PhantoonEnemyState state = RequirePhantoonState(part);
        ushort instruction = part.Parameter2 switch
        {
            1 => PhantoonInitialEyeInstruction,
            2 => PhantoonInitialTentacleInstruction,
            3 => PhantoonInitialMouthInstruction,
            _ => throw new InvalidDataException(
                $"Phantoon part slot {part.SlotIndex} has invalid part index ${part.Parameter2:X4}."),
        };
        InitializePhantoonPart(part, instruction);

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

    private void InitializePhantoonPart(RoomEnemySlot part, ushort instruction)
    {
        PhantoonEnemyState state = _phantoonState ??
            throw new InvalidOperationException("Phantoon body must initialize before its parts.");
        part.SpritemapPointer = 0xa5df; // Spritemap_Common_Nothing in bank $A7.
        part.InstructionTimer = 1;
        part.Timer = 0;
        part.PaletteIndex = state.Body.PaletteIndex;
        part.VramTilesIndex = state.Body.VramTilesIndex;
        part.CurrentInstruction = instruction;
        part.VariableF = (ushort)PhantoonAiFunction.SpawnStartingFlames;

        // Every call writes these shared aliases, including the body fallthrough. Slot three
        // owns the casual-flame pattern index/control words used much later in the fight.
        if (_slots.Length > 3)
        {
            _slots[3].Parameter1 = 0;
            _slots[3].VariableC = 0xffff;
        }
    }

    /// <summary>Ports <c>MainAI_Phantoon</c> at $A7:CEA6.</summary>
    private void RunPhantoonMain(
        RoomEnemySlot body,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY,
        byte nmiFrameCounter8)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(body);
        state.LastMaterializationSound = null;

        switch ((PhantoonAiFunction)body.VariableF)
        {
            case PhantoonAiFunction.SpawnStartingFlames:
                RunPhantoonStartingFlameSpawner(body, state);
                break;
            case PhantoonAiFunction.WaitBeforeActivatingStartingFlames:
                RunPhantoonStartingFlamePause(body);
                break;
            case PhantoonAiFunction.WaitForStartingFlamesToDisappear:
                RunPhantoonStartingFlameOrbitWait(body, state);
                break;
            case PhantoonAiFunction.WavyFadeIn:
                RunPhantoonWavyFadeIn(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.PickFirstRoundPattern:
                RunPhantoonPickFirstRoundPattern(body, state, nmiFrameCounter8);
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
                    RequirePhantoonSamus(samus),
                    nmiFrameCounter8);
                break;
            case PhantoonAiFunction.WaitAfterFadeOut:
                RunPhantoonHiddenWait(body);
                break;
            case PhantoonAiFunction.PickNextAppearance:
                PlacePhantoonForNextFigureEight(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.FadeInBeforeFigureEight:
                RunPhantoonFadeInBeforeFigureEight(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.BecomeSolidAfterFlameRain:
                BeginPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonAiFunction.FadeInDuringFlameRain:
                RunPhantoonFlameRainFadeIn(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.TrackSamusDuringFlameRain:
                RunPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonAiFunction.FadeOutDuringFlameRain:
                RunPhantoonFlameRainFadeOut(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.SpawnFlameRain:
                RunPhantoonHiddenFlameRain(body, state);
                break;
            case PhantoonAiFunction.FadeOutBeforeFirstFlameRain:
                RunPhantoonInitialFlameRain(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.FadeOutBeforeRage:
                RunPhantoonFadeOutBeforeRage(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.MoveToTopCenterForRage:
                RunPhantoonRageHiddenWait(body, state);
                break;
            case PhantoonAiFunction.FadeInForRage:
                RunPhantoonRageFadeIn(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.Enraged:
                RunPhantoonRage(body, state);
                break;
            case PhantoonAiFunction.FadeOutAfterRage:
                RunPhantoonRageFadeOut(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.FinishFatalSwoop:
                RunPhantoonFatalSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.DyingFadeInOut:
                RunPhantoonDyingFadeCycles(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.DyingExplosions:
                RunPhantoonDyingExplosions(body, state);
                break;
            case PhantoonAiFunction.BeginFinalWavyDeath:
                BeginPhantoonWavyMosaicDeath(body, state);
                break;
            case PhantoonAiFunction.DyingFadeOut:
                RunPhantoonWavyMosaicDeath(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.AlmostDead:
                ClearPhantoonDeathGraphics(body, state);
                break;
            case PhantoonAiFunction.Dead:
                ActivateWreckedShipAfterPhantoon(body, state, nmiFrameCounter8);
                break;
            case PhantoonAiFunction.NoOperation:
                break;
            default:
                throw new InvalidDataException(
                    $"Phantoon function $A7:{body.VariableF:X4} is not translated.");
        }

        // The source contains a documented X-register bug that makes only the slot-zero
        // invocation run this synchronization block. Our main dispatcher likewise calls it
        // only for the body, then copies the updated point into the three drawing records.
        state.Eye!.XPosition = state.Tentacles!.XPosition = state.Mouth!.XPosition = body.XPosition;
        state.Eye.YPosition = state.Tentacles.YPosition = state.Mouth.YPosition = body.YPosition;
        if (state.Eye.Parameter1 == 0)
        {
            state.Bg2HorizontalScroll = unchecked((ushort)(cameraX - body.XPosition + 40));
            state.Bg2VerticalScroll = unchecked((ushort)(cameraY - body.YPosition + 40));
        }
    }

    private void RunPhantoonStartingFlameSpawner(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        state.StartingFlameRequests++;
        if (SpawnPhantoonStartingFlame(body, unchecked((byte)body.VariableA)))
            state.StartingFlamesSpawned++;
        state.LastMaterializationSound = 0x001d;
        body.VariableE = 30;
        body.VariableA = unchecked((ushort)(body.VariableA + 1));
        if (unchecked((short)(body.VariableA - 8)) < 0)
            return;

        body.VariableA = 0;
        state.Tentacles!.VariableB = 0;
        body.VariableF = (ushort)PhantoonAiFunction.WaitBeforeActivatingStartingFlames;
        body.VariableE = 30;
        state.BossDoorPlmRequest = RoomPlmHeaders.DrawPhantoonDoorDuringBossFight;
    }

    private static void RunPhantoonStartingFlamePause(RoomEnemySlot body)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;
        body.VariableE = 240;
        body.VariableB = 1;
        body.VariableF = (ushort)PhantoonAiFunction.WaitForStartingFlamesToDisappear;
    }

    private static void RunPhantoonStartingFlameOrbitWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        body.VariableB = 0;
        state.SemiTransparencyLayerFlags |= 0x4000;
        body.VariableF = (ushort)PhantoonAiFunction.WavyFadeIn;
        state.Mouth!.Parameter1 = 0x8001;
        body.VariableE = 120;
        state.Mouth.VariableD = PhantoonIntroMaximumAmplitude;
        state.Mouth.VariableE = 0;
        state.Eye!.VariableF = 0;
        state.MusicRequest = MusicCommand.SelectTrack(5);
        state.Mouth.VariableF = PhantoonWavyPhaseDelta;
        state.Tentacles!.Parameter1 = PhantoonWaveRomData.IntroMode;
        state.Wave.Begin(PhantoonWaveRomData.IntroMode);
    }

    private void RunPhantoonWavyFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 12, nmiFrameCounter8);
        if (AdvancePhantoonWaveAmplitude(
                state.Mouth!,
                PhantoonIntroAmplitudeDelta,
                PhantoonIntroMaximumAmplitude))
        {
            body.VariableF = (ushort)PhantoonAiFunction.PickFirstRoundPattern;
            state.Mouth!.Parameter1 = 1;
            body.VariableE = 30;
            return;
        }

        if (TickPhantoonFunctionTimer(body))
            state.Mouth!.VariableE = 1;
    }

    private void RunPhantoonPickFirstRoundPattern(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        RoomEnemySlot eye = state.Eye!;
        eye.Parameter1 = 0;
        eye.VariableA = ReadWord(
            _bus!,
            PhantoonEyeClosedTimerTable + ((nmiFrameCounter8 >> 1) & 3) * 2);
        body.VariableF = (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye;
        body.VariableB = 0;
        body.VariableD = 0;
        if ((_nextRandom!() & 1) != 0)
        {
            body.VariableC = 0;
            eye.VariableC = 1;
            body.VariableA = 533;
        }
        else
        {
            body.VariableC = 1;
            eye.VariableC = 0;
            body.VariableA = 0;
        }
    }

    private void RunPhantoonFirstRoundFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        StepPhantoonFigureEight(body, state.Eye!);
        StepPhantoonCasualFlameSchedule(body, state.Mouth!);

        state.Eye!.VariableA = unchecked((ushort)(state.Eye.VariableA - 1));
        if (unchecked((short)state.Eye.VariableA) >= 0)
            return;

        body.VariableF = (ushort)PhantoonAiFunction.NoOperation;
        state.Eye.InstructionTimer = 1;
        state.Eye.CurrentInstruction = PhantoonInstructionLists.BodyFollowUp;
        body.Parameter2 = 0;
        SpawnPhantoonSpiralFlames(body);
    }

    private void StepPhantoonFigureEight(RoomEnemySlot body, RoomEnemySlot eye)
    {
        if (eye.VariableC != 0)
            AdjustPhantoonReverseFigureEightSpeed(body);
        else
            AdjustPhantoonForwardFigureEightSpeed(body);

        int steps = eye.VariableC != 0
            ? unchecked((ushort)-body.VariableC)
            : body.VariableC;
        for (int step = 0; step < steps; step++)
        {
            int address = PhantoonMovementTable + body.VariableA * 2;
            sbyte dx = unchecked((sbyte)_bus!.ReadByte(address));
            sbyte dy = unchecked((sbyte)_bus.ReadByte(address + 1));
            if (eye.VariableC != 0)
            {
                body.XPosition = unchecked((ushort)(body.XPosition - dx));
                body.YPosition = unchecked((ushort)(body.YPosition - dy));
                body.VariableA = body.VariableA == 0 ? (ushort)533 : unchecked((ushort)(body.VariableA - 1));
            }
            else
            {
                body.XPosition = unchecked((ushort)(body.XPosition + dx));
                body.YPosition = unchecked((ushort)(body.YPosition + dy));
                body.VariableA = body.VariableA >= 533 ? (ushort)0 : unchecked((ushort)(body.VariableA + 1));
            }
        }
    }

    private static void AdjustPhantoonForwardFigureEightSpeed(RoomEnemySlot body)
    {
        if (body.VariableD == 0)
        {
            AddPhantoonSpeed(
                body,
                low: PhantoonMotionDefinitions.SlowFraction,
                high: PhantoonMotionDefinitions.SlowWhole);
            if (unchecked((short)(body.VariableC - PhantoonMotionDefinitions.ForwardSlowCap)) >= 0)
            {
                body.VariableC = unchecked((ushort)(PhantoonMotionDefinitions.ForwardSlowCap - 1));
                body.VariableB = 0;
                body.VariableD = 1;
            }
            return;
        }

        if ((body.VariableD & 1) != 0)
        {
            AddPhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole);
            if (unchecked((short)(body.VariableC - PhantoonMotionDefinitions.ForwardFastCap)) >= 0)
            {
                body.VariableC = PhantoonMotionDefinitions.ForwardFastCap;
                body.VariableB = 0;
                body.VariableD++;
            }
            return;
        }

        SubtractPhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole);
        ushort minimum = PhantoonMotionDefinitions.ForwardMinimum;
        if (body.VariableC == minimum || unchecked((short)(body.VariableC - minimum)) < 0)
        {
            body.VariableC = unchecked((ushort)(minimum + 1));
            body.VariableB = 0;
            body.VariableD = 0;
        }
    }

    private static void AdjustPhantoonReverseFigureEightSpeed(RoomEnemySlot body)
    {
        if (body.VariableD == 0)
        {
            SubtractPhantoonSpeed(
                body,
                PhantoonMotionDefinitions.SlowFraction,
                PhantoonMotionDefinitions.SlowWhole);
            ushort cap = PhantoonMotionDefinitions.ReverseSlowCap;
            if (body.VariableC == cap || unchecked((short)(body.VariableC - cap)) < 0)
            {
                body.VariableC = unchecked((ushort)(cap + 2));
                body.VariableB = 0;
                body.VariableD = 1;
            }
            return;
        }

        if ((body.VariableD & 1) != 0)
        {
            SubtractPhantoonSpeed(
                body,
                PhantoonMotionDefinitions.FastFraction,
                PhantoonMotionDefinitions.FastWhole);
            ushort cap = PhantoonMotionDefinitions.ReverseFastCap;
            if (body.VariableC == cap || unchecked((short)(body.VariableC - cap)) < 0)
            {
                body.VariableC = unchecked((ushort)(cap + 1));
                body.VariableB = 0;
                body.VariableD++;
            }
            return;
        }

        AddPhantoonSpeed(
            body,
            PhantoonMotionDefinitions.FastFraction,
            PhantoonMotionDefinitions.FastWhole);
        ushort maximum = PhantoonMotionDefinitions.ReverseMaximum;
        if (unchecked((short)(body.VariableC - maximum)) >= 0)
        {
            body.VariableC = maximum;
            body.VariableB = 0;
            body.VariableD = 0;
        }
    }

    private static void AddPhantoonSpeed(RoomEnemySlot body, ushort low, ushort high)
    {
        uint value = ((uint)body.VariableC << 16) | body.VariableB;
        value = unchecked(value + (((uint)high << 16) | low));
        body.VariableB = unchecked((ushort)value);
        body.VariableC = unchecked((ushort)(value >> 16));
    }

    private static void SubtractPhantoonSpeed(RoomEnemySlot body, ushort low, ushort high)
    {
        uint value = ((uint)body.VariableC << 16) | body.VariableB;
        value = unchecked(value - (((uint)high << 16) | low));
        body.VariableB = unchecked((ushort)value);
        body.VariableC = unchecked((ushort)(value >> 16));
    }

    private void StepPhantoonCasualFlameSchedule(RoomEnemySlot body, RoomEnemySlot mouth)
    {
        mouth.VariableB = unchecked((ushort)(mouth.VariableB - 1));
        if (unchecked((short)mouth.VariableB) >= 0)
            return;

        if (unchecked((short)mouth.VariableC) >= 0)
        {
            mouth.VariableC = unchecked((ushort)(mouth.VariableC - 1));
            int patternPointer = ReadWord(
                _bus!, EnemyRomTablePointers.Phantoon.MouthPatternPointerWords + mouth.VariableA * 2);
            int timerIndex = unchecked((short)mouth.VariableC) < 0 ? 0 : mouth.VariableC + 1;
            mouth.VariableB = ReadWord(_bus!, 0xa70000 | unchecked((ushort)(patternPointer + timerIndex * 2)));
            mouth.InstructionTimer = 1;
            mouth.CurrentInstruction = PhantoonInstructionLists.MouthFollowUp;
            return;
        }

        mouth.VariableA = unchecked((ushort)(_nextRandom!() & 3));
        ushort pointer = ReadWord(
            _bus!, EnemyRomTablePointers.Phantoon.MouthPatternPointerWords + mouth.VariableA * 2);
        mouth.VariableC = ReadWord(_bus!, 0xa70000 | pointer);
        mouth.VariableB = ReadWord(
            _bus!,
            0xa70000 | unchecked((ushort)(pointer + (mouth.VariableC + 1) * 2)));
    }

    private void AdvancePhantoonFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        ushort denominator,
        byte nmiFrameCounter8)
    {
        if ((nmiFrameCounter8 & 1) != 0 || state.Eye!.VariableF != 0)
            return;

        RoomEnemySlot eye = state.Eye;
        eye.VariableD = denominator;
        ushort numerator = eye.VariableE;
        if (unchecked((ushort)(denominator + 1)) < numerator)
        {
            eye.VariableE = 0;
            eye.VariableF = 1;
            return;
        }

        int healthBand = Math.Min(7, Math.Max(0, (body.Health - 1) / 312));
        for (int color = 0; color < 16; color++)
        {
            ushort current = _cgram!.Colors[112 + color];
            ushort target = ReadWord(
                _bus!,
                PhantoonHealthPaletteTable + healthBand * 32 + color * 2);
            _cgram.SetColor(
                112 + color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.VariableE = unchecked((ushort)(numerator + 1));
    }

    private static ushort CalculatePhantoonTransitionColor(
        ushort numerator,
        ushort denominator,
        ushort current,
        ushort target)
    {
        int red = CalculatePhantoonTransitionComponent(
            numerator, denominator, current & 0x1f, target & 0x1f);
        int green = CalculatePhantoonTransitionComponent(
            numerator, denominator, (current >> 5) & 0x1f, (target >> 5) & 0x1f);
        int blue = CalculatePhantoonTransitionComponent(
            numerator, denominator, (current >> 10) & 0x1f, (target >> 10) & 0x1f);
        return unchecked((ushort)(red | (green << 5) | (blue << 10)));
    }

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
        int quotient = remainingSteps == 0 ? ushort.MaxValue : (Math.Abs(target - current) << 8) / remainingSteps;
        int signedStep = target < current ? -quotient : quotient;
        return unchecked((ushort)((current << 8) + signedStep)) >> 8;
    }

    private static bool AdvancePhantoonWaveAmplitude(
        RoomEnemySlot mouth,
        ushort delta,
        ushort maximum)
    {
        if (mouth.VariableE != 0)
        {
            ushort previous = mouth.VariableD;
            mouth.VariableD = unchecked((ushort)(mouth.VariableD - delta));
            if (previous < delta)
            {
                mouth.VariableD = 0;
                return true;
            }
            return false;
        }

        mouth.VariableD = unchecked((ushort)(mouth.VariableD + delta));
        if (mouth.VariableD >= maximum)
            mouth.VariableD = maximum;
        return false;
    }

    private static bool TickPhantoonFunctionTimer(RoomEnemySlot body)
    {
        ushort oldTimer = body.VariableE;
        body.VariableE = unchecked((ushort)(body.VariableE - 1));
        return oldTimer == 1 || unchecked((short)body.VariableE) < 0;
    }

    private PhantoonEnemyState RequireCompletePhantoonState(RoomEnemySlot body)
    {
        PhantoonEnemyState state = RequirePhantoonState(body);
        if (state.Eye is null || state.Tentacles is null || state.Mouth is null)
            throw new InvalidOperationException("Phantoon's four-part population is incomplete.");
        return state;
    }

    private static SamusState RequirePhantoonSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Phantoon's eye and swoop programs require the active Samus actor.");

    // The spiral fireballs are the first ordinary combat attack. Their exact bank-$86
    // translation belongs to the next combat slice; retaining the explicit call boundary
    // prevents the eye-opening instruction from being mistaken for completed behavior.
    private void SpawnPhantoonSpiralFlames(RoomEnemySlot body)
    {
        for (int direction = 7; direction >= 0; direction--)
            SpawnPhantoonDestroyableFlame(body, unchecked((ushort)(0x0600 | direction)));
    }
}
