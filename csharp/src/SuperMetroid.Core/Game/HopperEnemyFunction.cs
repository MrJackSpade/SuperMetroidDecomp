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
