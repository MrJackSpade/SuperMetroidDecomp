namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a Magdollite's tracking overlay, its hand, is doing. Its values are arbitrary.
/// </summary>
/// <remarks>
/// Thrown is a state of its course, not a request another owner stores for it (#2562): a cue, a touch or a shot ends
/// the state before, and the game acts on it one update later, the state naming where it stands until then; the update
/// that acts on it moves it on, as every other state's does. The update between is the game's.
/// </remarks>
public enum MagdolliteOverlayAction
{
    /// <summary>Waits for the head to begin forming the base pillar before tracking arm growth.</summary>
    WaitingForAttack,
    /// <summary>Holds the head's reach while the throw animation plays.</summary>
    WaitingForThrowAnimation,
    /// <summary>
    /// The throw has finished, as its cue says (#2439): the next update restores the captured hand position and starts arm
    /// shrinkage.
    /// </summary>
    Thrown,
    /// <summary>Follows the growing arm cap and, at the apex, selects a Samus-facing throw animation and captures its origin.</summary>
    TrackingRisingBody,
    /// <summary>Follows the shrinking arm cap until the head returns to idle.</summary>
    TrackingFallingBody,
}
