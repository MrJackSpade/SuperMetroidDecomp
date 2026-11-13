namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The behaviours of the Sidehopper/Dessgeega state machine. The apparently duplicated members are real,
/// separate entry points: the direction-selection setup chooses a different one even though both currently dispatch
/// identical motion.
/// Its values are arbitrary.
/// </summary>
public enum HopperEnemyFunction
{
    /// <summary>Consumes a shared RNG value and chooses small or big hop by whether it is odd, without starting movement that update.</summary>
    ChooseHopSize = 1,
    /// <summary>Installs the precomputed small-hop speed index, index delta 3, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareSmallHop,
    /// <summary>Installs the precomputed big-hop speed index, index delta 4, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareBigHop,
    /// <summary>Compares Samus X against a floor Hopper and selects the leftward/backward or rightward/forward start.</summary>
    ChooseDirectionUpsideUp,
    /// <summary>Compares Samus X against a ceiling Hopper and selects the leftward/backward or rightward/forward start.</summary>
    ChooseDirectionUpsideDown,
    /// <summary>Negates X velocity, starts the floor jumping animation, and selects the floor backward movement entry.</summary>
    StartBackwardHopUpsideUp,
    /// <summary>Retains positive X velocity, starts the floor jumping animation, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideUp,
    /// <summary>Negates X velocity, starts the ceiling jumping animation, and selects the ceiling backward movement entry.</summary>
    StartBackwardHopUpsideDown,
    /// <summary>Retains positive X velocity, starts the ceiling jumping animation, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideDown,
    /// <summary>Restores the variant's floor or ceiling idle animation and waits for its cue-driven ready flag.</summary>
    Landed,
    /// <summary>Floor hop motion selected by the initial leftward branch, resolving vertical movement before horizontal movement and landing.</summary>
    JumpingUpsideUpBackward,
    /// <summary>Distinct floor hop entry selected by the initial rightward branch, with the same motion as <see cref="JumpingUpsideUpBackward"/>.</summary>
    JumpingUpsideUpForward,
    /// <summary>Ceiling hop motion selected by the initial leftward branch, reversing the quadratic table's world-Y halves relative to floor hopping.</summary>
    JumpingUpsideDownBackward,
    /// <summary>Distinct ceiling hop entry selected by the initial rightward branch, with the same motion as <see cref="JumpingUpsideDownBackward"/>.</summary>
    JumpingUpsideDownForward,
    /// <summary>Consumes and clears the ready flag published by the idle animation's ready cue, then returns to random hop-size selection.</summary>
    WaitToHop,
}
