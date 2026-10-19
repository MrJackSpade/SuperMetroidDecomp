namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Which of the sweep's two joint-turn thresholds a link or the head has crossed. A change between
/// sweeps or within one spawns the joint-turn overlays at the link's previous position.
/// </summary>
public enum NuclearWaffleTurnState
{
    /// <summary>Neither threshold crossed; every latch returns here when a new sweep starts.</summary>
    BeforeTurns,
    /// <summary>Past the rising threshold (<see cref="NuclearWaffleEnemyState.SecondTurnThreshold"/>); the turn queues its sound.</summary>
    PastRisingThreshold,
    /// <summary>Past the falling threshold (<see cref="NuclearWaffleEnemyState.FirstTurnThreshold"/>); the second turn is silent.</summary>
    PastFallingThreshold,
}
