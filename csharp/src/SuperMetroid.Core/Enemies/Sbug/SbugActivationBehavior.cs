namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>The parameter-two high-byte behaviors accepted by Sbug's activation table.</summary>
public enum SbugActivationBehavior
{
    /// <summary>Selector zero: continuously moves along the spawn angle without block collision checks.</summary>
    MoveForward = 0,
    /// <summary>Selector one: alternates the spawn angle minus and plus 32 in sixteen-update legs.</summary>
    ZigZag = 1,
    /// <summary>Selector two: captures the direction toward Samus at activation and thereafter moves straight, without homing.</summary>
    MoveTowardSamus = 2,
    /// <summary>Selector three: chooses random-angle segments and returns to proximity waiting on block collision.</summary>
    RandomUntilCollision = 3,
    /// <summary>Selector four: chooses random segments and reverses when both axis distances from Samus reach 96 pixels during its long countdown.</summary>
    RandomAndReverseWhenFar = 4,
    /// <summary>Selector five: moves along the spawn angle for 32 updates, then waits for Samus to activate it again.</summary>
    MoveForwardThenWait = 5,
    /// <summary>Selector six: captures the direction opposite Samus at activation and thereafter moves straight, without homing.</summary>
    MoveAwayFromSamus = 6,
}
