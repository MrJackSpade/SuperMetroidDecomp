using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The state of a Boulder, with its otherwise anonymous words.</summary>
public sealed class BoulderEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<BoulderProgram> Program { get; set; }

    /// <summary>Current indirect AI target.</summary>
    public BoulderAiFunction? Function { get; internal set; }

    /// <summary>
    /// The largest travel clock: its larger cap.
    /// </summary>
    internal const int LargestTravelSpeedAccumulator = 20480;

    /// <summary>
    /// The largest bounce clock: the quadratic table rejects the record past it before another increment.
    /// </summary>
    internal const int LargestBounceSpeedAccumulator = EightEight.FromWhole(EnemyQuadraticSpeedDefinitions.RowCount);

    /// <summary>
    /// The travel acceleration clock in 256ths of a quadratic-speed record: advanced by 32 or 64 per update and capped
    /// at 20480 or 16384, so it stays within 0 through 20480; its whole part selects the quadratic speed record. It
    /// drives horizontal travel while bouncing and rolling, and also the ground-following fall while rolling, so it is
    /// an independent clock rather than one axis of a velocity.
    /// </summary>
    public int TravelSpeedAccumulator
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, LargestTravelSpeedAccumulator, value);
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
        internal set => field = Ensure.BetweenInclusive(0, LargestBounceSpeedAccumulator, value);
    }

    /// <summary>The bounces left; the impact that takes it from zero to minus one ends the bounce phase.</summary>
    public int BounceCounter { get; internal set; }

    /// <summary>How the boulder rolls, as its population entry names it.</summary>
    public BoulderRoll Roll { get; internal set; }

    /// <summary>
    /// The window, relative to the boulder, that Samus's displacement must enter to start it. X is the signed pixel
    /// threshold from parameter two's low byte, positive for rightward activation and negative for leftward
    /// activation.
    /// Y is the exclusive upper threshold (0 through 255) for Samus's nonnegative Y displacement, taken from the
    /// population initialization instruction word's low byte.
    /// </summary>
    public Offset16<RoomPixels> TriggerReach { get; internal set; }
    /// <summary>Extra downward collision reach during rolling, subtracted from the position afterward; only its Y is meaningful. Initially two or five pixels from parameter one's low byte, then zero once the full Y position stops changing.</summary>
    public Offset16<RoomPixels> VerticalCompensation { get; internal set; }
    /// <summary>Saved Y position with its subpixel, compared with the current Y to detect stationary rolling; captured at initialization and on the final bounce, then refreshed after each rolling update.</summary>
    public FixedCoordinate PreviousY { get; internal set; }
    /// <summary>
    /// Original population position in room pixels: X retained
    /// (the translated movement does not subsequently consume it), and Y before
    /// the initial upward offset, which the first fall snaps to before rebounding.
    /// </summary>
    public Position16<RoomPixels> InitialPosition { get; internal set; }
    /// <summary>True when parameter two's high byte is zero: initialization substitutes a one-pixel upward offset and activation bypasses the falling and bounce sequence.</summary>
    public bool StartsRollingWithoutBounce { get; internal set; }
}
