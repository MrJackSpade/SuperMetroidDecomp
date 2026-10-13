using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Same-bank function pointers used by the Sidehopper/Dessgeega state machine. The apparent
/// duplicates at $AD20 and $AD44 are real ROM entry points: the direction-selection setup
/// chooses a different pointer even though both entries currently dispatch identical motion.
/// </summary>
public enum HopperEnemyFunction : ushort
{
    /// <summary>$A3:ABD6, Function_Hopper_Hop: consumes a shared RNG value and chooses small or big hop from its low bit without starting movement that update.</summary>
    ChooseHopSize = 0xabd6,
    /// <summary>$A3:ABE6, Function_Hopper_SmallHop: installs the precomputed small-hop speed index, index delta 3, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareSmallHop = 0xabe6,
    /// <summary>$A3:AC13, Function_Hopper_BigHop: installs the precomputed big-hop speed index, index delta 4, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareBigHop = 0xac13,
    /// <summary>$A3:AC40, Function_Hopper_Hop_UpsideUp: compares Samus X against a floor Hopper and selects the leftward/backward or rightward/forward start pointer.</summary>
    ChooseDirectionUpsideUp = 0xac40,
    /// <summary>$A3:AC56, Function_Hopper_Hop_UpsideDown: compares Samus X against a ceiling Hopper and selects the leftward/backward or rightward/forward start pointer.</summary>
    ChooseDirectionUpsideDown = 0xac56,
    /// <summary>$A3:AC6C, Function_Hopper_HopBackwards_UpsideUp: negates X velocity, installs the floor jumping list, and selects the floor backward movement entry.</summary>
    StartBackwardHopUpsideUp = 0xac6c,
    /// <summary>$A3:AC8F, Function_Hopper_HopForwards_UpsideUp: retains positive X velocity, installs the floor jumping list, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideUp = 0xac8f,
    /// <summary>$A3:ACA8, Function_Hopper_HopBackwards_UpsideDown: negates X velocity, installs the ceiling jumping list, and selects the ceiling backward movement entry.</summary>
    StartBackwardHopUpsideDown = 0xaca8,
    /// <summary>$A3:ACCB, Function_Hopper_HopForwards_UpsideDown: retains positive X velocity, installs the ceiling jumping list, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideDown = 0xaccb,
    /// <summary>$A3:ACE4, Function_Hopper_Landed: restores the variant's floor or ceiling idle list and waits for its instruction-driven ready flag.</summary>
    Landed = 0xace4,
    /// <summary>$A3:AD0E, Function_Hopper_Jumping_UpsideUp: floor hop motion selected by the initial leftward branch, resolving vertical movement before horizontal movement and landing.</summary>
    JumpingUpsideUpBackward = 0xad0e,
    /// <summary>$A3:AD20, Function_Hopper_Jumping_UpsideUp_duplicate: distinct floor hop entry selected by the initial rightward branch, with the same native motion as $AD0E.</summary>
    JumpingUpsideUpForward = 0xad20,
    /// <summary>$A3:AD32, Function_Hopper_Jumping_UpsideDown: ceiling hop motion selected by the initial leftward branch, reversing the quadratic table's world-Y halves relative to floor hopping.</summary>
    JumpingUpsideDownBackward = 0xad32,
    /// <summary>$A3:AD44, Function_Hopper_Jumping_UpsideDown_duplicate: distinct ceiling hop entry selected by the initial rightward branch, with the same native motion as $AD32.</summary>
    JumpingUpsideDownForward = 0xad44,
    /// <summary>$A3:AD56, Function_Hopper_WaitToHop: consumes and clears the ready flag published by instruction $A3:AAFE, then returns to random hop-size selection.</summary>
    WaitToHop = 0xad56,
}

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
