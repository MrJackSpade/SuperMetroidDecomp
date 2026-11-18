namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The behaviour Mama Turtle's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MamaTurtleAction
{
    /// <summary>
    /// Placed and not yet updated: her first update leaves her dormant in her shell. Until it, her babies crawl without
    /// following the shell's contour, as in the original, whose first update links them to her.
    /// </summary>
    Placed,
    /// <summary>
    /// Placed, and woken by a baby before her first update (#2439): her first update still leaves her in her shell, awake,
    /// and her babies crawl without following its contour until it.
    /// </summary>
    PlacedAndWoken,
    /// <summary>Does nothing of its own while Mama's animation plays, such as leaving or entering her shell; its cues move her.</summary>
    Idle,
    /// <summary>Waits dormant in her shell, which Samus can stand on, until a touch or shot on one of her babies wakes her.</summary>
    Asleep,
    /// <summary>
    /// Still in her shell, woken by a touch or shot on one of her babies (#2439): her next update starts her leaving it. The
    /// update between the wake and the leaving is the game's.
    /// </summary>
    Woken,
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
