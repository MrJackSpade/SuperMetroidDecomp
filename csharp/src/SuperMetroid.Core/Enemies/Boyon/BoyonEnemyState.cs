using SuperMetroid.Core.Animation;
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
    /// The height of the arc, from its spawn settings: 48 to 96 pixels for retail Boyons, never more than
    /// <see cref="BoyonSpeedDefinitions.LargestJumpHeight"/>, the height up to which the game's height test is a plain
    /// comparison (<see cref="HasReachedJumpHeight"/>); a higher arc is refused.
    /// </summary>
    public SignedSixteenSixteen JumpHeight
    {
        get;
        internal set => field = value.IsNegative || value.InTwoFiftySixths > SignedSixteenSixteen.FromWholePixels(BoyonSpeedDefinitions.LargestJumpHeight).InTwoFiftySixths
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A Boyon's arc is no lower than nothing and no higher than 128 pixels.")
            : value;
    }

    /// <summary>
    /// The distance the arc covers from the top of the curve down to <see cref="CurvePlace"/>, accumulated entry by entry
    /// while the first update derives the arc's baseline (<see cref="AccumulateArcDistance"/>).
    /// </summary>
    public SignedSixteenSixteen ArcDistance { get; internal set; }

    /// <summary>
    /// Where the Boyon stands along its bounce curve: rising moves it up one entry an update (<see cref="RiseOneEntry"/>),
    /// from the top entry to past the top, and falling moves it down one entry an update (<see cref="FallOneEntry"/>),
    /// from past the top back to the top entry and on to the baseline.
    /// </summary>
    public BoyonCurvePlace CurvePlace { get; internal set; }

    /// <summary>Whether the accumulated arc distance has reached the jump height.</summary>
    internal bool HasReachedJumpHeight => ArcDistance.InTwoFiftySixths >= JumpHeight.InTwoFiftySixths;

    /// <summary>Adds one entry's speed, in 256ths of a pixel, to the accumulated arc distance.</summary>
    internal void AccumulateArcDistance(int speed) =>
        ArcDistance += SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(Ensure.AtLeastZero(speed));

    /// <summary>Moves one entry down the curve, as falling does: from past the top, to the top entry.</summary>
    internal void FallOneEntry() => CurvePlace = CurvePlace.Lower;

    /// <summary>
    /// Moves one entry up the curve, as rising does, and reports whether the rise has passed the top of the arc: from the
    /// top entry it leaves the Boyon past the top.
    /// </summary>
    internal bool RiseOneEntry()
    {
        CurvePlace = CurvePlace.Higher;
        return CurvePlace.IsPastTheTop;
    }

    /// <summary>Latch preventing a second animation restart mid-bounce.</summary>
    public bool Bouncing { get; internal set; }

    /// <summary>The place along the curve at which falling reaches the spawn-height baseline, derived on the first update.</summary>
    public BoyonCurvePlace BaselineCurvePlace { get; internal set; }

    /// <summary>The bounce movement the Boyon runs each update.</summary>
    public BoyonBounceMovement BounceMovement { get; internal set; }

    /// <summary>
    /// Whether the next bounce is held back. Set at the baseline and cleared while Samus is close enough to request another bounce.
    /// </summary>
    public bool BounceDisabled { get; internal set; }

    /// <summary>Prevents the idle animation from being reinstalled every frame.</summary>
    public bool IdleDisabled { get; internal set; }

    /// <summary>One-time gate for the initial curve-index calculation.</summary>
    public bool BounceSpeedCalculated { get; internal set; }
}
