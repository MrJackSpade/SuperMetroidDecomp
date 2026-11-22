using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// The state of a Boyon: its bounce and its animation. It scales its bounce curve by its spawn settings' multiplier and
/// bounces as high as their jump height, which it reads where it uses them (#2476); each update's speed is the product
/// it moves by on that update (#2531).
/// </summary>
public sealed class BoyonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BoyonSegment> Playback { get; set; }

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

    /// <summary>
    /// Where it stands in its rest-and-bounce cycle (#2432), a bounce's rise and fall each a phase of its own (#2531); a
    /// freshly loaded Boyon derives its arc first.
    /// </summary>
    public BoyonPhase Phase { get; internal set; }

    /// <summary>The place along the curve at which falling reaches the spawn-height baseline, derived on the first update.</summary>
    public BoyonCurvePlace BaselineCurvePlace { get; internal set; }
}
