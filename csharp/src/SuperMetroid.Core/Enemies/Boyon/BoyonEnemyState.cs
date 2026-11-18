using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The state of a Boyon: its bounce, its speed and its animation.</summary>
public sealed class BoyonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BoyonSegment> Playback { get; set; }

    /// <summary>The multiplier its bounce curve is scaled by, from its spawn settings: one or more (8 to 16 for the Boyons the game places).</summary>
    public int SpeedMultiplier
    {
        get;
        internal set => field = value >= 1
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "A Boyon's curve is scaled by one or more.");
    }

    /// <summary>
    /// The latest product of the curve sample and the multiplier, read as a speed in 256ths of a pixel per update; the
    /// Boyon moves by its whole pixels.
    /// </summary>
    public UnsignedSixteenSixteen Speed { get; internal set; }

    /// <summary>
    /// The height of the arc, from its spawn settings: 48 to 96 pixels for Boyons, compared plainly with the arc's
    /// distance as the first update derives its baseline (<see cref="BoyonBehavior.DeriveBoyonArcBaseline"/>, #2282).
    /// </summary>
    public SignedSixteenSixteen JumpHeight
    {
        get;
        internal set => field = value.IsNegative
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A Boyon's arc is no lower than nothing.")
            : value;
    }


    /// <summary>
    /// Where the Boyon stands along its bounce curve: rising moves it up one entry an update (<see cref="RiseOneEntry"/>),
    /// from the top entry to past the top, and falling moves it down one entry an update (<see cref="FallOneEntry"/>),
    /// from past the top back to the top entry and on to the baseline.
    /// </summary>
    public BoyonCurvePlace CurvePlace { get; internal set; }


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

    /// <summary>Where it stands in its rest-and-bounce cycle (#2432); a freshly loaded Boyon derives its arc first.</summary>
    public BoyonPhase Phase { get; internal set; }

    /// <summary>The place along the curve at which falling reaches the spawn-height baseline, derived on the first update.</summary>
    public BoyonCurvePlace BaselineCurvePlace { get; internal set; }

    /// <summary>The bounce movement the Boyon runs each update.</summary>
    public BoyonBounceMovement BounceMovement { get; internal set; }
}
