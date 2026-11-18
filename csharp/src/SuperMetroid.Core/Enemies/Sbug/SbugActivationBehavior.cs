namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// How an Sbug moves once Samus activates it, as its placement names it: the three ways the game's rooms place Sbugs
/// (#2454).
/// </summary>
public enum SbugActivationBehavior
{
    /// <summary>Continuously moves along the spawn angle without block collision checks.</summary>
    MoveForward,
    /// <summary>Alternates the spawn angle minus and plus an eighth of a turn in sixteen-update legs.</summary>
    ZigZag,
    /// <summary>Moves along the spawn angle for 32 updates, then waits for Samus to activate it again.</summary>
    MoveForwardThenWait,
}
