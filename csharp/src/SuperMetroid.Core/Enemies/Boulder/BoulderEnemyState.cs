using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The state of a Boulder.</summary>
public sealed class BoulderEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal BoulderEnemyState(BoulderAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BoulderSegment> Playback { get; set; }

    /// <summary>The action the Boulder runs each update.</summary>
    public BoulderAction Action { get; internal set; }

    /// <summary>
    /// The travel acceleration clock along the quadratic speed curve: advanced by 32 or 64 256ths of a mark per update
    /// and capped at mark 80 or 64, so it stays within the start through mark 80; its whole marks select the speed. It
    /// drives horizontal travel while bouncing and rolling, and also the ground-following fall while rolling, so it is
    /// an independent clock rather than one axis of a velocity.
    /// </summary>
    public QuadraticSpeedCurvePlace TravelSpeedAccumulator
    {
        get;
        internal set => field = InRange(value, BoulderMotionDefinitions.MaximumBouncingSpeed);
    }

    /// <summary>
    /// The bounce acceleration clock along the quadratic speed curve: the running sum for the initial fall and the
    /// rebound arcs, with its own steps, caps and resets. A rebound that would take it below the start clamps it to the
    /// start, and the uncapped falling increment cannot pass the place past the curve's last mark, because the curve rejects that
    /// place before another increment, so it stays within the start through that place.
    /// </summary>
    public QuadraticSpeedCurvePlace BounceSpeedAccumulator
    {
        get;
        internal set => field = InRange(value, BoulderMotionDefinitions.LargestBounceSpeed);
    }

    /// <summary><paramref name="value"/>, checked to lie from the curve's start through <paramref name="largest"/>.</summary>
    private static QuadraticSpeedCurvePlace InRange(QuadraticSpeedCurvePlace value, QuadraticSpeedCurvePlace largest)
    {
        _ = Ensure.BetweenInclusive(0, largest.InTwoHundredFiftySixthsOfMark, value.InTwoHundredFiftySixthsOfMark);
        return value;
    }

    /// <summary>
    /// The ground impacts so far, zero through <see cref="BoulderBounces.ImpactCount"/>; the last impact ends the bounce
    /// phase.
    /// </summary>
    public int ImpactsSoFar
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, BoulderBounces.ImpactCount, value);
    }

    /// <summary>Extra downward collision reach during rolling, in pixels, subtracted from the Y position afterward. Initially two pixels, then zero once the full Y position stops changing.</summary>
    public int VerticalCompensation { get; internal set; }
    /// <summary>Saved Y position with its subpixel, compared with the current Y to detect stationary rolling; captured at initialization and on the final bounce, then refreshed after each rolling update.</summary>
    public FixedCoordinate PreviousY { get; internal set; }
    /// <summary>
    /// The population's spawn height in room pixels, before the initial upward offset: the first fall lands there before
    /// rebounding (#2432).
    /// </summary>
    public int RestingY { get; internal set; }
}
