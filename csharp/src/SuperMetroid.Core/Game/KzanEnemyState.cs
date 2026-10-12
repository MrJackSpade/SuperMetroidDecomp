using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function pointer stored in Kzan top variable A. These are kept as ROM
/// addresses so debugger watches agree directly with <c>$0FA8,x</c> and corrupt/custom
/// population state fails at the exact indirect target instead of becoming a plausible enum.
/// </summary>
public enum KzanEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Kzan_WaitingToFall</c> at <c>$A6:8BB4</c>; counts down the population-defined top wait before resetting fall acceleration.</summary>
    WaitingToFall = 0x8bb4,
    /// <summary>Native <c>Function_Kzan_Falling</c> at <c>$A6:8BDC</c>; accelerates downward, carries a touching rider, and clamps to the lower target on landing.</summary>
    Falling = 0x8bdc,
    /// <summary>Native <c>Function_Kzan_WaitingToRise</c> at <c>$A6:8C4A</c>; waits at the lower target until the post-landing countdown expires.</summary>
    WaitingToRise = 0x8c4a,
    /// <summary>Native <c>Function_Kzan_Rising</c> at <c>$A6:8C5D</c>; returns upward at one-half pixel per NTSC actor update and resumes the top wait.</summary>
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

    /// <summary>Current bank-$A6 AI entry, backed by enemy variable A at native <c>$0FA8,x</c>; this view reads the owning top slot directly.</summary>
    public KzanEnemyFunction Function
    {
        get => (KzanEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Top-position wait in actor updates, backed by variable B at <c>$0FAA,x</c>; decremented with 16-bit wrapping, so an initial zero does not expire immediately.</summary>
    public ushort FallWaitTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Original top-slot Y coordinate in whole room pixels, backed by variable C at <c>$0FAC,x</c>; upward motion clamps here before restarting the wait.</summary>
    public ushort RisingTargetYPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Lower Y limit in whole room pixels, backed by variable D at <c>$0FAE,x</c>; initialized to the spawn Y plus population parameter 2's high-byte fall distance, with 16-bit wrapping.</summary>
    public ushort FallingTargetYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Fractional low word of the initial 16.16 downward speed sample, backed by variable E at <c>$0FB0,x</c>; this initialization snapshot is not the accelerating current velocity.</summary>
    public ushort InitialFallingYSubspeed
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Signed whole-pixel high word of the initial 16.16 downward speed sample, stored as raw bits in variable F at <c>$0FB2,x</c>; selected by population parameter 1's low byte.</summary>
    public ushort InitialFallingYSpeed
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Reload value for the top wait, copied from population parameter 2's low byte into the owning slot's native <c>$7E:7800,x</c> extension word.</summary>
    public ushort FallWaitTimerResetValue
    {
        get => _fallWaitTimerResetValues[_slot.SlotIndex];
        internal set => _fallWaitTimerResetValues[_slot.SlotIndex] = value;
    }

    /// <summary>Whole room-pixel Y coordinate saved before a falling or rising update, backed by <c>$7E:7802,x</c>; the accepted whole-pixel movement determines rider displacement.</summary>
    public ushort PreviousYPosition
    {
        get => _previousYPositions[_slot.SlotIndex];
        internal set => _previousYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>
    /// Byte offset into <c>CommonEnemySpeeds_LinearlyIncreasing</c>. Kzan advances by eight
    /// bytes per falling frame because each logical speed owns positive and negative pairs.
    /// Backed by <c>$7E:7804,x</c>, initialized from population parameter 1's low byte times
    /// eight, and capped at <c>$0200</c> after each falling update.
    /// </summary>
    public ushort FallingYSpeedTableIndex
    {
        get => _fallingYSpeedTableIndexes[_slot.SlotIndex];
        internal set => _fallingYSpeedTableIndexes[_slot.SlotIndex] = value;
    }

    /// <summary>Post-landing wait in actor updates, backed by <c>$7E:7806,x</c>; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public ushort RiseWaitTimer
    {
        get => _riseWaitTimers[_slot.SlotIndex];
        internal set => _riseWaitTimers[_slot.SlotIndex] = value;
    }
}
