namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon's ordinary combat choreography from <c>$A7:D2D1-$D92D</c>. This file keeps
/// the fight's movement/fade state machine separate from load and animation-bytecode
/// plumbing: the body still stores the native function pointer in variable F, while the
/// eye and tentacle records retain their deliberately aliased combat words.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Body instruction list that removes the vulnerable hitbox during hidden and fade phases.</summary>
    private const ushort PhantoonInvulnerableBodyInstruction =
        PhantoonInstructionProgramDefinitions.InvulnerableBody;
    /// <summary>Body instruction list that restores Phantoon's full combat hitbox while visible.</summary>
    private const ushort PhantoonFullHitboxBodyInstruction =
        PhantoonInstructionProgramDefinitions.FullHitboxBody;
    /// <summary>Eye list used to close the eye before selecting another appearance pattern.</summary>
    private const ushort PhantoonEyeCloseInstruction = PhantoonInstructionProgramDefinitions.EyeClose;
    /// <summary>Eye list that centers the pupil during the flame-rain vulnerable window.</summary>
    private const ushort PhantoonEyeCenteredInstruction = PhantoonInstructionProgramDefinitions.EyeballCentered;

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
                InstallPhantoonInstruction(state.Eye!, PhantoonInstructionProgramDefinitions.EyeCloseAndPickNewPattern);
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
        InstallPhantoonInstruction(eye, PhantoonEyeCloseInstruction);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        eye.VariableF = 0;
        state.Tentacles!.VariableB = 0;
    }

    /// <summary>Continues the opaque swoop while fading Phantoon out, then enters the post-fade hidden wait.</summary>
    /// <param name="body">The boss record whose movement, timer, function, and hitbox are updated.</param>
    /// <param name="state">Phantoon's eye, tentacle, palette, and fade state.</param>
    /// <param name="samus">The target used by the ongoing nonfatal swoop.</param>
    /// <param name="nmiFrameCounter8">Frame phase used by the fade cadence.</param>
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

    /// <summary>Advances the invisible pause after fading out and selects the next appearance when its timer expires.</summary>
    /// <param name="body">The boss record containing the wait timer and function word.</param>
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

    /// <summary>Fades Phantoon in before the second-round figure-eight, then hands control to its movement phase.</summary>
    /// <param name="body">The boss record receiving the next movement function.</param>
    /// <param name="state">Eye and palette state used by the fade.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the palette transition.</param>
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

    /// <summary>Fades Phantoon into the flame-rain attack and opens the timed vulnerable tracking window.</summary>
    /// <param name="body">The boss record whose collision property, function, and timer change after the fade.</param>
    /// <param name="state">Eye and palette state for the transition.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the palette transition.</param>
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

    /// <summary>Ends the flame-rain vulnerability window, choosing a swoop after damage or fading out otherwise.</summary>
    /// <param name="body">The boss record whose attack branch and collision property are updated.</param>
    /// <param name="state">Tentacle damage and eye state used to decide the transition.</param>
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
        InstallPhantoonInstruction(state.Eye, PhantoonEyeCloseInstruction);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.SemiTransparencyLayerFlags |= 0x4000;
    }

    /// <summary>Fades Phantoon out after a flame-rain window and schedules the next hidden rain pattern.</summary>
    /// <param name="body">The boss record receiving the next function and delay.</param>
    /// <param name="state">Eye and palette state used during the fade.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the palette transition.</param>
    private void RunPhantoonFlameRainFadeOut(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        if (state.Eye!.VariableF == 0)
            return;

        body.VariableF = (ushort)PhantoonAiFunction.SpawnFlameRain;
        body.VariableE = PhantoonTimerDefinitions.RainHiding[_nextRandom!() & 7];
    }

    /// <summary>Selects a randomized rain placement after the hidden delay, then spawns its eight staggered flames.</summary>
    /// <param name="body">The boss record repositioned for the chosen pattern.</param>
    /// <param name="state">Eye, mouth, and tentacle state associated with the attack.</param>
    private void RunPhantoonHiddenFlameRain(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        ushort pattern = unchecked((ushort)(_nextRandom!() & 7));
        var placement = PhantoonPatternDefinitions.RainPlacement(pattern);
        body.VariableA = placement.Cursor;
        body.XPosition = placement.X;
        body.YPosition = placement.Y;
        state.Eye!.VariableC = 0;
        body.VariableF = (ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, pattern);
    }

    /// <summary>Runs the opening rain's fade, figure-eight motion, and casual flame schedule until its countdown expires.</summary>
    /// <param name="body">The boss record whose opening attack state is advanced.</param>
    /// <param name="state">Eye, mouth, tentacle, and palette state used by the sequence.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the fade.</param>
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
        // $A7:D836 DEC/BEQ/BPL: reaching zero expires the timer, as does underflow.
        if (unchecked((short)eye.VariableA) > 0)
            return;

        state.Tentacles!.VariableA = 0;
        body.VariableF = (ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain;
        SpawnPhantoonFlameRain(body, body.XPosition < 128 ? (ushort)0 : (ushort)2);
    }

    /// <summary>Fades Phantoon out before the enraged appearance and starts the hidden transition timer.</summary>
    /// <param name="body">The boss record receiving the rage-positioning function and delay.</param>
    /// <param name="state">Eye and palette state for the fade.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the palette transition.</param>
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

    /// <summary>Waits invisibly between rage setup and rage fade-in, then positions Phantoon at the arena top center.</summary>
    /// <param name="body">The boss record containing the timer and receiving the new position and function.</param>
    /// <param name="state">The eye state reset as the hidden wait ends.</param>
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

    /// <summary>Fades Phantoon into the enraged state and initializes the cadence for its radial flame waves.</summary>
    /// <param name="body">The boss record whose function and wave timer are advanced.</param>
    /// <param name="state">Eye and palette state used during the fade.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the palette transition.</param>
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
            for (int direction = PhantoonRageRomData.EvenWaveFirstDirection; direction >= 0; direction--)
                SpawnPhantoonDestroyableFlame(body, unchecked((ushort)(PhantoonRageRomData.FlameParameter | direction)));
        }
        else
        {
            // Native DEY/CPY/BPL includes direction eight: alternating waves
            // intentionally contain seven and eight flames, not seven each.
            for (int direction = PhantoonRageRomData.OddWaveFirstDirection; direction >= PhantoonRageRomData.OddWaveLastDirection; direction--)
                SpawnPhantoonDestroyableFlame(body, unchecked((ushort)(PhantoonRageRomData.FlameParameter | direction)));
        }

        QueueEnemySound(PhantoonRageRomData.WaveSound, PhantoonRageRomData.WaveSoundQueueCapacity);
        state.Eye.VariableF = unchecked((ushort)(state.Eye.VariableF + 1));
        if (unchecked((short)(state.Eye.VariableF - PhantoonRageRomData.WaveCount)) < 0)
        {
            body.VariableE = PhantoonRageRomData.WaveInterval;
            return;
        }

        InstallPhantoonInstruction(state.Eye, PhantoonEyeCloseInstruction);
        state.Eye.VariableF = 0;
        body.VariableF = (ushort)PhantoonAiFunction.FadeOutAfterRage;
    }

    /// <summary>Fades Phantoon out after the final rage wave and schedules its hidden wait.</summary>
    /// <param name="body">The boss record receiving the next function and delay.</param>
    /// <param name="state">Eye and palette state used during the fade.</param>
    /// <param name="nmiFrameCounter8">Frame phase used to advance the palette transition.</param>
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

    /// <summary>Initializes tentacle-controlled swoop velocities and starts the 360-update swoop function.</summary>
    /// <param name="body">The boss record receiving the swoop function and duration.</param>
    /// <param name="state">Phantoon state whose tentacle velocity words seed the movement.</param>
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
            if (unchecked((short)(tentacles.VariableC - PhantoonMotionDefinitions.SwoopNegativeXThreshold)) >= 0)
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
            if (unchecked((short)(tentacles.VariableD - PhantoonMotionDefinitions.SwoopNegativeYThreshold)) >= 0)
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Retains instance ownership inside the native Phantoon phase dispatcher without forcing unrelated phase methods static.")]
    /// <summary>Selects the eye animation for the octant containing Samus relative to Phantoon.</summary>
    /// <param name="body">The boss position used as the aim origin.</param>
    /// <param name="eye">The eye record whose animation pointer is replaced.</param>
    /// <param name="samus">The player position used to select an eye direction.</param>
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
            PhantoonPatternDefinitions.EyeInstruction(direction));
    }

    /// <summary>Applies one palette step toward the hidden target on the native even-frame cadence and marks completion at its endpoint.</summary>
    /// <param name="state">Phantoon's eye fade counters and installed palette state.</param>
    /// <param name="denominator">Transition duration scale used to calculate each palette color.</param>
    /// <param name="nmiFrameCounter8">Caller-supplied frame phase retained by the phase interface; cadence uses the current enemy-frame counter.</param>
    private void AdvancePhantoonFadeOut(
        PhantoonEnemyState state,
        ushort denominator,
        byte nmiFrameCounter8)
    {
        if ((_enemyFrameNmiFrameCounter & 1) != 0 || state.Eye!.VariableF != 0)
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
            ushort target = (TileArtwork?.PhantoonColors ?? throw new InvalidDataException(
                "Phantoon fade palette requires installed artwork.")).ResolveFadeOut(color);
            _cgram.SetColor(
                112 + color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.VariableE = unchecked((ushort)(numerator + 1));
    }

    /// <summary>Spawns eight destroyable flames across the selected rain row with staggered activation delays.</summary>
    /// <param name="body">The boss record supplying the flame origin.</param>
    /// <param name="pattern">Rain placement index selecting the first column.</param>
    private void SpawnPhantoonFlameRain(RoomEnemySlot body, ushort pattern)
    {
        byte column = PhantoonPatternDefinitions.FirstRainColumns[pattern];
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

    /// <summary>Restarts an actor's animation command timer and installs the requested Phantoon instruction list.</summary>
    /// <param name="slot">The body, eye, or tentacle actor receiving the instruction.</param>
    /// <param name="instruction">Bank-local instruction-list pointer to install.</param>
    private static void InstallPhantoonInstruction(
        RoomEnemySlot slot,
        ushort instruction)
    {
        slot.InstructionTimer = 1;
        slot.CurrentInstruction = instruction;
    }
}
