namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours a Holtz parent's main AI runs; its flame follower only follows it.
/// Its values are arbitrary.
/// </summary>
public enum HoltzAction
{
    /// <summary>Attacks when Samus is strictly within 112 horizontal room pixels and the update's random number does not hold it back.</summary>
    WaitForAttackOpportunity,
    /// <summary>Waits for the animation cue's completion signal before clearing it and starting downward motion.</summary>
    WaitForTakeoffAnimation,
    /// <summary>Moves horizontally and downward, reverses X velocity on walls, and transitions to return on vertical collision or a negative vertical velocity.</summary>
    Dive,
    /// <summary>Accelerates upward while retaining horizontal movement, switches ascent artwork on the animation signal, and enters landing wait on vertical collision rather than targeting a saved Y.</summary>
    ReturnToPerch,
    /// <summary>Consumes the landing animation's signal, installs cooldown artwork, and resumes attack-opportunity waiting.</summary>
    WaitForLandingAnimation,
}
