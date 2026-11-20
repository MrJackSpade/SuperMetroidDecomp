namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours a Holtz parent's main AI runs; its flame follower only follows it.
/// Its values are arbitrary.
/// </summary>
public enum HoltzAction
{
    /// <summary>Attacks when Samus is strictly within its trigger distance across (<see cref="RioLaunchDefinitions.LowerNorfairTriggerDistance"/>) and the update's random number does not hold it back.</summary>
    WaitForAttackOpportunity,
    /// <summary>Waits for the animation cue's completion signal before clearing it and starting downward motion.</summary>
    WaitForTakeoffAnimation,
    /// <summary>Moves horizontally and downward, reverses X velocity on walls, and transitions to return on vertical collision or a negative vertical velocity.</summary>
    Dive,
    /// <summary>Climbs while keeping its horizontal movement until it meets the ceiling, then rests.</summary>
    ReturnToPerch,
    /// <summary>Once the landing animation ends, plays the cooldown animation and waits for Samus again.</summary>
    WaitForLandingAnimation,
}
