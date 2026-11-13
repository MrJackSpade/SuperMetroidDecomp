namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The behaviour Mama Turtle's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MamaTurtleAction
{
    /// <summary>Links the four following Baby Turtle slots to their parent.</summary>
    Initial = 1,
    /// <summary>Inert state used after parent/child setup.</summary>
    Idle,
    /// <summary>Waits asleep until the shared wake counter is cleared.</summary>
    Asleep,
    /// <summary>Moves Samus along the shell while Mama Turtle emerges.</summary>
    LeavingShell,
    /// <summary>Returns Mama Turtle and a carried Samus into the shell.</summary>
    EnteringShell,
    /// <summary>Rises from the shell toward the hovering height.</summary>
    RisingToHover,
    /// <summary>Hovers while updating shell collision and rider displacement.</summary>
    Hovering,
    /// <summary>Accelerates upward toward the authored peak position.</summary>
    RisingToPeak,
    /// <summary>Pauses at the peak before falling.</summary>
    HoveringAtPeak,
    /// <summary>Falls back toward the shell with a capped velocity.</summary>
    Falling,
}
