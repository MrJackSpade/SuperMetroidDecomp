namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours Lower Norfair Rio's main AI dispatches.
/// Its values are arbitrary.
/// </summary>
public enum LowerNorfairRioAction
{
    /// <summary>Follows the immediately preceding parent twelve pixels below, mirrors its freeze state and flame visibility, and deletes the follower when the parent dies.</summary>
    FollowParent,
    /// <summary>Admits an attack when either half of the random sample is odd and Samus is strictly within 112 horizontal room pixels.</summary>
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
