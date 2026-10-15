namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The six literal bank-$A6 function words stored in Boulder's variable A.</summary>
public enum BoulderAiFunction : ushort
{
    /// <summary>$A6:879A, <c>Function_Boulder_WaitForSamusToGetNear</c>: waits for Samus to enter the direction-dependent X window and vertical trigger range before selecting an initial fall or immediate roll.</summary>
    WaitingForSamus = 0x879a,
    /// <summary>$A6:87ED, <c>Function_Boulder_Falling</c>: accelerates downward without terrain collision checks until the original spawn Y is reached, then starts the first rebound.</summary>
    InitialFall = 0x87ed,
    /// <summary>$A6:8832, <c>Function_Boulder_Bounce_Rising</c>: decelerates the upward quadratic-table motion while accelerating horizontally, then enters the falling half of the bounce.</summary>
    Rebound = 0x8832,
    /// <summary>$A6:888B, <c>Function_Boulder_Bounce_Falling</c>: falls with terrain collision, selects progressively smaller rebounds, and enters rolling when the bounce counter underflows; direction two breaks on impact.</summary>
    Falling = 0x888b,
    /// <summary>$A6:8942, <c>Function_Boulder_Rolling</c>: follows terrain with vertical compensation and accelerates horizontally until wall collision hides and deletes the actor with a dust effect.</summary>
    Rolling = 0x8942,
    /// <summary>$A6:89FC, <c>Function_Boulder_LoadEnemyIndex</c>: native load-index-and-return target written after terminal rolling impact; no further movement occurs before slot cleanup.</summary>
    Inert = 0x89fc,
}
