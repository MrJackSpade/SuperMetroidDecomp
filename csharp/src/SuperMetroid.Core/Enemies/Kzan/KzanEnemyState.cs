namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Debugger-facing projection of every private Kzan-top word. The first six fields alias
/// ordinary enemy variables A-F; the final four live in the native bank-$7E extension arrays.
/// The bottom half has no private state: it follows the immediately preceding top slot.
/// </summary>
public sealed class KzanEnemyState
{
    /// <summary>Top-half enemy slot containing the native A-F state words.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot extension words for the top wait's reload value.</summary>
    private readonly ushort[] _fallWaitTimerResetValues;
    /// <summary>Per-slot previous Y coordinates used for rider displacement.</summary>
    private readonly ushort[] _previousYPositions;
    /// <summary>Per-slot offsets into the accelerating fall-speed table.</summary>
    private readonly ushort[] _fallingYSpeedTableIndexes;
    /// <summary>Per-slot post-landing waits before upward travel.</summary>
    private readonly ushort[] _riseWaitTimers;

    /// <summary>Projects a Kzan top slot and its bank-$7E extension arrays into typed state.</summary>
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
    public NativeCountdown FallWaitTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
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
    public NativeCountdown RiseWaitTimer
    {
        get => NativeCountdown.FromRaw(_riseWaitTimers[_slot.SlotIndex]);
        internal set => _riseWaitTimers[_slot.SlotIndex] = value.RawValue;
    }
}
