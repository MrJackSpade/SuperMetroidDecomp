namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-facing view of the otherwise anonymous Boulder words at enemy offsets
/// <c>$7800-$780E</c>. The function and three motion counters remain backed by the common
/// enemy slot so watches retain the same values that the instruction/AI dispatcher sees.
/// </summary>
public sealed class BoulderEnemyState
{
    /// <summary>Common enemy slot backing the boulder's function and four motion-control words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches the debugger-facing state view to one physical Boulder record.</summary>
    internal BoulderEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Current bank-$A6 indirect AI target, backed by common variable A here and corresponding to native <c>Boulder.function</c> at $0FA8,x.</summary>
    public BoulderAiFunction Function
    {
        get => (BoulderAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Variable B, the travel acceleration clock: advanced in $20/$40 steps and indexed by
    /// its high byte into the quadratic speed table. It drives horizontal travel while
    /// bouncing and rolling, and also the ground-following fall while rolling, so it is an
    /// independent clock rather than one axis of a velocity.
    /// </summary>
    public ushort TravelSpeedAccumulator
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>
    /// Variable C, the bounce acceleration clock: the quadratic-table accumulator for the
    /// initial fall and the rebound arcs, with its own steps, caps and resets.
    /// </summary>
    public ushort BounceSpeedAccumulator
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D. Its deliberate zero-to-$FFFF underflow ends the bounce phase.</summary>
    public NativeCountdown BounceCounter
    {
        get => NativeCountdown.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
    }

    /// <summary>Variable E, copied from parameter one's high byte and used as direction.</summary>
    public ushort DirectionSelector
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Signed pixel threshold for Samus's X displacement, from parameter two's low byte: positive for rightward activation and negative for leftward activation. Native <c>XProximity</c> at $7E:7800,x.</summary>
    public short HorizontalTriggerLimit { get; internal set; }
    /// <summary>Whole-pixel extra downward collision reach during rolling, subtracted from Y afterward; initially two or five from parameter one's low byte, then zero once the full Y position stops changing. Native <c>minimumDistanceFromGround</c> at $7E:7802,x.</summary>
    public ushort VerticalCompensation { get; internal set; }
    /// <summary>Saved fractional Y position in 1/65536 pixel units, compared with the saved whole position to detect stationary rolling and refreshed after each rolling update. Native <c>previousEnemyYSubPosition</c> at $7E:7804,x.</summary>
    public ushort PreviousYSubposition { get; internal set; }
    /// <summary>Saved room Y coordinate in pixels for the rolling stability check; captured at initialization and on the final bounce, then refreshed during rolling. Native <c>previousEnemyYPosition</c> at $7E:7806,x.</summary>
    public ushort PreviousYPosition { get; internal set; }
    /// <summary>
    /// Original population position in room pixels: X retained as native <c>spawnXPosition</c>
    /// at $7E:7808,x (the translated movement does not subsequently consume it), and Y before
    /// the initial upward offset, which the first fall snaps to before rebounding (native
    /// <c>fallingTargetYPosition</c> at $7E:780A,x).
    /// </summary>
    public Position16<RoomPixels> InitialPosition { get; internal set; }
    /// <summary>Exclusive upper pixel threshold for Samus's nonnegative Y displacement, taken from the population initialization instruction word's low byte. Native <c>YProximity</c> at $7E:780C,x.</summary>
    public byte VerticalTriggerLimit { get; internal set; }
    /// <summary>True when parameter two's high byte is zero: initialization substitutes a one-pixel upward offset and activation bypasses the falling and bounce sequence. Native <c>type</c> at $7E:780E,x.</summary>
    public bool StartsRollingWithoutBounce { get; internal set; }
}
