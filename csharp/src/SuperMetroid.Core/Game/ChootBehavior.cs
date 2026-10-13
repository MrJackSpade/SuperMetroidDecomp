using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Choot species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ChootBehavior : IEnemySpecies
{

    private const ushort ChootActivationDistance = 0x0050;

    internal readonly Position16<RoomPixels>[] _chootSpawnPositions = new Position16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    internal readonly Position16<RoomPixels>[] _chootInitialFallingPositions = new Position16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    internal readonly Position16<RoomPixels>[] _chootFallingOrigins = new Position16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ushort[] _chootInitialYSpeedTableIndexes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ushort[] _chootJumpDelayTimers = new ushort[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ChootEnemyState?[] _chootStates =
        new ChootEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Choot</c> at <c>$A2:DF76</c>.</summary>
    internal void InitializeChoot(RoomEnemySlot slot)
    {
        var state = new ChootEnemyState(
            slot,
            _chootSpawnPositions,
            _chootInitialFallingPositions,
            _chootFallingOrigins,
            _chootInitialYSpeedTableIndexes,
            _chootJumpDelayTimers);
        _chootStates[slot.SlotIndex] = state;

        SetChootInstructionList(slot, ChootInstructionProgramDefinitions.Idle);
        state.Function = ChootEnemyFunction.WaitingForSamus;
        state.SpawnPosition = slot.Position.Whole;

        // Parameter one's high byte selects one of five genuine pattern streams. The sixth
        // ROM table word points back into the pointer table and is documented garbage; no
        // retail population selects it, so expose corrupt/high parameters as invalid data.
        ushort patternIndex = SnesLittleEndian.HighByte(slot.Parameter1);
        ChootPatternDefinition pattern = ChootPatternDefinitions.ForIndex(patternIndex);
        state.FallingPatternPointer = (ushort)pattern.FallingPatternPointer;
        state.FallingPatternYDistance = pattern.FallingPatternYDistance;

        // The SNES multiplier consumes only the low bytes. Retail loop counts are nonzero,
        // but byte multiplication also preserves the native result for debugger corruption.
        ushort jumpHeight = unchecked((ushort)(
            (SnesLittleEndian.LowByte(slot.Parameter1)) * (SnesLittleEndian.LowByte(state.FallingPatternYDistance))));
        state.InitialYSpeedTableIndex = CalculateInitialChootYSpeedTableIndex(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(unchecked((ushort)(
            state.SpawnPosition.Y - jumpHeight)));
        state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
    }

    /// <summary>Ports <c>MainAI_Choot</c> and its four targets at <c>$A2:E02E-$E143</c>.</summary>
    private static void RunChootMain(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case ChootEnemyFunction.WaitingForSamus:
                WaitForChootActivation(slot, state, samus);
                return;
            case ChootEnemyFunction.PreparingJump:
                PrepareChootJump(slot, state);
                return;
            case ChootEnemyFunction.Jumping:
                RunChootJump(slot, state);
                return;
            case ChootEnemyFunction.Falling:
                RunChootFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Choot function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void WaitForChootActivation(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Choot proximity AI requires the active Samus actor.");

        // GetSignedYMinusX treats the wrapped word as signed before taking its magnitude.
        // In particular, $8000 remains $8000 after negation; ordinary Math.Abs(short) would
        // throw and a wider subtraction would disagree near the world-coordinate seam.
        ushort distance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if ((distance & 0x8000) != 0)
            distance = unchecked((ushort)-distance);
        if (unchecked((short)(distance - ChootActivationDistance)) >= 0)
            return;

        state.JumpDelayTimer = NativeCountdown.FromRaw(slot.Parameter2);
        state.Function = ChootEnemyFunction.PreparingJump;
    }

    internal static void PrepareChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        NativeWordCounterStep timer = state.JumpDelayTimer.Decrement();
        state.JumpDelayTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        SetChootInstructionList(slot, ChootInstructionProgramDefinitions.Jumping);
        state.Function = ChootEnemyFunction.Jumping;
    }

    internal static void RunChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        // Only the cursor's high byte reaches the eight-byte quadratic table index. Choot
        // applies the table's separately stored negative pair, including its fractional
        // negation bug, without consulting room collision during this authored leap.
        ushort tableIndex = unchecked((ushort)(SnesEightEight.WholeOf(state.YSpeedTableIndex)));
        int upwardDisplacement = EnemyMath.ReadQuadraticEnemySpeed(tableIndex, negative: true);
        uint fixedY = slot.Position.Y.RawValue;
        fixedY = unchecked(fixedY + (uint)upwardDisplacement);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)fixedY)));

        state.YSpeedTableIndex = unchecked((ushort)(state.YSpeedTableIndex - 0x0200));
        if (unchecked((short)state.YSpeedTableIndex) >= 0)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // subpositions deliberately survive until the final spawn reset, just as in WRAM.
        slot.Position = slot.Position.WithWhole(state.InitialFallingPosition);
        state.FallingOrigin = state.InitialFallingPosition;
        state.FallingPatternIndex = 0;
        state.FallingPatternLoopCounter = NativeCountdown.FromRaw(unchecked((ushort)(
            (SnesLittleEndian.LowByte(slot.Parameter1)) - 1)));
        SetChootInstructionList(slot, ChootInstructionProgramDefinitions.Falling);
        state.Function = ChootEnemyFunction.Falling;
    }

    internal static void RunChootFall(RoomEnemySlot slot, ChootEnemyState state)
    {
        int frameIndex = SnesLittleEndian.HighByte(state.FallingPatternIndex);
        ChootFallingPathPoint point = ChootFallingPathDefinitions.At(
            state.FallingPatternPointer,
            frameIndex);
        if (point.IsTerminator)
        {
            state.FallingOrigin = state.FallingOrigin.WithY(unchecked((ushort)(
                state.FallingOrigin.Y + state.FallingPatternYDistance)));
            state.FallingPatternIndex = 0;
            state.FallingPatternLoopCounter = state.FallingPatternLoopCounter.Decrement().Remaining;
            if (!state.FallingPatternLoopCounter.IsNegative)
                return;

            // The last terminator restores exact spawn coordinates/subpositions, reinstalls
            // idle bytecode, and rearms the same calculated ascent for the next activation.
            state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
            slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
            SetChootInstructionList(slot, ChootInstructionProgramDefinitions.Idle);
            state.Function = ChootEnemyFunction.WaitingForSamus;
            return;
        }

        slot.Position = slot.Position.WithWhole(state.FallingOrigin + point.Offset);
        state.FallingPatternIndex = unchecked((ushort)(
            state.FallingPatternIndex + 0x0100));
    }

    /// <summary>Ports <c>CalculateChootInitialJumpSpeed</c> at <c>$A2:DFE9</c>.</summary>
    private static ushort CalculateInitialChootYSpeedTableIndex(ushort jumpHeight)
    {
        ushort encodedIndex = 0;
        uint accumulatedDistance = 0;
        for (int iteration = 0; iteration < 128; iteration++)
        {
            encodedIndex = unchecked((ushort)(encodedIndex + 0x0200));
            ushort tableIndex = unchecked((ushort)(SnesLittleEndian.HighByte(encodedIndex)));
            accumulatedDistance = unchecked(accumulatedDistance +
                (uint)EnemyMath.ReadQuadraticEnemySpeed(tableIndex, negative: false));
            ushort wholeDistance = SnesFixedPosition.WholeOf(unchecked((int)accumulatedDistance));
            if (unchecked((short)(wholeDistance - jumpHeight)) >= 0)
                return encodedIndex;
        }

        throw new InvalidDataException(
            $"Choot jump-height calculation did not reach {jumpHeight} pixels.");
    }

    private static void SetChootInstructionList(
        RoomEnemySlot slot,
        ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    private ChootEnemyState RequireChootState(RoomEnemySlot slot) =>
        _chootStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Choot state.");

    internal void RegisterChootInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Choot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeChoot(slot);
        });
    }

    internal void RegisterChootMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Choot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot, (slot, frame, nmiFrameCounter8) =>
        {
            RunChootMain(slot, RequireChootState(slot), frame.Samus);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_chootStates);
        Array.Clear(_chootSpawnPositions);
        Array.Clear(_chootInitialFallingPositions);
        Array.Clear(_chootFallingOrigins);
        Array.Clear(_chootInitialYSpeedTableIndexes);
        Array.Clear(_chootJumpDelayTimers);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
