namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The behaviours of the Sidehopper/Dessgeega state machine. Each runs alike on the floor and on the ceiling, the
/// surface the hopper was placed on deciding which way is up.
/// Its values are arbitrary.
/// </summary>
public enum HopperAction
{
    /// <summary>Takes a random number that chooses a small or a big hop by whether it is odd, and hops from the next update.</summary>
    ChooseHopSize,
    /// <summary>
    /// Sets the chosen hop size's precomputed speed-curve place and its place step (three marks a pass for a small hop,
    /// four for a big one) before choosing the hop's direction (#2338).
    /// </summary>
    PrepareHop,
    /// <summary>Turns the hopper's heading toward Samus: left when she is to its left, otherwise right.</summary>
    ChooseDirection,
    /// <summary>Starts the jumping animation and the hop along the chosen heading (#2338).</summary>
    StartHop,
    /// <summary>Starts the variant's floor or ceiling landing animation and goes on to wait for the next hop (<see cref="WaitToHop"/>).</summary>
    Landed,
    /// <summary>
    /// Hops away from its surface, resolving vertical movement before horizontal movement, until the apex, a wall or the
    /// room beyond turns it back (<see cref="Returning"/>): up for a floor hopper, down for a ceiling one (#2473).
    /// </summary>
    HoppingAway,
    /// <summary>
    /// Returns to its surface, resolving vertical movement before horizontal movement, speeding up to its fastest mark until
    /// it lands (#2473).
    /// </summary>
    Returning,
    /// <summary>Consumes and clears the ready flag published by the idle animation's ready cue, then returns to random hop-size selection.</summary>
    WaitToHop,
}
