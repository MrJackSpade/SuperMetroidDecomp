namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours a Geruta parent's main AI runs; its flame follower only follows it.
/// Its values are arbitrary.
/// </summary>
public enum GerutaAction
{
    /// <summary>Attacks when Samus is strictly within 192 horizontal room pixels and the update's random number does not hold it back, the same number choosing its launch velocity.</summary>
    WaitForAttackOpportunity,
    /// <summary>Waits for its swoop's start to finish, then plays its descent, enters the dive, and plays the dive sound.</summary>
    WaitForTakeoffAnimation,
    /// <summary>Moves down and horizontally with wall reversal, transitioning to return on vertical collision or when vertical velocity becomes negative.</summary>
    Dive,
    /// <summary>Accelerates upward while moving horizontally and responding to animation signals, ending on vertical collision rather than a saved perch coordinate.</summary>
    ReturnToPerch,
    /// <summary>One-update state that restores only the idle behaviour, leaving the existing looping animation and follower visibility intact.</summary>
    FinishLanding,
}
