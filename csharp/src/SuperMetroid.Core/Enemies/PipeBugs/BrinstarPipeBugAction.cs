namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The behaviours a Brinstar Pipe Bug runs (#2063).
/// Its values are arbitrary.
/// </summary>
public enum BrinstarPipeBugAction
{
    /// <summary>Waits until a Brinstar Pipe Bug's origin is on screen.</summary>
    WaitUntilOnScreen,
    /// <summary>Waits for Samus to enter the Brinstar emergence region.</summary>
    WaitForSamus,
    /// <summary>Raises a Brinstar Pipe Bug out of its pipe.</summary>
    Emerge,
    /// <summary>Flies a Brinstar Pipe Bug horizontally until it leaves the screen.</summary>
    FlyHorizontally,
    /// <summary>Holds a hidden Brinstar Pipe Bug before rearming it.</summary>
    RespawnDelay,
}
