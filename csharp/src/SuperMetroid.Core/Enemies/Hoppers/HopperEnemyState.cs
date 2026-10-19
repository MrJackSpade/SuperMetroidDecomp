using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The state of a hopper, with the seven parallel extension words at $7E:7800-$780C. These names intentionally retain
/// “speed table index” where the cartridge does: replacing them with host gravity fields would hide the buggy quadratic
/// table that defines the retail jump arc. The values the cartridge keeps in the enemy's common record are emptied with
/// it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class HopperEnemyState
{
    /// <summary>
    /// The current bank-$A3 hopper dispatcher pointer, advanced one setup stage per AI call; none once a kill has
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
        internal set => field = checked((ushort)value);
    }

    /// <summary>Signed whole-pixel horizontal velocity; the fractional word is always zero.</summary>
    public short XVelocity { get; internal set; }

    /// <summary>
    /// The record step, 3 for small hops or 4 for big hops, subtracted while moving away from the attachment surface
    /// and added while returning.
    /// </summary>
    public int YSpeedTableIndexDelta
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>Native extension offset $00's installed landed or jumping animation: where the typed program (#641) resumes.</summary>
    internal AnimationProgramCursor<HopperProgram> Program { get; set; }
    /// <summary>Native extension offset $02: initial quadratic record index, within the table's 95 records, calculated with step 3 and accumulated-height threshold 4096 using the cartridge's odd-byte speed-word reads.</summary>
    public int SmallHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Native extension offset $04: initial quadratic record index, within the table's 95 records, calculated with step 4 and accumulated-height threshold 12288 using the cartridge's odd-byte speed-word reads.</summary>
    public int BigHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Native extension offset $06: true selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Native extension offset $08: latch set by idle-animation instruction $A3:AAFE and consumed by WaitToHop; authored animation cadence, not a host jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>The header variant, 0 through 3: Sidehopper, large Sidehopper, large Dessgeega or Dessgeega; it selects the animation programs.</summary>
    public int Variant { get; internal set; }

    /// <summary>Population parameter one / native <c>$0FB4</c>: zero means floor, nonzero means ceiling.</summary>
    public bool UpsideDown { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        YSpeedTableIndex = 0;
        XVelocity = 0;
        YSpeedTableIndexDelta = 0;
    }
}
