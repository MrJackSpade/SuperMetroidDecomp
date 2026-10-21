using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The state of a Boulder, with its otherwise anonymous words at enemy offsets <c>$7800-$780E</c>. The values the
/// cartridge keeps in the enemy's common record are emptied with it when the enemy is killed (<see
/// cref="ClearWithRecord"/>).
/// </summary>
public sealed class BoulderEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BoulderProgram> Program { get; set; }

    /// <summary>
    /// Current bank-$A6 indirect AI target (native <c>Boulder.function</c>); none once a kill has emptied the record.
    /// </summary>
    public BoulderAiFunction? Function { get; internal set; }

    /// <summary>
    /// The travel acceleration clock in 256ths of a quadratic-speed record: advanced by 32 or 64 per update and capped
    /// at 20480 or 16384, so it stays within 0 through 20480; its whole part selects the quadratic speed record. It
    /// drives horizontal travel while bouncing and rolling, and also the ground-following fall while rolling, so it is
    /// an independent clock rather than one axis of a velocity.
    /// </summary>
    public int TravelSpeedAccumulator
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>
    /// The bounce acceleration clock in 256ths of a quadratic-speed record: the accumulator for the initial fall and
    /// the rebound arcs, with its own steps, caps and resets. A rebound that would take it below zero clamps it to
    /// zero, and the uncapped falling increment cannot pass 24320 because the quadratic table rejects record 95 before
    /// another increment, so it stays within 0 through 24320.
    /// </summary>
    public int BounceSpeedAccumulator
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>Variable D. Its deliberate zero-to-$FFFF underflow ends the bounce phase.</summary>
    public WrappingCountdown BounceCounter { get; internal set; }

    /// <summary>
    /// Parameter one's high byte (0 through 255): zero rolls right, any other value rolls left, and two also breaks the
    /// boulder on its first impact.
    /// </summary>
    [NativeWord("population parameter high byte: zero rolls right, any other value rolls left, and two also breaks on first impact")]
    public int DirectionSelector
    {
        get;
        internal set => field = checked((ushort)value);
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

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        TravelSpeedAccumulator = 0;
        BounceSpeedAccumulator = 0;
        BounceCounter = WrappingCountdown.Start(0);
        DirectionSelector = 0;
    }
}
