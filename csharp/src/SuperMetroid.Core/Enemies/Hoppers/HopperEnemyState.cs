using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The state of a hopper, with its seven extra values. These names keep the place along the quadratic speed curve
/// where the game keeps one: replacing them with gravity fields would hide the buggy quadratic speed curve that defines the
/// retail jump arc.
/// </summary>
public sealed class HopperEnemyState
{
    /// <summary>The current hopper AI step, advanced one setup stage per AI call.</summary>
    public HopperAction? Action { get; internal set; }

    /// <summary>
    /// The whole-mark place along the quadratic speed curve of the current vertical step. It moves back by
    /// <see cref="YSpeedCurvePlaceDelta"/> marks while moving away from the surface, where a place before the curve's start
    /// is stored as the curve's start at the apex, and on by them up to mark 64 while returning, so it stays within the
    /// curve's marks; a place outside them is rejected.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace
    {
        get;
        internal set => field = value.IsNegative || value.WholeMarks >= QuadraticSpeedCurveDefinitions.MarkCount
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A hop's place lies outside the quadratic speed curve.")
            : value;
    }

    /// <summary>Signed whole-pixel horizontal velocity; the fractional word is always zero. Only three or its negation is stored.</summary>
    public int XVelocity { get; internal set; }

    /// <summary>
    /// The mark step, 3 for small hops or 4 for big hops, subtracted while moving away from the attachment surface
    /// and added while returning.
    /// </summary>
    public int YSpeedCurvePlaceDelta { get; internal set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HopperSegment> Playback { get; set; }
    /// <summary>Initial whole-mark place within the quadratic speed curve's 95 marks, calculated with step 3 and accumulated-height threshold 4096 by summing each mark's coarse speed in 256ths of a pixel.</summary>
    public QuadraticSpeedCurvePlace SmallHopInitialYSpeedCurvePlace { get; internal set; }
    /// <summary>Initial whole-mark place within the quadratic speed curve's 95 marks, calculated with step 4 and accumulated-height threshold 12288 by summing each mark's coarse speed in 256ths of a pixel.</summary>
    public QuadraticSpeedCurvePlace BigHopInitialYSpeedCurvePlace { get; internal set; }
    /// <summary>True selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Latch set by the idle animation's ready cue and consumed by WaitToHop; authored animation cadence, not a separate jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>The kind of hopper its definition is, which selects its animations.</summary>
    public HopperKind Kind { get; internal set; }

    /// <summary>Whether it is mounted on the ceiling rather than the floor.</summary>
    public bool UpsideDown { get; internal set; }
}
