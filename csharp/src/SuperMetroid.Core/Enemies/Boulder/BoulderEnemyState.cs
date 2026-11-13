using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The state of a Boulder, with its otherwise anonymous words.</summary>
public sealed class BoulderEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BoulderProgram> Playback { get; set; }

    /// <summary>Current indirect AI target.</summary>
    public BoulderAiFunction? Function { get; internal set; }

    /// <summary>
    /// The largest travel clock: its larger cap.
    /// </summary>
    internal static readonly EnemyQuadraticSpeedIndex LargestTravelSpeedAccumulator = EnemyQuadraticSpeedIndex.FromRows(80);

    /// <summary>
    /// The largest bounce clock: the quadratic table rejects the record past it before another increment.
    /// </summary>
    internal static readonly EnemyQuadraticSpeedIndex LargestBounceSpeedAccumulator = EnemyQuadraticSpeedIndex.FromRows(EnemyQuadraticSpeedDefinitions.RowCount);

    /// <summary>
    /// The travel acceleration clock along the quadratic speed table: advanced by 32 or 64 256ths of a record per update
    /// and capped at record 80 or 64, so it stays within the start through record 80; its whole records select the speed. It
    /// drives horizontal travel while bouncing and rolling, and also the ground-following fall while rolling, so it is
    /// an independent clock rather than one axis of a velocity.
    /// </summary>
    public EnemyQuadraticSpeedIndex TravelSpeedAccumulator
    {
        get;
        internal set => field = InRange(value, LargestTravelSpeedAccumulator);
    }

    /// <summary>
    /// The bounce acceleration clock along the quadratic speed table: the accumulator for the initial fall and the
    /// rebound arcs, with its own steps, caps and resets. A rebound that would take it below the start clamps it to the
    /// start, and the uncapped falling increment cannot pass record 95 because the quadratic table rejects that record
    /// before another increment, so it stays within the start through record 95.
    /// </summary>
    public EnemyQuadraticSpeedIndex BounceSpeedAccumulator
    {
        get;
        internal set => field = InRange(value, LargestBounceSpeedAccumulator);
    }

    /// <summary><paramref name="value"/>, checked to lie from the start of the table through <paramref name="largest"/>.</summary>
    private static EnemyQuadraticSpeedIndex InRange(EnemyQuadraticSpeedIndex value, EnemyQuadraticSpeedIndex largest)
    {
        _ = Ensure.BetweenInclusive(0, largest.InTwoHundredFiftySixthsOfRow, value.InTwoHundredFiftySixthsOfRow);
        return value;
    }

    /// <summary>The bounces left; the impact that takes it from zero to minus one ends the bounce phase.</summary>
    public int BounceCounter { get; internal set; }

    /// <summary>How the boulder rolls, as its population entry names it.</summary>
    public BoulderRoll Roll { get; internal set; }

    /// <summary>
    /// The window, relative to the boulder, that Samus's displacement must enter to start it. X is the signed pixel
    /// threshold from its authored trigger reach, positive for rightward activation and negative for leftward
    /// activation.
    /// Y is the exclusive upper threshold (0 through 255) for Samus's nonnegative Y displacement, its authored trigger
    /// height.
    /// </summary>
    public Offset16<RoomPixels> TriggerReach { get; internal set; }
    /// <summary>Extra downward collision reach during rolling, subtracted from the position afterward; only its Y is meaningful. Initially two or five pixels by its authored landing, then zero once the full Y position stops changing.</summary>
    public Offset16<RoomPixels> VerticalCompensation { get; internal set; }
    /// <summary>Saved Y position with its subpixel, compared with the current Y to detect stationary rolling; captured at initialization and on the final bounce, then refreshed after each rolling update.</summary>
    public FixedCoordinate PreviousY { get; internal set; }
    /// <summary>
    /// Original population position in room pixels: X retained
    /// (the translated movement does not subsequently consume it), and Y before
    /// the initial upward offset, which the first fall snaps to before rebounding.
    /// </summary>
    public Position16<RoomPixels> InitialPosition { get; internal set; }
    /// <summary>True when its authored drop is zero: initialization substitutes a one-pixel upward offset and activation bypasses the falling and bounce sequence.</summary>
    public bool StartsRollingWithoutBounce { get; internal set; }
}
