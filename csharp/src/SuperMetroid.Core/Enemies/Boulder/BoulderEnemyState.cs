using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Boulder;

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

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BoulderProgram> Program { get; set; }

    /// <summary>Current bank-$A6 indirect AI target, backed by common variable A here and corresponding to native <c>Boulder.function</c> at $0FA8,x.</summary>
    public BoulderAiFunction Function
    {
        get => (BoulderAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Variable B, the travel acceleration clock in 256ths of a quadratic-speed record: advanced by 32 or 64 per
    /// update and capped at 20480 or 16384, so it stays within 0 through 20480; its whole part selects the quadratic
    /// speed record. It drives horizontal travel while bouncing and rolling, and also the ground-following fall while
    /// rolling, so it is an independent clock rather than one axis of a velocity. The slot variable keeps the exact
    /// value for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int TravelSpeedAccumulator
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = checked((ushort)value);
    }

    /// <summary>
    /// Variable C, the bounce acceleration clock in 256ths of a quadratic-speed record: the accumulator for the
    /// initial fall and the rebound arcs, with its own steps, caps and resets. A rebound that would take it below zero
    /// clamps it to zero, and the uncapped falling increment cannot pass 24320 because the quadratic table rejects
    /// record 95 before another increment, so it stays within 0 through 24320. The slot variable keeps the exact value
    /// for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int BounceSpeedAccumulator
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>Variable D. Its deliberate zero-to-$FFFF underflow ends the bounce phase.</summary>
    public NativeCountdown BounceCounter
    {
        get => NativeCountdown.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
    }

    /// <summary>
    /// Variable E, parameter one's high byte (0 through 255): zero rolls right, any other value rolls left, and two
    /// also breaks the boulder on its first impact. The slot variable keeps the exact value for other enemies'
    /// aliased enemy-RAM reads.
    /// </summary>
    public int DirectionSelector
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = checked((ushort)value);
    }

    /// <summary>Signed pixel threshold for Samus's X displacement, from parameter two's low byte: positive for rightward activation and negative for leftward activation. Native <c>XProximity</c> at $7E:7800,x.</summary>
    public short HorizontalTriggerLimit { get; internal set; }
    /// <summary>Extra downward collision reach during rolling, subtracted from the position afterward; only its Y is meaningful. Initially two or five pixels from parameter one's low byte, then zero once the full Y position stops changing. Native <c>minimumDistanceFromGround</c> at $7E:7802,x.</summary>
    public Offset16<RoomPixels> VerticalCompensation { get; internal set; }
    /// <summary>Saved Y position with its subpixel, compared with the current Y to detect stationary rolling; captured at initialization and on the final bounce, then refreshed after each rolling update. Native <c>previousEnemyYSubPosition</c> and <c>previousEnemyYPosition</c> at $7E:7804,x and $7E:7806,x.</summary>
    public SnesFixedPosition PreviousY { get; internal set; }
    /// <summary>
    /// Original population position in room pixels: X retained as native <c>spawnXPosition</c>
    /// at $7E:7808,x (the translated movement does not subsequently consume it), and Y before
    /// the initial upward offset, which the first fall snaps to before rebounding (native
    /// <c>fallingTargetYPosition</c> at $7E:780A,x).
    /// </summary>
    public Position16<RoomPixels> InitialPosition { get; internal set; }
    /// <summary>Exclusive upper pixel threshold (0 through 255) for Samus's nonnegative Y displacement, taken from the population initialization instruction word's low byte. Native <c>YProximity</c> at $7E:780C,x.</summary>
    public int VerticalTriggerLimit { get; internal set; }
    /// <summary>True when parameter two's high byte is zero: initialization substitutes a one-pixel upward offset and activation bypasses the falling and bounce sequence. Native <c>type</c> at $7E:780E,x.</summary>
    public bool StartsRollingWithoutBounce { get; internal set; }
}
