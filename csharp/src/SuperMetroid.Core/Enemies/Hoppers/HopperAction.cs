namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The behaviours of the Sidehopper/Dessgeega state machine. Each runs alike on the floor and on the ceiling, the
/// surface the hopper was placed on deciding which way is up.
/// Its values are arbitrary.
/// </summary>
public enum HopperAction
{
    /// <summary>Consumes a shared RNG value and chooses small or big hop by whether it is odd, without starting movement that update.</summary>
    ChooseHopSize = 1,
    /// <summary>Installs the precomputed small-hop speed-curve place, place step 3, and positive three-pixel X velocity before choosing the hop's direction.</summary>
    PrepareSmallHop,
    /// <summary>Installs the precomputed big-hop speed-curve place, place step 4, and positive three-pixel X velocity before choosing the hop's direction.</summary>
    PrepareBigHop,
    /// <summary>Compares Samus X against the hopper and selects the leftward/backward or rightward/forward start.</summary>
    ChooseDirection,
    /// <summary>Negates X velocity and starts the jumping animation and the hop.</summary>
    StartBackwardHop,
    /// <summary>Retains positive X velocity and starts the jumping animation and the hop.</summary>
    StartForwardHop,
    /// <summary>Restores the variant's floor or ceiling idle animation and waits for its cue-driven ready flag.</summary>
    Landed,
    /// <summary>
    /// Hops, resolving vertical movement before horizontal movement and landing; a ceiling hopper takes the quadratic speed
    /// curve's world-Y halves the other way round from a floor hopper.
    /// </summary>
    Jumping,
    /// <summary>Consumes and clears the ready flag published by the idle animation's ready cue, then returns to random hop-size selection.</summary>
    WaitToHop,
}
