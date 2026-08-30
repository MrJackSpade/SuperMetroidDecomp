namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon's ordinary combat choreography from <c>$A7:D2D1-$D92D</c>. This file keeps
/// the fight's movement/fade state machine separate from load and animation-bytecode
/// plumbing: the body still stores the native function pointer in variable F, while the
/// eye and tentacle records retain their deliberately aliased combat words.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PhantoonInvulnerableBodyInstruction = 0xcc41;
    private const ushort PhantoonEyeHitboxBodyInstruction = 0xcc4d;
    private const ushort PhantoonFullHitboxBodyInstruction = 0xcc47;
    private const ushort PhantoonEyeClosedInstruction = 0xcc91;
    private const ushort PhantoonEyeCenteredInstruction = 0xcc9d;
    private const int PhantoonEyeDirectionTable = 0xa7d40d;
    private const int PhantoonFadeOutPalette = 0xa7ca41;
    private const int PhantoonFlameRainHidingTimerTable = 0xa7cd63;
    private const int PhantoonFlameRainPositionTable = 0xa7cdad;
    private const int PhantoonFlameRainFirstColumnTable = 0xa7cfc2;

    /// <summary>Ports the eye-open vulnerable window at $A7:D60D.</summary>
    private void RunPhantoonEyeTracking(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        if (TickPhantoonFunctionTimer(body))
        {
            // Variable B in the tentacle record is the damage accumulated during this
            // opening. Native clears it before deciding whether a shot requested a swoop.
            state.Tentacles!.VariableB = 0;
            if (state.Tentacles.VariableA == 0)
            {
                body.VariableF = (ushort)PhantoonAiFunction.NoOperation;
                InstallPhantoonInstruction(body, PhantoonInvulnerableBodyInstruction);
                InstallPhantoonInstruction(state.Eye!, 0xcc81);
                body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);

                // This flag makes PickNewPhantoonPattern choose the initial flame-rain
                // branch after the eye-close list invokes its $D076 callback.
                body.Parameter2 = 1;
                return;
            }

            state.Tentacles.VariableA = 0;
            body.VariableE = 60;
            body.VariableF = (ushort)PhantoonAiFunction.BecomeSolidAndSwoop;
        }

        PointPhantoonEyeAtSamus(body, state.Eye!, samus);
    }

    /// <summary>Ports the one-frame opaque-swoop setup at $A7:D65C.</summary>
    private void BeginPhantoonVulnerableSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state.Eye!, samus);
        state.SemiTransparencyLayerFlags &= 0xbfff;
        BeginPhantoonSwoop(body, state);
        InstallPhantoonInstruction(body, PhantoonFullHitboxBodyInstruction);
    }

    /// <summary>Ports the 360-frame opaque swoop at $A7:D678.</summary>
    private void RunPhantoonSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state.Eye!, samus);
        MovePhantoonInSwoop(body, state, samus, fatal: false);
        if (!TickPhantoonFunctionTimer(body))
            return;

        body.VariableF = (ushort)PhantoonAiFunction.FadeOutWhileSwooping;
        state.SemiTransparencyLayerFlags |= 0x4000;
        InstallPhantoonInstruction(body, PhantoonInvulnerableBodyInstruction);
        RoomEnemySlot eye = state.Eye!;
        InstallPhantoonInstruction(eye, PhantoonEyeClosedInstruction);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        eye.VariableF = 0;
        state.Tentacles!.VariableB = 0;
    }

    private void RunPhantoonFadeOutWhileSwooping(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus,
        byte nmiFrameCounter8)
    {
        MovePhantoonInSwoop(body, state, samus, fatal: false);
        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;

        body.VariableF = (ushort)PhantoonAiFunction.WaitAfterFadeOut;
        body.VariableE = 120;
    }

    private static void RunPhantoonHiddenWait(RoomEnemySlot body)
    {
        if (TickPhantoonFunctionTimer(body))
            body.VariableF = (ushort)PhantoonAiFunction.PickNextAppearance;
    }

    /// <summary>Ports the hidden left/right placement and round-two picker at $A7:D6E2.</summary>
    private void PlacePhantoonForNextFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        if ((_nextRandom!() & 1) != 0)
        {
            body.VariableA = 136;
            body.XPosition = 208;
        }
        else
        {
            body.VariableA = 399;
            body.XPosition = 48;
        }
        body.YPosition = 96;
        state.Eye!.VariableC = 0;
        body.VariableC = 1;
        body.VariableB = 0;
        body.Parameter2 = 0;

        // The native call deliberately installs a pattern function which is immediately
        // overwritten with the fade-in function. Its timer, direction, and movement-side
        // effects remain live, so call the shared callback implementation in full.
        PickPhantoonSecondRoundPattern(state, nmiFrameCounter8);
        body.VariableF = (ushort)PhantoonAiFunction.FadeInBeforeFigureEight;
        state.Eye.VariableF = 0;
    }

    private void RunPhantoonFadeInBeforeFigureEight(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF != 0)
            body.VariableF = (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye;
    }

    /// <summary>Ports the visible setup for every flame-rain window at $A7:D73F.</summary>
    private static void BeginPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.Eye!.VariableF = 0;
        state.SemiTransparencyLayerFlags &= 0xbfff;
        InstallPhantoonInstruction(body, PhantoonFullHitboxBodyInstruction);
        InstallPhantoonInstruction(state.Eye, PhantoonEyeCenteredInstruction);
        body.VariableF = (ushort)PhantoonAiFunction.FadeInDuringFlameRain;
    }

    private void RunPhantoonFlameRainFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 1, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        body.VariableF = (ushort)PhantoonAiFunction.TrackSamusDuringFlameRain;
        body.VariableE = 90;
    }

    private static void RunPhantoonFlameRainVulnerableWindow(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        state.Tentacles!.VariableB = 0;
        if (state.Tentacles.VariableA != 0)
        {
            state.Tentacles.VariableA = 0;
            body.Parameter2 = 1;
            BeginPhantoonSwoop(body, state);
            return;
        }

        body.VariableF = (ushort)PhantoonAiFunction.FadeOutDuringFlameRain;
        state.Eye!.VariableF = 0;
        InstallPhantoonInstruction(body, PhantoonInvulnerableBodyInstruction);
        InstallPhantoonInstruction(state.Eye, PhantoonEyeClosedInstruction);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.SemiTransparencyLayerFlags |= 0x4000;
    }

    private void RunPhantoonFlameRainFadeOut(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;

        body.VariableF = (ushort)PhantoonAiFunction.SpawnFlameRain;
        body.VariableE = ReadWord(
            _bus!,
            PhantoonFlameRainHidingTimerTable + (_nextRandom!() & 7) * 2);
    }

    private void RunPhantoonHiddenFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        ushort pattern = unchecked((ushort)(_nextRandom!() & 7));
        int entry = PhantoonFlameRainPositionTable + pattern * 8;
        body.VariableA = ReadWord(_bus!, entry);
        body.XPosition = ReadWord(_bus!, entry + 2);
        body.YPosition = ReadWord(_bus!, entry + 4);
        state.Eye!.VariableC = 0;
        body.VariableF = (ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    private void RunPhantoonInitialFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        RoomEnemySlot eye = state.Eye!;
        StepPhantoonFigureEight(body, eye);
        StepPhantoonCasualFlameSchedule(body, state.Mouth!);
        eye.VariableA = unchecked((ushort)(eye.VariableA - 1));
        if (unchecked((short)eye.VariableA) >= 0)
            return;

        state.Tentacles!.VariableA = 0;
        body.VariableF = (ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.XPosition < 128 ? (ushort)0 : (ushort)2);
    }

    private void RunPhantoonFadeOutBeforeRage(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;
        body.VariableF = (ushort)PhantoonAiFunction.MoveToTopCenterForRage;
        body.VariableE = 120;
    }

    private static void RunPhantoonRageHiddenWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;
        body.VariableF = (ushort)PhantoonAiFunction.FadeInForRage;
        body.XPosition = 128;
        body.YPosition = 32;
        state.Eye!.VariableF = 0;
    }

    private void RunPhantoonRageFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeIn(body, state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;
        body.VariableF = (ushort)PhantoonAiFunction.Enraged;
        body.VariableE = 4;
        state.Eye.VariableF = 0;
    }

    /// <summary>Ports all eight alternating radial flame waves at $A7:D8AC.</summary>
    private void RunPhantoonRage(RoomEnemySlot body, PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        if ((state.Eye!.VariableF & 1) == 0)
        {
            for (int direction = 6; direction >= 0; direction--)
                SpawnPhantoonDestroyableFlame(body, unchecked((ushort)(0x0200 | direction)));
        }
        else
        {
            for (int direction = 15; direction >= 9; direction--)
                SpawnPhantoonDestroyableFlame(body, unchecked((ushort)(0x0200 | direction)));
        }

        state.Eye.VariableF = unchecked((ushort)(state.Eye.VariableF + 1));
        if (unchecked((short)(state.Eye.VariableF - 8)) < 0)
        {
            body.VariableE = 128;
            return;
        }

        InstallPhantoonInstruction(state.Eye, PhantoonEyeClosedInstruction);
        state.Eye.VariableF = 0;
        body.VariableF = (ushort)PhantoonAiFunction.FadeOutAfterRage;
    }

    private void RunPhantoonRageFadeOut(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;
        body.VariableF = (ushort)PhantoonAiFunction.WaitAfterFadeOut;
        body.VariableE = 120;
    }

    private static void BeginPhantoonSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot tentacles = state.Tentacles!;
        tentacles.VariableC = 0x0400;
        tentacles.VariableD = 0x0400;
        tentacles.VariableE = 0;
        body.VariableF = (ushort)PhantoonAiFunction.Swooping;
        body.VariableE = 360;
    }

    /// <summary>
    /// Ports $A7:D2D1 using the NTSC/Japan-USA regional immediates present in the target
    /// cartridge. The asymmetric negative velocity thresholds are intentional; replacing
    /// them with tidy +/- clamps changes the retail swoop path.
    /// </summary>
    private static void MovePhantoonInSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus,
        bool fatal)
    {
        RoomEnemySlot tentacles = state.Tentacles!;
        ushort targetX;
        if (unchecked((short)tentacles.VariableE) < 0)
        {
            tentacles.VariableE = unchecked((ushort)(tentacles.VariableE - 2));
            targetX = unchecked((ushort)(tentacles.VariableE & 0x7fff));
            if (targetX == 0)
                tentacles.VariableE = 0;
        }
        else
        {
            tentacles.VariableE = unchecked((ushort)(tentacles.VariableE + 2));
            if (unchecked((short)(tentacles.VariableE - 256)) >= 0)
                tentacles.VariableE |= 0x8000;
            targetX = unchecked((ushort)(tentacles.VariableE & 0x7fff));
        }

        if (unchecked((short)(targetX - body.XPosition)) < 0)
        {
            if (unchecked((short)(tentacles.VariableC - 0xf671)) >= 0)
                tentacles.VariableC = unchecked((ushort)(tentacles.VariableC - 0x20));
        }
        else if (unchecked((short)(tentacles.VariableC - 0x0800)) < 0)
        {
            tentacles.VariableC = unchecked((ushort)(tentacles.VariableC + 0x20));
        }
        (body.XPosition, body.XSubposition) = AddEightBitVelocity(
            body.XPosition,
            body.XSubposition,
            tentacles.VariableC);
        if (unchecked((short)(body.XPosition - 0xffc0)) < 0)
            body.XPosition = 0xffc0;
        else if (unchecked((short)(body.XPosition - 0x01c0)) >= 0)
            body.XPosition = 0x01c0;

        ushort targetY = fatal ? (ushort)112 : unchecked((ushort)(samus.YPosition - 48));
        if (unchecked((short)(targetY - body.YPosition)) < 0)
        {
            if (unchecked((short)(tentacles.VariableD - 0xf8d1)) >= 0)
                tentacles.VariableD = unchecked((ushort)(tentacles.VariableD - 0x40));
        }
        else if (unchecked((short)(tentacles.VariableD - 0x0600)) < 0)
        {
            tentacles.VariableD = unchecked((ushort)(tentacles.VariableD + 0x40));
        }
        (body.YPosition, body.YSubposition) = AddEightBitVelocity(
            body.YPosition,
            body.YSubposition,
            tentacles.VariableD);
        if (unchecked((short)(body.YPosition - 64)) < 0)
            body.YPosition = 64;
        else if (unchecked((short)(body.YPosition - 216)) >= 0)
            body.YPosition = 216;
    }

    private void PointPhantoonEyeAtSamus(
        RoomEnemySlot body,
        RoomEnemySlot eye,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.XPosition - body.XPosition));
        short deltaY = unchecked((short)(samus.YPosition - body.YPosition));
        ushort direction;
        if (Math.Abs((int)deltaY) < 32)
            direction = deltaX < 0 ? (ushort)7 : (ushort)2;
        else if (Math.Abs((int)deltaX) < 32)
            direction = deltaY < 0 ? (ushort)0 : (ushort)4;
        else if (deltaX < 0)
            direction = deltaY < 0 ? (ushort)8 : (ushort)6;
        else
            direction = deltaY < 0 ? (ushort)1 : (ushort)3;

        InstallPhantoonInstruction(
            eye,
            ReadWord(_bus!, PhantoonEyeDirectionTable + direction * 2));
    }

    private void AdvancePhantoonFadeOut(
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

        for (int color = 0; color < 16; color++)
        {
            ushort current = _cgram!.Colors[112 + color];
            ushort target = ReadWord(_bus!, PhantoonFadeOutPalette + color * 2);
            _cgram.SetColor(
                112 + color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.VariableE = unchecked((ushort)(numerator + 1));
    }

    private void SpawnPhantoonFlameRain(RoomEnemySlot body, ushort pattern)
    {
        byte column = _bus!.ReadByte(PhantoonFlameRainFirstColumnTable + pattern);
        ushort delay = 0x10;
        for (int flame = 0; flame < 8; flame++)
        {
            SpawnPhantoonDestroyableFlame(
                body,
                unchecked((ushort)(0x0400 | delay | column)));
            column++;
            if (column >= 9)
                column = 0;
            delay = unchecked((ushort)(delay + 0x10));
        }
    }

    private static void InstallPhantoonInstruction(
        RoomEnemySlot slot,
        ushort instruction)
    {
        slot.InstructionTimer = 1;
        slot.CurrentInstruction = instruction;
    }
}
