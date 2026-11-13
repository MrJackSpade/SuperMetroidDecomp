using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The state of a Boyon: its bounce, its speed and its animation.</summary>
public sealed class BoyonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BoyonSegment> Playback { get; set; }

    /// <summary>
    /// The multiplier its bounce curve is scaled by, from its spawn settings. The multiply takes it whole, so a
    /// multiplier the multiply could not hold, above <see cref="BoyonSpeedDefinitions.LargestMultiplier"/>, is rejected.
    /// </summary>
    public int SpeedMultiplier
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, BoyonSpeedDefinitions.LargestMultiplier, value);
    }

    /// <summary>
    /// The latest product of the curve sample and the multiplier, in 256ths of a pixel per update, 0 through
    /// <see cref="BoyonSpeedDefinitions.LargestScaledSample"/>.
    /// </summary>
    public int Speed
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, BoyonSpeedDefinitions.LargestScaledSample, value);
    }

    /// <summary>
    /// The arc's target height in 256ths of a pixel, 12288 through 45056 from its spawn settings. It is compared with
    /// <see cref="DistanceAccumulator"/> by the sign of their wrapped difference (<see cref="HasReachedJumpHeight"/>), so
    /// heights of 32769 and above behave as the game's do.
    /// </summary>
    public int JumpHeight { get; internal set; }

    /// <summary>
    /// The distance accumulated while deriving the initial curve index, in 256ths of a pixel, as the exact value the
    /// game keeps (0 through 65535): it advances only through <see cref="AccumulateBounceDistance"/>, which wraps it.
    /// </summary>
    public int DistanceAccumulator { get; internal set; }

    /// <summary>
    /// The current entry of the triangular speed curve, as the exact value the game keeps. Rising retreats it with
    /// <see cref="RetreatSpeedTableIndex"/>, which takes it from zero to <see cref="LargestSpeedTableIndex"/> at the
    /// top of the arc; falling advances it with <see cref="AdvanceSpeedTableIndex"/>, which takes it from there back to
    /// zero.
    /// </summary>
    public int SpeedTableIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, LargestSpeedTableIndex, value);
    }

    /// <summary>The largest curve index the index holds: rising past entry zero leaves this value.</summary>
    internal const int LargestSpeedTableIndex = 65535;

    /// <summary>
    /// Whether the accumulated distance has reached the jump height. The game subtracts the height from the
    /// distance and tests the sign of the wrapped result, so any height above 32768 is reached by a zero distance.
    /// </summary>
    internal bool HasReachedJumpHeight => !WrappingCounter.IsBehind(DistanceAccumulator, JumpHeight);

    /// <summary>Adds one update's speed to the accumulated distance, wrapping as the game's addition does.</summary>
    internal void AccumulateBounceDistance(int speed) =>
        DistanceAccumulator = WrappingCounter.AddCount(DistanceAccumulator, speed);

    /// <summary>Moves one entry down the curve (falling), wrapping from <see cref="LargestSpeedTableIndex"/> to zero.</summary>
    internal void AdvanceSpeedTableIndex() =>
        SpeedTableIndex = WrappingCounter.IncrementCount(SpeedTableIndex);

    /// <summary>
    /// Moves one entry up the curve (rising). The index wraps from zero to <see cref="LargestSpeedTableIndex"/>, and
    /// the result reports whether the decremented index is negative, the game's signal that the top of the arc was
    /// passed.
    /// </summary>
    internal bool RetreatSpeedTableIndex()
    {
        CountdownStep step = WrappingCounter.Decrement(SpeedTableIndex);
        SpeedTableIndex = step.Remaining.RemainingFrames;
        return step.IsNegative;
    }

    /// <summary>Latch preventing a second animation restart mid-bounce.</summary>
    public bool Bouncing { get; internal set; }

    /// <summary>Curve index at which falling reaches the spawn-height baseline; the derivation ends within the curve's first 200 entries.</summary>
    public int InitialBounceSpeedTableIndex { get; internal set; }

    /// <summary>Current indirect movement-table selection.</summary>
    public BoyonBounceMovement BounceMovement { get; internal set; }

    /// <summary>
    /// Whether the next bounce is held back. Set at the baseline and cleared while Samus is close enough to request another bounce.
    /// </summary>
    public bool BounceDisabled { get; internal set; }

    /// <summary>Per-frame copy of <see cref="SpeedMultiplier"/> restored before movement.</summary>
    public int SpeedMultiplierMirror { get; internal set; }

    /// <summary>Prevents the idle animation from being reinstalled every frame.</summary>
    public bool IdleDisabled { get; internal set; }

    /// <summary>One-time gate for the initial curve-index calculation.</summary>
    public bool BounceSpeedCalculated { get; internal set; }
}
