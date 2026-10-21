namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Indirect function pointers stored in each Baby Turtle's variable A.</summary>
public enum BabyTurtleAiFunction : ushort
{
    /// <summary>$A2:9142, crawls along the shell without carrying Samus.</summary>
    CrawlingNotCarryingSamus = 0x9142,
    /// <summary>$A2:916E, hides while remaining attached to a carried Samus.</summary>
    HidingCarryingSamus = 0x916e,
    /// <summary>$A2:9198, hides without carrying Samus.</summary>
    HidingNotCarryingSamus = 0x9198,
    /// <summary>$A2:91F8, spins while Samus cannot stop the child.</summary>
    SpinningUnstoppable = 0x91f8,
    /// <summary>$A2:9239, spins in the state that Samus can interrupt.</summary>
    SpinningStoppable = 0x9239,
    /// <summary>$A2:925E, crawls while carrying Samus along the shell contour.</summary>
    CrawlingCarryingSamus = 0x925e,
}
