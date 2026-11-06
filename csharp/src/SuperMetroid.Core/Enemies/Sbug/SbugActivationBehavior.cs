namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>How an Sbug moves once Samus activates it, as its placement names it.</summary>
public enum SbugActivationBehavior
{
    /// <summary>Continuously moves along the spawn angle without block collision checks.</summary>
    MoveForward,
    /// <summary>Alternates the spawn angle minus and plus an eighth of a turn in sixteen-update legs.</summary>
    ZigZag,
    /// <summary>Captures the direction toward Samus at activation and thereafter moves straight, without homing.</summary>
    MoveTowardSamus,
    /// <summary>Chooses random-angle segments and returns to proximity waiting on block collision.</summary>
    RandomUntilCollision,
    /// <summary>Chooses random segments and reverses when both axis distances from Samus reach 96 pixels during its long countdown.</summary>
    RandomAndReverseWhenFar,
    /// <summary>Moves along the spawn angle for 32 updates, then waits for Samus to activate it again.</summary>
    MoveForwardThenWait,
    /// <summary>Captures the direction opposite Samus at activation and thereafter moves straight, without homing.</summary>
    MoveAwayFromSamus,
}
