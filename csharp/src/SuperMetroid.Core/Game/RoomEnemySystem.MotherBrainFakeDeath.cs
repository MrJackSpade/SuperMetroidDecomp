using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's fake-death descent and shattered-tube actors from
/// <c>$A9:881D-$A9:8D48</c>. These routines deliberately remain separate from the later
/// phase-two combat dispatcher: the cartridge runs the body, head sub-dispatch, room
/// palette bytecode, bank-$86 projectiles, and dynamically spawned enemies independently.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainPaletteFlashStart = 0xd046;
    private const ushort MotherBrainPaletteFlashGoto = 0x9b0f;
    private const ushort MotherBrainPaletteFlashFinal = 0xd082;
    private const ushort MotherBrainGrayFadePointerTable = 0xed8a;

    private static readonly (ushort X, ushort Y)[] MotherBrainFakeDeathExplosionPositions =
    [
        (136, 116),
        (120, 132),
        (124, 90),
        (138, 146),
        (120, 52),
        (124, 170),
        (138, 72),
        (120, 206),
    ];

    private static readonly ushort[] MotherBrainFallingTubeXRadius = [16, 16, 8, 8, 16];
    private static readonly ushort[] MotherBrainFallingTubeYRadius = [32, 32, 24, 24, 32];
    private static readonly ushort[] MotherBrainFallingTubeFloor = [0x00f8, 0x00f8, 0x00f0, 0x00f0, 0x00f6];
    private static readonly int[] MotherBrainFallingTubeSmokeXOffsets = [-8, 2, -4, 6];

    private Func<int, byte>? _readMotherBrainRoomScrollByte;
    private Action<int, byte>? _setMotherBrainRoomScrollByte;

    /// <summary>Dispatches the body pointer after the phase-one function changes to $881D.</summary>
    private void RunMotherBrainFakeDeath(MotherBrainEnemyState state, SamusState? samus)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
                // `$A9:881D` installs the following countdown and falls through to it. The
                // first observable value is therefore 63, not the host-friendly value 64.
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
                state.FunctionTimer = 64;
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
                if (!DecrementMotherBrainTimerPastZero(state))
                    return;
                state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
                state.RequestMusic(MusicCommand.LoadData(0x21), MusicCommandDelay.EightFrames);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock;
                state.FunctionTimer = 12;
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
                if (!DecrementMotherBrainTimerPastZero(state))
                    return;
                if (samus is not null)
                    samus.InputLocked = false;
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash;
                state.FunctionTimer = 8;
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
                if (!DecrementMotherBrainTimerPastZero(state))
                    return;

                // `$A9:88A0-$88AF` starts the looping room flash, selects FX entry two,
                // arms the head's independent tube sequence, and clears both fade words.
                state.RoomPaletteInstructionPointer = MotherBrainPaletteFlashStart;
                state.RoomPaletteInstructionTimer = 1;
                state.FxEntry = 2;
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
                state.TubeCollapseTimer = 0;
                state.Function = MotherBrainBodyFunction.FakeDeathDescentFadeToGray;
                state.FunctionTimer = 0;
                state.GrayFadeIndex = 0;
                state.RequestPlm(
                    blockX: 14,
                    blockY: 2,
                    header: RoomPlmHeaders.ClearMotherBrainCeilingBlock);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
                RunMotherBrainFadeToGray(state);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
                RunMotherBrainTubeCollapse(state);
                RunMotherBrainFakeDeathExplosion(state);
                return;

            case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
                RequestMotherBrainRoomRows(
                    state,
                    RoomPlmHeaders.MotherBrainsBackgroundRow2,
                    RoomPlmHeaders.MotherBrainsBackgroundRow3,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
                RequestMotherBrainRoomRows(
                    state,
                    RoomPlmHeaders.MotherBrainsBackgroundRow4,
                    RoomPlmHeaders.MotherBrainsBackgroundRow5,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
                RequestMotherBrainRoomRows(
                    state,
                    RoomPlmHeaders.MotherBrainsBackgroundRow6,
                    RoomPlmHeaders.MotherBrainsBackgroundRow7,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
                RequestMotherBrainRoomRows(
                    state,
                    RoomPlmHeaders.MotherBrainsBackgroundRow8,
                    RoomPlmHeaders.MotherBrainsBackgroundRow9,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
                RequestMotherBrainRoomRows(
                    state,
                    RoomPlmHeaders.MotherBrainsBackgroundRowA,
                    RoomPlmHeaders.MotherBrainsBackgroundRowB,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
                RequestMotherBrainRoomRows(
                    state,
                    RoomPlmHeaders.MotherBrainsBackgroundRowC,
                    RoomPlmHeaders.MotherBrainsBackgroundRowD,
                    MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                SetupMotherBrainPhaseTwoGraphics(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain fake-death function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunMotherBrainPauseBeforeLock(MotherBrainEnemyState state, SamusState? samus)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        // Samus command zero at `$90:F084` locks input. The room scroll assignment is a
        // literal byte copy from index zero to index one, not a guessed red/green value.
        SamusState requiredSamus = samus ?? throw new InvalidOperationException(
            "Mother Brain's fake-death input lock requires the active Samus state.");
        requiredSamus.InputLocked = true;
        RequireSetRoomScrollByte(1, RequireReadRoomScrollByte(0));

        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic;
        state.FunctionTimer = 32;
        RunMotherBrainFakeDeath(state, samus);
    }

    /// <summary>
    /// Native timers expire only after unsigned decrement produces a negative signed word.
    /// A zero timer therefore expires immediately, while a value N remains live for N ticks.
    /// </summary>
    private static bool DecrementMotherBrainTimerPastZero(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        return timer.IsNegative;
    }

    private void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNegative)
        {
            state.FunctionTimer = 8;
            ushort step = state.GrayFadeIndex++;
            ushort palettePointer = ReadWord(
                _bus!,
                0xad0000 | unchecked((ushort)(MotherBrainGrayFadePointerTable + step * 2)));
            if (palettePointer == 0)
            {
                state.Function = MotherBrainBodyFunction.FakeDeathDescentCollapseTubes;
            }
            else
            {
                // Fake-death fade replaces colors 145..147 only. Copying a convenient
                // whole palette would visibly alter unrelated room art and is not what
                // `$AD:ED5A` does.
                _cgram!.LoadFromBus(
                    _bus!,
                    0xad0000 | palettePointer,
                    colorCount: 3,
                    destinationIndex: 0x0122 / 2);
            }
        }

        // `$A9:88CF` is an unconditional tail-call. Tube sequencing and dust therefore
        // begin during the grey fade rather than waiting for the eighth palette entry.
        RunMotherBrainTubeCollapse(state);
        RunMotherBrainFakeDeathExplosion(state);
    }

    /// <summary>Executes Mother Brain's independent bank-$A9 room-palette bytecode.</summary>
    private void RunMotherBrainRoomPalette(MotherBrainEnemyState state)
    {
        if (state.RoomPaletteInstructionPointer == 0)
            return;

        for (int commandCount = 0; commandCount < 16; commandCount++)
        {
            ushort pointer = state.RoomPaletteInstructionPointer;
            ushort command = ReadWord(_bus!, 0xa90000 | pointer);
            if ((command & 0x8000) != 0)
            {
                if (command != MotherBrainPaletteFlashGoto)
                {
                    throw new InvalidDataException(
                        $"Mother Brain room-palette opcode $A9:{command:X4} is not translated.");
                }

                state.RoomPaletteInstructionPointer = ReadWord(
                    _bus!,
                    0xa90000 | unchecked((ushort)(pointer + 2)));
                state.RoomPaletteInstructionTimer = 0;
                continue;
            }

            if (command == 0)
            {
                state.RoomPaletteInstructionPointer = 0;
                state.RoomPaletteInstructionTimer = 0;
                return;
            }

            if (state.RoomPaletteInstructionTimer == 0)
            {
                state.RoomPaletteInstructionTimer = 1;
                ApplyMotherBrainRoomPalette(pointer);
                return;
            }

            if (state.RoomPaletteInstructionTimer != command)
            {
                state.RoomPaletteInstructionTimer++;
                ApplyMotherBrainRoomPalette(pointer);
                return;
            }

            state.RoomPaletteInstructionPointer = unchecked((ushort)(pointer + 4));
            state.RoomPaletteInstructionTimer = 0;
        }

        throw new InvalidDataException(
            "Mother Brain room-palette list exceeded 16 commands without selecting a timed entry.");
    }

    private void ApplyMotherBrainRoomPalette(ushort timedEntryPointer)
    {
        ushort source = ReadWord(
            _bus!,
            0xa90000 | unchecked((ushort)(timedEntryPointer + 2)));
        CopyMotherBrainRoomPalette(source);
    }

    private void CopyMotherBrainRoomPalette(ushort source)
    {

        // `$A9:D025` copies three twelve-color slices. The second source slice is written
        // twice (to sprite palettes four and six), an intentional cartridge duplication.
        _cgram!.LoadFromBus(_bus!, 0xa90000 | source, colorCount: 12, destinationIndex: 0x0068 / 2);
        ushort secondSource = unchecked((ushort)(source + 24));
        _cgram.LoadFromBus(_bus!, 0xa90000 | secondSource, colorCount: 12, destinationIndex: 0x00a6 / 2);
        _cgram.LoadFromBus(_bus!, 0xa90000 | secondSource, colorCount: 12, destinationIndex: 0x00e6 / 2);
    }

    private void StopMotherBrainRoomPalette(MotherBrainEnemyState state)
    {
        state.RoomPaletteInstructionPointer = 0;
        state.RoomPaletteInstructionTimer = 0;
        CopyMotherBrainRoomPalette(MotherBrainPaletteFlashFinal);
    }

    private void RunMotherBrainFakeDeathExplosion(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FakeDeathExplosionTimer);
        state.FakeDeathExplosionTimer = timer.Value;
        if (timer.IsNonNegative)
            return;

        state.FakeDeathExplosionTimer = 8;
        state.FakeDeathExplosionIndex = unchecked((ushort)((state.FakeDeathExplosionIndex - 1) & 7));
        (ushort x, ushort y) = MotherBrainFakeDeathExplosionPositions[state.FakeDeathExplosionIndex];

        // `$A9:8904` samples the existing RNG word and never advances it. A host random
        // call here would desynchronize every later Rinka and phase-two attack decision.
        ushort sampledRandom = RequireRandomNumber();
        ushort animation = sampledRandom < 0x4000 ? (ushort)12 : (ushort)3;
        SpawnRoomGraphicsDustExplosion(x, y, animation);
        state.LastSoundEffect = 0x24;
    }

    private void RunMotherBrainTubeCollapse(MotherBrainEnemyState state)
    {
        switch (state.TubeCollapseFunction)
        {
            case MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots:
                if (_enemyProjectiles.Count(projectile => !projectile.IsActive) < 4)
                    return;
                SpawnMotherBrainFallingTube(0x8ae5);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomLeftTube;
                return;

            case MotherBrainTubeCollapseFunction.ClearBottomLeftTube:
                RequestMotherBrainTubePlm(state, 5, 9,
                    RoomPlmHeaders.ClearMotherBrainBottomLeftTube,
                    MotherBrainTubeCollapseFunction.SpawnTopRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, 152, 47);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn9;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn9:
                RequestMotherBrainTubePlm(state, 9, 2,
                    RoomPlmHeaders.ClearMotherBrainCeilingBlock,
                    MotherBrainTubeCollapseFunction.SpawnTopLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopLeftTube, 104, 47);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn6;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn6:
                RequestMotherBrainTubePlm(state, 6, 2,
                    RoomPlmHeaders.ClearMotherBrainCeilingBlock,
                    MotherBrainTubeCollapseFunction.SpawnBottomRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomRightTube:
                SpawnMotherBrainFallingTube(0x8af5);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomRightTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomRightTube:
                RequestMotherBrainTubePlm(state, 10, 9,
                    RoomPlmHeaders.ClearMotherBrainBottomRightTube,
                    MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube:
                SpawnMotherBrainFallingTube(0x8b05);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube:
                RequestMotherBrainTubePlm(state, 6, 10,
                    RoomPlmHeaders.ClearMotherBrainBottomMiddleSideTube,
                    MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube, 120, 59);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn7;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn7:
                RequestMotherBrainTubePlm(state, 7, 2,
                    RoomPlmHeaders.ClearMotherBrainCeilingTube,
                    MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube, 136, 59);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn8;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn8:
                RequestMotherBrainTubePlm(state, 8, 2,
                    RoomPlmHeaders.ClearMotherBrainCeilingTube,
                    MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube:
                SpawnMotherBrainFallingTube(0x8b15);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube:
                RequestMotherBrainTubePlm(state, 9, 10,
                    RoomPlmHeaders.ClearMotherBrainBottomMiddleSideTube,
                    MotherBrainTubeCollapseFunction.SpawnMainTube, 2);
                return;
            case MotherBrainTubeCollapseFunction.SpawnMainTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(0x8b25);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes:
                RequestMotherBrainTubePlm(state, 7, 7,
                    RoomPlmHeaders.ClearMotherBrainBottomMiddleTubes,
                    MotherBrainTubeCollapseFunction.Finished, 0);
                return;
            case MotherBrainTubeCollapseFunction.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain tube function $A9:{(ushort)state.TubeCollapseFunction:X4} is not translated.");
        }
    }

    private static bool DecrementMotherBrainTubeTimerPastZero(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.TubeCollapseTimer);
        state.TubeCollapseTimer = timer.Value;
        return timer.IsNegative;
    }

    private static void RequestMotherBrainTubePlm(
        MotherBrainEnemyState state,
        byte blockX,
        byte blockY,
        ushort header,
        MotherBrainTubeCollapseFunction next,
        ushort timer)
    {
        state.RequestPlm(blockX, blockY, header);
        state.TubeCollapseFunction = next;
        state.TubeCollapseTimer = timer;
    }

    private void SpawnMotherBrainTopTube(
        RoomEnemyProjectileKind kind,
        ushort xPosition,
        ushort yPosition)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0x0e00);
        projectile.XPosition = xPosition;
        projectile.YPosition = yPosition;
        projectile.XVelocity = 0;
        projectile.YVelocity = 0;
        projectile.Variable0 = 0xcbea;
    }

    /// <summary>Runs pre-instruction $86:CBE7 for the four falling ceiling-tube actors.</summary>
    private void RunMotherBrainTopTubePreInstruction(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.Variable0 == 0xcbea)
        {
            SpawnRoomGraphicsDustExplosion(
                projectile.XPosition,
                unchecked((ushort)(projectile.YPosition + 8)),
                animationIndex: 9);
            projectile.Variable0 = 0xcc08;
        }
        else if (projectile.Variable0 != 0xcc08)
        {
            throw new InvalidDataException(
                $"Mother Brain ceiling tube function $86:{projectile.Variable0:X4} is not translated.");
        }

        projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 6));
        (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
            projectile.YPosition,
            projectile.YSubposition,
            projectile.YVelocity);
        if (projectile.YPosition < 0x00d0)
            return;

        ushort x = projectile.XPosition;
        ushort y = projectile.YPosition;
        projectile.Clear();
        SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 12);
    }

    private void SpawnMotherBrainFallingTube(ushort populationPointer)
    {
        int slotIndex = Array.FindIndex(_slots, slot => slot.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            return; // SpawnEnemy silently fails when all 32 native records are occupied.

        int record = 0xa90000 | populationPointer;
        RoomEnemyPopulationRecord population = new(
            ReadWord(_bus!, record),
            ReadWord(_bus!, record + 2),
            ReadWord(_bus!, record + 4),
            ReadWord(_bus!, record + 6),
            ReadWord(_bus!, record + 8),
            ReadWord(_bus!, record + 10),
            ReadWord(_bus!, record + 12),
            ReadWord(_bus!, record + 14));
        if (population.DefinitionPointer != MotherBrainFallingTubeDefinition)
        {
            throw new InvalidDataException(
                $"Mother Brain tube record $A9:{populationPointer:X4} names enemy " +
                $"${population.DefinitionPointer:X4}, not $ECFF.");
        }

        RoomEnemySlot tube = _slots[slotIndex];
        RoomEnemyDefinition definition = ReadDefinition(_bus!, population.DefinitionPointer);
        InitializeSlotFromDefinition(tube, population, definition);
        RunInitializationAi(tube);
        tube.SpritemapPointer = tube.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? (ushort)0x804d
            : (ushort)0;
        EnemyCount = unchecked((ushort)Math.Max(EnemyCount, slotIndex + 1));
        FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * NativeSlotSize));
        if (_motherBrain is not null)
            _motherBrain.SpawnedFallingTubeCount++;
    }

    /// <summary>Ports falling-tube initialization $A9:8B35.</summary>
    private static void InitializeMotherBrainFallingTube(RoomEnemySlot tube)
    {
        int tubeIndex = tube.Parameter1 / 2;
        if ((uint)tubeIndex >= MotherBrainFallingTubeXRadius.Length)
        {
            throw new InvalidDataException(
                $"Mother Brain falling tube parameter ${tube.Parameter1:X4} is outside table range.");
        }

        tube.XRadius = MotherBrainFallingTubeXRadius[tubeIndex];
        tube.YRadius = MotherBrainFallingTubeYRadius[tubeIndex];
        tube.VariableA = tubeIndex == 4 ? (ushort)0x8bcb : (ushort)0x8b88;
        tube.VariableB = MotherBrainFallingTubeFloor[tubeIndex];
        tube.VariableC = 0;
        tube.VariableD = 0;
        tube.VariableE = 0;
    }

    /// <summary>Ports the $A9:8B85 function-pointer dispatcher for physical tube enemies.</summary>
    private void RunMotherBrainFallingTubeMain(RoomEnemySlot tube)
    {
        switch (tube.VariableA)
        {
            case MotherBrainInstructionCodes.Function_MotherBrainTubes_NonMainTube:
                FallMotherBrainTube(tube, mainTube: false);
                return;
            case MotherBrainInstructionCodes.Function_MotherBrainTubes_MainTube_WaitingToFall:
                tube.Parameter2 = unchecked((ushort)(tube.Parameter2 - 1));
                if (unchecked((short)tube.Parameter2) >= 0)
                    return;
                tube.VariableA = 0x8bd6;
                FallMotherBrainTube(tube, mainTube: true);
                return;
            case MotherBrainInstructionCodes.Function_MotherBrainTubes_MainTube_Falling:
                FallMotherBrainTube(tube, mainTube: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain falling tube function $A9:{tube.VariableA:X4} is not translated.");
        }
    }

    private void FallMotherBrainTube(RoomEnemySlot tube, bool mainTube)
    {
        tube.VariableC = unchecked((ushort)(tube.VariableC + 6));
        (tube.YPosition, tube.YSubposition) = AddEightBitVelocity(
            tube.YPosition,
            tube.YSubposition,
            tube.VariableC);

        if (!mainTube)
        {
            if (tube.YPosition >= tube.VariableB)
                ExplodeMotherBrainTube(tube);
            else
                SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        if (tube.YPosition < 0x00f4)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        tube.Properties = tube.Properties.With(EnemyProperties.Invisible);
        MotherBrainEnemyState state = _motherBrain ??
            throw new InvalidOperationException("A falling main tube has no Mother Brain encounter state.");
        ushort headY = unchecked((ushort)(tube.YPosition - 56));
        state.Head!.YPosition = headY;
        if (headY < 196)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        StopMotherBrainRoomPalette(state);
        EarthquakeType = 0x0019;
        EarthquakeTimer = 0x0020;
        state.Head.YPosition = 196;
        state.Body.XPosition = 59;
        state.Body.YPosition = 279;

        // `$A9:903F` is part of the main-tube landing path, not phase-two setup. These
        // lengths and starting angles must exist before `$87A2` gets installed several
        // frames later or the first visible neck frame collapses every joint onto zero.
        state.NeckSegment0Distance = 2;
        state.NeckSegment1Distance = 10;
        state.NeckSegment2Distance = 20;
        state.NeckSegment3Distance = 10;
        state.NeckSegment4Distance = 20;
        state.LowerNeckAngle = 0x4800;
        state.UpperNeckAngle = 0x5000;
        state.NeckAngleDelta = 0x0100;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3;
        ExplodeMotherBrainTube(tube);
    }

    private void SpawnMotherBrainTubeSmoke(RoomEnemySlot tube)
    {
        tube.VariableD = unchecked((ushort)(tube.VariableD - 1));
        if (unchecked((short)tube.VariableD) >= 0)
            return;

        tube.VariableD = 8;
        tube.VariableE = unchecked((ushort)((tube.VariableE + 1) & 3));
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(tube.XPosition + MotherBrainFallingTubeSmokeXOffsets[tube.VariableE])),
            208,
            animationIndex: 9);
    }

    private void ExplodeMotherBrainTube(RoomEnemySlot tube)
    {
        ushort x = tube.XPosition;
        ushort y = tube.YPosition;
        tube.Properties = tube.Properties.With(EnemyProperties.Deleted);
        SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 3);
        if (_motherBrain is not null)
            _motherBrain.LastSoundEffect = 0x24;
    }

    private static void RequestMotherBrainRoomRows(
        MotherBrainEnemyState state,
        ushort firstHeader,
        ushort secondHeader,
        MotherBrainBodyFunction next)
    {
        byte firstRow = state.Function switch
        {
            MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3 => 2,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5 => 4,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7 => 6,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9 => 8,
            MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB => 10,
            MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD => 12,
            _ => throw new InvalidOperationException("Mother Brain row draw called outside fake ascent."),
        };
        state.RequestPlm(blockX: 2, blockY: firstRow, header: firstHeader);
        state.RequestPlm(blockX: 2, blockY: unchecked((byte)(firstRow + 1)), header: secondHeader);
        state.Function = next;
    }

    private void SetupMotherBrainPhaseTwoGraphics(MotherBrainEnemyState state)
    {
        // `$A9:8D11` installs colors 1..15 of the attack and back-leg palettes. Color zero
        // belongs to the room backdrop and must remain untouched.
        _cgram!.LoadFromBus(_bus!, 0xa994b4, colorCount: 15, destinationIndex: 0x0142 / 2);
        _cgram.LoadFromBus(_bus!, 0xa99494, colorCount: 15, destinationIndex: 0x0162 / 2);
        state.EnableUnpauseHook = true;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain;
    }
}
