using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The Choot species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ChootBehavior : IEnemySpecies
{

    /// <summary>Samus must be within 80 horizontal pixels for a waiting Choot to arm its jump timer.</summary>
    private const ushort ChootActivationDistance = 0x0050;

    /// <summary>Population positions restored after the final falling-path loop.</summary>
    internal readonly Position16<RoomPixels>[] _chootSpawnPositions = new Position16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Calculated apex positions where each slot begins its falling pattern.</summary>
    internal readonly Position16<RoomPixels>[] _chootInitialFallingPositions = new Position16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Current origin advanced by the pattern's Y distance after each terminator.</summary>
    internal readonly Position16<RoomPixels>[] _chootFallingOrigins = new Position16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Ascent speed-table cursor restored for each subsequent activation.</summary>
    internal readonly ushort[] _chootInitialYSpeedTableIndexes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Parameter-two delay remaining before each armed Choot jumps.</summary>
    internal readonly ushort[] _chootJumpDelayTimers = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot movement phase and pattern state cleared on room load.</summary>
    internal readonly ChootEnemyState?[] _chootStates =
        new ChootEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Choot</c> at <c>$A2:DF76</c>, selecting a falling pattern and calculating ascent speed from native parameters.</summary>
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

    /// <summary>Ports <c>MainAI_Choot</c> and its four wait, prepare, jump, and fall targets at <c>$A2:E02E-$E143</c>.</summary>
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

    /// <summary>Uses wrapped signed horizontal distance to arm the parameter-two jump delay when Samus enters range.</summary>
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

    /// <summary>Decrements the native delay word until it turns negative, then starts the jumping instruction list.</summary>
    internal static void PrepareChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        NativeWordCounterStep timer = state.JumpDelayTimer.Decrement();
        state.JumpDelayTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        SetChootInstructionList(slot, ChootInstructionProgramDefinitions.Jumping);
        state.Function = ChootEnemyFunction.Jumping;
    }

    /// <summary>Applies native quadratic ascent without room collision and snaps whole coordinates to the calculated falling origin at the apex.</summary>
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

    /// <summary>Follows authored path offsets, advancing its origin at terminators and restoring the exact spawn state after the final loop.</summary>
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

    /// <summary>Ports <c>CalculateChootInitialJumpSpeed</c> at <c>$A2:DFE9</c>, integrating quadratic steps until they cover the requested height.</summary>
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

    /// <summary>Installs idle, jumping, or falling bytecode and restarts its instruction timers.</summary>
    private static void SetChootInstructionList(
        RoomEnemySlot slot,
        ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects Choot AI dispatch for an enemy slot that has not run its initializer.</summary>
    private ChootEnemyState RequireChootState(RoomEnemySlot slot) =>
        _chootStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Choot state.");

    /// <summary>Registers the Choot-only initializer in the enemy AI table.</summary>
    internal void RegisterChootInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Choot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeChoot(slot);
        });
    }

    /// <summary>Registers the four-phase Choot update for initialized Choot slots.</summary>
    internal void RegisterChootMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Choot, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot, (slot, frame, nmiFrameCounter8) =>
        {
            RunChootMain(slot, RequireChootState(slot), frame.Samus);
        });
    }

    /// <summary>Clears every room-slot movement array before a new enemy population loads.</summary>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_chootStates);
        Array.Clear(_chootSpawnPositions);
        Array.Clear(_chootInitialFallingPositions);
        Array.Clear(_chootFallingOrigins);
        Array.Clear(_chootInitialYSpeedTableIndexes);
        Array.Clear(_chootJumpDelayTimers);
    }

    /// <summary>Choot has no shared per-frame work beyond its registered slot AI.</summary>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
