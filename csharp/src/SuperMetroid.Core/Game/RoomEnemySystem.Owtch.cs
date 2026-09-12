namespace SuperMetroid.Core.Game;

/// <summary>
/// Values stored in Owtch's native <c>$0FB0,x</c> direction/state word. Unlike a simple
/// facing enum, this word also dispatches the underground animation. The all-ones value is
/// a real one-frame table-underflow state produced by retail code at the left patrol bound.
/// </summary>
public enum OwtchBehaviorState : ushort
{
    MovingLeft = 0,
    MovingRight = 1,
    Underground = 2,
    Sinking = 3,
    Rising = 4,
    LeftBoundaryUnderflow = 0xffff,
}

/// <summary>
/// Typed view of Owtch's six ordinary enemy words and three extended WRAM words. Keeping
/// the native split velocities visible makes debugger watches directly comparable with
/// <c>$0FA8-$0FB2</c> and the extended <c>$7E:7800-$7804</c> record.
/// </summary>
public sealed class OwtchEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal OwtchEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>$0FA8,x</c>, fractional half of positive 16.16 speed.</summary>
    public ushort RightSubvelocity { get; internal set; }

    /// <summary>Native <c>$0FAA,x</c>, whole half of positive 16.16 speed.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native <c>$0FAC,x</c>, fractional half of negative 16.16 speed.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native <c>$0FAE,x</c>, whole half of negative 16.16 speed.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native <c>$0FB0,x</c> state/table index.</summary>
    public OwtchBehaviorState Behavior
    {
        get => (OwtchBehaviorState)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Native <c>$0FB2,x</c>, current zero-to-sixteen burial depth.</summary>
    public ushort SinkYOffset
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native extended word <c>$7E:7800,x</c>.</summary>
    public ushort UndergroundTimer { get; internal set; }

    /// <summary>Native extended word <c>$7E:7802,x</c>.</summary>
    public ushort MinimumXPosition { get; internal set; }

    /// <summary>Native extended word <c>$7E:7804,x</c>.</summary>
    public ushort MaximumXPosition { get; internal set; }
}

