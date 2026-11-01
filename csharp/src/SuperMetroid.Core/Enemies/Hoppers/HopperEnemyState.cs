using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The state of a hopper, with the seven parallel extension words. These names intentionally retain
/// “speed table index” where the game does: replacing them with host gravity fields would hide the buggy quadratic
/// table that defines the retail jump arc. The values the game keeps in the enemy's common record are emptied with
/// it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class HopperEnemyState
{
    /// <summary>
    /// The current hopper dispatcher pointer, advanced one setup stage per AI call; none once a kill has
    /// emptied the record.
    /// </summary>
    public HopperEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The quadratic-speed record of the current vertical step. It retreats by <see cref="YSpeedTableIndexDelta"/>
    /// while moving away from the surface, where a result below zero is stored as zero at the apex, and grows by it up
    /// to 64 while returning, so it stays within 0 through 94.
    /// </summary>
    public int YSpeedTableIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Signed whole-pixel horizontal velocity; the fractional word is always zero. Only three or its negation is stored.</summary>
    public int XVelocity { get; internal set; }

    /// <summary>
    /// The record step, 3 for small hops or 4 for big hops, subtracted while moving away from the attachment surface
    /// and added while returning.
    /// </summary>
    public int YSpeedTableIndexDelta
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Installed landed or jumping animation: where the typed program (#641) resumes.</summary>
    internal AnimationProgramCursor<HopperProgram> Program { get; set; }
    /// <summary>Initial quadratic record index, within the table's 95 records, calculated with step 3 and accumulated-height threshold 4096 using the game's odd-byte speed-word reads.</summary>
    public int SmallHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Initial quadratic record index, within the table's 95 records, calculated with step 4 and accumulated-height threshold 12288 using the game's odd-byte speed-word reads.</summary>
    public int BigHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>True selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Latch set by the idle animation's ready cue and consumed by WaitToHop; authored animation cadence, not a host jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>The header variant, 0 through 3: Sidehopper, large Sidehopper, large Dessgeega or Dessgeega; it selects the animation programs.</summary>
    public int Variant { get; internal set; }

    /// <summary>Population parameter one: zero means floor, nonzero means ceiling.</summary>
    public bool UpsideDown { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        YSpeedTableIndex = 0;
        XVelocity = 0;
        YSpeedTableIndexDelta = 0;
    }
}
