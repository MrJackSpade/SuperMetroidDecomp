namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// What Owtch is doing this AI update. Besides the walking, burrowing and surfacing states, Owtch spends one
/// update turning around at the left patrol bound before it walks right again; that frame is observable (Owtch accepts
/// ordinary shots during it and restarts its right-facing animation), so it is a state of its own.
/// </summary>
public enum OwtchBehaviorState
{
    /// <summary>Walks left at the patrol speed and may start sinking; crossing the minimum X bound turns it at the left bound.</summary>
    MovingLeft,
    /// <summary>Walks right at the patrol speed and may start sinking; reaching the maximum X bound turns it left.</summary>
    MovingRight,
    /// <summary>Stays buried while the underground timer counts down; reaching zero starts rising.</summary>
    Underground,
    /// <summary>Moves down one pixel per AI update to depth sixteen, then reloads the placement's underground wait.</summary>
    Sinking,
    /// <summary>Moves up one pixel per AI update until the burial depth reaches zero, then picks a walking direction at random.</summary>
    Rising,
    /// <summary>
    /// The one update spent at the left patrol bound: Owtch does not move, restarts its right-facing animation (whose
    /// first cue then sets <see cref="MovingRight"/>) and, like <see cref="MovingLeft"/>, accepts ordinary shots.
    /// </summary>
    TurningAtLeftBound,
}
