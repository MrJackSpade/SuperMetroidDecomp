namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The routine each Baby Turtle's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum BabyTurtleAiFunction
{
    /// <summary>$A2:9142, crawls along the shell without carrying Samus.</summary>
    CrawlingNotCarryingSamus = 1,
    /// <summary>$A2:916E, hides while remaining attached to a carried Samus.</summary>
    HidingCarryingSamus,
    /// <summary>$A2:9198, hides without carrying Samus.</summary>
    HidingNotCarryingSamus,
    /// <summary>$A2:91F8, spins while Samus cannot stop the child.</summary>
    SpinningUnstoppable,
    /// <summary>$A2:9239, spins in the state that Samus can interrupt.</summary>
    SpinningStoppable,
    /// <summary>$A2:925E, crawls while carrying Samus along the shell contour.</summary>
    CrawlingCarryingSamus,
}
