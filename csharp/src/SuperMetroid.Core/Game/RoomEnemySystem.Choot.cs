namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A2 function pointer stored in a Choot's variable A.</summary>
public enum ChootEnemyFunction : ushort
{
    WaitingForSamus = 0xe035,
    PreparingJump = 0xe04f,
    Jumping = 0xe06a,
    Falling = 0xe0cd,
}

/// <summary>
/// Typed debugger view of Choot's six common-slot words and eight words in per-enemy extra
/// WRAM. Every property aliases the same native-width storage used by the translated AI, so
/// frame stepping exposes the actual table cursors instead of a second host-only model.
/// </summary>
public sealed class ChootEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _spawnXPositions;
    private readonly ushort[] _spawnYPositions;
    private readonly ushort[] _initialFallingXPositions;
    private readonly ushort[] _initialFallingYPositions;
    private readonly ushort[] _fallingXOrigins;
    private readonly ushort[] _fallingYOrigins;
    private readonly ushort[] _initialYSpeedTableIndexes;
    private readonly ushort[] _jumpDelayTimers;

    internal ChootEnemyState(
        RoomEnemySlot slot,
        ushort[] spawnXPositions,
        ushort[] spawnYPositions,
        ushort[] initialFallingXPositions,
        ushort[] initialFallingYPositions,
        ushort[] fallingXOrigins,
        ushort[] fallingYOrigins,
        ushort[] initialYSpeedTableIndexes,
        ushort[] jumpDelayTimers)
    {
        _slot = slot;
        _spawnXPositions = spawnXPositions;
        _spawnYPositions = spawnYPositions;
        _initialFallingXPositions = initialFallingXPositions;
        _initialFallingYPositions = initialFallingYPositions;
        _fallingXOrigins = fallingXOrigins;
        _fallingYOrigins = fallingYOrigins;
        _initialYSpeedTableIndexes = initialYSpeedTableIndexes;
        _jumpDelayTimers = jumpDelayTimers;
    }

    public ChootEnemyFunction Function
    {
        get => (ChootEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Encoded ascent cursor: its high byte is the quadratic-table index and native AI
    /// changes the word by <c>$0200</c> per frame.
    /// </summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>8.8 falling-pattern frame cursor; main AI advances it by <c>$0100</c>.</summary>
    public ushort FallingPatternIndex
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed-underflow loop counter initialized to parameter-one low minus one.</summary>
    public ushort FallingPatternLoopCounter
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Same-bank pointer to one of five ROM-authored X/Y offset streams.</summary>
    public ushort FallingPatternPointer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Whole-pixel Y origin advance applied between falling-pattern loops.</summary>
    public ushort FallingPatternYDistance
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort SpawnXPosition
    {
        get => _spawnXPositions[_slot.SlotIndex];
        internal set => _spawnXPositions[_slot.SlotIndex] = value;
    }

    public ushort SpawnYPosition
    {
        get => _spawnYPositions[_slot.SlotIndex];
        internal set => _spawnYPositions[_slot.SlotIndex] = value;
    }

    public ushort InitialFallingXPosition
    {
        get => _initialFallingXPositions[_slot.SlotIndex];
        internal set => _initialFallingXPositions[_slot.SlotIndex] = value;
    }

    public ushort InitialFallingYPosition
    {
        get => _initialFallingYPositions[_slot.SlotIndex];
        internal set => _initialFallingYPositions[_slot.SlotIndex] = value;
    }

    public ushort FallingXOrigin
    {
        get => _fallingXOrigins[_slot.SlotIndex];
        internal set => _fallingXOrigins[_slot.SlotIndex] = value;
    }

    public ushort FallingYOrigin
    {
        get => _fallingYOrigins[_slot.SlotIndex];
        internal set => _fallingYOrigins[_slot.SlotIndex] = value;
    }

    public ushort InitialYSpeedTableIndex
    {
        get => _initialYSpeedTableIndexes[_slot.SlotIndex];
        internal set => _initialYSpeedTableIndexes[_slot.SlotIndex] = value;
    }

    /// <summary>
    /// Parameter-two countdown. The jump begins only after decrementing through zero to
    /// <c>$FFFF</c>, so a population delay of zero launches on the first preparation frame.
    /// </summary>
    public ushort JumpDelayTimer
    {
        get => _jumpDelayTimers[_slot.SlotIndex];
        internal set => _jumpDelayTimers[_slot.SlotIndex] = value;
    }
}

/// <summary>Literal translation of Choot enemy <c>$D3BF</c> at <c>$A2:D82C-$E143</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort ChootDefinition = 0xd3bf;

    private const int ChootPatternPointerTable = 0xa2df5e;
    private const int ChootPatternDistancePointerTable = 0xa2df6a;
    private const ushort ChootIdleInstruction = 0xd82c;
    private const ushort ChootJumpingInstruction = 0xd834;
    private const ushort ChootFallingInstruction = 0xd840;
    private const ushort ChootActivationDistance = 0x0050;

    private readonly ushort[] _chootSpawnXPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootSpawnYPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootInitialFallingXPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootInitialFallingYPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootFallingXOrigins = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootFallingYOrigins = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootInitialYSpeedTableIndexes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _chootJumpDelayTimers = new ushort[MaximumEnemyCount];
    private readonly ChootEnemyState?[] _chootStates =
        new ChootEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Choot state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<ChootEnemyState?> ChootStates => _chootStates;

    /// <summary>Ports <c>InitAI_Choot</c> at <c>$A2:DF76</c>.</summary>
    private void InitializeChoot(RoomEnemySlot slot)
    {
        var state = new ChootEnemyState(
            slot,
            _chootSpawnXPositions,
            _chootSpawnYPositions,
            _chootInitialFallingXPositions,
            _chootInitialFallingYPositions,
            _chootFallingXOrigins,
            _chootFallingYOrigins,
            _chootInitialYSpeedTableIndexes,
            _chootJumpDelayTimers);
        _chootStates[slot.SlotIndex] = state;

        SetChootInstructionList(slot, ChootIdleInstruction);
        state.Function = ChootEnemyFunction.WaitingForSamus;
        state.SpawnXPosition = slot.XPosition;
        state.SpawnYPosition = slot.YPosition;

        // Parameter one's high byte selects one of five genuine pattern streams. The sixth
        // ROM table word points back into the pointer table and is documented garbage; no
        // retail population selects it, so expose corrupt/high parameters as invalid data.
        int patternIndex = (slot.Parameter1 >> 8) & 0xff;
        if (patternIndex >= 5)
        {
            throw new InvalidDataException(
                $"Choot falling-pattern index {patternIndex} exceeds its five real streams.");
        }
        state.FallingPatternPointer = ReadWord(
            _bus!,
            ChootPatternPointerTable + patternIndex * 2);
        ushort distancePointer = ReadWord(
            _bus!,
            ChootPatternDistancePointerTable + patternIndex * 2);
        state.FallingPatternYDistance = ReadWord(_bus!, 0xa20000 | distancePointer);

        // The SNES multiplier consumes only the low bytes. Retail loop counts are nonzero,
        // but byte multiplication also preserves the native result for debugger corruption.
        ushort jumpHeight = unchecked((ushort)(
            (slot.Parameter1 & 0x00ff) * (state.FallingPatternYDistance & 0x00ff)));
        state.InitialYSpeedTableIndex = CalculateInitialChootYSpeedTableIndex(jumpHeight);
        state.InitialFallingXPosition = state.SpawnXPosition;
        state.InitialFallingYPosition = unchecked((ushort)(
            state.SpawnYPosition - jumpHeight));
        state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
    }

    /// <summary>Ports <c>MainAI_Choot</c> and its four targets at <c>$A2:E02E-$E143</c>.</summary>
    private void RunChootMain(
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
        ushort distance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        if ((distance & 0x8000) != 0)
            distance = unchecked((ushort)-distance);
        if (unchecked((short)(distance - ChootActivationDistance)) >= 0)
            return;

        state.JumpDelayTimer = slot.Parameter2;
        state.Function = ChootEnemyFunction.PreparingJump;
    }

    private static void PrepareChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        state.JumpDelayTimer = unchecked((ushort)(state.JumpDelayTimer - 1));
        if (unchecked((short)state.JumpDelayTimer) >= 0)
            return;

        SetChootInstructionList(slot, ChootJumpingInstruction);
        state.Function = ChootEnemyFunction.Jumping;
    }

    private void RunChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        // Only the cursor's high byte reaches the eight-byte quadratic table index. Choot
        // applies the table's separately stored negative pair, including its fractional
        // negation bug, without consulting room collision during this authored leap.
        ushort tableIndex = unchecked((ushort)(state.YSpeedTableIndex >> 8));
        int upwardDisplacement = ReadQuadraticEnemySpeed(tableIndex, negative: true);
        uint fixedY = ((uint)slot.YPosition << 16) | slot.YSubposition;
        fixedY = unchecked(fixedY + (uint)upwardDisplacement);
        slot.YPosition = unchecked((ushort)(fixedY >> 16));
        slot.YSubposition = unchecked((ushort)fixedY);

        state.YSpeedTableIndex = unchecked((ushort)(state.YSpeedTableIndex - 0x0200));
        if (unchecked((short)state.YSpeedTableIndex) >= 0)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // subpositions deliberately survive until the final spawn reset, just as in WRAM.
        slot.XPosition = state.InitialFallingXPosition;
        state.FallingXOrigin = state.InitialFallingXPosition;
        slot.YPosition = state.InitialFallingYPosition;
        state.FallingYOrigin = state.InitialFallingYPosition;
        state.FallingPatternIndex = 0;
        state.FallingPatternLoopCounter = unchecked((ushort)(
            (slot.Parameter1 & 0x00ff) - 1));
        SetChootInstructionList(slot, ChootFallingInstruction);
        state.Function = ChootEnemyFunction.Falling;
    }

    private void RunChootFall(RoomEnemySlot slot, ChootEnemyState state)
    {
        int frameIndex = (state.FallingPatternIndex >> 8) & 0xff;
        int entryAddress = 0xa20000 |
            unchecked((ushort)(state.FallingPatternPointer + frameIndex * 4));
        ushort xOffset = ReadWord(_bus!, entryAddress);
        if (xOffset == 0x8000)
        {
            state.FallingYOrigin = unchecked((ushort)(
                state.FallingYOrigin + state.FallingPatternYDistance));
            state.FallingPatternIndex = 0;
            state.FallingPatternLoopCounter = unchecked((ushort)(
                state.FallingPatternLoopCounter - 1));
            if (unchecked((short)state.FallingPatternLoopCounter) >= 0)
                return;

            // The last terminator restores exact spawn coordinates/subpositions, reinstalls
            // idle bytecode, and rearms the same calculated ascent for the next activation.
            state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
            slot.XPosition = state.SpawnXPosition;
            slot.XSubposition = 0;
            slot.YPosition = state.SpawnYPosition;
            slot.YSubposition = 0;
            SetChootInstructionList(slot, ChootIdleInstruction);
            state.Function = ChootEnemyFunction.WaitingForSamus;
            return;
        }

        short yOffset = unchecked((short)ReadWord(_bus!, entryAddress + 2));
        slot.XPosition = unchecked((ushort)(
            state.FallingXOrigin + unchecked((short)xOffset)));
        slot.YPosition = unchecked((ushort)(state.FallingYOrigin + yOffset));
        state.FallingPatternIndex = unchecked((ushort)(
            state.FallingPatternIndex + 0x0100));
    }

    /// <summary>Ports <c>CalculateChootInitialJumpSpeed</c> at <c>$A2:DFE9</c>.</summary>
    private ushort CalculateInitialChootYSpeedTableIndex(ushort jumpHeight)
    {
        ushort encodedIndex = 0;
        uint accumulatedDistance = 0;
        for (int iteration = 0; iteration < 128; iteration++)
        {
            encodedIndex = unchecked((ushort)(encodedIndex + 0x0200));
            ushort tableIndex = unchecked((ushort)(encodedIndex >> 8));
            accumulatedDistance = unchecked(accumulatedDistance +
                (uint)ReadQuadraticEnemySpeed(tableIndex, negative: false));
            ushort wholeDistance = unchecked((ushort)(accumulatedDistance >> 16));
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
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private ChootEnemyState RequireChootState(RoomEnemySlot slot) =>
        _chootStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Choot state.");
}
