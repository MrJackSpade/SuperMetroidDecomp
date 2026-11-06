using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// The state of a Boyon, with its six parallel extended words. The values the game keeps in the enemy's
/// common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class BoyonEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<BoyonProgram> Program { get; set; }

    /// <summary>
    /// The multiplier its bounce curve is scaled by, from its spawn settings; a kill empties it to zero. The multiply
    /// takes it whole, so a multiplier the multiply could not hold, above 255, is rejected.
    /// </summary>
    public int SpeedMultiplier
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>
    /// The latest product of the curve sample and the multiplier, in 256ths of a pixel per update (0 through 65025).
    /// </summary>
    public int Speed
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The arc's target height in 256ths of a pixel, 12288 through 45056 from its spawn settings. It is compared with
    /// <see cref="DistanceAccumulator"/> by the sign of their word difference (<see cref="HasReachedJumpHeight"/>), so
    /// heights of 32769 and above behave as the game's do.
    /// </summary>
    public int JumpHeight
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The distance accumulated while deriving the initial curve index, in 256ths of a pixel, as the exact word the
    /// game keeps (0 through 65535). It advances only through <see cref="AccumulateBounceDistance"/>.
    /// </summary>
    public int DistanceAccumulator
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The current entry of the triangular speed curve, as the exact word the game keeps. Rising retreats it with
    /// <see cref="RetreatSpeedTableIndex"/>, which takes it from zero to <see cref="LargestSpeedTableIndex"/> at the
    /// top of the arc; falling advances it with <see cref="AdvanceSpeedTableIndex"/>, which takes it from there back to
    /// zero.
    /// </summary>
    public int SpeedTableIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The largest curve index the word holds: rising past entry zero leaves this value.</summary>
    internal const int LargestSpeedTableIndex = 65535;

    /// <summary>
    /// Whether the accumulated distance has reached the jump height. The game subtracts the height from the
    /// distance and tests the sign of the word result, so any height above 32768 is reached by a zero distance.
    /// </summary>
    internal bool HasReachedJumpHeight => !EnemyMath.IsNegative16(DistanceAccumulator - JumpHeight);

    /// <summary>Adds one update's speed to the accumulated distance, wrapping as the game's word addition does.</summary>
    internal void AccumulateBounceDistance(int speed) =>
        DistanceAccumulator = WrappingCounter.AddWord(DistanceAccumulator, speed);

    /// <summary>Moves one entry down the curve (falling), wrapping from <see cref="LargestSpeedTableIndex"/> to zero.</summary>
    internal void AdvanceSpeedTableIndex() =>
        SpeedTableIndex = Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedTableIndex == LargestSpeedTableIndex ? 0 : SpeedTableIndex + 1);

    /// <summary>
    /// Moves one entry up the curve (rising). The index wraps from zero to <see cref="LargestSpeedTableIndex"/>, and
    /// the result reports whether the decremented word is negative, the game's signal that the top of the arc was
    /// passed.
    /// </summary>
    internal bool RetreatSpeedTableIndex()
    {
        CountdownStep step = WrappingCounter.Decrement(Ensure.BetweenInclusive(0, ushort.MaxValue, SpeedTableIndex));
        SpeedTableIndex = step.Remaining.RemainingFrames;
        return step.IsNegative;
    }

    /// <summary>One-word latch preventing a second animation restart mid-bounce.</summary>
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

    /// <summary>Prevents the idle instruction list from being reinstalled every frame.</summary>
    public bool IdleDisabled { get; internal set; }

    /// <summary>One-time gate for the initial curve-index calculation.</summary>
    public bool BounceSpeedCalculated { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        SpeedMultiplier = 0;
        Speed = 0;
        JumpHeight = 0;
        DistanceAccumulator = 0;
        SpeedTableIndex = 0;
        Bouncing = false;
    }
}
