namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function pointer stored in Kzan top variable A. These are kept as ROM
/// addresses so debugger watches agree directly with <c>$0FA8,x</c> and corrupt/custom
/// population state fails at the exact indirect target instead of becoming a plausible enum.
/// </summary>
public enum KzanEnemyFunction : ushort
{
    WaitingToFall = 0x8bb4,
    Falling = 0x8bdc,
    WaitingToRise = 0x8c4a,
    Rising = 0x8c5d,
}

/// <summary>
/// Debugger-facing projection of every private Kzan-top word. The first six fields alias
/// ordinary enemy variables A-F; the final four live in the native bank-$7E extension arrays.
/// The bottom half has no private state: it follows the immediately preceding top slot.
/// </summary>
public sealed class KzanEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _fallWaitTimerResetValues;
    private readonly ushort[] _previousYPositions;
    private readonly ushort[] _fallingYSpeedTableIndexes;
    private readonly ushort[] _riseWaitTimers;

    internal KzanEnemyState(
        RoomEnemySlot slot,
        ushort[] fallWaitTimerResetValues,
        ushort[] previousYPositions,
        ushort[] fallingYSpeedTableIndexes,
        ushort[] riseWaitTimers)
    {
        _slot = slot;
        _fallWaitTimerResetValues = fallWaitTimerResetValues;
        _previousYPositions = previousYPositions;
        _fallingYSpeedTableIndexes = fallingYSpeedTableIndexes;
        _riseWaitTimers = riseWaitTimers;
    }

    public KzanEnemyFunction Function
    {
        get => (KzanEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort FallWaitTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort RisingTargetYPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort FallingTargetYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort InitialFallingYSubspeed
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort InitialFallingYSpeed
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort FallWaitTimerResetValue
    {
        get => _fallWaitTimerResetValues[_slot.SlotIndex];
        internal set => _fallWaitTimerResetValues[_slot.SlotIndex] = value;
    }

    public ushort PreviousYPosition
    {
        get => _previousYPositions[_slot.SlotIndex];
        internal set => _previousYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>
    /// Byte offset into <c>CommonEnemySpeeds_LinearlyIncreasing</c>. Kzan advances by eight
    /// bytes per falling frame because each logical speed owns positive and negative pairs.
    /// </summary>
    public ushort FallingYSpeedTableIndex
    {
        get => _fallingYSpeedTableIndexes[_slot.SlotIndex];
        internal set => _fallingYSpeedTableIndexes[_slot.SlotIndex] = value;
    }

    public ushort RiseWaitTimer
    {
        get => _riseWaitTimers[_slot.SlotIndex];
        internal set => _riseWaitTimers[_slot.SlotIndex] = value;
    }
}

/// <summary>
/// Literal translation of Kzan (the paired spike platform/crusher) from
/// <c>$A6:8B09-$8CE4</c>. The visible top owns animation, acceleration, solidity, contact
/// damage, and rider displacement. The invisible bottom exists as a separate retail enemy
/// slot and copies the preceding top's position plus twelve pixels on every actor frame.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort KzanTopDefinition = 0xdfff;
    internal const ushort KzanBottomDefinition = 0xe03f;

    private const ushort KzanInstructionList = 0x8b29;
    private const ushort KzanLandingSoundEffect = 0x001b;
    private const ushort KzanNtscRiseWaitFrames = 0x0040;
    private const ushort KzanNtscRisingSubspeed = 0x8000;
    private const ushort KzanMaximumFallingSpeedTableOffset = 0x0200;

    private readonly KzanEnemyState?[] _kzanStates =
        new KzanEnemyState?[MaximumEnemyCount];
    private readonly ushort[] _kzanFallWaitTimerResetValues = new ushort[MaximumEnemyCount];
    private readonly ushort[] _kzanPreviousYPositions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _kzanFallingYSpeedTableIndexes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _kzanRiseWaitTimers = new ushort[MaximumEnemyCount];

    /// <summary>Typed state for Kzan top halves; bottom and unrelated slots are null.</summary>
    public IReadOnlyList<KzanEnemyState?> KzanStates => _kzanStates;

    private static bool IsKzanDefinition(ushort definitionPointer) =>
        definitionPointer is KzanTopDefinition or KzanBottomDefinition;

    /// <summary>Ports <c>InitAI_KzanTop</c> at <c>$A6:8B2F</c>.</summary>
    private void InitializeKzanTop(RoomEnemySlot slot)
    {
        var state = new KzanEnemyState(
            slot,
            _kzanFallWaitTimerResetValues,
            _kzanPreviousYPositions,
            _kzanFallingYSpeedTableIndexes,
            _kzanRiseWaitTimers);
        _kzanStates[slot.SlotIndex] = state;

        slot.CurrentInstruction = KzanInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        state.Function = KzanEnemyFunction.WaitingToFall;

        // init0's low byte is a logical speed-table index. The 65C816 multiplies it by
        // eight with three ASLs and permits word wrapping; do the same before reading the
        // cartridge table. Retail values include $40, selecting its final NTSC entry.
        ushort speedTableOffset = unchecked((ushort)((slot.Parameter1 & 0x00ff) << 3));
        state.FallingYSpeedTableIndex = speedTableOffset;
        (short initialWhole, ushort initialFraction) = ReadLinearEnemySpeed(speedTableOffset);
        state.InitialFallingYSubspeed = initialFraction;
        state.InitialFallingYSpeed = unchecked((ushort)initialWhole);

        // init1 packs two unrelated bytes: the high byte is fall distance in pixels and the
        // low byte is the top wait countdown. A zero countdown deliberately wraps to $FFFF
        // on its first DEC rather than triggering immediately.
        state.FallingTargetYPosition = unchecked((ushort)(
            slot.YPosition + (slot.Parameter2 >> 8)));
        state.RisingTargetYPosition = slot.YPosition;
        state.FallWaitTimerResetValue = unchecked((byte)slot.Parameter2);
        state.FallWaitTimer = state.FallWaitTimerResetValue;
    }

    /// <summary>Ports <c>InitAI_KzanBottom</c> at <c>$A6:8B85</c>.</summary>
    private void InitializeKzanBottom(RoomEnemySlot slot) => FollowKzanTop(slot);

    /// <summary>Ports <c>MainAI_KzanBottom</c> at <c>$A6:8B99</c>.</summary>
    private void RunKzanBottomMain(RoomEnemySlot slot) => FollowKzanTop(slot);

    /// <summary>
    /// Reproduces the literal <c>Enemy[-1]</c> reads. Retail populations always place the
    /// bottom immediately after its top; only slot zero is rejected because there is no
    /// managed representation of the unrelated WRAM that malformed data would under-read.
    /// </summary>
    private void FollowKzanTop(RoomEnemySlot bottom)
    {
        if (bottom.SlotIndex == 0)
            throw new InvalidDataException("Kzan bottom cannot occupy enemy slot zero.");

        RoomEnemySlot precedingSlot = _slots[bottom.SlotIndex - 1];
        bottom.XPosition = precedingSlot.XPosition;
        bottom.YPosition = unchecked((ushort)(precedingSlot.YPosition + 12));
    }

    /// <summary>Ports the indirect <c>MainAI_KzanTop</c> dispatcher at <c>$A6:8BAD</c>.</summary>
    private void RunKzanTopMain(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case KzanEnemyFunction.WaitingToFall:
                RunKzanWaitingToFall(slot, state);
                return;

            case KzanEnemyFunction.Falling:
                RunKzanFalling(slot, state, samus);
                return;

            case KzanEnemyFunction.WaitingToRise:
                RunKzanWaitingToRise(state);
                return;

            case KzanEnemyFunction.Rising:
                RunKzanRising(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Kzan function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>Function_Kzan_WaitingToFall</c> at <c>$A6:8BB4</c>.</summary>
    private static void RunKzanWaitingToFall(RoomEnemySlot slot, KzanEnemyState state)
    {
        state.FallWaitTimer = unchecked((ushort)(state.FallWaitTimer - 1));
        if (state.FallWaitTimer != 0)
            return;

        state.FallWaitTimer = state.FallWaitTimerResetValue;
        state.FallingYSpeedTableIndex = unchecked((ushort)(
            (slot.Parameter1 & 0x00ff) << 3));
        state.Function = KzanEnemyFunction.Falling;
    }

    /// <summary>Ports <c>Function_Kzan_Falling</c> at <c>$A6:8BDC</c>.</summary>
    private void RunKzanFalling(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        state.PreviousYPosition = slot.YPosition;
        (short wholeSpeed, ushort fractionalSpeed) =
            ReadLinearEnemySpeed(state.FallingYSpeedTableIndex);
        AddKzanY(slot, wholeSpeed, fractionalSpeed);

        // CMP/BMI tests only the sign bit of the wrapped subtraction. That is intentionally
        // not a host integer >= comparison; coordinates near the word boundary retain the
        // same cartridge behavior.
        if (!IsNegative16(slot.YPosition - state.FallingTargetYPosition))
        {
            state.RiseWaitTimer = KzanNtscRiseWaitFrames;
            state.Function = KzanEnemyFunction.WaitingToRise;
            slot.YPosition = state.FallingTargetYPosition;
            LastKzanSoundEffect = KzanLandingSoundEffect;
        }

        CarrySamusWithKzanIfTouchingFromBelow(slot, state, samus);

        state.FallingYSpeedTableIndex = unchecked((ushort)(
            state.FallingYSpeedTableIndex + 8));
        if (!IsNegative16(
                state.FallingYSpeedTableIndex - KzanMaximumFallingSpeedTableOffset))
        {
            state.FallingYSpeedTableIndex = KzanMaximumFallingSpeedTableOffset;
        }
    }

    /// <summary>Ports <c>Function_Kzan_WaitingToRise</c> at <c>$A6:8C4A</c>.</summary>
    private static void RunKzanWaitingToRise(KzanEnemyState state)
    {
        state.RiseWaitTimer = unchecked((ushort)(state.RiseWaitTimer - 1));
        if (state.RiseWaitTimer == 0)
            state.Function = KzanEnemyFunction.Rising;
    }

    /// <summary>Ports <c>Function_Kzan_Rising</c> at <c>$A6:8C5D</c>.</summary>
    private static void RunKzanRising(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        state.PreviousYPosition = slot.YPosition;

        // The NTSC build rises by exactly 0.8000h pixels per actor frame. Perform the same
        // split SEC/SBC so a fractional borrow decrements the whole position word.
        SubtractKzanY(slot, wholeSpeed: 0, KzanNtscRisingSubspeed);
        if (IsNegative16(slot.YPosition - state.RisingTargetYPosition))
        {
            state.Function = KzanEnemyFunction.WaitingToFall;
            slot.YPosition = state.RisingTargetYPosition;
        }

        CarrySamusWithKzanIfTouchingFromBelow(slot, state, samus);
    }

    /// <summary>Adds the native <c>[$14].[$12]</c> split pair to enemy Y.</summary>
    private static void AddKzanY(
        RoomEnemySlot slot,
        short wholeSpeed,
        ushort fractionalSpeed)
    {
        uint fractionalSum = (uint)slot.YSubposition + fractionalSpeed;
        slot.YSubposition = unchecked((ushort)fractionalSum);
        slot.YPosition = unchecked((ushort)(
            slot.YPosition + wholeSpeed + (fractionalSum > ushort.MaxValue ? 1 : 0)));
    }

    /// <summary>Subtracts the native <c>[$14].[$12]</c> split pair from enemy Y.</summary>
    private static void SubtractKzanY(
        RoomEnemySlot slot,
        ushort wholeSpeed,
        ushort fractionalSpeed)
    {
        bool borrowed = slot.YSubposition < fractionalSpeed;
        slot.YSubposition = unchecked((ushort)(slot.YSubposition - fractionalSpeed));
        slot.YPosition = unchecked((ushort)(
            slot.YPosition - wholeSpeed - (borrowed ? 1 : 0)));
    }

    /// <summary>
    /// Ports Kzan's private copy of <c>CheckIfEnemyIsTouchingSamusFromBelow</c>. Its only
    /// difference from the common helper is the five-pixel shifted Y sample. Horizontal
    /// overlap is strict; vertical tangency is accepted after the shifted center test.
    /// </summary>
    private static bool IsKzanTouchingSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        int xDistance = Math.Abs(unchecked((short)(samus.XPosition - slot.XPosition)));
        if (xDistance >= samus.Kinematics.XRadius + slot.XRadius)
            return false;

        short shiftedYDelta = unchecked((short)(
            unchecked((ushort)(samus.YPosition + 5 - slot.YPosition))));
        if (shiftedYDelta >= 0)
            return false;

        int yDistance = Math.Abs((int)shiftedYDelta);
        if (yDistance < samus.Kinematics.YRadius)
            return true;

        return yDistance - samus.Kinematics.YRadius <= slot.YRadius;
    }

    /// <summary>
    /// Adds only the accepted whole-pixel Kzan delta to WRAM-equivalent <c>$0B5C</c>.
    /// Native code deliberately leaves extra Y subdisplacement untouched.
    /// </summary>
    private static void CarrySamusWithKzanIfTouchingFromBelow(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !IsKzanTouchingSamusFromBelow(slot, samus))
            return;

        ushort yDelta = unchecked((ushort)(slot.YPosition - state.PreviousYPosition));
        samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
            samus.Kinematics.ExtraYDisplacement + yDelta));
    }

    private KzanEnemyState RequireKzanState(RoomEnemySlot slot) =>
        _kzanStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Kzan-top state.");
}
