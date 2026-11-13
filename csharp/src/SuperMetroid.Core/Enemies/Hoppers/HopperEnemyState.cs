using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The state of a hopper, with its seven extra values. These names intentionally retain “speed table index”
/// where the game does: replacing them with host gravity fields would hide the buggy quadratic table that defines the
/// retail jump arc.
/// </summary>
public sealed class HopperEnemyState
{
    /// <summary>The current hopper AI step, advanced one setup stage per AI call.</summary>
    public HopperEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The whole-row place along the quadratic speed table of the current vertical step. It moves back by
    /// <see cref="YSpeedTableIndexDelta"/> rows while moving away from the surface, where a place before the first row
    /// is stored as the first row at the apex, and on by them up to row 64 while returning, so it stays within the
    /// table's rows; a place outside them is rejected.
    /// </summary>
    public EnemyQuadraticSpeedIndex YSpeedTableIndex
    {
        get;
        internal set => field = value.IsNegative || value.WholeRows >= EnemyQuadraticSpeedDefinitions.RowCount
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A hop's place lies outside the quadratic speed table.")
            : value;
    }

    /// <summary>Signed whole-pixel horizontal velocity; the fractional word is always zero. Only three or its negation is stored.</summary>
    public int XVelocity { get; internal set; }

    /// <summary>
    /// The record step, 3 for small hops or 4 for big hops, subtracted while moving away from the attachment surface
    /// and added while returning.
    /// </summary>
    public int YSpeedTableIndexDelta { get; internal set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HopperProgram> Playback { get; set; }
    /// <summary>Initial whole-row place within the quadratic speed table's 95 rows, calculated with step 3 and accumulated-height threshold 4096 by summing each row's coarse speed in 256ths of a pixel.</summary>
    public EnemyQuadraticSpeedIndex SmallHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Initial whole-row place within the quadratic speed table's 95 rows, calculated with step 4 and accumulated-height threshold 12288 by summing each row's coarse speed in 256ths of a pixel.</summary>
    public EnemyQuadraticSpeedIndex BigHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>True selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Latch set by the idle animation's ready cue and consumed by WaitToHop; authored animation cadence, not a host jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>The kind of hopper its definition is, which selects its animation programs.</summary>
    public HopperKind Kind { get; internal set; }

    /// <summary>Population parameter one: zero means floor, nonzero means ceiling.</summary>
    public bool UpsideDown { get; internal set; }
}
