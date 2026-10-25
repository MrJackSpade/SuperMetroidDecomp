namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The routine Mama Turtle's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum MamaTurtleAiFunction : ushort
{
    /// <summary>$A2:8DD8, links the four following Baby Turtle slots to their parent.</summary>
    Initial = 1,
    /// <summary>$A2:8E09, inert state used after parent/child setup.</summary>
    Idle,
    /// <summary>$A2:8E0A, waits asleep until the shared wake counter is cleared.</summary>
    Asleep,
    /// <summary>$A2:8EE0, moves Samus along the shell while Mama Turtle emerges.</summary>
    LeavingShell,
    /// <summary>$A2:8F3F, returns Mama Turtle and a carried Samus into the shell.</summary>
    EnteringShell,
    /// <summary>$A2:8F8D, rises from the shell toward the hovering height.</summary>
    RisingToHover,
    /// <summary>$A2:8FEB, hovers while updating shell collision and rider displacement.</summary>
    Hovering,
    /// <summary>$A2:9083, accelerates upward toward the authored peak position.</summary>
    RisingToPeak,
    /// <summary>$A2:90CC, pauses at the peak before falling.</summary>
    HoveringAtPeak,
    /// <summary>$A2:90E1, falls back toward the shell with a capped velocity.</summary>
    Falling,
}
