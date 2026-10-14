namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// Typed projection of the four common enemy words at $0FAA-$0FB0 and the seven parallel
/// extension words at $7E:7800-$780C used by hopper AI. These names intentionally retain
/// “speed table index” where the cartridge does: replacing them with host gravity fields
/// would hide the buggy quadratic table that defines the retail jump arc.
/// </summary>
public sealed class HopperEnemyState
{
    /// <summary>Enemy slot whose variables A through D carry the hopper dispatcher and jump motion words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds the typed hopper state to an existing room enemy slot.</summary>
    internal HopperEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>The current bank-$A3 hopper dispatcher pointer, backed by the owning slot's VariableA and advanced one setup stage per AI call.</summary>
    public HopperEnemyFunction Function
    {
        get => (HopperEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Quadratic velocity record index backed by VariableB, multiplied by eight to address its positive/negative 16.16 pair; retreats to zero at the apex and grows up to $40 during return.</summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Signed whole-pixel horizontal velocity; the fractional word is always zero.</summary>
    public short XVelocity
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    /// <summary>Record-index step backed by VariableD: 3 for small hops or 4 for big hops, subtracted while moving away from the attachment surface and added while returning.</summary>
    public ushort YSpeedTableIndexDelta
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native extension offset $00: last installed bank-$A3 idle/jumping instruction-list identity, retained alongside the live instruction cursor.</summary>
    public ushort InstalledInstructionList { get; internal set; }
    /// <summary>Native extension offset $02: initial quadratic record index calculated with step 3 and accumulated-height threshold $1000 using the cartridge's odd-byte speed-word reads.</summary>
    public ushort SmallHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Native extension offset $04: initial quadratic record index calculated with step 4 and accumulated-height threshold $3000 using the cartridge's odd-byte speed-word reads.</summary>
    public ushort BigHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Native extension offset $06: true selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Native extension offset $08: latch set by idle-animation instruction $A3:AAFE and consumed by WaitToHop; authored animation cadence, not a host jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>Byte offset zero for Sidehopper physics, two for every other definition.</summary>
    public ushort HopTableIndex { get; internal set; }

    /// <summary>Twice the header variant, used as a byte offset into four-word list tables.</summary>
    public ushort VariantTableOffset { get; internal set; }

    /// <summary>Population parameter one / native <c>$0FB4</c>: zero means floor, nonzero means ceiling.</summary>
    public bool UpsideDown { get; internal set; }
}
