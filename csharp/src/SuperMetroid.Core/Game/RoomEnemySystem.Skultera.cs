using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skultera's variable A.</summary>
public enum SkulteraEnemyFunction : ushort
{
    SwimmingLeft = 0x9132,
    SwimmingRight = 0x91ab,
    TurningRight = 0x9224,
    TurningLeft = 0x9256,
}

/// <summary>
/// Typed view of Skultera's six common-slot words and five per-slot words in extra WRAM.
/// The upstream symbols spell the structure <c>Skulltera</c>, but the enemy header and the
/// game's visible name use <c>Skultera</c>; the public decompilation API follows the latter.
/// </summary>
public sealed class SkulteraEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _radii;
    private readonly ushort[] _turnFinishedFlags;
    private readonly ushort[] _angleDeltas;
    private readonly ushort[] _previousYOffsets;
    private readonly ushort[] _currentYOffsets;

    internal SkulteraEnemyState(
        RoomEnemySlot slot,
        ushort[] radii,
        ushort[] turnFinishedFlags,
        ushort[] angleDeltas,
        ushort[] previousYOffsets,
        ushort[] currentYOffsets)
    {
        _slot = slot;
        _radii = radii;
        _turnFinishedFlags = turnFinishedFlags;
        _angleDeltas = angleDeltas;
        _previousYOffsets = previousYOffsets;
        _currentYOffsets = currentYOffsets;
    }

    /// <summary>Direct bank-$A3 state-function pointer dispatched by main AI.</summary>
    public SkulteraEnemyFunction Function
    {
        get => (SkulteraEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Fractional word of the positive/right signed 16.16 speed.</summary>
    public ushort RightSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Whole word of the positive/right signed 16.16 speed.</summary>
    public short RightVelocity
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    /// <summary>Fractional word of the negative/left signed 16.16 speed.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Whole word of the negative/left signed 16.16 speed.</summary>
    public short LeftVelocity
    {
        get => unchecked((short)_slot.VariableE);
        internal set => _slot.VariableE = unchecked((ushort)value);
    }

    /// <summary>Low-byte sine-table angle; high-byte garbage is discarded after movement.</summary>
    public ushort Angle
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Low byte of population parameter two: vertical oscillation radius.</summary>
    public ushort Radius
    {
        get => _radii[_slot.SlotIndex];
        internal set => _radii[_slot.SlotIndex] = value;
    }

    /// <summary>Published by animation command <c>$A3:90AA</c> after a complete turn.</summary>
    public bool TurnFinished
    {
        get => _turnFinishedFlags[_slot.SlotIndex] != 0;
        internal set => _turnFinishedFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Signed low-byte angle increment. Native code stores a word and negates the complete
    /// word at every turn, so the debugger-facing value deliberately remains a <c>short</c>.
    /// </summary>
    public short AngleDelta
    {
        get => unchecked((short)_angleDeltas[_slot.SlotIndex]);
        internal set => _angleDeltas[_slot.SlotIndex] = unchecked((ushort)value);
    }

    /// <summary>Sine result retained from the preceding successful movement attempt.</summary>
    public short PreviousYOffset
    {
        get => unchecked((short)_previousYOffsets[_slot.SlotIndex]);
        internal set => _previousYOffsets[_slot.SlotIndex] = unchecked((ushort)value);
    }

    /// <summary>Current sine result at extra WRAM <c>$7E:7808,x</c>.</summary>
    public short CurrentYOffset
    {
        get => unchecked((short)_currentYOffsets[_slot.SlotIndex]);
        internal set => _currentYOffsets[_slot.SlotIndex] = unchecked((ushort)value);
    }

    /// <summary>Complete signed 16.16 rightward displacement selected at initialization.</summary>
    public int RightDisplacement => (RightVelocity << 16) | RightSubvelocity;

    /// <summary>Complete signed 16.16 leftward displacement selected at initialization.</summary>
    public int LeftDisplacement => (LeftVelocity << 16) | LeftSubvelocity;
}

/// <summary>Literal translation of Skultera enemy <c>$D6FF</c> at <c>$A3:902A-$9287</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort SkulteraDefinition = 0xd6ff;

    private const ushort SkulteraSwimmingLeftInstruction = 0x902a;
    private const ushort SkulteraTurningRightInstruction = 0x903c;
    private const ushort SkulteraSwimmingRightInstruction = 0x9060;
    private const ushort SkulteraTurningLeftInstruction = 0x9072;

    private readonly ushort[] _skulteraRadii = new ushort[MaximumEnemyCount];
    private readonly ushort[] _skulteraTurnFinishedFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _skulteraAngleDeltas = new ushort[MaximumEnemyCount];
    private readonly ushort[] _skulteraPreviousYOffsets = new ushort[MaximumEnemyCount];
    private readonly ushort[] _skulteraCurrentYOffsets = new ushort[MaximumEnemyCount];
    private readonly SkulteraEnemyState?[] _skulteraStates =
        new SkulteraEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Skultera state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<SkulteraEnemyState?> SkulteraStates => _skulteraStates;

    /// <summary>Ports <c>InitAI_Skultera</c> at <c>$A3:90B5</c>.</summary>
    private void InitializeSkultera(RoomEnemySlot slot)
    {
        var state = new SkulteraEnemyState(
            slot,
            _skulteraRadii,
            _skulteraTurnFinishedFlags,
            _skulteraAngleDeltas,
            _skulteraPreviousYOffsets,
            _skulteraCurrentYOffsets);
        _skulteraStates[slot.SlotIndex] = state;

        // Parameter one's high byte selects initial facing: zero means right, any nonzero
        // value means left. Its low byte is multiplied by eight because each common speed
        // magnitude stores adjacent {right whole, right fraction, left whole, left fraction}
        // words. Keep the byte offset visible; treating it as an array index is a common
        // source of subtly wrong half-pixel speeds in ports of this AI.
        bool startsLeft = (slot.Parameter1 & 0xff00) != 0;
        slot.CurrentInstruction = startsLeft
            ? SkulteraSwimmingLeftInstruction
            : SkulteraSwimmingRightInstruction;
        state.Function = startsLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;

        ushort speedTableOffset = unchecked((ushort)((slot.Parameter1 & 0x00ff) * 8));
        (state.RightVelocity, state.RightSubvelocity) =
            ReadLinearEnemySpeed(speedTableOffset);
        (state.LeftVelocity, state.LeftSubvelocity) =
            ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));

        // Parameter two packs radius in its low byte and unsigned initial angle delta in
        // its high byte. The delta becomes a signed word only when a turn negates it.
        state.Radius = unchecked((ushort)(slot.Parameter2 & 0x00ff));
        state.AngleDelta = unchecked((short)((slot.Parameter2 >> 8) & 0x00ff));
        state.Angle = 0;
        state.TurnFinished = false;
        state.CurrentYOffset = 0;
        state.PreviousYOffset = unchecked((short)ReadEightBitNegativeSineProduct(
            state.Angle,
            state.Radius));
    }

    /// <summary>Ports <c>MainAI_Skultera</c> at <c>$A3:912B</c>.</summary>
    private void RunSkulteraMain(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Skultera AI requires active room collision data.");

        switch (state.Function)
        {
            case SkulteraEnemyFunction.SwimmingLeft:
                SwimSkultera(slot, state, level, movingLeft: true);
                return;
            case SkulteraEnemyFunction.SwimmingRight:
                SwimSkultera(slot, state, level, movingLeft: false);
                return;
            case SkulteraEnemyFunction.TurningRight:
                FinishSkulteraTurn(slot, state, nowMovingLeft: false);
                return;
            case SkulteraEnemyFunction.TurningLeft:
                FinishSkulteraTurn(slot, state, nowMovingLeft: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Skultera function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the mirrored swimming functions at <c>$A3:9132/$91AB</c>.</summary>
    private void SwimSkultera(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData level,
        bool movingLeft)
    {
        int horizontalDisplacement = movingLeft
            ? state.LeftDisplacement
            : state.RightDisplacement;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: movingLeft);
            // Horizontal collision jumps directly to the native merge label. Consequently
            // CurrentYOffset remains the preceding frame's word and is copied below again.
            state.PreviousYOffset = state.CurrentYOffset;
            return;
        }

        state.CurrentYOffset = unchecked((short)ReadEightBitNegativeSineProduct(
            state.Angle,
            state.Radius));
        short yDifference = unchecked((short)(
            state.CurrentYOffset - state.PreviousYOffset));
        if (MoveEnemyVertically(level, slot, yDifference << 16))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: movingLeft);
        }
        else
        {
            // Only unobstructed movement advances the low-byte phase. The AND is not
            // cosmetic: native AI discards any high word produced by signed delta wrapping.
            state.Angle = unchecked((ushort)((state.Angle + state.AngleDelta) & 0x00ff));
        }

        // Both successful movement and vertical collision retain the newly calculated
        // offset. This prevents a failed vertical step from being retried after the turn.
        state.PreviousYOffset = state.CurrentYOffset;
    }

    private static void BeginSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool currentlyMovingLeft)
    {
        state.Function = currentlyMovingLeft
            ? SkulteraEnemyFunction.TurningRight
            : SkulteraEnemyFunction.TurningLeft;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = currentlyMovingLeft
            ? SkulteraTurningRightInstruction
            : SkulteraTurningLeftInstruction;
    }

    /// <summary>Ports the mirrored turn completion functions at <c>$A3:9224/$9256</c>.</summary>
    private static void FinishSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool nowMovingLeft)
    {
        if (!state.TurnFinished)
            return;

        state.TurnFinished = false;
        state.Function = nowMovingLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;
        state.AngleDelta = unchecked((short)-state.AngleDelta);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = nowMovingLeft
            ? SkulteraSwimmingLeftInstruction
            : SkulteraSwimmingRightInstruction;
    }

    private SkulteraEnemyState RequireSkulteraState(RoomEnemySlot slot) =>
        _skulteraStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skultera state.");
}
