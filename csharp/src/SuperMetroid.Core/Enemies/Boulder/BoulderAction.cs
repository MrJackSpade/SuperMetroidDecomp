namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The six behaviours Boulder's main AI dispatches.
/// </summary>
public enum BoulderAction
{
    /// <summary>Waits for Samus to enter the direction-dependent X window and vertical trigger range before selecting an initial fall or immediate roll.</summary>
    WaitingForSamus,
    /// <summary>Accelerates downward without terrain collision checks until the authored spawn Y is reached, then starts the first rebound.</summary>
    InitialFall,
    /// <summary>Decelerates the upward quadratic-table motion while accelerating horizontally, then enters the falling half of the bounce.</summary>
    Rebound,
    /// <summary>Falls with terrain collision, selects progressively smaller rebounds, and enters rolling when the bounce counter passes zero; a Boulder that breaks on impact breaks into dust at its first landing instead.</summary>
    Falling,
    /// <summary>Follows terrain with vertical compensation and accelerates horizontally until wall collision hides and deletes the actor with a dust effect.</summary>
    Rolling,
    /// <summary>Inert after the terminal rolling impact; no further movement occurs before slot cleanup.</summary>
    Inert,
}
