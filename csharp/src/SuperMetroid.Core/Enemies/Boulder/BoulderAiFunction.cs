namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The six routines Boulder's main AI dispatches.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum BoulderAiFunction
{
    /// <summary>$A6:879A, <c>Function_Boulder_WaitForSamusToGetNear</c>: waits for Samus to enter the direction-dependent X window and vertical trigger range before selecting an initial fall or immediate roll.</summary>
    WaitingForSamus = 1,
    /// <summary>$A6:87ED, <c>Function_Boulder_Falling</c>: accelerates downward without terrain collision checks until the original spawn Y is reached, then starts the first rebound.</summary>
    InitialFall,
    /// <summary>$A6:8832, <c>Function_Boulder_Bounce_Rising</c>: decelerates the upward quadratic-table motion while accelerating horizontally, then enters the falling half of the bounce.</summary>
    Rebound,
    /// <summary>$A6:888B, <c>Function_Boulder_Bounce_Falling</c>: falls with terrain collision, selects progressively smaller rebounds, and enters rolling when the bounce counter underflows; direction two breaks on impact.</summary>
    Falling,
    /// <summary>$A6:8942, <c>Function_Boulder_Rolling</c>: follows terrain with vertical compensation and accelerates horizontally until wall collision hides and deletes the actor with a dust effect.</summary>
    Rolling,
    /// <summary>$A6:89FC, <c>Function_Boulder_LoadEnemyIndex</c>: native load-index-and-return target written after terminal rolling impact; no further movement occurs before slot cleanup.</summary>
    Inert,
}
