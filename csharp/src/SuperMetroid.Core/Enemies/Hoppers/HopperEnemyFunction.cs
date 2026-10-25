namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The routines of the Sidehopper/Dessgeega state machine. The apparently duplicated members are real,
/// separate entry points: the direction-selection setup chooses a different one even though both currently dispatch
/// identical motion.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum HopperEnemyFunction : ushort
{
    /// <summary>$A3:ABD6, Function_Hopper_Hop: consumes a shared RNG value and chooses small or big hop from its low bit without starting movement that update.</summary>
    ChooseHopSize = 1,
    /// <summary>$A3:ABE6, Function_Hopper_SmallHop: installs the precomputed small-hop speed index, index delta 3, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareSmallHop,
    /// <summary>$A3:AC13, Function_Hopper_BigHop: installs the precomputed big-hop speed index, index delta 4, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareBigHop,
    /// <summary>$A3:AC40, Function_Hopper_Hop_UpsideUp: compares Samus X against a floor Hopper and selects the leftward/backward or rightward/forward start pointer.</summary>
    ChooseDirectionUpsideUp,
    /// <summary>$A3:AC56, Function_Hopper_Hop_UpsideDown: compares Samus X against a ceiling Hopper and selects the leftward/backward or rightward/forward start pointer.</summary>
    ChooseDirectionUpsideDown,
    /// <summary>$A3:AC6C, Function_Hopper_HopBackwards_UpsideUp: negates X velocity, installs the floor jumping list, and selects the floor backward movement entry.</summary>
    StartBackwardHopUpsideUp,
    /// <summary>$A3:AC8F, Function_Hopper_HopForwards_UpsideUp: retains positive X velocity, installs the floor jumping list, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideUp,
    /// <summary>$A3:ACA8, Function_Hopper_HopBackwards_UpsideDown: negates X velocity, installs the ceiling jumping list, and selects the ceiling backward movement entry.</summary>
    StartBackwardHopUpsideDown,
    /// <summary>$A3:ACCB, Function_Hopper_HopForwards_UpsideDown: retains positive X velocity, installs the ceiling jumping list, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideDown,
    /// <summary>$A3:ACE4, Function_Hopper_Landed: restores the variant's floor or ceiling idle list and waits for its instruction-driven ready flag.</summary>
    Landed,
    /// <summary>$A3:AD0E, Function_Hopper_Jumping_UpsideUp: floor hop motion selected by the initial leftward branch, resolving vertical movement before horizontal movement and landing.</summary>
    JumpingUpsideUpBackward,
    /// <summary>$A3:AD20, Function_Hopper_Jumping_UpsideUp_duplicate: distinct floor hop entry selected by the initial rightward branch, with the same native motion as $AD0E.</summary>
    JumpingUpsideUpForward,
    /// <summary>$A3:AD32, Function_Hopper_Jumping_UpsideDown: ceiling hop motion selected by the initial leftward branch, reversing the quadratic table's world-Y halves relative to floor hopping.</summary>
    JumpingUpsideDownBackward,
    /// <summary>$A3:AD44, Function_Hopper_Jumping_UpsideDown_duplicate: distinct ceiling hop entry selected by the initial rightward branch, with the same native motion as $AD32.</summary>
    JumpingUpsideDownForward,
    /// <summary>$A3:AD56, Function_Hopper_WaitToHop: consumes and clears the ready flag published by instruction $A3:AAFE, then returns to random hop-size selection.</summary>
    WaitToHop,
}
