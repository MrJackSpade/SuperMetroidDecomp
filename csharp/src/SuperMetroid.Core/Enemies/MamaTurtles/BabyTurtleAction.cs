namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The behaviour each Baby Turtle's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum BabyTurtleAction
{
    /// <summary>Crawls along the shell without carrying Samus.</summary>
    CrawlingNotCarryingSamus = 1,
    /// <summary>Hides while remaining attached to a carried Samus.</summary>
    HidingCarryingSamus,
    /// <summary>Hides without carrying Samus.</summary>
    HidingNotCarryingSamus,
    /// <summary>Spins while Samus cannot stop the child.</summary>
    SpinningUnstoppable,
    /// <summary>Spins in the state that Samus can interrupt.</summary>
    SpinningStoppable,
    /// <summary>Crawls while carrying Samus along the shell contour.</summary>
    CrawlingCarryingSamus,
}