/// <summary>
/// Literal translation of retail Owtch enemy <c>$A0:D03F</c> (the Maridia spiky shell),
/// covering initialization, split 16.16 patrol, randomized burial, underground waiting,
/// rising, instruction-driven direction changes, and its state-dependent shot callback.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort OwtchDefinition = 0xd03f;
    internal const ushort OwtchShotAi = EnemyAiCodePointers.BankA2.OwtchShot;

    private const ushort OwtchMovingLeftInstructionList = 0xa3ab;
    private const ushort OwtchMovingRightInstructionList = 0xa3bd;
    private const int OwtchTravelDistanceTable = 0xa2a3dd;
    private const int OwtchUndergroundTimerTable = 0xa2a3ed;
    private const int OwtchTravelDistanceCount = 8;
    private const int OwtchUndergroundTimerCount = 6;
    private const ushort OwtchMaximumBurialDepth = 16;

    private readonly OwtchEnemyState?[] _owtchStates =
        new OwtchEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical Owtch-capable enemy slot.</summary>
    public IReadOnlyList<OwtchEnemyState?> OwtchStates => _owtchStates;

    /// <summary>Ports <c>InitAI_Owtch</c> at $A2:A3F9.</summary>
    private void InitializeOwtch(RoomEnemySlot slot)
    {
        int initialState = slot.Parameter1 & 0x00ff;
        int undergroundTimerIndex = slot.Parameter1 >> 8;
        int speedIndex = slot.Parameter2 & 0x00ff;
        int distanceIndex = slot.Parameter2 >> 8;
        if (initialState >= 5)
        {
            throw new InvalidDataException(
                $"Owtch parameter one ${slot.Parameter1:X4} selects state {initialState}, " +
                "outside its five-entry main-AI table.");
        }
        if (undergroundTimerIndex >= OwtchUndergroundTimerCount)
        {
            throw new InvalidDataException(
                $"Owtch parameter one ${slot.Parameter1:X4} selects underground timer " +
                $"index {undergroundTimerIndex}, outside its six-word table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Owtch parameter two ${slot.Parameter2:X4} selects linear-speed " +
                $"index {speedIndex}, outside the NTSC retail table.");
        }
        if (distanceIndex >= OwtchTravelDistanceCount)
        {
            throw new InvalidDataException(
                $"Owtch parameter two ${slot.Parameter2:X4} selects travel-distance " +
                $"index {distanceIndex}, outside its eight-word table.");
        }

        // The initializer masks the raw state only for its two-entry facing table. Thus
        // states two and four begin with the left list, while state three begins right.
        SetOwtchInstructionList(
            slot,
            (initialState & 1) == 0
                ? OwtchMovingLeftInstructionList
                : OwtchMovingRightInstructionList);

        int speedRecord = speedIndex * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedRecord);
        var left = EnemyLinearSpeedDefinitions.Read(speedRecord + 4);
        ushort travelDistance = ReadWord(
            _bus!,
            OwtchTravelDistanceTable + distanceIndex * 2);
        var state = new OwtchEnemyState(slot)
        {
            RightVelocity = unchecked((ushort)right.Whole),
            RightSubvelocity = right.Fraction,
            LeftVelocity = unchecked((ushort)left.Whole),
            LeftSubvelocity = left.Fraction,
            Behavior = (OwtchBehaviorState)initialState,
            SinkYOffset = 0,
            UndergroundTimer = ReadOwtchUndergroundTimer(slot),
            MinimumXPosition = unchecked((ushort)(slot.XPosition - travelDistance)),
            MaximumXPosition = unchecked((ushort)(slot.XPosition + travelDistance)),
        };
        _owtchStates[slot.SlotIndex] = state;

        // State two is the cartridge's special initially-buried form. It moves only the
        // whole coordinate and records the same sixteen-pixel depth used by normal sinking.
        if (state.Behavior == OwtchBehaviorState.Underground)
        {
            state.SinkYOffset = OwtchMaximumBurialDepth;
            slot.YPosition = unchecked((ushort)(slot.YPosition + OwtchMaximumBurialDepth));
        }
    }

    /// <summary>Ports <c>MainAI_Owtch</c> and its indirect state table at $A2:A47E.</summary>
    private void RunOwtchMain(RoomEnemySlot slot, OwtchEnemyState state)
    {
        switch (state.Behavior)
        {
            case OwtchBehaviorState.MovingLeft:
                MoveOwtch(slot, state, movingRight: false);
                MaybeMakeOwtchSink(slot, state);
                return;

            case OwtchBehaviorState.MovingRight:
                MoveOwtch(slot, state, movingRight: true);
                MaybeMakeOwtchSink(slot, state);
                return;

            case OwtchBehaviorState.Underground:
                state.UndergroundTimer = unchecked((ushort)(state.UndergroundTimer - 1));
                if (state.UndergroundTimer == 0)
                    state.Behavior = OwtchBehaviorState.Rising;
                return;

            case OwtchBehaviorState.Sinking:
                slot.YPosition = unchecked((ushort)(slot.YPosition + 1));
                state.SinkYOffset = unchecked((ushort)(state.SinkYOffset + 1));
                if (unchecked((short)(state.SinkYOffset - OwtchMaximumBurialDepth)) >= 0)
                {
                    state.Behavior = OwtchBehaviorState.Underground;
                    state.UndergroundTimer = ReadOwtchUndergroundTimer(slot);
                }
                return;

            case OwtchBehaviorState.Rising:
                slot.YPosition = unchecked((ushort)(slot.YPosition - 1));
                state.SinkYOffset = unchecked((ushort)(state.SinkYOffset - 1));
                if (state.SinkYOffset == 0)
                {
                    // $A2:A549 samples RandomNumberSeed+1 without advancing the RNG. The
                    // gameplay runtime supplies that read seam; isolated callers without it
                    // retain the established advancing fallback used by other translated AI.
                    ushort random = RequireRandomNumber();
                    state.Behavior = (random & 0x0100) == 0
                        ? OwtchBehaviorState.MovingLeft
                        : OwtchBehaviorState.MovingRight;
                }
                return;

            case OwtchBehaviorState.LeftBoundaryUnderflow:
                // The moving-left routine decrements zero instead of incrementing it. On
                // the next frame, direction $FFFF indexes two bytes before the function
                // table and accidentally calls the right-list initializer at $A2:A49D.
                SetOwtchInstructionList(slot, OwtchMovingRightInstructionList);
                return;

            default:
                throw new InvalidDataException(
                    $"Owtch state/table index ${(ushort)state.Behavior:X4} is not translated.");
        }
    }

    /// <summary>Ports the carry-sensitive additions at $A2:A4B0/$A4D9.</summary>
    private static void MoveOwtch(
        RoomEnemySlot slot,
        OwtchEnemyState state,
        bool movingRight)
    {
        ushort subvelocity = movingRight
            ? state.RightSubvelocity
            : state.LeftSubvelocity;
        ushort velocity = movingRight
            ? state.RightVelocity
            : state.LeftVelocity;

        uint fractionalSum = (uint)slot.XSubposition + subvelocity;
        slot.XSubposition = unchecked((ushort)fractionalSum);
        if (fractionalSum > ushort.MaxValue)
            slot.XPosition = unchecked((ushort)(slot.XPosition + 1));
        slot.XPosition = unchecked((ushort)(slot.XPosition + velocity));

        if (!movingRight)
        {
            if (!IsNegative16(unchecked((ushort)(
                    slot.XPosition - state.MinimumXPosition))))
            {
                return;
            }

            // This DEC is an original-game bug. Zero wraps to $FFFF; main AI then indexes
            // the preceding initializer pointer rather than a declared function entry.
            state.Behavior = OwtchBehaviorState.LeftBoundaryUnderflow;
            return;
        }

        if (!IsNegative16(unchecked((ushort)(
                slot.XPosition - state.MaximumXPosition))))
        {
            state.Behavior = OwtchBehaviorState.MovingLeft;
        }
    }

    /// <summary>Ports Owtch's six-in-256 per-frame burial gate at $A2:A553.</summary>
    private void MaybeMakeOwtchSink(RoomEnemySlot slot, OwtchEnemyState state)
    {
        ushort random = _nextRandom!();
        byte decision = unchecked((byte)((byte)random + (byte)slot.FrameCounter));
        if (decision < 6)
            state.Behavior = OwtchBehaviorState.Sinking;
    }

    /// <summary>Animation command $A2:A56D: select the moving-left function index.</summary>
    private static void SetOwtchMovingLeft(OwtchEnemyState state) =>
        state.Behavior = OwtchBehaviorState.MovingLeft;

    /// <summary>Animation command $A2:A571: select the moving-right function index.</summary>
    private static void SetOwtchMovingRight(OwtchEnemyState state) =>
        state.Behavior = OwtchBehaviorState.MovingRight;

    /// <summary>
    /// Enemy-shot callback $A2:A579 calls common damage only when signed(state - 1) is
    /// negative. In shipped states that means moving left (plus the transient $FFFF bug).
    /// </summary>
    private static bool OwtchAcceptsOrdinaryShot(OwtchEnemyState state) =>
        unchecked((short)((ushort)state.Behavior - 1)) < 0;

    private ushort ReadOwtchUndergroundTimer(RoomEnemySlot slot) =>
        ReadWord(_bus!, OwtchUndergroundTimerTable + (slot.Parameter1 >> 8) * 2);

    private static void SetOwtchInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private OwtchEnemyState RequireOwtchState(RoomEnemySlot slot) =>
        _owtchStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Owtch state.");
}
